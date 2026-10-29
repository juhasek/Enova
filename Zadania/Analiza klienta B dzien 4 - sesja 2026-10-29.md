# Analiza przedwdrożeniowa klienta „B” – dzień 4 (integracja z systemem planowania przewozów) – sesja 2026-10-29

## 1. Zlecenie

Po czwartym spotkaniu (29.10.2026, spółka transportowa – integracja enova365 z systemem planowania i rozliczania
przewozów Veritum XLBT w zakresie kierowców) użytkownik wrzucił do `Pobrane\` transkrypcję Teams (2 h 53 min)
i swoje notatki. Zadanie: przygotować materiały tak jak po dniach 1–3.

## 2. Wynik (poza gitem – `Klienci\<klient>\Notatki\`)

- Oryginały skopiowane do `Pobrane\Analiza\<klient>\`, notatki konsultanta do `Notatki\2026-10-29_notatki_konsultanta.txt`.
- Notatka D4 – `gen_d1.ps1` na szablonie notatki PM z D1, blok „Informacje podstawowe” skopiowany z notatki D3
  i przestawiony na dzień 4 skryptem w scratchpadzie (podmiana tekstów runów). Nowa sekcja 4 – tabela kierunków
  wymiany danych (zakres / kierunek / wyzwalacz / uwagi), o którą prosił użytkownik w notatkach.
- Kwestionariusz v0.5 (`gen_kw.ps1`) – 14 odpowiedzi zaktualizowanych, 3 nowe pytania, historia zmian i metryka.
  v0.4 w `Archiwum\`.
- `2026-10-29_D4_weryfikacja_materialow.md` – rozbieżności notatki/transkrypcja, tematy nieomówione, rzeczy do
  sprawdzenia przed wysłaniem.
- Arkusz zadań (wersja użytkownika) – dopisane AltOne 47–56, Klient 28–32; wiersz spotkania w spółce transportowej
  zamknięty, termin rezerwy urlopowej „do ustalenia”, „Stan na” 29.10.2026. Dopisywanie konsolówką net10
  + DevExpress.Spreadsheet 25.2 (serwer 2610) w scratchpadzie – format kopiowany z ostatniego wiersza.
- Pliki treści: `zrodla\2026-10-29_D4_tresc.txt`, `zrodla\2026-10-29_kwestionariusz_odpowiedzi.txt`.

## 3. Najważniejsze ustalenia (bez danych klienta)

- Komunikacja na tym etapie przez WebAPI enova365 (szyna danych – decyzja IT). AltOne wysyła dokumentację metod
  dostawcy systemu przewozowego; tabela wymiany danych z oznaczeniem wyeksportowania.
- enova365 → system przewozowy: kartoteka i przedłużenie umowy (wyzwalacz: nowe pole statusu podpisania umowy na etacie,
  we wszystkich spółkach), zakończenie zatrudnienia (wyzwalacz: przyczyna rozwiązania umowy), forma zatrudnienia
  (UoP / B2B), nieobecności na bieżąco, terminy badań lekarskich.
- System przewozowy → enova365: uprawnienia zawodowe kierowców (widoczne w pulpicie), plan pracy kierowców (każda zmiana),
  dane do naliczenia wynagrodzeń (po akceptacji rozliczenia, potem zmiany pojedynczych rekordów).
- Planowani są tylko kierowcy z podpisaną umową; w enova365 nie ma etapu „planowane zatrudnienie” (ryzyko).
- Ryczałty za nadgodziny i dyżury przychodzą w pełnej kwocie, pomniejsza enova365. Premia uznaniowa – grupowy wniosek
  w pulpicie kierownika po wstępnym naliczeniu + raport brutto/netto w „Mój zespół”.
- Nagłe nieobecności – wniosek za pracownika w pulpicie kierownika (do przygotowania).
- Nieomówione: rezerwa urlopowa i pora nocna kierowców (zapowiadane na ten dzień) – otwarte.

## 4. Uwagi do warsztatu

- Wstęp do tabeli kierunków wymiany w notatce D4 zmieniony z „do potwierdzenia” na neutralny opis – zgodnie z zasadą,
  że notatka dla klienta nie zawiera sformułowań „do potwierdzenia” (zostało tylko standardowe zdanie o priorytetach,
  jak w D3).
- Lp. w dopisanych wierszach arkusza zapisane jako tekst (konsolówka ustawia `Value` stringiem) – wpływa tylko
  na sortowanie; przy kolejnym dopisywaniu warto parsować liczby.

## 5. Dalej

- Przed wysłaniem notatki D4: uczestnicy, link do nagrania, ryczałt za porę nocną, czy rezerwę omówiono poza nagraniem.
- AltOne: dokumentacja WebAPI, potwierdzenie z IT (REST, architektura, aplikacja mobilna), metody dla danych płacowych,
  struktura danych z dostawcą, pole statusu umowy, wnioski i raport w pulpicie, dieta wirtualna w dodatku kierowców.
- 4.11.2026 – spotkanie o księgowaniu wynagrodzeń (dokument księgowy do modułu księgowego systemu przewozowego).
