# Nadgodziny okresowe silnik – dokumentacja biznesowa

Część zestawu „Nadgodziny … silnik”. Pełny opis – po co ten zestaw, jak dzieli pracę między
silnik i cechę, od jakiej konfiguracji zależy, jakie są ryzyka i status – znajduje się
w `Cechy/Nadgodziny 50 silnik.md`. Ta cecha korzysta z dokładnie tej samej mechaniki
i różni się tylko ostatnim krokiem.

Zwraca część godzin tej strefy, która jest pracą **poza oknem planu dnia**, ale **nie**
weszła do nadgodzin dobowych wyliczonych przez silnik. W praktyce są to:

- godziny przed rozpoczęciem zaplanowanej zmiany i w „czarnej dziurze” (przerwie w planie,
  między dobami pracowniczymi) – przy ustawieniu `Nadgodziny między dobami pracowniczymi:
  warunkowo` silnik odejmuje je od czasu pracy dnia, więc nie są nadgodzinami dobowymi
  i rozlicza je okres rozliczeniowy,
- nadwyżka nad planem, która nie wyszła ponad normę dobową – czyli „dopracowanie” do normy
  w dniu zaplanowanym krócej niż norma.

Inaczej niż w cesze `Nadgodziny okresowe` ze starego zestawu, nie ma tu osobnej reguły na
„czarną dziurę” – ten przypadek wychodzi sam z arytmetyki przedziałów (`praca dnia` minus
`plan dnia` minus `nadgodziny dobowe`).

W dniu niedzielno-świątecznym zwraca 0 – takie dni rozlicza `Nadgodziny NSW silnik`.

Uwaga: okresowe po stronie silnika są liczone na **całym okresie rozliczeniowym**
(`TrybRozliczaniaNadgodzin.TylkoOkresowe`), a nie per dzień, więc ta cecha nie jest
odwzorowaniem liczby okresowych ze Statystyki – pokazuje, które godziny dnia są kandydatami
do rozliczenia okresowego. Zgodność ze Statystyką dotyczy sumy cech dobowych (50/100/NSW).
