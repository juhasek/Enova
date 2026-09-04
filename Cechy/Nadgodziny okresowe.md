# Nadgodziny okresowe – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy cecha

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`). Dla wierszy, których definicja zawiera w nazwie „Praca poza normą" (w tym
warianty typu „Praca poza normą awaria"), wylicza liczbę godzin, które mają zostać rozliczone jako
**nadgodziny okresowe**.

## 2. Zgłoszony błąd i poprawka (27.08.2026)

**Zgłoszenie klienta:** nadgodziny okresowe błędnie się naliczają, gdy „Praca poza normą"/„Praca
poza normą awaria" przypada **w święto**. W święto praca tego typu **nie powinna w ogóle** trafiać
do nadgodzin okresowych — ma być rozliczana osobną cechą „Nadgodziny NSW" (nadgodziny
niedzielno-świąteczne, poza zakresem tego pliku).

**Przyczyna:** kod rozróżniał tylko jeden szczególny typ dnia — `TypDnia.Wolny` (dzień wolny w
grafiku, o nazwie zawierającej „Dzień wolny") — dla którego liczył nadgodziny proporcjonalnie do
realnie przepracowanego czasu w danej strefie. Święto to jednak inny typ dnia —
`TypDnia.Świąteczny` — więc praca w święto nie łapała się w ten warunek i trafiała do gałęzi
`else` napisanej z myślą o zwykłym dniu roboczym pełnoetatowca. Ponieważ w święto planowany czas
pracy (`plan`) wynosi zwykle 0, ta gałąź zwracała **sztywne 8 godzin** (norma − plan) za samą
obecność jakiejkolwiek pracy „poza normą", niezależnie od tego, ile godzin faktycznie przepracowano
— a przy kilku strefach tego typu w tym samym święcie wynik zwielokrotniał się przy sumowaniu na
liście płac.

**Poprawka:** dodano wczesny warunek — jeśli `dzienPracy.Definicja.Typ == TypDnia.Świąteczny`,
cecha od razu zwraca `0`, zanim wykona się jakakolwiek dalsza logika (gałąź dla dnia wolnego czy
dla zwykłego dnia roboczego). Nadgodziny za pracę „poza normą" w święto mają być liczone przez
oddzielną cechę „Nadgodziny NSW".

## 3. Drugi zgłoszony błąd i poprawka (27.08.2026) — dzień wolny w grafiku

**Obserwacja klienta:** po poprawce ze święta, praca „poza normą" wpisana w zwykłą sobotę (dzień
wolny w grafiku, 4h) też naliczała błędne, sztywne **8h** zamiast realnych 4h.

**Diagnoza (log z produkcji):** dla tej soboty `dzienPracy.Definicja.Typ = Wolny`, ale
`dzienPracy.Definicja.Nazwa = "Wolny"` — **nie** „Dzień wolny". Warunek pierwszej gałęzi
(`dzienPracy.Definicja.Nazwa.Contains("Dzień wolny")`) był więc zawsze fałszywy w tym środowisku,
mimo że dzień faktycznie jest dniem wolnym (`TypDnia.Wolny`, `NadgodzinySW = False`) — praca
trafiała do gałęzi `else` (pełny etat) i dostawała to samo płaskie `norma - plan = 8h`, co opisany
wyżej błąd świąteczny.

**Poprawka:** usunięto dopasowanie po nazwie (`Nazwa.Contains("Dzień wolny")`) z warunku pierwszej
gałęzi — zostaje tylko `dzienPracy.Definicja.Typ == TypDnia.Wolny && !dzienPracy.Definicja.NadgodzinySW`,
analogicznie do sposobu, w jaki rozpoznawana jest gałąź świąteczna. Dzięki temu praca „poza normą"
w dowolnym dniu typu `Wolny` (niezależnie od jego nazwy w konkretnej konfiguracji) liczy się
proporcjonalnie do realnie przepracowanego czasu (z limitem 8h/dobę), a nie płaskim `norma - plan`.

Zweryfikowano logiem produkcyjnym: dla soboty 17.10.2026 (4h pracy poza normą, `Wolny`/`Wolny`,
`NadgodzinySW=False`) nowy warunek poprawnie zwraca `4h`; dla niedzieli 25.10.2026
(`Świąteczny`/„Niedziela", `NadgodzinySW=True`) nadal poprawnie zwraca `0h`.

## 4. Trzeci zgłoszony błąd i poprawka (04.09.2026) — wiele stref „Praca poza normą" w jednym dniu

**Zgłoszenie klienta:** w zwykłym dniu roboczym zaplanowanym w grafiku na mniej niż 8 h
(np. 3 h) cecha „Nadgodziny okresowe" wykazuje **tę samą wartość `norma − plan`
(np. 5 h) na każdej strefie** typu „Praca poza normą" / „Praca poza normą awaria".
Przykład (dzień 2026-09-21, grafik 3 h):

| Strefa | Czas | Okresowe (błędnie) | Okresowe (poprawnie) |
|---|---|---|---|
| Praca w normie 8:00–11:00 | 3:00 | 0 | 0 |
| Praca poza normą awaria 12:00–19:00 | 7:00 | 5,00 | 5,00 |
| Praca poza normą 19:00–20:00 | 1:00 | 5,00 | 0,00 |
| Praca poza normą 20:00–21:00 | 1:00 | 5,00 | 0,00 |

Suma nadgodzin okresowych w dniu rosła więc z 5 h do 15 h.

**Przyczyna:** gałąź `else` (zwykły dzień roboczy pełnoetatowca) robiła bezwarunkowe
`return (decimal)(norma - plan).TotalHours` już na **pierwszej iteracji** pętli po
strefach — nie patrzyła, czy iterowana strefa to `Row`, nie sumowała godzin
rozliczonych już w poprzednich strefach „poza normą" ani nie uwzględniała realnego
czasu bieżącej strefy. Ponieważ `praca` (czas przepracowany w dobie) i `plan` są
stałymi dnia, warunek `praca > plan && plan < norma` był spełniony dla każdej strefy,
więc każda strefa „poza normą" dostawała pełne `norma − plan`.

**Poprawka:** gałąź `else` liczy teraz nadgodziny okresowe analogicznie do gałęzi dnia
wolnego — akumuluje czas stref po kolei chronologicznie (`sumaDoby`) i dla strefy
równej `Row` zwraca tylko tę część jej godzin, która mieści się w **dobowym oknie
nadgodzin okresowych `(plan, norma]`**:

```
przedStrefa = sumaDoby - st.Czas            // czas pracy w dobie przed bieżącą strefą
dolna = clamp(przedStrefa, plan, norma)
gorna = clamp(sumaDoby,    plan, norma)
wynik = max(0, gorna - dolna)
```

Godziny do wysokości `plan` nie są okresowe (to praca w normie), godziny powyżej
`norma` (8 h/dobę) to nadgodziny dobowe 50 %/100 %, a przedział pomiędzy nimi to
nadgodziny okresowe — rozdzielony pomiędzy kolejne strefy „poza normą" wg kolejności
godzin. Gdy `plan >= norma` (dzień zaplanowany na pełne 8 h) lub `praca <= plan`,
cecha zwraca `0`.

Blok obsługi „czarnych dziur" (poniedziałek po dobie niedzielno-świątecznej) w gałęzi
`else` pozostaje — jak dotąd — nieaktywny: po znalezieniu strefy `Row` następuje
`return`, a dla pozostałych stref `continue`. Zmiana zachowania tego fragmentu nie
była przedmiotem zgłoszenia.

## 5. Do potwierdzenia przed wdrożeniem

- Kod cechy „Nadgodziny NSW", która ma faktycznie rozliczać pracę „poza normą" w święto/dzień
  wolny objęty NSW, nie jest jeszcze dodany do repozytorium — do uzupełnienia, gdy będzie dostępny.
- Poprawkę z pkt 4 (wiele stref „poza normą") zweryfikowano dotąd tylko analitycznie na
  przykładzie ze zgłoszenia (grafik 3 h, strefy 7 h + 1 h + 1 h → 5 h + 0 h + 0 h) — do
  potwierdzenia na żywej bazie klienta, w tym dla etatu niepełnego i dla dnia z pracą
  w normie krótszą niż `plan`.
- Reszta pliku (obsługa „czarnych dziur" wokół doby niedzielno-świątecznej — nadal
  nieaktywna w gałęzi dnia roboczego, martwe zmienne `licznik`/`roznica`, duży blok
  zakomentowanego starego kodu) pozostała bez zmian — nie była przedmiotem tych zgłoszeń.
