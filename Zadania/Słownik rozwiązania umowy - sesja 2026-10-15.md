# Słownik rozwiązania umowy — sesja 2026-10-15

## Cel

Potrzebny Excel z kodami rozwiązania umowy i podstawami prawnymi, które uzupełnia się na pracowniku
(Etat → Rozwiązanie umowy). Program nie pokazuje tych wartości jako listy w słownikach.

## Ustalenia

- Kod zwolnienia, podstawa prawna i inicjatywa **nie są słownikiem w bazie**. To typy wyliczeniowe
  w `Soneta.KadryPlace.dll` (enova 2610):
  - `Soneta.Kadry.KodZwolnienia`: 20R–53W, w bazie zapisywana liczba (np. 22R → 22),
  - `Soneta.Kadry.KodPodstawyPrawnejZwolnienia`: 400–463 i 550, w bazie zapisywany sam kod,
  - `Soneta.Kadry.KodInicjatywyZwolnienia`: 0 Nie dotyczy, 1 Pracownik, 2 Pracodawca.
- Pola na pracowniku: `PracHistorie.EtatRozwiazanieUmowyKodZwolnienia`, `...PodstawaPrawna`,
  `...Inicjatywa`, `...PrzyczynaRozwUmowy`.
- Słownik przyczyn rozwiązania to tabela `PrzyczRozwUmow` (28 standardowych pozycji). Każda przyczyna
  podpowiada podstawę prawną, kod zwolnienia i inicjatywę. Pole Typ: 0 = Rozwiązanie, 1 = Wygaśnięcie
  (enum `TypPrzyczynyRozwUmowy`).

## Wynik

`Zadania/Rozwiązanie umowy - kody zwolnienia i podstawy prawne.xlsx` (commit a3931a7), arkusze:

1. **Kody zwolnienia**: 34 kody z opisem i wartością w bazie.
2. **Podstawy prawne**: 65 pozycji z pełnym brzmieniem przepisu.
3. **Przyczyny rozwiązania**: słownik przyczyn z bazy Al z rozwiniętą podstawą prawną.
4. **Info**: skąd dane i gdzie są zapisywane.

## Jak odświeżyć po aktualizacji enovy

1. `ilspycmd -t Soneta.Kadry.<typ> Soneta.KadryPlace.dll -r <katalog serwera> -o <katalog>`
   dla trzech typów powyżej. Dekompilować do plików, bo na konsoli psują się polskie znaki.
2. Wyeksportować `PrzyczRozwUmow` do TSV w UTF-8 (PowerShell + System.Data.SqlClient; sqlcmd psuje
   znaki).
3. Konsolówka DevExpress.Spreadsheet (wzorzec `Analiza/Generator`) parsuje `[Caption]` z plików i
   składa arkusz. Generator był jednorazowy, w scratchpadzie sesji, nie w repo.
