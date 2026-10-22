# Weryfikatory Al — sesja 2026-10-19 (norma tygodniowa 35/40h)

## 0. Ustalenie na start

Instrukcję dla klienta użytkownik pobiera **z repo** (`Weryfikatory czasu pracy/Instrukcja_Weryfikatory.docx`).
Wersja w Pobrane była tylko wzorem stylu i nie jest aktualizowana.

## 1. Zlecenie

Specyfikacja przekazana przez użytkownika: weryfikatory planu dla osób z orzeczonym stopniem niepełnosprawności:
- 35h/tydz. (7h/doba): stopień umiarkowany i znaczny,
- 40h/tydz. (8h/doba): stopień lekki,
- tydzień to 7 dni od pierwszego dnia okresu rozliczeniowego (art. 128 § 3 pkt 1 KP),
- kalendarz podstawowy: liczenie sztywne (każdy tydzień), kalendarz równoważny: średnio (wymiar okresu
  wg art. 130 KP z normą 7/8h),
- ostrzeżenie, gdy w równoważnym zaplanowano dobę dłuższą niż 7/8h.

## 2. Inwentaryzacja (zdekompilowane Soneta.KadryPlace 2610)

- Standardowy `WeryfikatorKalendarza.WeryfikujOdpoczynekTygodniowy` liczy tygodnie od
  `WyliczOkresRoliczeniowyNadgodzin(...).From` co 7 dni, czyli tak jak wymaga specyfikacja.
- `KalkulatorKodeksowy.Norma(FromTo)` daje wymiar z art. 130 KP (40h × pełne tygodnie + 8h × dni pon–pt
  − 8h × święta poza niedzielą). Przeskalowany przez d/8 daje wymiar dla normy 35/7.
- Godziny planowane: `KalkulatorPlanu(pracownik).Norma(...)` (strefy planu z bazy i bieżącej sesji),
  bez odejmowania nieobecności. Tak samo liczy standardowa „Norma w okresie rozliczeniowym”.
- Wpięcie: definicja weryfikatora rodzaju „Plan pracy” (`Weryfikuj(Pracownik, args)`, `args.Miesiąc`),
  jak 37 i 2044.

## 3. Wykonane w bazie Al

- CodeFiles 13 (`A1WeryfikatoryKalendarza`): metody `A1NormaTygodniowaNiepelnosprawnych(kp, YearMonth)`
  (+ prywatne `...WOkresie`, `A1NormaDobowaNiepelnosprawnego`) i `A1DobaNiepelnosprawnegoWRownowaznym(kp, Date)`.
  Kompilacja całej klasy na 2610/.NET 10 bez błędów. Stamp RuntimeProjects 29 podbity.
- DefWeryfKalend **3070** „A1_Norma tygodniowa niepełnosprawnych” (Plan pracy, prio 33) jako **Błąd** i
  **3071** „A1_Doba niepełnosprawnych (równoważny)” (Dzień planu, prio 34) jako Ostrzeżenie. Obie podpięte
  do tych samych 14 kalendarzy co 2069 (45, 47, 1070–1081) oraz do testowego 1086.
- Decyzje w kodzie: zgoda na pracę 8h oznacza normę 40/8. Przy zmianie stopnia w tygodniu norma tygodnia
  to średnia norm dni pon–pt. Niepełny tydzień liczony sztywno do W. Dzień bez orzeczenia liczony jako 8h.
  Wymiar etatu nie obniża limitu.
- Decyzja użytkownika: przy kalendarzu podstawowym **Błąd** (blokada zapisu).

## 4. Dane testowe

- Kalendarz testowy **1086** „TEST Podstawowy_3_8:15_N”: SQL-kopia 1072 z okresem 3-mies. od 1.03.2024
  (IX–XI) dla NP3. NP4 przeniesiony na kalendarz 1076 (równoważny 8h).
- Plan: `ImportyXML/Weryfikator norma tygodniowa niepelnosprawnych - plan.xml`. Każdy dzień roboczy ma
  jawny plan, bo kalendarze klienta „_N” nie generują planu z definicji kalendarza (dzień bez wpisu = 0h).
- Problemy po drodze:
  - import z `fromto="(wszystko)"` usunął wcześniejsze dni planu NP1–NP9 (z testów 2069),
  - w pierwszej wersji danych tydzień bazowy miał 35:30, co blokowało każdy zapis; teraz test wyzwala
    zmiana piątku, a tydzień bazowy ma dokładnie 35/40h,
  - kalendarz 1086 skopiowany z niepustym `Algorytm` dawał „Object reference” (`Kalendarz.GetTools()`)
    przy zapisie dnia planu NP3; pole wyczyszczone (zawierało same `return null`).
- Restart serwera wykonał operator (auto mode blokuje zabijanie `server.dll`).

## 5. Test na żywo: wszystkie scenariusze pozytywne

| # | Scenariusz | Wynik |
|---|---|---|
| 1 | NP1, podstawowy 35h, tygodnie po 35h | brak komunikatu |
| 2 | NP2, tydzień 8–14.10 = 36h | Błąd „…normę (35:00) … w tygodniu (8...14.10.2026) … zaplanowano (36:00)” |
| 3 | NP3, okres 3-mies., tydzień 29.09–5.10 = 36h | Błąd, tydzień przez granicę miesięcy |
| 4 | NP7, tydzień czw–śr 35h (pon–nd 42h) | brak komunikatu |
| 5 | NP9, równoważny, tydzień 42h, okres 447h | brak komunikatu 3070 |
| 6/7 | NP9, okres 449h | Błąd „(449:00) przekracza wymiar (448:00) … o (1:00)” (448 = 455 − 7 za 11.11) |
| 8 | NP5, lekki, tydzień 41h | Błąd, limit 40:00 |
| 8a | NP4, zgoda 8h, równoważny, 2.10 = 9h | Błąd (wymiar 176h) + Ostrzeżenie 3071 |
| 9 | NP6, bez orzeczenia, tydzień 48h | brak komunikatu 3070 |
| 10a | NP8, lekki od 16.11, tydzień 15–21.11 = 40h | brak komunikatu 3070 |
| 10b | NP8, tydzień 1–7.11 = 36h (umiarkowany) | Błąd, limit 35:00 |

## 6. Repo

- `Weryfikatory czasu pracy/KontrolePlanuZAlgorytmuKalendarza` (kod) i `.md` (sekcja 8b).
- `Weryfikatory czasu pracy/Scenariusze testowe weryfikatorow czasu pracy.xlsx`: arkusz „Norma tygodniowa 35-40h”.
- `Weryfikatory czasu pracy/Instrukcja_Weryfikatory.docx`: dwie nowe sekcje (generator .NET w scratchpadzie
  sesji, na wzór generatora z sesji zgód).
- Commity: 51f144c, 1ad271f, 3a22e34.

## 7. Do potwierdzenia z klientem

1. Niepełny tydzień na końcu okresu (podstawowy): limit W (35/40h) czy d × dni pon–pt.
2. Czy włączyć zablokowaną definicję 2039 (norma dobowa 7/8h niepełnosprawnych).
3. Zgoda na pracę 8h oznacza normę 40/8 (tak działa teraz).
4. Zmiana orzeczenia w środku tygodnia: średnia norm dni pon–pt.
5. Ograniczenie enova: weryfikatory kalendarza nie uruchamiają się przy zmianie kalendarza na etacie ani
   przy zmianie orzeczenia, tylko przy edycji planu.
