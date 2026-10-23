# Analiza przedwdrożeniowa klienta „B” – dzień 2 (Kadry cz. 2, Płace cz. 1) – sesja 2026-10-23

## 1. Zlecenie

Po drugim dniu warsztatów (23.10.2026) użytkownik przekazał cztery transkrypcje Teams (ok. 4,3 h) i swoje notatki.
Pliki trafiły do `Analiza\<klient>\` w repo. Zadanie: zweryfikować materiały w tym samym trybie co po dniu 1.

## 2. Wynik (poza gitem – `Klienci\<klient>\Notatki\`)

- Oryginały skopiowane do `Pobrane\Analiza\<klient>\`, notatki konsultanta do `Notatki\`.
- Notatka D2 – wygenerowana `gen_d1.ps1` na szablonie notatki PM z D1. Blok „Informacje podstawowe” poprawiony
  osobnym skryptem: dzień, data, uczestnicy, link do nagrania z D1 zastąpiony zwykłym tekstem.
- Kwestionariusz v0.3 (`gen_kw.ps1`) – 60 odpowiedzi zaktualizowanych, 2 nowe pytania, historia zmian i metryka.
  Wersja 0.2 trafiła do `Archiwum\`.
- `2026-10-23_D2_weryfikacja_materialow.md` – rozbieżności między notatkami a transkrypcją, luka w nagraniu,
  rzeczy do sprawdzenia przed wysłaniem.
- Pliki treści: `zrodla\2026-10-23_D2_tresc.txt`, `zrodla\2026-10-23_kwestionariusz_odpowiedzi.txt`.

## 3. Najważniejsze ustalenia (bez danych klienta)

- Nowa formuła spotkań: Klient opisuje proces, AltOne pokazuje, jak go odwzorować w programie.
- Zatrudnienie: kartoteka kandydata po akceptacji wniosku, rola pulpitu „Kandydat” z kwestionariuszem, pakiet PDF
  Klienta w pulpicie (bez wstępnego wypełniania danymi). Informacja dla przełożonego o skierowaniu na badania przez
  termin i opis na badaniu.
- Definicje stanowisk tworzy Klient w programie po migracji produkcyjnej – zamyka kwestię otwartą z dnia 1.
- Kod pracownika: osobna numeracja w każdej spółce. To kolejny argument Klienta za osobnymi bazami – decyzja nadal
  otwarta.
- Płace: harmonogram list płac, ZFŚS jako osobne składniki, polisy grupowe przez wskaźniki i składnik na polisę,
  kod RSA w kolumnie listy płac. Klient przekaże opis składników dla każdej spółki.

## 4. Uwagi

- Między 12:32 a 13:02 nie ma nagrania. Część płacowa z tego czasu pochodzi tylko z notatek konsultanta.
- Folder `Analiza\` jest w gicie (szablony). Surowych materiałów klienta nie wolno tam zostawiać – nie commitowano
  ich, użytkownik ma je usunąć z `Analiza\<klient>\`.
- `Podglad/` trzeba zbudować przed użyciem (`dotnet build -o bin`) i utworzyć katalog wyjściowy.

## 5. Dalej

- Uzupełnić uczestników i link do nagrania w notatce D2, rozstrzygnąć punkty z pliku weryfikacji (wycena paczki
  dokumentów, dopełnianie zerami migrowanych kodów).
- 27.10.2026 – czas pracy; 26.10 tylko w razie potrzeby.
- Zadania AltOne: link do demo, zestawienie dokumentów zatrudnienia, opcje importu z PUE, informacja mailowa
  o rekrutacji i onboardingu, pozycje „do sprawdzenia” z notatki D2.
