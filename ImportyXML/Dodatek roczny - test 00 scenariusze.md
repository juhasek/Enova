# Dodatek roczny — dane testowe (baza `Claude`)

Pliki importu XML zasilające bazę testową **`Claude`** danymi pod scenariusze testowe
elementu „Dodatek roczny" (opis elementu i pełna lista scenariuszy:
[`ElementyPlac/Dodatek roczny.md`](../ElementyPlac/Dodatek%20roczny.md)).

## Środowisko

Baza `Claude` na `localhost\SQL2025DEV` (= `ET-097\SQL2025DEV`), enova **2512.5.6** —
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

## Dlaczego pracownik musi być „kompletny"

`dbmgr importxml` (tryb wg rekordów) **nie uruchamia** logiki kreatora „Nowy pracownik" —
nie tworzy sam wierszy `PracHistoria2` ani `Adresy`. Bez nich kartoteka w GUI rzuca
`Object reference not set` (`Obywatelstwo GET`, `AdresZameldowania GET`, `Pokaż lokalizację`),
a pól adresowych nie da się edytować. Dlatego plik 01 jawnie zawiera — wzorem danych demo
enova (`<instalacja serwera>/Demo/100.Kadry.gold.xml`, pracownik 006):

- `<Historia><PracHistoria>` z back-referencją `<Pracownik>P1</Pracownik>` po `id` lokalnym,
- `<Dodatkowe_historii><PracHistoria2><Host>PH1</Host> …Obywatelstwo, StanRodzinny, Wykształcenie, Dokument, PFRON…`,
- `<Adresy><AdresExt><Host>PH1</Host><Typ>Zameldowania|Zamieszkania|Korespondencyjny</Typ><Adres>…`
  (`Typ` to **enum**, nie liczba).

`business="true"` w `dbmgr importxml` **nie działa** (`Property set method not found`).

## Odwzorowane scenariusze

| Kod | Pracownik | Scenariusz | Warunek wejściowy | Oczekiwany wynik wypłaty 01/2027 |
|---|---|---|---|---|
| `TS-01` | Kowalska Anna | TS-01 | cały 2026, brak nieobecności | Dodatek = **1500** |
| `TS-02` | Wiśniewski Piotr | TS-02 | + Nieob. nieusprawiedliwiona 11.05.2026 | Dodatek = **0** |
| `TS-03` | Wójcik Katarzyna | TS-03 | + Zwolnienie chorobowe 14–18.09.2026 | Dodatek = **0** |
| `TS-05` | Lewandowski Marek | TS-05 | + Urlop wypoczynkowy planowy 8–19.06.2026 (dozwolony) | Dodatek = **1500** |
| `TS-09` | Zielińska Agnieszka | TS-09 | zatrudniona od 1.03.2026 (niepełny rok) | Dodatek = **0** |

Wszyscy: pełny etat, kalendarz Standard, wydział „Główny wydział firmy", umowa na czas nieokreślony.

## Stan weryfikacji

- Import wszystkich plików na `Claude` przechodzi; dane widoczne w `Pracownicy`,
  `PracHistorie`, `PracHistorie2`, `Adresy`, `Nieobecnosci`, `Dodatki`/`DodHistorie` (2026-09-03).
- **Kartoteka pracownika otwiera się w GUI bez błędów, adresy edytowalne** — potwierdzone.
- **Nie zweryfikowano przeliczeniem wypłaty** — `dbmgr` nie nalicza płac. Kolumnę „wynik
  końcowy" i zakładkę „Zapis obliczeń" trzeba potwierdzić przeliczeniem listy płac 01/2027 w GUI.
- Dodatek wchodzi na poziomie pracownika bez `Powiazania` z etatem — jeśli nie pojawi się
  na LPE, wskazać etat ręcznie.
