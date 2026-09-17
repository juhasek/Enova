# Kalendarz: kwartalny okres rozliczeniowy od 1 lutego

Plik importu: `Kalendarz - kwartalny okres rozliczeniowy od 01-02.xml`
(import wg rekordów, `dbmgr importxml Claude <plik> --standard`).

## Co tworzy

Nową definicję kalendarza pracy **„Kwartał rozliczeniowy od 1.02"** (pole `Nazwa`
w tabeli `Kalendarze` ma limit 30 znaków, stąd skrócona nazwa) — GUID
`7f3a9c14-2e5b-4d86-b1a0-9c6e8d4f2071`, w bazie **Claude** wgrany jako `ID = 23`.

Kalendarz jest **kopią kalendarza „Standard"** tej bazy (wszystkie pola przepisane 1:1),
zmienione są tylko parametry okresu rozliczeniowego nadgodzin.

## Okres rozliczeniowy — 3 miesiące od 1 lutego

Zakładka kalendarza **Ogólne → grupa „Rozliczany okres"**:

| Pole formularza | Pole ORM | Wartość |
|---|---|---|
| Typ okresu | `Nadgodziny.TypOkresu` | `Miesięczny` |
| Okres | `Nadgodziny.Okres` | `3` (miesiące = kwartał) |
| Początek pierwszego okresu | `Nadgodziny.OdDnia` | `2026-02-01` |
| Rozliczenie w obrębie roku kalendarzowego | `Nadgodziny.Rocznie` | `False` |
| Opóźnienie rozliczenia (miesiące) | `Nadgodziny.Przesuniecie` | `0` |

Okresy rozliczeniowe wypadają: **01.02–30.04**, **01.05–31.07**, **01.08–31.10**,
**01.11–31.01**.

### Dlaczego rok w „OdDnia" nie ma znaczenia

Weryfikator `Nadgodziny.TypOkresuOdDniaVerifier` wymaga, by dla typu `Miesięczny`
pole `OdDnia` było puste albo ustawione na **1. dzień miesiąca**. Przy
`Rocznie = False` metoda `Kalendarz.WyliczOkresRoliczeniowyNadgodzin`
(i `Nadgodziny.OkresRozliczeniowyOkresowych`) bierze z `OdDnia` **tylko numer
miesiąca** (luty) i odmierza kolejne okresy co `Okres` miesięcy — rok `2026`
jest wyłącznie czytelną wartością startową. `Przesuniecie` to „opóźnienie
rozliczenia", a nie przesunięcie granic okresu — dlatego zostaje `0`.

## Weryfikacja po imporcie

- `dbmgr importxml` — OK (za drugim podejściem; pierwszą próbę odrzuciła baza:
  nazwa > 30 znaków).
- Ponowny import idempotentny — nadal 1 rekord o tym GUID.
- Odczyt `dbo.Kalendarze`: `NadgodzinyOkres = 3`, `NadgodzinyOdDnia = 2026-02-01`,
  `NadgodzinyTypOkresu = 0` (Miesięczny), reszta pól = jak „Standard"
  (`Bilansowanie = _50_100_SW`, `Rozliczanie = Zawsze100`, norma tygodniowa 40:00 itd.).
- **Do sprawdzenia w GUI:** przypisać kalendarz pracownikowi i przeliczyć wypłatę
  z nadgodzinami przechodzącymi przez granicę kwartału (np. kwiecień/maj) —
  `dbmgr` nie nalicza płac.
