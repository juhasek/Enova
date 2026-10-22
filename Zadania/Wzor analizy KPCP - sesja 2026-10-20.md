# Wzór analizy przedwdrożeniowej KPCP – sesja 2026-10-20

## 1. Zlecenie

Zbudować wzór dokumentu analizy przedwdrożeniowej Kadry, Płace, Czas pracy (enova365) na podstawie
wcześniejszych analiz z `Pobrane\`. Wzorcowa była analiza klienta „S” (dokument główny i zał. 1, 2, 3, 5),
uzupełniona o dwie starsze analizy (kody A i B w `raport_analizy_dokumentow.md`). Zakres prac:
- raport struktury,
- `STYLE_GUIDE.md`,
- wzory `.docx`,
- kwestionariusz spotkań,
- procedura „uzupełnij analizę” w `CLAUDE.md`.

## 2. Decyzje użytkownika

- Zał. 4 (księgowanie i podzielnik) pominięty, bo użytkownik tego nie robi. Numeracja załączników stała (1, 2, 3, 5).
- Każdy dokument ma być osobnym wzorem, złożonym z najlepszych fragmentów wszystkich analiz źródłowych.
- Narzędzia w .NET (OpenXML i DevExpress z serwera 2610), nie Python.
- `Klienci/` i `Analiza/*.docx` są poza gitem. Pytania z `Analiza/` wykorzystane jako źródło kwestionariusza.
- Raporty, integracje, migracja i workflow to rozdziały dokumentu głównego.
- Logo i nazwa wykonawcy zostają. Kontakt jest w polach `[DO UZUPEŁNIENIA]`.
- Zrzuty ekranu: tylko standardowe okna bez danych osobowych.
- Sekcje projektowe są oznaczone jako opcjonalne.
- **Dopłata za nadgodziny średniotygodniowe we wzorze zawsze 100%.** Nie zamieniać na wariant do wyboru,
  mimo że kalendarz ma 3 ustawienia.

## 3. Wynik (`Wzor_analizy/`)

- Wzory `.docx`:
  - dokument główny,
  - Z1 Kadry,
  - Z2 Czas pracy,
  - Z3 Płace,
  - Z5 Pulpity,
  - `Kwestionariusz_spotkan_KPCP.docx`.
- Pliki `.docx` są generowane z `Tresc/*.txt` generatorem `Generator/` (net8, czyste OpenXML). Generator
  kontroluje:
  - style,
  - ciągłość nagłówków,
  - zakazane słowa (`_robocze/zakazane_slowa.txt`, poza gitem).
- Uruchomienie generatora:
  ```
  dotnet build -c Release -o bin
  dotnet bin/GenWzor.dll "<Wzor_analizy>"
  ```
- `raport_zgodnosci_z_instrukcja.md` to porównanie wzorów z instrukcją KP (PDF, 2196 stron):
  - 12 rozbieżności, z czego punkty 2–12 poprawione, a punkt 1 (nadgodziny 100%) bez zmian z decyzji użytkownika;
  - rozdz. 5 to pozycje niepotwierdzone w instrukcji, sprawdzone w plikach serwera 2610 bez logowania do bazy;
  - wynik rozdz. 5: nazwa wydziału ma maks. **100** znaków (kod 20), a nie 80;
  - potwierdzone: raport „Lista nierozliczonych elementów” (Wydruki/Wynagrodzenia i zasiłki), parametry
    ujemnej podstawy ZUS (Płace/Składki ZUS i Płace/Ogólne), rozszerzony zapis zmian (Systemowe/Zmiany ustawienia,
    Systemowe/Zmiany rekordów).

## 4. Dalsza praca

Po spotkaniu z klientem użytkownik przekazuje nazwę klienta i notatki i pisze „uzupełnij analizę”. Dalej
postępuje się według procedury w `CLAUDE.md`:
- tworzy się `Klienci\<Nazwa>\` z kopiami wzorów w wersji 0.1;
- ustalenia trafiają do pól według kwestionariusza;
- braki trafiają do „Pytań otwartych”;
- każda zmiana to nowa wersja, a poprzednia trafia do `Archiwum\`;
- na koniec powstaje lista pozostałych pól.

Danych osobowych pracowników (PESEL, wynagrodzenia) nie przekazywać.

## 5. Uwagi techniczne

- Podgląd `.docx` → PDF → PNG bez Worda: DevExpress RichEdit v25.2 działa tylko na net10 (`~/.dotnet10`).
- Fakty o programie bez bazy można sprawdzić w plikach serwera 2610:
  - grep po DLL-ach: `CaptionHtml` w `*.UI.dll` daje ścieżki w Narzędzia/Opcje, a rejestracja wydruków jest
    w `*.Reports.dll` i `*.UI.dll`;
  - refleksja (MaxLength, enumy) przez `dotnet X.dll`, bo nowy `.exe` blokuje Defender.

Commity: `aff4ec8`, `1519527`, `22d20e7`, `76d95bf`.
