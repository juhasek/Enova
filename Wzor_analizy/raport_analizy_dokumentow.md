# Raport z analizy dokumentów źródłowych – wzór analizy przedwdrożeniowej KPCP

Data: 2026-10-20. Źródła: `Pobrane\Analiza` (oryginały nienaruszone, kopie robocze
w `Wzor_analizy\_robocze\zrodla`, poza gitem). Zrzuty struktury i tekstu wykonane
narzędziem .NET (czyste OpenXML) – `Wzor_analizy\_robocze\zrzuty`.

Oznaczenia klientów (w repo nie używamy nazw):

| Kod | Rok | Dokumenty | Uwagi |
|---|---|---|---|
| **S** | 2025 | Dokument główny v1.4 + Zał. 1 Kadry v1.5, Zał. 2 Czas pracy v1.6, Zał. 3 Płace v1.6, Zał. 5 Pulpity v1.5 | **wzorcowy**; Zał. 4 (podzielnik/księgowanie) poza zakresem użytkownika; Zał. 6–9 (raporty, integracje, migracja, workflow) wymienione w wykazie, ale nie dostarczone |
| **A** | 2024 | Jeden dokument „Analiza wdrożeniowa” v1.4 (wszystko w rozdziale 3) | ma role/licencje w tabelach, asystę, metodykę projektu |
| **B** | 2022 | Dokument główny v4 + Zał. 01 B2B, Zał. 02 Oddelegowani, Zał. 03 Rezerwy | ma wymagania techniczne, licencyjne, „poza zakresem”, kilka baz |

---

## 1. Struktura dokumentów

### 1.1 Dokument główny S (wzorzec)

```
[strona tytułowa: logo, „ANALIZA PRZEDWDROŻENIOWA”, „DLA <klient>”, „Obszar: Kadry i Płace”, grafika, tabela metryki, klauzula poufności]
[Spis treści – pole TOC \o "1-3"]
1. Wprowadzenie
   1.1 Zakres projektu
   1.2 Cel dokumentu
   1.3 Zakres dokumentu
   1.4 Streszczenie dla zarządu i kierownictwa projektu
   1.5 Wymagania
2. Ogólna charakterystyka
   2.1 Otoczenie biznesowe
3. Realizacja wymagań Zamawiającego
   3.1 Dane konfiguracyjne spółki
   3.2 Konfiguracja dostępów, lista użytkowników
   3.3 Struktura organizacyjna
       3.3.1 Struktura wydziałowa (formalno-prawna)
       3.3.2 Struktura podległościowa
   3.4 Konfiguracja kadr                      → odesłanie do Zał. 1
   3.5 Kalendarze i ewidencja czasu pracy     → odesłanie do Zał. 2
   3.6 Konfiguracja płac, istotne zagadnienia → odesłanie do Zał. 3
   3.7 Podzielnik kosztów                     → odesłanie do Zał. 4
   3.8 eDeklaracje PIT-11                     (pełny opis standardu + 3 rysunki)
   3.9 Pulpity Pracownika i Kierownika        → odesłanie do Zał. 5
   3.10 Raportowanie HR                       → Zał. 6
        3.10.1 Informacje ogólne              (standard: wydruki / raporty / widoki)
   3.11 Integracje                            → Zał. 7
   3.12 Migracja danych kadrowo-płacowych     → Zał. 8
   3.13 Workflow – dedykowane wnioski         → Zał. 9
4. Dokumentacja systemu
5. Pozostałe wymagania
6. Postanowienia końcowe
7. Wykaz załączników
8. Wykaz rysunków (+ na końcu linie podpisów „Akceptacja dokumentu”)
9. Kontakt
```

### 1.2 Załączniki S

| Zał. | H1 | H2 |
|---|---|---|
| 1 Kadry | 1. Kadry | 1.1 Charakterystyka kadr (zakładki kartoteki: Ogólne, Adresy, Dane podatkowe, Dane ubezpieczeniowe, Historia zatrudnienia, Rachunki bankowe, Rodzina, Wykształcenie, Nagrody i kary, Etat/Ogólne, Etat/Rozwiązanie umowy, Etat/Umowa o pracę, PPK, Deklaracje zgłoszeniowe ZUS; dane z integracji; wieloetatowość; zapis zmian) · 1.2 Numeracja pracowników · 1.3 Szkolenia BHP · 1.4 Badania lekarskie · 1.5 Powiadomienia · 1.6 Definicje stanowisk · 1.7 Umowy cywilnoprawne · 1.8 Ewidencja współpracowników B2B · 1.9 Oddelegowani za granicę · 1.10 Umowa o zakazie konkurencji · 1.11 Upoważnienia RODO · 1.12 Uprawnienia · 1.13 Praca zdalna · 1.14 Nieobecności oraz limity · 1.15 Elektroniczna dokumentacja pracownicza · 1.16 Integracja z Płatnikiem · 1.17 Szczególne warunki pracy · 1.18 Szkolenia · 1.19 Wypowiedzenia zmieniające |
| | 2. Kryteria odbioru · 3. Wykaz tabel i rysunków · 4. Kontakt | |
| 2 Czas pracy | 1. Kalendarze i ewidencja czasu pracy | **brak H2** – podział tylko pogrubionymi akapitami: Nadgodziny, Harmonogramowanie (×2), Czas przepracowany, Uzupełnianie czasu faktycznie przepracowanego |
| | 2. Kryteria odbioru · 3. Wykaz tabel i rysunków · 4. Kontakt | |
| 3 Płace | 1. Konfiguracja płac, istotne zagadnienia | 1.1 Zagadnienia ogólne · 1.2 Listy płac · 1.3 PPK · 1.4 Elementy płacowe · 1.5 ZFŚS (+ podrozdziały per spółka) · 1.8 PKZP · 1.9 Rezerwa urlopowa · 1.10 Benefity · 1.11 Program akcji pracowniczych · 1.12 Przelewy bankowe · 1.13 Import zwolnień z PUE · 1.14 Obsługa ujemnych podstaw ZUS |
| | 2. Kryteria odbioru · 3. Wykaz tabel i rysunków · 4. Kontakt | |
| 5 Pulpity | 1. Pulpity Pracownika i Kierownika | 1.1 Informacje i konfiguracja ogólna (logowanie, role) · 1.2 Pulpit Pracownika i Kierownika (widoczność, wypłaty, dashboard, powiadomienia) · 1.3 Wnioski · 1.4 Zastępstwa · 1.5 Zasobnik dokumentów · 1.6 Serwer SMTP |
| | 2. Kryteria odbioru · 3. Wykaz tabel i rysunków · 4. Kontakt | |

### 1.3 Macierz występowania sekcji (S / A / B)

✔ – sekcja jest, ↗ – jest jako odesłanie do załącznika, – – brak.

| Obszar / sekcja | S | A | B | Wniosek dla wzoru |
|---|---|---|---|---|
| Wprowadzenie 1.1–1.5 | ✔ | ✔ | ✔ | STANDARD |
| Otoczenie biznesowe | ✔ | ✔ | ✔ | DEDYKOWANE (treść), sekcja stała |
| Wdrożenie w kilku bazach | – | – | ✔ | OPCJONALNA |
| Dane konfiguracyjne spółki | ✔ | ✔ | ✔ | stała, treść = plik specyfikacji |
| Dostępy, role, operatorzy | ✔ | ✔ (tabele ról, operatorów, licencji) | ✔ (tabela operatorów) | stała; tabele z A |
| Struktura wydziałowa / podległościowa | ✔ | ✔ | ✔ | STANDARD + pola dedykowane |
| Dodatkowe pola (cechy) – zestawienie | – | – | ✔ | OPCJONALNA (dobra praktyka) |
| Charakterystyka kadr / kartoteka | ✔ (Zał.1) | ✔ | ✔ | STANDARD |
| Numeracja pracowników | ✔ | ✔ | ✔ | stała, treść dedykowana |
| Szkolenia BHP, badania lekarskie | ✔ | ✔ | ✔ | STANDARD + listy klienta |
| Uprawnienia zawodowe | ✔ | – | ✔ | OPCJONALNA |
| Powiadomienia | ✔ | ✔ | ✔ | STANDARD (tabela) |
| Definicje stanowisk | ✔ | ✔ | ✔ | STANDARD + pola dedykowane |
| Umowy cywilnoprawne | ✔ | ✔ | ✔ | STANDARD |
| B2B | ✔ | ✔ (nie dotyczy) | ✔ (osobny zał.) | OPCJONALNA |
| Oddelegowani za granicę | ✔ | ✔ (nie dotyczy) | ✔ (osobny zał.) | OPCJONALNA |
| Zakaz konkurencji, RODO, praca zdalna, warunki szczególne, szkolenia, wypowiedzenia zmieniające | ✔ | – | – | OPCJONALNE |
| Kalendarze i czas pracy | ✔ (Zał.2) | ✔ | ✔ | stała; treść w większości DEDYKOWANA |
| Weryfikatory planu | ✔ | ✔ | – | OPCJONALNA (rozwiązanie wykonawcy) |
| Zatwierdzanie czasu pracy | ✔ (dokument aktualizacji) | ✔ (blokada) | ✔ | OPCJONALNA |
| Nieobecności i limity (tabela 56 definicji) | ✔ | ✔ | ✔ (bez tabeli) | STANDARD + dedykowane nieobecności |
| E-dokumentacja, Płatnik | ✔ | ✔ | ✔ | STANDARD |
| Płace – zagadnienia ogólne, listy, PPK, elementy | ✔ (Zał.3) | ✔ | ✔ | STANDARD + charakterystyka klienta |
| ZFŚS | ✔ | ✔ | ✔ | STANDARD + zasady klienta |
| PKZP | ✔ | – | ✔ | OPCJONALNA |
| Rezerwa urlopowa | ✔ | ✔ | ✔ (zał.) | STANDARD (mechanizm) + algorytm klienta |
| Rezerwa na nadgodziny | – | – | ✔ | OPCJONALNA |
| Benefity, program akcji pracowniczych | ✔ | – | – | OPCJONALNE |
| Przelewy, PUE, ujemne podstawy ZUS | ✔ | ✔ | – | STANDARD + decyzja klienta |
| Podzielnik kosztów / Księgowania | ↗ | ↗ | ↗ | **poza zakresem** – tylko odesłanie |
| eDeklaracje PIT-11 | ✔ | ✔ | ✔ | STANDARD (cały tekst) |
| Pulpity (konfiguracja, role, widoczność, wnioski, zastępstwa, zasobnik, SMTP) | ✔ (Zał.5) | ✔ | ✔ | STANDARD + ustalenia |
| 2FA, dashboard, planowanie urlopów | ✔/✔ | ✔ | – | OPCJONALNE (produkty wykonawcy) |
| Wnioski modyfikowane / dedykowane (tabela) | ✔ | ✔ | ✔ | stała, treść DEDYKOWANA |
| Raportowanie | ↗ + standard | ↗ | ✔ | STANDARD + lista raportów |
| Integracje | ↗ | – | ↗ | OPCJONALNA |
| Migracja danych | ↗ | ↗ | ↗ | stała, treść dedykowana |
| Asysta wdrożeniowa | – | ✔ | – | OPCJONALNA (STANDARD tekst) |
| Opis realizacji projektu (metodyka, role, komunikacja, zmiany, ryzyka) | – | ✔ | ✔ | OPCJONALNA (STANDARD tekst) |
| Wymagania licencyjne | – | ✔ (tabele) | ↗ | stała, treść dedykowana |
| Wymagania techniczne, dostępy dla konsultantów | – | ✔ (dostępy) | ✔ | STANDARD |
| Rozwiązania poza zakresem wdrożenia | – | – | ✔ | stała (dobra praktyka) |
| Dokumentacja systemu, Pozostałe wymagania, Postanowienia końcowe | ✔ | ✔ | ✔ | STANDARD |
| Kryteria odbioru (w załącznikach) | ✔ | – | – | STANDARD |
| Wykaz załączników / tabel i rysunków / Kontakt | ✔ | ✔ | ✔ | STANDARD |
| **Historia zmian dokumentu** | – | – | – | **brak we wszystkich** – do dodania |
| **Tabela akceptacji** | linie podpisów | – | – | do dodania jako tabela |
| **Pytania otwarte, ryzyka, harmonogram** | – | (ryzyka – procedura) | (ryzyka – procedura) | do dodania |

---

## 2. Formatowanie

### 2.1 Strona i sekcje (identyczne w S, A, B-główny)

| Parametr | Wartość |
|---|---|
| Rozmiar | A4 (11907 × 16839 twip), pionowo |
| Sekcja 1 (strona tytułowa + spis) | marginesy: góra 1080 (1,9 cm), prawy/lewy 1080 (1,9 cm), dół 851 (1,5 cm); nagłówek 680–720, stopka 720; „inna pierwsza strona” |
| Sekcje 2–3 (treść) | marginesy 1080 / 1440 (2,54 cm) / 1080 / 1440; nagłówek/stopka 720 |
| Nagłówek | logo wykonawcy (image1, PNG) wyrównane do prawej; w S dodatkowo **etykieta klasyfikacji „General / Ogólne”** (kształt Microsoft Purview z tenanta klienta) – **do usunięcia we wzorze** |
| Stopka | akapit 1 wyrównany do prawej: `Strona | {PAGE}` (styl Stopka: Century Gothic 8 pt, KAPITALIKI, kolor 418AB3); akapit 2: „system enova365” (kolor 5E5E5E) |
| Wersja w nagłówku/stopce | brak – wersja tylko w tabeli metryki |
| Załączniki B (2022) | inny układ: marginesy 1417 (2,5 cm), dokument na starszym szablonie – nie bierzemy |

### 2.2 Strona tytułowa (S)

1. Akapit styl **Logo** (odstęp przed 30 pt) – logo wykonawcy.
2. „ANALIZA PRZEDWDROŻENIOWA” / „ZAŁĄCZNIK NR X DO ANALIZY PRZEDWDROŻENIOWEJ” – wyśrodkowane, kolor 306785, ok. 24 pt.
3. „DLA <KLIENT>” – styl Tytuł lub bezpośrednio: 24 pt, kolor 306785, odstępy znaków −0,35 pt.
4. „Obszar: Kadry i Płace” – 22 pt, kolor 306785, wyśrodkowane (tylko dokument główny).
5. Grafika tytułowa (image2 – ilustracja enova365).
6. **Tabela metryki** 6×2 (szer. 9923 dxa, kolumny 2547 + 7376, bez obramowań, cieniowanie **F2F2F2** wszystkich komórek, styl „Kopia tekstu podstawowego” 10 pt czarny, interlinia 1,5): Data utworzenia / Wersja / Autor / Ostatnia modyfikacja / Status dokumentu / Ważna do. (B ma 8 wierszy: + „Tytuł”.)
7. Klauzula poufności (Normal): „Dokument został opracowany przez <wykonawca> wyłącznie dla <klient>. Nie może być reprodukowany…”.
8. „Spis treści” (styl Nagłówek spisu treści) + pole `TOC \o "1-3" \h \z \u`.

### 2.3 Style (motyw: Calibri / Calibri Light w motywie, faktycznie wszędzie **Century Gothic** z domyślnych ustawień dokumentu)

| Styl (nazwa wewn.) | Czcionka / rozmiar | Kolor | Akapit | Uwagi |
|---|---|---|---|---|
| Normal (Normalny) | Century Gothic 10,5 pt | auto | odstępy 0/0, w treści często interlinia 1,5 i wyjustowanie (bezpośrednio) | tekst podstawowy |
| Heading 1 | 18 pt | **306785** | przed 20 pt, po 2 pt, wcięcie 1,27 cm, **linia dolna 0,5 pt 418AB3**, z następnym | numeracja `1.` z listy powiązanej ze stylem |
| Heading 2 | 14 pt | 306785 | przed 8 pt, z następnym | numeracja `1.1.` – w dok. głównym przez listę, w załącznikach **wpisywana ręcznie** (błędy: „1.18 1.19”) |
| Heading 3 | 13 pt | 306785 | przed 4 pt | |
| Heading 4 | 11 pt | 306785 | przed 4 pt | nieużywany |
| Title (Tytuł) | 40 pt | 306785 | | |
| Subtitle | 15 pt | 404040 | po 12 pt | nieużywany |
| List Bullet (Lista punktowana) | 9 pt | **404040** | po 3 pt, interlinia 1,2 | punktor z numeracji stylu; często dodatkowo bezpośrednie numPr |
| List Paragraph | 10,5 pt | auto | wcięcie 1,27 cm | używany zamiennie z List Bullet (niekonsekwencja) |
| Caption (Legenda) | 10 pt **pogrubiony** | 404040 | | podpisy `Rys. {SEQ Rys.}` i `Tabela {SEQ Tabela}`; w A „Rysunek” |
| No Spacing (Bez odstępów) | 10,5 pt | auto | | blok „Kontakt” |
| Kopia tekstu podstawowego | 8 pt (w metryce nadpisane na 10 pt) | | | metryka |
| annotation text (Tekst komentarza) | 10 pt | | | używany jako „ramka uwagi/standard” w treści |
| TOC Heading / toc 1 / toc 2 / toc 3 | toc1 13 pt pogr., toc2 11 pt, toc3 10,5 pt | | kropki do prawego tabulatora | |
| table of figures | 10,5 pt | | | „Wykaz tabel i rysunków” w załącznikach (pole `TOC \c "Rys."`, `\c "Tabela"`) |
| Header / Footer | Stopka: 8 pt KAPITALIKI 418AB3 | | tabulatory 4536/9072 | |

Paleta: **306785** (granat – nagłówki, tytuł), **418AB3** (niebieski – linie, obramowania), **D7E7F0** (jasnoniebieski – nagłówki tabel), **F2F2F2** (szary – metryka), **404040** (tekst list i podpisów).

### 2.4 Tabele

| Typ | Układ | Formatowanie |
|---|---|---|
| Metryka | 2 kol. (25/75%) | brak obramowań, cieniowanie F2F2F2, 10 pt |
| Lista definicji (nieobecności 56 poz., listy płac) | Nazwa / Typ, Nazwa listy / Symbol | obramowania, wiersz nagłówka D7E7F0 pogrubiony 10 pt, treść 9–10 pt |
| Styl **Proposal Table** (S Zał.1, A) – role, operatorzy, powiadomienia | Lp / Definicja / Termin; Uprawnienie / Opis | obramowania 0,5 pt 418AB3, 1. wiersz D7E7F0 pogrubiony, 9 pt 404040 – **najlepszy kandydat na jednolity styl tabel** |
| Licencje (A) | Nazwa modułu / Ilość, tytuł tabeli w 1. wierszu | |
| Role projektowe (A, B) | 1 kolumna, nagłówki ciemne (525252, 4472C4, 538135) z białym tekstem | odstaje kolorystycznie |
| Komunikacja (A, B) | Element komunikacji / Opis | nagłówek 4472C4 biały |
| Widoczność w pulpitach (S Zał.5) | macierz: wiersz = grupa, kolumny = spółki | |
| Wnioski modyfikowane (B) | Wniosek / Kto akceptuje | nagłówek DDEBF7 |
| Kontakt (stopka bloku) | 1×2/2×3 z ikonami social media | |

### 2.5 Pola i inne elementy

- Spis treści: pole TOC poziomy 1–3, hiperłącza.
- Podpisy: `SEQ Rys.` / `SEQ Tabela` (S), `SEQ Rysunek` (A) – niejednolite.
- Wykaz rysunków: w S głównym **wpisany ręcznie** listą, w załącznikach pole „table of figures”.
- Zaznaczenia (highlight): 14–185 w dokumentach – ślady roboczych uwag; we wzorze żółte zakreślenie zarezerwowane wyłącznie dla `[DO UZUPEŁNIENIA: …]`.
- Śledzenie zmian: w S pozostały nieprzyjęte rewizje (do 30 wstawień/usunięć) – we wzorze wszystkie przyjęte.
- Osadzone obiekty („w poniższym pliku”) – w A/B załączniki Excela osadzone w treści; w S odwołania tekstowe do plików w cudzysłowie.

---

## 3. Konwencje językowe

| Element | Jak zapisywane w źródłach | Przykład (uogólniony) |
|---|---|---|
| Opis standardu | czas teraźniejszy, „System enova365 umożliwia…”, „enova365 w standardzie oferuje…”, „Producent systemu zapewnia…” | „Enova365 w standardowej wersji stosownie do przepisów prawa pracy umożliwia obsługę limitów nieobecności dla: …” |
| Ustalenie z klientem | „Na etapie analizy (przedwdrożeniowej / wdrożeniowej) ustalono, że…”, „W trakcie spotkań analitycznych ustalono…”, „Podczas spotkań ustalono…” | „Na etapie analizy ustalono, że Klient nie będzie ewidencjonował…” |
| Rozwiązanie dedykowane | czas przyszły, „W ramach wdrożenia przygotowane zostaną dodatkowe pola: …” + **„(mechanizm N)”** – kolejny numer mechanizmu w obrębie załącznika | „…przygotowany zostanie mechanizm, który dokona automatycznego wpięcia… (mechanizm 1)” |
| Pole dodatkowe | „<Nazwa> – pole typu słownikowego / data / tekstowe / warunkowe true/false / referencja do listy pracowników” | „Data wręczenia – pole typu data” |
| Decyzja klienta | „Klient wybrał / wskazał…”, „Decyzją Klienta…”, „Klient na etapie akceptacji analizy wybrał rozwiązanie…” | |
| Zobowiązanie klienta | „Klient na etapie wdrożenia przekaże / przedstawi / wskaże…” | „Klient na etapie wdrożenia przekaże aktualną listę definicji…” |
| Wymaganie nierealizowalne | „Po przeprowadzeniu wewnętrznej analizy … nie jest ona możliwa do realizacji… zaproponujemy producentowi… W przypadku odpowiedzi negatywnej…” | |
| Rozwiązanie alternatywne | „W ramach wdrożenia proponujemy dwa rozwiązania: …” + decyzja | |
| Odesłanie do pliku | „W pliku „<nazwa>.xlsx” załączonym do niniejszego dokumentu analizy…” | |
| Odesłanie do załącznika | „Założenia do … zostały opisane w <KOD>_ZAL_NR X_<NAZWA> jako załącznik analizy wdrożeniowej.” | |
| Uwaga / ostrzeżenie | „UWAGA: …” w akapicie Normal | znak „&” w nazwach wydziałów |
| Poza zakresem | „Rozwiązania te nie wchodzą w zakres wdrożenia. Będą one przedmiotem odrębnej wyceny…” | |
| Kryteria odbioru | „Kryterium odbioru poprawności konfiguracji … będzie przeprowadzenie testów przez Klienta z pozytywnym wynikiem. Testy … na podstawie scenariuszy testowych zawartych w pliku „Scenariusze testowe – <obszar>.xlsx”…” | |
| Terminologia | Klient / Zamawiający, Wykonawca, Operator (kadrowo-płacowy), Administrator kadr i płac, kartoteka, zakładka X/Y (np. Etat/Ogólne), definicja, cecha, widok, lista płac, element (składnik) płacowy, pulpit pracownika/kierownika, e-wniosek | |
| Pytania otwarte, ryzyka | **nie występują jako sekcja** – braki zapisywane w treści („Klient na etapie wdrożenia wskaże…”) | do wprowadzenia we wzorze |

---

## 4. Podział STANDARD vs DEDYKOWANE

### STANDARD (tekst do wzoru, ew. drobna edycja)

- Wprowadzenie (cel, zakres dokumentu – lista obszarów do skrócenia, streszczenie – lista celów), Wymagania (lista typowa).
- Konfiguracja dostępów – opis ról, praw obiektowych, licencji serwisowych wykonawcy, logowania.
- Struktura organizacyjna – dwie niezależne struktury, historyczność, zarządzanie strukturą (zasady zmiany nazwy, 80 znaków, „&”).
- Kadry – charakterystyka danych kartoteki, opis zakładek kartoteki, numeracja (opis mechanizmu), definicje stanowisk (opis), umowy cywilnoprawne (opis), tabela 56 standardowych definicji nieobecności, standardowe limity, naliczanie limitów, e-dokumentacja, Płatnik, wieloetatowość, zapis zmian.
- Czas pracy – konfiguracja ogólna (opcje Kadry i płace/Kalendarze/Czas pracy), definicje dni i kalendarzy, trzy kalendarze (definicja / Norma czasu pracy / Czas pracy), przypisanie kalendarza w Etat/Ogólne, rozliczanie nadgodzin okresowych (Miesięcy wstecz), wersjonowanie kalendarzy (opis).
- Płace – zagadnienia ogólne (lista funkcji), definicje list płac, automatyczne tworzenie list, rozliczanie KUP/ulgi przy kilku wypłatach, PPK (lista procesów), elementy płacowe (opis standardu, import dodatków i potrąceń, korekty, Lista nierozliczonych elementów), ZFŚS (moduł, limit podatkowy), PKZP (moduł), rezerwy (mechanizm planowanych list płac), przelewy (standard), import z PUE (3 opcje nadpisywania), ujemna podstawa ZUS (3 warianty).
- eDeklaracje PIT-11, udostępnianie PIT/IMIR w pulpicie.
- Pulpity – sposoby logowania, opis pulpitu pracownika/kierownika, publikacja wypłat, struktura podległościowa jako warunek widoczności, standard wniosków (domyślnie zablokowane, jednostopniowa akceptacja, maile), zastępstwa (licencje), zasobnik, SMTP (lista parametrów).
- Raportowanie – podział na wydruki / raporty / widoki.
- Asysta wdrożeniowa, opis realizacji projektu (metodyka, role, komunikacja, zmiany, ryzyka), wymagania techniczne (zakres po stronie klienta, dostępy dla konsultantów), dokumentacja systemu, pozostałe wymagania, postanowienia końcowe, kryteria odbioru.

### DEDYKOWANE (do pól `[DO UZUPEŁNIENIA]`)

- Nazwa klienta i spółek, liczba pracowników/zleceniobiorców, obecne systemy, konfiguracja wielooddziałowa/kilka baz, integracje wiodące.
- Lista operatorów i ról, liczba i rodzaj licencji.
- Pliki: specyfikacja konfiguracyjna, struktura podległości, operatorzy, uprawnienia, wnioski, role w pulpitach, scenariusze.
- Pola dodatkowe (cechy) na kartotece, etacie, umowach, badaniach, szkoleniach, rozwiązaniu umowy, definicjach stanowisk i jednostek; słowniki wartości.
- Listy definicji klienta: szkolenia BHP, badania, czynniki zagrożenia, kary/nagrody, uprawnienia, upoważnienia RODO, warunki szczególne, nieobecności dedykowane i ich limity, limity pracy zdalnej.
- Mechanizmy (automaty, weryfikatory, blokady, integracje) z numeracją „mechanizm N”.
- Systemy i okresy rozliczeniowe, godziny pracy, pora nocna, kalendarze wzorcowe, zasady harmonogramowania i zatwierdzania, wypłata nadgodzin.
- Charakterystyka płacowa (terminy wypłat, podział list, nazewnictwo list), algorytmy składników dedykowanych, zasady ZFŚS, benefity, programy akcji, rezerwy (algorytm), konta bankowe, decyzje PUE / ujemna podstawa.
- Widoczność danych w pulpitach, role pulpitowe, wnioski modyfikowane i dedykowane, powiadomienia.
- Raporty i wydruki klienta, zakres migracji, harmonogram.

---

## 5. Niespójności do ujednolicenia we wzorze

1. Numeracja nagłówków: H1 ze stylu (lista 1-poziomowa), H2 w dokumencie głównym przez osobną listę, w załącznikach wpisywana ręcznie (z błędami). **Wzór: jedna lista wielopoziomowa `1.` / `1.1.` / `1.1.1.` / `1.1.1.1.` powiązana ze stylami Nagłówek 1–4.**
2. Zał. 2 Czas pracy nie ma nagłówków H2 – **wzór: podrozdziały H2.**
3. Punktory: List Bullet i List Paragraph używane zamiennie, różne rozmiary (9 vs 10,5 pt) – **wzór: List Bullet dla wyliczeń, List Paragraph tylko dla wcięć.**
4. Podpisy: „Rys.” vs „Rysunek”, ręczne numery – **wzór: „Rys. {SEQ Rys.}” i „Tabela {SEQ Tabela}”.**
5. „enova365” / „Enova365” / „Enova” / „ENOVA” – **wzór: zawsze „enova365”** (wielka litera tylko na początku zdania – „Enova365”, jak w źródłach).
6. Tabela akceptacji zapisana jako punkty listy w sekcji „Wykaz rysunków” – **wzór: osobny rozdział z tabelą.**
7. Brak historii zmian – **wzór: tabela historii zmian po metryce.**
8. Etykieta klasyfikacji „General / Ogólne” w nagłówku – **usunąć.**
9. Kontakt zawiera imiona, nazwiska, telefony i adres wykonawcy – **wzór: pola `[DO UZUPEŁNIENIA]`.**

---

## 6. Proponowany zestaw wzorów (zgodnie z decyzją: osobny dokument na każdy element analizy)

| Plik wzoru | Na bazie | Uzupełnienia z innych analiz |
|---|---|---|
| `Wzor_00_Analiza_przedwdrozeniowa_KPCP.docx` | S dokument główny | A: tabele ról/operatorów/licencji, asysta, metodyka projektu, dostępy; B: kilka baz, wymagania techniczne, poza zakresem; nowe: historia zmian, pytania otwarte i ryzyka, harmonogram i odpowiedzialności, akceptacja |
| `Wzor_ZAL_01_Kadry.docx` | S Zał. 1 | A/B: cechy (zestawienie), uprawnienia zawodowe, powiadomienia (pełna lista), B2B (B Zał. 01), oddelegowani (B Zał. 02 – część kadrowa) |
| `Wzor_ZAL_02_Czas_pracy.docx` | S Zał. 2 (z nadaną strukturą H2) | A: systemy czasu pracy, definicje dni, serie dni, weryfikatory, rozliczanie nadgodzin okresowych; B: zatwierdzanie czasu pracy, ewidencja czasu na projektach; nowe: RCP / import czasu (opcjonalna) |
| `Wzor_ZAL_03_Place.docx` | S Zał. 3 | A: listy płac (tabela definicji), elementy (przykłady), ZFŚS (pożyczki); B: rezerwy – warianty + rezerwa na nadgodziny, PKZP; nowe: umowy cywilnoprawne, PFRON, PIT/ZUS (krótko, standard) |
| `Wzor_ZAL_05_Pulpity.docx` | S Zał. 5 | A: 2FA, role pulpitowe, widoczność dodatków, blokada czasu pracy; B: wnioski modyfikowane (tabela), przełożony wskazany, B2B w pulpitach |

Załącznik nr 4 (podzielnik kosztów / księgowanie) – poza zakresem: w dokumencie głównym zostaje sekcja-odesłanie oznaczona jako opcjonalna.

## 7. Pytania do użytkownika

Zebrane w czacie przy akceptacji raportu (numeracja załączników 6–9, nazwa wykonawcy i logo, zrzuty ekranu z danymi demo, sekcje projektowe).
