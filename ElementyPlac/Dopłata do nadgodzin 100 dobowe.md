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
