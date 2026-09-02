# Weryfikator ilości dni okresu rozliczeniowego – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

Weryfikator bliźniaczy do [Weryfikatora normy okresu rozliczeniowego](Weryfikator%20normy%20okresu%20rozliczeniowego.md),
ale zamiast **godzin** sprawdza **liczbę dni wolnych** w okresie rozliczeniowym.

## 1. Do czego służy weryfikator

Przy zapisie kalendarza (planu pracy) pracownika weryfikator sprawdza, czy liczba **dni wolnych**
w bieżącym **okresie rozliczeniowym nadgodzin** zgadza się z liczbą wymaganą przez normę kodeksową.

Wymagana liczba dni wolnych liczona jest jako:

> liczba dni kalendarzowych okresu − liczba dni roboczych normy kodeksowej (`CzasDni.Dni`)

Weryfikator jest zarejestrowany na zapisie **całego kalendarza**, a nie pojedynczego dnia.

### Skąd bierze się liczba dni wolnych „w planie”

- **dni z jawnym wpisem w planie** (użytkownik faktycznie coś tam ustawił) — liczone wprost:
  dzień z zerowym czasem pracy to dzień wolny, dzień z czasem > 0 to dzień pracy,
- **dni bez jawnego wpisu, domyślnie wolne wg kalendarza** (weekendy, święta) — liczone jako wolne,
  dopóki planista jawnie ich nie zmieni,
- **dni bez jawnego wpisu, domyślnie robocze wg kalendarza** — „elastyczne": bez jawnej zmiany
  pozostają dniem pracy, ale planista może je jeszcze zamienić na dzień wolny.

## 2. Dwa tryby działania — okres 1-miesięczny vs wielomiesięczny

Zachowanie weryfikatora zależy od długości okresu rozliczeniowego (liczba miesięcy w okresie
wyliczonym przez `WyliczOkresRoliczeniowyNadgodzin`).

### 2a. Okres jednomiesięczny — tryb ścisły

W okresie 1-miesięcznym **nie ma innego miesiąca do zbilansowania**, więc liczy się plan
**w kształcie, w jakim jest zapisywany**. Dni domyślnie robocze bez jawnej zmiany traktowane są
jako dni pracy.

Błąd zgłaszany jest, **gdy plan jak zapisany nie daje dokładnie wymaganej liczby dni wolnych** —
nawet jeśli teoretycznie zostały jeszcze dni robocze, które planista mógłby zamienić na wolne.

Przykład: okres miesięczny, planista zamienia jedną sobotę i jedną niedzielę na dni pracujące i nic
więcej nie zmienia. Liczba dni wolnych w planie spada o 2 poniżej wymaganej → **błąd**, mimo że
w miesiącu wciąż są dni robocze, które dałoby się „oddać" jako wolne.

### 2b. Okres wielomiesięczny (równoważny) — tryb tolerancyjny

W okresie 3/4/12-miesięcznym bilans dni wolnych i roboczych liczy się **dla całego okresu**, nie dla
pojedynczego miesiąca — pierwsze miesiące mogą być zaplanowane „gęściej" pracą, a odrobione dłuższymi
przerwami w kolejnych. Dopóki pozostałe elastyczne dni robocze pozwalają jeszcze dopiąć bilans,
weryfikator nie przeszkadza.

Błąd zgłaszany jest dopiero, gdy domknięcie wymaganej liczby dni wolnych jest już **matematycznie
niemożliwe**:

- **nadmiar** — same dni już jawnie oznaczone jako wolne przekraczają wymóg całego okresu,
- **niedobór** — nawet zamiana **wszystkich** pozostałych elastycznych dni roboczych na wolne nie
  wystarczy do osiągnięcia wymaganej liczby. Typowa sytuacja: w pierwszych miesiącach okresu
  zaplanowano za dużo pracy (za mało dni wolnych), a planowany jest już ostatni miesiąc i nie ma
  gdzie „nadrobić" dni wolnych.

## 3. Kiedy się uruchamia

Przy zapisie kalendarza (planu pracy) pracownika. Weryfikator działa tylko wtedy, gdy źródłem
planu jest pracownik (`Pracownik`) — dla innych źródeł planu (grupa, stanowisko) nic nie sprawdza.

**Ważne:** jeśli w systemie działa inny (np. wbudowany w Enova) weryfikator planu, oba mogą dawać
osobne komunikaty. Przed testowaniem warto wyłączyć konkurencyjne weryfikatory dla tego samego
zdarzenia.

## 4. Komunikaty

### Okres jednomiesięczny

> Zaplanowano zbyt dużo dni wolnych (X) wobec wymaganej liczby (Y) w okresie rozliczeniowym (okres) - (pracownik). Trzeba dodać dni pracy: (X−Y).

> Za mało dni wolnych (X) wobec wymaganej liczby (Y) w okresie rozliczeniowym (okres) - (pracownik). Trzeba jeszcze zamienić na wolne dni robocze: (Y−X) (dostępnych dni roboczych bez jawnego wpisu: (liczba)).

### Okres wielomiesięczny

> Zaplanowano zbyt dużo dni wolnych (X) wobec wymaganej liczby (Y) w okresie rozliczeniowym (okres) - (pracownik). Nadmiaru nie da się już cofnąć bez zmiany dni jawnie oznaczonych jako wolne.

> Nie da się już osiągnąć wymaganej liczby dni wolnych (Y) w okresie rozliczeniowym (okres), brakuje (Y−max) dni - (pracownik). Pozostało dni roboczych możliwych do zamiany na wolne: (liczba).

Brak błędu (`null`) oznacza, że liczba dni wolnych jest zgodna z wymaganą (tryb ścisły) albo wciąż
matematycznie osiągalna (tryb tolerancyjny).

## 5. Scenariusze testowe

| # | Scenariusz | Oczekiwany wynik | Zweryfikowano na żywo |
|---|---|---|---|
| 5.1 | Okres miesięczny, sobota + niedziela zamienione na dni pracujące, nic więcej | Błąd „za mało dni wolnych", brakuje 2 — mimo że w miesiącu są jeszcze dni robocze do oddania. | Nie |
| 5.2 | Okres miesięczny, plan wprost z kalendarza (weekendy wolne, dni robocze pracujące) | Brak błędu — liczba dni wolnych = wymagana. | Nie |
| 5.3 | Okres miesięczny, jeden dzień roboczy jawnie oznaczony jako wolny, nic więcej | Błąd „zbyt dużo dni wolnych" o 1 — trzeba dodać dzień pracy. | Nie |
| 5.4 | Okres 3-miesięczny, miesiąc 1 z dodatkowym dniem pracy (sobota), miesiące 2–3 nietknięte | Brak błędu — w miesiącach 2–3 wciąż da się zaplanować brakujący dzień wolny. | Nie |
| 5.5 | Okres 3-miesięczny, miesiące 1–2 mocno przeplanowane pracą, planowany miesiąc 3 | Błąd „nie da się już osiągnąć" — sam miesiąc 3, nawet cały wolny, nie nadrobi deficytu dni wolnych z miesięcy 1–2. | Nie |
| 5.6 | Okres 3-miesięczny, jawnie oznaczono jako wolne więcej dni niż wymóg całego okresu | Błąd „nadmiaru nie da się cofnąć". | Nie |

**Uwaga:** żaden scenariusz nie został jeszcze zweryfikowany na żywym środowisku Enova — środowisko
robocze repo nie ma dostępu do DLL/live-testu. Kolumnę należy uzupełnić po testach w GUI.

## 6. Znane ograniczenia / do weryfikacji

- **Dni z nieobecnością.** Dzień z zerowym czasem pracy może wynikać z nieobecności (urlop,
  chorobowe), a nie z celowego dnia wolnego. Weryfikator obecnie tego nie rozróżnia (TODO w kodzie)
  — do rozstrzygnięcia, czy dzień nieobecności ma się liczyć jako dzień wolny, czy być wyłączony
  z rachunku wymaganej liczby.
- **Wymiar etatu < 1.** `KalkulatorKodeksowyPracownika.Norma` zwraca normę kodeksową; nie
  potwierdzono, czy `CzasDni.Dni` jest wtedy skalowane wymiarem etatu. Dla niepełnoetatowca
  wymagana liczba dni wolnych może wyjść zaniżona — do sprawdzenia.
- **Dni bez etatu w okresie.** Jeśli okres wyliczony przez `WyliczOkresRoliczeniowyNadgodzin`
  obejmuje dni poza faktycznym okresem zatrudnienia, wynik `normaKodeksowa.Dni` może być mylący.
- **Materializacja planu przy zapisie.** To, czy zapis planu tworzy jawne rekordy dni tylko dla
  dni zmienionych, czy dla całego widocznego miesiąca, wpływa na podział dni na „jawne" vs
  „elastyczne". W trybie ścisłym (okres miesięczny) nie zmienia to wyniku scenariusza 5.1, ale
  w trybie tolerancyjnym może. Do potwierdzenia w GUI.
