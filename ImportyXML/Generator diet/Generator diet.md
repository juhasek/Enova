# Generator importu zestawień diet (Excel → XML)

## Co to jest

Narzędzie do wczytywania do enova365 zestawień diet pracownika z arkusza Excel.
enova nie ma wbudowanego importu tych danych z Excela (sprawdzone w bibliotekach
serwera 2512.5.6 — brak workera/czynności importu), standardowo wpisuje się je ręcznie.

Skład:

- `Generator diet.xlsx` — arkusze Instrukcja / Konfiguracja / Diety zagraniczne /
  Pakiet mobilnosci / Kraje,
- `modGeneratorDiet.bas` — makro VBA `GenerujDiety` (import do skoroszytu: Alt+F11 →
  File → Import File…, zapis jako `.xlsm`),
- `Przyklad - *.xml` — pliki dokładnie w postaci, jaką makro tworzy z przykładowych
  wierszy arkusza (zaimportowane próbnie na bazie Claude),
- `Test - pracownik DIET-01.xml` — pracownik testowy do bazy Claude.

**Makro nie łączy się z bazą SQL.** Pracownik jest wskazywany po kodzie
(`<Pracownik where="Kod=...">`), kraj — po kodzie z arkusza „Kraje”.

Import pliku: `dbmgr importxml <baza> "<plik>"` (import wg rekordów, `<session>`).
Uwaga: `dbmgr` przy błędzie importu kończy się kodem 0 — trzeba czytać wypisany
komunikat `Error:`.

## Dwa zestawienia w enova

| | Diety zagraniczne | Pakiet mobilności |
|---|---|---|
| Tabela / klasa | `ZestawDietZagr` / `Soneta.Kalend.ZestawienieDietZagr` | `ZestDietPaMob` / `Soneta.Kalend.ZestDietPakietMobil` |
| Kolekcja na pracowniku | `ZestawieniaDiet` (FromToSubTable) | `ZestDietPakietMobil` (SubTable) |
| Pola | `Okres` (od–do), `Diety` (int) | `KrajOddelegowania`, `Czas`, `DzienMiesiaca`, `Diety`, `DietyKorekta`, `WartoscDiet`, `KorektaReczna`, `WartoscDietPIT` |
| Host | Pracownik lub Umowa | Pracownik lub Umowa |
| Generator obsługuje | Pracownika | Pracownika |

Rekordy nie są guidowane; jedynym kluczem pakietu mobilności jest unikalny
`KrajOddelegowania + DzienMiesiaca + Host`.

### Diety zagraniczne

- W GUI enova pilnuje (setter `Okres`): okres w **jednym miesiącu kalendarzowym**,
  w okresie zatrudnienia z `Etat.RodzajZatrudnienia` = *Pracownik za granicą* /
  *Pracownik tymczasowy za granicą* (`Pracownik.PracaZaGranicą`), poza blokadą okresu.
- **Import wg rekordów tych reguł NIE sprawdza** (sprawdzone: przyjął okres
  15.10–10.11). Makro sprawdza miesiąc, kolejność dat, nakładanie się okresów
  i liczbę całkowitą; okresu zatrudnienia i blokady bez bazy sprawdzić się nie da.

### Pakiet mobilności

- `DzienMiesiaca` = ostatni dzień miesiąca (`Miesiac` w enova to `DzienMiesiaca.ToYearMonth()`).
- Przy naliczaniu podatku/ZUS (`Pracownik.GetWartoscDietPakietMobil`) — tylko dla
  etatu z `PracownikZaGranicą && PakietMobilnosci` (lub umowy z pakietem):
  - `KorektaReczna = false` → wartość = stawka diety kraju (`StawkaDelegacji(data).Dieta`,
    albo wirtualna dieta) × (`Diety` − `DietyKorekta`), przeliczona na PLN;
  - `KorektaReczna = true` → `WartoscDiet` (ZUS) / `WartoscDietPIT` (podatek) wprost.
  Dlatego makro wysyła kwoty tylko przy korekcie ręcznej (inaczej 0).
- `Czas` przyjmuje wartości powyżej 24h (`120:30` → 7230 min).

## Jak import zachowuje się w kolekcjach (sprawdzone próbnie na bazie Claude)

| Zapis w XML | Diety zagraniczne | Pakiet mobilności |
|---|---|---|
| kolekcja z `addnew="true"` | dopisuje; ponowny import **dubluje** | dopisuje; pozycja już istniejąca (kraj+miesiąc) → **cały plik odrzucony**, nic nie zapisane |
| kolekcja bez `addnew` | zastępuje zestawienia pracownika **tylko w zakresie `fromto` sesji** | zastępuje **całą** kolekcję pracownika (fromto nie działa) |
| `where`/`key` na pozycji | — | niemożliwe: `SessionReader.FindWhere` obsługuje jedno pole z kluczem, a klucz jest 3-polowy |

Stąd wybrane tryby:

- **Diety zagraniczne** — bez `addnew`, `fromto` = od 1. dnia pierwszego do ostatniego
  dnia ostatniego miesiąca w pliku. Plik zastępuje zestawienia pracowników z pliku
  w tych miesiącach; inne miesiące i inni pracownicy zostają (test: listopad spoza
  pliku przetrwał). Ponowny import nie dubluje. Każdy pracownik występuje w pliku
  jednym elementem `<Pracownik>` (drugi zastąpiłby pierwszy).
- **Pakiet mobilności** — `addnew="true"`. Poprawki istniejącej pozycji: w enova
  albo usunięcie i ponowny import. Całkowite wyczyszczenie pakietu pracownika:
  `<Pracownik where="Kod=..."><ZestDietPakietMobil /></Pracownik>`.

Przy jakimkolwiek błędzie w arkuszu makro nie tworzy żadnego pliku (pominięcie wiersza
w trybie „zastąp” skasowałoby istniejący wpis bez wstawienia nowego).

## Kraje

Arkusz „Kraje” = słownik `KrajeDelegacji` (122 kraje). GUID-y są systemowe
(`00000000-0019-0001-<ID>-000000000000`) — identyczne w bazach Claude i Al.
Waluta z arkusza jest domyślną walutą `WartoscDiet`, gdy kolumna Waluta jest pusta.

## Typowe problemy

- **Arkusz „Dane”, arkusz „Bledy” z „Strefa 1: brak czasu.”** — do skoroszytu
  zaimportowano `modGeneratorCzasuPracy.bas` (generator czasu pracy) zamiast
  `modGeneratorDiet.bas`. Usunąć ten moduł, zaimportować `modGeneratorDiet.bas`,
  przywrócić nazwę arkusza „Diety zagraniczne”, usunąć arkusz „Bledy”, uruchomić
  `GenerujDiety` (2026-10-08, pierwszy test użytkownika).
- Brak któregoś arkusza (zmieniona nazwa) — makro wypisuje brakujące nazwy i kończy.

## Status

- Format XML i zachowanie importu: **sprawdzone** `dbmgr importxml` na bazie Claude
  (pracownik DIET-01, ID 1021) — 2026-10-08.
- Makro VBA: **działa** — użytkownik wygenerował XML w Excelu (2026-10-08, po poprawce `mOd` → `miesOd`).
- Nieprzetestowane: import pliku wygenerowanego przez makro u użytkownika, import tych plików z GUI enova (zamiast `dbmgr`), zestawienia na umowach.
