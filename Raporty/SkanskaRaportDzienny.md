# Raport dzienny (Skanska) – karta ewidencji czasu pracy

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy raport

Raport drukuje „Kartę ewidencji czasu pracy” za wskazany miesiąc, osobno dla
każdego wybranego pracownika. Dla każdego dnia miesiąca pokazuje m.in.
godziny „od–do”, harmonogram z planu, czas nocny, nieobecność, informacje
o strefie pracy (praca zdalna, dodatek brygadzisty, projekt/zadanie/MPK) oraz
nadgodziny naliczone w danym dniu. Na końcu karty pracownika wyświetlane są
podsumowania miesięczne: norma godzin, przepracowane godziny, godziny nocne
oraz suma nadgodzin (**RazemNad**).

Raport uwzględnia tylko pracowników, dla których w wybranym miesiącu
występuje okres zatrudnienia z niezerową normą czasu pracy.

## 2. Parametry raportu

| Parametr | Opis |
|---|---|
| **Miesiąc** (wymagany) | Miesiąc, za który drukowana jest karta ewidencji. |

## 3. Poprawka: błędna suma w kolumnie „RazemNad”

**Objaw 1:** przy drukowaniu raportu dla wielu pracowników jednocześnie kolumna
podsumowania **RazemNad** (suma nadgodzin) pokazywała wartość narastającą
między pracownikami — np. jeśli pierwszy pracownik miał 5 nadgodzin, a drugi
2, to u drugiego pracownika raport pokazywał 7.

**Przyczyna 1:** zmienne pomocnicze służące do sumowania w obrębie karty
jednego pracownika (`nadlicz`, `nocne`, `czas`, `karmienie`, `nadZP`,
`nadDoP`, `swieta`, `wolne` i pozostałe pola typu `Time` zadeklarowane przy
`Grid1ListaWiersz_BeforePrint`) są polami całej instancji raportu. Raport
jest jednak jedną instancją obsługującą wszystkich wydrukowanych
pracowników — bez jawnego wyzerowania tych pól przy przejściu do kolejnego
pracownika, ich wartości sumowały się „w poprzek” pracowników zamiast
liczyć się od nowa dla każdego z nich.

**Poprawka 1:** w `detailReportBand1_BeforePrint` (metoda wywoływana raz na
każdego drukowanego pracownika, przed przygotowaniem jego wierszy dziennych)
dodano jawne wyzerowanie wszystkich tych akumulatorów do `Time.Zero` na
początku przetwarzania danego pracownika.

**Objaw 2 (ujawnił się po poprawce 1):** pracownik bez żadnych nadgodzin w
danym miesiącu pokazywał w kolumnie **RazemNad** wartość nadgodzin
**poprzedniego** wydrukowanego pracownika (np. pracownik 1 ma 28 nadgodzin,
pracownik 2 nie ma żadnych, ale w raporcie u pracownika 2 nadal widnieje 28;
kolejny pracownik z realnymi nadgodzinami, np. 12, pokazuje już poprawną
wartość).

**Przyczyna 2:** `RazemNad.Text` jest ustawiane wyłącznie wewnątrz
`Grid1ListaWiersz_BeforePrint`, w bloku wykonywanym tylko dla dni, w których
wystąpiła strefa z listy `strefynadgodzin`. Jeśli pracownik w całym miesiącu
nie ma ani jednego takiego dnia, przypisanie do `RazemNad.Text` nigdy się nie
wykonuje — a ponieważ komórka raportu jest tym samym obiektem UI używanym dla
wszystkich pracowników, zachowuje ona tekst wydrukowany ostatnio, czyli wynik
poprzedniego pracownika. Samo wyzerowanie zmiennej `nadlicz` (poprawka 1) nie
wystarcza, bo nie ma to wpływu na już wydrukowany tekst komórki.

**Poprawka 2:** w `detailReportBand1_BeforePrint`, zaraz po zresetowaniu
`nadlicz`, dodano `RazemNad.Text = nadlicz.ToString();` — czyli jawne
ustawienie komórki na wartość startową (zero) dla każdego pracownika, zanim
zaczną się drukować jego wiersze dzienne. Jeśli w trakcie drukowania trafi
się dzień z nadgodzinami, wartość zostanie poprawnie nadpisana; jeśli nie —
zostanie poprawne zero zamiast wyniku poprzedniego pracownika.

Przy okazji poprawiono formatowanie/wcięcia całego pliku na standardowe dla C#.

## 4. Poprawka: błędne godziny „od–do” przy kilku strefach w jednym dniu

**Objaw:** gdy w danym dniu pracownik miał więcej niż jedną strefę (np. „Praca
w normie” 7:00–15:00 i „Praca poza normą” 15:00–18:00 jako nadgodziny), raport
drukował dla obu wierszy tego samego dnia identyczne godziny „Godz. pracy
od/do” — pełny zakres całego dnia (np. 7:00–18:00) zamiast osobnego przedziału
dla każdej strefy. W efekcie karta pokazywała dwa (lub więcej) pozornie
zdublowane wiersze dla tej samej daty, a nadgodziny w kolumnie były trudne do
zweryfikowania względem godzin „od–do”.

**Przyczyna:** w `Grid1ListaWiersz_BeforePrint`, w gałęzi obsługującej wiersz
z przypisaną strefą (`linia.Strefa != null`), kolumny `colOd`/`colDo` były
liczone z `dzień.OdGodziny` / `Dzien.DoGodziny(dzień)` — czyli z
zagregowanego, wspólnego dla całego dnia obiektu `Dzien`, a nie z przedziału
konkretnej strefy (`linia.Strefa`) reprezentowanej przez dany wiersz.
Obliczenie pory nocnej (`PoliczPoreNocna`) tuż niżej już poprawnie używało
`linia.Strefa.OdGodziny`/`linia.Strefa.Czas` — tylko godziny „od–do” tego nie
robiły.

**Poprawka:** `colOd.Text` ustawiane jest teraz na `linia.Strefa.OdGodziny`, a
`colDo.Text` na `linia.Strefa.OdGodziny + linia.Strefa.Czas` — czyli
rzeczywisty przedział czasowy strefy danego wiersza. Dzięki temu dzień z
kilkoma strefami drukuje kilka wierszy z rozłącznymi przedziałami godzin (np.
7:00–15:00, 15:00–16:00, 16:00–18:00) zamiast powtarzania godzin całego dnia
w każdym wierszu.

## 5. Scalanie wielu zapisów tej samej strefy z tym samym Projektem/Taskiem

**Objaw:** dla dnia z kilkoma osobnymi zapisami strefy tego samego typu (np.
dwa zapisy „Praca poza normą” o różnych godzinach — 15:00–16:00 i
16:00–18:00), ale z tymi samymi wartościami cech **Projekt** i **Task**, karta
drukowała osobny wiersz dla każdego zapisu. Wizualnie wyglądało to jak
zdublowane wiersze tego samego dnia, a kolumna nadgodzin (licząca się jako
suma dla całego dnia — patrz `PracaWStrefie`) była dodatkowo powielana przy
każdym takim wierszu, co zawyżało odczyt.

**Przyczyna:** budowanie listy wierszy (`linie`) w
`detailReportBand1_BeforePrint` tworzyło jeden obiekt `Linia` na każdy
pojedynczy rekord strefy z `dzienPracy.Strefy`, bez sprawdzania, czy kolejny
zapis tego samego dnia nie jest w istocie kontynuacją poprzedniego (ten sam
typ strefy, ten sam Projekt, ten sam Task, inne tylko godziny).

**Poprawka:** przy budowaniu `linie`, zapisy strefy danego dnia są sortowane
po godzinie rozpoczęcia; jeśli kolejny zapis ma ten sam typ strefy
(`Definicja.Nazwa`) oraz te same wartości cech **Projekt** i **Task** co
poprzednio dodany wiersz tego dnia, nie tworzy się dla niego nowy wiersz —
zamiast tego rozszerza się godzinę zakończenia poprzedniego wiersza
(`Linia.DoGodzinyLaczna`, nowe pole, domyślnie `OdGodziny + Czas` pojedynczej
strefy, dla scalonych wierszy — maksimum z dotychczasowego i nowego końca).
Kolumny „Godz. pracy od/do” oraz obliczenie pory nocnej
(`PoliczPoreNocna`) korzystają teraz z `Linia.DoGodzinyLaczna` zamiast
liczyć koniec bezpośrednio z pojedynczej strefy. Zapisy o innym typie strefy
(np. „Praca w normie”) albo innym Projekcie/Tasku nadal drukują się jako
osobne wiersze — scalane są wyłącznie zapisy identyczne pod względem typu
strefy i Projektu/Tasku.

## 6. Poprawka: nadgodziny liczone z sumy dnia zamiast z własnego wiersza

**Objaw:** gdy dzień miał kilka odrębnych zapisów nadgodzin (np. strefa
„Praca poza normą” w dwóch kawałkach: 1h i 2h), kolumna „Nadgodziny” nie
pokazywała 1h przy pierwszym i 2h przy drugim zapisie — pokazywała 3h przy
obu. Ponieważ wartość ta jest też dodawana do akumulatora `nadlicz`
(a stąd do sumy miesięcznej `RazemNad`), dzienny wynik doliczał się do sumy
tyle razy, ile było takich wierszy tego dnia — zawyżając miesięczne
nadgodziny.

**Przyczyna:** wartość wpisywana do `colNadlicz.Text`/`nadlicz` pochodziła z
`pracaPozaNorma = PracaWStrefie(dzień, ..."Praca poza normą"..., false)` —
funkcji sumującej czas strefy „Praca poza normą” dla **całego dnia**
(obiekt `Dzien`), a nie tylko dla zapisu/wiersza, który akurat jest
drukowany. Każdy wiersz z zapisem nadgodzin tego dnia dostawał więc tę samą,
dzienną sumę zamiast swojej własnej wartości.

**Poprawka:** kolumna „Nadgodziny” oraz akumulator `nadlicz` korzystają
teraz z `linia.CzasLaczny` — czasu należącego wyłącznie do danego wiersza
(sumy Czas zapisów scalonych w ten wiersz zgodnie z poprawką z punktu 5, a
dla niescalonego wiersza po prostu Czas jego pojedynczej strefy). Dzięki
temu: (a) różne, niescalone zapisy nadgodzin tego samego dnia poprawnie
pokazują swoje własne wartości (1h i 2h) zamiast powielonej sumy dnia (3h),
(b) zapisy scalone w jeden wiersz (bo ten sam typ strefy i ten sam
Projekt/Task) poprawnie pokazują sumę tylko swoich godzin, oraz (c)
`nadlicz`/`RazemNad` nie jest już wielokrotnie zawyżane przez powtórne
doliczanie tej samej dziennej sumy przy każdym wierszu nadgodzin danego dnia.
Nieużywane już po tej zmianie wyliczenie `pracaPozaNorma` (dzienna suma przez
`PracaWStrefie`) zostało usunięte.

## 7. Poprawka: dzień z nietypową nazwą strefy pracy zdalnej znikał z raportu

**Objaw:** pracownik, który zamiast „Praca w normie” miał danego dnia strefę
pracy zdalnej o nazwie spoza zamkniętej listy (np. „Praca zdalna
uprzywilejowana”), w ogóle nie miał wydrukowanego tego dnia w karcie — wiersz
całkowicie znikał z raportu.

**Przyczyna:** budowanie listy wierszy (`linie`) w `detailReportBand1_BeforePrint`
filtrowało zapisy strefy danego dnia przez `strefyraportu.Contains(s.Definicja.Nazwa)`
— porównanie z zamkniętą listą dokładnych nazw stref. Lista wymieniała tylko
trzy konkretne warianty pracy zdalnej („Praca zdalna”, „Praca zdalna
regulaminowa”, „Praca zdalna okazjonalna”). Podobnie flaga „Praca zdalna”
(kolumna `pracazdalna`) sprawdzana była przez `strefyPracyZdalnej.Contains(...)`
— osobną, też zamkniętą listę tych samych trzech nazw. Gdy w danych pojawiał
się jakikolwiek inny wariant nazwy (np. dopisany później „Praca zdalna
uprzywilejowana”), żaden z zapisów strefy tego dnia nie przechodził filtra —
a ponieważ `dzienPracy.Strefy.Any` było prawdą (strefa jednak istniała), kod
nie wchodził też w gałąź dodającą pusty wiersz dnia bez strefy. Dzień znikał
całkowicie.

**Poprawka:** rozpoznawanie strefy pracy zdalnej nie opiera się już o
zamkniętą listę nazw, tylko o nową metodę `JestStrefaZdalna(nazwa)`, która
uznaje strefę za zdalną, gdy jej nazwa zawiera (bez rozróżniania wielkości
liter) słowo „zdalna”. Filtr budujący `linie` oraz flaga kolumny „Praca
zdalna” korzystają teraz z tej metody zamiast z tablic
`strefyPracyZdalnej`/wpisów „Praca zdalna…” w `strefyraportu` (tablica
`strefyPracyZdalnej` została usunięta jako zbędna, a jej trzy wpisy wykreślone
z `strefyraportu`). Dzięki temu każdy przyszły wariant nazwy strefy pracy
zdalnej (np. „Praca zdalna uprzywilejowana”) jest automatycznie rozpoznawany
bez konieczności edycji kodu raportu.
