# Analiza przedwdrożeniowa – Kadry, Płace, Czas pracy (enova365)

Szablon do prowadzenia spotkania analitycznego z klientem. Nie jest to ankieta
do wysłania mailem – to materiał, po którym konsultant prowadzi warsztat,
notując odpowiedzi i od razu klasyfikując każdy temat.

Dokument ma dwie części, które się uzupełniają:

- **Rozdział 1 – zakres.** Macierz 482 pozycji funkcjonalnych obszaru Kadry,
  Płace i Czas pracy, zbudowana ze spisu treści instrukcji producenta. To w niej
  zapisujemy ustalenia w formie nadającej się do umowy i wyceny: identyfikator
  pozycji, wymaganie klienta, klasyfikacja, opis realizacji.
- **Rozdziały 2–18 – pytania.** To, co konsultant *pyta*, żeby te ustalenia
  z klienta wydobyć. Pytania wyciągają rzeczy, których klient sam nie powie,
  i prowadzą rozmowę; zakres zapisujemy w rozdziale 1.

Praktycznie: pytaniami rozmawiamy, w rozdziale 1 notujemy. Klasyfikacja jest
wspólna dla obu części:

| Klasyfikacja | Znaczenie |
|---|---|
| **S** – standard | działa „z pudełka", wystarczy pokazać |
| **K** – konfiguracja | standard, ale wymaga ustawienia (definicje, cechy, kalendarze, prawa) |
| **C** – customizacja | wymaga kodu: element płacowy z algorytmem, weryfikator, raport, worker, dodatek DLL |
| **X** – poza zakresem / do decyzji klienta | proces zostaje poza systemem albo wymaga zmiany po stronie klienta |

Każdy punkt oznaczony **C** musi na koniec analizy mieć: opis funkcjonalny,
wskazanie osoby decyzyjnej po stronie klienta, szacunek pracochłonności
i informację, czy jest krytyczny dla startu (blokuje go), czy może powstać po starcie.

---

## 0. Przygotowanie – dokumenty do zebrania PRZED spotkaniem

Bez tych materiałów analiza zamienia się w zgadywanie. Prosić o nie z góry:

- [ ] Regulamin pracy (rozkłady, okresy rozliczeniowe, pory nocne, dyżury).
- [ ] Regulamin wynagradzania / układ zbiorowy / porozumienia zbiorowe.
- [ ] Regulamin ZFŚS wraz z tabelą progów dochodowych.
- [ ] Regulamin premiowania (jeśli osobny) i przykładowe wyliczenia premii.
- [ ] Wzory umów o pracę, aneksów, świadectw pracy, zaświadczeń (jeśli klient ma własne).
- [ ] 2–3 **przykładowe listy płac** z poprzedniego systemu – najlepiej „trudny" miesiąc
      (nadgodziny, chorobowe, korekta, wypłata po ustaniu zatrudnienia).
- [ ] Przykładowa ewidencja czasu pracy / grafik / plik z RCP (surowy eksport, nie raport).
- [ ] Schemat organizacyjny i lista stanowisk.
- [ ] Lista raportów i wydruków, których faktycznie używają (nie „wszystkich, jakie mamy").
- [ ] Wykaz systemów, z którymi enova ma się wymieniać danymi + przykładowe pliki.
- [ ] Informacja o posiadanej licencji (moduły, wariant Srebro/Złoto/Platyna, liczba pracowników).

> **Uwaga:** wariant licencji rozstrzyga o tym, czy część tematów jest w ogóle
> dostępna (m.in. dodatkowe algorytmy płacowe, rozbudowana ewidencja czasu pracy,
> pulpity, wielooddziałowość). Ustalić to na starcie – inaczej ryzykujemy obiecanie
> funkcji, której klient nie kupił.

---

## 1. Zakres procesów objętych analizą i wdrożeniem

Rozdział otwierający dokument analizy przedwdrożeniowej. Jego zadaniem jest
**zamknięcie zakresu**: wymienić wszystkie procesy i funkcjonalności, które
wchodzą do analizy, i przy każdej z nich zapisać, czy realizujemy ją standardem,
konfiguracją, czy kodem pisanym pod klienta.

Zakres poniżej pokrywa obszar **Kadry, Płace i Czas pracy**. Świadomie pominięto
moduły HR sąsiadujące z tym obszarem (Zarządzanie Kapitałem Ludzkim, Szkolenia,
Oceny, Rekrutacja) — jeśli wejdą do projektu, dopisujemy je jako osobne obszary
`ZZL-xx`, `SZK-xx`, `OCE-xx`, `REK-xx`.

### Skąd pochodzi lista funkcjonalności

Wiersze nie są listą życzeń ani wyciągiem z materiałów marketingowych. Powstały
z **dokumentacji producenta** — instrukcji obszaru „Kadry Płace i HR" dla
enova365 (2196 stron, spis treści obejmuje 671 pozycji). Kolumna **Instr.**
podaje numer strony instrukcji, na której opisana jest dana funkcjonalność.

Dzięki temu każdą pozycję można na warsztacie pokazać klientowi na dokumentacji
producenta, a w kolumnie *Opis realizacji* wskazać docelowo akapit tej
dokumentacji albo załącznik z opisem rozwiązania dedykowanego.

### Jak czytać kolumny

| Kolumna | Znaczenie |
|---|---|
| **Nr** | Identyfikator pozycji, niezmienny przez cały projekt — po nim odwołujemy się do zakresu w umowie, harmonogramie i protokołach odbioru. |
| **Funkcjonalność systemowa** | Co system robi — nazwa funkcji produktu, nie życzenie klienta. |
| **Instr.** | Strona instrukcji producenta z opisem funkcjonalności. Znak `—` oznacza funkcję spoza instrukcji obszaru Kadry Płace i HR (np. realizowaną w innym module albo nieopisaną), wymagającą wskazania źródła na warsztacie. |
| **Lic.** | Wariant licencji wymagany według instrukcji: **P** — platynowy, **Z** — złoty (lub wyżej), **Z+** — złoty/platynowy i dodatkowo osobna licencja na dodatek, `—` — instrukcja nie stawia warunku. Pole wypełnione tylko tam, gdzie instrukcja mówi to wprost. |
| **Wymagania Klienta** | Wypełniane na warsztacie: opis wymagania klienta albo odsyłacz do załącznika. |
| **Kl.** | Klasyfikacja (poniżej). Wartości wpisane w tabeli to **propozycja wyjściowa konsultanta**, nie ustalenie — potwierdzamy je z klientem pozycja po pozycji. |
| **Opis realizacji** | Wypełniane na warsztacie: odsyłacz do akapitu z opisem realizacji albo do załącznika. |

### Klasyfikacja i konsekwencja dla wyceny

Klasyfikujemy literami **S / K / C / X** z legendy na początku dokumentu.
Poniżej to samo z dopisaną konsekwencją dla wyceny:

| Kl. | Znaczenie | Konsekwencja dla wyceny |
|---|---|---|
| **S** | Standard — działa bez zmian, wystarczy pokazać i przeszkolić. | Bez wyceny prac programistycznych. |
| **K** | Konfiguracja — standard, ale wymaga ustawienia (definicje, kalendarze, cechy, prawa, ścieżki akceptacji). | Wycena pracy konsultanta. |
| **C** | Customizacja — wymaga kodu: element płacowy z algorytmem, weryfikator, raport, worker, dodatek DLL. | Wycena analizy, programowania i testów. |
| **X** | Poza zakresem, decyzja po stronie klienta albo funkcja historyczna. | Zapisać powód wykluczenia. |

Jeśli klient używa w swoim szablonie litery **D** („rozwiązanie dedykowane"),
odpowiada ona naszemu **C** — przy przepisywaniu zakresu do jego formatki
podmieniamy tylko oznaczenie, nie znaczenie.

> **Uwaga o licencji.** Kolumna *Lic.* rozstrzyga, czy temat jest w ogóle
> dostępny. Pozycje oznaczone **P** przy licencji złotej nie zadziałają — i nie
> da się tego obejść konfiguracją, tylko zakupem licencji albo rozwiązaniem
> dedykowanym. Wariant licencji klienta ustalamy **przed** warsztatem, inaczej
> ryzykujemy obiecanie funkcji, której klient nie kupił.

---

### KAD — Kadry

#### KAD-01 · Prowadzenie kartotek pracowników

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| KAD-01-001 | Zatrudnienie pracownika — rejestracja kartoteki | 1104 | — |  | S |  |
| KAD-01-002 | Dane osobowe i identyfikacyjne | 28 | — |  | S |  |
| KAD-01-003 | Adresy | 29 | — |  | S |  |
| KAD-01-004 | Dane kontaktowe | 30 | — |  | S |  |
| KAD-01-005 | Dane podatkowe | 35 | — |  | S |  |
| KAD-01-006 | Dane podatkowe — bilans otwarcia | 32 | — |  | K |  |
| KAD-01-007 | Dane ubezpieczeniowe | 44 | — |  | S |  |
| KAD-01-008 | Dane ubezpieczeniowe pozostałe | 47 | — |  | S |  |
| KAD-01-009 | Podstawy składek ZUS | 68 | — |  | S |  |
| KAD-01-010 | Podstawa składek FP i FGŚP | 66 | — |  | S |  |
| KAD-01-011 | Historia zatrudnienia i staż pracy | 51 | — |  | S |  |
| KAD-01-012 | Definicje podstaw stażu pracy | 1491 | — |  | K |  |
| KAD-01-013 | Rodzina i osoby zależne | 74 | — |  | S |  |
| KAD-01-014 | Wykształcenie | 83 | — |  | S |  |
| KAD-01-015 | Znajomość języków obcych | 111 | — |  | S |  |
| KAD-01-016 | Uprawnienia zawodowe | 107 | P |  | S |  |
| KAD-01-017 | Badania lekarskie | 88 | — |  | S |  |
| KAD-01-018 | Szkolenia BHP | 102 | — |  | S |  |
| KAD-01-019 | Czynniki szkodliwe i uciążliwe | 94 | P |  | S |  |
| KAD-01-020 | Wypadki przy pracy | 109 | Z |  | S |  |
| KAD-01-021 | Nagrody i kary | 98 | — |  | S |  |
| KAD-01-022 | Służba wojskowa | 78 | — |  | S |  |
| KAD-01-023 | Rachunki bankowe pracownika | 71 | — |  | S |  |
| KAD-01-024 | Dostęp WWW (konto pulpitu) | 48 | — |  | K |  |
| KAD-01-025 | Prawa dostępu na kartotece | 70 | — |  | K |  |
| KAD-01-026 | Struktura organizacyjna pracownika | 101 | — |  | K |  |
| KAD-01-027 | Wieloetatowość | 80 | — |  | S |  |
| KAD-01-028 | Karty RCP pracownika | 97 | — |  | K |  |
| KAD-01-029 | Lokalizacje pracy zdalnej | 64 | — |  | S |  |
| KAD-01-030 | Dane statystyczne (GUS) | 92 | — |  | S |  |
| KAD-01-031 | Kartoteka centralna | 93 | — |  | S |  |
| KAD-01-032 | Informacje PFRON na kartotece | 54 | — |  | S |  |
| KAD-01-033 | Informacje ZUS na kartotece | 59 | — |  | S |  |
| KAD-01-034 | Schorzenia (dane historyczne) | 130 | — |  | S |  |
| KAD-01-035 | Zaniechania podatkowe (dane historyczne) | 131 | — |  | S |  |
| KAD-01-036 | Uwagi i inne dane | 60 | — |  | S |  |
| KAD-01-037 | Historia zapisów (log zmian na kartotece) | 372 | — |  | S |  |
| KAD-01-038 | Pracownik w archiwum | 371 | — |  | S |  |
| KAD-01-039 | Archiwum kartoteki pracownika | 901 | — |  | K |  |
| KAD-01-040 | Modyfikacja danych — aktualizacje i zapisy historyczne | 1120 | — |  | S |  |

**Na co zwrócić uwagę:** pozycje `006`, `012`, `024`–`026`, `028` i `039` to
typowe miejsca, gdzie klient zakłada, że „samo się wypełni". Bilans otwarcia
danych podatkowych i definicje podstaw stażu trzeba ustawić ręcznie przed
pierwszą wypłatą — warto przypisać je do konkretnej osoby i terminu już na
warsztacie.

#### KAD-02 · Obsługa umów o pracę i etatów

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| KAD-02-001 | Etat — dane podstawowe, stanowisko, wynagrodzenie | 200 | — |  | S |  |
| KAD-02-002 | Umowa o pracę — ewidencja i wydruk | 223 | — |  | S |  |
| KAD-02-003 | Przyczyny zawarcia umowy o pracę | 1536 | — |  | K |  |
| KAD-02-004 | Ewidencja umów o pracę (lista) | 477 | — |  | S |  |
| KAD-02-005 | Wieloetatowość | 80 | — |  | S |  |
| KAD-02-006 | Zmiana warunków zatrudnienia (aneks, aktualizacja) | 1120 | — |  | S |  |
| KAD-02-007 | Przeszeregowania (awans, zmiana stawki) | 1530 | P |  | S |  |
| KAD-02-008 | Grupy zaszeregowania | 1508 | — |  | K |  |
| KAD-02-009 | Formy organizacji pracy | 1506 | — |  | K |  |
| KAD-02-010 | Zaktualizuj wydział (przeniesienie organizacyjne) | 396 | P |  | S |  |
| KAD-02-011 | Dodatki etatowe | 205 | — |  | K |  |
| KAD-02-012 | Zestawy dodatków | 1540 | — |  | K |  |
| KAD-02-013 | Akordy | 203 | — |  | K |  |
| KAD-02-014 | Definicje akordów | 1476 | — |  | K |  |
| KAD-02-015 | Ubezpieczenia na etacie | 221 | — |  | S |  |
| KAD-02-016 | Rozliczenia czasu pracy na etacie (parametry) | 216 | — |  | K |  |
| KAD-02-017 | Rozwiązanie umowy | 217 | — |  | S |  |
| KAD-02-018 | Przyczyny rozwiązania umowy o pracę | 1534 | — |  | K |  |
| KAD-02-019 | Zakończenie umowy z pracownikiem (proces) | 1107 | — |  | S |  |
| KAD-02-020 | Wyrejestrowanie z ubezpieczeń | 226 | — |  | S |  |
| KAD-02-021 | Świadectwo pracy | 1082 | P |  | S |  |
| KAD-02-022 | Świadectwo pracy — wykazywanie nieobecności | 1092 | — |  | S |  |
| KAD-02-023 | Świadectwo pracy — turnus rehabilitacyjny | 1094 | — |  | S |  |
| KAD-02-024 | Świadectwo pracy — urlop bezpłatny na ćwiczenia wojskowe | 1097 | — |  | S |  |
| KAD-02-025 | Świadectwo pracy — odprawy | 1100 | — |  | S |  |
| KAD-02-026 | Świadectwo pracy — urlopy związane z macierzyństwem | 1101 | — |  | S |  |
| KAD-02-027 | Pracownik tymczasowy | 881 | P |  | S |  |
| KAD-02-028 | Pracownicy zewnętrzni (B2B, APT) | 1317 | P |  | S |  |
| KAD-02-029 | APT — agencja pracy tymczasowej | 1305 | P |  | S |  |
| KAD-02-030 | Umowy zewnętrzne | 302 | P |  | S |  |
| KAD-02-031 | Centralny Rejestr Umów | 1416 | Z+ |  | K |  |
| KAD-02-032 | Segmentowy kod wydziału | 1310 | — |  | K |  |
| KAD-02-033 | Jednostki organizacyjne | 1512 | — |  | K |  |
| KAD-02-034 | Kategorie jednostek organizacyjnych | 1510 | — |  | K |  |
| KAD-02-035 | Obsługa firm wielooddziałowych | 1515 | P |  | K |  |
| KAD-02-036 | Struktury organizacyjne (ewidencja) | 474 | P |  | K |  |
| KAD-02-037 | Kody wykonywanych zawodów (GUS) | 1529 | — |  | S |  |
| KAD-02-038 | Dane o firmie | 1103 | — |  | K |  |

**Na co zwrócić uwagę:** `031` wymaga licencji DMS — jeśli klient jej nie ma,
rejestr umów trzeba zastąpić czymś innym i ta decyzja należy do klienta, nie do
nas. `035` przy licencji złotej nie zadziała, a wielooddziałowość bywa odkrywana
dopiero na etapie deklaracji PIT i PFRON.

#### KAD-03 · Umowy cywilnoprawne

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| KAD-03-001 | Umowy cywilnoprawne — obsługa | 1046 | — |  | S |  |
| KAD-03-002 | Dodawanie umowy na kartotece | 290 | — |  | S |  |
| KAD-03-003 | Umowy — lista i ewidencja | 412 | — |  | S |  |
| KAD-03-004 | Formularz umowy — konfiguracja | 1075 | — |  | K |  |
| KAD-03-005 | Zgłoszenia ZUA, ZZA, ZWUA z umów cywilnoprawnych | 1068 | — |  | S |  |
| KAD-03-006 | Dodatki i potrącenia do umowy | 422 | — |  | K |  |
| KAD-03-007 | Aktualizacja umowy | 1046 | — |  | S |  |
| KAD-03-008 | Anulowanie umowy cywilnoprawnej | 1193 | — |  | S |  |
| KAD-03-009 | Wyrównanie dla umowy cywilnoprawnej | 1074 | — |  | K |  |
| KAD-03-010 | Nieobecności dla umów cywilnoprawnych i zewnętrznych | 1413 | — |  | S |  |
| KAD-03-011 | Rozliczenie zasiłku zleceniobiorcy | 985 | — |  | S |  |
| KAD-03-012 | Rozliczenia pracowników zewnętrznych | 507 | P |  | S |  |
| KAD-03-013 | Rozliczenia kontrahentów | 508 | P |  | S |  |
| KAD-03-014 | Członkowie Rolniczych Spółdzielni Produkcyjnych | 1045 | — |  | X |  |

#### KAD-04 · Elektroniczna dokumentacja pracownicza (e-teczki)

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| KAD-04-001 | Elektroniczna dokumentacja pracownicza — zasady i podstawy prawne | 817 | Z |  | S |  |
| KAD-04-002 | Akta osobowe A–E | 194 | Z |  | S |  |
| KAD-04-003 | Pozostała dokumentacja | 196 | Z |  | S |  |
| KAD-04-004 | Dokumenty techniczne | 198 | Z |  | S |  |
| KAD-04-005 | Ręczne dodawanie dokumentów do teczki | 830 | Z |  | S |  |
| KAD-04-006 | Automatyczne generowanie dokumentów do teczki | 864 | Z |  | K |  |
| KAD-04-007 | Seryjne dodawanie dokumentów (np. regulamin pracy) | 848 | Z |  | S |  |
| KAD-04-008 | Dodawanie do teczki wniosków o nieobecności i kadrowych | 840 | Z |  | K |  |
| KAD-04-009 | Dokument dot. kontroli trzeźwości i środków odurzających | 856 | Z |  | S |  |
| KAD-04-010 | Poświadczenia odbioru | 114 | — |  | S |  |
| KAD-04-011 | Udostępnianie deklaracji PIT w Pulpicie Pracownika | 870 | Z |  | K |  |
| KAD-04-012 | Wydawanie kopii dokumentacji pracownikowi | 877 | Z |  | S |  |
| KAD-04-013 | Usuwanie dokumentów z teczki | 880 | Z |  | S |  |
| KAD-04-014 | Definicje dokumentów | 1484 | — |  | K |  |
| KAD-04-015 | Elektroniczna dokumentacja pracownicza — lista | 420 | Z |  | S |  |
| KAD-04-016 | Teczki pracownicze obowiązujące do wersji 15.3 | 800 | Z |  | X |  |

#### KAD-05 · KZP, ZFM i fundusze pożyczkowe

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| KAD-05-001 | Obsługa funduszy pożyczkowych KZP i ZFM | 1009 | — |  | S |  |
| KAD-05-002 | Składki członkowskie i wkłady | 314 | — |  | S |  |
| KAD-05-003 | Pożyczki | 323 | — |  | S |  |
| KAD-05-004 | Pożyczki — lista | 511 | — |  | S |  |
| KAD-05-005 | Harmonogram i kontrola spłat pożyczki | 940 | — |  | S |  |
| KAD-05-006 | Spłata pożyczki wkładem | 318 | — |  | S |  |
| KAD-05-007 | Indywidualny rachunek bankowy do spłaty pożyczki | 1038 | — |  | K |  |
| KAD-05-008 | Synchronizacja KZP z bazą księgową | 1026 | — |  | K |  |
| KAD-05-009 | Konfiguracja KZP, ZFM | 1833 | Z |  | K |  |
| KAD-05-010 | Rozrachunki z funduszu pożyczkowego (dodatek) | 2183 | Z+ |  | S |  |

#### KAD-06 · ZFŚS

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| KAD-06-001 | Definicje świadczeń socjalnych | 1499 | — |  | K |  |
| KAD-06-002 | Świadczenia socjalne na kartotece | 104 | P |  | S |  |
| KAD-06-003 | Świadczenia socjalne — lista | 513 | P |  | S |  |
| KAD-06-004 | Kwalifikacja do progu dochodowego wg regulaminu ZFŚS | — | P |  | C |  |
| KAD-06-005 | Wnioski socjalne w pulpicie | 2006 | P |  | K |  |
| KAD-06-006 | Rozliczenie i sprawozdawczość funduszu | — | — |  | C |  |

**Na co zwrócić uwagę:** `004` i `006` to klasyczne pozycje dedykowane. enova
prowadzi ewidencję świadczeń, ale progów dochodowych z regulaminu klienta
(dochód na członka rodziny, tabela dopłat) nie wyliczy sama — potrzebny jest
algorytm albo cecha wyliczana. To zwykle pierwsza pozycja **C** w projekcie
kadrowym i warto ją wycenić osobno.

---

### CP — Czas pracy

#### CP-01 · Plan pracy, kalendarze i grafiki

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| CP-01-001 | Definicje stref | 1572 | — |  | K |  |
| CP-01-002 | Definicje dni | 1576 | — |  | K |  |
| CP-01-003 | Serie dni | 1581 | — |  | K |  |
| CP-01-004 | Kalendarze wzorcowe | 1585 | — |  | K |  |
| CP-01-005 | Algorytmy naliczania normy | 1612 | — |  | K |  |
| CP-01-006 | Norma czasu pracy pracownika | 266 | — |  | S |  |
| CP-01-007 | Przykład kalendarza 7-godzinnego (norma nietypowa) | 1609 | — |  | K |  |
| CP-01-008 | Kalendarz pracownika — czas pracy | 230 | — |  | S |  |
| CP-01-009 | Wersjonowanie kalendarzy | 1568 | P |  | K |  |
| CP-01-010 | Grafiki | 435 | — |  | S |  |
| CP-01-011 | Definicje grafików | 1624 | — |  | K |  |
| CP-01-012 | Planowanie kalendarzy (moduł) | 699 | P |  | K |  |
| CP-01-013 | Planowanie kalendarzy — konfiguracja | 701 | P |  | K |  |
| CP-01-014 | Planowanie kalendarzy czasu pracy na obiektach | 706 | P |  | K |  |
| CP-01-015 | Planowanie kalendarzy pracowników | 710 | P |  | K |  |
| CP-01-016 | Dyspozycyjność (dostępność) pracowników | 712 | P |  | K |  |
| CP-01-017 | Dokumenty aktualizacji kalendarza na kartotece | 246 | P |  | S |  |
| CP-01-018 | Dokumenty aktualizacji kalendarzy — lista | 460 | P |  | S |  |
| CP-01-019 | Ścieżki akceptacji dokumentów aktualizacji kalendarzy | 704 | P |  | K |  |
| CP-01-020 | Historia kalendarzy i dokumentów aktualizacji | 708 | P |  | S |  |
| CP-01-021 | Edycja kalendarza w Pulpicie Pracownika (dodatek) | 2175 | Z+ |  | S |  |
| CP-01-022 | Bilans otwarcia czasu pracy | 238 | — |  | K |  |
| CP-01-023 | Import planu pracy z pliku zewnętrznego (grafik z Excela) | — | — |  | C |  |

**Na co zwrócić uwagę:** `023` nie jest funkcją produktu — jeśli klient planuje
pracę w arkuszu i chce go wczytywać, to zawsze pozycja **C**. Pytać wprost:
„czy grafik powstaje w enovie, czy poza nią?". Odpowiedź „u nas w Excelu"
oznacza dodatkowy zakres, a nie szczegół techniczny.

#### CP-02 · Rejestracja rzeczywista i RCP

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| CP-02-001 | Dane z RCP | 431 | P |  | S |  |
| CP-02-002 | Oryginalne dane z RCP | 444 | P |  | S |  |
| CP-02-003 | Konfiguracja RCP | 1645 | — |  | K |  |
| CP-02-004 | RCP — algorytmy | 1648 | — |  | K |  |
| CP-02-005 | RCP — reguły | 1649 | — |  | K |  |
| CP-02-006 | Karty RCP pracownika | 97 | — |  | K |  |
| CP-02-007 | Import odbić z czytnika / pliku czytnika | — | — |  | C |  |
| CP-02-008 | Rozbudowany mechanizm weryfikacji czasu pracy | 1337 | P |  | K |  |
| CP-02-009 | Weryfikacja dni RCP (lista) | 1345 | P |  | S |  |
| CP-02-010 | Zestawienie weryfikacji w pulpicie | 1348 | P |  | K |  |
| CP-02-011 | Proces Pracownik — Kierownik — Pracownik | 1351 | P |  | K |  |
| CP-02-012 | Proces Operator HR — Pracownik — Operator HR | 1358 | P |  | K |  |
| CP-02-013 | Proces Operator HR — Kierownik — Operator HR | 1365 | P |  | K |  |
| CP-02-014 | Akceptacja RCP w pulpicie (lista zadań) | 1374 | P |  | K |  |
| CP-02-015 | Praca zdalna — ewidencja | 665 | — |  | S |  |
| CP-02-016 | Praca zdalna na kartotece (kalendarz) | 278 | — |  | S |  |
| CP-02-017 | Praca zdalna — ewidencja i lokalizacje (listy) | 470 | — |  | S |  |
| CP-02-018 | Praca zdalna — konfiguracja | 1642 | — |  | K |  |
| CP-02-019 | Praca zdalna — parametry płacowe (ryczałt) | 1792 | — |  | K |  |

**Na co zwrócić uwagę:** `001`, `002` i `008` to funkcje platynowe. Projekt
z RCP na licencji złotej jest projektem **C** od pierwszego dnia — trzeba to
powiedzieć klientowi na warsztacie, nie w trakcie wdrożenia. `007` zależy od
modelu czytnika: enova czyta dane z RCP, ale sposób ich dostarczenia (plik,
baza czytnika, usługa) bywa dedykowany.

#### CP-03 · Weryfikacja i rozliczenie czasu pracy

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| CP-03-001 | Weryfikatory kalendarza | 679 | P |  | S |  |
| CP-03-002 | Definicje weryfikatorów kalendarza | 1550 | P |  | K |  |
| CP-03-003 | Weryfikatory na definicji kalendarza | 1599 | P |  | K |  |
| CP-03-004 | Weryfikatory reguł kadrowych (konfiguracja Kadry) | 1538 | — |  | K |  |
| CP-03-005 | Weryfikatory własne (reguły spoza standardu) | — | P |  | C |  |
| CP-03-006 | Rozliczenie czasu pracy | 457 | P |  | S |  |
| CP-03-007 | Rozliczenie czasu pracy — dokumenty | 459 | P |  | S |  |
| CP-03-008 | Definicje dokumentów rozliczenia czasu pracy | 1615 | P |  | K |  |
| CP-03-009 | Statystyka czasu pracy na kartotece | 282 | — |  | S |  |
| CP-03-010 | Statystyki czasu pracy (lista) | 443 | — |  | S |  |
| CP-03-011 | Zestawienia czasu pracy | 286 | — |  | K |  |
| CP-03-012 | Definicje zestawień czasu pracy (kolumny własne) | — | — |  | C |  |
| CP-03-013 | Karta ewidencji czasu pracy — wydruk | 1298 | — |  | S |  |
| CP-03-014 | Karta ewidencji czasu pracy szczegółowa — godziny dyżurów | 1301 | — |  | S |  |

**Na co zwrócić uwagę:** `005` i `012` to najczęstsze pozycje **C** w obszarze
czasu pracy. Standardowe weryfikatory pilnują norm kodeksowych; reguły
z regulaminu pracy klienta (np. odpoczynek dobowy liczony inaczej niż
w standardzie, limity nadgodzin per wydział) wymagają własnego kodu. To samo
dotyczy kolumn zestawienia, których nie ma w standardzie.

#### CP-04 · Nadgodziny i dodatki za czas pracy

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| CP-04-001 | Bilansowanie nadgodzin w okresie rozliczeniowym | 1601 | P |  | K |  |
| CP-04-002 | Przekroczenie normy średniotygodniowej | 1608 | — |  | S |  |
| CP-04-003 | Magazyn nadgodzin | 805 | P |  | S |  |
| CP-04-004 | Magazyn nadgodzin — konfiguracja kalendarza | 1548 | P |  | K |  |
| CP-04-005 | Magazyn nadgodzin w pulpicie | 2036 | P |  | K |  |
| CP-04-006 | Oddawanie dni wolnych za nadgodziny | 974 | — |  | S |  |
| CP-04-007 | Dodatki za pracę w godzinach nadliczbowych (elementy) | 1668 | — |  | K |  |
| CP-04-008 | Dodatki za pracę w nocy (strefy i elementy) | 1572 | — |  | K |  |
| CP-04-009 | Dyżury | 1301 | — |  | K |  |
| CP-04-010 | Rozliczenie nadgodzin wg reguł własnych klienta | — | — |  | C |  |
| CP-04-011 | Przerwa na karmienie | 905 | — |  | S |  |
| CP-04-012 | Postojowe | 917 | — |  | S |  |

**Na co zwrócić uwagę:** `010` pojawia się prawie w każdym wdrożeniu
produkcyjnym. Silnik enovy dzieli nadgodziny na 50/100 i średniotygodniowe wg
własnego algorytmu; jeśli klient ma inne zasady (np. inny podział przy pracy
zmianowej, własny sposób bilansowania okresu), potrzebne są własne cechy
wyliczane albo element z algorytmem. Prosić o **pisemną regułę i trzy przykłady
wyliczeń** — bez tego pozycji nie da się wycenić.

---

### UN — Urlopy i nieobecności

#### UN-01 · Nieobecności i urlopy

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| UN-01-001 | Definicje nieobecności | 1629 | — |  | K |  |
| UN-01-002 | Nieobecności na kartotece | 256 | — |  | S |  |
| UN-01-003 | Nieobecności — lista | 446 | — |  | S |  |
| UN-01-004 | Dodawanie dwóch nieobecności w ciągu dnia | 449 | — |  | S |  |
| UN-01-005 | Planowane nieobecności na kartotece | 268 | P |  | S |  |
| UN-01-006 | Planowane nieobecności — lista | 442 | P |  | S |  |
| UN-01-007 | Limity nieobecności na kartotece | 247 | — |  | S |  |
| UN-01-008 | Limity nieobecności — lista | 441 | P |  | S |  |
| UN-01-009 | Konfiguracja limitów nieobecności | 1553 | — |  | K |  |
| UN-01-010 | Bilans otwarcia urlopów | 241 | — |  | K |  |
| UN-01-011 | Zbiegi pracy i rodzicielstwa | 284 | — |  | S |  |
| UN-01-012 | Urlop wypoczynkowy — konfiguracja | 1805 | — |  | K |  |
| UN-01-013 | Urlopy inne — konfiguracja | 1811 | — |  | K |  |
| UN-01-014 | Wnioski o urlopy i delegacje na kartotece | 120 | P |  | S |  |
| UN-01-015 | Wnioski o urlopy i delegacje — lista | 461 | P |  | S |  |
| UN-01-016 | Limity i zasady urlopowe wg regulaminu klienta | — | — |  | C |  |

#### UN-02 · Podstawy urlopowe i zasiłkowe

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| UN-02-001 | Podstawy na kartotece | 269 | — |  | S |  |
| UN-02-002 | Podstawy naliczania urlopu wypoczynkowego | 271 | — |  | S |  |
| UN-02-003 | Podstawy naliczania zwolnień ZUS | 273 | — |  | S |  |
| UN-02-004 | Elementy zmienne w podstawie urlopu i ekwiwalentu | 994 | — |  | K |  |
| UN-02-005 | Urlop wypoczynkowy w podstawie chorobowego | 1003 | — |  | S |  |
| UN-02-006 | Zasiłek chorobowy — konfiguracja | 1819 | — |  | K |  |
| UN-02-007 | Zasiłki ogólne — konfiguracja | 1824 | — |  | K |  |
| UN-02-008 | Zasiłki inne — konfiguracja | 1821 | — |  | K |  |
| UN-02-009 | Zasiłki (inny płatnik) | 349 | — |  | S |  |
| UN-02-010 | Oświadczenia do zasiłku opiekuńczego | 116 | — |  | S |  |
| UN-02-011 | Obsługa wniosku Z-15 | 1375 | — |  | S |  |
| UN-02-012 | E-wniosek Z-15 w pulpicie | 1394 | P |  | K |  |
| UN-02-013 | Zaświadczenia ZUS Z-3, Z-3a | 1398 | — |  | S |  |

#### UN-03 · e-ZLA, PUE i korekty nieobecności

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| UN-03-001 | Import e-zwolnień z PUE | 681 | Z |  | S |  |
| UN-03-002 | Import z PUE — konfiguracja | 1626 | Z |  | K |  |
| UN-03-003 | Import nieobecności o statusie Anulowane | 690 | Z |  | S |  |
| UN-03-004 | Import nieobecności o statusie Skorygowane | 692 | Z |  | S |  |
| UN-03-005 | Zasiłek opiekuńczy w trakcie urlopu wypoczynkowego | 694 | Z |  | S |  |
| UN-03-006 | Import nieobecności Wsteczne do wyjaśnienia | 695 | Z |  | S |  |
| UN-03-007 | Import nieobecności Wsteczne nie uzasadnione | 697 | Z |  | S |  |
| UN-03-008 | Import nieobecności z PUE — lista | 439 | Z |  | S |  |
| UN-03-009 | Korekty nieobecności | 1242 | — |  | S |  |
| UN-03-010 | Korekty z opcją miesięcy wstecz | 1243 | — |  | S |  |
| UN-03-011 | Korekty z kodami wyrównania RSA | 1255 | — |  | S |  |
| UN-03-012 | Edycja miesiąca ZUS | 1273 | — |  | S |  |
| UN-03-013 | Druga korekta do korekty | 1281 | — |  | S |  |
| UN-03-014 | Korekta nieobecności w wypłacie | 1152 | — |  | S |  |

**Na co zwrócić uwagę:** blok korekt (`009`–`014`) klienci zwykle pomijają na
warsztacie, a potem okazuje się, że to codzienność działu płac. Warto przejść go
na żywym przykładzie z ich poprzedniego systemu — zwłaszcza korektę wsteczną
z wyrównaniem RSA.

---

### PL — Płace

#### PL-01 · Naliczanie wynagrodzeń

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| PL-01-001 | Definiowanie list płac | 502 | — |  | K |  |
| PL-01-002 | Konfiguracja list płac | 1765 | — |  | K |  |
| PL-01-003 | Listy płac — obsługa | 1124 | — |  | S |  |
| PL-01-004 | Dodatkowy opis listy płac | 1130 | — |  | K |  |
| PL-01-005 | Naliczanie wypłat | 506 | — |  | S |  |
| PL-01-006 | Wypłaty etatowe z list płac | 1288 | — |  | S |  |
| PL-01-007 | Wypłaty dla pracowników | 1133 | — |  | S |  |
| PL-01-008 | Wypłaty z tytułu umów cywilnoprawnych | 1136 | — |  | S |  |
| PL-01-009 | Wypłaty (wszystkie) na kartotece | 327 | — |  | S |  |
| PL-01-010 | Elementy wypłaty — lista | 495 | P |  | S |  |
| PL-01-011 | Definicje elementów wynagrodzenia | 1668 | — |  | K |  |
| PL-01-012 | Algorytm elementu — ogólne | 1679 | — |  | K |  |
| PL-01-013 | Algorytm elementu — edytor kodu | 1704 | — |  | C |  |
| PL-01-014 | Algorytm elementu — nazwy parametrów | 1705 | — |  | K |  |
| PL-01-015 | Algorytm elementu — staż pracy | 1702 | — |  | K |  |
| PL-01-016 | Element rozliczenia | 1665 | — |  | K |  |
| PL-01-017 | Dodatki i potrącenia — lista | 422 | — |  | S |  |
| PL-01-018 | Premie i dodatki wg regulaminu wynagradzania klienta | — | — |  | C |  |
| PL-01-019 | Zaliczki | 346 | — |  | S |  |
| PL-01-020 | Koszty autorskie | 328 | — |  | S |  |
| PL-01-021 | Odpis na OPP | 331 | — |  | S |  |
| PL-01-022 | Rozliczenia pracownika | 352 | — |  | S |  |
| PL-01-023 | Kalkulator wynagrodzeń | 1291 | — |  | S |  |
| PL-01-024 | Planowane listy płac | 896 | P |  | S |  |
| PL-01-025 | Wynagrodzenia — konfiguracja | 1815 | — |  | K |  |
| PL-01-026 | Naliczanie podatków i składek | 1727 | — |  | S |  |
| PL-01-027 | Podatki — konfiguracja | 1772 | — |  | K |  |
| PL-01-028 | Składki ZUS — konfiguracja i wskaźniki | 1799 | — |  | S |  |
| PL-01-029 | Składki na FP, FGŚP i FEP | 1796 | — |  | S |  |
| PL-01-030 | Wskaźniki płacowe | 1814 | — |  | S |  |
| PL-01-031 | Zaokrąglenia | 1818 | — |  | K |  |
| PL-01-032 | Świadczenia inne | 1803 | — |  | K |  |
| PL-01-033 | Jawność wynagrodzeń | 1408 | P |  | S |  |
| PL-01-034 | Luka płacowa | 1418 | — |  | S |  |
| PL-01-035 | Eksport przelewów do banku | — | — |  | K |  |

**Na co zwrócić uwagę:** `013` i `018` to serce wyceny części płacowej. Każdy
składnik, którego nie ma w standardzie (premia wg własnego wzoru, dodatek
stażowy liczony inaczej, świadczenie regulaminowe), to osobny element
z algorytmem — czyli osobna pozycja **C**. Zamiast jednej linii „premie" trzeba
wypisać każdy składnik z listy płac klienta osobno. `035` realizuje moduł
Ewidencji Środków Pieniężnych — potwierdzić, że klient go ma.

#### PL-02 · Korekty wypłat

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| PL-02-001 | Korekta zrealizowanych wypłat | 1140 | — |  | S |  |
| PL-02-002 | Zalecany mechanizm korygowania wypłat | 1216 | — |  | S |  |
| PL-02-003 | Automatyczny mechanizm korygowania wypłat | 1232 | — |  | K |  |
| PL-02-004 | Korekta wartości | 1164 | — |  | S |  |
| PL-02-005 | Korekta składki wypadkowej | 1169 | — |  | S |  |
| PL-02-006 | Korekta składek ZUS i podatków | 1142 | — |  | S |  |
| PL-02-007 | Przekroczenie podstawy składki emerytalno-rentowej | 1201 | — |  | S |  |
| PL-02-008 | Storno płacowe | 1175 | — |  | S |  |
| PL-02-009 | Elementy stornowane | 351 | — |  | S |  |
| PL-02-010 | Oznaczenie do przeliczenia | 1176 | — |  | S |  |
| PL-02-011 | Anulowanie niesłusznie wypłaconego elementu | 1184 | — |  | S |  |
| PL-02-012 | Korekty wypłat osób do 26 roku życia | 1187 | — |  | S |  |
| PL-02-013 | Korekty składek PPK | 1143 | — |  | S |  |
| PL-02-014 | Korekta składek PPK — przestój | 1145 | — |  | S |  |
| PL-02-015 | Korekta składek PPK — rezygnacja | 1148 | — |  | S |  |
| PL-02-016 | Korekta niesłusznie zapłaconych składek PPK | 1159 | — |  | S |  |
| PL-02-017 | Korekta tytułu ubezpieczenia | 1142 | — |  | S |  |

#### PL-03 · Potrącenia i zajęcia wynagrodzenia

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| PL-03-001 | Zajęcia wynagrodzenia na kartotece | 334 | P |  | S |  |
| PL-03-002 | Zajęcia wynagrodzenia — lista | 494 | P |  | S |  |
| PL-03-003 | Obsługa zajęć komorniczych | 924 | P |  | S |  |
| PL-03-004 | Zajęcia wynagrodzenia — konfiguracja | 1816 | — |  | K |  |
| PL-03-005 | Potrącenia dobrowolne (definicje elementów) | 1668 | — |  | K |  |
| PL-03-006 | Kolejność i limity potrąceń wg zasad klienta | — | — |  | C |  |

#### PL-04 · Podzielniki kosztów i dekretacja

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| PL-04-001 | Podzielniki kosztów na kartotece | 100 | — |  | K |  |
| PL-04-002 | Opis analityczny wypłat | 1295 | — |  | K |  |
| PL-04-003 | Zbiorcza lista opisów analitycznych | 1295 | — |  | S |  |
| PL-04-004 | Schematy księgowe list płac | — | — |  | K |  |
| PL-04-005 | Automatyzacja podziału kosztów wg reguł klienta | — | — |  | C |  |
| PL-04-006 | Eksport danych do księgowości (system zewnętrzny) | — | — |  | C |  |
| PL-04-007 | Pracownicy Koszty Projektów (dodatek) | 2180 | Z+ |  | S |  |

**Na co zwrócić uwagę:** jeśli księgowość jest w innym systemie niż enova,
`006` jest zawsze pozycją **C** i wymaga ustalenia formatu pliku po stronie
odbiorcy — to element, którego klient nie kontroluje sam, więc trzeba go wcześnie
umówić z dostawcą systemu księgowego.

---

### ZUS — Deklaracje i rozliczenia ZUS

#### ZUS-01 · Deklaracje zgłoszeniowe i rozliczeniowe

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| ZUS-01-001 | KEDU — pliki wymiany z Płatnikiem | 567 | — |  | S |  |
| ZUS-01-002 | ZUS DRA | 569 | — |  | S |  |
| ZUS-01-003 | ZUS RCA | 569 | — |  | S |  |
| ZUS-01-004 | ZUS RSA | 569 | — |  | S |  |
| ZUS-01-005 | ZUS ZUA | 142 | — |  | S |  |
| ZUS-01-006 | ZUS ZZA | 148 | — |  | S |  |
| ZUS-01-007 | ZUS ZIUA | 154 | — |  | S |  |
| ZUS-01-008 | ZUS ZWUA | 157 | — |  | S |  |
| ZUS-01-009 | ZUS ZCNA | 163 | — |  | S |  |
| ZUS-01-010 | ZUS ZCZA (historyczna) | 168 | — |  | X |  |
| ZUS-01-011 | ZUS RUD | 169 | — |  | S |  |
| ZUS-01-012 | ZUS RIA | 584 | — |  | S |  |
| ZUS-01-013 | Bilans otwarcia deklaracji RIA | 186 | — |  | K |  |
| ZUS-01-014 | Informacje IWA | 574 | Z |  | S |  |
| ZUS-01-015 | ZUS OSW | 576 | — |  | S |  |
| ZUS-01-016 | ZUS ZSWA | 577 | — |  | S |  |
| ZUS-01-017 | Okresy pracy w szczególnych warunkach | 187 | — |  | S |  |
| ZUS-01-018 | Okresy wykonywania pracy nauczycielskiej | 189 | — |  | S |  |
| ZUS-01-019 | Informacje IMIR | 583 | — |  | S |  |
| ZUS-01-020 | Roczna informacja IMIR | 969 | — |  | S |  |
| ZUS-01-021 | Zaświadczenia Z-3, Z-3a | 586 | — |  | S |  |
| ZUS-01-022 | ERP-7 — bilans otwarcia | 183 | — |  | K |  |
| ZUS-01-023 | ERP-7 — nieobecności | 184 | — |  | K |  |
| ZUS-01-024 | ERP-7 — wynagrodzenia | 185 | — |  | K |  |
| ZUS-01-025 | Indywidualny rachunek ZUS | 892 | — |  | S |  |
| ZUS-01-026 | Konfiguracja ZUS | 1454 | — |  | K |  |
| ZUS-01-027 | Konfiguracja ZUS — rozliczeniowe | 1458 | — |  | K |  |
| ZUS-01-028 | Korekty deklaracji ZUS | 1273 | — |  | S |  |

#### PIT-01 · Deklaracje podatkowe

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| PIT-01-001 | PIT-4R | 589 | — |  | S |  |
| PIT-01-002 | PIT-8AR | 595 | — |  | S |  |
| PIT-01-003 | PIT-11 | 603 | — |  | S |  |
| PIT-01-004 | PIT-R | 615 | — |  | S |  |
| PIT-01-005 | PIT-8C | 619 | — |  | S |  |
| PIT-01-006 | IFT-1, IFT-1R | 622 | — |  | S |  |
| PIT-01-007 | CIT-ST | 599 | — |  | S |  |
| PIT-01-008 | PIT-11Z | 626 | — |  | S |  |
| PIT-01-009 | PIT-RZ | 631 | — |  | S |  |
| PIT-01-010 | PIT-8CZ | 633 | — |  | S |  |
| PIT-01-011 | PIT-40, PIT-40Z (wycofane) | 636 | — |  | X |  |
| PIT-01-012 | Generowanie PIT-4R i PIT-8AR | 954 | — |  | S |  |
| PIT-01-013 | Generowanie PIT-11 | 959 | — |  | S |  |
| PIT-01-014 | Naliczanie rocznych deklaracji — konfiguracja | 948 | — |  | K |  |
| PIT-01-015 | Bilans otwarcia deklaracji PIT | 180 | — |  | K |  |
| PIT-01-016 | Ulga innowacyjna (PIT) | 174 | — |  | S |  |
| PIT-01-017 | Informacje dla PIT-40 | 175 | — |  | X |  |
| PIT-01-018 | Konfiguracja PIT — ogólne | 1448 | — |  | K |  |
| PIT-01-019 | Udostępnianie PIT pracownikom w pulpicie | 870 | Z |  | K |  |

#### PPK-01 · Pracownicze Plany Kapitałowe

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| PPK-01-001 | PPK — konfiguracja | 720 | — |  | K |  |
| PPK-01-002 | PPK — konfiguracja płacowa | 1785 | — |  | K |  |
| PPK-01-003 | Kwalifikacja osób do PPK | 763 | — |  | S |  |
| PPK-01-004 | Kwalifikacja PPK — lista | 483 | — |  | S |  |
| PPK-01-005 | Uczestnicy PPK | 480 | — |  | S |  |
| PPK-01-006 | Zgłoszenie i rezygnacja uczestników | 786 | — |  | S |  |
| PPK-01-007 | Rezygnacje PPK — lista | 481 | — |  | S |  |
| PPK-01-008 | Autozapis do PPK | 769 | — |  | S |  |
| PPK-01-009 | Naliczanie i rozliczenie składek PPK | 736 | — |  | S |  |
| PPK-01-010 | Rozliczenie składek — dokumenty | 657 | — |  | S |  |
| PPK-01-011 | Opodatkowanie składki finansowanej przez pracodawcę | 724 | — |  | S |  |
| PPK-01-012 | Opodatkowanie przychodu od PPK osób zagranicznych | 733 | — |  | S |  |
| PPK-01-013 | Eksport dokumentów pracodawcy do instytucji | 751 | — |  | K |  |
| PPK-01-014 | Import dokumentów instytucji finansowej | 755 | — |  | K |  |
| PPK-01-015 | Dokumenty zgłoszeniowe PPK | 659 | — |  | S |  |
| PPK-01-016 | Rozliczenie nadpłat PPK | 662 | — |  | S |  |
| PPK-01-017 | Dokumenty zwrotne | 663 | — |  | S |  |
| PPK-01-018 | Prezentacja danych PPK | 785 | — |  | S |  |
| PPK-01-019 | Wnioski PPK w pulpicie | 2006 | P |  | K |  |

**Na co zwrócić uwagę:** `013` i `014` zależą od instytucji finansowej klienta —
formaty plików różnią się między instytucjami. Ustalić na warsztacie, z którą
instytucją klient współpracuje, i czy enova obsługuje jej format w standardzie.

#### PFR-01 · PFRON

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| PFR-01-001 | Informacje PFRON na kartotece | 54 | — |  | S |  |
| PFR-01-002 | Wn-D | 643 | — |  | S |  |
| PFR-01-003 | DEK-R | 649 | — |  | S |  |
| PFR-01-004 | INF-2 | 652 | — |  | S |  |
| PFR-01-005 | Konfiguracja PFRON | 1427 | — |  | K |  |
| PFR-01-006 | PFRON — dane pełnomocnika | 1434 | — |  | K |  |
| PFR-01-007 | PFRON — rozliczeniowe | 1436 | — |  | K |  |
| PFR-01-008 | Deklaracje PFRON wycofane | 653 | — |  | X |  |

---

### PP — Pulpity i samoobsługa pracownicza

#### PP-01 · Pulpit Pracownika i Pulpit Kierownika

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| PP-01-001 | Ustawienia Pulpitów HR | 1940 | — |  | K |  |
| PP-01-002 | Ustawienia Pulpitów HR (od wersji 2604.1.1) | 2189 | — |  | K |  |
| PP-01-003 | Budowa Pulpitu Pracownika | 2191 | — |  | K |  |
| PP-01-004 | Budowa Pulpitu Kierownika | 2194 | — |  | K |  |
| PP-01-005 | Panel użytkownika | 1947 | — |  | K |  |
| PP-01-006 | Struktura HR | 1949 | — |  | K |  |
| PP-01-007 | Grupy pracownicze | 1952 | P |  | K |  |
| PP-01-008 | Struktura podległościowa — historyczność | 1330 | — |  | K |  |
| PP-01-009 | Przełożony na wniosku wg elementów struktury podległościowej | 1389 | — |  | K |  |
| PP-01-010 | Dane kadrowe pracownika w pulpicie | 1965 | — |  | S |  |
| PP-01-011 | Kadry pozostałe w pulpicie | 1966 | — |  | S |  |
| PP-01-012 | Deklaracje pracownika — podgląd PIT | 1970 | — |  | S |  |
| PP-01-013 | Dane finansowe — podgląd pasków wypłat | 1971 | — |  | S |  |
| PP-01-014 | Czas pracy i nieobecności w pulpicie | 1973 | — |  | S |  |
| PP-01-015 | Umowy w pulpicie | 1987 | — |  | S |  |
| PP-01-016 | Pulpit Kierownika — dane o pracownikach | 1989 | — |  | S |  |
| PP-01-017 | Pulpit Kierownika — czas pracy i nieobecności | 2002 | — |  | S |  |
| PP-01-018 | Obieg wniosku o nieobecność | 2028 | — |  | K |  |
| PP-01-019 | Obieg wniosku kadrowego | 2022 | — |  | K |  |
| PP-01-020 | Obieg wniosku kadrowego związanego z Polskim Ładem | 2025 | — |  | K |  |
| PP-01-021 | Wnioski związane z czasem pracy | 2036 | — |  | K |  |
| PP-01-022 | Wnioski związane z pracą zdalną | 2041 | — |  | K |  |
| PP-01-023 | Grupowy wniosek o premię | 2044 | P |  | K |  |
| PP-01-024 | Obieg wniosków opartych o dokumenty dodatkowe | 2006 | P |  | K |  |
| PP-01-025 | Konfiguracja obiegu wniosków | 2010 | P |  | K |  |
| PP-01-026 | E-wnioski własne klienta (poza standardem) | — | P |  | C |  |
| PP-01-027 | Zastępstwa pracownicze | 2053 | — |  | K |  |
| PP-01-028 | Moje zastępstwa | 1958 | — |  | S |  |
| PP-01-029 | Delegacje PWS w pulpicie | 1972 | — |  | S |  |
| PP-01-030 | Powiadomienia | 1950 | — |  | K |  |
| PP-01-031 | Historia powiadomień | 1951 | — |  | S |  |
| PP-01-032 | Ankiety | 1946 | — |  | K |  |
| PP-01-033 | Moje aktualności | 1948 | — |  | K |  |
| PP-01-034 | Konwersacje | 1962 | — |  | S |  |
| PP-01-035 | Biblioteka | 1961 | — |  | K |  |
| PP-01-036 | Zasobnik dokumentów | 1959 | — |  | S |  |
| PP-01-037 | Menu pulpitów od wersji 2604.1.1 | 2195 | — |  | K |  |

**Na co zwrócić uwagę:** `026` bywa niedoszacowane. Standardowe e-wnioski
pokrywają nieobecności, wnioski kadrowe, czas pracy i pracę zdalną. Każdy wniosek
poza tą listą (np. wniosek o świadczenie socjalne z własnym formularzem, wniosek
o szkolenie z akceptacją budżetu) to osobna pozycja **C** z definicją dokumentu
dodatkowego i ścieżką akceptacji.

---

### MIG — Migracja danych i bilans otwarcia

#### MIG-01 · Przeniesienie danych z systemu źródłowego

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| MIG-01-001 | Import kartotek pracowników | — | — |  | C |  |
| MIG-01-002 | Import historii zatrudnienia | — | — |  | C |  |
| MIG-01-003 | Bilans otwarcia danych podatkowych | 32 | — |  | K |  |
| MIG-01-004 | Bilans otwarcia deklaracji PIT | 180 | — |  | K |  |
| MIG-01-005 | Bilans otwarcia urlopów | 241 | — |  | K |  |
| MIG-01-006 | Bilans otwarcia czasu pracy | 238 | — |  | K |  |
| MIG-01-007 | Bilans otwarcia ERP-7 | 183 | — |  | K |  |
| MIG-01-008 | Bilans otwarcia deklaracji RIA | 186 | — |  | K |  |
| MIG-01-009 | Import planu pracy i zrealizowanego czasu pracy | — | — |  | C |  |
| MIG-01-010 | Import danych do e-teczek | — | Z |  | C |  |
| MIG-01-011 | Uzgodnienie danych po migracji (raport kontrolny) | — | — |  | C |  |

**Na co zwrócić uwagę:** cały ten obszar jest w praktyce **C** — enova nie ma
uniwersalnego importu z dowolnego systemu kadrowo-płacowego. Wycena zależy od
tego, w jakiej formie klient dostarczy dane, a nie od liczby pracowników. Ustalić
na warsztacie: format eksportu z systemu źródłowego, datę odcięcia i kto
odpowiada za jakość danych wejściowych.

---

### RAP — Raporty, wydruki i powiadomienia

#### RAP-01 · Raportowanie kadrowo-płacowe

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| RAP-01-001 | Wydruki kadrowe — standard | 1539 | — |  | S |  |
| RAP-01-002 | Wydruki płacowe i deklaracji | 1298 | — |  | S |  |
| RAP-01-003 | Karta ewidencji czasu pracy | 1298 | — |  | S |  |
| RAP-01-004 | Karta ewidencji szczegółowa z godzinami dyżurów | 1301 | — |  | S |  |
| RAP-01-005 | Zestawienia czasu pracy | 286 | — |  | K |  |
| RAP-01-006 | Statystyki czasu pracy | 443 | — |  | S |  |
| RAP-01-007 | Eksport danych do Excela z listy | — | — |  | S |  |
| RAP-01-008 | Raporty i wydruki dedykowane klienta | — | — |  | C |  |
| RAP-01-009 | Modyfikacja wydruków standardowych (własny layout) | — | — |  | C |  |
| RAP-01-010 | Powiadomienia systemowe | 1950 | — |  | K |  |
| RAP-01-011 | Powiadomienia mailowe | 1950 | — |  | K |  |
| RAP-01-012 | Automatyzacja wysyłki powiadomień | — | — |  | C |  |

**Na co zwrócić uwagę:** `008` i `009` to najczęściej rozdmuchiwana część
zakresu. Poprosić o **listę raportów faktycznie używanych**, nie o wszystkie, jakie
klient ma w starym systemie — i wycenić każdy osobno, bo raport z podziałem na
wydziały i raport z wyliczeniami wg własnych reguł to zupełnie inna praca.

---

### INT — Integracje

#### INT-01 · Wymiana danych z systemami zewnętrznymi

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| INT-01-001 | eDeklaracje — konfiguracja | 1922 | — |  | K |  |
| INT-01-002 | eDeklaracje — instalacja certyfikatów | 1923 | — |  | K |  |
| INT-01-003 | eDeklaracje — lista i wysyłka | 1924 | — |  | S |  |
| INT-01-004 | eDeklaracje w module Kadry Płace | 1925 | — |  | S |  |
| INT-01-005 | UPO — urzędowe potwierdzenie odbioru | 1937 | — |  | S |  |
| INT-01-006 | Integracja z PUE (import e-zwolnień) | 681 | Z |  | S |  |
| INT-01-007 | Pliki KEDU do programu Płatnik | 567 | — |  | S |  |
| INT-01-008 | Eksport i import dokumentów PPK | 751 | — |  | K |  |
| INT-01-009 | Synchronizacja KZP z bazą księgową | 1026 | — |  | K |  |
| INT-01-010 | Wymiana danych (ochrona danych osobowych) | 563 | P |  | S |  |
| INT-01-011 | Integracja z systemem kadrowym grupy kapitałowej | — | — |  | C |  |
| INT-01-012 | Integracja z systemem RCP (poza standardem) | — | — |  | C |  |
| INT-01-013 | Integracja z systemem księgowym | — | — |  | C |  |
| INT-01-014 | Dedykowane integracje klienckie (pozostałe) | — | — |  | C |  |

**Na co zwrócić uwagę:** każda pozycja **C** w tym bloku wymaga drugiej strony —
dostawcy systemu, z którym się integrujemy. Bez ustalonego kontraktu wymiany
(format, kierunek, częstotliwość, kto inicjuje, co przy błędzie) pozycji nie da
się wycenić. To najczęstsza przyczyna poślizgów harmonogramu.

---

### OG — Funkcjonalności ogólnosystemowe

#### OG-01 · Konfiguracja, uprawnienia i bezpieczeństwo

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| OG-01-001 | Cechy (atrybuty własne) | 1917 | — |  | K |  |
| OG-01-002 | Cechy wyliczane (z algorytmem) | 1917 | — |  | C |  |
| OG-01-003 | Prawa dostępu do danych kadrowych | 1794 | — |  | K |  |
| OG-01-004 | Prawa dostępu na kartotece pracownika | 70 | — |  | K |  |
| OG-01-005 | Ustawienia operatora | 1845 | — |  | K |  |
| OG-01-006 | Ochrona danych osobowych (moduł) | 367 | P |  | S |  |
| OG-01-007 | Oświadczenia RODO | 559 | P |  | S |  |
| OG-01-008 | Uprawnienia do danych osobowych | 561 | P |  | S |  |
| OG-01-009 | Oświadczenia GIODO (lista) | 469 | — |  | S |  |
| OG-01-010 | Historia zapisów (log zmian) | 372 | — |  | S |  |
| OG-01-011 | Dokumenty dodatkowe | 369 | P |  | K |  |
| OG-01-012 | Definicje dokumentów dodatkowych | 1887 | P |  | K |  |
| OG-01-013 | Formularze — konfiguracja zakładek | 1462 | — |  | K |  |
| OG-01-014 | Pracownika — dodatkowa zakładka | 1469 | — |  | K |  |
| OG-01-015 | Zakładki definiowane na elementach wynagrodzenia | 1762 | — |  | K |  |
| OG-01-016 | Rozrachunki | 368 | — |  | S |  |
| OG-01-017 | Definicje oświadczeń | 1489 | — |  | K |  |
| OG-01-018 | Definicje zadań | 1500 | — |  | K |  |
| OG-01-019 | Zadania (dokumenty i wnioski) | 363 | P |  | S |  |
| OG-01-020 | Weryfikatory pól i obiektów | 1652 | — |  | K |  |
| OG-01-021 | COVID-19 (konfiguracja historyczna) | 1475 | — |  | X |  |

---

### DOD — Dodatki do modułu Kadry i Płace

Dodatki producenta wymagają osobnej licencji (kolumna **Z+**). Wchodzą do zakresu
tylko wtedy, gdy klient je kupi — do czasu decyzji trzymamy je jako **X**
z adnotacją, czego dotyczy decyzja.

| Nr | Funkcjonalność systemowa | Instr. | Lic. | Wymagania Klienta | Kl. | Opis realizacji |
|---|---|---|---|---|---|---|
| DOD-01-001 | Czas pracy (współpraca z 4Trans, RCP Regitech) | 2174 | Z+ |  | X |  |
| DOD-01-002 | Edycja Kalendarza w Pulpicie Pracownika | 2175 | Z+ |  | X |  |
| DOD-01-003 | Pakiet mobilności (kierowcy) | 2178 | Z+ |  | X |  |
| DOD-01-004 | Pracownicy Eksportowi | 2179 | Z+ |  | X |  |
| DOD-01-005 | Pracownicy Koszty Projektów | 2180 | Z+ |  | X |  |
| DOD-01-006 | Pracownicy Prokuratury | 2181 | Z+ |  | X |  |
| DOD-01-007 | Pracownicy Uczelni | 2182 | Z+ |  | X |  |
| DOD-01-008 | Rozrachunki z funduszu pożyczkowego | 2183 | Z+ |  | X |  |
| DOD-01-009 | Symmetrical | 2184 | Z+ |  | X |  |
| DOD-01-010 | Worksmile | 2185 | Z+ |  | X |  |
| DOD-01-011 | Zarządzanie odzieżą roboczą | 2186 | Z+ |  | X |  |
| DOD-01-012 | Archivista | 2173 | Z+ |  | X |  |
| DOD-01-013 | eRecruiter | 2176 | Z+ |  | X |  |

---

### Podsumowanie zakresu

Zakres obejmuje **27 procesów** i **482 pozycje** funkcjonalne.

| Obszar | Procesy | Pozycji | S | K | C | X |
|---|---|---|---|---|---|---|
| KAD — Kadry | 6 | 124 | 85 | 35 | 2 | 2 |
| CP — Czas pracy | 4 | 68 | 25 | 38 | 5 | 0 |
| UN — Urlopy i nieobecności | 3 | 43 | 31 | 11 | 1 | 0 |
| PL — Płace | 4 | 65 | 41 | 19 | 5 | 0 |
| ZUS — Deklaracje ZUS | 1 | 28 | 21 | 6 | 0 | 1 |
| PIT — Deklaracje podatkowe | 1 | 19 | 13 | 4 | 0 | 2 |
| PPK | 1 | 19 | 14 | 5 | 0 | 0 |
| PFR — PFRON | 1 | 8 | 4 | 3 | 0 | 1 |
| PP — Pulpity | 1 | 37 | 13 | 23 | 1 | 0 |
| MIG — Migracja | 1 | 11 | 0 | 6 | 5 | 0 |
| RAP — Raporty | 1 | 12 | 6 | 3 | 3 | 0 |
| INT — Integracje | 1 | 14 | 6 | 4 | 4 | 0 |
| OG — Ogólnosystemowe | 1 | 21 | 7 | 12 | 1 | 1 |
| DOD — Dodatki producenta | 1 | 13 | 0 | 0 | 0 | 13 |
| **Razem** | **27** | **482** | **266** | **169** | **27** | **20** |

Rozkład wariantów licencji: **72** pozycje wymagają wersji platynowej, **28** co
najmniej złotej, **17** osobnej licencji na dodatek, a **365** nie ma w instrukcji
warunku licencyjnego.

#### Jak czytać te liczby

Proporcja **266 S / 169 K / 27 C** to obraz *projektu bez ustaleń z klientem* —
tak wygląda zakres, gdy patrzymy tylko na to, co produkt umie. Na warsztacie
liczba **C** zawsze rośnie, bo dochodzą do niej wymagania z regulaminów klienta.
Jeśli po analizie nadal jest 27, to znaczy, że nie zadaliśmy dość pytań, a nie że
klient nie ma nietypowych zasad.

Liczba **169 K** to realna praca konsultanta i ona decyduje o długości wdrożenia
częściej niż pozycje **C**. Warto to pokazać klientowi wprost: „standard" nie
znaczy „nic nie trzeba robić".

Uwaga na **72 pozycje platynowe**. Jeśli klient ma licencję złotą, to znaczna
część obszaru czasu pracy (RCP, rozliczenie czasu pracy, weryfikatory, planowanie
kalendarzy, magazyn nadgodzin) oraz ZFŚS i ochrona danych osobowych są
niedostępne. Ustalenie wariantu licencji jest więc warunkiem wstępnym, a nie
formalnością — bez tego cała tabela jest hipotezą.

---

### Zasady numeracji

1. **Identyfikator jest niezmienny.** Raz nadany `Nr` zostaje z pozycją do końca
   projektu, także gdy zmieni się jej nazwa albo klasyfikacja. Po nim odwołują się
   do zakresu umowa, harmonogram i protokoły odbioru.
2. **Nie przenumerowujemy.** Usunięcie pozycji z zakresu oznacza oznaczenie jej
   jako **X** z powodem, nie usunięcie wiersza i przesunięcie kolejnych numerów.
3. **Nowe pozycje dopisujemy na końcu procesu**, kolejnym wolnym numerem — także
   gdy w numeracji zostały luki po pozycjach z punktu 2.
4. **Prefiks obszaru jest unikalny.** Nowy obszar to nowy prefiks; nie wolno
   dwóm różnym procesom nadać tego samego identyfikatora (np. dwa razy `KAD-03`),
   bo wtedy numery pozycji przestają być jednoznaczne i rozjeżdżają się odwołania
   w umowie.
5. **Wymagania klienta nie tworzą nowych numerów w tej tabeli.** Jeśli klient
   zgłasza potrzebę bez odpowiednika w produkcie, zakładamy nową pozycję
   z klasyfikacją **C** i pustą kolumną *Instr.* — to sygnał, że nie ma jej
   w dokumentacji producenta.

---

### Co musi być na wyjściu tego rozdziału

Rozdział uznajemy za zamknięty, gdy:

- [ ] każda pozycja ma wypełnioną kolumnę **Kl.** potwierdzoną z klientem
      (nie zostawioną na propozycji konsultanta);
- [ ] każda pozycja **C** ma opis wymagania w kolumnie *Wymagania Klienta*
      albo odsyłacz do załącznika z takim opisem;
- [ ] każda pozycja **C** ma wskazaną osobę decyzyjną po stronie klienta,
      szacunek pracochłonności i informację, czy blokuje start produkcyjny;
- [ ] każda pozycja **X** ma zapisany powód wykluczenia;
- [ ] potwierdzony jest wariant licencji klienta i wynikająca z niego lista
      pozycji niedostępnych;
- [ ] klient podpisał zakres — od tej chwili każda nowa pozycja jest zmianą
      zakresu, a nie „doprecyzowaniem analizy".

> **Uwaga metodyczna.** Ta tabela jest listą **funkcjonalności produktu**, nie
> listą wymagań klienta. Wymagania wpisujemy do kolumny *Wymagania Klienta*
> przy odpowiedniej funkcjonalności. Jeśli wymagania nie da się nigdzie wpisać,
> to jest dokładnie ten moment, w którym odkrywamy rozwiązanie dedykowane —
> i właśnie po to ta tabela jest zbudowana z funkcji produktu, a nie z życzeń.

---

## 2. Kontekst i zakres wdrożenia

1. Ile podmiotów (baz) obsługujemy? Jedna firma czy grupa kapitałowa?
2. Ile pracowników na etacie, ilu zleceniobiorców, ilu emerytów/byłych pracowników
   nadal obsługiwanych (ZFŚS, PIT)?
3. Jaka jest **data startu produkcyjnego** i od którego miesiąca pierwsza wypłata z enovy?
4. Czy równolegle działa stary system (praca podwójna) i jak długo?
5. Kto w firmie jest właścicielem procesu kadrowego, a kto płacowego? Kto podpisuje odbiór?
6. Czy kadry i płace robi ten sam zespół? Czy płace są outsourcowane?
7. Czego klient oczekuje jako największej zmiany na plus (to jest kryterium sukcesu wdrożenia
   i warto je zapisać dosłownie, jego słowami).

**Na co zwrócić uwagę:** jeśli klient nie umie nazwać kryterium sukcesu, wdrożenie skończy się
sporem o zakres. Zapisać jedno–dwa zdania i wrócić do nich przy odbiorze.

---

## 3. Struktura organizacyjna

1. Jak wygląda struktura: oddziały, wydziały, działy, projekty, MPK?
2. Czy struktura służy tylko do raportowania, czy też rozdziela koszty na konta księgowe?
3. Czy pracownik może być przypisany do więcej niż jednej jednostki jednocześnie
   (np. praca na dwóch wydziałach, rozdzielnik kosztów procentowy)?
4. Czy jest zdefiniowana **struktura podległości** (kto jest przełożonym kogo)
   i czy ma sterować akceptacją wniosków na pulpitach?
5. Jak często struktura się zmienia? Czy potrzebna jest jej historia (stan na dzień)?
6. Czy pracownicy zmieniają oddział/wydział w trakcie miesiąca – i jak wtedy dzielimy koszt?

**Czerwone flagi (→ C):** rozdzielnik kosztów procentowy per pracownik, potrzeba
raportowania „stan struktury na dzień X" wstecz, dwie równoległe hierarchie
(służbowa i kosztowa), podległość inna niż wynikająca z wydziału.

---

## 4. Kadry – kartoteka pracownika i zatrudnienie

1. Jakie dane muszą być na kartotece poza standardem? (numer karty RCP, numer szafki,
   identyfikator z systemu produkcyjnego, dane do ubezpieczenia grupowego)
2. Jakie rodzaje umów: o pracę (na czas określony/nieokreślony/zastępstwo), zlecenie,
   dzieło, B2B, umowa z uczniem, kontrakt menedżerski?
3. Czy występuje zatrudnianie na kilku umowach jednocześnie u tego samego pracodawcy?
4. Kto i kiedy tworzy kartotekę – kadry po podpisaniu umowy, czy wcześniej (rekrutacja)?
5. Jak numerowani są pracownicy? Czy numer/kod ma znaczenie biznesowe (np. zawiera dział)?
6. Czy trzeba pilnować terminów: badania lekarskie, szkolenia BHP, uprawnienia, zezwolenia
   na pracę cudzoziemców, ważność dokumentów? Kto ma być o nich powiadamiany i jak?
7. Jak wygląda proces zakończenia zatrudnienia? Kto wystawia świadectwo pracy,
   kto liczy ekwiwalent, kto odbiera sprzęt?
8. Czy są pracownicy zagraniczni / oddelegowani / z ograniczoną rezydencją podatkową?
9. Czy potrzebne są własne wydruki kadrowe (umowa, aneks, zaświadczenie o zarobkach)
   w layoucie klienta?

**Czerwone flagi (→ C):** własne szablony umów z klauzulami zależnymi od warunków
zatrudnienia, automatyczne powiadamianie mailem o terminach, walidacja „nie zapisz
pracownika bez X" (to weryfikator – czyli kod), obsługa cudzoziemców z pełną kontrolą
ważności zezwoleń.

---

## 5. Czas pracy – plan (kalendarze, normy, rozkłady)

1. Jakie systemy czasu pracy występują? (podstawowy, równoważny, ruchomy, weekendowy,
   praca w ruchu ciągłym, zadaniowy, skrócony tydzień)
2. Jakie są **okresy rozliczeniowe** – 1, 3, 4, 12 miesięcy? Czy różne dla różnych grup?
3. Kto tworzy grafiki i w jakim narzędziu? Excel czy system? Na ile tygodni do przodu?
4. Czy grafik jest zatwierdzany? Przez kogo? Czy po zatwierdzeniu wolno go zmieniać?
5. Ile jest zmian i jak są oznaczane? (I/II/III, nocka, dyżur, „12-godzinna")
6. Czy praca zmianowa przechodzi przez północ – i do której doby przypisujemy godziny?
7. Jak wyliczana jest **norma** dla pracownika (pełny etat, część etatu, zmiana etatu
   w trakcie miesiąca, zatrudnienie/zwolnienie w trakcie okresu rozliczeniowego)?
8. Jak traktowane są święta wypadające w dniu wolnym pracownika?
9. Czy występują strefy czasu pracy inne niż praca podstawowa (szkolenie, delegacja,
   przestój, praca zdalna, dyżur, gotowość)?
10. Czy praca zdalna/hybrydowa jest ewidencjonowana i rozliczana (ryczałt, ekwiwalent)?

**Czerwone flagi (→ C):** własne reguły wyznaczania normy przy zmianie etatu w trakcie
okresu, grafik generowany automatycznie z reguł (obsada minimalna), kontrola
11-godzinnego odpoczynku dobowego i 35-godzinnego tygodniowego, zakaz edycji grafiku
po zamkniętym okresie, import grafików z Excela w formacie klienta.

---

## 6. Czas pracy – rejestracja rzeczywista i RCP

1. Czy jest czytnik RCP? Jaki system, jaki format eksportu (plik CSV/XML, baza, API)?
2. Czy odbicia wchodzą do enovy automatycznie (harmonogram) czy ręcznym importem?
3. Czy pracownik odbija wejście i wyjście, czy tylko wejście? Co przy braku odbicia?
4. Jak traktujemy **wczesne wejście i późne wyjście** – jako nadgodziny, czy tylko
   przekroczenie ponad plan liczy się po akceptacji przełożonego?
5. Czy istnieje tolerancja (np. 5 minut spóźnienia bez konsekwencji)? Zaokrąglenia
   (do 15 minut w górę/dół)?
6. Czy przerwy są odbijane (przerwa, wyjście prywatne, wyjście służbowe)?
7. Kto poprawia błędne odbicia i na jakiej podstawie? Czy zostaje ślad korekty?
8. Czy odbicia mają być widoczne dla pracownika na pulpicie?
9. Jak rozliczamy pracowników nieodbijających (kierownictwo, pracownicy mobilni)?
10. Kiedy zamykamy miesiąc czasu pracy i czy po zamknięciu można coś zmienić
    (blokada wstecz)?

**Czerwone flagi (→ C):** własna logika „odbicie → dzień pracy" (podział na strefy,
tolerancje, zaokrąglenia, odcinanie nadgodzin nieakceptowanych), integracja z RCP,
raport rozbieżności plan/wykonanie, blokada edycji dni wstecz, przypisywanie godzin
do doby poprzedniej przy pracy nocnej.

---

## 7. Nadgodziny, dodatki za czas pracy, dyżury

1. Kto i na jakim etapie **akceptuje nadgodziny**? Czy bez akceptacji nie są płatne?
2. Jak dzielone są nadgodziny na 50% i 100%? Czy stosowane są reguły ustawowe,
   czy własne (np. wszystkie nadgodziny 100%)?
3. Czy nadgodziny dobowe i średniotygodniowe rozliczane są zgodnie z kodeksem,
   czy klient ma tu swoje odstępstwa (korzystniejsze)?
4. Czy pracownik może wybrać **odbiór nadgodzin czasem wolnym** zamiast wypłaty?
   Na wniosek pracownika (1:1) czy polecenie pracodawcy (1:1,5)? Jaki jest termin odbioru?
5. Jak rozliczamy pracę w dniu wolnym (niedziela, święto, dzień harmonogramowo wolny)?
6. Jakie dodatki za warunki pracy? (nocny, szkodliwe, uciążliwe, wysokościowe,
   za pracę w brygadzie, dodatek zmianowy)
7. Czy dodatek nocny to stawka ustawowa, czy stała kwota/procent ustalony u klienta?
8. Jak wyglądają dyżury – domowe, pod telefonem, w zakładzie? Jak płatne?
9. Czy występuje delegacja/podróż służbowa i czy ma wpływ na czas pracy i wynagrodzenie?
10. Czy występują przestoje i jak są płatne?

**Czerwone flagi (→ C):** własny podział nadgodzin na strefy, dodatki liczone od innej
podstawy niż standard, limity nadgodzin z kontrolą przy zapisie, dodatek zmianowy
zależny od liczby przepracowanych zmian w miesiącu, rozliczanie odbiorów nadgodzin
z kontrolą terminu wykorzystania.

---

## 8. Nieobecności i urlopy

1. Jakie rodzaje nieobecności występują poza standardowymi? Czy klient ma własne
   (np. urlop dodatkowy branżowy, wolne za krew, dzień na poszukiwanie pracy)?
2. Jak liczony jest wymiar urlopu – w dniach czy w godzinach?
3. Czy są urlopy dodatkowe (niepełnosprawność, staż, układ zbiorowy)?
4. Jak obsługiwane są urlopy zaległe i ich przedawnienie? Czy system ma pilnować limitu?
5. Jak wygląda ścieżka **wniosku urlopowego**: papier, mail czy pulpit pracownika?
   Ile poziomów akceptacji? Co, gdy przełożony jest nieobecny (zastępstwo)?
6. Czy urlop na żądanie ma odrębny limit i odrębną ścieżkę (np. zgłoszenie tego samego dnia)?
7. Kto wprowadza zwolnienia lekarskie – czy pobierane są z **e-ZLA (ZUS PUE)**?
8. Czy firma jest płatnikiem zasiłków? Kto liczy zasiłki – kadry czy ZUS?
9. Jak obsługiwane są korekty nieobecności (zwolnienie wpłynęło po wypłacie)?
10. Czy potrzebna jest kontrola kolizji nieobecności z grafikiem i z obsadą działu?

**Czerwone flagi (→ C):** własne rodzaje nieobecności z własnym wymiarem, e-wniosek
z niestandardową logiką (np. „dzisiaj = automatycznie na żądanie"), akceptacja
wielopoziomowa z macierzą zastępstw, kontrola obsady minimalnej przy akceptacji urlopu,
limit liczony inaczej niż w standardzie.

---

## 9. Płace – składniki wynagrodzenia

Najważniejszy blok. Przejść **po kolei przez każdą pozycję z przykładowej listy płac**
klienta i dla każdej ustalić: podstawę, sposób liczenia, wpływ na ZUS/PIT, wpływ na
podstawy urlopu/chorobowego/nadgodzin i sposób wykazania na pasku.

1. Jakie formy wynagrodzenia zasadniczego: miesięczne, godzinowe, akordowe, prowizyjne?
2. Jakie premie? Dla każdej: **od czego liczona** (podstawa), za jaki okres,
   kto podaje dane wejściowe, czy jest uznaniowa, czy pomniejszana za nieobecności,
   czy wchodzi do podstawy urlopu i chorobowego.
3. Czy występują dodatki stażowe, funkcyjne, za znajomość języka, za mentoring?
4. Czy są nagrody jubileuszowe? Jaka tabela, jaki staż jest liczony (tylko u tego
   pracodawcy czy cały?), jak ustalana podstawa?
5. Odprawy: emerytalno-rentowa, z tytułu zwolnień grupowych, pośmiertna – jakie tabele,
   jaka podstawa, czy staż liczony ze wszystkich świadectw pracy?
6. Wynagrodzenie za czas niedostępności: jak liczony jest urlop, przestój, opieka?
7. Czy są świadczenia rzeczowe / benefity doliczane do podstawy (samochód, mieszkanie,
   ubezpieczenie, karta sportowa, pakiet medyczny)? Kto dostarcza dane i w jakim formacie?
8. Czy występuje **wyrównanie do minimalnego wynagrodzenia** i jak liczone przy części etatu?
9. Czy są wypłaty niestandardowe: wyrównania wsteczne, podwyżka z datą wsteczną,
   wypłata po ustaniu zatrudnienia, świadczenia po śmierci pracownika?
10. Czy są wypłaty pozalistowe i zaliczki? Jak rozliczane?

**Czerwone flagi (→ C):** każda premia z regulaminu, którą da się opisać wzorem – to
element płacowy z własnym algorytmem. Każdy składnik zależny od danych z innego systemu.
Nagroda jubileuszowa i odprawa emerytalna – prawie zawsze kod, bo tabele i sposób
liczenia stażu są indywidualne.

**Praktyczna wskazówka:** przy każdym nietypowym składniku zapytać, czy w obecnym
systemie działa automatycznie, czy księgowa wpisuje kwotę ręcznie. Jeśli ręcznie –
często najtaniej zostawić to jako wpis ręczny (S/K) niż kodować (C). Decyzja klienta,
ale trzeba mu pokazać różnicę w koszcie.

---

## 10. Płace – proces miesięczny i listy płac

1. Ile jest **list płac w miesiącu** i jakich rodzajów? (etatowa, umowy cywilnoprawne,
   premiowa, korygująca, zasiłkowa, ZFŚS, nagrody)
2. Jakie są terminy: dzień wypłaty, dzień zamknięcia czasu pracy, dzień przekazania
   danych do płac?
3. Czy wynagrodzenie jest wypłacane w miesiącu przepracowanym, czy do 10. następnego?
   (to determinuje rozliczanie nadgodzin i nieobecności „do przodu")
4. Kto dostarcza dane zmienne (premie, godziny, potrącenia) i w jakiej formie?
5. Jak wygląda ścieżka akceptacji listy płac przed przelewem?
6. Jak robione są **korekty** – lista korygująca, storno, rozliczenie w kolejnym miesiącu?
7. Czy potrzebna jest symulacja / plan listy płac (rezerwy, budżet wynagrodzeń)?
8. Jak wygląda przekazanie do banku – plik w jakim formacie, jaki bank, przelewy
   zbiorcze czy indywidualne?
9. Jak wyglądają **paski wypłaty** – papier, mail, pulpit pracownika? Czy zaszyfrowane?
   Czy klient wymaga własnego layoutu?
10. Jak wygląda księgowanie płac – schemat dekretacji, rozdzielnik na MPK/projekty,
    czy księgowość jest w enovie czy w innym systemie?

**Czerwone flagi (→ C):** własny format pliku bankowego, własny layout paska,
paski wysyłane mailem z hasłem, wieloetapowa akceptacja listy płac,
rozdzielnik kosztów wg przepracowanych godzin na projektach.

---

## 11. Potrącenia, PPK, ZFŚS, ubezpieczenia grupowe

1. Jakie potrącenia dobrowolne? (kasa zapomogowa, pożyczki, związki zawodowe, ubezpieczenia)
2. Jak obsługiwane są **zajęcia komornicze** – ile ich jest, czy trzeba pilnować
   kwoty wolnej i kolejności zajęć, czy potrzebna jest korespondencja z komornikiem?
3. Pożyczki pracownicze: z ZFŚS czy z zakładowej kasy? Raty, oprocentowanie, harmonogram,
   zawieszenie spłaty, rozliczenie przy zwolnieniu?
4. PPK: kto jest instytucją finansową, jak wygląda plik do PPK, kto obsługuje rezygnacje
   i automatyczne ponowne zapisy?
5. ZFŚS: jakie świadczenia (wczasy pod gruszą, paczki, zapomogi, dofinansowanie
   wypoczynku dzieci)? Jaki jest **próg dochodowy** i jak liczony dochód na członka rodziny?
6. Czy ZFŚS obsługuje też emerytów i byłych pracowników?
7. Kto zatwierdza wnioski socjalne (komisja socjalna)? Czy potrzebny jest raport dla komisji?
8. Ubezpieczenia grupowe: ile polis, jakie warianty, kto przekazuje listę do ubezpieczyciela?
9. Czy są świadczenia opodatkowane i nieopodatkowane (limity zwolnień) – kto pilnuje limitów?

**Czerwone flagi (→ C):** tabela progów ZFŚS z własnym sposobem liczenia dochodu
na członka rodziny, raport dla komisji socjalnej, plik do ubezpieczyciela,
obsługa zajęć z wieloma tytułami i kwotą wolną liczoną nietypowo.

---

## 12. Deklaracje, sprawozdawczość, rozliczenia zewnętrzne

1. Kto wysyła deklaracje ZUS – enova → Płatnik, czy bezpośrednio e-Płatnik?
2. Jakie deklaracje PIT i kiedy? Czy PIT-11 przekazywany jest pracownikom elektronicznie?
3. Czy firma rozlicza PFRON (Wn-D, INF)? Czy potrzebne są dane o stopniu
   niepełnosprawności i schorzeniach szczególnych?
4. Jakie sprawozdania GUS są robione? (Z-03, Z-05, Z-06, Z-12, DG-1)
5. Czy są raporty dla instytucji zewnętrznych: PIP, urząd pracy, ubezpieczyciel, audyt?
6. Czy występuje raportowanie do grupy kapitałowej / zagranicznej centrali
   (headcount, FTE, koszty w podziale na kategorie)? W jakim formacie i języku?
7. Czy obowiązuje raportowanie jawności wynagrodzeń / luki płacowej?
8. Czy wymagane są zestawienia do budżetowania i planowania kosztów osobowych?

**Czerwone flagi (→ C):** każdy raport do centrali w obcym formacie, raporty
w innej walucie/układzie niż polski, luka płacowa (wymaga uzgodnienia metodologii),
sprawozdania łączone z wielu baz.

---

## 13. Pulpity i samoobsługa pracownicza

1. Czy kupione są Pulpity: Pracownika, Kierownika, Kadrowy (HR)? Ilu użytkowników?
2. Jak pracownik loguje się do pulpitu (własne konto, AD/SSO, telefon)?
   Czy pracownicy produkcyjni mają komputery/telefony?
3. Co pracownik ma widzieć: paski, limity urlopu, czas pracy, dane osobowe,
   zaświadczenia, PIT-11, e-teczka?
4. Które wnioski mają być elektroniczne: urlop, nadgodziny, delegacja, zmiana danych,
   zaświadczenie, ZFŚS, PPK?
5. Jaka jest ścieżka akceptacji każdego wniosku i kto jest zastępcą przy nieobecności?
6. Czy pracownik może sam edytować swoje dane (adres, konto bankowe) – i czy zmiana
   wymaga akceptacji kadr?
7. Czy kierownik ma planować grafik na pulpicie? Czy ma widzieć wynagrodzenia podwładnych?
8. Czy potrzebne są powiadomienia (mail) o wniosku do akceptacji?

**Czerwone flagi (→ C):** własne typy e-wniosków, akceptacja wg macierzy innej niż
struktura podległości, ograniczanie widoczności danych na pulpicie wg cechy pracownika,
własne zakładki na pulpicie.

---

## 14. Migracja danych i bilans otwarcia

1. Z jakiego systemu migrujemy? W jakiej formie da się dane wyeksportować?
2. Co migrujemy, a co zostaje w starym systemie (dostęp „do wglądu")?
3. Zakres minimalny: kartoteki pracowników aktywnych, historia zatrudnienia, etaty,
   dane do ZUS/PIT, **limity urlopowe**, dane do podstaw chorobowego (12 miesięcy),
   nierozliczone pożyczki i zajęcia, staże do jubileuszy i odpraw.
4. Czy migrujemy **historyczne wypłaty**? Ile lat wstecz? Po co (PIT, zaświadczenia,
   podstawy zasiłkowe, Rp-7)?
5. Czy migrujemy historię czasu pracy? Czy wystarczy saldo okresu rozliczeniowego?
6. Czy migrujemy pracowników zwolnionych? Ilu i jak głęboko?
7. Kto odpowiada za jakość danych źródłowych i kto potwierdza poprawność migracji?
8. Jak wygląda **test migracji**: równoległe naliczenie tego samego miesiąca w obu
   systemach i uzgodnienie co do złotówki. Kto to robi i ile iteracji zakładamy?
9. Co z e-teczkami / dokumentacją skanowaną?

**Na co zwrócić uwagę:** migracja jest najczęstszą przyczyną opóźnień wdrożenia.
Zapisać wprost: kto przygotowuje pliki, w jakim terminie, w jakim formacie,
ile rund poprawek jest w cenie. Podstawy chorobowego i limity urlopowe to dwa
miejsca, w których braki wyjdą dopiero przy pierwszej wypłacie.

---

## 15. Raporty i wydruki

1. Które raporty klient realnie drukuje/wysyła co miesiąc? (poprosić o egzemplarze)
2. Które z nich pokrywa standard enovy, a które wymagają własnego wydruku?
3. Jaki format wyjściowy: PDF, XLSX, CSV? Czy plik ma być dalej przetwarzany
   (wtedy potrzebna jest „płaska" tabela, nie ładny wydruk)?
4. Czy wydruki mają mieć logo/pieczątkę/stopkę klienta i dane oddziału?
5. Czy raporty mają być uruchamiane z listy z zaznaczeniem wielu rekordów?
6. Kto ma mieć dostęp do których raportów (raporty płacowe = dane wrażliwe)?
7. Czy potrzebne są zestawienia czasu pracy w układzie „pracownik × dni miesiąca"?
8. Czy dane mają iść do narzędzia BI / hurtowni?

**Czerwone flagi (→ C):** każdy raport w layoucie klienta, eksport do XLSX
z formatowaniem (scalenia, szerokości kolumn, hasło na pliku), zestawienia z własnymi
kolumnami wyliczanymi, raporty łączące dane kadrowe z zewnętrznymi.

---

## 16. Integracje

Dla **każdej** integracji ustalić: kierunek, zakres danych, format, częstotliwość,
mechanizm (plik/API/baza), stronę odpowiedzialną za wykonanie, obsługę błędów
i sposób testowania.

1. RCP / kontrola dostępu.
2. Bank (przelewy wynagrodzeń, zaliczek, PIT/ZUS).
3. Płatnik / e-Deklaracje / e-ZLA.
4. PPK – instytucja finansowa.
5. Systemy HR: rekrutacja, ocena pracownicza, szkolenia, benefity.
6. System produkcyjny / MES / projektowy (godziny na zlecenia i projekty).
7. Księgowość, jeśli inna niż enova.
8. Active Directory / SSO / zakładanie i blokowanie kont pracowniczych.
9. Hurtownia danych / BI / raportowanie do centrali.
10. Kto jest właścicielem technicznym po drugiej stronie i czy jest dostępny w projekcie?

**Na co zwrócić uwagę:** integracja bez wskazanej osoby technicznej po stronie
drugiego systemu jest ryzykiem harmonogramu, nie zakresu. Zapisać kontakt (rolą)
i zażądać przykładowego pliku albo dostępu do API na etapie analizy, nie na etapie testów.
Osobno ustalić, co się dzieje, gdy integracja zawiedzie – kto to zauważy.

---

## 17. Uprawnienia, bezpieczeństwo, RODO

1. Jakie role użytkowników i kto ma widzieć wynagrodzenia (całej firmy / swojego działu)?
2. Czy kierownik widzi dane kadrowe podwładnych? W jakim zakresie?
3. Czy potrzebne jest ograniczenie dostępu do danych wg oddziału/wydziału
   (prawa do danych, nie tylko do funkcji)?
4. Kto ma prawo modyfikować definicje (elementy płacowe, kalendarze) na produkcji?
5. Czy wymagany jest **dziennik zmian / audyt** kto co zmienił i kiedy?
6. Jakie są zasady retencji danych (usuwanie po okresie przechowywania)?
7. Jak realizowane są prawa osób: wgląd, sprostowanie, przeniesienie danych?
8. Jak wygląda polityka backupów i kto za nią odpowiada (klient czy my)?
9. Środowiska: czy jest baza testowa/szkoleniowa i jak często odświeżana z produkcji?
   Czy dane są wtedy anonimizowane?

**Czerwone flagi (→ C):** prawa do danych w układzie macierzowym, audyt zmian poza
standardowym logiem, anonimizacja bazy testowej.

---

## 18. Organizacja projektu

1. Kto po stronie klienta jest kierownikiem projektu i kto decyduje o zakresie?
2. Kto jest **kluczowym użytkownikiem** kadr, płac i czasu pracy (osoby, które będą testować)?
3. Ile czasu klient realnie przeznaczy na testy? (najczęstsza przyczyna poślizgu)
4. Jak wygląda ścieżka zgłaszania uwag i kto je priorytetyzuje?
5. Kiedy i w jakim zakresie potrzebne są szkolenia? Ilu użytkowników, w jakich grupach?
6. Jakie są kamienie milowe i jaki jest warunek odbioru każdego etapu?
7. Jak dokumentujemy ustalenia – kto pisze protokół z warsztatu i kto go akceptuje?
8. Co zostaje na etap 2 (po starcie)? Świadome odłożenie tematów jest lepsze
   niż wciśnięcie wszystkiego przed start.

---

## 19. Podsumowanie analizy – co musi być na wyjściu

Dokument wyjściowy z analizy powinien zawierać:

1. **Rejestr ustaleń** – każda decyzja z datą i osobą decydującą.
2. **Rejestr customizacji (C)** – lista z opisem funkcjonalnym, szacunkiem pracochłonności
   i oznaczeniem, czy blokuje start.
3. **Rejestr otwartych pytań** – z terminem odpowiedzi i właścicielem.
4. **Zakres migracji** – co, skąd, kto przygotowuje, w jakim formacie, do kiedy.
5. **Listę integracji** z osobami kontaktowymi.
6. **Listę rzeczy poza zakresem** – wypisaną wprost, żeby nie wróciła jako
   „przecież o tym mówiliśmy".
7. **Harmonogram** z datami testów i startu produkcyjnego.

---

## 20. Zbiorcza lista sygnałów ostrzegawczych

Zdania, które na warsztacie oznaczają, że temat najprawdopodobniej jest
customizacją – warto reagować na nie od razu, dopytując o szczegół:

| Klient mówi | Co to zwykle znaczy |
|---|---|
| „U nas to działa inaczej niż w kodeksie, ale korzystniej" | własny algorytm płacowy lub reguła czasu pracy |
| „Mamy to w Excelu, wystarczy przenieść" | logika niespisana nigdzie; trzeba ją odtworzyć z formuł |
| „To liczy nasza pani z kadr, ona wie jak" | wiedza nieudokumentowana; ryzyko, że wyjdzie po starcie |
| „Chcemy dokładnie taki sam wydruk jak teraz" | własny raport/layout |
| „System ma pilnować, żeby nie dało się wpisać…" | weryfikator = kod |
| „Chcemy, żeby to się liczyło samo z danych z…" | integracja + logika |
| „Nasz zarząd dostaje takie zestawienie co miesiąc" | własny raport, często w XLSX |
| „Potrzebujemy tego wstecz, za poprzednie lata" | rozszerzony zakres migracji |
| „Kierownik nie może widzieć wynagrodzeń, ale musi widzieć…" | prawa do danych, nie do funkcji |
| „To się zmienia co miesiąc" | parametr w konfiguracji albo cecha, nie wartość zaszyta w kodzie |

---

## 21. Uwagi metodyczne dla konsultanta

- **Prezentuj standard, zanim zapytasz o wymagania.** Klient opisujący proces „jak dziś"
  nieświadomie zamawia customizację. Pokazanie standardowego mechanizmu enovy często
  kończy temat.
- **Nie obiecuj na warsztacie.** Na każde „a czy da się…" odpowiadaj „da się,
  sprawdzę jakim kosztem" – i zapisuj.
- **Dopytuj o wyjątki.** Główny scenariusz zwykle jest prosty; koszt wdrożenia robią
  wyjątki (część etatu, zmiana w trakcie miesiąca, zatrudnienie od 15., zwolnienie
  z wyrównaniem, korekta po wypłacie).
- **Proś o liczby, nie o opisy.** „Ilu pracowników tego dotyczy?" często przesuwa temat
  z C na X – nie warto kodować obsługi trzech przypadków w roku.
- **Każdą regułę licz na przykładzie.** Weź konkretnego pracownika z listy płac klienta
  i policz z nim wynik ręcznie. To najszybszy sposób wykrycia, że regulamin mówi
  co innego niż praktyka.
- **Rozdziel „musi być na start" od „byłoby dobrze".** Zapisz to w dokumencie,
  bo przy poślizgu to będzie pierwsze pytanie.
- **Zapisuj także to, czego nie robimy.** Lista wykluczeń chroni obie strony.
