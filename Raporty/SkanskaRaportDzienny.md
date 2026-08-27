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
