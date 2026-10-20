# STYLE GUIDE – analiza przedwdrożeniowa KPCP (enova365)

Reguły pozwalające odtworzyć dokument analizy 1:1. Dotyczą wzorów w `Wzor_analizy/`
i wszystkich analiz klientów w `Klienci/<Nazwa>/`. Podstawa: analiza wzorcowa z 2025 r.
(dokument główny + załączniki), uzupełniona o elementy z analiz 2022 i 2024 –
szczegóły w `raport_analizy_dokumentow.md`.

**Zasada nadrzędna:** formatujemy wyłącznie stylami zdefiniowanymi we wzorze. Nie zmieniamy
ręcznie czcionki, rozmiaru, koloru, odstępów ani numeracji. Jeśli czegoś brakuje,
poprawiamy styl lub generator, a nie pojedynczy akapit.

---

## 1. Zestaw dokumentów

| Plik wzoru | Dokument klienta | Zawartość |
|---|---|---|
| `Wzor_Analiza_przedwdrozeniowa_KPCP.docx` | `Analiza_<Nazwa>_vX.Y.docx` | dokument główny: wprowadzenie, charakterystyka, realizacja wymagań (dostępy, licencje, struktura, PIT-11, raporty, integracje, migracja), organizacja projektu, pytania otwarte i ryzyka, harmonogram, akceptacja |
| `Wzor_ZAL_01_Kadry_KPCP.docx` | `Analiza_<Nazwa>_ZAL_01_Kadry_vX.Y.docx` | Załącznik nr 1 – Kadry |
| `Wzor_ZAL_02_Czas_pracy_KPCP.docx` | `Analiza_<Nazwa>_ZAL_02_Czas_pracy_vX.Y.docx` | Załącznik nr 2 – Kalendarze i ewidencja czasu pracy |
| `Wzor_ZAL_03_Place_KPCP.docx` | `Analiza_<Nazwa>_ZAL_03_Place_vX.Y.docx` | Załącznik nr 3 – Płace |
| – | (opcjonalnie) `…_ZAL_04_…` | Załącznik nr 4 – Podzielnik kosztów/księgowanie: **poza zakresem Kadr i Płac**, w dokumencie głównym zostaje tylko odesłanie |
| `Wzor_ZAL_05_Pulpity_KPCP.docx` | `Analiza_<Nazwa>_ZAL_05_Pulpity_vX.Y.docx` | Załącznik nr 5 – Pulpity Pracownika i Kierownika |
| `Kwestionariusz_spotkan_KPCP.docx` | `Klienci/<Nazwa>/Notatki/Kwestionariusz_<Nazwa>.docx` | pytania na spotkania z mapowaniem na sekcje |

Numery załączników są stałe (1 Kadry, 2 Czas pracy, 3 Płace, 4 Podzielnik, 5 Pulpity) – także wtedy,
gdy załącznika nr 4 nie ma. Raporty, integracje, migracja i workflow są rozdziałami dokumentu głównego,
a nie osobnymi załącznikami.

## 2. Strona i sekcje

| Parametr | Wartość |
|---|---|
| Papier | A4 pionowo (11907 × 16839 twip) |
| Sekcja 1 – strona tytułowa, historia zmian, spis treści | marginesy: góra 1,9 cm, dół 1,5 cm, lewy i prawy 1,9 cm; nagłówek 1,2 cm, stopka 1,27 cm; **inna pierwsza strona** (bez logo w nagłówku) |
| Sekcja 2 – treść | marginesy: góra i dół 1,9 cm, lewy i prawy 2,54 cm; nagłówek i stopka 1,27 cm; zaczyna się od nowej strony |
| Nagłówek | logo wykonawcy wyrównane do prawej (szer. ok. 3,5 cm); bez etykiet klasyfikacji (np. „General / Ogólne”) |
| Stopka | akapit 1 (styl Stopka, do prawej): `Strona | {PAGE}`; akapit 2: „system enova365” (kolor 5E5E5E, 9 pt) |
| Wersja dokumentu | tylko w tabeli metryki i w historii zmian – nie w nagłówku ani stopce |

## 3. Kolory i czcionka

- Czcionka całego dokumentu: **Century Gothic** (ustawienie domyślne dokumentu; motyw zawiera Calibri, ale nie jest używany).
- Paleta: **306785** – nagłówki, tytuł; **418AB3** – linia pod Nagłówkiem 1, obramowania tabel, punktory, stopka; **D7E7F0** – wiersz nagłówka tabeli; **F2F2F2** – tło metryki i znacznika sekcji opcjonalnej; **404040** – tekst list, tabel i podpisów; **FFFF00** – wyłącznie pola `[DO UZUPEŁNIENIA]`.

## 4. Style

| Styl (nazwa w Wordzie) | Zastosowanie | Czcionka | Akapit |
|---|---|---|---|
| Normalny | cały tekst ciągły | 10 pt, czarny | interlinia 1,5, wyjustowany, odstępy 0 |
| Nagłówek 1 | rozdział (1.) | 18 pt, 306785 | przed 20 pt, po 6 pt, linia dolna 0,5 pt 418AB3, numeracja `1.` |
| Nagłówek 2 | podrozdział (1.1.) | 14 pt, 306785 | przed 12 pt, po 4 pt, numeracja `1.1.` |
| Nagłówek 3 | punkt (1.1.1.) | 13 pt, 306785 | przed 10 pt, po 3 pt, numeracja `1.1.1.` |
| Nagłówek 4 | wyjątkowo (1.1.1.1.) | 11 pt, 306785 | nie wchodzi do spisu treści – unikać |
| Lista punktowana | każde wyliczenie | 10 pt, 404040 | punktor ● w kolorze 418AB3, interlinia 1,2, po 3 pt; poziom 2 – półpauza |
| Tekst tabeli | treść komórek tabel | 9 pt, 404040 | interlinia 1,0, odstęp 2 pt, do lewej |
| Legenda | podpis tabeli i rysunku | 10 pt pogrubiony, 404040 | pod tabelą lub rysunkiem |
| Tekst komentarza | ramka „standard enova365 / uwaga” w treści | 10 pt | – |
| Sekcja opcjonalna | znacznik `[SEKCJA OPCJONALNA – usuń, jeśli nie dotyczy]` | 9 pt, pogrubiona kursywa, 7F7F7F | tło F2F2F2 |
| Pole do uzupełnienia (znakowy) | `[DO UZUPEŁNIENIA: …]` | jak otoczenie | żółte zakreślenie |
| Tytuł analizy | „ANALIZA PRZEDWDROŻENIOWA” / „ZAŁĄCZNIK NR X…”, „DLA …” | 24 pt, 306785 | wyśrodkowany |
| Obszar analizy | „Obszar: …” na stronie tytułowej | 20 pt, 306785 | wyśrodkowany |
| Metryka dokumentu | tabela metryki | 10 pt, czarny | do lewej |
| Klauzula poufności | zdanie o prawach do dokumentu | 8 pt, kursywa, 595959 | przed 12 pt |
| Nagłówek spisu treści | „Historia zmian dokumentu”, „Spis treści” | jak Nagłówek 1, bez numeru | nie wchodzi do spisu |
| Spis treści 1–3 | wpisy spisu | 11 pt pogrubiony / 10 pt | tabulator z kropkami do prawej |
| Spis ilustracji | wykaz tabel i rysunków | 10 pt | tabulator z kropkami |
| Bez odstępów | blok „Kontakt” | 10,5 pt | – |

Niedozwolone: ręczne numery w treści nagłówka (np. „1.3 Szkolenia BHP”), wpisywane ręcznie punktory „•”,
akapity w stylu listy bez punktora jako zwykły tekst, pogrubianie całych akapitów zamiast nagłówka,
ręczne zakreślanie inne niż pola `[DO UZUPEŁNIENIA]`.

## 5. Numeracja i spisy

- Jedna lista wielopoziomowa powiązana ze stylami Nagłówek 1–4: `1.` / `1.1.` / `1.1.1.` / `1.1.1.1.`.
- Spis treści: pole `TOC \o "1-3" \h \z \u` (poziomy 1–3). Po każdej zmianie: Ctrl+A, F9 → „Aktualizuj cały spis”.
  Dokument przy otwarciu pyta o aktualizację pól – należy potwierdzić.
- Podpisy: `Rys. {SEQ Rys.} Opis` pod rysunkiem, `Tabela {SEQ Tabela} Opis` pod tabelą (wstawianie: Odwołania → Wstaw podpis).
- Rozdział „Wykaz tabel i rysunków”: dwa pola spisu ilustracji (`TOC \h \z \c "Tabela"` i `TOC \h \z \c "Rys."`).
- Odesłania między dokumentami podajemy słownie z numerem sekcji: „opisane w załączniku nr 2 – Kalendarze i ewidencja czasu pracy”.

## 6. Elementy stałe dokumentu

1. **Strona tytułowa**: logo wykonawcy → „ANALIZA PRZEDWDROŻENIOWA” lub „ZAŁĄCZNIK NR X DO ANALIZY PRZEDWDROŻENIOWEJ” → „DLA <nazwa klienta>” → „Obszar: …” → grafika tytułowa → tabela metryki → klauzula poufności.
2. **Tabela metryki** (2 kolumny 2500/7247 twip, tło F2F2F2, bez obramowań): Data utworzenia / Wersja / Autor / Ostatnia modyfikacja / Status dokumentu / Ważna do. Statusy: `Roboczy`, `Do akceptacji Klienta`, `Obustronnie zaakceptowany – FINAL`.
3. **Historia zmian dokumentu** (strona 2): Wersja | Data | Autor | Opis zmian – nowy wiersz przy każdej wersji.
4. **Spis treści**.
5. Treść.
6. W dokumencie głównym: **Pytania otwarte i ryzyka**, **Harmonogram i odpowiedzialności**, **Dokumentacja systemu**, **Pozostałe wymagania**, **Postanowienia końcowe**, **Wykaz załączników**, **Wykaz tabel i rysunków**, **Akceptacja dokumentu** (tabela Zamawiający/Wykonawca), **Kontakt**.
7. W załącznikach: **Pytania otwarte – <obszar>**, **Kryteria odbioru**, **Wykaz tabel i rysunków**, **Kontakt**.

## 7. Tabele

- Styl tabeli: **Proposal Table** – obramowanie 0,5 pt 418AB3, pierwszy wiersz D7E7F0 pogrubiony, powtarzany na każdej stronie; wiersze nie dzielą się między stronami.
- Szerokość: pełna szerokość tekstu (9027 twip w treści), kolumny proporcjonalnie; tekst komórek w stylu Tekst tabeli.
- Pod każdą tabelą podpis „Tabela N …” (wyjątek: metryka i historia zmian).
- Typowe układy:

| Tabela | Kolumny |
|---|---|
| Role | Rola \| Opis |
| Operatorzy | Lp. \| Operator \| Login \| Rola i zakres dostępu |
| Licencje | Moduł \| Liczba |
| Definicje (nieobecności, listy płac, szkolenia) | Nazwa \| Typ / Symbol / Ważność |
| Pola dodatkowe (cechy) | Obiekt \| Nazwa pola \| Typ \| Wartości lub sposób wyliczenia \| Historyczne \| Mechanizm |
| Powiadomienia | Lp. \| Definicja \| Termin \| Odbiorcy \| Uruchomić |
| Ustalenia | Zagadnienie \| Ustalenie |
| Składniki dedykowane | Element \| Algorytm i zasady \| Lista płac \| Źródło danych \| Mechanizm |
| Wnioski w pulpitach | Wniosek \| Akceptacja \| Skutek w kartotece \| Modyfikacja |
| Raporty | Lp. \| Raport \| Rodzaj \| Opis \| Wzór |
| Integracje | Lp. \| System \| Kierunek \| Tryb \| Zakres i sposób realizacji |
| Migracja | Lp. \| Zakres danych \| Okres \| Źródło i format \| Odpowiedzialny |
| Pytania otwarte | Lp. \| Pytanie lub brakująca informacja \| Sekcja \| Odpowiedzialny \| Termin \| Status |
| Ryzyka | Lp. \| Ryzyko \| Wpływ \| Działanie zapobiegawcze \| Właściciel |
| Harmonogram | Lp. \| Etap \| Zakres lub produkt \| Termin \| Odpowiedzialny |

## 8. Język i konwencje zapisu

Piszemy po polsku, w trzeciej osobie, bezosobowo; „Klient” (wielka litera) dla zamawiającego,
„Wykonawca” lub „Alt One” dla nas. Nazwa systemu zawsze **enova365** (na początku zdania „Enova365”).

| Treść | Formuła |
|---|---|
| Standard systemu | „System enova365 umożliwia…”, „enova365 w standardzie oferuje…”, „Producent systemu zapewnia…” (czas teraźniejszy) |
| Ustalenie z Klientem | „Na etapie analizy ustalono, że…”, „W trakcie spotkań analitycznych ustalono…” |
| Decyzja Klienta | „Klient wybrał wariant…”, „Decyzją Klienta…”, „Na etapie akceptacji analizy Klient wybrał rozwiązanie…” |
| Rozwiązanie dedykowane | „W ramach wdrożenia przygotowane zostaną…” + **„(mechanizm N)”** – numeracja mechanizmów ciągła w obrębie dokumentu |
| Pole dodatkowe | „<Nazwa> – pole typu słownikowego / data / tekstowe / warunkowe (tak/nie) / referencja do listy pracowników” |
| Zobowiązanie Klienta | „Klient na etapie wdrożenia przekaże / wskaże / przedstawi…” |
| Wymaganie niemożliwe | „Po przeprowadzeniu analizy… nie jest możliwe do realizacji w systemie enova365, ponieważ… Proponujemy…” |
| Warianty | „W ramach wdrożenia proponujemy dwa rozwiązania: …” + decyzja Klienta |
| Odwołanie do pliku | „W pliku „<nazwa>.xlsx” załączonym do niniejszego dokumentu analizy…” |
| Uwaga | akapit zaczynający się od pogrubionego „**UWAGA:**” |
| Poza zakresem | wpis w sekcji „Rozwiązania poza zakresem wdrożenia” – „Rozwiązania te nie wchodzą w zakres wdrożenia…” |
| Brak informacji | **nigdy nie zgadujemy** – pole `[DO UZUPEŁNIENIA: …]` w treści + wiersz w tabeli Pytania otwarte |

Liczby: kwoty z separatorem tysięcy spacją i przecinkiem dziesiętnym („1 250,00 zł”), daty `dd.mm.rrrr`,
godziny `7:00–15:00` (półpauza), procenty bez spacji („50%”).

## 9. Pola do uzupełnienia i sekcje opcjonalne

- Format pola: `[DO UZUPEŁNIENIA: opis, czego dotyczy]` – żółte zakreślenie, styl znakowy „Pole do uzupełnienia”.
  Warianty do wyboru zapisujemy w opisie po ukośniku: `[DO UZUPEŁNIENIA: tak / nie]`.
- Znacznik sekcji opcjonalnej: akapit w stylu „Sekcja opcjonalna” bezpośrednio pod nagłówkiem:
  `[SEKCJA OPCJONALNA – usuń, jeśli nie dotyczy]` + ewentualnie, kiedy sekcja ma zastosowanie.
- Po uzupełnieniu treści **usuwamy cały nawias** razem z zakreśleniem. Jeśli sekcja nie dotyczy Klienta – usuwamy nagłówek i całą sekcję (numeracja przeliczy się sama), a do historii zmian dopisujemy, które sekcje usunięto.
- Dokument w statusie `Do akceptacji Klienta` lub `FINAL` nie może zawierać ani jednego pola `[DO UZUPEŁNIENIA` ani znacznika sekcji opcjonalnej.

## 10. Rysunki

- Zrzuty ekranu wyłącznie z bazy demonstracyjnej lub testowej, **bez danych osobowych** (nazwisk, PESEL, kwot wynagrodzeń konkretnych osób) i bez danych innych klientów.
- Rysunek wyśrodkowany, szerokość maks. szerokość tekstu; podpis „Rys. N Opis” w stylu Legenda.
- Miejsce na zrzut, którego jeszcze nie ma: `[DO UZUPEŁNIENIA: zrzut ekranu – <opis>]` + podpis.

## 11. Nazewnictwo plików i wersjonowanie

- Katalog klienta: `Klienci/<Nazwa>/` (poza gitem) – dokumenty analizy w katalogu głównym, notatki w `Notatki/`, poprzednie wersje w `Archiwum/`.
- Nazwa: `Analiza_<Nazwa>_vX.Y.docx`, załączniki `Analiza_<Nazwa>_ZAL_NN_<Obszar>_vX.Y.docx` (`<Nazwa>` bez spacji i polskich znaków).
- Wersje: `0.1` – pierwsza wersja ze wzoru; `0.2, 0.3…` – kolejne uzupełnienia po spotkaniach (wersje robocze); `1.0` – wersja przekazana Klientowi do akceptacji; `1.1, 1.2…` – poprawki po uwagach Klienta; status `FINAL` ustawiany przy akceptacji obu stron.
- Przy każdej zmianie: nowa wersja = nowy plik (poprzedni przenosimy do `Archiwum/`), wiersz w historii zmian, aktualizacja metryki (wersja, ostatnia modyfikacja, status) i spisów.
- Wersje załączników mogą się różnić od wersji dokumentu głównego; aktualne wersje załączników wpisujemy w „Wykazie załączników”.

## 12. Generator wzorów

Wzory `.docx` są **generowane** – nie edytujemy ich ręcznie. Zmiana wzoru = zmiana pliku treści
w `Tresc/` (lub stylów w `Generator/Program.cs`) i ponowne uruchomienie generatora:

```
cd Wzor_analizy/Generator
dotnet build -c Release -o bin
dotnet bin/GenWzor.dll "<ścieżka do Wzor_analizy>"
```

Generator wypisuje liczbę nagłówków, pól do uzupełnienia, sekcji opcjonalnych, tabel i rysunków,
a następnie wykonuje kontrolę: czy użyte style istnieją, czy hierarchia nagłówków jest ciągła oraz
czy w tekście nie ma słów z lokalnej listy `_robocze/zakazane_slowa.txt` (nazwy klientów, nazwiska,
identyfikatory z dokumentów źródłowych – plik poza gitem).

### Format plików treści (`Tresc/*.txt`, UTF-8)

| Zapis | Wynik |
|---|---|
| `@tytul`, `@klient`, `@obszar`, `@plik`, `@opis` (na początku pliku) | strona tytułowa, nazwa pliku wynikowego, właściwości dokumentu |
| `# Tekst` … `#### Tekst` | Nagłówek 1–4 (numeracja automatyczna) |
| `- tekst` / `  - tekst` | Lista punktowana poziomu 1 / 2 |
| `> tekst` | akapit w stylu Tekst komentarza |
| zwykła linia | akapit Normalny |
| `**tekst**` | pogrubienie w akapicie |
| `[DO UZUPEŁNIENIA: …]` | pole z żółtym zakreśleniem (automatycznie) |
| `@kolumny 30,70` | proporcje kolumn następnej tabeli |
| `\| a \| b \|` (kolejne linie) | tabela Proposal Table; pierwszy wiersz = nagłówek; w komórce `<br>` = nowy akapit, `- ` = punktor |
| `@tabela Opis` | podpis „Tabela N Opis” |
| `@rys plik.png \| Opis` | rysunek z `Generator/obrazy` + podpis „Rys. N Opis”; `@rys BRAK \| Opis` – pole na zrzut |
| `@opcjonalna [kiedy dotyczy]` | znacznik sekcji opcjonalnej |
| `@spis-tab`, `@spis-rys` | wykaz tabel / rysunków |
| `@akceptacja`, `@kontakt` | tabela akceptacji, blok kontaktowy |
| `@strona` | podział strony |
| `// komentarz` | pomijane |
