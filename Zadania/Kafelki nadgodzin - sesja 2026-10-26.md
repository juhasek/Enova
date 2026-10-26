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

## Uwagi klienta po testach (ta sama data) – ocena
Spełnione (zgodne z kodem i oznaczone przez klienta „ok”): wnioski i ręczne WP, odpracowanie WP wnioskiem
nie rusza bilansu nadgodzin, ręczny odbiór zmniejsza bilans, nadgodziny do odbioru w dni robocze, praca w normie
nie wpływa na bilanse.

Niespełnione:
1. Ręczne odpracowanie WP 1 h zwiększa NadgRazem o 2 h – wzór PPN − „Rozliczenie wyjścia prywatnego” nie
   rozpoznaje sposobu zapisu odpracowania u klienta (potrzebne strefy takiego dnia).
2. NadgRazem nie widzi wniosków na kolejne miesiące – liczy tylko strefę „Praca poza normą” z roku `Date.Today`;
   wniosek z wyprzedzeniem prawdopodobnie zakłada tylko strefę magazynu. Nazwa kafelka („ze wszystkich złożonych
   wniosków”) wskazuje, że źródłem mają być wnioski/magazyn, nie RCP.
3. Dni wolne: wartości −1 i „losowe” 3/4/6 dni są niemożliwe w obecnym `DniDoOdbioru` (tylko `Dni++`) –
   do sprawdzenia, czy u klienta jest wgrany kod z CodeFiles A1KafelkiNadgodzin i co liczy kafelek pulpitu.
   Godziny ≤ 8 h w sobotę trafiają do godzin, gdy dzień w planie ma typ „Pracy” (np. po zamianie dnia).
4. Do wypłaty po zmianie przestało działać przy ręcznym wniosku – regresja: wzór ze stref zamiast silnika.
   Propozycja: `Czasy.Nadgodziny(okres).Razem` (silnik sam odejmuje magazyn i dolicza wypłatę) − elementy z listy.

## Test stref wpisanych na kalendarz (bez wniosków)
- Strefy wpisane przez MCP `import_business_xml` (logika biznesowa) u S1, S2, S5, S6, S7, T1, T2, T3 od 27.10.2026; przypadki K01–K11 w `Cechy/Kafelek nadgodzin.md`.
- Wynik 1. wersji: odbiór nie zdejmował dnia, godziny szły w minus (odbiór za sobotę/święto, także w kolejnym miesiącu, częściowy odbiór);
  odpracowanie WP samą strefą rozliczenia zmniejszało NadgRazem.
- Przyczyna: enova paruje zasoby magazynu tylko czynnością „Rozlicz nadgodziny” / dokumentem rozliczenia czasu pracy (`DniPracy.WykonajRozliczenieWorker`);
  zwykły wpis strefy zostawia odbiór jako osobny zasób „minus”. Najpewniej to samo widziała klientka (−1, dzień nie schodzi).
- Poprawka (commit b0ca6a9): suma odbiorów/wypłat zdejmowana z nadgodzin FIFO po dacie, bez względu na parowanie; odpracowanie WP odejmowane tylko od pracy poza normą z tego samego dnia.
- Po poprawce wszystkie przypadki OK, bez regresji u pozostałych pracowników testowych.
- Dysk C: zapełnił się w trakcie (SQL timeout) – usunięte 2 kopie Claude_przed_widokiem_DAK (14.10 i 22.10 10:05) za zgodą użytkownika.

Otwarte: rok kalendarzowy w NadgRazem (nadgodziny z kolejnego roku nie wchodzą); kolejność FIFO – odbiór 8 h przy starszych godzinach z dnia roboczego zdejmie najpierw je, a dopiero potem dzień wolny; „do wypłaty” − elementy listy płac niesprawdzone.
