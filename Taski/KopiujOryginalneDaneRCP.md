# Task „Kopiuj oryginalne dane RCP”

## Problem

Integracja czasu pracy wczytuje odbicia z czytników do **Oryginalnych danych
RCP** (tabela `WejsciaWyjsciaO`). Do **Danych RCP** (`WejsciaWyjsciaI`), na
których pracuje algorytm RCP, trzeba je było przenosić ręcznie czynnością
**„Kopiuj oryginalne dane”**.

Standardowy import enova z pliku czytnika (`AutoImportRCP`) zapisuje od razu
do obu tabel — integracja klienta zapisuje tylko do oryginalnych.

## Co robi

Każdy zapis z Oryginalnych danych RCP kopiuje do Danych RCP **dokładnie raz**,
tak samo jak worker enova `WejsciaWyjsciaOWorker.Kopiuj` (zdekompilowany, 2610):

| Dane RCP | Skąd |
|---|---|
| Data | Data edytowana oryginału, a gdy pusta — Data |
| Godzina | Godzina edytowana oryginału, a gdy pusta — Godzina |
| Typ, Operacja, Czytnik RCP, Uwagi | z oryginału |
| Powiązanie z oryginałem (`WeWyO`) | oryginał |
| Zaimportowany | nie |
| Stan | Nieoznaczony |

Zapis **nie jest kopiowany**, gdy w Danych RCP już jest:

- zapis powiązany z tym oryginałem (wcześniej skopiowany workerem albo tym taskiem), albo
- zapis bez powiązania z tą samą datą, godziną i typem (np. ze standardowego
  importu z czytnika, który pisze do obu tabel bez powiązania).

Dzięki temu można bezpiecznie mieszać task z ręcznym kopiowaniem i nie powstają duplikaty.
Późniejsza zmiana oryginału ani usunięcie kopii z Danych RCP **nie są
synchronizowane** — każdy oryginał jest kopiowany raz.

## Dwie drogi uruchomienia

1. **Przy zapisie** (wyzwalacz na `WejsciaWyjsciaO`) — kopia powstaje w tej
   samej transakcji co oryginał. enova uruchamia wyzwalacze **tylko w sesji
   z operatorem** (`Saver.doEventsAndTasks`: `!IsInternal && Login.Operator != null`).
   Błąd kopiowania jest przechwytywany, żeby nie blokować zapisu oryginału.
2. **Harmonogram** — akcja automatyczna **„Kopiuj oryginalne dane RCP”**
   (`[AutoAction]`, tak jak istniejące zadanie „Import RCP”). Przegląda
   oryginalne zapisy z ostatnich **31 dni** (stała `DniWsteczHarmonogramu`)
   i dogania wszystko, czego nie skopiował wyzwalacz — np. gdy integracja
   zapisuje bez operatora albo prosto do bazy. Skopiowana liczba zapisów
   i błędy trafiają do logu harmonogramu (kategoria „Scheduler”).

Nie wiadomo, jak dokładnie zapisuje integracja klienta, dlatego są obie drogi:
wyzwalacz daje natychmiastową kopię, a harmonogram gwarantuje, że niczego nie zabraknie.

## Pliki

- `KopiujOryginalneDaneRCP` — sekcje kodu definicji zadania (IsEnable, IsActive,
  IsRealised, Action, pozostałe metody) w formacie znaczników `//** begin ... **`.
- `KopiujOryginalneDaneRCP - wyzwalacz WejsciaWyjsciaO` — kod wyzwalacza.

## Konfiguracja w enova

Nowa definicja zadania:

| Pole | Wartość |
|---|---|
| Nazwa | Kopiuj oryginalne dane RCP |
| Tabela | Pracownicy |
| Algorytm (kod) | tak |
| Typ definicji | **Brak** |
| Blokada | nie |

Sekcje kodu wkleić z pliku `KopiujOryginalneDaneRCP` (każdą w odpowiednie
miejsce edytora; metody `KopiujOryginalneDaneRCP`, `KopiujZapisRCP`,
`LogHarmonogramu` i stała — w „pozostałe metody”).

Wyzwalacz (przeznaczenie „Uniwersalny”):

| Tabela | Kod |
|---|---|
| WejsciaWyjsciaO | `KopiujOryginalneDaneRCP - wyzwalacz WejsciaWyjsciaO` |

Harmonogram: nowa definicja harmonogramu wskazująca tę definicję zadania i akcję
„Kopiuj oryginalne dane RCP”, częstotliwość np. co 15 minut (albo zaraz po
harmonogramie integracji).

Typ „Brak” + `IsEnable() == false` → enova nie tworzy rekordów zadań (Tasks)
przy zapisach (zob. `PracaWNormieZPlanu.md`).

## Status

- 2026-10-16: kod napisany na podstawie zdekompilowanego workera i importu RCP
  z enova 2610; **kompiluje się** lokalnie na bibliotekach 2512.5.6 razem
  ze skompilowanym projektem Business bazy (klasa bazowa `TaskCalculatorPracownicy`).
- Nie założony w żadnej bazie, nie testowany na żywo.
