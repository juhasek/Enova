# Dodatek „Rezerwy urlopowe” (A1.RezerwyUrlopowe) — Etap 1: Wizja i kontekst biznesowy

Status: **projekt do akceptacji** · wersja 0.1 · 2026-10-09
Punkt wyjścia: działające rozwiązanie skryptowe w repo (`Rezerwy urlopowe/`) — algorytm elementu
wynagrodzenia na planowanej liście płac, Dodatkowy kod kompilacji, 16 cech widoku. Dodatek przenosi
je do skompilowanego produktu i rozbudowuje.

## 1.1. Cel biznesowy projektu

Dodatek dostarcza firmom korzystającym z enova365 Kadry i Płace gotowe narzędzie do miesięcznej
**rezerwy urlopowej** (zobowiązanie z tytułu niewykorzystanych urlopów) oraz **budżetu rezerwy** na kolejny
rok. Kluczowe jest to, by rezerwa liczyła się z danych już obecnych w enova (limity urlopowe, wypłaty,
konfiguracja współczynnika) w sposób powtarzalny, sprawdzalny i zgodny z polityką rachunkowości klienta —
bez arkuszy liczonych ręcznie. Zasady wyliczenia (podstawy, zaokrąglenia, warianty) są **parametrami
w konfiguracji**, a nie kodem, więc ten sam dodatek obsłuży różne firmy. Wynik trafia do Zarządu jako
czytelne zestawienie z historią miesięcy oraz do księgowości przez standardowe planowane listy płac.

## 1.2. Profil klienta docelowego

- Firmy średnie i duże (od kilkudziesięciu do kilku tysięcy pracowników etatowych), które sporządzają
  sprawozdania miesięczne lub kwartalne i muszą wyceniać rezerwę urlopową (UoR, MSR 19).
- Branże z dużą liczbą pracowników i rotacją: handel, produkcja, logistyka, usługi.
- Użytkownicy enova365 z modułem Płace w licencji Platynowej (planowane listy płac).

## 1.3. Korzyści dla klienta

- **Rezerwa w kilka minut zamiast dni** — naliczenie dla całej firmy jedną czynnością, po zamknięciu list płac.
- **Spójność z danymi kadrowymi** — stan urlopu wprost z limitów enova, podstawa z wypłat; brak przepisywania.
- **Przejrzystość dla Zarządu i audytora** — zestawienie per pracownik i MPK, historia miesięcy, zmiana
  rezerwy miesiąc do miesiąca, pełny zapis obliczeń dla każdej pozycji.
- **Narzuty pracodawcy liczone przez silnik płac** — te same zasady co przy wypłacie (limity, zwolnienia FP/FGŚP).
- **Budżet na kolejny rok** — symulacja stanu urlopu na 01.01 bez naruszania danych.
- **Dopasowanie do polityki firmy bez programisty** — wariant podstawy, zaokrąglenia, składniki w konfiguracji.
- **Księgowanie** — rezerwa i jej rozwiązanie przez standardowy schemat księgowy planowanych list płac.

## 1.4. Najważniejsze funkcjonalności (priorytetyzacja)

**Krytyczne (must-have)**
1. Naliczenie rezerwy urlopowej za miesiąc dla wybranych pracowników (czynność z oknem parametrów:
   miesiąc, rodzaj: rezerwa / budżet).
2. Stan urlopu: zaległy + bieżący proporcjonalny − wykorzystany, z regułami zaokrąglenia (kolejny urlop
   w górę do dnia, pierwszy urlop bez zaokrąglenia), urlop wypoczynkowy i dodatkowy.
3. Podstawy: podstawa 1 (średnia z 3 miesięcy z wypłatą, wartości nominalne w niepełnym miesiącu),
   podstawa 2 (standard ekwiwalentu enova), współczynnik do ekwiwalentu z konfiguracji.
4. Narzuty pracodawcy z silnika płac (planowana lista płac).
5. **Konfiguracja** w Narzędzia → Opcje: wariant podstawy, zasadnicze nominalne/naliczone, zaokrąglenia,
   ujemny stan, limity uwzględniane, element i definicja planowanej listy.
6. Lista „Rezerwy urlopowe” z kolumnami dla Zarządu (Kod, Imię, Nazwisko, MPK, urlop zaległy / bieżący /
   wykorzystany, podstawa, kwota, narzuty) i eksportem do Excela.
7. Zapis obliczeń per pracownik (obecny układ sekcji [1]–[5]).

**Ważne (should-have)**
8. **Historia — migawki miesięczne** we własnej tabeli: zamknięcie miesiąca (blokada zmian), porównanie
   z poprzednim miesiącem (zmiana rezerwy = zawiązanie / rozwiązanie), raport zmian dla Zarządu.
9. Budżet rezerwy (symulacja limitu na 01.01 roku następnego) jako drugi rodzaj naliczenia.
10. Raport/wydruk zestawienia rezerwy (per MPK, sumy).
11. Kontrole przed naliczeniem: niezatwierdzone listy płac w miesiącu, brak współczynnika w konfiguracji,
    brak licencji / wyłączone planowane listy płac.

**Opcjonalne (nice-to-have)**
12. PPK pracodawcy w narzutach (parametr).
13. Rezerwy na inne świadczenia (odprawy emerytalne, nagrody jubileuszowe) na tej samej infrastrukturze.
14. Wskaźniki BI (rezerwa w czasie, per MPK).
15. Naliczanie automatyczne (zadanie cykliczne po zamknięciu miesiąca).

## 1.5. Pokrycie przez standardowe funkcjonalności platformy

Weryfikacja na podstawie dekompilacji bibliotek enova 2512.5.6 (Soneta.KadryPlace, Soneta.Ksiega) wykonanej
przy budowie rozwiązania skryptowego; formalna inwentaryzacja `scan-modules` / `scan-folders` — otwarta
kwestia nr 1 (nieblokująca).

| Funkcjonalność | Pokrycie | Moduł / tabela platformy | Uwagi |
|---|---|---|---|
| Przechowanie rezerwy, narzuty, księgowanie | Częściowe | Płace — `DefPlanListPlac`, `PlanListyPlac`, `PlanowaneWyplaty`, `PlanElementyWyp`; ewidencja RUEW | Mechanizm „rezerw” enova; widoczny tylko przy włączonym „Rezerwy urlopowe do testów” (status testowy) — ryzyko R1 |
| Stan urlopu (limity) | Pełne (dane) | Kalend — `LimNieobecnosci`, `DefinicjeLimitow`, `Nieobecnosci` | Brak gotowej „proporcji na dzień” — liczy dodatek |
| Podstawa 2 (ekwiwalent) | Pełne | Płace — `NaliczanieEkwiwalent` | Wywoływane z algorytmu elementu |
| Współczynnik do ekwiwalentu | Pełne | Konfiguracja Płace → Nieobecności → Średnia norma miesięczna | — |
| Symulacja limitu na rok następny | Pełne (mechanizm) | Kalend — `NaliczanieLimitowUrlopowych` | Uruchamiane w niezapisywanej sesji |
| Podstawa 1 (średnia 3 mies. wg zasad klienta) | Brak | — | Do zbudowania (logika z rozwiązania skryptowego) |
| Lista rezerw dla Zarządu | Brak | Planowane listy płac pokazują listy, nie płaską listę pozycji | Do zbudowania (lista + ewentualnie cechy) |
| Historia / migawki, zamknięcie miesiąca | Brak | — | Do zbudowania (własna tabela) |
| Konfiguracja zasad | Brak | — | Do zbudowania (zakładka Opcji) |

**Wniosek:** dodatek nie zastępuje planowanych list płac — korzysta z nich jako silnika narzutów i warstwy
księgowej. Dostarcza: logikę wyliczenia, konfigurację, czynność naliczenia, listę, historię i kontrole.

## 1.6. Scenariusze użytkownika

**S1. Rezerwa miesięczna (kadry/płace, co miesiąc)**
1. Po zatwierdzeniu list płac za miesiąc otwiera Kadry i płace → Rezerwy urlopowe → „Nalicz rezerwę”.
2. Wybiera miesiąc i zakres pracowników (domyślnie zatrudnieni na koniec miesiąca).
3. Dodatek sprawdza warunki (zatwierdzone listy, współczynnik), nalicza planowaną listę płac i zapisuje migawkę.
4. Kadrowa przegląda listę, w razie korekt nalicza ponownie; na koniec „Zamyka miesiąc”.

**S2. Raport dla Zarządu**
1. Zarząd / kontroling otwiera listę rezerw za miesiąc, grupuje po MPK, porównuje z poprzednim miesiącem.
2. Eksportuje do Excela lub drukuje zestawienie.

**S3. Budżet rezerwy (sierpień)**
1. Płace uruchamiają „Nalicz rezerwę” z rodzajem „Budżet”, okres 08/RRRR.
2. Dodatek symuluje limity na 01.01 roku następnego, liczy podstawy i budżet; nie zmienia limitów w bazie.

**S4. Konfiguracja przy wdrożeniu (konsultant)**
1. Narzędzia → Opcje → Rezerwy urlopowe: wariant podstawy, zaokrąglenia, limity, element / definicja planowanej listy.
2. Inicjalizacja definicji (element, planowane listy, numeracja) przy instalacji dodatku.

**S5. Kontrola audytora**
1. Dla wybranego pracownika otwiera pozycję rezerwy → Zapis obliczeń (limity, miesiące podstawy, współczynnik, wynik).

## 1.7. Procesy biznesowe

- **Realizowane w całości:** miesięczne naliczenie i zamknięcie rezerwy urlopowej; roczny budżet rezerwy;
  raportowanie rezerwy dla Zarządu.
- **Realizowane częściowo (z innymi elementami platformy):** zamknięcie miesiąca płacowego (warunek: zatwierdzone
  listy płac); księgowanie rezerwy (schemat księgowy planowanych list płac w module Księga); limity urlopowe
  (Kadry — naliczanie limitów, nieobecności).

## 1.8. Ograniczenia modułu i wymagania licencyjne

- Wymagana licencja enova365 **Płace Platynowe** (planowane listy płac).
- Standard pokazuje planowane listy płac tylko przy włączonym ustawieniu „Rezerwy urlopowe do testów” — dodatek
  musi to sprawdzać i komunikować.
- Pracownicy etatowi; umowy cywilnoprawne poza zakresem v1.
- Wersja platformy: 2512.x (bieżąca u pilotażowego klienta); każda kolejna wersja enova wymaga rekompilacji
  i testu regresji.
- Dodatek wgrywany jako DLL (ExtPath) — wymaga akceptacji klienta dla rozwiązań skompilowanych.

## 1.9. Założenia i zależności

- **Założenia:** limity urlopowe naliczane w enova w godzinach; współczynnik do ekwiwalentu wpisywany co roku
  w konfiguracji; listy płac zatwierdzane przed naliczeniem rezerwy; enova on-premise (wgranie DLL przez ExtPath).
- **Zależności:** pilotażowy klient (dane testowe, akceptacja zasad); rozwiązanie skryptowe jako wzorzec obliczeń
  i źródło scenariuszy testowych (30 scenariuszy RU-01…RU-30); dostępność SDK / bibliotek enova w wersji klienta.

## 1.10. Ryzyka projektu

| Nr | Ryzyko | P-stwo | Wpływ | Mitygacja |
|---|---|---|---|---|
| R1 | Planowane listy płac mają w enova status „do testów” — zmiana lub wycofanie w kolejnej wersji | Średnie | Wysoki | Warstwa dostępu do planowanych list w jednym miejscu; własna tabela migawek niezależna od planowanych list; test regresji na każdej wersji |
| R2 | Klient nie akceptuje DLL | Średnie | Wysoki | Potwierdzić przed startem; rozwiązanie skryptowe zostaje jako wariant awaryjny |
| R3 | Silnik planu usuwa niezatwierdzone wypłaty przed naliczeniem | Wysokie | Średni | Kontrola przed naliczeniem w czynności (blokada lub ostrzeżenie) |
| R4 | Różne polityki rachunkowości klientów (podstawa, zaokrąglenia) | Wysokie | Średni | Parametry w konfiguracji; domyślne wartości zgodne z pilotażem |
| R5 | Wydajność dla tysięcy pracowników (silnik planu + symulacja limitów) | Średnie | Średni | Testy wolumenowe; naliczanie w partiach, postęp w czynności |
| R6 | Niejawne zachowania silnika (priorytet, scoping dodatku, numeracja, pole Naliczanie) | Średnie | Średni | Udokumentowane już w rozwiązaniu skryptowym; inicjalizacja definicji przez dodatek |

## 1.11. Kryteria akceptacji

- **Funkcjonalne:** wszystkie scenariusze RU-01…RU-30 dają wynik zgodny z arkuszem scenariuszy (te same kwoty
  co rozwiązanie skryptowe przy tych samych parametrach); S1–S5 wykonywalne bez programisty.
- **Wydajnościowe (orientacyjnie):** naliczenie rezerwy dla 1 000 pracowników < 10 min; otwarcie listy rezerw za
  miesiąc < 3 s.
- **Jakościowe:** testy integracyjne dla czynności naliczenia, algorytmu, symulacji budżetu i zamknięcia miesiąca;
  brak błędów krytycznych w pilotażu przez 2 zamknięcia miesiąca.

## 1.12. Harmonogram i kamienie milowe (orientacyjnie)

| Faza | Zakres | Szacunek |
|---|---|---|
| Planowanie | Etapy 1–3, zamknięcie otwartych kwestii | 8–12 h |
| MVP | Rusztowanie, konfiguracja, czynność naliczenia, algorytm (przeniesienie 1:1), lista | 35–45 h |
| Historia | Tabela migawek, zamknięcie miesiąca, porównanie, raport | 20–25 h |
| Testy | Testy integracyjne, regresja scenariuszy RU, wolumen | 15–20 h |
| Pilotaż i dokumentacja | Wdrożenie u pilotażowego klienta, instrukcja, 2 zamknięcia miesiąca | 10–15 h |
| **Razem** | | **ok. 90–115 h** |

Kamienie: (1) akceptacja planu, (2) MVP zgodny z RU-01…RU-30, (3) historia i zamknięcie miesiąca,
(4) koniec testów integracyjnych, (5) pilotaż – 2 miesiące, (6) wersja produkcyjna.
