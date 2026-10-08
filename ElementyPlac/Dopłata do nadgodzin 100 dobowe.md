# Dopłata do nadgodzin 100% dobowe

Algorytm z edytora C# definicji elementu (`_Param` + `_Wylicz`), oparty na
algorytmie standardowym enova (zmieniona tylko ostatnia linia `_Param`).

## Stawka

Bez zmian względem standardu: stawka zaszeregowania za 1h (wg normy KP lub
zwykła, zależnie od `Kalendarz.Nadgodziny.PodstawaWgNormyKP`) plus
`WnagrodzenieZaNadgodziny`; przy akordzie z opcją `NadgodzinyAkordowe60` —
60% podstawy urlopowej.

## Czas — zmiana z 2026-10-08

Standardowo element płacił tylko `N100Doba` (nadgodziny dobowe 100%).
Teraz płaci `N100Doba + NSW`, czyli także nadgodziny **świąteczne** —
wykonane w niedzielę lub święto, które nie są dniem pracy wg rozkładu
(art. 151¹ §1 pkt 1 KP — dodatek 100%).

Silnik wpisuje nadmiar dnia do `NSW` tylko wtedy, gdy definicja dnia
(np. „Niedziela”) ma zaznaczone **Nadgodziny świąteczne**. Bez tej opcji
nadmiar trafia do N50/N100 jak w zwykły dzień.

**Uwaga:** jeśli w schemacie płac jest osobny element płacący `NSW`
(dopłata do nadgodzin świątecznych), trzeba go wyłączyć — inaczej te same
godziny zostaną zapłacone dwa razy.

## Procent — poprawka z 2026-10-08

Standardowy kod brał procent dodatku z
`Element.Definicja.Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent`. Przy
algorytmie z edytora C# współczynnik kreatora nie jest wypełniony, więc
procent wynosił 0 i element liczył 0 zł mimo poprawnej liczby godzin.
Teraz procent jest ustawiony na sztywno: `new Percent(1m)` = 100%
(`Percent(0.6m)` w tym samym kodzie oznacza 60%).

## Niedziela: 8h okresowe + reszta dobowe 100% — zmiana z 2026-10-08

Wymaganie klienta: z pracy w niedzielę pierwsze 8h (norma dobowa) to
nadgodziny **okresowe** (płatne na koniec okresu rozliczeniowego), nadwyżka
ponad 8h to nadgodziny **dobowe 100%** płatne w miesiącu.

Natywny silnik tego nie umie:

- definicja dnia „Niedziela” z **Nadgodziny świąteczne** → całość (np. 15h)
  idzie do `NSW`, a przy kalendarzu bez „Świąteczne miesięcznie” `NSW` w trybie
  dobowym jest zerowane (wypłata dopiero na koniec okresu),
- bez tej opcji → 8h okresowe + nadwyżka jako dobowe, ale podzielona jak w
  zwykły dzień: N50 (do limitu / poza nocą) i N100 (np. godzina nocna przy
  „Nocne 100%”).

Dlatego: **„Niedziela” bez Nadgodzin świątecznych**, a algorytm dolicza do
`N100Doba` część `N50` z dni typu `TypDnia.Świąteczny` (liczoną silnikiem
per dzień, tryb `TylkoDobowe`). Element **Dopłata do nadgodzin 50%** musi
odjąć tę samą wartość (`Składnik.Czas = zestNad.N50 - n50Swieta`), inaczej
godziny zostaną zapłacone podwójnie.

Przykład 13.09.2026 (praca 7–20 i 21–23 = 15h): 8h okresowe, 7h dobowe 100%
(6h przeniesione z N50 + 1h nocna).
