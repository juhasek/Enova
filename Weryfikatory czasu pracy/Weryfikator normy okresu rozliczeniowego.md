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

Poniższe scenariusze opisują oczekiwane zachowanie weryfikatora. Kolumna „Zweryfikowano na żywo”
wskazuje, które z nich zostały faktycznie sprawdzone na żywym środowisku Enova w trakcie
tworzenia weryfikatora, a które wynikają wprost z konstrukcji kodu.

| # | Scenariusz | Opis sytuacji | Oczekiwany wynik | Zweryfikowano na żywo |
|---|---|---|---|---|
| 4.1 | Nadwyżka w jednym miesiącu, reszta okresu wolna | Okres 3-miesięczny. Miesiąc 1: jeden dzień zaplanowany z nadwyżką godzin (np. dłuższa zmiana w systemie równoważnym). Miesiące 2–3: brak jakichkolwiek wpisów. | Brak błędu — nadwyżka z jednego dnia jest znikoma wobec zapasu dostępnego w niezaplanowanej reszcie okresu. | Tak |
| 4.2 | Cały pierwszy miesiąc wyzerowany, reszta okresu nietknięta | Okres 3-miesięczny (lipiec–wrzesień). Lipiec: jawnie wpisane 0 godzin dla każdego dnia. Sierpień, wrzesień: brak wpisów (w pełni elastyczne). | Komunikat o niedoborze z bardzo małą brakującą wartością (rzędu pojedynczych godzin) — zapas z możliwości wydłużenia dni w sierpniu i wrześniu do 12:00 (system równoważny) niemal w całości pokrywa normę kodeksową wyzerowanego lipca. To oczekiwane zachowanie „skrajnej wykonalności”, nie błąd. | Tak |
| 4.3 | Cały pierwszy i drugi miesiąc wyzerowane | Jak w 4.2, ale lipiec i sierpień są jawnie wyzerowane; elastyczny pozostaje tylko wrzesień. | Komunikat o niedoborze ze znacznie większą brakującą wartością — sam wrzesień, nawet zaplanowany w całości po 12:00 dziennie, nie pokrywa normy kodeksowej dwóch wyzerowanych miesięcy. | Tak |
| 4.4 | Realne, niemożliwe do skompensowania przekroczenie | Suma godzin z dni już jawnie wpisanych (bez dni jeszcze niezaplanowanych) sama w sobie przekracza normę kodeksową całego okresu. | Komunikat o przekroczeniu, niezależnie od tego, ile dni okresu pozostało niezaplanowanych — nadwyżki nie da się już cofnąć. | Tak |
| 4.5 | Dokładne dopasowanie do normy kodeksowej | Wszystkie dni okresu jawnie wpisane, suma godzin dokładnie równa normie kodeksowej. | Brak błędu (norma kodeksowa mieści się w przedziale [minimum, maksimum], który sprowadza się tu do jednej wartości). | Nie |
| 4.6 | Zmiana systemu czasu pracy w trakcie okresu rozliczeniowego | Pracownik ma w trakcie okresu zmianę systemu czasu pracy, np. z równoważnego na podstawowy. | Dobowe maksimum dla dni bez jawnego wpisu liczone jest osobno dla każdego dnia na podstawie historii etatu obowiązującej w tym dniu (`Historia[data]`) — dni sprzed zmiany dostają maksimum właściwe systemowi równoważnemu (12:00), dni po zmianie maksimum właściwe nowemu systemowi. Wynika wprost z konstrukcji kodu — patrz pkt 5. | Nie |

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
