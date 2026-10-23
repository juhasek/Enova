# Widok „Nadgodziny” — Dokumenty dodatkowe

Miejsce: **Kadry i płace → Kadry → Ewidencje → Dokumenty dodatkowe → Nadgodziny**.

Widok globalny (dla wszystkich operatorów) zestawiający e-wnioski o pracę w godzinach nadliczbowych.

## Jak używać

Lista „Dokumenty dodatkowe” pokazuje dokumenty wszystkich definicji. W panelu filtrów trzeba wybrać
**Definicja = e-wniosek o nadgodziny** (albo definicję klienckiego wniosku) — program nie pamięta tej
wartości, a widok nie ma własnego warunku na definicję (format widoku nie przewiduje filtra wierszy).
Dokumenty innych definicji mają w kolumnach wniosku puste wartości.

## Kolumny

| Kolumna | Pole wniosku | Uwagi |
|---|---|---|
| Kod | `Pracownik.Kod` | kod pracownika w enova |
| Imię i nazwisko | `Pracownik.ImięNazwisko` | |
| Data pracy | `WniosekData` | data wykonywania pracy nadliczbowej (jeden wniosek = jeden dzień) |
| Od godziny | `WniosekElement.OdGodziny` | |
| Do godziny | `DoGodziny` | |
| Uwagi | `WniosekOpis` | opis wpisany przez pracownika |
| Przełożony 1 | `Przelozony1` | pole definicji klienckiego wniosku (zastępuje `Kierownik`) |
| Przełożony 2 | `Przelozony2` | pole definicji klienckiego wniosku (zastępuje `Kierownik`) |
| Data decyzji | `DataDecyzji` | |
| Decyzja | `WniosekDokument.Stan` | stan wniosku: oczekujący / zaakceptowany / odrzucony (pole „Stan wniosku” z formularza) |
| Do przeniesienia | `DoPrzeniesienia` | odbiór (Tak) / zapłata (Nie) — **pole tylko w klienckim wniosku** |

Standardowy e-wniosek o nadgodziny ma „Do przeniesienia” wyłącznie jako parametr całej definicji
(`Pars.DoPrzeniesienia`), a nie jako pole pojedynczego wniosku. Kolumna wypełni się na definicji klienta,
w której pole `DoPrzeniesienia` jest właściwością wniosku. Jeśli tam to cecha, EditValue trzeba zmienić
na `{Features.DoPrzeniesienia}`.

## Zakładanie i zmiany

Kolumny są zdefiniowane w `widok-definicja.ps1` (wspólne dla obu skryptów). Zmiana kolumn istniejącego widoku:
edytować `widok-definicja.ps1` i uruchomić `aktualizuj-widok.ps1` (kopia bazy + nadpisanie viewform), potem restart `server.dll`.


`dodaj-widok.ps1` (PowerShell 5.1, plik z BOM) — kopia bazy, węzeł `foldery/…kadry.ewidencje.dokumenty dodatkowe.xml`
(nazwa ucięta od lewej do 40 znaków) + `tableviews/<guid>.viewform.xml` w `CfgNodes`/`CfgAttributes`.
Po zapisie trzeba zrestartować proces `server.dll`, bo serwer trzyma konfigurację w pamięci.
