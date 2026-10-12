# Weryfikator normy okresu rozliczeniowego v2

Nowa wersja metody `A1WeryfikujNormeWOkresieRozliczeniowym` (plik obok: „Weryfikator normy okresu
rozliczeniowego”). Dodawana **obok** starej metody w klasie `A1WeryfikatoryKalendarza` (Dodatkowy kod
do kompilacji) — stara metoda i jej definicja zostają bez zmian, przełączenie = zmiana podpięcia
w kalendarzu.

## Co się zmienia względem v1

| | v1 | v2 |
|---|---|---|
| Zakres | cały okres rozliczeniowy | okres rozliczeniowy **przycięty do zatrudnienia na etacie** (`GetPlanOkresZatrudnienia`) |
| Miesiące minione | dni bez wpisu liczone jako rezerwa (do 12h / normy dobowej) | wszystkie dni przed bieżącym miesiącem liczą się wg planu jako stałe — **nie są rezerwą** |
| Komunikat | tylko suma | wskazuje **dni do poprawy** (maks. 10 + „i N kolejnych”) |

Norma kodeksowa (`KalkulatorKodeksowyPracownika`) sama ogranicza się do okresu etatu — przycięcie
dotyczy pętli po dniach (dni spoza zatrudnienia nie dają już rezerwy ani błędu braku kalendarza).

„Bieżący miesiąc” = miesiąc daty systemowej (`Date.Today`), tak jak w weryfikatorze dni wolnych.

## Komunikaty

- **Nadmiar:** „Przekroczono limit godzin (X) o (Y) w okresie rozliczeniowym (…) - pracownik. Skróć plan
  dni: 14.10 (12:00), 15.10 (10:00), …” — dni z wpisem od bieżącego miesiąca, od najdłuższego.
  Gdy brak takich dni: „brak - nadmiar pochodzi z miesięcy minionych”.
- **Niedobór:** „Nie da się już osiągnąć normy godzin (X) w okresie rozliczeniowym (…), brakuje (Y) - pracownik.
  Dni bez wpisu od bieżącego miesiąca: N (maks. Z). Wydłuż plan dni: 14.10 (+4:00), …” — dni robocze
  (pn–pt bez świąt) z wpisem od bieżącego miesiąca, którym brakuje do maksimum dobowego (12:00
  w równoważnym, `Nadgodziny.WartoscDobowa` w pozostałych). Gdy brak: „brak - trzeba zamienić dzień
  wolny na roboczy”.

## Kiedy się uruchamia

Rodzaj **Plan pracy** (40), poziom **Błąd**. Enova tworzy jeden weryfikator na parę pracownik + miesiąc
w sesji edycji i ocenia go przy **zapisie** (`KontrolaPlanuVerifier`, klucz `YearMonth`) — zmiany wielu
dni w jednym oknie sprawdzane są razem, stan pośredni nie blokuje. Blokuje natomiast zapis, w którym
okres nadal jest „niemożliwy” — przy zapisie dzień po dniu (np. pulpit) poprawki trzeba zrobić
w jednym zapisie.

## Definicja weryfikatora

```csharp
public override string Weryfikuj(Soneta.Kadry.Pracownik pracownik, WeryfikujEventArgs args) {
     return A1.Runtime.KadryPlace.WeryfikatoryKalendarza.TblCodeFiles.A1WeryfikatoryKalendarza.A1WeryfikujNormeWOkresieRozliczeniowymV2(new KalkulatorPlanu(pracownik), args.Miesiąc.LastDay);
}
```

Stan: kod kompiluje się na DLL 2512.5.6 (lokalna kompilacja całej klasy). Wgrany do bazy Al 2026-10-12:
metoda w CodeFiles ID 13, definicja `DefWeryfKalend` ID 2052 „A1_Norma w okresie rozl. v2” (Plan pracy,
nie podpięta). Definicja 37 (v1) bez zmian. Podpinać zamiennie z 37, nie razem. Niesprawdzony na żywo.
