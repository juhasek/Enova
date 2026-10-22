# Analiza przedwdrożeniowa klienta „B” – dzień 1 (Kadry cz. 1) – sesja 2026-10-21

## 1. Zlecenie

Po pierwszym dniu warsztatów (21.10.2026, Kadry cz. 1) użytkownik przekazał w `Pobrane\Analiza\<klient>\`:
- dwie transkrypcje Teams (ok. 4 h i 13 min),
- swoje luźne notatki (`.txt`),
- kwestionariusz v0.1 z pierwszymi odpowiedziami,
- notatkę „D1” przygotowaną przez PM.

Zadanie: zweryfikować całość i przygotować własną wersję kwestionariusza oraz notatki D1.

## 2. Wynik (poza gitem – `Klienci\<klient>\Notatki\`)

- `Kwestionariusz_<klient>.docx` v0.2 – plik użytkownika z dopisanymi odpowiedziami. Wpisy użytkownika
  zachowane, uzupełnienia z transkrypcji jako kolejne akapity. Statusy: „Wyjaśnione” tylko przy jasnych
  odpowiedziach, w pozostałych przypadkach „Do potwierdzenia”. Dodane 4 pytania, uzupełnione metryka
  i historia zmian.
- `2026-10-21_D1_Kadry_cz1_notatka_konsultant.docx` – układ i formatowanie notatki PM zachowane, treść
  poprawiona i rozszerzona.
- `2026-10-21_D1_weryfikacja_materialow.md` – wewnętrzna lista rozbieżności (notatka PM, kwestionariusz,
  notatki konsultanta).
- `zrodla\` – pliki treści, z których generowane są oba `.docx`. Poprawki wprowadzać w nich
  i generować ponownie.
- Archiwum klienta utworzone. Dokumentów analizy (dokument główny i załączniki) jeszcze nie zakładano.

## 3. Najważniejsze wnioski z weryfikacji (bez danych klienta)

- Notatka PM pomyliła nazwy dwóch podmiotów i pominęła jeden podmiot wraz z jego danymi.
- Rekrutacja i onboarding są w niej oznaczone jako „P2”. W rozmowie ustalono, że proces jest w zakresie
  analizy, a o starcie (od razu albo ok. miesiąc po starcie) zdecydujemy w trakcie analizy.
- Notatka podaje jako decyzję „definicje stanowisk przed migracją”, a była to rekomendacja AltOne.
- W notatce brakuje wielu wątków: zatrudnienie w kilku spółkach, ADR/FEP, pakiet mobilności, kierunek
  integracji z systemem transportowym, ryzyko wymiaru urlopu po migracji, służba wojskowa, akord
  i prowizja, PFRON/SODiR.
- Transkrypcja jest bardzo zniekształcona, więc niepewne fakty są oznaczone „do potwierdzenia”.
  Do wyjaśnienia na spotkaniu 23.10.2026.

## 4. Narzędzia (`Wzor_analizy/Narzedzia/`, PowerShell 5.1 + XLinq, bez Worda)

- `docx2txt.ps1 -Path x.docx -Out x.txt` – tekst z docx (akapity, nagłówki, tabele `| a | b |`).
  Transkrypcję Teams warto potem podzielić po ` / ` na wiersze.
- `gen_d1.ps1 -Src <notatka PM>.docx -Content tresc.txt -Out wynik.docx` – przebudowuje treść notatki
  na formatowaniu PM. Zachowuje akapity 1–6 (tytuł, informacje podstawowe z hiperłączem do nagrania).
  Znaczniki w treści:
  - `H|` – nagłówek,
  - `S|` – podnagłówek,
  - `P|` – akapit (`**pogrubienie**`),
  - `B|` – punktor,
  - `T|kol|kol` + `R|…` + `E|` – tabela.

  Prototypy elementów są brane po indeksach z notatki PM D1 – przy innym szablonie trzeba je sprawdzić.
- `gen_kw.ps1 -Src kwestionariusz.docx -Content odp.txt -Out wynik.docx -ClientName "…"` – wypełnia
  kwestionariusz ze wzoru. Znaczniki:
  - `A|początek pytania|odpowiedź|status` – akapity w odpowiedzi rozdzielane znakiem `¶`,
  - `N|pytanie-kotwica|nowe pytanie|odpowiedź|sekcja|status` – wiersz wstawiany pod kotwicą,
  - `M|etykieta:|wartość` – metryka,
  - `H|…` / `H+|…` – historia zmian.

  Placeholder nazwy klienta zastępowany jest w treści i nagłówkach. Skrypt wypisuje pytania,
  których nie znalazł.
- `Podglad/` – konsola net10 (`~/.dotnet10/dotnet build`), która przez DevExpress RichEdit z serwera 2610
  tworzy z docx plik PDF i obrazy PNG wybranych stron:
  `dotnet bin/Preview.dll x.docx outDir 1,2,5`.
- Pliki `.ps1` nie zawierają polskich znaków (PS 5.1 czyta je jako ANSI). Polskie teksty trzymamy
  w plikach treści UTF-8. Konstruktory XLinq: `[XElement]::new(...)`, nie `New-Object` – inaczej
  PowerShell źle przekazuje argumenty.

## 5. Dalej

- Spotkanie 23.10.2026, godz. 9:00: dokończenie Kadr, ponowne omówienie onboardingu i pozycji
  „do potwierdzenia”.
- Po kolejnych dniach: założenie dokumentów analizy ze wzoru (procedura „uzupełnij analizę” w `CLAUDE.md`).

## 6. Korekta konsultanta (22.10.2026)

- Konsultant poprawił notatkę D1 – jego wersja jest obowiązująca i została wysłana do klienta. Plik treści
  w `zrodla\` zsynchronizowano z nią, a wersję Claude przeniesiono do `Archiwum\` klienta.
- Styl notatki dla klienta: bez uwag o transkrypcji i bez „(do potwierdzenia)”, bez niepewnych faktów i wewnętrznych
  ograniczeń systemu, tylko istotne kwestie otwarte; nazwa wykonawcy zawsze „AltOne” (poprawione też we wzorze analizy).
