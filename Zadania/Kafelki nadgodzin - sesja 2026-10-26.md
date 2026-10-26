# Kafelki nadgodzin – sesja 2026-10-26

## Cel
Analiza cech kafelka nadgodzin wgranych do bazy Al (FeatureDefs 21–26) wobec zgłoszenia błędów
(dokument podsumowania w Pobrane – 7 punktów) i wprowadzenie poprawek.

## Słabe punkty starego kodu
1. **Dni wolne** – typ dnia z dnia pracy, a nie z planu; dzień liczony przy każdym zasobie, także po częściowym odbiorze; godziny ≤ 8 h w dniu wolnym liczone podwójnie (dzień + godziny), próg 8 h na sztywno.
2. **Do odbioru** – w dniu roboczym odejmowany niezwiązany zasób wypłaty z tego samego dnia (podwójne odjęcie); tylko bieżący okres rozliczeniowy (godziny z poprzednich okresów i odbiory w kolejnym okresie znikały).
3. **NadgRazem** – w pętli po dniach roku `nadgodziny = …` (przypisanie zamiast sumy) dla pełnego etatu kasowało sumę z dni niepełnego etatu; dla niepełnego etatu dzień wolny pomniejszany o normę dobową.
4. **Do wypłaty** – silnik `Czasy.Nadgodziny(miesiąc)` bez uwzględnienia tego, co już naliczono na liście płac.
5. **Niepełny etat** – patrz 3; logika zależna od wymiaru per dzień.
6. **Wyjścia prywatne** – enova paruje strefy wyjścia prywatnego z nadgodzinami w magazynie, co zmniejszało bilans nadgodzin.
7. **Wnioski z terminem odbioru** – odbiór poza bieżącym okresem rozliczeniowym nie był widoczny.
Dodatkowo: `WgNazwy[...]` bez sprawdzenia null (brak strefy = suma wszystkich stref dnia), kod powielony w 2 cechach.

## Co zrobiono
- Repo: oryginały z bazy (commit b685431), nowy kod wspólny `Cechy/Kafelek nadgodzin - kod wspolny`, cechy jako jednolinijkowe wywołania, opis `Cechy/Kafelek nadgodzin.md`.
- Baza Al: CodeFiles **2016** „A1KafelkiNadgodzin” (projekt NULL jak CodeFiles 1013), podmiana `FeatureDefs.Code` 21, 22, 23, 24, 26 (25 bez zmian), podbity stamp RuntimeProjects 2. Serwer przekompilował kod sam.
- Kompilacja lokalna (net10, DLL serwera 2610) – bez błędów.

## Wyniki testów (MCP, Al)
S2 0 / 0:00 / 2:00, S4 1 dzień / 0:00 / 4:00, S5 0 / 6:00 / 6:00, T4 0 / 1:00 / 1:00, T6 1 / 0:00 / 3:00,
026 0 / −8:00 (odbiór bez pokrycia) – zgodne z oczekiwaniami. Szczegóły w `Cechy/Kafelek nadgodzin.md`.

## Otwarte
- Przetestować na danych: częściowy odbiór dnia wolnego, wypłata z magazynu, wyjście prywatne przy nadgodzinach w magazynie, nadgodziny naliczone na liście płac.
- Do potwierdzenia: czy ujemny bilans „na zapas” ma być widoczny (teraz tak); czy strefa „Potrącenie wyjścia prywatnego” u klienta ma zmniejszać kafelek WyjsciePrywatne (teraz nie – nie znam jej konfiguracji); czy „Bilans wszystkich nadgodzin” ma iść ze stref (teraz) czy z silnika enova.
- Kod wymaga wersji klienta zgodnej z API magazynu (`ZasobyRN`, `ObrotyRN.WgPracownik`) – sprawdzone tylko na 2610.
