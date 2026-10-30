# Inicjacja pozycji aktualizacji czasu pracy – sesja 2026-10-30

## Cel

Zgłoszenie: dzień z jedną strefą „Rozliczenie nadgodzin (firma)” nie przechodzi na dokument
aktualizacji. Dzień z dwiema strefami przechodzi poprawnie.

## Co zrobiono

- `Cechy/Inicjacja pozycji aktualizacji czasu pracy` (+ `.md`) przywrócone z `Produkcja` na `Testy`.
- Warunek kopiowania dnia rozszerzony o `d.Any(s => s.Definicja.Kod != "NB")`.
- Bez zmian w bazach.

## Ustalenia

- `Dzien.Praca` to tylko strefy `Wchodzi && Typ == Zwieksza`. Strefy rozliczeniowe mają `Wchodzi = 0`
  (sprawdzone w DefinicjeStref, ID 11), więc dzień wyglądał na pusty.
- `PrzeliczDzieńPracyAktualizacja` kopiuje plan dnia, dlatego strefę zastępował plan.
- `DzienPracyAktualizacja.Copy` ustawia czas z `recalc: false`, więc nie dodaje domyślnej strefy pracy.
- Dla dnia bez zapisów `kp[data]` ma puste strefy albo jest dniem planu, a dzień tylko z nieobecnością
  ma puste strefy. Te przypadki działają jak dotąd.

## Testy

- Warunek kompiluje się na DLL 2610.
- Cechy nie ma w FeatureDefs baz Al i Claude, więc nie było testu na dokumencie.

## Otwarte punkty

- Wgrać kod do bazy, w której działa cecha, i przejść tabelę testów z punktu 4 dokumentacji.
- Po akceptacji przenieść rozwiązanie na `Produkcja`.
