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
  pozostają dniem pracy, ale planista może je jeszcze zamienić na dzień wolny. Za elastyczne
  uznawane są **tylko dni z miesięcy wciąż otwartych do edycji** (patrz „horyzont edycji" niżej) —
  dzień roboczy z zamkniętego, minionego miesiąca jest już dniem pracy na stałe.

### Horyzont edycji

Weryfikator wyznacza **najwcześniejszy miesiąc, który wciąż można modyfikować**:

- jeśli ustawiona jest **cecha globalna `BlokadaOdDnia`** (ta sama, której używa weryfikator
  „Blokada modyfikacji czasu pracy wstecz") — horyzont liczony jest tak samo jak tam (dzień progu
  w bieżącym miesiącu decyduje, czy otwarty jest jeszcze miesiąc poprzedni),
- jeśli cecha nie jest ustawiona — przyjmowany jest **1. dzień bieżącego miesiąca** (miesiące
  minione i tak są zwykle domknięte naliczonymi wypłatami).

Miesiące przed horyzontem są „zamknięte": ich dni roboczych bez jawnego wpisu nie da się już
zamienić na wolne, więc weryfikator nie wlicza ich do zapasu elastyczności.

## 2. Dwa tryby działania — ścisły vs tolerancyjny

### Tryb ścisły

Liczy się plan **w kształcie, w jakim jest zapisywany**: dni domyślnie robocze bez jawnej zmiany
są dniami pracy. Błąd zgłaszany jest, **gdy plan jak zapisany nie daje dokładnie wymaganej liczby
dni wolnych** — nawet jeśli teoretycznie zostały jeszcze dni robocze, które planista mógłby
zamienić na wolne.

Tryb ścisły włącza się, gdy **nie ma już późniejszego miesiąca, do którego dałoby się odłożyć
bilans**:

- **okres jednomiesięczny** — z definicji nie ma innego miesiąca, albo
- **ostatni otwarty miesiąc okresu wielomiesięcznego** — okres nie sięga już poza horyzont edycji,
  wcześniejsze miesiące są zamknięte i nie da się w nich oddać zabranych dni wolnych.

Przykład (jednomiesięczny): planista zamienia jedną sobotę i jedną niedzielę na dni pracujące i nic
więcej nie zmienia → **błąd**, brakuje 2 dni wolnych, mimo że w miesiącu wciąż są dni robocze do
oddania.

Przykład (ostatni miesiąc): okres lipiec–wrzesień, w lipcu wszystkie soboty zamienione na pracę,
sierpień bez zmian, jest wrzesień i lipiec/sierpień są już zamknięte. Zapisując wrzesień bez oddania
zabranych w lipcu dni wolnych → **błąd**.

### Tryb tolerancyjny

W okresie wielomiesięcznym, dopóki **są jeszcze późniejsze miesiące** do zbilansowania, bilans dni
wolnych i roboczych liczy się **dla całego okresu**, nie dla pojedynczego miesiąca — pierwsze
miesiące mogą być zaplanowane „gęściej" pracą, a odrobione w kolejnych. Dopóki pozostałe elastyczne
dni robocze (z otwartych miesięcy) pozwalają jeszcze dopiąć bilans, weryfikator nie przeszkadza.

Błąd zgłaszany jest dopiero, gdy domknięcie wymaganej liczby dni wolnych jest już **matematycznie
niemożliwe**:

- **nadmiar** — same dni już jawnie oznaczone jako wolne przekraczają wymóg całego okresu,
- **niedobór** — nawet zamiana **wszystkich** elastycznych dni roboczych (z otwartych miesięcy) na
  wolne nie wystarczy do osiągnięcia wymaganej liczby.

## 3. Kiedy się uruchamia

Przy zapisie kalendarza (planu pracy) pracownika. Weryfikator działa tylko wtedy, gdy źródłem
planu jest pracownik (`Pracownik`) — dla innych źródeł planu (grupa, stanowisko) nic nie sprawdza.

**Ważne:** jeśli w systemie działa inny (np. wbudowany w Enova) weryfikator planu, oba mogą dawać
osobne komunikaty. Przed testowaniem warto wyłączyć konkurencyjne weryfikatory dla tego samego
zdarzenia.

## 4. Komunikaty

### Tryb ścisły

> Zaplanowano zbyt dużo dni wolnych (X) wobec wymaganej liczby (Y) w okresie rozliczeniowym (okres) - (pracownik). Trzeba dodać dni pracy: (X−Y).

> Za mało dni wolnych (X) wobec wymaganej liczby (Y) w okresie rozliczeniowym (okres) - (pracownik). Trzeba jeszcze zamienić na wolne dni robocze: (Y−X) (dostępnych dni roboczych bez jawnego wpisu: (liczba)).

### Tryb tolerancyjny

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
| 5.4 | Okres 3-miesięczny, jesteśmy w miesiącu 1, miesiąc 1 z dodatkowym dniem pracy (sobota), miesiące 2–3 nietknięte | Brak błędu (tryb tolerancyjny) — w miesiącach 2–3 wciąż da się zaplanować brakujący dzień wolny. | Nie |
| 5.5 | Okres lipiec–wrzesień, jesteśmy we wrześniu, w lipcu wszystkie soboty zamienione na pracę, sierpień bez zmian, lipiec/sierpień zamknięte, we wrześniu brak dni wolnych oddających te z lipca | Błąd „za mało dni wolnych" (tryb ścisły — wrzesień to ostatni otwarty miesiąc) o liczbę sobót zabranych w lipcu. | Nie |
| 5.6 | Okres 3-miesięczny, miesiące 1–2 mocno przeplanowane pracą, jesteśmy w miesiącu 3 (ostatnim), nawet cały wolny nie nadrobi deficytu | Błąd — tryb ścisły; brak dni wolnych do domknięcia normy. | Nie |
| 5.7 | Okres 3-miesięczny, jesteśmy w miesiącu 1, jawnie oznaczono jako wolne więcej dni niż wymóg całego okresu | Błąd „nadmiaru nie da się cofnąć" (tryb tolerancyjny). | Nie |

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
  „elastyczne". W trybie ścisłym nie zmienia to wyniku scenariuszy 5.1 / 5.5, ale w trybie
  tolerancyjnym może. Do potwierdzenia w GUI.
- **Wyznaczanie horyzontu edycji.** Horyzont opiera się na cesze globalnej `BlokadaOdDnia` lub —
  gdy jej brak — na 1. dniu bieżącego miesiąca (`Date.Today`). To heurystyka „miniony miesiąc jest
  domknięty", nie odczyt faktycznego stanu (np. czy istnieje naliczona, niebuforowa wypłata za dany
  miesiąc). Jeśli potrzebna jest twardsza reguła, sygnałem powinno być istnienie wypłaty
  (`pracownik.Wyplaty` z `Bufor == false`) obejmującej miesiąc — do rozważenia.
- **Znaczenie parametru `data`.** Rozróżnienie trybu ścisły/tolerancyjny opiera się na `Date.Today`
  i miesiącach okresu, nie na `data` przekazanym do weryfikatora — dzięki temu działa niezależnie
  od tego, czy `data` to edytowany dzień, czy początek okresu. Gdyby przyjąć, że `data` to zawsze
  edytowany dzień, można by rozróżniać precyzyjniej (miesiąc zapisu vs miesiące późniejsze).
