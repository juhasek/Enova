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

**Objaw:** przy drukowaniu raportu dla wielu pracowników jednocześnie kolumna
podsumowania **RazemNad** (suma nadgodzin) pokazywała wartość narastającą
między pracownikami — np. jeśli pierwszy pracownik miał 5 nadgodzin, a drugi
2, to u drugiego pracownika raport pokazywał 7.

**Przyczyna:** zmienne pomocnicze służące do sumowania w obrębie karty
jednego pracownika (`nadlicz`, `nocne`, `czas`, `karmienie`, `nadZP`,
`nadDoP`, `swieta`, `wolne` i pozostałe pola typu `Time` zadeklarowane przy
`Grid1ListaWiersz_BeforePrint`) są polami całej instancji raportu. Raport
jest jednak jedną instancją obsługującą wszystkich wydrukowanych
pracowników — bez jawnego wyzerowania tych pól przy przejściu do kolejnego
pracownika, ich wartości sumowały się „w poprzek” pracowników zamiast
liczyć się od nowa dla każdego z nich.

**Poprawka:** w `detailReportBand1_BeforePrint` (metoda wywoływana raz na
każdego drukowanego pracownika, przed przygotowaniem jego wierszy dziennych)
dodano jawne wyzerowanie wszystkich tych akumulatorów do `Time.Zero` na
początku przetwarzania danego pracownika. Dzięki temu każda karta zaczyna
liczenie nadgodzin (i pozostałych sum) od zera, niezależnie od wyników
poprzednich pracowników na tym samym wydruku.
