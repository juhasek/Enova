# Dodatek roczny — dane testowe (baza `Claude`)

Trzy pliki importu XML zasilające bazę testową **`Claude`** danymi pod scenariusze
testowe elementu „Dodatek roczny" (opis elementu i pełna lista scenariuszy:
[`ElementyPlac/Dodatek roczny.md`](../ElementyPlac/Dodatek%20roczny.md)).

## Środowisko

Baza `Claude` na `localhost\SQL2025DEV` (= `ET-097\SQL2025DEV`), enova **2512.5.6** —
prywatna piaskownica. Import:

```
C:\enovaServer\2512.5.6\Soneta.Products.Server.Standard\dbmgr.exe importxml Claude "<plik>" --standard
```

Kolejność wczytywania: najpierw `Dodatek roczny.dbinit.xml` (definicja elementu), potem 01 → 02 → 03.
Wszystkie pliki mają stałe GUID-y (`a1000000-…-00000000NN**`) i są **idempotentne**.

## Pliki

| Plik | Zawartość |
|---|---|
| `Dodatek roczny - test 01 pracownicy.xml` | 5 pracowników etatowych (Kod 2–6), pełny etat, kalendarz Standard, „Główny wydział firmy", umowa na czas nieokreślony. Import wg rekordów. |
| `Dodatek roczny - test 02 nieobecnosci.xml` | Nieobecności łamiące / niełamiące frekwencję (Kod 3, 4, 5). `<Nieobecnosc class="Soneta.Kalend.NieobecnośćPracownika,Soneta.KadryPlace">` w kolekcji `<Nieobecnosci addnew="true">`, selektor `<TypZrodla>Pracownik</TypZrodla>` obowiązkowy. |
| `Dodatek roczny - test 03 dodatek w kartotece.xml` | Element „Dodatek roczny" w kartotece każdego z piątki: kolekcja `<Dodatki>` → `<Dodatek><Historia><DodHistoria>` z Element / Okres `2026-01-01...2026-12-31` / Podstawa `1500.00 PLN`. |

## Odwzorowane scenariusze

| Kod | Pracownik | Scenariusz | Warunek wejściowy | Oczekiwany wynik wypłaty 01/2027 |
|---|---|---|---|---|
| 2 | Kowalska Anna | **TS-01** | cały 2026, brak nieobecności | Dodatek = **1500** |
| 3 | Wiśniewski Piotr | **TS-02** | + Nieob. nieusprawiedliwiona 11.05.2026 | Dodatek = **0** |
| 4 | Wójcik Katarzyna | **TS-03** | + Zwolnienie chorobowe 14–18.09.2026 | Dodatek = **0** |
| 5 | Lewandowski Marek | **TS-05** | + Urlop wypoczynkowy planowy 8–19.06.2026 (dozwolony wyjątek) | Dodatek = **1500** |
| 6 | Zielińska Agnieszka | **TS-09** | zatrudniona od 1.03.2026 (niepełny rok) | Dodatek = **0** |

## Stan weryfikacji

- Import wszystkich trzech plików na bazie `Claude` przechodzi; dane widoczne w tabelach
  `Pracownicy`, `PracHistorie`, `Nieobecnosci`, `Dodatki`/`DodHistorie` (2026-09-03).
- **Nie zweryfikowano przeliczeniem wypłaty** — `dbmgr` nie nalicza płac. Kolumnę „wynik
  końcowy" i zawartość zakładki „Zapis obliczeń" trzeba potwierdzić przeliczeniem listy płac
  01/2027 w GUI enova (to jedyna wiarygodna weryfikacja algorytmu Edytora — por. p. 7
  w `ElementyPlac/Dodatek roczny.md`).
- Dodatek wchodzi na poziomie pracownika bez `Powiazania` z konkretnym etatem — dla dodatku
  „Rodzaj wypłaty = Etat" powinno wystarczyć; jeśli nie pojawi się na LPE, wskazać etat ręcznie.
