# Dodatek „Rezerwy urlopowe” — otwarte kwestie

| Nr | Etap | Obszar | Kwestia | Wpływ | Blokująca | Status | Decyzja i uzasadnienie | Data |
|----|------|--------|---------|-------|-----------|--------|------------------------|------|
| 1 | 1 | Standard | Formalna inwentaryzacja `scan-modules` / `scan-folders` (pokrycie standardem w 1.5 oparte na dekompilacji) | Niski | Nie | Zamknięta | Wykonana na enova 2512.5.6 (skrypty przeniesione do konsoli .NET 8): brak tabel rezerw w standardzie, potwierdza 1.5 | 2026-10-09 |
| 2 | 1 | Licencje / ryzyko | Planowane listy płac widoczne tylko przy „Rezerwy urlopowe do testów” — czy opierać produkt na tym mechanizmie, czy migawki mają być niezależnym źródłem prawdy (planowana lista tylko do księgowania) | Wysoki | Tak (dla Etapu 2) | Zamknięta | Źródłem prawdy są własne tabele dodatku; planowana lista płac tylko do narzutów i księgowania — zmiana/wycofanie mechanizmu w enova nie niszczy historii (zaakceptowane przez użytkownika) | 2026-10-09 |
| 3 | 1 | Wdrożenie | Akceptacja rozwiązania DLL przez pilotażowego klienta | Wysoki | Tak (dla implementacji) | Otwarta | — | 2026-10-09 |
| 4 | 1 | Zakres | Domyślne parametry produktu: wariant podstawy, zasadnicze nominalne, ujemny stan = 0, budżet = zaległy + pełny limit, PPK poza narzutami — przyjąć wartości z pilotażu? | Średni | Nie | Otwarta | — | 2026-10-09 |
| 5 | 1 | Zakres | Umowy cywilnoprawne poza v1 — potwierdzić | Niski | Nie | Otwarta | — | 2026-10-09 |
| 6 | 1 | Zakres | Odbiorca | Średni | Nie | Zamknięta | Produkt dla wielu klientów, obecny klient jako pilotaż — parametry w konfiguracji | 2026-10-09 |
| 7 | 1 | Dane | Historia rezerw | Średni | Nie | Zamknięta | Tak — migawki miesięczne we własnej tabeli jako funkcja ważna (should-have) | 2026-10-09 |
| 8 | 0 | Organizacja | Przedrostek i lokalizacja | Niski | Nie | Zamknięta | Przedrostek A1 (jak istniejący dodatek), projekty A1.RezerwyUrlopowe*; plan w Addon/Plan RezerwyUrlopowe | 2026-10-09 |
| 9 | 2 | Dane | Własne tabele vs rozszerzenie planowanych elementów wypłaty | Wysoki | Nie | Zamknięta | Własne tabele RezerwaUrlopowa + PozycjaRezerwyUrlopowej: PlanElementyWyp nie ma pól na stan urlopu/podstawy/status, mechanizm ma status testowy (kwestia 2) | 2026-10-09 |
| 10 | 2 | Logika | Algorytm elementu rezerwy jako „Klasa algorytmu” z dodatku (TypAlgorytmuElementu.KlasaAlgorytmu) — potwierdzić sposób wskazania klasy i działanie na planowanej liście | Średni | Tak (dla Etapu 3, sekcja logiki) | Otwarta | Wariant awaryjny: Edytor algorytmu z jednolinijkowym wywołaniem klasy dodatku | 2026-10-09 |
| 11 | 2 | Uprawnienia | Otwarcie zamkniętego miesiąca — osobne prawo (rola Administrator) czy prawo do edycji dokumentu | Niski | Nie | Otwarta | — | 2026-10-09 |
| 12 | 2 | Księgowanie | Czy zamknięcie miesiąca w dodatku ma zatwierdzać powiązane planowane listy płac (PlanowanaListaPłac.Zatwierdzona) | Średni | Nie | Otwarta | — | 2026-10-09 |
| 13 | 2 | Role | Role i zamknięcie | Średni | Nie | Zamknięta | Płace naliczają i zamykają; Zarząd/kontroling tylko odczyt; otwarcie — uprawnienie specjalne | 2026-10-09 |
| 14 | 2 | Menu / księgowanie / migracja | Ustalenia architektury | Średni | Nie | Zamknięta | Menu Kadry i płace/Płace/Rezerwy urlopowe; księgowanie schematem planowanych list (RUEW); bez migracji historii | 2026-10-09 |
