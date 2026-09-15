# Nadgodziny 50 nowe – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Kontekst i cel

Standardowo klienci enova365 korzystają z wbudowanego silnika nadgodzin
(`pracownik.Czasy.Nadgodziny(okres)`, klasa `KalkulatorPracownika`) i nie potrzebują
dodatkowych cech. U tego klienta nadgodziny muszą jednak zostać rozbite **per strefa
dnia** — konkretnie dla stref „Praca poza normą” oraz „Praca poza normą awaria” — bo na
liście płac te dwie strefy zasilają różne elementy wynagrodzenia i są inaczej księgowane
(inne konto/MPK, w zależności czy nadgodziny wynikły z normalnej pracy poza harmonogramem
czy z awarii). Wbudowany silnik nie daje takiego rozbicia — liczy tylko zagregowany wynik
za pracownika/okres, bez wskazania, z której konkretnej strefy dnia wzięły się godziny.

Wcześniejsza cecha „Nadgodziny 50 test” (`Cechy/Nadgodziny 50 test`) realizowała ten sam
cel, ale zawierała błędy (pomylenie `Row.Definicja`/`st.Definicja` w pętli sumującej,
usunięty/niekonsekwentny blok „czarnej dziury” — patrz `Cechy/Nadgodziny 50 test.md`) i
liczyła normę dobową jako sztywne 8h niezależnie od konfiguracji kalendarza pracownika.

**„Nadgodziny 50 nowe” to nowe podejście od zera** — nie jest modyfikacją/poprawką starej
cechy, tylko osobną, równoległą implementacją zaprojektowaną tak, by wprost odtwarzać
sposób liczenia nadgodzin 50% **przez sam system enova** (silnik `KalkulatorPracownika`/
`CzasPracyBaseWorker`, zdekompilowany i opisany w pamięci
`reference_enova_nadgodziny_kalkulator.md`), tylko zawężony do stref „Praca poza normą”
(w tym „awaria”). Dopisek „nowe” w nazwie ma odróżnić ją od starej, błędnej cechy — obie
mogą tymczasowo współistnieć w bazie na czas porównania wyników.

## 2. Zakres (Row) i warunek wejścia

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`, tabela `StrefyPracy`). Liczy wartość **tylko** dla wierszy, których
`Definicja.Nazwa` zawiera „Praca poza normą” (obejmuje też „Praca poza normą awaria” —
dopasowanie przez `Contains`, tak samo jak w starej cesze). Dla wszystkich innych stref
zwraca `0`.

## 3. Algorytm — odtworzenie logiki systemu

W przeciwieństwie do starej cechy (sztywna norma 8h, dwie osobne, ręcznie pisane gałęzie
dla dnia roboczego i dnia wolnego), nowa cecha **czyta rzeczywistą konfigurację** i stosuje
jedną, wspólną logikę dla każdego typu dnia:

1. **Dni niedzielno-świąteczne** (`Definicja.NadgodzinySW` lub `Typ == Świąteczny`) —
   zwraca `0`. Te godziny rozlicza osobna, już istniejąca cecha „Nadgodziny NSW”
   (niezmieniona, bez zgłoszonych błędów).
2. **Norma dobowa** — liczona tak jak w systemie, wg pola konfiguracyjnego kalendarza
   pracownika `Kalendarz.Nadgodziny.AlgorytmDobowa`:
   - `Kalendarz` → z planu dnia, a gdy plan wynosi 0 → z `Etat.NormaDobowa`,
   - `RównoważnyCzasPracy` → większa z (plan dnia, `Etat.NormaDobowa`),
   - w każdym innym przypadku (`Konfiguracja` i domyślnie) → zawsze `Etat.NormaDobowa`.

   W bazie Claude wszystkie kalendarze poza „4msc” mają `AlgorytmDobowa = Konfiguracja`
   (norma = stała `NormaDobowa` z Etatu, 8h dla większości, 7h dla kalendarza „Podstawowy -
   Orzeczenie Niep.”, **niezależnie od typu dnia** — w tym dni wolne od pracy). To jest
   istotna różnica względem starej cechy, która dla dni typu „Wolny” liczyła osobno,
   sztywnym `8:00` — w nowej cesze wychodzi to samo (8h), ale jako efekt odczytania
   konfiguracji, nie osobnej gałęzi kodu, więc automatycznie działa poprawnie też dla
   kalendarza „4msc” (równoważny czas pracy) czy przyszłej zmiany normy na 7h.
3. **Przypisanie nadwyżki do konkretnej strefy** — chronologiczne sumowanie stref
   „Praca w normie” + „Praca poza normą” (obu wariantów) w ramach dnia; dla strefy `Row`
   liczona jest tylko ta część jej godzin, która mieści się w oknie **ponad normę dobową**
   (technika identyczna jak już sprawdzona w cesze „Nadgodziny okresowe” pkt 4 — `dolna`/
   `gorna` na sumie narastającej), więc przy kilku strefach „poza normą” w jednym dniu
   godziny nie są liczone podwójnie.
4. **Limit dobowy 50%** (`Kalendarz.Nadgodziny.Nadgodz50`) — stosowany **tylko wtedy**, gdy
   globalna konfiguracja `Config.Nadgodziny.Dobowe100` = prawda (reguła sprzed/po 2003 roku
   używana przez sam silnik). W bazie Claude ta opcja jest **wyłączona** (`Dobowe 100 =
   false`), więc limit `Nadgodz50` (2:00 na wszystkich kalendarzach) faktycznie **nie
   obowiązuje** — cały dobowy nadmiar (poza godzinami nocnymi, patrz pkt 5) liczy się jako
   50%. Gdyby klient kiedyś włączył `Dobowe 100`, cecha automatycznie zacznie stosować
   limit i przekaże nadmiar do „Nadgodziny 100 nowe”.
5. **Godziny nocne wymuszone na 100%** — w bazie Claude `Config.Nadgodziny.Nocne100 = true`,
   czyli ta część nadwyżki, która przypada na godziny nocne kalendarza
   (`Kalendarz.Nocne.Od`/`Do`), **nie** jest liczona jako 50% — przechodzi do
   „Nadgodziny 100 nowe”. Okno nocne (z uwzględnieniem przejścia przez północ) wyznacza
   sam system przez wywołanie `KalkulatorPracy.NocOkres(ph)` — cecha go nie przelicza
   ręcznie, żeby nie powielać (i nie pomylić) logiki przejścia przez dobę.

## 4. Zależność z „Nadgodziny 100 nowe”

Cecha siostrzana `Cechy/Nadgodziny 100 nowe` liczy dokładnie tę samą nadwyżkę dobową i
zwraca dopełnienie: część ponad limit `Nadgodz50` (gdy `Dobowe100` aktywne) oraz część
przypadającą na godziny nocne (gdy `Nocne100` aktywne). Suma obu cech dla danej strefy w
danym dniu = cała nadwyżka ponad normę dobową przypadająca na tę strefę. Obie cechy są
samodzielne (nie odwołują się do siebie nawzajem w kodzie) — każda liczy identyczny
fragment logiki od zera, zgodnie z ograniczeniami edytora skryptów enova (bezpieczniej
trzymać całą logikę w jednej metodzie, patrz `reference_enova_edytor_skryptow_ograniczenia`).

## 5. Świadomie poza zakresem

- **Nadgodziny okresowe** (miesięczne bilansowanie normy okresu rozliczeniowego) — to
  osobny, dużo bardziej złożony mechanizm (bilansowanie między miesiącami, magazyn
  nadgodzin). Istniejąca cecha „Nadgodziny okresowe” już to obsługuje (poprawiona wcześniej
  w tej sesji) — nowa cecha go nie duplikuje.
- **Nadgodziny między dobami pracowniczymi jako okresowe 100%**
  (`Config.Nadgodziny.NadgodzinyMiędzyDobJakoOkr100Ext`) — rzadki przypadek graniczny z
  silnika, nieuwzględniony w tej cesze.
- Przypadki wielodniowe („czarna dziura” między dobą niedzielno-świąteczną a kolejnym dniem
  roboczym) — jak w innych cechach tego repo, obsługuje je „Nadgodziny okresowe”, nie ta
  cecha.

## 6. Status: NIEZWERYFIKOWANE

Środowisko robocze tego repo nie ma dostępu do żywego testu w edytorze skryptów enova
(brak buscall/GUI). Kod zweryfikowano **analitycznie** względem:
- zdekompilowanego silnika enova (`KalkulatorPracownika`, `CzasPracyBaseWorker` —
  `Soneta.KadryPlace.dll`, opis w pamięci `reference_enova_nadgodziny_kalkulator`),
- rzeczywistej konfiguracji bazy Claude (`FeatureDefs`, `Kalendarze`, `CfgAttributes`,
  `DefinicjeStref` — sprawdzone SQL-em),
- już działających fragmentów innych cech tego repo (np. `.OrderBy`, indeksator
  `Pracownik[Data]`, `KalendModule.GetInstance(Row).Config...`).

Główny element **niesprawdzony bezpośrednio**: użycie typów `FromTimes`/`FromTime` oraz
metody `KalkulatorPracy.NocOkres(ph)` wprost w kodzie cechy (nowość względem wszystkich
dotychczasowych cech w tym repo, które nie sięgały po ten fragment API). Przed wdrożeniem
produkcyjnym: wkleić kod do edytora skryptów w enova na bazie Claude, zapisać i sprawdzić,
czy się kompiluje; jeśli `FromTimes`/`FromTime` nie są widoczne w tym kontekście, zgłosić to
do poprawki (alternatywa: przeliczyć okno nocne ręcznie na `Time`, z obsługą przejścia przez
północ).

Do utworzenia w bazie Claude/testowania potrzebna jest strefa „Praca poza normą awaria” w
`DefinicjeStref` — w bazie Claude jej **nie było** (istniała tylko „Praca poza normą”), więc
została dodana jako klon konfiguracji technicznej strefy „Praca poza normą” (ID 16) pod
nową nazwą — dokładna konfiguracja tej strefy w **produkcyjnej** bazie klienta nie była
dostępna do porównania, więc traktować jako przybliżenie do potwierdzenia.
