# Weryfikator niedoboru normy okresu rozliczeniowego – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

- **ID definicji (`DefWeryfKalend`):** 40
- **GUID:** `0015552D-B8E6-4609-8928-FC98CCC0F35B`
- **Nazwa w enova:** `Niedobór normy okresu rozliczeniowego`
- **Rodzaj weryfikatora:** `Plan pracy` (`RodzajWeryfikacjiKalendarza.Plan` = 40) – weryfikator
  poziomu **pracownika**, nie pojedynczego dnia.
- **Priorytet:** 8 (kontynuacja numeracji wbudowanego katalogu Enova: 1–7 to standardowe
  weryfikatory Kadr i Płac, patrz [pkt 2](#2-kiedy-się-uruchamia)).
- **Blokada:** `False` (definicja nie jest zablokowana/wyłączona).
- **Zgłoszenie:** ZS/2026/16174.
- **Zweryfikowano na żywo:** Nie – środowisko robocze repo nie ma dostępu do GUI/DLL klienta
  (tylko baza `Claude` i SQL, patrz pamięć repo). Analiza oparta o kod z bazy + dekompilację
  `Soneta.KadryPlace.dll` (2512.5.6).
- **Powiązanie z kalendarzem (`WeryfKalend`):** brak – definicja **nie jest jeszcze podpięta**
  do żadnego kalendarza. Sama obecność wiersza w `DefWeryfKalend` nic nie sprawdza w praktyce,
  dopóki nie zostanie dodana do konkretnego kalendarza z `Typ` (poziom: Error/Warning/Info) –
  patrz [pkt 3](#3-konfiguracja).

## 1. Do czego służy weryfikator

Weryfikator dopilnowuje, aby w trakcie planowania **równoważnego okresu rozliczeniowego**
(1, 3, 4 lub 12-miesięcznego) nie doszło do sytuacji, w której zaplanowano **stanowczo za mało**
godzin w miesiącach już „za nami” w obrębie okresu, a w pozostałych miesiącach tego okresu **nie da
się już tego nadrobić** – nawet przy najbardziej intensywnym możliwym planowaniu.

Uzupełnia **wbudowany** weryfikator enova „Norma w okresie rozliczeniowym”
(`WeryfikatorKalendarza.WeryfikujNormeWOkresieRozliczeniowym`), który sprawdza **wyłącznie
nadmiar** (czy suma zaplanowanych godzin w całym okresie nie przekracza normy kodeksowej) –
nie reaguje w ogóle, jeśli w okresie zaplanowano za mało. Ten weryfikator domyka drugą stronę tego
samego problemu.

**Uwaga:** w repozytorium istnieje też odrębny, bardziej rozbudowany customowy weryfikator
o (mylącej) tej samej nazwie handlowej – patrz [Weryfikator normy okresu rozliczeniowego](Weryfikator%20normy%20okresu%20rozliczeniowego.md)
(rozróżnia dni „jawnie wpisane” od „elastycznych” i sam w sobie wykrywa też niedobór). Ten kod
**nie jest obecnie zaimportowany do bazy `Claude`** (nie ma go wśród definicji `Rodzaj=0`) –
wygląda na wcześniejszą, niewdrożoną koncepcję tego samego problemu. Opis definicji ID 40 w bazie
(„standardowy weryfikator … wykrywa wyłącznie nadmiar”) wskazuje, że przy tworzeniu tego
weryfikatora punktem odniesienia był **wbudowany** weryfikator Enova, a nie ten dokument. Jeśli
oba mechanizmy miałyby kiedyś działać jednocześnie, mogłyby dawać dwa niezależne komunikaty o
niedoborze dla tej samej sytuacji – do wyjaśnienia z klientem, zanim któryś z nich zostanie
faktycznie podpięty do kalendarza produkcyjnie.

### Logika w skrócie

Przy każdym zapisie dnia planu enova sprawdza – dla miesiąca, w którym leży edytowany dzień –
czy suma godzin zaplanowanych **od początku okresu rozliczeniowego do końca tego miesiąca**
osiąga normę kodeksową za ten sam zakres:

- jeśli zaplanowano **co najmniej tyle, ile wynosi norma kodeksowa** (lub więcej) – brak błędu
  (nadmiarem zajmuje się inny weryfikator),
- jeśli jest **niedobór**, sprawdzane jest, czy da się go jeszcze nadrobić w pozostałych miesiącach
  okresu: teoretyczne maksimum liczone jest jako **jeden pełny dobowy wymiar etatu za każdy
  dzień roboczy** (bez sobót, niedziel i świąt) pozostałych miesięcy, pomniejszone o normę
  kodeksową tych miesięcy (czyli „ile ponad normę kodeksową da się maksymalnie jeszcze
  zaplanować”),
- jeśli **niedobór jest większy niż ta teoretyczna rezerwa** – błąd. W przeciwnym razie – brak
  błędu (nadrobienie jest wciąż matematycznie możliwe).

**Ważna różnica względem weryfikatora „Norma” z p. 4.6 opisanego wyżej dokumentu:** ten weryfikator
liczy „ile zaplanowano” jako **zwykłą sumę z aktualnego stanu kalendarza planu** (`KalkulatorPlanu.Norma`)
– bez rozróżniania dni **jawnie wpisanych przez planistę** od dni **wciąż dziedziczących wartość
domyślną z kalendarza wzorcowego**. Dla miesięcy, których nikt jeszcze nie dotknął, wynik zwykle
odpowiada domyślnemu rozkładowi z kalendarza wzorcowego (nie zeru) – patrz [pkt 4](#4-scenariusze-testowe),
scenariusz dotyczący miesięcy nietkniętych.

## 2. Kiedy się uruchamia

Weryfikator jest zarejestrowany jako `Rodzaj = Plan pracy` (`RodzajWeryfikacjiKalendarza.Plan`).
Ten rodzaj weryfikatorów uruchamia się **automatycznie przy zapisie każdego pojedynczego dnia
planu pracy** (`Soneta.Kalend.DzienPlanu.OnVerify()`) – enova dodaje `KontrolaPlanuVerifier` do
kolejki weryfikatorów **bezwarunkowo**, niezależnie od ustawienia „Kontrola planu”
(`Config.Ogólne.KontrolaPlanu` – to ustawienie gates tylko dwa inne, wbudowane weryfikatory:
wymaganą przerwę i dobę pracowniczą). Zachowanie potwierdzone dekompilacją, analogicznie do
ustalenia z dokumentacji weryfikatora [11-godzinnego odpoczynku](11-sto%20godzinny%20odpoczynek.md).

Miesiąc przekazywany do weryfikatora (`args.Miesiąc`) to zawsze miesiąc kalendarzowy edytowanego
dnia (`YearMonth(dzień.Data)`) – nie miesiąc bieżący systemowy ani początek okresu rozliczeniowego.
Oznacza to, że **edycja dowolnego dnia w dowolnym miesiącu okresu** uruchamia sprawdzenie „od
początku okresu do końca tego miesiąca” – łącznie z edycją dnia w środku okresu, nie tylko na jego
końcu.

Jeśli dla edytowanego dnia `pracownik.WyliczOkresRoliczeniowyNadgodzin(...)` zwraca pusty okres
(np. kalendarz bez zdefiniowanego okresu rozliczeniowego nadgodzin) – weryfikator nic nie sprawdza.

## 3. Konfiguracja

- Definicja istnieje w bazie (`DefWeryfKalend`, ID 40), ale **nie jest jeszcze przypisana do
  żadnego kalendarza** (`WeryfKalend` – brak wierszy z `Definicja = 40`). Dopóki ktoś nie doda jej
  do konkretnego kalendarza (`Kalendarz → Weryfikatory`, analogicznie do konfiguracji opisanej w
  dokumentacji weryfikatora 11h) i nie ustawi tam poziomu (`Typ`: Error/Warning/Info), weryfikator
  **nie działa w praktyce** mimo obecności w bazie.
- Weryfikator nie ma własnych parametrów ustawianych przez użytkownika – korzysta z danych już
  zapisanych w kartotece pracownika: bieżącego planu, historii etatów (`Historia`/`pracownik[data]`)
  oraz kalendarza (okres rozliczeniowy nadgodzin, norma dobowa etatu).
- **Ostrzeżenie o edytorze skryptów:** kod zawiera osobną metodę pomocniczą
  (`NormaTeoretycznaBezSwiat`) poza główną metodą `Weryfikuj`. Wg wcześniejszych obserwacji w tym
  repo (patrz pamięć projektu) wbudowany edytor skryptów enova bywa zawodny przy takiej
  konstrukcji (choć kod trafił do bazy poza edytorem, przez import/SQL, więc mógł tego ominąć) –
  jeśli ktoś otworzy i zapisze tę definicję przez GUI, warto od razu sprawdzić, czy kompilacja
  nadal przechodzi.

## 4. Komunikat błędu

> Zaplanowano za malo godzin do miesiaca {miesiąc} (brakuje {niedobór}); w pozostalych miesiacach
> okresu rozliczeniowego {pozostały zakres} maksymalna mozliwa kompensacja to {maks. kompensacja} -
> {pracownik}

Pojawia się wyłącznie, gdy niedobór godzin narastający od początku okresu rozliczeniowego do końca
edytowanego miesiąca **przewyższa teoretyczną maksymalną rezerwę** możliwą do zaplanowania w
pozostałych miesiącach tego okresu.

**Uwaga:** treść komunikatu (i opisu definicji w bazie) jest zapisana **bez polskich znaków
diakrytycznych** („malo”, „biezacego” itp.) – w przeciwieństwie do identyfikatorów w kodzie, gdzie
polskie znaki (`Miesiąc`) są poprawne. To wygląda na to, jak faktycznie wpisano tekst w edytorze
(nie na błąd kodowania przy odczycie – zweryfikowano dwoma niezależnymi metodami odczytu,
`sqlcmd -f 65001` i ADO.NET/PowerShell, wynik identyczny). Do rozważenia poprawka przed
produkcyjnym użyciem, jeśli komunikat ma być prezentowany użytkownikom końcowym.

Brak komunikatu (`null`) oznacza, że narastający niedobór (o ile istnieje) jest wciąż matematycznie
możliwy do nadrobienia w pozostałych miesiącach okresu, albo że zaplanowano już wystarczająco dużo
godzin.

## 5. Scenariusze testowe

Pełna lista scenariuszy (z kolumnami na wynik testu w GUI) w arkuszu:
[Scenariusze testowe weryfikatorow czasu pracy.xlsx](Scenariusze%20testowe%20weryfikatorow%20czasu%20pracy.xlsx),
arkusz **Niedobór normy okresu rozliczeniowego**. Poniższa tabela jest kopią poglądową – żaden
scenariusz nie został zweryfikowany na żywo (patrz nagłówek dokumentu).

| Lp | Scenariusz | Dane wejściowe | Oczekiwany wynik |
|---|---|---|---|
| 1 | Okres jednomiesięczny, plan dokładnie równy normie kodeksowej | Okres = 1 miesiąc; zaplanowano dokładnie tyle godzin, ile wynosi norma kodeksowa miesiąca | Brak błędu (`deficyt <= 0`) |
| 2 | Okres jednomiesięczny (= ostatni miesiąc okresu), realny niedobór | Okres = 1 miesiąc; zaplanowano wyraźnie mniej niż norma kodeksowa | Błąd – nie ma już żadnego kolejnego miesiąca do kompensacji (`maxKompensacja = 0`), każdy dodatni niedobór skutkuje błędem |
| 3 | Okres 3-miesięczny, edycja miesiąca 1, duży niedobór, ale możliwy do odrobienia | Okres 3-mies.; miesiąc 1 mocno niedoplanowany; miesiące 2–3 bez zmian (domyślny rozkład z kalendarza wzorcowego, pełne dni robocze) | Brak błędu — niedobór ≤ teoretyczna rezerwa z miesięcy 2–3 (pełny wymiar dobowy × dni robocze − norma kodeksowa tych miesięcy) |
| 4 | Okres 3-miesięczny, edycja miesiąca 1, niedobór niemożliwy do odrobienia | Okres 3-mies.; miesiąc 1 wyzerowany (0 godzin) lub prawie wyzerowany, tak by niedobór przekroczył teoretyczną rezerwę miesięcy 2–3 | Błąd — z podaną w komunikacie wartością niedoboru i maksymalnej kompensacji, możliwą do ręcznej weryfikacji |
| 5 | Okres 3-miesięczny, edycja ostatniego miesiąca (miesiąc 3), skumulowany niedobór z całego okresu | Okres 3-mies.; miesiące 1–2 niedoplanowane, edycja dowolnego dnia w miesiącu 3 | Błąd, jeśli skumulowany niedobór (okres.From → koniec miesiąca 3) > 0 — `maxKompensacja = 0`, bo `startPo > okres.To` |
| 6 | Granica: niedobór dokładnie równy maksymalnej kompensacji | Okres wielomiesięczny; niedobór miesiąca bieżącego == maksymalna teoretyczna rezerwa pozostałych miesięcy (co do minuty) | Brak błędu — warunek to `deficyt > maxKompensacja` (ostry), więc równość nie generuje błędu |
| 7 | Zwolnienie pracownika w trakcie pozostałych miesięcy okresu | Okres wielomiesięczny; niedobór w miesiącu bieżącym; etat pracownika kończy się w trakcie miesięcy 2–3 (przed końcem okresu) | Dni po dacie zwolnienia nie liczą się do teoretycznej rezerwy (`pracownik[d]` lub `.Etat` = null → dzień pomijany) — mniejsza rezerwa niż w scenariuszu bez zwolnienia, błąd pojawia się przy mniejszym niedoborze niż w scenariuszu 3 |
| 8 | Święto ustawowe w pozostałych miesiącach okresu | Okres wielomiesięczny z niedoborem; w pozostałych miesiącach występuje dzień ustawowo wolny wypadający w dzień roboczy | Dzień świąteczny pomijany w `NormaTeoretycznaBezSwiat` (nie dolicza normy dobowej) — teoretyczna rezerwa mniejsza o jedną normę dobową za każde takie święto |
| 9 | Miesiąc jeszcze nietknięty przez planistę | Okres wielomiesięczny; edycja dnia w miesiącu 1, miesiące 2–3 bez żadnej ręcznej ingerencji | „Zaplanowano” dla miesięcy 2–3 liczone jest z bieżącego stanu kalendarza (zwykle = domyślny rozkład z kalendarza wzorcowego), nie z zera — inaczej niż w dokumentacji [Weryfikator normy okresu rozliczeniowego](Weryfikator%20normy%20okresu%20rozliczeniowego.md) |
| 10 | Edycja pojedynczego dnia w środku okresu (nie na końcu miesiąca) | Okres wielomiesięczny; zmiana jednego dnia roboczego w środku miesiąca 1 | Weryfikator i tak uruchamia się (Rodzaj=Plan działa na każdym zapisie dnia planu) i liczy zakres „od początku okresu do końca **miesiąca**, w którym leży edytowany dzień” — nie tylko do edytowanego dnia |
| 11 | Definicja nie podpięta do kalendarza (stan bieżący w bazie `Claude`) | Dowolny zapis dnia planu na dowolnym kalendarzu | Brak jakiegokolwiek efektu — `WeryfKalend` nie ma wiersza z `Definicja = 40`, więc `GetWeryfikatory(RodzajWeryfikacjiKalendarza.Plan)` nigdy nie zwróci tej definicji |

## 6. Ograniczenia / do potwierdzenia

- **Brak polskich znaków w komunikacie** – patrz [pkt 4](#4-komunikat-błędu). Kosmetyczne, ale
  widoczne dla użytkownika końcowego.
- **Definicja nie jest podpięta do żadnego kalendarza** – bez tego kroku weryfikator jest
  „martwy” mimo obecności w bazie (scenariusz 11).
- **Potencjalne nakładanie się z innym customowym weryfikatorem** o tej samej nazwie handlowej
  opisanym w [Weryfikator normy okresu rozliczeniowego.md](Weryfikator%20normy%20okresu%20rozliczeniowego.md)
  – ten kod nie jest obecnie w bazie `Claude`, ale gdyby kiedyś też został wdrożony, oba
  mechanizmy mogłyby zgłaszać niezależne komunikaty dla tej samej sytuacji niedoboru. Do ustalenia
  z klientem, czy docelowo ma działać tylko jeden z nich.
- **Brak kontroli poprawności etatu w zakresie `doBiezacego`.** Jeśli w zakresie „od początku
  okresu do bieżącego miesiąca” występują dni bez etatu (np. przed faktycznym zatrudnieniem), sam
  `KalkulatorKodeksowyPracownika.Norma` je pomija (nie rzuca wyjątkiem – zweryfikowane
  dekompilacją, `Historia.GetIntersectedRows` po prostu ich nie zwraca), więc to nie jest ryzyko
  wyjątku, ale wynik `normaKodeksowaDoBiezacego` dla takiego zakresu będzie z założenia niższy –
  niesprawdzone scenariuszem testowym.
- **Metoda pomocnicza w kodzie skryptu** – patrz [pkt 3](#3-konfiguracja), ryzyko przy edycji przez
  wbudowany edytor GUI.
- **`Date.Holidays(d.Year)` vs `Date.IsHoliday(d)`.** Kod używa pierwszej formy (lista świąt danego
  roku); nie sprawdzono, czy dla dni na przełomie roku (`d.Year` vs `d.Year` pozostałego zakresu)
  zachowanie jest identyczne z resztą kodu weryfikatorów w tym repo (które gdzie indziej używają
  `Date.IsHoliday`) – w praktyce różnicy raczej nie będzie (funkcje powinny być spójne), ale nie
  zweryfikowano wprost.
