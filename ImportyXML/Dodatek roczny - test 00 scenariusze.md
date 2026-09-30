# Dodatek roczny — dane testowe (baza `Claude`)

Pliki importu XML zasilające bazę testową **`Claude`** danymi pod scenariusze testowe
elementu „Dodatek roczny" (opis elementu i pełna lista scenariuszy:
[`ElementyPlac/Dodatek roczny.md`](../ElementyPlac/Dodatek%20roczny.md)).

## Środowisko

Baza `Claude` na `localhost\SQLEXPRESS` (= `ET-097-vm2\SQLEXPRESS`), enova **2512.5.6** —
prywatna piaskownica. Import:

```
C:\enovaServer\2512.5.6\Soneta.Products.Server.Standard\dbmgr.exe importxml Claude "<plik>" --standard
```

Kolejność: `Dodatek roczny.dbinit.xml` (definicja elementu) → 01 → 02 → 03.
Wszystkie pliki mają stałe GUID-y i są **idempotentne**.

## Pliki

| Plik | Zawartość |
|---|---|
| `Dodatek roczny - test 01 pracownicy.xml` | 5 **kompletnych** pracowników etatowych, `Kod` = numer scenariusza (`TS-01`, `TS-02`, `TS-03`, `TS-05`, `TS-09`). |
| `Dodatek roczny - test 02 nieobecnosci.xml` | Nieobecności łamiące / niełamiące frekwencję (TS-02, TS-03, TS-05). |
| `Dodatek roczny - test 03 dodatek w kartotece.xml` | Element „Dodatek roczny" w kartotece całej piątki: Okres `2026-01-01...2026-12-31`, Podstawa `1500,00 PLN`. |
| `Dodatek roczny - test 04 podwyzka od 2026-05.xml` | Podwyżka od 2026-05-01 (+10%) dla całej piątki — **aktualizacja historyczna kartoteki**, patrz niżej. |

## Aktualizacja historyczna (podwyżka „od dnia")

Plik 04 pokazuje wzorzec zmiany warunków **od wskazanej daty** (nie nadpisania bieżącego zapisu):

```xml
<Pracownik where="Kod=TS-01">
  <Historia addnew="true">
    <PracHistoria date="2026-05-01">
      <Etat><Zaszeregowanie><Stawka>6,600.00 PLN</Stawka></Zaszeregowanie></Etat>
    </PracHistoria>
  </Historia>
</Pracownik>
```

- `date="2026-05-01"` → silnik **tnie okres**: klonuje zapis obowiązujący na tę datę
  (z całym `PracHistoria2` i adresami), w klonie nadpisuje **tylko** podane pole (`Stawka`),
  a poprzedni zapis obowiązuje do 2026-04-30. Okres zatrudnienia (`Etat.Okres`) bez zmian.
- `<Historia addnew="true">` — chroni pozostałe zapisy historii przed skasowaniem.
- **Nie jest idempotentny** — ponowny import rzuca `DateDuplicateException` („nie da się
  aktualizować dwa razy tego samego dnia"). Import jednorazowy; ponowne wczytanie → najpierw
  usuń nowy zapis albo zmień datę.
- Efekt w GUI: zakładka „Historia zapisów" pracownika — nowy wiersz „Ważny od = 1.05.2026"
  z nową stawką.

## Dlaczego pracownik musi być „kompletny"

`dbmgr importxml` (tryb wg rekordów) **nie uruchamia** logiki kreatora „Nowy pracownik" —
nie tworzy sam wierszy `PracHistoria2` ani `Adresy`. Bez nich kartoteka w GUI rzuca
`Object reference not set` (`Obywatelstwo GET`, `AdresZameldowania GET`, `Pokaż lokalizację`),
a pól adresowych nie da się edytować. Dlatego plik 01 jawnie zawiera — wzorem danych demo
enova (`<instalacja serwera>/Demo/100.Kadry.gold.xml`, pracownik 006):

- `<Historia addnew="true"><PracHistoria guid="…">` z back-referencją `<Pracownik>P1</Pracownik>` po `id` lokalnym
  (guid + `addnew` = idempotencja: ponowny import aktualizuje, nie duplikuje),
- `<Dodatkowe_historii><PracHistoria2><Host>PH1</Host> …Obywatelstwo, StanRodzinny, Wykształcenie, Dokument, PFRON…`,
- `<Adresy><AdresExt><Host>PH1</Host><Typ>Zameldowania|Zamieszkania|Korespondencyjny</Typ><Adres>…`
  (`Typ` to **enum**, nie liczba).

### Ubezpieczenia — zawsze zaznaczone społeczne i zdrowotne

Każde ubezpieczenie (`Emerytalne`, `Rentowe`, `Chorobowe`, `Wypadkowe`, `Zdrowotne`) musi mieć
komplet: `<Typ>Obowiazkowe</Typ>` **oraz jawnie** `<Od>{data zatrudnienia}</Od>` i `<Do>(max)</Do>`
(`Zdrowotne` dodatkowo `<Skladka>0.00</Skladka>`). Import wg rekordów **nie wylicza** daty `Od`
z `Ubezpieczenia.ObowiazkoweOd` (to robi dopiero logika biznesowa) — bez jawnego `<Od>` ubezpieczenie
zapisuje się z datą `1900-01-01` i w GUI figuruje jako niezaznaczone.

`business="true"` w `dbmgr importxml` **nie działa** (`Property set method not found`).

## Odwzorowane scenariusze

Scenariusze testowe i to, którzy pracownicy z bazy `Claude` je odwzorowują, są w arkuszu
[`ElementyPlac/Dodatek roczny - scenariusze testowe.xlsx`](../ElementyPlac/Dodatek%20roczny%20-%20scenariusze%20testowe.xlsx)
— arkusze **Scenariusze** (kolumna „Pracownik w bazie Claude") i **Dane testowe (Claude)**
(pracownik, nieobecności, oczekiwany wynik wypłaty 01/2027).

W skrócie: `TS-01` Kowalska Anna (1500), `TS-02` Wiśniewski Piotr (0), `TS-03` Wójcik Katarzyna (0),
`TS-05` Lewandowski Marek (1500; odwzorowuje też TS-06), `TS-09` Zielińska Agnieszka (0).
Wszyscy: pełny etat, kalendarz Standard, wydział „Główny wydział firmy", umowa na czas nieokreślony.

**Definicja elementu w bazie `Claude`** — od 2026-09-10 wczytana z eksportu bazy `Al`
(identyczna kolumna po kolumnie, w tym okres naliczania „co 12 miesięcy"); plik
`Dodatek roczny.dbinit.xml` doprowadzony do tego stanu.

## Stan weryfikacji

- Import wszystkich plików na `Claude` przechodzi; dane widoczne w `Pracownicy`,
  `PracHistorie`, `PracHistorie2`, `Adresy`, `Nieobecnosci`, `Dodatki`/`DodHistorie` (2026-09-03).
- **Kartoteka pracownika otwiera się w GUI bez błędów, adresy edytowalne** — potwierdzone.
- **Nie zweryfikowano przeliczeniem wypłaty** — `dbmgr` nie nalicza płac. Kolumnę „wynik
  końcowy" i zakładkę „Zapis obliczeń" trzeba potwierdzić przeliczeniem listy płac 01/2027 w GUI.
- Dodatek wchodzi na poziomie pracownika bez `Powiazania` z etatem — jeśli nie pojawi się
  na LPE, wskazać etat ręcznie.
