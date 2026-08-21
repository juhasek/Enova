# Weryfikator normy okresu rozliczeniowego – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy weryfikator

Weryfikator sprawdza, przy zapisie kalendarza (planu pracy) pracownika, czy zaplanowane godziny
pracy w bieżącym **okresie rozliczeniowym nadgodzin** (np. 1, 3, 4 lub 12-miesięcznym) wciąż
pozwalają na osiągnięcie **normy kodeksowej** (ustawowej liczby godzin do przepracowania w tym
okresie) — zarówno w kierunku **przekroczenia**, jak i **niedoboru**.

Weryfikator jest zarejestrowany na zapisie **całego kalendarza**, a nie pojedynczego dnia — sprawdzenie
uruchamia się więc dopiero przy zapisie widoku kalendarza, biorąc pod uwagę cały jego bieżący stan.

### Dlaczego to nie jest proste porównanie „zaplanowano vs. norma”

Przy równoważnym systemie czasu pracy pracownik może w jednym miesiącu pracować więcej, a w innym
mniej — liczy się suma za cały okres rozliczeniowy, a nie każdy miesiąc z osobna. Prosta suma
„ile zaplanowano do tej pory” byłaby myląca, bo dni, których jeszcze nikt nie zaplanował, wciąż
mogą przyjąć dowolną wartość — nie wiadomo jeszcze, czy będą tam długie zmiany, krótkie, czy dzień
wolny.

Dlatego weryfikator liczy nie jedną sumę, tylko **przedział możliwych do osiągnięcia godzin**
w całym okresie:

- **dni z jawnym wpisem w planie** (użytkownik faktycznie coś tam wpisał) są traktowane jako
  ostatecznie zdecydowane — ich godzin nie da się już cofnąć,
- **dni bez jawnego wpisu**, ale będące dniami roboczymi wg kalendarza pracownika, są traktowane
  jako wciąż elastyczne — mogą ostatecznie przyjąć dowolną wartość od **zera** do **dobowego
  maksimum** wynikającego z systemu czasu pracy pracownika w tym dniu (12:00 dla systemu
  równoważnego, albo wartość z konfiguracji nadgodzin dobowych dla pozostałych systemów),
- dni z definicji wolne wg kalendarza (weekendy, święta) nie zwiększają maksimum — nie da się ich
  „podciągnąć” do pełnego wymiaru.

Błąd zgłaszany jest **tylko wtedy, gdy osiągnięcie normy kodeksowej jest już matematycznie
niemożliwe**:

- **przekroczenie** — gdy suma godzin z dni już jawnie wpisanych sama w sobie przekracza normę
  kodeksową (więc nawet zerując resztę okresu, i tak będzie za dużo),
- **niedobór** — gdy nawet zaplanowanie maksymalnych, dopuszczalnych zmian we wszystkich
  pozostałych dniach roboczych okresu i tak nie wystarczy do osiągnięcia normy.

Dopóki norma kodeksowa mieści się w tym przedziale — weryfikator nie zgłasza błędu, bo wciąż jest
matematycznie możliwe dojście do normy poprzez odpowiednie zaplanowanie pozostałych dni.

## 2. Kiedy się uruchamia

Przy zapisie kalendarza (planu pracy) pracownika. Weryfikator działa tylko wtedy, gdy źródłem
planu jest pracownik (`Pracownik`) — dla innych źródeł planu (np. grupa, stanowisko) nic nie
sprawdza.

**Ważne:** jeśli w systemie istnieje inny weryfikator sprawdzający normę okresu rozliczeniowego
(np. wbudowany w Enova, niezależny od tego opisanego tutaj), oba mogą działać równolegle i dawać
osobne, niezależne komunikaty. Przed testowaniem tego weryfikatora warto upewnić się, że nie jest
aktywny żaden inny, konkurencyjny weryfikator dla tego samego zdarzenia — inaczej łatwo pomylić,
który komunikat pochodzi z którego mechanizmu.

## 3. Komunikaty

### Przekroczenie

> Przekroczono limit godzin (norma kodeksowa) o (nadwyżka) w okresie rozliczeniowym (okres) - (pracownik)

Pojawia się, gdy suma godzin z dni jawnie wpisanych do planu sama w sobie przekracza normę
kodeksową całego okresu — bez względu na to, co jeszcze zostanie zaplanowane w pozostałych dniach.

### Niedobór

> Nie da się już osiągnąć normy godzin (norma kodeksowa) w okresie rozliczeniowym (okres), brakuje (brakująca liczba godzin) - (pracownik). W pozostałych dniach można maksymalnie zaplanować: (liczba dni roboczych) x (dobowe maksimum) = (suma)

Pojawia się, gdy nawet zaplanowanie maksymalnych, dopuszczalnych zmian we wszystkich pozostałych
dniach roboczych okresu nie wystarczy do osiągnięcia normy kodeksowej. Komunikat dodatkowo pokazuje,
ile dni roboczych zostało jeszcze do zaplanowania i jaki jest ich dobowy limit — żeby wynik dało
się łatwo zweryfikować „ręcznie”, bez zaglądania w kod. Jeśli w obrębie okresu dobowy limit różni
się w zależności od dnia (np. przez zmianę systemu czasu pracy w trakcie okresu — patrz pkt 5),
komunikat pokazuje tylko łączną sumę, bez mnożenia „dni x limit”.

Brak błędu (wartość `null`) oznacza, że osiągnięcie normy kodeksowej jest wciąż matematycznie
możliwe — niezależnie od tego, czy aktualny plan wygląda na razie na „za dużo”, czy „za mało”.

## 4. Scenariusze testowe

Poniższe scenariusze zostały zweryfikowane na żywym środowisku Enova w trakcie tworzenia
weryfikatora i opisują oczekiwane zachowanie.

### 4.1. Nadwyżka w jednym miesiącu, reszta okresu wolna — brak błędu

- Okres rozliczeniowy: 3-miesięczny.
- Miesiąc 1: jeden dzień zaplanowany z nadwyżką godzin (np. dłuższa zmiana w systemie
  równoważnym).
- Miesiące 2–3: brak jakichkolwiek wpisów.

**Oczekiwany wynik:** brak błędu. Nadwyżka z jednego dnia jest znikoma w porównaniu z zapasem
dostępnym w niezaplanowanej reszcie okresu.

### 4.2. Cały pierwszy miesiąc wyzerowany, reszta okresu nietknięta — komunikat o niedoborze, blisko granicy

- Okres rozliczeniowy: 3-miesięczny (lipiec–wrzesień).
- Lipiec: jawnie wpisane 0 godzin dla każdego dnia (np. cały miesiąc bezpłatny/wyłączony z pracy).
- Sierpień, wrzesień: brak wpisów (w pełni elastyczne).

**Oczekiwany wynik:** komunikat o niedoborze, z bardzo małą brakującą wartością (rzędu
pojedynczych godzin) — bo zapas wynikający z możliwości wydłużenia dni w sierpniu i wrześniu do
12:00 (system równoważny) niemal w całości pokrywa normę kodeksową wyzerowanego lipca. To
oczekiwane zachowanie „skrajnej wykonalności”, nie błąd — sprawdzamy, czy da się jeszcze dojść do
normy przy maksymalnym możliwym wykorzystaniu pozostałych dni, a nie przy typowym, ośmiogodzinnym
planowaniu.

### 4.3. Cały pierwszy i drugi miesiąc wyzerowane — komunikat o niedoborze, duży brak

- Jak w 4.2, ale zarówno lipiec, jak i sierpień są jawnie wyzerowane; elastyczny pozostaje tylko
  wrzesień.

**Oczekiwany wynik:** komunikat o niedoborze ze znacznie większą brakującą wartością — sam
wrzesień, nawet zaplanowany w całości po 12:00 dziennie, nie jest w stanie pokryć normy kodeksowej
dwóch wyzerowanych miesięcy.

### 4.4. Realne, niemożliwe do skompensowania przekroczenie

- Sytuacja, w której suma godzin z dni już jawnie wpisanych (bez uwzględniania dni jeszcze
  niezaplanowanych) sama w sobie przekracza normę kodeksową całego okresu.

**Oczekiwany wynik:** komunikat o przekroczeniu, niezależnie od tego, ile dni okresu pozostało
jeszcze niezaplanowanych — nadwyżki nie da się już cofnąć.

### 4.5. Dokładne dopasowanie do normy kodeksowej

- Wszystkie dni okresu jawnie wpisane, suma godzin dokładnie równa normie kodeksowej.

**Oczekiwany wynik:** brak błędu (norma kodeksowa mieści się w przedziale [minimum, maksimum],
który w tym przypadku sprowadza się do jednej wartości).

### 4.6. Zmiana systemu czasu pracy w trakcie okresu rozliczeniowego

- Pracownik ma w trakcie okresu rozliczeniowego zmianę systemu czasu pracy, np. z równoważnego na
  podstawowy.

**Oczekiwany wynik:** dobowe maksimum dla dni bez jawnego wpisu jest liczone osobno dla każdego
dnia na podstawie historii etatu pracownika obowiązującej w tym konkretnym dniu (`Historia[data]`)
— dni sprzed zmiany dostają maksimum właściwe systemowi równoważnemu (12:00), dni po zmianie
maksimum właściwe nowemu systemowi. Nie testowano tego scenariusza na żywym środowisku (brak
danych testowych z pracownikiem zmieniającym system w trakcie okresu), ale wynika to wprost z
konstrukcji kodu — patrz pkt 5.

## 5. Znane ograniczenia

- Weryfikator nie sprawdza, czy dla każdego dnia okresu rozliczeniowego pracownik ma w ogóle
  przypisany etat (`Historia[data].Etat`). Jeśli okres rozliczeniowy wyliczony przez
  `WyliczOkresRoliczeniowyNadgodzin` obejmowałby dzień bez etatu (np. poza faktycznym okresem
  zatrudnienia), odwołanie do `Historia[data].Etat.Kalendarz` rzuciłoby wyjątek zamiast zwrócić
  komunikat weryfikacyjny. Świadomie zrezygnowano z dodatkowego zabezpieczenia tego przypadku.
- Jeśli w systemie działa równolegle inny weryfikator sprawdzający normę okresu rozliczeniowego
  (patrz pkt 2), użytkownik może zobaczyć dwa niezależne, różniące się treścią komunikaty dla tej
  samej sytuacji.

## 6. Zalecany sposób pracy z weryfikatorem

1. Planuj okres rozliczeniowy stopniowo, miesiąc po miesiącu — dopóki norma kodeksowa jest jeszcze
   matematycznie osiągalna, weryfikator nie będzie przeszkadzał.
2. Jeśli pojawi się komunikat o **przekroczeniu**, zmniejsz liczbę godzin w już zaplanowanych
   (jawnie wpisanych) dniach — nadwyżki w dniach jeszcze niezaplanowanych nie da się już użyć do
   kompensacji.
3. Jeśli pojawi się komunikat o **niedoborze**, sprawdź w treści komunikatu, ile dni roboczych
   zostało jeszcze do zaplanowania i jaki jest ich dobowy limit — to pokazuje, o ile trzeba
   wydłużyć zmiany w pozostałych dniach (lub dodać dni pracy), żeby domknąć normę.
4. Przy pracy z pracownikami zmieniającymi system czasu pracy w trakcie okresu rozliczeniowego
   zwróć uwagę, że dobowe maksimum może się różnić w obrębie tego samego okresu (patrz pkt 4.6).
