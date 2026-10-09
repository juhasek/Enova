# Dodatek „Rezerwy urlopowe” (A1.RezerwyUrlopowe) — Etap 2: Architektura modułu

Status: **projekt do akceptacji** · wersja 0.1 · 2026-10-09
Podstawa: Etap 1 (zaakceptowany), inwentaryzacja `scan-modules` / `scan-folders` na bibliotekach enova
2512.5.6 (36 modułów; brak tabel rezerw urlopowych w standardzie; w menu „Kadry i płace/Płace” jest tylko
„Planowane listy płac” — `PlanListyPlacViewInfo`).

**Zasada architektoniczna (kwestia nr 2):** źródłem prawdy są **własne tabele dodatku** (migawki); planowana
lista płac służy wyłącznie do naliczenia narzutów pracodawcy i księgowania.

## 2.1. Role użytkowników

| Rola | Zadania | Procesy |
|---|---|---|
| **Płace** | nalicza rezerwę i budżet, przegląda pozycje i zapis obliczeń, nalicza ponownie, **zamyka miesiąc** | rezerwa miesięczna, budżet roczny |
| **Kadry** | utrzymuje dane wejściowe: limity urlopowe, nieobecności, kartoteki (wydział, zaszeregowanie) | — (dane wejściowe) |
| **Zarząd / kontroling** | podgląd listy rezerw (tylko odczyt), porównanie miesięcy, eksport do Excela, wydruk | raportowanie |
| **Księgowość** | księguje planowane listy płac rezerwy (standardowy schemat księgowy, ewidencja RUEW) | zamknięcie miesiąca w FK |
| **Administrator / konsultant** | konfiguracja dodatku, inicjalizacja definicji, **otwarcie zamkniętego miesiąca** (uprawnienie specjalne) | wdrożenie, korekty |

## 2.2. Zakres konfiguracji modułu

**Narzędzia → Opcje → Kadry i płace → Rezerwy urlopowe** (zakładka dodatku, węzeł konfiguracji `CfgNodes`):

| Grupa | Parametr | Domyślnie (z pilotażu) |
|---|---|---|
| Podstawa | Wariant podstawy decydującej o kwocie: podstawa 1 / podstawa 2 / wyższa | podstawa 1 |
| | Zasadnicze w podstawie 1: nominalne z kartoteki / naliczone na listach | nominalne |
| | Liczba miesięcy podstawy 1 | 3 |
| | Miesiące bez wypłaty: pomijać / liczyć nominalnie | pomijać |
| Urlop | Limity uwzględniane w rezerwie (lista definicji limitów) | wypoczynkowy + dodatkowy |
| | Zaokrąglenie bieżącego proporcjonalnego (kolejny urlop) w górę do dnia | tak |
| | Ujemny stan urlopu: rezerwa 0 / dopuszczalna ujemna | 0 |
| Narzuty | Doliczać wpłatę pracodawcy PPK | nie |
| Budżet | Stan na 01.01: zaległy + pełny limit / tylko zaległy | zaległy + pełny limit |
| Powiązania | Element wynagrodzenia rezerwy / budżetu; definicja planowanej listy płac rezerwy / budżetu | inicjalizowane przez dodatek |
| Kontrole | Niezatwierdzone listy płac w miesiącu rezerwy: blokada / ostrzeżenie | blokada |

**Konfiguracja wstępna (inicjalizacja przy instalacji, `*.dbinit.xml` w dodatku):** elementy wynagrodzenia
„Rezerwa urlopowa” i „Budżet rezerwy urlopowej” (dodatek automatyczny, Rodzaj naliczania = Tylko planowane,
Do wypłaty = Nie, ZUS naliczać, PIT nie, Naliczanie = płatna z dołu, algorytm = klasa z dodatku), definicje
planowanych list REZURL / BUDREZURL (numeracja, ewidencja RUEW). Wszystkie pułapki inicjalizacji ustalone
w rozwiązaniu skryptowym (priorytet, pole Naliczanie, numeracja, scoping po elemencie) zapisane w danych inicjujących.

**Wymaganie środowiska:** ustawienie standardu „Rezerwy urlopowe do testów” = Tak (inaczej planowane listy
płac są niedostępne) — dodatek sprawdza je przed naliczeniem i informuje.

## 2.3. Kluczowe struktury danych

Decyzja „budować vs użyć istniejącego” po inwentaryzacji: tabele planowanych list płac (`PlanElementyWyp`)
nie mają pól na stan urlopu, podstawy, współczynnik ani statusu miesiąca, a mechanizm ma status testowy —
**budujemy własne tabele** (kwestia nr 9), powiązane z planowaną listą tylko referencją.

```
                        ┌──────────────────────────────┐
                        │ RezerwaUrlopowa  (root)      │  nagłówek: rodzaj + miesiąc
                        │ Rodzaj (Rezerwa/Budżet)      │
                        │ Okres (miesiąc), Stan        │  Naliczona → Zamknięta
                        │ parametry naliczenia, sumy   │
                        └──────────────┬───────────────┘
                                       │ 1:N (child, Guided)
                        ┌──────────────▼───────────────┐
  Kadry.Pracownik ◄─────┤ PozycjaRezerwyUrlopowej      ├────► Place.PlanowanyElementWypłaty
  Kadry.Wydzial  ◄──────┤ stan urlopu (h/dni), podstawy│        (narzuty, księgowanie)
  Core.CentrumKosztow ◄─┤ współczynnik, stawka, kwota, │
                        │ narzuty, zmiana m/m, zapis   │
                        └──────────────────────────────┘
```

- **RezerwaUrlopowa** — dokument operacyjny (korzeń `Guided = root`), jeden na rodzaj i miesiąc (klucz unikalny
  Rodzaj + Okres). Przechowuje: stan (Naliczona / Zamknięta), datę i operatora naliczenia, kopię parametrów
  konfiguracji użytych do naliczenia (audyt), współczynnik do ekwiwalentu, sumy.
- **PozycjaRezerwyUrlopowej** — szczegół (`child: Rezerwa → RezerwaUrlopowa`), jedna na pracownika w nagłówku.
  Migawka danych na moment naliczenia (wydział, MPK, wymiar etatu), stan urlopu, podstawy, kwota, narzuty,
  referencja do planowanego elementu wypłaty, zapis obliczeń.
- Wzorzec jak w standardzie: dokument (nagłówek) + pozycje; dane konfiguracyjne w `CfgNodes`, nie w tabelach.

## 2.4. Struktura menu i elementy interfejsu

```
Kadry i płace
└── Płace
    ├── Planowane listy płac        (standard)
    └── Rezerwy urlopowe            ← NOWY folder: lista nagłówków (Rodzaj, Okres, Stan, sumy)
         └── formularz rezerwy: Pozycje | Podsumowanie wg MPK | Parametry naliczenia | Planowane listy
Kadry i płace → Pracownik (kartoteka) → zakładka „Rezerwy urlopowe” (historia pracownika, opcjonalnie)
Narzędzia → Opcje → Kadry i płace → Rezerwy urlopowe   ← konfiguracja
```

**Czynności:**
- „Nalicz rezerwę urlopową” — na liście Rezerwy urlopowe i na liście Pracownicy (dla zaznaczonych); okno
  parametrów: rodzaj, miesiąc, zakres pracowników, ponowne naliczenie (nadpisz pozycje).
- „Zamknij miesiąc” / „Otwórz miesiąc” (otwarcie — uprawnienie specjalne).
- „Porównaj z poprzednim miesiącem” (kolumny zmiany na liście pozycji).
- Eksport do Excela (standard listy) i wydruk zestawienia wg MPK.

**Kluczowe dla sukcesu:** lista pozycji z kolumnami dla Zarządu i grupowaniem po MPK, porównanie m/m,
zapis obliczeń dostępny jednym kliknięciem z pozycji.

## 2.5. Relacje z modułami platformy Soneta

| Moduł | RowType / TableType | Rola w dodatku | Relacja |
|---|---|---|---|
| Kadry | `Pracownik` / `Pracownicy`; `PracHistoria`; `Wydzial` / `Wydzialy` | pracownik pozycji, wymiar etatu, wydział | lookup |
| Core | `CentrumKosztow` / `CentraKosztow` | MPK pozycji (z wydziału lub nadrzędnego) | lookup |
| Kalend | `LimitNieobecnosci` / `LimNieobecnosci`; `DefinicjaLimitu`; `Nieobecnosc` | stan urlopu, pierwszy urlop, wykorzystanie; symulacja limitu (`NaliczanieLimitowUrlopowych`) | odczyt |
| Place | `DefinicjaElementu` / `DefElementow` | elementy rezerwy i budżetu (algorytm = klasa dodatku) | konfiguracja |
| Place | `WypElement` / `WypElementy` | składniki podstawy 1 | odczyt |
| Place | `DefinicjaPlanowanejListyPłac`, `PlanowanaListaPłac`, `PlanowanyElementWypłaty` | naliczenie narzutów, księgowanie; referencja z pozycji | lookup |
| Place | `NaliczanieEkwiwalent`, `NaliczaniePlanowanychListPłac` (klasy) | podstawa 2, uruchomienie planu | wywołanie |
| Ksiega | schemat księgowy, ewidencja RUEW | księgowanie planowanych list | standard |

Rozszerzanie istniejących tabel: **nie** (ewentualnie zakładka w kartotece pracownika — tylko widok).

**Przepływ naliczenia:**
1. Kontrole (licencja, „Rezerwy urlopowe do testów”, zatwierdzone listy płac, współczynnik).
2. Dla każdego pracownika: kalkulator rezerwy (logika z rozwiązania skryptowego, jedna klasa) → pozycja.
3. Uruchomienie standardowego naliczenia planowanej listy płac; element rezerwy (algorytm = klasa dodatku)
   bierze kwotę z pozycji → silnik liczy narzuty.
4. Przepisanie narzutów i referencji planowanego elementu do pozycji; sumy w nagłówku.

## 2.6. Relacje z innymi systemami

Brak integracji zewnętrznych w v1. Wyjście: eksport listy do Excela, wydruk, księgowanie w module Księga
(lub przez istniejący eksport FK klienta). Dane dla BI (rezerwa w czasie, per MPK) — kierunek rozwoju.

## 2.7. Migracja danych

Brak. Historia zaczyna się od pierwszego naliczenia w dodatku; wcześniejsze miesiące można naliczyć wstecz
czynnością (dane źródłowe są w enova). Rozwiązanie skryptowe wycofywane po pilotażu (procedura w Etapie 3).

## 2.8. Wydajność i skalowalność

- **Wolumeny:** pozycje = pracownicy × 13 nagłówków / rok (12 rezerw + budżet). 1 000 pracowników → ~13 tys.
  pozycji / rok; 5 000 → ~65 tys. Nagłówków kilkanaście rocznie.
- **Indeksy:** pozycja (Rezerwa, Pracownik) unikalny; (Pracownik, Okres) dla historii pracownika i porównania m/m.
- **Przetwarzanie wsadowe:** naliczenie w czynności z paskiem postępu, per pracownik; równoległość jak
  w standardowym naliczaniu (osobne sesje per pracownik); symulacja limitów (budżet) w niezapisywanej sesji —
  najdroższa operacja, tylko dla rodzaju Budżet.
- **Wąskie gardła:** silnik planowanych list płac (pełne naliczenie pracownika) i `NaliczanieEkwiwalent`;
  mitygacja: naliczanie przyrostowe (tylko pracownicy ze zmianami) jako kierunek rozwoju, test wolumenowy w Etapie 3.
- Lista i porównanie m/m — odczyt z własnych tabel (bez przeliczeń).

## 2.9. Kierunki rozwoju

- Inne rezerwy pracownicze (odprawy emerytalne, nagrody jubileuszowe) — nagłówek z polem Rodzaj przygotowany.
- Wskaźniki BI, zadanie cykliczne naliczenia po zamknięciu miesiąca, naliczanie przyrostowe.
- Umowy cywilnoprawne (jeśli potrzebne).
