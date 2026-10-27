# Test odwrotny ankiety na definicjach standardowych enova

Wygenerowano: 2026-10-27 · ankieta 2.0 · enova 2610.0.0-beta.241094 · licencja złota.

Dla każdej definicji standardowej szukane są odpowiedzi na **pytania ogólne** (bez „Inaczej” i „Nie wiem”), które najlepiej ją odtwarzają. Różnice to pola, które klient musiałby poprawić pytaniem szczegółowym – kolumna „Pytanie” mówi, czy i w jakim zestawie klient je zobaczy. Materiał do przeglądu z konsultantem: duża liczba różnic = brakuje pytania ogólnego albo mapowania.

| Definicja | Pól różnych | Odpowiedzi |
|---|---|---|
| Premia | 0 | Premia · Pracownikom zatrudnionym na umowę o pracę · Kwota ustalana indywidualnie dla pracownika · Co miesiąc · Za poprzedni miesiąc (z dołu) · Wysokość ustala przełożony, biorąc pod uwagę nieobecności · Składnik zmienny – wchodzi do wynagrodzenia urlopowego ze średniej · Nie · Tak, jak od wynagrodzenia · Tak |
| Premia procentowa | 7 | Premia · Pracownikom zatrudnionym na umowę o pracę · Procent od wynagrodzenia zasadniczego · Co miesiąc · Za poprzedni miesiąc (z dołu) · Jest pomniejszany za dni choroby · Pomniejszany za urlop, w zamian wliczany do wynagrodzenia urlopowego · Tak, proporcjonalnie do czasu nieobecności · Tak, jak od wynagrodzenia · Tak |
| Dodatek funkcyjny | 0 | Nadgodziny, dodatek nocny, ekwiwalent za urlop, odprawa, zasiłek |
| Dodatek stażowy | 0 | Dodatek stażowy · Przysługuje w pełnej wysokości · Wypłacany normalnie, w pełnej wysokości · Tak, proporcjonalnie do czasu nieobecności · Tak |
| Składka PZU | 1 | Potrącenie z wypłaty · I pracownikom, i zleceniobiorcom · Stała kwota, taka sama dla wszystkich · Co miesiąc |
| Nagroda pieniężna | 9 | Premia · Pracownikom zatrudnionym na umowę o pracę · Stała kwota, taka sama dla wszystkich · Jednorazowo · Tak, jak od wynagrodzenia · Tak |
| Odprawa | 21 | Premia · Pracownikom zatrudnionym na umowę o pracę · Stała kwota, taka sama dla wszystkich · Jednorazowo · Podatek tak, składki ZUS nie · Nie |
| Odprawa emerytalna | 20 | Premia · Pracownikom zatrudnionym na umowę o pracę · Stała kwota, taka sama dla wszystkich · Jednorazowo · Podatek tak, składki ZUS nie · Nie |
| Ekwiwalent za pracę zdalną | 18 | Potrącenie z wypłaty · Pracownikom zatrudnionym na umowę o pracę · Stała kwota, taka sama dla wszystkich · Co miesiąc |
| Ryczałt za pracę zdalną | 18 | Potrącenie z wypłaty · Pracownikom zatrudnionym na umowę o pracę · Stała kwota, taka sama dla wszystkich · Co miesiąc |
| Świadczenie socjalne | 10 | Potrącenie z wypłaty · Pracownikom zatrudnionym na umowę o pracę · Stała kwota, taka sama dla wszystkich · Jednorazowo |
| Ryczałt za paliwo do 900 cm | 21 | Potrącenie z wypłaty · Pracownikom zatrudnionym na umowę o pracę · Stała kwota, taka sama dla wszystkich · Co miesiąc |

## Premia – 0 różnic

Pytania ogólne odtwarzają definicję w całości.

## Premia procentowa – 7 różnic

| Pole | Z ankiety | W enova | Pytanie (zestaw) |
|---|---|---|---|
| Nazwa „Podstawa 1” (element wypłaty) | Wynagrodzenie zasadnicze: | Wynagrodzenie zasadnicze | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Procent” (element wypłaty) | Procent premii: | Procent premii | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Generuj zerowy element | Nie | Tak | Czy pokazywać składnik na pasku także wtedy, gdy wynosi 0 zł? (rozszerzony) |
| Kolejność na listach płac | 0 | 50 | Miejsce składnika na pasku i liście płac (tylko wdrożeniowiec) |
| Naliczanie w okresie danej umowy o pracę | Nie | — | Gdy pracownik ma w miesiącu dwie kolejne umowy o pracę, czy liczyć składnik osobno dla każdej z nich? (pełny) |
| Co ile okresów (tygodni / miesięcy / lat) | 1 | — | Co ile tygodni / miesięcy / lat? (pełny) |
| Typ okresu naliczania | Co n miesięcy | Każdy okres | Jak często składnik jest naliczany? (pełny) |

## Dodatek funkcyjny – 0 różnic

Pytania ogólne odtwarzają definicję w całości.

## Dodatek stażowy – 0 różnic

Pytania ogólne odtwarzają definicję w całości.

## Składka PZU – 1 różnic

| Pole | Z ankiety | W enova | Pytanie (zestaw) |
|---|---|---|---|
| Nazwa parametru „Podstawa” (dodatek) | — | Składka PZU | Jak nazwać parametry składnika w kartotece pracownika? (tylko wdrożeniowiec) |

## Nagroda pieniężna – 9 różnic

| Pole | Z ankiety | W enova | Pytanie (zestaw) |
|---|---|---|---|
| Aktualizuj zapis po wypłacie | Nie | — | Czy po każdej wypłacie zapamiętywać w kartotece pracownika wartości (np. procent premii) z tej wypłaty? (pełny) |
| Nazwa „Podstawa 1” (element wypłaty) | Wartość premii: | Nagroda: | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Kwota | 0 | — | Jaka jest kwota? (podstawowy) |
| Podstawa | Kwota | Wyrażenie | Od czego zależy kwota (co jest podstawą wyliczenia)? (pełny) |
| Pomniejsz za okres niezatrudnienia | Proporcjonalnie | Nie | Gdy pracownik nie był zatrudniony przez cały miesiąc (przyjęcie lub zwolnienie w trakcie), jak zmniejszyć składnik? (rozszerzony) |
| Podstawa składek osób na urlopie wychowawczym | Wliczać dla | Nie wliczać | Jak wliczać składnik do podstawy składek osób na urlopie wychowawczym? (pełny) |
| Dodatek wynika z umowy o pracę | Tak | — | Czy składnik wynika z umowy o pracę (ma być na wydruku umowy)? (rozszerzony) |
| Rodzaj elementu | Dodatek | Nagroda | Jak program ma traktować ten składnik? (pełny) |
| Modyfikacja odbiorcy na dodatku | Nie | — | Czy odbiorcę można zmienić indywidualnie u pracownika (np. inny komornik)? (pełny) |

## Odprawa – 21 różnic

| Pole | Z ankiety | W enova | Pytanie (zestaw) |
|---|---|---|---|
| Nazwa parametru „Dni” (dodatek) | — | Liczba miesięcy | Jak nazwać parametry składnika w kartotece pracownika? (tylko wdrożeniowiec) |
| Nazwa „Dni” (element wypłaty) | — | Liczba miesięcy | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Podstawa 1” (element wypłaty) | Wartość premii: | Podstawa /miesiąc | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Standardowy okres naliczania podstawy | — | Tak | Czy podstawę liczyć ze standardowego okresu (jak dla ekwiwalentu)? (pełny) |
| Standardowy współczynnik | — | Tak | Czy stosować standardowy współczynnik ekwiwalentu? (pełny) |
| Uwzględniaj wymiar etatu we współczynniku | — | Nie | Czy we współczynniku uwzględniać wymiar etatu pracownika? (pełny) |
| Kwota | 0 | — | Jaka jest kwota? (podstawowy) |
| Podstawa | Kwota | — | Od czego zależy kwota (co jest podstawą wyliczenia)? (pełny) |
| Wartość za | Nie zależy od czasu | — | Czy kwota jest za miesiąc, za godzinę, czy nie zależy od czasu pracy? (rozszerzony) |
| Sposób pomniejszenia do zera | Nie zeruj | — | Czy składnik przepada w całości, gdy pracownik był nieobecny? (pełny) |
| Przelicz na wartość za 1 godzinę | Nie przeliczaj | — | Czy kwotę miesięczną przeliczać na stawkę za godzinę? (pełny) |
| Pomniejsz za okres niezatrudnienia | Proporcjonalnie | — | Gdy pracownik nie był zatrudniony przez cały miesiąc (przyjęcie lub zwolnienie w trakcie), jak zmniejszyć składnik? (rozszerzony) |
| Typ mnożnika | Bez współczynnika | — | Czy podstawa jest mnożona przez procent, ułamek albo współczynnik? (pełny) |
| Priorytet | 0 | 98 | W jakiej kolejności liczyć składnik w wypłacie? (tylko wdrożeniowiec) |
| Algorytm | Kreator algorytmu | Jak ekwiwalent za urlop | W jaki sposób program ma wyliczać kwotę? (pełny) |
| Definicja listy płac | LPE | — | Na której liście płac ma się pojawiać składnik? (rozszerzony) |
| Podstawa składek osób na urlopie wychowawczym | Wliczać dla | Nie wliczać | Jak wliczać składnik do podstawy składek osób na urlopie wychowawczym? (pełny) |
| Kod składnika wynagrodzenia (ERP-7) | Składniki zmienne | Nie dotyczy | W której kolumnie zaświadczenia ERP-7 (kapitał początkowy) wykazywać składnik? (pełny) |
| Dodatek wynika z umowy o pracę | Tak | Nie | Czy składnik wynika z umowy o pracę (ma być na wydruku umowy)? (rozszerzony) |
| Kolejność na listach płac | 0 | 50 | Miejsce składnika na pasku i liście płac (tylko wdrożeniowiec) |
| Korygowany | Nie | Tak | Czy poprawiać składnik za poprzedni miesiąc, gdy później dojdzie np. zwolnienie lekarskie? (rozszerzony) |

## Odprawa emerytalna – 20 różnic

| Pole | Z ankiety | W enova | Pytanie (zestaw) |
|---|---|---|---|
| Nazwa parametru „Dni” (dodatek) | — | Liczba miesięcy: | Jak nazwać parametry składnika w kartotece pracownika? (tylko wdrożeniowiec) |
| Nazwa „Dni” (element wypłaty) | — | Liczba miesięcy: | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Podstawa 1” (element wypłaty) | Wartość premii: | Podstawa /miesiąc: | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Standardowy okres naliczania podstawy | — | Tak | Czy podstawę liczyć ze standardowego okresu (jak dla ekwiwalentu)? (pełny) |
| Standardowy współczynnik | — | Tak | Czy stosować standardowy współczynnik ekwiwalentu? (pełny) |
| Uwzględniaj wymiar etatu we współczynniku | — | Nie | Czy we współczynniku uwzględniać wymiar etatu pracownika? (pełny) |
| Kwota | 0 | — | Jaka jest kwota? (podstawowy) |
| Podstawa | Kwota | — | Od czego zależy kwota (co jest podstawą wyliczenia)? (pełny) |
| Wartość za | Nie zależy od czasu | — | Czy kwota jest za miesiąc, za godzinę, czy nie zależy od czasu pracy? (rozszerzony) |
| Sposób pomniejszenia do zera | Nie zeruj | — | Czy składnik przepada w całości, gdy pracownik był nieobecny? (pełny) |
| Przelicz na wartość za 1 godzinę | Nie przeliczaj | — | Czy kwotę miesięczną przeliczać na stawkę za godzinę? (pełny) |
| Pomniejsz za okres niezatrudnienia | Proporcjonalnie | — | Gdy pracownik nie był zatrudniony przez cały miesiąc (przyjęcie lub zwolnienie w trakcie), jak zmniejszyć składnik? (rozszerzony) |
| Typ mnożnika | Bez współczynnika | — | Czy podstawa jest mnożona przez procent, ułamek albo współczynnik? (pełny) |
| Priorytet | 0 | 98 | W jakiej kolejności liczyć składnik w wypłacie? (tylko wdrożeniowiec) |
| Algorytm | Kreator algorytmu | Jak ekwiwalent za urlop | W jaki sposób program ma wyliczać kwotę? (pełny) |
| Definicja listy płac | LPE | — | Na której liście płac ma się pojawiać składnik? (rozszerzony) |
| Podstawa składek osób na urlopie wychowawczym | Wliczać dla | Nie wliczać | Jak wliczać składnik do podstawy składek osób na urlopie wychowawczym? (pełny) |
| Kod składnika wynagrodzenia (ERP-7) | Składniki zmienne | Nie dotyczy | W której kolumnie zaświadczenia ERP-7 (kapitał początkowy) wykazywać składnik? (pełny) |
| Dodatek wynika z umowy o pracę | Tak | Nie | Czy składnik wynika z umowy o pracę (ma być na wydruku umowy)? (rozszerzony) |
| Korygowany | Nie | Tak | Czy poprawiać składnik za poprzedni miesiąc, gdy później dojdzie np. zwolnienie lekarskie? (rozszerzony) |

## Ekwiwalent za pracę zdalną – 18 różnic

| Pole | Z ankiety | W enova | Pytanie (zestaw) |
|---|---|---|---|
| Nazwa parametru „Czas” (dodatek) | — | Ilość godzin | Jak nazwać parametry składnika w kartotece pracownika? (tylko wdrożeniowiec) |
| Nazwa „Czas” (element wypłaty) | — | Ilość godzin | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Podstawa 1” (element wypłaty) | Składka PZU: | Stawka za 1 godzinę | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Czas” (suma składników) | — | Ilość godzin | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Kwota | 0 | — | Jaka jest kwota? (podstawowy) |
| Podstawa | Kwota | — | Od czego zależy kwota (co jest podstawą wyliczenia)? (pełny) |
| Sposób pomniejszenia do zera | Nie zeruj | — | Czy składnik przepada w całości, gdy pracownik był nieobecny? (pełny) |
| Przelicz na wartość za 1 godzinę | Nie przeliczaj | — | Czy kwotę miesięczną przeliczać na stawkę za godzinę? (pełny) |
| Pomniejsz za okres niezatrudnienia | Nie | — | Gdy pracownik nie był zatrudniony przez cały miesiąc (przyjęcie lub zwolnienie w trakcie), jak zmniejszyć składnik? (rozszerzony) |
| Typ mnożnika | Bez współczynnika | — | Czy podstawa jest mnożona przez procent, ułamek albo współczynnik? (pełny) |
| Potrącenie do limitu | Nie dotyczy | — | Czy potrącenie jest ograniczone limitem (np. kwota wolna od potrąceń)? (pełny) |
| Potrącenie | Tak | Nie | Czy składnik zmniejsza wypłatę (jest potrąceniem)? (rozszerzony) |
| Algorytm | Kreator algorytmu | Edytor algorytmu | W jaki sposób program ma wyliczać kwotę? (pełny) |
| Blokada | Nie | Tak | Czy to składnik archiwalny, którego nie przypisujecie już pracownikom? (rozszerzony) |
| Definicja listy płac | — | LPE | Na której liście płac ma się pojawiać składnik? (rozszerzony) |
| Wliczaj do wynagrodzenia osiąganego i wypłaconego (SOD) | Tak | Nie | Czy składnik wliczać do wynagrodzenia w deklaracjach PFRON (Wn-D, INF-DP)? (pełny) |
| Kolejność na listach płac | 0 | 50 | Miejsce składnika na pasku i liście płac (tylko wdrożeniowiec) |
| Przyrównuj do najniższego wynagrodzenia | Nie | Tak | Czy składnik liczy się do minimalnego wynagrodzenia (wyrównanie do płacy minimalnej)? (pełny) |

## Ryczałt za pracę zdalną – 18 różnic

| Pole | Z ankiety | W enova | Pytanie (zestaw) |
|---|---|---|---|
| Nazwa „Czas” (element wypłaty) | — | Czas | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Podstawa 1” (element wypłaty) | Składka PZU: | Podstawa | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Podstawa 2” (element wypłaty) | — | Kwota | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Czas” (suma składników) | — | Czas | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Kwota | 0 | — | Jaka jest kwota? (podstawowy) |
| Podstawa | Kwota | — | Od czego zależy kwota (co jest podstawą wyliczenia)? (pełny) |
| Sposób pomniejszenia do zera | Nie zeruj | — | Czy składnik przepada w całości, gdy pracownik był nieobecny? (pełny) |
| Przelicz na wartość za 1 godzinę | Nie przeliczaj | — | Czy kwotę miesięczną przeliczać na stawkę za godzinę? (pełny) |
| Pomniejsz za okres niezatrudnienia | Nie | — | Gdy pracownik nie był zatrudniony przez cały miesiąc (przyjęcie lub zwolnienie w trakcie), jak zmniejszyć składnik? (rozszerzony) |
| Typ mnożnika | Bez współczynnika | — | Czy podstawa jest mnożona przez procent, ułamek albo współczynnik? (pełny) |
| Potrącenie do limitu | Nie dotyczy | — | Czy potrącenie jest ograniczone limitem (np. kwota wolna od potrąceń)? (pełny) |
| Potrącenie | Tak | Nie | Czy składnik zmniejsza wypłatę (jest potrąceniem)? (rozszerzony) |
| Algorytm | Kreator algorytmu | Edytor algorytmu | W jaki sposób program ma wyliczać kwotę? (pełny) |
| Blokada | Nie | Tak | Czy to składnik archiwalny, którego nie przypisujecie już pracownikom? (rozszerzony) |
| Definicja listy płac | — | LPE | Na której liście płac ma się pojawiać składnik? (rozszerzony) |
| Wliczaj do wynagrodzenia osiąganego i wypłaconego (SOD) | Tak | Nie | Czy składnik wliczać do wynagrodzenia w deklaracjach PFRON (Wn-D, INF-DP)? (pełny) |
| Kolejność na listach płac | 0 | 50 | Miejsce składnika na pasku i liście płac (tylko wdrożeniowiec) |
| Przyrównuj do najniższego wynagrodzenia | Nie | Tak | Czy składnik liczy się do minimalnego wynagrodzenia (wyrównanie do płacy minimalnej)? (pełny) |

## Świadczenie socjalne – 10 różnic

| Pole | Z ankiety | W enova | Pytanie (zestaw) |
|---|---|---|---|
| Aktualizuj zapis po wypłacie | Nie | — | Czy po każdej wypłacie zapamiętywać w kartotece pracownika wartości (np. procent premii) z tej wypłaty? (pełny) |
| Nazwa „Podstawa 1” (element wypłaty) | Składka PZU: | Świadczenie socjalne: | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Kwota | 0 | — | Jaka jest kwota? (podstawowy) |
| Podstawa | Kwota | Wyrażenie | Od czego zależy kwota (co jest podstawą wyliczenia)? (pełny) |
| Potrącenie do limitu | Nie dotyczy | — | Czy potrącenie jest ograniczone limitem (np. kwota wolna od potrąceń)? (pełny) |
| Potrącenie | Tak | Nie | Czy składnik zmniejsza wypłatę (jest potrąceniem)? (rozszerzony) |
| Dodatek wynika z umowy o pracę | Nie | — | Czy składnik wynika z umowy o pracę (ma być na wydruku umowy)? (rozszerzony) |
| Rodzaj elementu | Dodatek | Świadczenie | Jak program ma traktować ten składnik? (pełny) |
| Modyfikacja odbiorcy na dodatku | Nie | — | Czy odbiorcę można zmienić indywidualnie u pracownika (np. inny komornik)? (pełny) |
| Uwzględniaj jak wypłaty z ZFŚS | Nie | Tak | Czy przy zajęciach komorniczych traktować składnik jak świadczenie z ZFŚS? (pełny) |

## Ryczałt za paliwo do 900 cm – 21 różnic

| Pole | Z ankiety | W enova | Pytanie (zestaw) |
|---|---|---|---|
| Nazwa parametru „Współczynnik” (dodatek) | — | Limit kilometrów: | Jak nazwać parametry składnika w kartotece pracownika? (tylko wdrożeniowiec) |
| Nazwa „Dni” (element wypłaty) | — | Liczba dni nieobecności: | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Podstawa 1” (element wypłaty) | Składka PZU: | Ryczałt za 1 kilometr: | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Nazwa „Współczynnik” (element wypłaty) | — | Limit kilometrów: | Jak nazwać parametry składnika na wypłacie (pasku)? (tylko wdrożeniowiec) |
| Kwota | 0 | — | Jaka jest kwota? (podstawowy) |
| Podstawa | Kwota | — | Od czego zależy kwota (co jest podstawą wyliczenia)? (pełny) |
| Sposób pomniejszenia do zera | Nie zeruj | — | Czy składnik przepada w całości, gdy pracownik był nieobecny? (pełny) |
| Przelicz na wartość za 1 godzinę | Nie przeliczaj | — | Czy kwotę miesięczną przeliczać na stawkę za godzinę? (pełny) |
| Pomniejsz za okres niezatrudnienia | Nie | — | Gdy pracownik nie był zatrudniony przez cały miesiąc (przyjęcie lub zwolnienie w trakcie), jak zmniejszyć składnik? (rozszerzony) |
| Typ mnożnika | Bez współczynnika | — | Czy podstawa jest mnożona przez procent, ułamek albo współczynnik? (pełny) |
| Potrącenie do limitu | Nie dotyczy | — | Czy potrącenie jest ograniczone limitem (np. kwota wolna od potrąceń)? (pełny) |
| Potrącenie | Tak | Nie | Czy składnik zmniejsza wypłatę (jest potrąceniem)? (rozszerzony) |
| Algorytm | Kreator algorytmu | Edytor algorytmu | W jaki sposób program ma wyliczać kwotę? (pełny) |
| Koszty uzyskania przychodu | — | Nie naliczać | Jakie koszty uzyskania przychodu odliczać? (pełny) |
| Pozycja na deklaracji PIT | — | PIT-11 1/PIT-4R 1 | Na której pozycji PIT wykazywać składnik? (pełny) |
| Ulga podatkowa | — | Nie naliczać | Czy odliczać kwotę zmniejszającą podatek (ulgę)? (pełny) |
| Naliczaj zwolnienie z podatku | — | Warunkowo | Czy stosować zwolnienia z podatku (do 26 lat, duża rodzina, senior, powrót z zagranicy)? (pełny) |
| Podstawa naliczania podatku pomniejszona o składki ZUS | — | Nie | Czy podstawę podatku zmniejszać o składki ZUS pracownika? (pełny) |
| Zaliczka podatku (podatek) | — | Naliczać wg skali podatkowej | Jak liczyć zaliczkę na podatek? (pełny) |
| Generuj zerowy element | Nie | Tak | Czy pokazywać składnik na pasku także wtedy, gdy wynosi 0 zł? (rozszerzony) |
| Kolejność na listach płac | 0 | 50 | Miejsce składnika na pasku i liście płac (tylko wdrożeniowiec) |
