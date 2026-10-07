# Generator importu czasu pracy (strefy dnia pracy)

## Co to jest

Narzędzie zamieniające prosty arkusz od klienta (kod pracownika, data,
strefy pracy) na plik XML importu **rzeczywistego czasu pracy** do enova365
— zakładka **Kalendarz / Czas pracy** w kartotece pracownika. Składa się z:

- `Generator czasu pracy.xlsx` — arkusze Instrukcja / Konfiguracja / Dane / Bledy,
- `modGeneratorCzasuPracy.bas` — makro VBA `GenerujCzasPracy`,
- `Czas pracy - test PP-01 wrzesien 2026.xml` — plik testowy (format wyjścia makra).

Strefy **„Praca w normie” nie ma w pliku** — dopisuje ją Task
`Taski/PracaWNormieZPlanu` przy zapisie dnia pracy, na godziny z planu pracy
niepokryte strefami z pliku.

Makro nie łączy się z bazą danych i nie używa GUID-ów — pracownik po kodzie,
strefa po nazwie definicji (ten sam mechanizm, co `Generator planu pracy`).

## Arkusz „Dane”

| Kolumna | Zawartość |
|---|---|
| A | Kod pracownika |
| B | Data dnia |
| C, D, E | Strefa 1 nazwa, Strefa 1 godzina od, Strefa 1 czas |
| F, G, H | Strefa 2 nazwa, godzina od, czas |
| … | analogicznie, po 3 kolumny na strefę |
| BH, BI, BJ | Strefa 20 nazwa, godzina od, czas |

- **Nazwa** — nazwa definicji strefy dokładnie jak w enova.
- **Godzina od** — opcjonalna, zależnie od konfiguracji strefy (np. „Dyżur
  domowy” bez godziny). Może przekraczać 24:00 (do 47:59) — tak enova zapisuje
  zdarzenia doby rozpoczętej dzień wcześniej.
- **Czas** — czas trwania strefy (nie godzina „do”), wymagany, > 0:00 i ≤ 24:00.

| Nazwa | Godzina od | Czas | Wynik |
|---|---|---|---|
| Nadgodziny 50% | 16:00 | 2:00 | strefa od 16:00, czas 2:00 |
| Dyżur domowy | | 4:00 | strefa bez godziny od, czas 4:00 |

Godziny mogą być wpisane jako godzina Excela (kolumny mają format `[g]:mm`)
albo jako tekst `G:MM` (także `G:MM:00`). Strefa z trzema pustymi kolumnami
jest pomijana.

## Błędy

Wiersz z jakimkolwiek błędem jest **pomijany w całości** i wypisany w arkuszu
„Bledy” (wiersz, kolumna, wartość, opis). Powód: import enova dla istniejącego
dnia **kasuje wszystkie jego strefy** i wpisuje strefy z pliku — częściowy
import dnia usunąłby pozostałe strefy. Z tego samego powodu pomijane są:

- wiersze bez żadnej strefy,
- powtórzony ten sam pracownik + dzień (import zostawiłby tylko jeden z wierszy).

Wykrywane błędy strefy: godziny bez nazwy, brak czasu, czas nie w formacie
godziny, czas zerowy lub > 24:00, godzina od nie w formacie godziny lub ≥ 48:00.
Makro **nie sprawdza**, czy nazwa strefy istnieje w enova — to zgłosi import
(„Definicja strefy o nazwie '...' nie została znaleziona”).

## Format XML (potwierdzony w kodzie importu)

Zdekompilowane `Soneta.CzasPracy.Utils.dll` (`Document.ImportDniaPracy`,
`Root.DzienPracy`) i sprawdzone deserializacją pliku testowego przez
oryginalną klasę `Root`:

```xml
<?xml version="1.0" encoding="Unicode" ?>
<Root xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
<CzasPracy>
<DzienPracy>
<Pracownik>PP-01</Pracownik>
<Data>2026-09-04</Data>
<Strefy>
<StrefaPracy Definicja="Praca poza normą" OdGodziny="15:00" Czas="4:00" />
<StrefaPracy Definicja="Dyżur domowy" Czas="4:00" />
</Strefy>
</DzienPracy>
</CzasPracy>
</Root>
```

- Dzień pracy **nie ma definicji dnia** (w odróżnieniu od dnia planu).
- Dzień istnieje → strefy kasowane i wpisywane od nowa; nie istnieje → tworzony.
- `OdGodziny` pominięte → strefa bez godziny od (dozwolone).
- Celowo **bez** `<OdGodziny>`/`<Czas>` na poziomie dnia — liczą się razem ze
  strefami przy kontroli nachodzenia stref (lekcja z generatora planu pracy).
- Plik zapisany jako Unicode (UTF-16LE z BOM).

Import: **Plik → Importuj zapisy → Import czasu pracy i wynagrodzeń**
(wymaga rozszerzenia Soneta.CzasPracy.Migrator/Utils w bazie).

## Plik testowy (baza Claude, pracownik PP-01)

PP-01 ma plan wrzesień 2026: dni robocze 8:00–12:00 i 13:00–17:00. Oczekiwany
wynik po imporcie i działaniu Taska:

| Dzień | Strefa z pliku | Oczekiwana „Praca w normie” |
|---|---|---|
| 01.09 | Nadgodziny 50% 17:00 2:00 (nie wchodzi do czasu pracy) | 8–12, 13–17 |
| 02.09 | Praca zdalna 8:00 4:00 (wchodzi, zwiększa) | 13–17 |
| 03.09 | Dyżur domowy 4:00 (bez godziny od) | 8–12, 13–17 |
| 04.09 | Praca poza normą 15:00 4:00 | 8–12, 13–15 |
| 05.09 (sobota, brak planu) | Praca poza normą 8:00 6:00 | brak |
| 07.09 | Praca poza normą 7:00 11:00 (pokrywa cały plan) | brak |

## Status

- Format XML — potwierdzony kodem importu i deserializacją (bez importu na żywo).
- Import pliku testowego + Task — POTWIERDZONE na żywo w bazie Claude (2026-10-07).
- Makro VBA (układ 3 kolumny na strefę, 2026-10-07) — nieprzetestowane w Excelu (brak Excela w środowisku repo).
