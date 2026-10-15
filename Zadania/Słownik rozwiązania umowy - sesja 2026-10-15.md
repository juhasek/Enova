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

Generator jest w `Zadania/Generator słownika rozwiązania umowy/`. Jedno polecenie:

```powershell
& ".\Zadania\Generator słownika rozwiązania umowy\Odswiez.ps1" -Wersja 2610 -Baza Al
```

Skrypt kolejno:

1. dekompiluje `KodZwolnienia` i `KodPodstawyPrawnejZwolnienia` z `Soneta.KadryPlace.dll` danej
   wersji (ilspycmd do plików, bo na konsoli psują się polskie znaki),
2. eksportuje `PrzyczRozwUmow` z bazy do TSV w UTF-8 (System.Data.SqlClient, bo sqlcmd psuje znaki),
3. buduje konsolówkę DevExpress.Spreadsheet (biblioteki v24.1 z katalogu 2512.5.6), która parsuje
   `[Caption]` i nadpisuje Excel w `Zadania/`.

Sprawdzone 2026-10-15 na 2610 / Al: 34 kody, 65 podstaw, 28 przyczyn.

## Commity

- a3931a7: pierwszy Excel (generator jeszcze poza repo).
- 96debec: generator w repo (`Program.cs`, `GenSlownik.csproj`, `Odswiez.ps1`), Excel odświeżony
  skryptem (w arkuszu Info numer wersji enovy), wyjątek dopisany w `CLAUDE.md`.

`Odswiez.ps1` musi być zapisany w UTF-8 **z BOM** — PowerShell 5.1 czyta plik bez BOM jako ANSI
i psuje polskie znaki w domyślnej ścieżce wyjściowej.
