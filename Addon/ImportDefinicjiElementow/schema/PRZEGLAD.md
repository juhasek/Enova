# Przegląd schematu definicji elementu wynagrodzenia

Wygenerowano 2026-10-21 z bibliotek enova **2610.0.0-beta.241094** (wersja schematu 1.0).
Plik generowany — nie edytować ręcznie. Opisy i warunki zmienia się w `opisy.json`, układ w `zrodla.json`,
potem uruchamia generator (patrz README).

- zakładek: 12, pól: 180, enumów: 55, słowników: 12
- pól z opisem do weryfikacji: **15**, pominiętych: 13, definicji standardowych enova: 255

Legenda: **Wym.** — wymagane zawsze (✔) lub warunkowo (w kolumnie warunków); ⚠ — opis do weryfikacji; ⓓ — wartość domyślna do weryfikacji.

## Ogólne

Licencja enova: `PLXL`. 
Nazwa, sposób i częstotliwość naliczania elementu oraz jego miejsce na liście płac.

### Ogólne


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Rodzaj elementu ⚠ | `RodzajZrodla` | lista: `RodzajŹródłaWypłaty` | ✔ | Dodatek |  | Określa, czym jest element w enova: dodatek lub potrącenie przypisywane pracownikowi (Dodatek), wynagrodzenie zasadnicze (Etat), rozliczenie nieobecności, np. wynagrodzenie za urlop lub zasiłek (Nieobecność), wypłata do umowy cywilnoprawnej (Umowa). Rodzaju nie można zmienić po utworzeniu definicji – większość nowych składników to Dodatek. |
| Nazwa | `Nazwa` | string(40) | ✔ |  | walidacja: {"unikalne":"blad"} | Pełna nazwa elementu (maks. 40 znaków). Musi być unikalna w całej bazie – enova nie zapisze drugiej definicji o tej samej nazwie. Nazwy definicji standardowych nie można zmienić. |
| Nazwa wyświetlana | `NazwaPomocnicza` | string(40) |  |  | licencja zlota | Nazwa elementu pokazywana na raportach (np. pasek wypłaty), jeżeli ma być inna niż nazwa definicji. |
| Skrót | `Skrot` | string(12) | ✔ |  | walidacja: {"unikalne":"ostrzezenie"} | Krótka nazwa (maks. 12 znaków) używana na wydrukach, gdzie pełna nazwa się nie mieści, np. na liście płac. Dobierz tak, by była czytelna, np. „Prem.kw.”. |
| Kod ⚠ | `Kod` | string(12) |  |  |  | Dodatkowy kod elementu (maks. 12 znaków), np. używany w raportach lub w wymianie danych z innymi systemami. Instrukcja nie opisuje jego zastosowania – wypełnij, jeśli firma stosuje kody składników. |
| Blokada | `Blokada` | bool |  | Nie |  | Zablokowanego elementu nie da się przypisać kolejnym pracownikom; pozostaje tylko tam, gdzie już był. Używa się go dla składników archiwalnych, których nie można usunąć, bo są na starych listach płac. |

### Naliczanie


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Naliczanie | `OkresNaliczania.Naliczanie` | lista: `TypNaliczenia` | ✔ | Płatna z dołu |  | Z dołu – element płacony po przepracowanym miesiącu (standard). Z góry – płacony przed przepracowaniem miesiąca, a nieobecności rozliczają się w następnej wypłacie; działa tylko wtedy, gdy także lista płac jest liczona z góry. |
| Opóźnienie (miesiące) | `OkresNaliczania.Opoznienie` | int |  | 0 | edytowalne: „Naliczanie” = Płatna z dołu; walidacja: {"min":0,"max":120} | O ile miesięcy okres, za który liczony jest element, jest przesunięty względem wypłaty. Przykład: nagroda roczna za ubiegły rok wypłacana w lutym – typ okresu „co n miesięcy”, 12 miesięcy, opóźnienie 2. |
| Typ okresu naliczania | `OkresNaliczania.Typ` | lista: `TypOkresuNaliczania` | ✔ | Co n miesięcy ⓓ | wartości zależne od warunków | Jak często element jest naliczany: jednorazowo, za każdy okres (miesiąc), z każdą wypłatą w miesiącu, co n tygodni, co n miesięcy lub co n lat. Uwaga: elementy za okres dłuższy niż rok nie są wliczane do podstawy zasiłków ZUS. |
| Co ile okresów (tygodni / miesięcy / lat) | `OkresNaliczania.Ilosc` | int | warunk. | 1 | edytowalne: „Typ okresu naliczania” ∈ {Co n miesięcy, Co n lat, Co n tygodni} i „Naliczanie” ≠ Płatna z góry; wymagane: „Typ okresu naliczania” ∈ {Co n miesięcy, Co n lat, Co n tygodni} i „Naliczanie” ≠ Płatna z góry; walidacja: {"min":1,"max":120} | Liczba tygodni, miesięcy lub lat (zależnie od typu okresu) między kolejnymi naliczeniami. Np. premia kwartalna: co n miesięcy = 3; nagroda roczna: co n lat = 1 (albo co 12 miesięcy). |
| Naliczanie w okresie danej umowy o pracę | `OkresNaliczania.DlaKazdejUmowy` | lista: `DlaKażdejUmowy` |  | Nie ⓓ | edytowalne: „Typ okresu naliczania” ∈ {Co n miesięcy, Co n lat, Co n tygodni} i „Naliczanie” ≠ Płatna z góry | Czy element ma być dzielony na okresy kolejnych umów o pracę. Domyślnie – zgodnie z ustawieniem w konfiguracji płac (Okres naliczania podstawy urlopów… ograniczony do danej umowy); Tak – dzieli zawsze; Nie – nie dzieli. Nie dotyczy nadgodzin i nocnych. |

### Najniższe wynagrodzenie


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Przyrównuj do najniższego wynagrodzenia | `MinimalneWynagrodzenie` | bool |  | Nie |  | Zaznacz, jeśli element ma być brany pod uwagę przy porównaniu wynagrodzenia z płacą minimalną (wyrównanie do minimalnej). Nie zaznacza się np. nagród jubileuszowych, odpraw, nadgodzin ani świadczeń nieopodatkowanych. |

### Pozostałe


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Korygowany | `Korygowany` | bool |  | Nie |  | Czy enova ma przeliczyć element za poprzedni, już rozliczony miesiąc, gdy później zmieni się czas pracy lub nieobecności (np. zwolnienie lekarskie dostarczone po wypłacie). Bez zaznaczenia zaległa nieobecność nie zmieni wartości tego elementu. |
| Generuj zerowy element | `GenerujZerowy` | bool |  | Nie |  | Czy pokazywać element na wypłacie także wtedy, gdy wyliczy się 0 zł. Standardowo elementy zerowe są pomijane. |
| Wypłata zawsze gotówką | `ZawszeGotowka` | bool |  | Nie | edytowalne: „Wypłata z ewidencji” puste | Element zostanie wypłacony gotówką bez względu na rachunki bankowe pracownika zapisane w kartotece. Nie można zaznaczyć, gdy wskazano ewidencję wypłaty na zakładce Rozliczenie. |
| Wpływa na kwotę „do wypłaty” | `DoWyplaty` | bool |  | Tak |  | Odznacz dla świadczeń w naturze (np. bony, pakiety): wartość wchodzi do podstawy podatku i składek, ale nie zwiększa kwoty przelewu/gotówki dla pracownika. |
| Aktualizuj zapis po wypłacie | `Aktualizacja` | bool |  | Nie | edytowalne: „Rodzaj elementu” = Dodatek | Po każdej wypłacie enova dopisuje zapis historyczny dodatku pracownika z wartościami parametrów (np. procentu premii), dzięki czemu każdą wypłatę można odtworzyć. Dostępne tylko dla rodzaju Dodatek. |
| Zeruj parametry po wypłacie | `Zerowanie` | bool |  | Nie | edytowalne: „Aktualizuj zapis po wypłacie” = Tak | Po wypłacie parametry dodatku (np. liczba godzin, kwota) są zerowane, żeby w kolejnym miesiącu nie wypłacić omyłkowo starej wartości. Dostępne, gdy zaznaczono „Aktualizuj zapis po wypłacie”. |

### Lista płac


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Definicja listy płac | `DefinicjaListyPlac` | słownik: `DefinicjaListyPlac` |  |  | licencja zlota | Na jakiej liście płac ma się pojawiać element (np. LPE – etaty, LPU – umowy, LPI – inne). Puste = enova wybierze listę sama wg rodzaju zatrudnienia. Dla wypłat osobom niezatrudnionym (np. emerytom) trzeba wskazać LPI. |
| Rodzaj wypłaty | `Zatrudnienie` | lista: `TypZatrudnienia` | ✔ | {"wgWarunku":[{"gdy":{"pole":"RodzajZrodla","rowne":"Umowa"},"wartosc":"Umowa"}],"inaczej":"Etat"} | licencja zlota | Komu wypłaca się element: Etat – pracownikom, Umowa – zleceniobiorcom, Inna wypłata – osobom niezatrudnionym (np. byłym pracownikom), Warunkowy – zarówno pracownikom, jak i zleceniobiorcom (np. rozliczenie delegacji). |
| Rodzaj naliczania ⚠ | `RodzajNaliczania` | lista: `RodzajeNaliczaniaListPłac` |  | Wszystkie | licencja platynowa | Na jakich listach płac naliczać element: wszystkich, tylko rzeczywistych lub tylko planowanych (planowanie wynagrodzeń, rezerwy). Instrukcja nie opisuje pola w rozdziale o definicjach. |
| Kolejność na listach płac | `Kolejnosc` | int |  | 0 | walidacja: {"min":0,"max":9999} | Kolejność elementu na paskach i listach płac (mniejsza liczba = wyżej). 0 – enova ustali kolejność sama wg rodzaju elementu (np. wynagrodzenie zasadnicze 0, dodatki 50, elementy nieopodatkowane +100). |

## Deklaracje

Podatek, koszty uzyskania, składki ZUS i miejsce elementu na deklaracjach PIT i ZUS. Dla elementów nieopodatkowanych i nieoskładkowanych zakładkę można pominąć.

### Deklaracje PIT


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Pozycja na deklaracji PIT | `Deklaracje.PozycjaPIT` | słownik: `PozycjaPIT` |  |  |  | Pozycja deklaracji PIT, na której wykazuje się wypłatę. Wybór pozycji PIT-11/PIT-4R oznacza, że element jest opodatkowany. Dla elementu nieopodatkowanego zostaw puste. Po pierwszej wypłacie nie można już wyczyścić tego pola. |
| Zaliczka podatku (podatek) | `Deklaracje.Zaliczka.Typ` | lista: `TypZaliczkiPodatku` |  | Nie naliczać | edytowalne: „Pozycja na deklaracji PIT” wypełnione | Jak liczyć zaliczkę podatku: nie naliczać (element nieopodatkowany), wg skali podatkowej (składniki pracownicze), procentowo (podany procent, np. umowy na IFT-1/PIT-8AR) lub procentowo jak umowy (procent z konfiguracji płac). |
| Procent zaliczki | `Deklaracje.Zaliczka.Procent` | percent | warunk. | 0,00% | edytowalne: „Zaliczka podatku (podatek)” = Procentowe; wymagane: „Zaliczka podatku (podatek)” = Procentowe; walidacja: {"min":0,"max":100} | Stawka podatku, gdy zaliczka jest liczona procentowo, np. 20% dla umowy rozliczanej na IFT-1. |
| Podstawa naliczania podatku pomniejszona o składki ZUS | `Deklaracje.Zaliczka.PomniejszonaZUS` | lista: `TypPomniejszeniaPodstawyFIS` |  | Nie ⓓ | edytowalne: „Zaliczka podatku (podatek)” ≠ Nie naliczać | Czy podstawę podatku pomniejszać o składki ZUS pracownika. Nie zaznacza się dla umów ryczałtowych. „Warunkowo” – umowy do 200 zł rozlicza ryczałtem, a wyższe w zwykły sposób. |
| Naliczaj zwolnienie z podatku | `Deklaracje.Zaliczka.NaliczaniePit26` | lista: `NaliczaniePit26` |  | Warunkowo | edytowalne: „Zaliczka podatku (podatek)” ≠ Nie naliczać | Czy stosować zwolnienia z PIT (do 26 lat, duża rodzina, pracujący senior, powrót z zagranicy). Warunkowo – dla elementów z pozycji PIT-4R 1, jeśli pracownik spełnia warunki; Tak – zawsze, gdy pracownik ma ustawione zwolnienie; Nie – nigdy (np. zasiłki, umowy o dzieło). |
| Koszty uzyskania przychodu | `Deklaracje.Koszty.Typ` | lista: `TypKosztowUzyskaniaPrzychodu` |  | Nie naliczać | edytowalne: „Zaliczka podatku (podatek)” ≠ Nie naliczać | Czy i jakie koszty uzyskania odliczać od elementu: ze stosunku pracy, ze stosunku pracy + 50% (prawa autorskie), procentowe (umowy cywilnoprawne) itd. enova nie odliczy w miesiącu więcej, niż przysługuje pracownikowi wg kartoteki. |
| Procent kosztów | `Deklaracje.Koszty.Procent` | percent | warunk. | 0,00% | edytowalne: „Koszty uzyskania przychodu” ∈ {Procentowe, Procentowe 50, Procentowe warunkowo}; wymagane: „Koszty uzyskania przychodu” ∈ {Procentowe, Procentowe 50, Procentowe warunkowo}; walidacja: {"min":0,"max":100} | Procent kosztów uzyskania dla kosztów procentowych, np. 20% dla umowy zlecenia lub 50% dla praw autorskich. |
| Ulga podatkowa | `Deklaracje.Ulga.Typ` | lista: `TypUlgiPodatkowej` |  | Nie naliczać | edytowalne: „Zaliczka podatku (podatek)” ≠ Nie naliczać | Czy od zaliczki tego elementu można odliczyć kwotę zmniejszającą podatek. Wybierz „Naliczać” dla opodatkowanych składników pracowniczych (także zasiłków); enova odliczy najwyżej tyle, ile przysługuje pracownikowi. |

### Deklaracje ZUS


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Kod świadczenia / przerwy (RSA) | `Deklaracje.KodRSA` | słownik: `KodRSA` |  |  |  | Kod świadczenia lub przerwy na raporcie ZUS RSA – tylko dla rozliczeń nieobecności (np. 313 zasiłek chorobowy, 331 wynagrodzenie chorobowe) albo pomniejszeń za nieobecności niepłatne (np. 111 urlop bezpłatny). Dla zwykłych dodatków zostaw puste. |
| Pozycja na deklaracji RCA | `Deklaracje.PozycjaRCA` | lista: `PozycjaRCA` |  | Nie dotyczy | edytowalne: „Kod świadczenia / przerwy (RSA)” puste | Dla świadczeń finansowanych z budżetu państwa wykazywanych na RCA (np. zasiłek rodzinny, pielęgnacyjny) lub podstawowej składki na PFE. Niedostępne, gdy wybrano kod RSA. |
| Kod składnika wynagrodzenia (ERP-7) | `Deklaracje.PozycjaRP7` | lista: `PozycjaRP7` |  | Nie dotyczy |  | Kolumna zaświadczenia ERP-7 (kapitał początkowy): składniki stałe (np. zasadnicze, stażowe, funkcyjne), zmienne (premie, nagrody, nadgodziny), inne, świadczenia w naturze, świadczenia z ubezpieczenia (zasiłki) albo nie dotyczy. |
| Podstawa naliczania składek ZUS | `Deklaracje.Spoleczne.Typ` | lista: `TypUbezpieczeniaSpolecznego` |  | Nie naliczać |  | Czy element wchodzi do podstawy składek emerytalnej, rentowej, chorobowej i wypadkowej. „Naliczać” – dla opodatkowanych składników, które nie są ustawowo wyłączone z oskładkowania; „Nie mniej niż” – tylko dla dochodu deklarowanego właściciela. |
| Składka na Fundusz Pracy | `Deklaracje.Narzuty.SkladkaFP` | lista: `TypSkładkiFP` |  | Domyślna | edytowalne: „Podstawa naliczania składek ZUS” ≠ Nie naliczać | Jak sprawdzać obowiązek składki na FP (porównanie z płacą minimalną): Domyślna – wg wbudowanych reguł enova; Bez przyrównania – bez przeliczania na pełny miesiąc; Z przyrównaniem – wartość przeliczana na pełny miesiąc (umowy zawarte w trakcie miesiąca). |
| Składka na FGŚP | `Deklaracje.Narzuty.SkladkaFGSP` | lista: `TypSkładkiFGŚP` |  | Domyślna | edytowalne: „Podstawa naliczania składek ZUS” ≠ Nie naliczać | Domyślna – składka na Fundusz Gwarantowanych Świadczeń Pracowniczych zależy od ustawienia w kartotece pracownika; Nie naliczać – np. dla umów o udział w organach stanowiących. |
| Składka na ubezpieczenie zdrowotne | `Deklaracje.Zdrowotne.Typ` | lista: `TypUbezpieczeniaZdrowotnego` |  | Nie naliczać |  | Czy od elementu liczyć składkę zdrowotną. Zwykle tak jak składki społeczne; wyjątek to wynagrodzenie chorobowe (bez społecznych, ze zdrowotną). „Warunkowo” – dla elementów rozliczających umowy zarówno ryczałtowe, jak i zwykłe. |
| Podstawa składki zdrowotnej pomniejszona o składki ZUS | `Deklaracje.Zdrowotne.PomniejszoneZUS` | bool |  | Nie | edytowalne: „Składka na ubezpieczenie zdrowotne” ≠ Nie naliczać | Czy podstawę składki zdrowotnej pomniejszać o składki społeczne pracownika. Nie zaznacza się m.in. dla dochodu deklarowanego właściciela. |
| Składka zdrowotna pomniejsza zaliczkę podatku | `Deklaracje.Zdrowotne.PomniejszaFIS` | lista: `TypPomniejszeniaFIS` |  | Nie pomniejsza | edytowalne: „Składka na ubezpieczenie zdrowotne” ≠ Nie naliczać | Jak składka zdrowotna ma się do zaliczki podatku: nie pomniejsza (np. umowy na PIT-8AR), pomniejsza do wysokości zaliczki (składniki ze stosunku pracy), pomniejsza bez ograniczenia lub warunkowo (nie dla umów ryczałtowych). |
| Element nieoskładkowany w okresie nieobecności ZUS | `Deklaracje.NieoskladkowanyZUS` | bool |  | Nie |  | Tak – element jest wykazywany na ZUS RPA/RIA jako przychód wypłacony obok zasiłku lub wynagrodzenia chorobowego, który w tym okresie nie był podstawą składek emerytalnej i rentowej (np. dodatek stażowy za okres choroby). |

### Naliczanie podatków i składek


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Priorytet naliczania podatków i składek | `Deklaracje.Priorytet` | int |  | 0 | walidacja: {"min":0,"max":999} | Kolejność przypisywania podatku i składek do poszczególnych elementów wypłaty (0, 1, 2…). Na elemencie o największej liczbie zostaje różnica z zaokrągleń. Wpływa tylko na podział kwot między elementy, nie na sumę. |
| Uwzględniaj podczas rozrzucania składek ZUS i podatku | `Deklaracje.RozrzucajPodatkiISkładki` | bool |  | Tak | edytowalne: „Wpływa na kwotę „do wypłaty”” = Nie | Dotyczy elementów, które nie wpływają na kwotę do wypłaty (np. bony). Nie – na tym elemencie podatek i składki są zawsze zerowe, a odpowiednie kwoty pokazują się na innych elementach wypłaty (np. na liście z wynagrodzeniem zasadniczym). |
| Podlega rozporządzeniu MF z 7.01.2022 | `Deklaracje.TrybPodleganiaRozpMF070122` | lista: `TrybPodleganiaRozporzadzeniuMF070122` |  | Domyślnie |  | Czy element podlega przepisom o przedłużonym terminie poboru zaliczek (rozporządzenie MF z 7.01.2022). Domyślnie – podlegają elementy z pozycji PIT-11 1/PIT-4R 1 (etat) i PIT-11 5/PIT-4R 3 (zlecenie). |

## Deklaracje (cd)

Ubezpieczenie umów cywilnoprawnych, deklaracje SOD (PFRON), PIT-11 cz. F i podstawa składek na urlopie wychowawczym.

### Ubezpieczenie umów cywilno-prawnych


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Umowa ubezpieczona jak | `Deklaracje.Umowa` | lista: `TypUmowyCywilnoPrawnej` |  | Nie dotyczy | edytowalne: „Rodzaj elementu” = Umowa cywilnoprawna | Tylko dla rodzaju Umowa: domyślny tytuł ubezpieczenia proponowany po dodaniu umowy – udział w organach stanowiących (2240), umowa o dzieło (bez ubezpieczenia), umowa zlecenia (0411), albo „Nie dotyczy”, aby wybrać tytuł samodzielnie. |
| Domyślny tytuł ubezpieczenia | `Deklaracje.UmowaTyUb` | słownik: `TytulUbezpieczenia4` |  |  | edytowalne: „Rodzaj elementu” = Umowa cywilnoprawna i „Umowa ubezpieczona jak” = Nie dotyczy | Kod tytułu ubezpieczenia podpowiadany przy umowie z tym elementem, gdy w polu „Umowa ubezpieczona jak” wybrano „Nie dotyczy”. |
| Ubezpieczenie pracownika etatowego | `Deklaracje.UmowaJakEtat` | bool |  | Nie | edytowalne: „Rodzaj elementu” = Umowa cywilnoprawna i „Umowa ubezpieczona jak” = Nie dotyczy | Tak – gdy umowę zawiera osoba zatrudniona też na etacie, tytuł ubezpieczenia zostanie przepisany z umowy o pracę; w przeciwnym razie użyty zostanie domyślny tytuł ubezpieczenia. |

### Deklaracje SOD


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Wliczaj do wynagrodzenia osiąganego i wypłaconego (SOD) | `Deklaracje.WliczajSOD` | bool |  | Tak |  | Czy element ma być uwzględniony w deklaracji Wn-D i załącznikach INF-DP (dofinansowanie PFRON do wynagrodzeń osób niepełnosprawnych). |
| Wliczaj do kosztów płacy (SOD) | `Deklaracje.WliczDoKosztowPlacy` | lista: `WliczajDoKosztowPlacyEnum` |  | Domyślnie |  | Czy element wlicza się do kosztów płacy na Wn-D: Domyślnie – tylko elementy opodatkowane; Tak – zawsze (np. ekwiwalent za pracę zdalną); Nie – nigdy. |

### Deklaracje PIT-11


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Opis rodzaju przychodu (PIT-11 cz. F) | `Deklaracje.OpisNaPIT8C` | string(80) |  |  |  | Dla przychodów z art. 20 ust. 1 ustawy o PIT wykazywanych w części F PIT-11 – rodzaj przychodu, który pojawi się na deklaracji (maks. 80 znaków). |

### Podstawa wymiaru składek


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Podstawa składek osób na urlopie wychowawczym | `Deklaracje.PodstawaSkladek.UrlopWychowawczy` | lista: `TypPodstawyZasilku` |  | Nie wliczać |  | Czy i jak element wliczać do podstawy składek emerytalnej i rentowej osób przebywających na urlopie wychowawczym (nie wliczać, wliczać, dopełniać wg godzin/dni/1/30, jak zasadnicze, w nominalnej wysokości). |

## Nieobecności

Licencja enova: `PLXL`. 
Czy i jak element wchodzi do podstawy wynagrodzenia urlopowego, ekwiwalentu i zasiłków ZUS.

### Definiowany element wynagrodzenia w podstawie naliczania urlopów


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Element o znacznych wahaniach wysokości | `Nieobecnosci.WahaniaWysokosci` | bool |  | Nie |  | Tak – zmienny element wlicza się do podstawy wynagrodzenia urlopowego z 12 miesięcy (zamiast 3), bo jego wysokość mocno się zmienia, np. prowizja. |

### Definiowany element wynagrodzenia w podstawie naliczania urlopów / Wynagrodzenia za czas urlopu


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Podstawa wynagrodzenia za czas urlopu | `Nieobecnosci.Urlop.Typ` | lista: `TypPodstawyUrlopu` |  | Nie wliczać |  | Jak element wlicza się do wynagrodzenia urlopowego: nie wliczać (np. nagrody, odprawy), wliczać aktualną wartość (stałe składniki miesięczne), wliczać po przeliczeniu (zmiana stawki, wymaga współczynnika), wliczać (zmienne – z 3 miesięcy), wliczać jak zasadnicze (np. premia procentowa od zasadniczego). |
| Współczynnik proporcjonalności (urlop) | `Nieobecnosci.Urlop.WspolczynnikTyp` | lista: `TypWspolczynnikaUrlopu` | warunk. |  | edytowalne: „Podstawa wynagrodzenia za czas urlopu” = Wliczać po przeliczeniu; wymagane: „Podstawa wynagrodzenia za czas urlopu” = Wliczać po przeliczeniu | Wielkość, według której przelicza się element przy zmianie stawek w okresie podstawy: wynagrodzenia (np. najniższe), zasiłki, wskaźniki firmy, stawka zaszeregowania 1h, zasadnicze nominalne, w zerowej wartości itd. |
| Rodzaj wynagrodzenia (współczynnik urlopu) | `Nieobecnosci.Urlop.Wynagrodzenie` | string | warunk. |  | widoczne: „Współczynnik proporcjonalności (urlop)” = Wynagrodzenia; wymagane: „Podstawa wynagrodzenia za czas urlopu” = Wliczać po przeliczeniu i „Współczynnik proporcjonalności (urlop)” = Wynagrodzenia; wartości zależne od warunków | Które wynagrodzenie z konfiguracji płac jest współczynnikiem przeliczenia, np. Najniższe. |
| Rodzaj zasiłku (współczynnik urlopu) | `Nieobecnosci.Urlop.Zasiłek` | string | warunk. |  | widoczne: „Współczynnik proporcjonalności (urlop)” = Zasiłki; wymagane: „Podstawa wynagrodzenia za czas urlopu” = Wliczać po przeliczeniu i „Współczynnik proporcjonalności (urlop)” = Zasiłki; wartości zależne od warunków | Który zasiłek z konfiguracji płac jest współczynnikiem przeliczenia. |
| Wskaźnik (współczynnik urlopu) | `Nieobecnosci.Urlop.Wskaźnik` | string | warunk. |  | widoczne: „Współczynnik proporcjonalności (urlop)” = Wskaźniki; wymagane: „Podstawa wynagrodzenia za czas urlopu” = Wliczać po przeliczeniu i „Współczynnik proporcjonalności (urlop)” = Wskaźniki | Nazwa wskaźnika z konfiguracji płac (Płace/Wskaźniki) używanego jako współczynnik przeliczenia. Wpisz nazwę wskaźnika istniejącego w bazie. |

### Definiowany element wynagrodzenia w podstawie naliczania urlopów / Wynagrodzenia za ekwiwalent


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Podstawa ekwiwalentu za urlop | `Nieobecnosci.Ekwiwalent.Typ` | lista: `TypPodstawyUrlopu` |  | Nie wliczać |  | Jak element wlicza się do podstawy ekwiwalentu za niewykorzystany urlop. Zasady jak dla wynagrodzenia urlopowego; składniki za okresy dłuższe niż miesiąc (np. premia kwartalna) wybierz „Wliczać” – wejdą średnio z 12 miesięcy. |
| Po dopełnieniu (ekwiwalent) | `Nieobecnosci.Ekwiwalent.Dopelnienie` | bool |  | Nie | edytowalne: „Podstawa ekwiwalentu za urlop” ≠ Nie wliczać | Dopełnia element do pełnego miesiąca, jeśli był pomniejszony za okres niezatrudnienia. Dostępne, gdy element wlicza się do podstawy ekwiwalentu; enova zaznacza je automatycznie przy zmianie z „Nie wliczać”. |
| Współczynnik proporcjonalności (ekwiwalent) | `Nieobecnosci.Ekwiwalent.WspolczynnikTyp` | lista: `TypWspolczynnikaUrlopu` | warunk. |  | edytowalne: „Podstawa ekwiwalentu za urlop” = Wliczać po przeliczeniu; wymagane: „Podstawa ekwiwalentu za urlop” = Wliczać po przeliczeniu | Wielkość, według której przelicza się element w podstawie ekwiwalentu przy opcji „Wliczać po przeliczeniu” – jak dla urlopu. |
| Rodzaj wynagrodzenia (współczynnik ekwiwalentu) | `Nieobecnosci.Ekwiwalent.Wynagrodzenie` | string | warunk. |  | widoczne: „Współczynnik proporcjonalności (ekwiwalent)” = Wynagrodzenia; wymagane: „Podstawa ekwiwalentu za urlop” = Wliczać po przeliczeniu i „Współczynnik proporcjonalności (ekwiwalent)” = Wynagrodzenia; wartości zależne od warunków | Które wynagrodzenie z konfiguracji płac jest współczynnikiem przeliczenia. |
| Rodzaj zasiłku (współczynnik ekwiwalentu) | `Nieobecnosci.Ekwiwalent.Zasiłek` | string | warunk. |  | widoczne: „Współczynnik proporcjonalności (ekwiwalent)” = Zasiłki; wymagane: „Podstawa ekwiwalentu za urlop” = Wliczać po przeliczeniu i „Współczynnik proporcjonalności (ekwiwalent)” = Zasiłki; wartości zależne od warunków | Który zasiłek z konfiguracji płac jest współczynnikiem przeliczenia. |
| Wskaźnik (współczynnik ekwiwalentu) | `Nieobecnosci.Ekwiwalent.Wskaźnik` | string | warunk. |  | widoczne: „Współczynnik proporcjonalności (ekwiwalent)” = Wskaźniki; wymagane: „Podstawa ekwiwalentu za urlop” = Wliczać po przeliczeniu i „Współczynnik proporcjonalności (ekwiwalent)” = Wskaźniki | Nazwa wskaźnika z konfiguracji płac (Płace/Wskaźniki) używanego jako współczynnik przeliczenia. |

### ... zasiłków (pracownicy)


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Wliczanie do podstawy zasiłków (pracownicy) | `Nieobecnosci.ZasilkiPracownicy.Typ` | lista: `TypPodstawyZasilku` |  | Nie wliczać |  | Jak element wchodzi do podstawy zasiłku pracownika: nie wliczać (składnik zachowany w czasie zasiłku), wliczać dla (w kwocie wypłaconej, bez uzupełnienia, np. nadgodziny), wliczać w nominalnej wysokości, dopełniać wg dni / godzin / jak zasadnicze / wg 1/30 (składniki pomniejszane proporcjonalnie za nieobecność). |
| … zasiłków chorobowych | `Nieobecnosci.ZasilkiPracownicy.Chorobowych` | bool |  | Nie | edytowalne: „Wliczanie do podstawy zasiłków (pracownicy)” ≠ Nie wliczać | Czy wliczać element do podstawy zasiłków chorobowych (pracownicy). Ten sam składnik może np. wchodzić do podstawy macierzyńskiego, a nie chorobowego. |
| … zasiłków rehabilitacyjnych | `Nieobecnosci.ZasilkiPracownicy.Rehabilitacyjnych` | bool |  | Nie | edytowalne: „Wliczanie do podstawy zasiłków (pracownicy)” ≠ Nie wliczać | Czy wliczać element do podstawy świadczenia rehabilitacyjnego (pracownicy). |
| … zasiłków macierzyńskich | `Nieobecnosci.ZasilkiPracownicy.Macierzynskich` | bool |  | Nie | edytowalne: „Wliczanie do podstawy zasiłków (pracownicy)” ≠ Nie wliczać | Czy wliczać element do podstawy zasiłków macierzyńskich (pracownicy). |
| … zasiłków opiekuńczych | `Nieobecnosci.ZasilkiPracownicy.Opiekunczych` | bool |  | Nie | edytowalne: „Wliczanie do podstawy zasiłków (pracownicy)” ≠ Nie wliczać | Czy wliczać element do podstawy zasiłków opiekuńczych (pracownicy). |

### ... zasiłków (inni ubezpieczeni)


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Wliczanie do podstawy zasiłków (inni ubezpieczeni) | `Nieobecnosci.ZasilkiInni.Typ` | lista: `TypPodstawyZasilku` |  | Nie wliczać |  | Jak element wchodzi do podstawy zasiłku osób niebędących pracownikami (np. zleceniobiorców). Najczęściej „Nie wliczać” albo „Wliczać dla”. |
| … zasiłków chorobowych | `Nieobecnosci.ZasilkiInni.Chorobowych` | bool |  | Nie | edytowalne: „Wliczanie do podstawy zasiłków (inni ubezpieczeni)” ≠ Nie wliczać | Czy wliczać element do podstawy zasiłków chorobowych (inni ubezpieczeni). |
| … zasiłków rehabilitacyjnych | `Nieobecnosci.ZasilkiInni.Rehabilitacyjnych` | bool |  | Nie | edytowalne: „Wliczanie do podstawy zasiłków (inni ubezpieczeni)” ≠ Nie wliczać | Czy wliczać element do podstawy świadczenia rehabilitacyjnego (inni ubezpieczeni). |
| … zasiłków macierzyńskich | `Nieobecnosci.ZasilkiInni.Macierzynskich` | bool |  | Nie | edytowalne: „Wliczanie do podstawy zasiłków (inni ubezpieczeni)” ≠ Nie wliczać | Czy wliczać element do podstawy zasiłków macierzyńskich (inni ubezpieczeni). |
| … zasiłków opiekuńczych | `Nieobecnosci.ZasilkiInni.Opiekunczych` | bool |  | Nie | edytowalne: „Wliczanie do podstawy zasiłków (inni ubezpieczeni)” ≠ Nie wliczać | Czy wliczać element do podstawy zasiłków opiekuńczych (inni ubezpieczeni). |

### ... płacy minimalnej


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Uwzględnij przy przyrównaniu podstawy chorobowego do płacy minimalnej | `Nieobecnosci.DoMinimalnej` | bool |  | Nie |  | Tak – pracownik zachowuje prawo do tego elementu w czasie zasiłku, więc jego wartość dolicza się przy sprawdzaniu, czy podstawa zasiłku nie jest niższa od płacy minimalnej (np. dodatek stażowy bez ZUS za okres choroby). |

### ... zasiłków


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Okres wliczania do podstawy zasiłków | `Nieobecnosci.WliczanieDoPodstawyChorobowego` | lista: `SposobyWliczaniaDoPodstawyChorobowego` |  | Domyślnie | edytowalne: „Rodzaj elementu” = Dodatek i („Wliczanie do podstawy zasiłków (pracownicy)” ≠ Nie wliczać lub „Wliczanie do podstawy zasiłków (inni ubezpieczeni)” ≠ Nie wliczać) | Do kiedy element jest wliczany do podstawy zasiłków: domyślnie (wg reguł enova), zawsze, do końca okresu wypłaty dodatku albo do daty zakończenia wypłaty wpisanej na dodatku. Dotyczy dodatków wliczanych do podstawy zasiłków. |
| Okres elementu w podstawie zasiłku liczony od miesiąca | `Nieobecnosci.OdMiesiaca` | lista: `DefinicjeMiesiecy` |  | styczeń | edytowalne: „Rodzaj elementu” = Dodatek i („Wliczanie do podstawy zasiłków (pracownicy)” ≠ Nie wliczać lub „Wliczanie do podstawy zasiłków (inni ubezpieczeni)” ≠ Nie wliczać) i („Typ okresu naliczania” = Co n lat lub („Typ okresu naliczania” = Co n miesięcy i „Co ile okresów (tygodni / miesięcy / lat)” > 1)) | Dla elementów za okres dłuższy niż miesiąc (np. premia kwartalna lub roczna): od którego miesiąca zaczyna się okres. Np. premia roczna z ustawieniem „marzec” wlicza się do podstawy za okresy marzec–luty. |

### Limity nieobecności


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Wypłacony dodatek jest ekwiwalentem z tytułu limitu | `Nieobecnosci.DefinicjaLimitu` | słownik: `DefinicjaLimitu` |  |  | edytowalne: „Rodzaj elementu” = Dodatek | Jeśli dodatek jest ekwiwalentem za limit nieobecności (np. urlop wypoczynkowy), wskaż ten limit – wypłata pomniejszy limit pracownika i zapisze się w polu „W tym wypłacony ekwiwalent”. Tylko dla rodzaju Dodatek. |

## Rozliczenie

Licencja enova: `PLXL`. 
Sposób wypłaty (gotówka/ewidencja), odbiorca potrącenia, zajęcia komornicze, koszty firmy, PPK i limit ZFŚS.

### Ewidencja Środków Pieniężnych


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Wypłata zawsze gotówką | `ZawszeGotowka` | bool |  | Nie | edytowalne: „Wypłata z ewidencji” puste | Element zostanie wypłacony gotówką bez względu na rachunki bankowe pracownika zapisane w kartotece. Nie można zaznaczyć, gdy wskazano ewidencję wypłaty na zakładce Rozliczenie. |
| Wypłata z ewidencji | `Rozliczenie.Ewidencja` | słownik: `EwidencjaSP` |  |  | edytowalne: „Wypłata zawsze gotówką” = Nie | Ewidencja środków pieniężnych (kasa/rachunek firmy), z której ma być wypłacany element; zobowiązania powstaną w tej ewidencji. Niedostępne, gdy zaznaczono „Wypłata zawsze gotówką”. |

### Odbiorca


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Odbiorca | `Rozliczenie.Odbiorca` | słownik: `IPodmiotKasowy` |  |  |  | Odbiorca kwoty potrącenia (np. towarzystwo ubezpieczeniowe, związek zawodowy). Po zatwierdzeniu listy płac enova przygotuje zobowiązanie wobec tego odbiorcy. Ustawia się dla potrąceń i dodatków niewpływających na wypłatę. |
| Rachunek odbiorcy ⚠ | `Rozliczenie.RachunekOdbiorcy` | słownik: `RachunekBankowyPodmiotu` |  |  | edytowalne: „Odbiorca” wypełnione | Rachunek bankowy odbiorcy, na który ma trafić przelew. Wpisz numer rachunku zapisanego u odbiorcy w enova. |
| Modyfikacja odbiorcy na dodatku | `Rozliczenie.ZmianaOdbiorcy` | bool |  | Nie | edytowalne: „Rodzaj elementu” ∈ {Dodatek, Zajęcie komornicze, Zajęcie komornicze zwrot nadpłaty, Zajęcie komornicze rozlicz depozytu} i „Wspólna płatność dla całej listy płac” = Nie | Tak – odbiorcę można zmienić na dodatku w kartotece pracownika (np. inny komornik dla każdego pracownika). Dostępne dla dodatków i zajęć komorniczych, gdy nie zaznaczono wspólnej płatności. |
| Wspólna płatność dla całej listy płac | `Rozliczenie.WspolnaPlatnosc` | bool |  | Nie | edytowalne: „Modyfikacja odbiorcy na dodatku” = Nie | Tak – dla wszystkich pracowników z listy płac powstanie jedno zsumowane zobowiązanie wobec odbiorcy; Nie – osobne dla każdego pracownika. Niedostępne, gdy odbiorcę można zmieniać na dodatku. |
| Opis do przelewu | `Rozliczenie.OpisPrzelewu` | string(80) |  |  | edytowalne: „Wspólna płatność dla całej listy płac” = Tak | Tytuł przelewu wspólnej płatności (maks. 80 znaków). Bez opisu enova użyje opisu standardowego. |

### Potrącenia z wynagrodzenia


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Zajęcie – alimenty | `Zajecie.Alimenty` | lista: `PodstawyZajęciaKomorniczego` |  | Domyślnie |  | Jak element wchodzi do podstawy potrąceń alimentacyjnych: domyślnie (enova pilnuje limitów i kwoty wolnej), nie podlega, do limitu (60%), do limitu zasiłki, w pełnej wysokości (np. umowy niebędące jedynym źródłem utrzymania), tylko podatki i składki. |
| Zajęcie – komornik | `Zajecie.Komornik` | lista: `PodstawyZajęciaKomorniczego` |  | Domyślnie |  | Jak element wchodzi do podstawy zajęć komorniczych (niealimentacyjnych): domyślnie, nie podlega, do limitu (50% i kwota wolna), do limitu zasiłki (25%), w pełnej wysokości, tylko podatki i składki. |
| Pozostałe zajęcia (art. 91 KP) | `Zajecie.Pozostale` | lista: `PodstawyZajęciaKomorniczego` |  | Domyślnie |  | Jak element wchodzi do podstawy innych potrąceń za zgodą pracownika (art. 91 KP): domyślnie / do limitu (procent z pola „do limitu” na potrąceniu), nie podlega, do limitu zasiłki (25%), w pełnej wysokości, tylko podatki i składki. |
| Uwzględniaj jak wypłaty z ZFŚS | `Zajecie.UwzgledniajJakWyplatyZFSS` | bool |  | Nie |  | Tak – element traktowany przy zajęciach komorniczych jak świadczenie z ZFŚS (standardowo ustawione na świadczeniach socjalnych). |

### Rozliczenie w kosztach uzyskania przychodu firmy


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Stanowi koszt uzyskania przychodu (firmy) | `StanowiKUP` | lista: `ElementStanowiKUP` |  | Domyślnie |  | Czy wartość elementu jest kosztem firmy (do schematów księgowych): domyślnie – elementy opodatkowane poza zasiłkami, świadczeniami socjalnymi i elementami niewpływającymi na wypłatę; wliczany; nie wliczany; PPK pracodawcy. |

### Zwrot składki PPK


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Uwzględnij w zwrotach składek PPK | `UwzgledniajZwrotSkladkiPPK` | lista: `UwzgledniajZwrotSkladkiPPK` |  | Nie dotyczy |  | Czy element jest zwrotem składki PPK – po stronie pracownika albo pracodawcy – na wydrukach płacowych. |

### Świadczenia socjalne


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Uwzględnij w limicie świadczeń zwolnionych z podatku | `UwzgledniajWLimicieZFSS` | bool |  | Nie |  | Dla świadczeń socjalnych liczonych kodem (edytor algorytmu) z obsługą limitu zwolnienia z podatku – enova pilnuje wspólnego limitu na wszystkich tak oznaczonych świadczeniach. |

## Dane statystyczne

Licencja enova: `PLXL`. 
Klasyfikacja elementu na sprawozdaniach GUS.

### Dane statystyczne GUS


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Rodzaj składnika wynagrodzenia (GUS) | `GUS.Kategoria` | lista: `KategoriaWynagrodzeniaGUS` |  | Wg definicji elementu |  | Jak element jest kwalifikowany na sprawozdaniach GUS. „Wg definicji elementu” – enova ustala sama; inną wartość wybierz, gdy element ma trafić do innej pozycji. |
| Rodzaj składnika wynagrodzenia GUS Z-12 | `GUS.RodzajSkladnikaWynagrodzeniaGus` | lista: `RodzajSkladnikaWynagrodzeniaGUSZ12` |  | Wg definicji elementu |  | Kolumna sprawozdania GUS Z-12, do której trafia element (np. wynagrodzenie zasadnicze, premie regulaminowe, nagrody i premie uznaniowe, dodatki za pracę zmianową). „Wg definicji elementu” – enova ustala sama. |
| Zaliczaj do czasu pracy ⚠ | `GUS.ZaliczajDoCzasuPracy` | bool |  | {"wgWarunku":[{"gdy":{"pole":"RodzajZrodla","rowne":"Odchy\u0142ki"},"wartosc":true}],"inaczej":false} | widoczne: „Rodzaj elementu” = Odchyłki | Pole widoczne tylko dla elementów rodzaju Odchyłki: czy odchyłki czasu pracy zaliczać do czasu pracy na sprawozdaniach GUS. Instrukcja nie opisuje pola. |

## Dodatkowe

Umowa o pracę, luka płacowa, refundacja z FP/FGŚP, pulpit pracownika, podstawa dopłat do nadgodzin, staż pracy.

### Dodatkowe informacje


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Dodatek wynika z umowy o pracę | `Dodatkowe.DodatekDoEtatu` | bool |  | Nie | edytowalne: „Rodzaj elementu” = Dodatek | Tak – element pojawi się na wydruku umowy o pracę pracownika, któremu go przypisano (zakładka Etat/Dodatki). Tylko dla rodzaju Dodatek. |
| Zakres danych (fundusz pożyczkowy) | `Zakres` | słownik: `IZakresDefinicji` | warunk. |  | widoczne: „Rodzaj elementu” ∈ {Fundusz pożyczkowy – wpisowe, Fundusz pożyczkowy – wycofanie wkładu, Fundusz pożyczkowy – składka, Pożyczka (wypłata), Pożyczka – spłata}; wymagane: „Rodzaj elementu” ∈ {Fundusz pożyczkowy – wpisowe, Fundusz pożyczkowy – wycofanie wkładu, Fundusz pożyczkowy – składka, Pożyczka (wypłata), Pożyczka – spłata} | Dla elementów obsługujących fundusz pożyczkowy (wpisowe, wycofanie, składka, pożyczka, spłata) – wskaż fundusz (PKZP, ZFM…), którego dotyczy element. |
| Uwzględniaj w wyliczeniu luki płacowej ⚠ | `Dodatkowe.LukaPlacowa` | lista: `UwzglednienieLukiPlacowe` |  | Domyślnie | licencja zlota | Czy i jako jaki składnik (stały/zmienny) element ma być uwzględniony przy wyliczeniu luki płacowej (dyrektywa o jawności wynagrodzeń). Instrukcja nie opisuje pola. |

### Refundacja wynagrodzenia z FP lub FGŚP


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Refundacja wynagrodzenia z FP lub FGŚP | `Dodatkowe.RefundowanyInfo` | lista: `RodzajRefundacjiWynagrodzenia` |  | Domyślny |  | Wykorzystywane w raporcie „Wykaz pracowników uprawnionych do świadczeń wypłaconych z FGŚP” (przestój ekonomiczny, obniżony wymiar czasu pracy). |

### Pulpit pracownika


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Widoczne w pulpicie pracownika | `PulpitPracownika` | bool |  | Nie |  | Tak – dodatek będzie widoczny dla pracownika w Pulpicie pracownika (Dane finansowe/Dodatki). |

### Dopłaty do nadgodzin


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Wliczaj do podstawy dopłat do nadgodzin | `WliczajDoPodstawyWynagrodzeniaZaNadgodziny` | lista: `WliczajDoPodstawyWynagrodzeniaZaNadgodziny` |  | Nie |  | Czy element wlicza się do stawki dopłat za nadgodziny: nie, z nominału (składnik podzielony przez czas nominalny) albo wypłacone (wartość wypłacona podzielona przez czas przepracowany). |

### Staż pracy


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Wliczaj umowę do stażu pracy | `WliczajUmoweDoStazuPracy` | bool |  | Nie | edytowalne: „Rodzaj elementu” = Umowa cywilnoprawna | Tylko dla elementów rodzaju Umowa: czy okres umowy z tym elementem wlicza się do stażu pracy. |

## Algorytm

Jak wyliczana jest wartość elementu (podstawa × mnożnik × zależność od czasu), priorytet, potrącenie i zaokrąglenie.

### Algorytm naliczania wartości


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Algorytm | `Algorytm.Typ` | lista: `TypAlgorytmuElementu` | ✔ | Kreator algorytmu | wartości zależne od warunków | Metoda wyliczenia wartości: kreator algorytmu (podstawa × mnożnik × czas – wystarcza w większości przypadków), edytor algorytmu (indywidualny kod – opisz zasadę w uwagach dla wdrożeniowca), za okres nieobecności, jak ekwiwalent za urlop, jak urlop wypoczynkowy / okolicznościowy, jak chorobowe. |
| Priorytet | `Algorytm.Priorytet` | int |  | 0 | walidacja: {"min":0,"max":9999} | Kolejność wyliczania elementu w wypłacie (0 = najwcześniej). Element liczony od innych (np. premia od zasadniczego i funkcyjnego) musi mieć większą liczbę niż one; potrącenia liczone od brutto – większą niż elementy opodatkowane. |
| Potrącenie | `Algorytm.Potracenie` | bool |  | Nie |  | Tak – element jest potrąceniem (np. składka PZU): enova pomnoży wynik przez −1, a kwoty wpisuje się zawsze bez minusa. |
| Potrącenie do limitu | `Algorytm.LimitPotracenia` | lista: `TypLimituPotrącenia` |  | Nie dotyczy | edytowalne: „Potrącenie” = Tak i „Zaliczka podatku (podatek)” = Nie naliczać i „Składka na ubezpieczenie zdrowotne” = Nie naliczać i „Podstawa naliczania składek ZUS” = Nie naliczać; wartości zależne od warunków | Dla nieopodatkowanych potrąceń: nie dotyczy, uwzględniaj (w logu zajęć), standardowy (50% wynagrodzenia netto, nie więcej niż kwota wolna), alimenty (40%), do 80% lub 100% minimalnego wynagrodzenia netto. Limity inne niż „uwzględniaj” działają tylko w kreatorze i edytorze algorytmu (w edytorze wymagają kodu). |
| Potrącaj tylko pełne kwoty | `Algorytm.TylkoPelnePotracenie` | bool |  | Nie | edytowalne: „Potrącenie” = Tak i „Potrącenie do limitu” ∈ {Alimenty, 80% minimalnego, 100% minimalnego} | Tak – jeśli całej kwoty nie da się potrącić w ramach limitu (alimenty, 80% lub 100% minimalnego), potrącenie w ogóle nie zostanie naliczone. |
| Jak ekwiwalent za urlop | `Algorytm.JakEkwiwalent` | bool |  | Nie | widoczne: „Algorytm” = Edytor algorytmu i „Rodzaj elementu” = Nieobecność | Dla rozliczeń nieobecności liczonych edytorem algorytmu: Tak – pokazuje parametry podstawy jak dla ekwiwalentu (okres podstawy, współczynnik), z których korzysta kod algorytmu. |

### Algorytm naliczania wartości / Jak urlop wypoczynkowy

Sekcja widoczna, gdy: „Algorytm” = Jak urlop wypoczynkowy.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Standardowy okres naliczania podstawy | `Algorytm.JakUrlopWypoczynkowy.StdOkresPodstawy` | bool |  | Tak ⓓ |  | Tak – podstawa z liczby miesięcy ustawionej w konfiguracji płac (Urlop wypoczynkowy: 3 miesiące, składniki o znacznych wahaniach 12). Nie – własna liczba miesięcy dla składników o małych wahaniach. |
| Okres naliczania podstawy (miesiące) | `Algorytm.JakUrlopWypoczynkowy.OkresPodstawy` | int | warunk. |  | edytowalne: „Standardowy okres naliczania podstawy” = Nie; wymagane: „Standardowy okres naliczania podstawy” = Nie; walidacja: {"min":1,"max":12} | Liczba miesięcy, z których liczona jest podstawa składników o małych wahaniach, gdy okres nie jest standardowy. |

### Algorytm naliczania wartości / Jak chorobowe

Sekcja widoczna, gdy: „Algorytm” = Jak chorobowe.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Standardowy okres naliczania podstawy | `Algorytm.JakChorobowe.StdOkresPodstawy` | bool |  | Tak ⓓ |  | Tak – podstawa zasiłku z 12 miesięcy (wartość z konfiguracji Zasiłki ogólne). Nie – własna liczba miesięcy. |
| Okres naliczania podstawy (miesiące) | `Algorytm.JakChorobowe.OkresPodstawy` | int | warunk. |  | edytowalne: „Standardowy okres naliczania podstawy” = Nie; wymagane: „Standardowy okres naliczania podstawy” = Nie; walidacja: {"min":1,"max":24} | Liczba miesięcy podstawy własnego zasiłku, gdy okres nie jest standardowy. |
| Procent zasiłku | `Algorytm.JakChorobowe.Procent` | percent | warunk. |  | wymagane: „Algorytm” = Jak chorobowe; walidacja: {"min":0,"max":100} | Wysokość definiowanego zasiłku jako procent podstawy, np. 80%. |
| Rodzaj zatrudnienia | `Algorytm.JakChorobowe.Zatrudnienie` | lista: `ChorobowyZatrudnienie` | warunk. |  | wymagane: „Algorytm” = Jak chorobowe | Komu wypłaca się zasiłek: pracownik (tylko etat), inny ubezpieczony (np. zleceniobiorcy), jak zatrudnienie (obu grupom). |

### Algorytm naliczania wartości / Jak ekwiwalent za urlop

Sekcja widoczna, gdy: „Algorytm” = Jak ekwiwalent za urlop lub („Algorytm” = Edytor algorytmu i „Rodzaj elementu” = Nieobecność i „Jak ekwiwalent za urlop” = Tak).

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Definicja limitu ⚠ | `Algorytm.JakEkwiwalentZaUrlop.DefinicjaLimitu` | słownik: `DefinicjaLimitu` |  |  |  | Limit nieobecności, którego dotyczy ekwiwalent (np. urlop wypoczynkowy). |
| Standardowy okres naliczania podstawy | `Algorytm.JakEkwiwalentZaUrlop.StdOkresPodstawy` | bool |  | Tak |  | Tak – podstawa z okresu standardowego dla ekwiwalentu; Nie – własna liczba miesięcy. |
| Okres naliczania podstawy (miesiące) | `Algorytm.JakEkwiwalentZaUrlop.OkresPodstawy` | int | warunk. |  | edytowalne: „Standardowy okres naliczania podstawy” = Nie; wymagane: „Standardowy okres naliczania podstawy” = Nie; walidacja: {"min":1,"max":24} | Liczba miesięcy podstawy, gdy okres nie jest standardowy. |
| Standardowy współczynnik | `Algorytm.JakEkwiwalentZaUrlop.StdWspółczynnik` | bool |  | Tak |  | Tak – miesięczna podstawa dzielona przez współczynnik standardowy (nominalna liczba dni pracy w roku / 12). Nie – własny współczynnik (np. dla odprawy liczonej jak ekwiwalent). |
| Współczynnik | `Algorytm.JakEkwiwalentZaUrlop.Ulamek` | fraction | warunk. |  | edytowalne: „Standardowy współczynnik” = Nie; wymagane: „Standardowy współczynnik” = Nie; walidacja: {"wzorzec":"^\\s*\\d\u002B([.,]\\d\u002B)?\\s*(/\\s*\\d\u002B([.,]\\d\u002B)?)?\\s*$","komunikat":"Wpisz u\u0142amek, np. 21/1 lub 2/3."} | Własny współczynnik w postaci ułamka (np. 21/1), gdy współczynnik nie jest standardowy. |
| Uwzględniaj wymiar etatu we współczynniku | `Algorytm.JakEkwiwalentZaUrlop.WgWymiaruEtatu` | bool |  | Tak ⓓ |  | Czy we współczynniku / średniej miesięcznej normie czasu pracy uwzględniać wymiar etatu pracownika. |

### Algorytm naliczania wartości / Za okres nieobecności

Sekcja widoczna, gdy: „Algorytm” = Za okres nieobecności.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Bazowa definicja elementu | `Algorytm.ZaNieobecnosc.Podstawowa` | słownik: `DefinicjaElementu` | warunk. |  | wymagane: „Algorytm” = Za okres nieobecności lub („Algorytm” = Kreator algorytmu i „Podstawa” = Za okres nieobecności) | Element wypłacany za okres obecności, którego pomniejszenia za wybrane nieobecności ma wypłacić ten element (np. „Dodatek stażowy oskładkowany”). Bazowy element musi mieć wartość „za miesiąc” i niższy priorytet liczbowo niż ten element. |

### Algorytm naliczania wartości / Za okres nieobecności / Wypłata elementu za okres

Sekcja widoczna, gdy: „Algorytm” = Za okres nieobecności.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Zwolnień lekarskich (wszystkich ZUS) | `Algorytm.ZaNieobecnosc.ZUS` | bool |  |  | widoczne: nie (licencja zlota) | W wersji srebrnej: wypłacaj element za okres wszystkich zwolnień ZUS (chorobowe, macierzyńskie, rehabilitacyjne, opiekuńcze) łącznie. |
| Urlopów macierzyńskich | `Algorytm.ZaNieobecnosc.Macierzynskiego` | bool |  | Nie | widoczne: licencja zlota | Wypłacaj element za okres urlopów macierzyńskich. |
| Urlopów rehabilitacyjnych | `Algorytm.ZaNieobecnosc.Rehabilitacyjnego` | bool |  | Nie | widoczne: licencja zlota | Wypłacaj element za okres świadczeń rehabilitacyjnych. |
| Urlopów opiekuńczych | `Algorytm.ZaNieobecnosc.Opiekunczego` | bool |  | Nie | widoczne: licencja zlota | Wypłacaj element za okres zasiłków opiekuńczych. |
| Zwolnień chorobowych | `Algorytm.ZaNieobecnosc.Chorobowego` | bool |  | Nie | widoczne: licencja zlota | Wypłacaj element za okres zwolnień chorobowych. |
| Urlopów wychowawczych | `Algorytm.ZaNieobecnosc.Wychowawczego` | bool |  | Nie |  | Wypłacaj element za okres urlopów wychowawczych. |

### Algorytm naliczania wartości / Edytor

Sekcja widoczna, gdy: „Algorytm” = Edytor algorytmu.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Wartość za | `Algorytm.KreatorAlgorytmu.PodstawaZa` | lista: `PodstawaAlgorytmuZa` |  | Nie zależy od czasu | wartości zależne od warunków | Czy wartość zależy od czasu pracy: nie zależy od czasu (np. premia kwotowa), za godzinę (stawka × liczba godzin), miesięcznie (kwota miesięczna pomniejszana za nieobecności – ustawienia pomniejszeń poniżej). |

### Algorytm naliczania wartości / Edytor / Korekta wartości z tytułu nieobecności (pomniejszenia)

Sekcja widoczna, gdy: „Algorytm” = Edytor algorytmu i „Wartość za” = Miesięcznie.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Nieobecności nieusprawiedliwione | `Algorytm.KreatorAlgorytmu.Korekta.Nieusprawiedliwione` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za nieobecności nieusprawiedliwione: nie pomniejsza, proporcjonalnie (do godzin), 1/30 za dzień, jak zasadnicze, za każdy dzień, proporcjonalnie zero, algorytm (tylko edytor). |
| Nieobecności niepłatne | `Algorytm.KreatorAlgorytmu.Korekta.Usprawiedliwione` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za nieobecności usprawiedliwione bez prawa do wynagrodzenia (np. urlop bezpłatny). |
| Urlopy i inne płatne | `Algorytm.KreatorAlgorytmu.Korekta.Platne` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za urlopy wypoczynkowe, okolicznościowe i inne płatne nieobecności. Zwykle nie pomniejsza – za urlop płaci się wynagrodzenie urlopowe. „Proporcjonalnie zero” – gdy wynagrodzenie za urlop ma wyjść zerowe, a element nie ma być pomniejszony. |
| Urlopy wychowawcze | `Algorytm.KreatorAlgorytmu.Korekta.Wychowawcze` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za urlopy wychowawcze. |
| Zwolnienia lekarskie (wszystkie ZUS) | `Algorytm.KreatorAlgorytmu.Korekta.ZUS` | lista: `RodzajPomniejszenia` |  |  | widoczne: nie (licencja zlota); wartości zależne od warunków | W wersji srebrnej: jedno ustawienie pomniejszania dla wszystkich nieobecności ZUS. |
| Urlopy macierzyńskie | `Algorytm.KreatorAlgorytmu.Korekta.Macierzynske` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za urlopy macierzyńskie (i rodzicielskie). |
| Urlopy rehabilitacyjne | `Algorytm.KreatorAlgorytmu.Korekta.Rehabilitacyjne` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za okres świadczenia rehabilitacyjnego. |
| Urlopy opiekuńcze (ZUS) | `Algorytm.KreatorAlgorytmu.Korekta.Opiekuncze` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za okres zasiłku opiekuńczego. |
| Zwolnienia chorobowe | `Algorytm.KreatorAlgorytmu.Korekta.Chorobowe` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za zwolnienia chorobowe. Np. dodatek stażowy, który przysługuje w czasie choroby: nie pomniejsza. |
| Ogranicz pomniejszenia do wartości elementu | `Algorytm.KreatorAlgorytmu.Korekta.OgraniczeniePomniejszen` | bool |  | Nie | widoczne: licencja zlota | Tak – suma pomniejszeń nie przekroczy wartości elementu, co chroni przed ujemnymi elementami wypłaty. |

### Algorytm naliczania wartości / Edytor / Korekta wartości z tytułu nieobecności (pomniejszenia) / Korekta wartości z tytułu czasu pracy (odchyłki)

Sekcja widoczna, gdy: „Algorytm” = Edytor algorytmu i „Wartość za” = Miesięcznie.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Odchyłki czasu pracy | `Algorytm.KreatorAlgorytmu.Korekta._Odchyłki` | lista: `KorektaOdchyłek` |  |  | widoczne: licencja zlota | Czy element zmienia się, gdy pracownik przepracuje więcej lub mniej godzin niż w normie (poza nieobecnościami): nie, wszystkich (plus i minus), wybranych (wskazujesz rodzaje poniżej). Np. dodatek stały wliczany do normalnego wynagrodzenia za nadgodziny: wszystkich. |
| Odchyłki czasu pracy | `Algorytm.KreatorAlgorytmu.Korekta.Odchylki` | bool |  | Nie | widoczne: nie (licencja zlota) | W wersji srebrnej: Tak – element zwiększa się o nadgodziny i zmniejsza o niedogodziny. |
| Odchyłki plus | `Algorytm.KreatorAlgorytmu.Korekta._OdchyłkiPlus` | lista: `KorektaOdchyłek` |  |  | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych | Które odchyłki dodatnie zwiększają element: nie, wszystkich (dobowe i średniotygodniowe), wybranych (50%, 100%, świąteczne, pozostałe). |
| Odchyłki plus – nadgodziny 50% | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlus50` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Odchyłki mające pokrycie w nadgodzinach 50%. |
| Odchyłki plus – nadgodziny 100% | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlus100` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Odchyłki mające pokrycie w nadgodzinach 100%. |
| Odchyłki plus – nadgodziny świąteczne | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlusSW` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Odchyłki mające pokrycie w nadgodzinach świątecznych. |
| Odchyłki plus – pozostałe | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlusPozostale` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Godziny ponad normę bez dopłat do nadgodzin, np. zbilansowane w okresie rozliczeniowym albo przepracowane przez osobę na części etatu poniżej progu nadgodzin. |
| Odchyłki minus | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiMinus` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych | Tak – element zmniejsza się, gdy pracownik przepracuje mniej godzin niż w normie (niedopracowanie). |
| Odchyłki akordowe | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiAkord` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych | Tak – uwzględniaj odchyłki z pracy akordowej. |
| Odchyłki za czas przestoju ⚠ | `Algorytm.KreatorAlgorytmu.Korekta.Postojowe` | bool |  | Nie | widoczne: (licencja zlota i „Odchyłki czasu pracy” ≠ Nie) lub (nie (licencja zlota) i „Odchyłki czasu pracy” = Tak) | Czy uwzględniać odchyłki za czas przestoju. Pole dostępne, gdy element uwzględnia odchyłki czasu pracy. Instrukcja nie opisuje pola szczegółowo. |
| Odchyłki indywidualnie rozliczane | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiRozliczaneUI` | bool |  |  | widoczne: (licencja zlota i „Odchyłki czasu pracy” ≠ Nie) lub (nie (licencja zlota) i „Odchyłki czasu pracy” = Tak) | Tak – element jest pomniejszany za czas ze stref z „indywidualnym rozliczeniem” (standardowo: przerwa na karmienie). Tak jest ustawione na standardowym wynagrodzeniu zasadniczym. |

### Algorytm naliczania wartości / Pomniejszenia i odchyłki

Sekcja widoczna, gdy: „Algorytm” = Pomniejszenia i odchyłki.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Strefa indywidualnie rozliczana ⚠ | `Algorytm.PomniejszeniaIOdchylki.WybranaStrefa` | słownik: `DefinicjaStrefy` |  |  |  | Dla elementów rodzaju Odchyłki z algorytmem „Pomniejszenia i odchyłki”: strefa czasu pracy rozliczana indywidualnie, za którą element jest liczony. Puste – pomniejszenia i odchyłki ustawia się jak w kreatorze. Instrukcja nie opisuje tej metody. |

### Algorytm naliczania wartości / Pomniejszenia i odchyłki / Korekta wartości z tytułu nieobecności (pomniejszenia)

Sekcja widoczna, gdy: „Algorytm” = Pomniejszenia i odchyłki i „Strefa indywidualnie rozliczana” puste.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Nieobecności nieusprawiedliwione | `Algorytm.KreatorAlgorytmu.Korekta.Nieusprawiedliwione` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za nieobecności nieusprawiedliwione: nie pomniejsza, proporcjonalnie (do godzin), 1/30 za dzień, jak zasadnicze, za każdy dzień, proporcjonalnie zero, algorytm (tylko edytor). |
| Nieobecności niepłatne | `Algorytm.KreatorAlgorytmu.Korekta.Usprawiedliwione` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za nieobecności usprawiedliwione bez prawa do wynagrodzenia (np. urlop bezpłatny). |
| Urlopy i inne płatne | `Algorytm.KreatorAlgorytmu.Korekta.Platne` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za urlopy wypoczynkowe, okolicznościowe i inne płatne nieobecności. Zwykle nie pomniejsza – za urlop płaci się wynagrodzenie urlopowe. „Proporcjonalnie zero” – gdy wynagrodzenie za urlop ma wyjść zerowe, a element nie ma być pomniejszony. |
| Urlopy wychowawcze | `Algorytm.KreatorAlgorytmu.Korekta.Wychowawcze` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za urlopy wychowawcze. |
| Zwolnienia lekarskie (wszystkie ZUS) | `Algorytm.KreatorAlgorytmu.Korekta.ZUS` | lista: `RodzajPomniejszenia` |  |  | widoczne: nie (licencja zlota); wartości zależne od warunków | W wersji srebrnej: jedno ustawienie pomniejszania dla wszystkich nieobecności ZUS. |
| Urlopy macierzyńskie | `Algorytm.KreatorAlgorytmu.Korekta.Macierzynske` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za urlopy macierzyńskie (i rodzicielskie). |
| Urlopy rehabilitacyjne | `Algorytm.KreatorAlgorytmu.Korekta.Rehabilitacyjne` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za okres świadczenia rehabilitacyjnego. |
| Urlopy opiekuńcze (ZUS) | `Algorytm.KreatorAlgorytmu.Korekta.Opiekuncze` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za okres zasiłku opiekuńczego. |
| Zwolnienia chorobowe | `Algorytm.KreatorAlgorytmu.Korekta.Chorobowe` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za zwolnienia chorobowe. Np. dodatek stażowy, który przysługuje w czasie choroby: nie pomniejsza. |
| Ogranicz pomniejszenia do wartości elementu | `Algorytm.KreatorAlgorytmu.Korekta.OgraniczeniePomniejszen` | bool |  | Nie | widoczne: licencja zlota | Tak – suma pomniejszeń nie przekroczy wartości elementu, co chroni przed ujemnymi elementami wypłaty. |

### Algorytm naliczania wartości / Pomniejszenia i odchyłki / Korekta wartości z tytułu nieobecności (pomniejszenia) / Korekta wartości z tytułu czasu pracy (odchyłki)

Sekcja widoczna, gdy: „Algorytm” = Pomniejszenia i odchyłki i „Strefa indywidualnie rozliczana” puste.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Odchyłki czasu pracy | `Algorytm.KreatorAlgorytmu.Korekta._Odchyłki` | lista: `KorektaOdchyłek` |  |  | widoczne: licencja zlota | Czy element zmienia się, gdy pracownik przepracuje więcej lub mniej godzin niż w normie (poza nieobecnościami): nie, wszystkich (plus i minus), wybranych (wskazujesz rodzaje poniżej). Np. dodatek stały wliczany do normalnego wynagrodzenia za nadgodziny: wszystkich. |
| Odchyłki czasu pracy | `Algorytm.KreatorAlgorytmu.Korekta.Odchylki` | bool |  | Nie | widoczne: nie (licencja zlota) | W wersji srebrnej: Tak – element zwiększa się o nadgodziny i zmniejsza o niedogodziny. |
| Odchyłki plus | `Algorytm.KreatorAlgorytmu.Korekta._OdchyłkiPlus` | lista: `KorektaOdchyłek` |  |  | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych | Które odchyłki dodatnie zwiększają element: nie, wszystkich (dobowe i średniotygodniowe), wybranych (50%, 100%, świąteczne, pozostałe). |
| Odchyłki plus – nadgodziny 50% | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlus50` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Odchyłki mające pokrycie w nadgodzinach 50%. |
| Odchyłki plus – nadgodziny 100% | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlus100` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Odchyłki mające pokrycie w nadgodzinach 100%. |
| Odchyłki plus – nadgodziny świąteczne | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlusSW` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Odchyłki mające pokrycie w nadgodzinach świątecznych. |
| Odchyłki plus – pozostałe | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlusPozostale` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Godziny ponad normę bez dopłat do nadgodzin, np. zbilansowane w okresie rozliczeniowym albo przepracowane przez osobę na części etatu poniżej progu nadgodzin. |
| Odchyłki minus | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiMinus` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych | Tak – element zmniejsza się, gdy pracownik przepracuje mniej godzin niż w normie (niedopracowanie). |
| Odchyłki akordowe | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiAkord` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych | Tak – uwzględniaj odchyłki z pracy akordowej. |
| Odchyłki za czas przestoju ⚠ | `Algorytm.KreatorAlgorytmu.Korekta.Postojowe` | bool |  | Nie | widoczne: (licencja zlota i „Odchyłki czasu pracy” ≠ Nie) lub (nie (licencja zlota) i „Odchyłki czasu pracy” = Tak) | Czy uwzględniać odchyłki za czas przestoju. Pole dostępne, gdy element uwzględnia odchyłki czasu pracy. Instrukcja nie opisuje pola szczegółowo. |
| Odchyłki indywidualnie rozliczane | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiRozliczaneUI` | bool |  |  | widoczne: (licencja zlota i „Odchyłki czasu pracy” ≠ Nie) lub (nie (licencja zlota) i „Odchyłki czasu pracy” = Tak) | Tak – element jest pomniejszany za czas ze stref z „indywidualnym rozliczeniem” (standardowo: przerwa na karmienie). Tak jest ustawione na standardowym wynagrodzeniu zasadniczym. |

### Algorytm naliczania wartości / Kreator algorytmu / Podstawa

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Podstawa | `Algorytm.KreatorAlgorytmu.PodstawaTyp` | lista: `TypPodstawyAlgorytmu` | ✔ | Kwota ⓓ | wartości zależne od warunków | Od czego liczony jest element: kwota (np. 300 zł), cecha pracownika, wynagrodzenia lub zasiłki z konfiguracji (np. najniższe), wskaźnik firmy, stawki godzinowe (najniższa, zaszeregowania), wynagrodzenie zasadnicze (nominalne, rzeczywiste), brutto, suma wybranych elementów, podstawa urlopu itd. Kod metody i wyrażenie wymagają programowania – opisz zasadę w uwagach. |
| Kwota | `Algorytm.KreatorAlgorytmu.Podstawa` | currency |  |  | widoczne: „Podstawa” = Kwota; walidacja: {"min":0} | Kwota podstawy, np. 100 zł premii uznaniowej. Może być potem zmieniana indywidualnie u pracownika, jeśli parametr ma nazwę. |
| Cecha | `Algorytm.KreatorAlgorytmu.Cecha` | string | warunk. |  | widoczne: „Podstawa” = Cecha; wymagane: „Podstawa” = Cecha | Nazwa cechy (np. pracownika lub etatu), z której pobierana jest kwota podstawy. Cecha musi istnieć w bazie – podaj jej nazwę, wdrożeniowiec wskaże pełną ścieżkę. |
| Wskaźnik | `Algorytm.KreatorAlgorytmu.Wskaźniki` | string | warunk. |  | widoczne: „Podstawa” = Wskaźniki; wymagane: „Podstawa” = Wskaźniki | Nazwa wskaźnika zdefiniowanego w konfiguracji płac (Płace/Wskaźniki), np. stawka dodatku za pracę w warunkach szkodliwych za godzinę. |
| Wynagrodzenie | `Algorytm.KreatorAlgorytmu.Wynagrodzenia` | lista: `TypWynagrodzenia` | warunk. |  | widoczne: „Podstawa” = Wynagrodzenia; wymagane: „Podstawa” = Wynagrodzenia | Które wynagrodzenie z konfiguracji płac jest podstawą (np. najniższe wynagrodzenie, przeciętne miesięczne). |
| Zasiłek | `Algorytm.KreatorAlgorytmu.Zasiłki` | lista: `TypZasiłku` | warunk. |  | widoczne: „Podstawa” = Zasiłki; wymagane: „Podstawa” = Zasiłki | Który zasiłek z konfiguracji płac (rodzinne, pielęgnacyjne, wychowawcze…) jest podstawą. |

### Algorytm naliczania wartości / Kreator algorytmu / Podstawa / Za okres nieobecności

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu i „Podstawa” = Za okres nieobecności.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Bazowa definicja elementu | `Algorytm.ZaNieobecnosc.Podstawowa` | słownik: `DefinicjaElementu` | warunk. |  | wymagane: „Algorytm” = Za okres nieobecności lub („Algorytm” = Kreator algorytmu i „Podstawa” = Za okres nieobecności) | Element wypłacany za okres obecności, którego pomniejszenia za wybrane nieobecności ma wypłacić ten element (np. „Dodatek stażowy oskładkowany”). Bazowy element musi mieć wartość „za miesiąc” i niższy priorytet liczbowo niż ten element. |

### Algorytm naliczania wartości / Kreator algorytmu / Podstawa / Za okres nieobecności / Wypłata elementu za okres

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu i „Podstawa” = Za okres nieobecności.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Zwolnień lekarskich (wszystkich ZUS) | `Algorytm.ZaNieobecnosc.ZUS` | bool |  |  | widoczne: nie (licencja zlota) | W wersji srebrnej: wypłacaj element za okres wszystkich zwolnień ZUS (chorobowe, macierzyńskie, rehabilitacyjne, opiekuńcze) łącznie. |
| Urlopów macierzyńskich | `Algorytm.ZaNieobecnosc.Macierzynskiego` | bool |  | Nie | widoczne: licencja zlota | Wypłacaj element za okres urlopów macierzyńskich. |
| Urlopów rehabilitacyjnych | `Algorytm.ZaNieobecnosc.Rehabilitacyjnego` | bool |  | Nie | widoczne: licencja zlota | Wypłacaj element za okres świadczeń rehabilitacyjnych. |
| Urlopów opiekuńczych | `Algorytm.ZaNieobecnosc.Opiekunczego` | bool |  | Nie | widoczne: licencja zlota | Wypłacaj element za okres zasiłków opiekuńczych. |
| Zwolnień chorobowych | `Algorytm.ZaNieobecnosc.Chorobowego` | bool |  | Nie | widoczne: licencja zlota | Wypłacaj element za okres zwolnień chorobowych. |
| Urlopów wychowawczych | `Algorytm.ZaNieobecnosc.Wychowawczego` | bool |  | Nie |  | Wypłacaj element za okres urlopów wychowawczych. |

### Algorytm naliczania wartości / Kreator algorytmu / Przeliczenie podstawy

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Pomniejsz za okres niezatrudnienia | `Algorytm.KreatorAlgorytmu.SposobProporcjonalnosci` | lista: `SposobyProporcjonalności` |  | Nie |  | Jak zmniejszyć element, gdy pracownik nie był zatrudniony cały miesiąc: nie, proporcjonalnie, o 1/30 za każdy dzień kalendarzowy niezatrudnienia, wg dni kalendarzowych miesiąca. |
| Przelicz na wartość za 1 godzinę | `Algorytm.KreatorAlgorytmu.PrzeliczNa1h` | lista: `SposobyPrzeliczeniaNa1h` |  | Nie przeliczaj |  | Czy podstawę przeliczyć na stawkę godzinową: nie przeliczaj, wg planu (norma pracownika), wg normy kodeksowej (art. 130 KP), wg planu z uwzględnieniem art. 130 KP, wg wybranej strefy. |
| Definicja strefy (przeliczenie na 1h) ⚠ | `Algorytm.KreatorAlgorytmu.Strefa` | słownik: `DefinicjaStrefy` | warunk. |  | widoczne: „Przelicz na wartość za 1 godzinę” = Wg wybranej strefy; wymagane: „Przelicz na wartość za 1 godzinę” = Wg wybranej strefy | Strefa czasu pracy, wg której przeliczana jest stawka za godzinę przy opcji „Wg wybranej strefy”. Instrukcja nie opisuje tej opcji. |

### Algorytm naliczania wartości / Kreator algorytmu / Mnożnik

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Typ mnożnika | `Algorytm.KreatorAlgorytmu.Wspolczynnik.Typ` | lista: `TypWspolczynnikaAlgorytmu` |  | Bez współczynnika |  | Przez co mnożona jest podstawa: bez współczynnika (×1), procent (np. premia 10%), ułamek (np. 1/2), współczynnik (liczba), zależny od stażu pracy (progi na zakładce Staż pracy) lub od stażu liczonego zwyczajowo. |
| Mnożnik ze wskaźnika | `Algorytm.KreatorAlgorytmu.Wspolczynnik.Wskaznik` | bool |  | Nie | widoczne: „Typ mnożnika” ∈ {Procent, Ułamek, Współczynnik} | Tak – wartość mnożnika (procent, ułamek, współczynnik) pobierana jest ze wskaźnika zdefiniowanego w konfiguracji płac zamiast wpisywać ją tutaj. |
| Nazwa wskaźnika (mnożnik) | `Algorytm.KreatorAlgorytmu.Wspolczynnik.NazwaWskaznika` | string(30) | warunk. |  | widoczne: „Typ mnożnika” ∈ {Procent, Ułamek, Współczynnik} i „Mnożnik ze wskaźnika” = Tak; wymagane: „Typ mnożnika” ∈ {Procent, Ułamek, Współczynnik} i „Mnożnik ze wskaźnika” = Tak | Wskaźnik z konfiguracji płac (Płace/Wskaźniki) o typie zgodnym z mnożnikiem (procent, ułamek albo liczba). |
| Współczynnik | `Algorytm.KreatorAlgorytmu.Wspolczynnik.Wspolczynnik` | decimal |  |  | widoczne: „Typ mnożnika” = Współczynnik i „Mnożnik ze wskaźnika” = Nie | Liczba, przez którą mnożona jest podstawa, np. 1,5. |
| Procent | `Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent` | percent |  |  | widoczne: „Typ mnożnika” = Procent i „Mnożnik ze wskaźnika” = Nie; walidacja: {"min":0,"max":1000} | Procent podstawy, np. 10% dla premii 10% od wynagrodzenia zasadniczego. Można go zmieniać indywidualnie u pracownika. |
| Ułamek | `Algorytm.KreatorAlgorytmu.Wspolczynnik.Ulamek` | fraction |  |  | widoczne: „Typ mnożnika” = Ułamek i „Mnożnik ze wskaźnika” = Nie; walidacja: {"wzorzec":"^\\s*\\d\u002B([.,]\\d\u002B)?\\s*(/\\s*\\d\u002B([.,]\\d\u002B)?)?\\s*$","komunikat":"Wpisz u\u0142amek, np. 1/2."} | Ułamek w postaci licznik/mianownik, np. 1/2. |

### Algorytm naliczania wartości / Kreator algorytmu / Zależność od czasu

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Wartość za | `Algorytm.KreatorAlgorytmu.PodstawaZa` | lista: `PodstawaAlgorytmuZa` |  | Nie zależy od czasu | wartości zależne od warunków | Czy wartość zależy od czasu pracy: nie zależy od czasu (np. premia kwotowa), za godzinę (stawka × liczba godzin), miesięcznie (kwota miesięczna pomniejszana za nieobecności – ustawienia pomniejszeń poniżej). |

### Algorytm naliczania wartości / Kreator algorytmu / Zależność od czasu / Korekta wartości z tytułu nieobecności (pomniejszenia)

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu i „Wartość za” = Miesięcznie.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Nieobecności nieusprawiedliwione | `Algorytm.KreatorAlgorytmu.Korekta.Nieusprawiedliwione` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za nieobecności nieusprawiedliwione: nie pomniejsza, proporcjonalnie (do godzin), 1/30 za dzień, jak zasadnicze, za każdy dzień, proporcjonalnie zero, algorytm (tylko edytor). |
| Nieobecności niepłatne | `Algorytm.KreatorAlgorytmu.Korekta.Usprawiedliwione` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za nieobecności usprawiedliwione bez prawa do wynagrodzenia (np. urlop bezpłatny). |
| Urlopy i inne płatne | `Algorytm.KreatorAlgorytmu.Korekta.Platne` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za urlopy wypoczynkowe, okolicznościowe i inne płatne nieobecności. Zwykle nie pomniejsza – za urlop płaci się wynagrodzenie urlopowe. „Proporcjonalnie zero” – gdy wynagrodzenie za urlop ma wyjść zerowe, a element nie ma być pomniejszony. |
| Urlopy wychowawcze | `Algorytm.KreatorAlgorytmu.Korekta.Wychowawcze` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | wartości zależne od warunków | Jak pomniejszać element za urlopy wychowawcze. |
| Zwolnienia lekarskie (wszystkie ZUS) | `Algorytm.KreatorAlgorytmu.Korekta.ZUS` | lista: `RodzajPomniejszenia` |  |  | widoczne: nie (licencja zlota); wartości zależne od warunków | W wersji srebrnej: jedno ustawienie pomniejszania dla wszystkich nieobecności ZUS. |
| Urlopy macierzyńskie | `Algorytm.KreatorAlgorytmu.Korekta.Macierzynske` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za urlopy macierzyńskie (i rodzicielskie). |
| Urlopy rehabilitacyjne | `Algorytm.KreatorAlgorytmu.Korekta.Rehabilitacyjne` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za okres świadczenia rehabilitacyjnego. |
| Urlopy opiekuńcze (ZUS) | `Algorytm.KreatorAlgorytmu.Korekta.Opiekuncze` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za okres zasiłku opiekuńczego. |
| Zwolnienia chorobowe | `Algorytm.KreatorAlgorytmu.Korekta.Chorobowe` | lista: `RodzajPomniejszenia` |  | Nie pomniejsza | widoczne: licencja zlota; wartości zależne od warunków | Jak pomniejszać element za zwolnienia chorobowe. Np. dodatek stażowy, który przysługuje w czasie choroby: nie pomniejsza. |
| Ogranicz pomniejszenia do wartości elementu | `Algorytm.KreatorAlgorytmu.Korekta.OgraniczeniePomniejszen` | bool |  | Nie | widoczne: licencja zlota | Tak – suma pomniejszeń nie przekroczy wartości elementu, co chroni przed ujemnymi elementami wypłaty. |

### Algorytm naliczania wartości / Kreator algorytmu / Zależność od czasu / Korekta wartości z tytułu nieobecności (pomniejszenia) / Korekta wartości z tytułu czasu pracy (odchyłki)

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu i „Wartość za” = Miesięcznie.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Odchyłki czasu pracy | `Algorytm.KreatorAlgorytmu.Korekta._Odchyłki` | lista: `KorektaOdchyłek` |  |  | widoczne: licencja zlota | Czy element zmienia się, gdy pracownik przepracuje więcej lub mniej godzin niż w normie (poza nieobecnościami): nie, wszystkich (plus i minus), wybranych (wskazujesz rodzaje poniżej). Np. dodatek stały wliczany do normalnego wynagrodzenia za nadgodziny: wszystkich. |
| Odchyłki czasu pracy | `Algorytm.KreatorAlgorytmu.Korekta.Odchylki` | bool |  | Nie | widoczne: nie (licencja zlota) | W wersji srebrnej: Tak – element zwiększa się o nadgodziny i zmniejsza o niedogodziny. |
| Odchyłki plus | `Algorytm.KreatorAlgorytmu.Korekta._OdchyłkiPlus` | lista: `KorektaOdchyłek` |  |  | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych | Które odchyłki dodatnie zwiększają element: nie, wszystkich (dobowe i średniotygodniowe), wybranych (50%, 100%, świąteczne, pozostałe). |
| Odchyłki plus – nadgodziny 50% | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlus50` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Odchyłki mające pokrycie w nadgodzinach 50%. |
| Odchyłki plus – nadgodziny 100% | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlus100` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Odchyłki mające pokrycie w nadgodzinach 100%. |
| Odchyłki plus – nadgodziny świąteczne | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlusSW` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Odchyłki mające pokrycie w nadgodzinach świątecznych. |
| Odchyłki plus – pozostałe | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlusPozostale` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych i „Odchyłki plus” = Wybranych | Godziny ponad normę bez dopłat do nadgodzin, np. zbilansowane w okresie rozliczeniowym albo przepracowane przez osobę na części etatu poniżej progu nadgodzin. |
| Odchyłki minus | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiMinus` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych | Tak – element zmniejsza się, gdy pracownik przepracuje mniej godzin niż w normie (niedopracowanie). |
| Odchyłki akordowe | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiAkord` | bool |  | Nie | widoczne: licencja zlota i „Odchyłki czasu pracy” = Wybranych | Tak – uwzględniaj odchyłki z pracy akordowej. |
| Odchyłki za czas przestoju ⚠ | `Algorytm.KreatorAlgorytmu.Korekta.Postojowe` | bool |  | Nie | widoczne: (licencja zlota i „Odchyłki czasu pracy” ≠ Nie) lub (nie (licencja zlota) i „Odchyłki czasu pracy” = Tak) | Czy uwzględniać odchyłki za czas przestoju. Pole dostępne, gdy element uwzględnia odchyłki czasu pracy. Instrukcja nie opisuje pola szczegółowo. |
| Odchyłki indywidualnie rozliczane | `Algorytm.KreatorAlgorytmu.Korekta.OdchylkiRozliczaneUI` | bool |  |  | widoczne: (licencja zlota i „Odchyłki czasu pracy” ≠ Nie) lub (nie (licencja zlota) i „Odchyłki czasu pracy” = Tak) | Tak – element jest pomniejszany za czas ze stref z „indywidualnym rozliczeniem” (standardowo: przerwa na karmienie). Tak jest ustawione na standardowym wynagrodzeniu zasadniczym. |

### Algorytm naliczania wartości / Kreator algorytmu / Zależność od czasu / Czas pracy

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu i „Wartość za” = Za godzinę.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Czas pracy (podstawa) | `Algorytm.KreatorAlgorytmu.Czas.Typ` | lista: `TypCzasuPracy` | warunk. | Nie uwzględniaj | wymagane: „Algorytm” = Kreator algorytmu i „Wartość za” = Za godzinę | Liczba godzin, przez którą mnożona jest stawka za godzinę: wpisany czas (wprowadzany ręcznie, np. z karty pracy), nominalny, rzeczywisty, w nocy, w niedziele i święta, nadgodziny 50%/100%/święta (miesięcznie lub za okres), wg wybranej strefy itd. |
| Czas (wpisany) | `Algorytm.KreatorAlgorytmu.Czas.Czas` | time |  |  | edytowalne: „Czas pracy (podstawa)” = Wpisany czas pracy; walidacja: {"wzorzec":"^\\s*\\d{1,4}:[0-5]\\d\\s*$","komunikat":"Wpisz czas w formacie gg:mm, np. 8:00."} | Domyślna liczba godzin (gg:mm) dla opcji „Wpisany czas pracy” – zwykle zmieniana co miesiąc na dodatku pracownika. |
| Definicja strefy (czas pracy) | `Algorytm.KreatorAlgorytmu.Czas.Strefa` | słownik: `DefinicjaStrefy` | warunk. |  | edytowalne: „Czas pracy (podstawa)” = Wg wybranej strefy; wymagane: „Czas pracy (podstawa)” = Wg wybranej strefy | Strefa czasu pracy, z której liczone są godziny przy opcji „Wg wybranej strefy”, np. Praca w godzinach nocnych, Nadgodziny 50%. |

### Algorytm naliczania wartości / Kreator algorytmu / Pomniejsz do zera

Sekcja widoczna, gdy: „Algorytm” = Kreator algorytmu.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Sposób pomniejszenia do zera ⚠ | `Algorytm.KreatorAlgorytmu.PomniejszDoZera` | lista: `SposóbZerowaniaWartości` |  | Nie zeruj |  | Czy element ma spaść do zera przy wystąpieniu nieobecności: nie zeruj, przy nieobecnościach nieusprawiedliwionych, niepłatnych, ZUS (powyżej podanej liczby dni) albo wg algorytmu. Instrukcja nie opisuje tego pola. |
| Zeruj powyżej (dni) ⚠ | `Algorytm.KreatorAlgorytmu.ProgZerowania` | int |  | 0 | widoczne: „Sposób pomniejszenia do zera” ∈ {Przy wystąpieniu nieobecności nieusprawiedliwionych, Przy wystąpieniu nieobecności niepłatnych, Przy wystąpieniu nieobecności ZUS}; walidacja: {"min":0,"max":31} | Liczba dni nieobecności, po przekroczeniu której element jest zerowany. Instrukcja nie opisuje tego pola. |

### Zaokrąglenie


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Zaokrąglenie – precyzja | `Zaokraglenie.Precyzja` | lista: `PrecyzjaZaokraglenia` |  | Do pełnego grosza ⓓ |  | Dokładność zaokrąglenia wyniku: do grosza (standard), do 5 lub 10 groszy, do złotego, do 5, 10, 50 lub 100 zł. |
| Zaokrąglenie – sposób | `Zaokraglenie.Sposob` | lista: `SposobZaokraglenia` |  | Standardowe |  | Standardowe (matematyczne), zawsze w górę albo zawsze w dół. |
| Podstawa zaokrąglana do pełnego grosza | `Zaokraglenie.Podstawa` | bool |  | Nie |  | Tak – także podstawa obliczeń (wynik pośredni) jest zaokrąglana do grosza. |

## Suma elementów

Zakładka widoczna, gdy: „Algorytm” = Kreator algorytmu i „Podstawa” = Suma elementów. 
Lista elementów, których suma jest podstawą (gdy w algorytmie wybrano podstawę „Suma elementów”).

### Suma elementów


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Elementy sumowane w podstawie ⚠ | `PodstawyDefinicji` | tabela: Definicja elementu, Rodzaj podstawy | warunk. |  | wymagane: „Algorytm” = Kreator algorytmu i „Podstawa” = Suma elementów | Lista elementów, których suma stanowi podstawę (gdy podstawa = „Suma elementów”). Dla każdego wskaż definicję elementu i czy brać wartość wyliczoną, czy nominalną. Instrukcja nie opisuje tej zakładki. |

## Staż pracy

Zakładka widoczna, gdy: („Algorytm” = Kreator algorytmu i „Typ mnożnika” ∈ {Zależny od stażu pracy, Zależny od stażu pracy (liczonego zwyczajowo)}) lub „Algorytm” = Edytor algorytmu. 
Progi stażu i rodzaj stażu dla dodatków zależnych od stażu pracy.

### Zależność od stażu pracy

⚠ Reguła widoczności w enova bez odwzorowania: `AlgorytmDefElementu.IsVisibleKreatorAlgorytmu`.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Praca w organizacji | `Algorytm.KreatorAlgorytmu.Wspolczynnik.PracaWFirmie` | bool |  | Nie | edytowalne: „Typ mnożnika” ∈ {Zależny od stażu pracy, Zależny od stażu pracy (liczonego zwyczajowo)} | Tak – do stażu wlicza się okres pracy w firmie (z historii kartoteki pracownika). |
| Staż w organizacji liczony na | `Algorytm.KreatorAlgorytmu.Wspolczynnik.PracaNaDzien` | lista: `TypPracyNaDzień` |  | Ostatni dzień okresu | edytowalne: „Praca w organizacji” = Tak | Na który dzień liczyć staż w firmie: ostatni dzień okresu, pierwszy dzień okresu, ostatni dzień poprzedniego okresu. |
| Podstawa stażu | `Algorytm.KreatorAlgorytmu.Wspolczynnik.PodstawaStazu` | słownik: `DefPodstawyStazu` |  |  | edytowalne: „Typ mnożnika” ∈ {Zależny od stażu pracy, Zależny od stażu pracy (liczonego zwyczajowo)} lub „Algorytm” = Edytor algorytmu | Okresy spoza firmy wliczane do stażu, np. „Zatrudnienie poza firmą” lub „Okres nauki” (albo podstawa zdefiniowana w konfiguracji Kadr). |

### Progi stażu pracy

⚠ Reguła widoczności w enova bez odwzorowania: `AlgorytmDefElementu.IsVisibleKreatorAlgorytmu`.

| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Progi stażu pracy | `Algorytm.KreatorAlgorytmu.Wspolczynnik.Progi` | tabela: Prog, Wartosc | warunk. |  | edytowalne: „Typ mnożnika” ∈ {Zależny od stażu pracy, Zależny od stażu pracy (liczonego zwyczajowo)}; wymagane: „Typ mnożnika” ∈ {Zależny od stażu pracy, Zależny od stażu pracy (liczonego zwyczajowo)} | Tabela: staż w latach i odpowiadający mu procent dodatku, np. 0 lat – 0%, 5 lat – 5%, 6 lat – 6%… Pierwszy wiersz powinien mieć 0 lat. |

## Rozliczenie czasu umowy

Zakładka widoczna, gdy: „Rodzaj elementu” = Umowa – rozliczenie czasu. 

### Rozliczenie czasu umowy


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Element podstawowy ⚠ | `Algorytm.RozliczenieCzasuUmowy.Podstawowa` | słownik: `DefinicjaElementu` |  |  |  | Dla elementów rozliczających czas umowy cywilnoprawnej: element umowy, którego dotyczy rozliczenie. Instrukcja nie opisuje tej zakładki. |
| Wliczany do rozliczenia umowy ⚠ | `Algorytm.RozliczenieCzasuUmowy.WliczanyDoRozliczenia` | bool |  | Nie |  | Czy element wlicza się do rozliczenia czasu umowy. Instrukcja nie opisuje tej zakładki. |

## Umowy

Zakładka widoczna, gdy: „Rodzaj elementu” = Umowa cywilnoprawna. 
Wymuszenia na formularzu umowy cywilnoprawnej, do której przypięto element.

### Umowy


| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |
|---|---|---|---|---|---|---|
| Rozliczenie umowy | `Algorytm.Umowa.RodzajRozliczenia` | lista: `RodzajeRozliczeniaUmowyAlgorytm` |  | Dowolny |  | Co narzucić na formularzu umowy cywilnoprawnej: dowolny (wybór kwota do wypłaty / stawka za okres), kwota do wypłaty, stawka za okres, stawka za godzinę. |
| Typ wartości umowy | `Algorytm.Umowa.TypWartosci` | lista: `TypWartosciUmowyAlgorytm` |  | Dowolny |  | Czy umowa jest rozliczana od kwoty netto, brutto, czy dowolnie (wybór na umowie). |
| Minimalna stawka godzinowa | `Algorytm.Umowa.MinimalnaStawkaGodz` | lista: `KontrolaMinimalnaStawkaGodz` |  | Nie dotyczy |  | Czy umowa podlega kontroli minimalnej stawki godzinowej: nie dotyczy, opcjonalna (wybór na umowie), dotyczy (zawsze). |

## Pola do weryfikacji (`opisDoWeryfikacji`)

Instrukcja enova nie opisuje tych pól wprost albo opis wymaga potwierdzenia — nie zgadujemy, pole czeka na decyzję.

| Pole | Ścieżka | Zakładka/sekcja | Uwagi |
|---|---|---|---|
| Rodzaj elementu | `RodzajZrodla` | Ogólne / Ogólne | Selektor tabeli DefElementow: nowy wiersz = new DefinicjaElementu(RodzajŹródłaWypłaty). Instrukcja nie opisuje rodzajów w rozdziale o definicjach – opisy wartości i lista ukrytych rodzajów do akceptacji. |
| Kod | `Kod` | Ogólne / Ogólne |  |
| Rodzaj naliczania | `RodzajNaliczania` | Ogólne / Lista płac | IsReadOnlyRodzajNaliczania: edycja tylko z licencją platynową i gdy PlanListyPlac.GetEnabled (moduł planowanych list płac). |
| Rachunek odbiorcy | `Rozliczenie.RachunekOdbiorcy` | Rozliczenie / Odbiorca | Słownik zależny od odbiorcy (RozliczenieElementu.GetListRachunekOdbiorcy) – klucz wyszukiwania rachunku do potwierdzenia. |
| Zaliczaj do czasu pracy | `GUS.ZaliczajDoCzasuPracy` | Dane statystyczne / Dane statystyczne GUS | Widoczność: DefinicjaElementuExtender.IsVisibleGUSZaliczajDoCzasuPracy; OnAdded ustawia Tak dla rodzaju Odchyłki. |
| Uwzględniaj w wyliczeniu luki płacowej | `Dodatkowe.LukaPlacowa` | Dodatkowe / Dodatkowe informacje |  |
| Definicja limitu | `Algorytm.JakEkwiwalentZaUrlop.DefinicjaLimitu` | Algorytm / Jak ekwiwalent za urlop |  |
| Odchyłki za czas przestoju | `Algorytm.KreatorAlgorytmu.Korekta.Postojowe` | Algorytm / Odchyłki czasu pracy | KorektaDefElementu.IsVisiblePostojowe => Odchylki (bool, ustawiane przez _Odchyłki ≠ Nie). |
| Strefa indywidualnie rozliczana | `Algorytm.PomniejszeniaIOdchylki.WybranaStrefa` | Algorytm / Pomniejszenia i odchyłki |  |
| Definicja strefy (przeliczenie na 1h) | `Algorytm.KreatorAlgorytmu.Strefa` | Algorytm / Przeliczenie podstawy |  |
| Sposób pomniejszenia do zera | `Algorytm.KreatorAlgorytmu.PomniejszDoZera` | Algorytm / Pomniejsz do zera |  |
| Zeruj powyżej (dni) | `Algorytm.KreatorAlgorytmu.ProgZerowania` | Algorytm / Pomniejsz do zera |  |
| Elementy sumowane w podstawie | `PodstawyDefinicji` | Suma elementów / Suma elementów | Podlista PodstawaDefinicjiElementu (ElementPodstawy, RodzajPodstawy); weryfikator PriorytetSumyElementowVerifier – sumowane elementy muszą mieć niższy priorytet. |
| Element podstawowy | `Algorytm.RozliczenieCzasuUmowy.Podstawowa` | Rozliczenie czasu umowy / Rozliczenie czasu umowy |  |
| Wliczany do rozliczenia umowy | `Algorytm.RozliczenieCzasuUmowy.WliczanyDoRozliczenia` | Rozliczenie czasu umowy / Rozliczenie czasu umowy |  |

## Reguły enova bez odwzorowania w warunkach

Pola, dla których kod enova ma metodę `IsVisible…`/`IsReadOnly…`, a schemat nie ma `widocznyGdy`/`edytowalnyGdy`/`dozwoloneWartosci`.
Sprawdzić, czy warunek jest potrzebny w formularzu klienta (np. reguły licencyjne lub zależne od bazy można pominąć).

- `Nazwa` (Nazwa): `DefinicjaElementu.IsReadOnlyNazwa` — IsReadOnlyNazwa blokuje tylko definicje standardowe – import ich nie nadpisuje.
- `Deklaracje.PozycjaPIT` (Pozycja na deklaracji PIT): `DeklaracjeDefElementu.IsReadOnlyPozycjaPIT` — IsReadOnlyPozycjaPIT zależy od stanu bazy (definicja już wypłacona) – sprawdza import przy aktualizacji, nie formularz.

## Wartości domyślne do weryfikacji (`domyslnaDoWeryfikacji`)

Wartość początkowa rekordu enova różni się od instrukcji albo nie jest ustawiona (0) — potwierdzić na nowym elemencie w enova.

- `OkresNaliczania.Typ` (Typ okresu naliczania): przyjęto **Co n miesięcy** — Rekord ma 0 (brak wartości); domyślną przyjęto wg standardowych definicji typu Dodatek (CoNMiesięcy/1) – potwierdzić na nowym elemencie w enova.
- `OkresNaliczania.DlaKazdejUmowy` (Naliczanie w okresie danej umowy o pracę): przyjęto **Nie** — Instrukcja nazywa „Domyślnie” ustawieniem domyślnym, a rekord ma wartość początkową Nie (0) – sprawdzić na nowym elemencie.
- `Deklaracje.Zaliczka.PomniejszonaZUS` (Podstawa naliczania podatku pomniejszona o składki ZUS): przyjęto **Nie**
- `Algorytm.JakUrlopWypoczynkowy.StdOkresPodstawy` (Standardowy okres naliczania podstawy): przyjęto **Tak**
- `Algorytm.JakChorobowe.StdOkresPodstawy` (Standardowy okres naliczania podstawy): przyjęto **Tak**
- `Algorytm.JakEkwiwalentZaUrlop.WgWymiaruEtatu` (Uwzględniaj wymiar etatu we współczynniku): przyjęto **Tak** — Instrukcja: domyślnie Tak (dla „jak ekwiwalent” w edytorze); rekord ma false – sprawdzić na nowym elemencie.
- `Algorytm.KreatorAlgorytmu.PodstawaTyp` (Podstawa): przyjęto **Kwota** — Rekord ma 0 (brak wartości) – domyślną „Kwota” potwierdzić na nowym elemencie.
- `Zaokraglenie.Precyzja` (Zaokrąglenie – precyzja): przyjęto **Do pełnego grosza** — Instrukcja: standardowo do pełnego grosza; wartość początkowa rekordu = DoPełnegoZłotego (0) – sprawdzić na nowym elemencie.

## Słowniki (wartości z bazy klienta)

Lista podpowiedzi + możliwość wpisania wartości spoza listy. Import sprawdza istnienie wartości w bazie.

| Słownik | Typ enova | Szukany po | Wartości standardowe |
|---|---|---|---|
| Podstawa stażu pracy | `Soneta.Kadry.DefPodstawyStazu` | Nazwa | 2 |
| Fundusz pożyczkowy (PKZP, ZFM…) | `Soneta.Kadry.IZakresDefinicji` | Nazwa | 0 |
| Tytuł ubezpieczenia | `Soneta.Kadry.TytulUbezpieczenia4` | Kod | 126 |
| Limit nieobecności | `Soneta.Kalend.DefinicjaLimitu` | Nazwa | 14 |
| Strefa czasu pracy | `Soneta.Kalend.DefinicjaStrefy` | Nazwa | 23 |
| Ewidencja środków pieniężnych | `Soneta.Kasa.EwidencjaSP` | Symbol | 0 |
| Odbiorca (kontrahent, bank, urząd, ZUS…) | `Soneta.Kasa.IPodmiotKasowy` | Kod | 0 |
| Rachunek bankowy odbiorcy | `Soneta.Kasa.RachunekBankowyPodmiotu` | Numer | 0 |
| Definicja elementu wynagrodzenia | `Soneta.Place.DefinicjaElementu` | Nazwa | 255 |
| Definicja listy płac | `Soneta.Place.DefinicjaListyPlac` | Symbol | 8 |
| Kod świadczenia/przerwy (RSA) | `Soneta.Place.KodRSA` | Kod | 44 |
| Pozycja na deklaracji PIT | `Soneta.Place.PozycjaPIT` | Kod | 69 |

## Enumy

**ChorobowyZatrudnienie** (`Soneta.Place.ChorobowyZatrudnienie`)

- `Pracownik` = 1 — Pracownik
- `InnyUbezpieczony` = 2 — Inny ubezpieczony
- `JakZatrudnienie` = 3 — Jak zatrudnienie

**DefinicjeMiesiecy** (`Soneta.Place.DefinicjeMiesiecy`)

- `styczen` = 0 — styczeń
- `luty` = 1 — luty
- `marzec` = 2 — marzec
- `kwiecien` = 3 — kwiecień
- `maj` = 4 — maj
- `czerwiec` = 5 — czerwiec
- `lipiec` = 6 — lipiec
- `sierpien` = 7 — sierpień
- `wrzesien` = 8 — wrzesień
- `pazdziernik` = 9 — październik
- `listopad` = 10 — listopad
- `grudzien` = 11 — grudzień

**DlaKażdejUmowy** (`Soneta.Place.DlaKażdejUmowy`)

- `Nie` = 0 — Nie: Element nie jest dzielony na okresy umów.
- `Tak` = 1 — Tak: Element zawsze dzielony na okresy umów o pracę.
- `Domyślnie` = 2 — Domyślnie: Wg ustawienia w konfiguracji płac (Ogólne).

**ElementStanowiKUP** (`Soneta.Place.ElementStanowiKUP`)

- `Domyślnie` = 0 — Domyślnie: Opodatkowane, poza zasiłkami, świadczeniami socjalnymi i elementami niewpływającymi na wypłatę.
- `Wliczany` = 1 — Wliczany: Zawsze koszt firmy.
- `NieWliczany` = 2 — Nie wliczany: Nigdy koszt firmy.
- `PPKPracodawcy` = 3 — PPK Pracodawcy: Koszt z tytułu PPK pracodawcy.

**KategoriaWynagrodzeniaGUS** (`Soneta.Place.KategoriaWynagrodzeniaGUS`)

- `WgDefinicjiElementu` = 0 — Wg definicji elementu
- `WypłataUdziałWZysku` = 1 — Wypłata z tytułu udziału w zysku lub nadwyżce bilnasowej w spółdzielniach
- `DodatkoweWynagrodzenieRoczne` = 2 — Dodatkowe wynagrodzenie roczne dla pracowników jednostek sfery budżetowej
- `Honorarium` = 3 — Honorarium
- `WynagrodzenieAkwizycyjnoProwizyjne` = 4 — Wynagrodzenie akwizycyjno-prowizyjne
- `WynagrodzenieOsobowe` = 5 — Wynagrodzenie osobowe
- `WynagrodzenieNadliczbowe` = 6 — Wynagrodzenie za pracę w godzinach nadliczbowych
- `WynagrodzeniePracaNakładcza` = 7 — Wynagrodzenie osób wykonujących pracę nakładczą
- `WynagrodzenieUmowa` = 8 — Wynagrodzenia z tytułu umowy-zlecenia lub umowy o dzieło
- `NagrodyIPremieUznaniowe` = 9 — Nagrody i premie uznaniowe
- `NieWykazujNaDeklaracjach` = 99 — Nie wykazuj na deklaracjach

**KontrolaMinimalnaStawkaGodz** (`Soneta.Place.KontrolaMinimalnaStawkaGodz`)

- `NieDotyczy` = 0 — Nie dotyczy
- `Opcjonalna` = 1 — Opcjonalna
- `Dotyczy` = 2 — Dotyczy

**KorektaOdchyłek** (`Soneta.Place.KorektaDefElementu+KorektaOdchyłek`)

- `Nie` = 0 — Nie: Odchyłki nie zmieniają elementu.
- `Wszystkich` = 1 — Wszystkich: Zmieniają: plus i minus.
- `Wybranych` = 2 — Wybranych: Wskaż rodzaje odchyłek.

**NaliczaniePit26** (`Soneta.Place.NaliczaniePit26`)

- `Warunkowo` = 0 — Warunkowo: Dla pozycji PIT-4R 1, gdy pracownik spełnia warunki zwolnienia.
- `Tak` = 1 — Tak: Zawsze, gdy pracownik/zleceniobiorca ma ustawione zwolnienie.
- `Nie` = 2 — Nie: Nigdy (np. zasiłki, umowy o dzieło, ryczałt).

**PodstawaAlgorytmuZa** (`Soneta.Place.PodstawaAlgorytmuZa`)

- `NieZależyOdCzasu` = 0 — Nie zależy od czasu: Wartość nie zależy od czasu pracy.
- `Godzinę` = 1 — Za godzinę: Stawka × liczba godzin.
- `Miesięcznie` = 2 — Miesięcznie: Kwota miesięczna pomniejszana za nieobecności.
- `ZaWybraneNieobecności` = 3 — Pomniejszenia za wybrane nieobecności *(ukryta)*: Powiązane z zakładką Pomniejszenia nieobecności (platynowa) – do decyzji.

**PodstawyZajęciaKomorniczego** (`Soneta.Place.PodstawyZajęciaKomorniczego`)

- `Domyślnie` = 0 — Domyślnie: Limity i kwota wolna wg rodzaju elementu.
- `NiePodlegaZajęciu` = 1 — Nie podlega zajęciu
- `DoLimitu` = 2 — Do limitu
- `DoLimituZasiłki` = 3 — Do limitu zasiłki
- `WPełnejWysokości` = 4 — W pełnej wysokości: Bez limitów i kwoty wolnej.
- `TylkoPodatkiISkładki` = 5 — Tylko podatki i składki: Element nie podlega zajęciu, ale jego podatek i składki tak.

**PozycjaRCA** (`Soneta.Place.PozycjaRCA`)

- `NieDotyczy` = 0 — Nie dotyczy
- `ZasiłekRodzinny` = 1 — Zasiłek rodzinny
- `DodatekRodzinny` = 2 — Dodatek rodzinny
- `ZasiłekPielęgnacyjny` = 3 — Zasiłek pielęgnacyjny
- `ZasiłekWychowawczy` = 4 — Zasiłek wychowawczy
- `DodDoZasRodzinnego` = 5 — Dodatek do zasiłku rodzinnego
- `PodstawowaSkładkaNaPFE` = 10 — Podstawowa składka na PFE
- `ZwrotSkładkiNaPPKUczestnika` = 11 — Zwrot składki na PPK uczestnika
- `ZwrotSkładkiNaPPKPracodawcy` = 12 — Zwrot składki na PPK pracodawcy

**PozycjaRP7** (`Soneta.Place.PozycjaRP7`)

- `NieDotyczy` = 0 — Nie dotyczy
- `SkładnikiStałe` = 1 — Składniki stałe: Np. zasadnicze, stażowe, funkcyjne.
- `SkładnikiZmienne` = 2 — Składniki zmienne: Premie, nagrody, nadgodziny, trzynastka.
- `Inne` = 3 — Inne: Np. rekompensaty, honoraria.
- `ŚwiadczeniaWNaturze` = 4 — Świadczenia w naturze
- `ŚwiadczeniaZUbezpieczenia` = 5 — Świadczenia z ubezpieczenia: Zasiłki i wynagrodzenie chorobowe.
- `SkłBezZUSwOkrNieob` = 6 — Skł.bez ZUS w okr.nieob.: Składniki wypłacane obok zasiłku, nieoskładkowane w tym okresie.

**PrecyzjaZaokraglenia** (`Soneta.Place.PrecyzjaZaokraglenia`)

- `DoPełnychSetekZłotych` = -2 — Do pełnych setek złotych
- `DoPełnychPięciuDziesiątekZłotych` = -501 — Do pełnych pięciu dziesiątek złotych
- `DoPełnychDziesiątekZłotych` = -1 — Do pełnych dziesiątek złotych
- `DoPełnychPięciuZłotych` = 500 — Do pełnych pięciu złotych
- `DoPełnegoZłotego` = 0 — Do pełnego złotego
- `DoPełnychPięciuDziesiątekGroszy` = 501 — Do pełnych pięciu dziesiątek groszy
- `DoPełnychDziesiątekGroszy` = 1 — Do pełnych dziesiątek groszy
- `DoPełnychPięciuGroszy` = 502 — Do pełnych pięciu groszy
- `DoPełnegoGrosza` = 2 — Do pełnego grosza

**RodzajeNaliczaniaListPłac** (`Soneta.Place.RodzajeNaliczaniaListPłac`)

- `Wszystkie` = 0 — Wszystkie
- `TylkoRzeczywiste` = 1 — Tylko rzeczywiste
- `TylkoPlanowane` = 2 — Tylko planowane

**RodzajeRozliczeniaUmowyAlgorytm** (`Soneta.Place.RodzajeRozliczeniaUmowyAlgorytm`)

- `Dowolny` = 0 — Dowolny
- `KwotaDoWypłaty` = 1 — Kwota do wypłaty
- `StawkaZaOkres` = 2 — Stawka za okres
- `StawkaZaGodzinę` = 3 — Stawka za godzinę

**RodzajPodstawyElementu** (`Soneta.Place.RodzajPodstawyElementu`)

- `Wartosc` = 0 — Wyliczona: Wartość wyliczona elementu.
- `WartoscNominalna` = 1 — Nominalna: Wartość nominalna (przed pomniejszeniami).

**RodzajPomniejszenia** (`Soneta.Place.RodzajPomniejszenia`)

- `NiePomniejsza` = 0 — Nie pomniejsza: Nieobecność nie zmniejsza elementu.
- `Proporcjonalnie` = 1 — Proporcjonalnie: Proporcjonalnie do godzin nieobecności.
- `JednaTrzydziesta` = 2 — 1/30 za dzień: O 1/30 za każdy dzień kalendarzowy nieobecności.
- `JakZasadnicze` = 3 — Jak zasadnicze: Tak jak wynagrodzenie zasadnicze (godzinowe proporcjonalnie, miesięczne 1/30).
- `ZaKażdyDzień` = 4 — Za każdy dzień: O 1/liczbę dni miesiąca za każdy dzień (np. dochód deklarowany).
- `ProporcjonalnieZero` = 5 — Proporcjonalnie zero: Wynagrodzenie urlopowe zerowe, element niepomniejszony.
- `Algorytm` = 6 — Algorytm: Wg kodu algorytmu (tylko edytor algorytmu).

**RodzajRefundacjiWynagrodzenia** (`Soneta.Place.RodzajRefundacjiWynagrodzenia`)

- `Domyślny` = 0 — Domyślny: Wg reguł enova.
- `NiePodlega` = 1 — Nie podlega
- `Wynagrodzenie` = 2 — Wynagrodzenie
- `PrzestójEkonomiczny` = 3 — Przestój ekonomiczny

**RodzajSkladnikaWynagrodzeniaGUSZ12** (`Soneta.Place.RodzajSkladnikaWynagrodzeniaGUSZ12`)

- `WgDefinicjiElementu` = 0 — Wg definicji elementu
- `WyplataZTytuluUdzialuWZyskuLubNadwyzceBilansowejWSpóldzielniach` = 1 — Wypłata z tytułu udziału w zysku lub nadwyżce bilansowej w spółdzielniach
- `DodatkoweWynagrodzenieRoczneDlaPracownikowJednostekSferyBudzetowej` = 2 — Dodatkowe wynagrodzenie roczne dla pracowników jednostek sfery budżetowej
- `Honorarium` = 3 — Honorarium
- `WynagrodzenieZaCzasNominalny` = 4 — Wynagrodzenie za czas nominalny
- `WynagrodzenieZaPraceWGodzinachNadliczbowych` = 5 — Wynagrodzenie za pracę w godzinach nadliczbowych
- `NagrodyIPremieUznaniowe` = 6 — Nagrody i premie uznaniowe
- `DodatkiZaPraceZmianowa` = 7 — Dodatki za pracę zmianową
- `PremieRegulaminowe` = 8 — Premie regulaminowe
- `WynagrodzenieZasadnicze` = 9 — Wynagrodzenie zasadnicze
- `NieWykazujNaDeklaracjach` = 10 — Nie wykazuj na deklaracjach

**RodzajŹródłaWypłaty** (`Soneta.Place.RodzajŹródłaWypłaty`)

- `Etat` = 1 — Etat (wynagrodzenie zasadnicze): Wynagrodzenie zasadnicze pracownika – element etatu.
- `Nieobecność` = 2 — Nieobecność: Rozliczenie nieobecności: wynagrodzenie za urlop, chorobowe, zasiłek.
- `Umowa` = 3 — Umowa cywilnoprawna: Wypłata do umowy zlecenia, o dzieło, w organach stanowiących.
- `Akord` = 4 — Akord: Wynagrodzenie akordowe.
- `Storno` = 5 — Storno *(ukryta)*
- `Dodatek` = 6 — Dodatek: Dodatek lub potrącenie przypisywane pracownikowi (premia, dodatek funkcyjny, składka PZU…).
- `NadgodzinyI` = 7 — Nadgodziny I: Dopłata do nadgodzin 50% (zwykle wystarcza definicja standardowa).
- `NadgodzinyII` = 8 — Nadgodziny II: Dopłata do nadgodzin 100% (zwykle wystarcza definicja standardowa).
- `NadgodzinyŚw` = 9 — Nadgodziny świąteczne: Dopłata do nadgodzin świątecznych (zwykle wystarcza definicja standardowa).
- `Nocne` = 10 — Nocne: Dodatek za pracę w nocy (zwykle wystarcza definicja standardowa).
- `Kurs` = 11 — Kurs: Różnice kursowe wypłat w walucie.
- `Świadczenie` = 12 — Świadczenie: Świadczenie (np. socjalne).
- `Nagroda` = 13 — Nagroda: Nagroda (powiązana z definicjami nagród i kar w kadrach).
- `Kara` = 14 — Kara: Kara porządkowa (powiązana z definicjami nagród i kar w kadrach).
- `FundPożWpisowe` = 15 — Fundusz pożyczkowy – wpisowe
- `FundPożWycofanie` = 16 — Fundusz pożyczkowy – wycofanie wkładu
- `FundPożSkładka` = 17 — Fundusz pożyczkowy – składka
- `Pożyczka` = 18 — Pożyczka (wypłata)
- `PożyczkaSpłata` = 19 — Pożyczka – spłata
- `Zaliczka` = 21 — Zaliczka (wypłata)
- `SpłataZaliczki` = 22 — Zaliczka – zwrot
- `ZajęcieKomornicze` = 23 — Zajęcie komornicze
- `Odchyłki` = 24 — Odchyłki: Pomniejszenia i odchyłki czasu pracy (zwykle definicje standardowe).
- `DodatekAutomatyczny` = 25 — Dodatek automatyczny: Dodatek naliczany automatycznie wszystkim, bez przypisywania go pracownikowi.
- `ZbiegPracyIRodzicielstwa` = 26 — Zbieg pracy i rodzicielstwa *(ukryta)*
- `PIT40` = 27 — PIT-40 *(ukryta)*
- `ZajęcieKomorniczeZwrotNadpłaty` = 28 — Zajęcie komornicze zwrot nadpłaty *(ukryta)*
- `ZajęcieKomorniczeUznanieNadpłaty` = 29 — Zajęcie komornicze uznanie nadpłaty *(ukryta)*
- `ZajęcieKomorniczeRozliczDepozytu` = 30 — Zajęcie komornicze rozlicz depozytu *(ukryta)*
- `UmowaRozliczenie` = 31 — Umowa – rozliczenie czasu
- `WyrównanieDoMinimalnej` = 32 — Wyrównanie do minimalnej *(ukryta)*
- `ZwrotNadpłatyPPK` = 33 — Zwrot nadpłaty PPK *(ukryta)*
- `PrzychódOdSkładkiPracodawcyPPK` = 34 — Przychód od składki pracodawcy PPK *(ukryta)*

**SposobyProporcjonalności** (`Soneta.Place.SposobyProporcjonalności`)

- `NiePomniejszany` = 0 — Nie: Bez pomniejszenia.
- `Proporcjonalnie` = 1 — Proporcjonalnie: Proporcjonalnie do okresu zatrudnienia.
- `JednaTrzydzista` = 2 — 1/30: 1/30 za każdy dzień kalendarzowy niezatrudnienia.
- `DniKalendarzowe` = 3 — Dni kalendarzowe: Wartość / dni miesiąca × dni zatrudnienia.

**SposobyPrzeliczeniaNa1h** (`Soneta.Place.SposobyPrzeliczeniaNa1h`)

- `NiePrzeliczaj` = 0 — Nie przeliczaj
- `WgPlanu` = 1 — Wg planu: Wg normy z kalendarza pracownika.
- `WgArt130KP` = 2 — Wg normy kodeksowej (art. 130 K.P.): Wg normy kodeksowej.
- `WgPlanuArt130KP` = 3 — Wg planu z uwzględnieniem art. 130 K.P.: Pełne miesiące wg planu, niepełne wg ustawienia konfiguracji.
- `WgStrefy` = 4 — Wg wybranej strefy: Wg wskazanej strefy czasu pracy (do weryfikacji).

**SposobyWliczaniaDoPodstawyChorobowego** (`Soneta.Place.SposobyWliczaniaDoPodstawyChorobowego`)

- `Domyślnie` = 0 — Domyślnie: Wg reguł enova.
- `Zawsze` = 1 — Zawsze: Także po zakończeniu wypłaty dodatku.
- `DoKońcaOkresuWypłaty` = 2 — Do końca okresu wypłaty
- `DoDatyZakończeniaWypłaty` = 3 — Do daty zakończenia wypłaty: Data z dodatku (wymaga ustawienia w konfiguracji zasiłków).

**SposobZaokraglenia** (`Soneta.Place.SposobZaokraglenia`)

- `Standardowe` = 0 — Standardowe
- `ZawszeWDół` = 1 — Zawsze wdół
- `ZawszeWGórę` = 2 — Zawsze wgórę

**SposóbZerowaniaWartości** (`Soneta.Place.SposóbZerowaniaWartości`)

- `NieZeruj` = 0 — Nie zeruj
- `PrzyNieobecnościachNieusprawiedliwionych` = 1 — Przy wystąpieniu nieobecności nieusprawiedliwionych
- `PrzyNieobecnościachNiepłatnych` = 2 — Przy wystąpieniu nieobecności niepłatnych
- `PrzyNieobecnościachZUS` = 3 — Przy wystąpieniu nieobecności ZUS
- `WgAlgorytmu` = 10 — Wg algorytmu

**TrybPodleganiaRozporzadzeniuMF070122** (`Soneta.Place.TrybPodleganiaRozporzadzeniuMF070122`)

- `Domyślnie` = 0 — Domyślnie
- `Nie` = 1 — Nie
- `Tak` = 2 — Tak

**TypAlgorytmuElementu** (`Soneta.Place.TypAlgorytmuElementu`)

- `KreatorAlgorytmu` = 1 — Kreator algorytmu: Podstawa × mnożnik × zależność od czasu – bez programowania.
- `EdytorAlgorytmu` = 2 — Edytor algorytmu: Indywidualny kod – napisze wdrożeniowiec; opisz zasadę w uwagach.
- `KlasaAlgorytmu` = 3 — Klasa algorytmu *(ukryta)*: Opcja historyczna.
- `ZaOkresNieobecności` = 10 — Za okres nieobecności: Część elementu bazowego za wybrane nieobecności ZUS (kod RSA 220).
- `PomniejszeniaIOdchyłki` = 11 — Pomniejszenia i odchyłki: Tylko dla rodzaju Odchyłki.
- `JakUrlopWypoczynkowy` = 20 — Jak urlop wypoczynkowy
- `JakUrlopOkolicznościowy` = 21 — Jak urlop okolicznościowy
- `JakEkwiwalentZaUrlop` = 22 — Jak ekwiwalent za urlop: Np. ekwiwalent, odprawa emerytalna.
- `JakChorobowe` = 23 — Jak chorobowe: Własny zasiłek.

**TypCzasuPracy** (`Soneta.Place.TypCzasuPracy`)

- `NieUwzględniaj` = 0 — Nie uwzględniaj *(ukryta)*
- `WpisanyCzasPracy` = 1 — Wpisany czas pracy: Godziny wpisywane ręcznie (np. z karty pracy).
- `NominalnyCzasPracy` = 2 — Nominalny czas pracy
- `RzeczywistyCzasPracy` = 3 — Rzeczywisty czas pracy
- `CzasPracyWNocy` = 4 — Czas pracy wnocy
- `CzasPracyWNiedzieleIŚwięta` = 5 — Czas pracy wniedziele iświęta
- `CzasPracyWDniDodatkowoWolneOdPracy` = 6 — Czas pracy wdni dodatkowo wolne od pracy
- `CzasPracyWNiedzieleIŚwiętaOrazDniWolneOdPracy` = 7 — Czas pracy wniedziele iświęta oraz dni wolne od pracy
- `LiczbaNadgodzin50` = 8 — Liczba nadgodzin50
- `LiczbaNadgodzin100` = 9 — Liczba nadgodzin100
- `LiczbaNadgodzinŚwięta` = 10 — Liczba nadgodzin święta
- `LiczbaNadgodzin100Święta` = 11 — Liczba nadgodzin 100 święta
- `LiczbaNadgodzinRazem` = 12 — Liczba nadgodzin razem
- `WgWybranejStrefy` = 13 — Wg wybranej strefy
- `CzasPracyWNormie` = 14 — Czas pracy wnormie
- `LiczbaNadgodzin50Okres` = 15 — Liczba nadgodzin 50 (za okres)
- `LiczbaNadgodzin100Okres` = 16 — Liczba nadgodzin 100 (za okres)
- `LiczbaNadgodzinŚwiętaOkres` = 17 — Liczba nadgodzin święta (za okres)
- `LiczbaNadgodzin100ŚwiętaOkres` = 18 — Liczba nadgodzin 100 święta (za okres)
- `LiczbaNadgodzinRazemOkres` = 19 — Liczba nadgodzin razem (za okres)
- `LiczbaNadgodzin100TylkoDoba` = 20 — Liczba nadgodzin 100 dobowe
- `LiczbaNadgodzin100TylkoOkres` = 21 — Liczba nadgodzin 100 okresowe
- `LiczbaNadgodzin100TylkoDobaOkres` = 22 — Liczba nadgodzin 100 dobowe (za okres)
- `LiczbaNadgodzin100TylkoOkresOkres` = 23 — Liczba nadgodzin 100 okresowe (za okres)
- `RzeczywistyCzasPracyUmowa` = 24 — Rzeczywisty czas pracy (umowa)
- `LiczbaNadgodzinŚwiętaTylkoDoba` = 25 — Liczba nadgodzin święta dobowe
- `LiczbaNadgodzinŚwiętaTylkoOkres` = 26 — Liczba nadgodzin święta okresowe
- `LiczbaNadgodzinŚwiętaTylkoDobaOkres` = 27 — Liczba nadgodzin święta dobowe (za okres)
- `LiczbaNadgodzinŚwiętaTylkoOkresOkres` = 28 — Liczba nadgodzin święta okresowe (za okres)

**TypKosztowUzyskaniaPrzychodu** (`Soneta.Place.TypKosztowUzyskaniaPrzychodu`)

- `NieNaliczać` = 0 — Nie naliczać: Bez kosztów uzyskania.
- `ZeStosunkuPracy` = 1 — Ze stosunku pracy: Standardowe koszty pracownicze.
- `ZeStosunkuPracy50` = 2 — Ze stosunku pracy + 50%: Z kosztami autorskimi wg procentu z danych podatkowych pracownika.
- `Procentowe` = 3 — Procentowe: Procent wpisany na definicji (umowy cywilnoprawne).
- `WWysokościStosunkuPracy` = 4 — W wysokości stosunku pracy: Kwota jak ze stosunku pracy (rady nadzorcze, kontrakty menedżerskie).
- `Procentowe50` = 5 — Procentowe 50: Procentowe koszty autorskie dla pozycji PIT-4R.
- `ProcentoweWarunkowo` = 6 — Procentowe warunkowo: Umowy do 200 zł bez kosztów, wyższe – procentowe.
- `WWysokościStosunkuPracyWarunkowo` = 7 — W wysokości stosunku pracy warunkowo: Jak stosunek pracy, ale bez kosztów dla umów do 200 zł.
- `KosztyAutorskie50` = 8 — Koszty autorskie 50: Tylko koszty autorskie, bez kosztów ze stosunku pracy.

**TypLimituPotrącenia** (`Soneta.Place.TypLimituPotrącenia`)

- `NieDotyczy` = 0 — Nie dotyczy: Cała kwota pomniejsza wypłatę, bez logu zajęć.
- `Uwzględniaj` = 1 — Uwzględniaj: Cała kwota pomniejsza wypłatę, widoczna w logu zajęć.
- `Standardowy` = 2 — Standardowy: Do 50% wynagrodzenia netto, z kwotą wolną.
- `Alimenty` = 3 — Alimenty: Do 40% wynagrodzenia netto.
- `Minimalne80` = 10 — 80% minimalnego: Pracownik zachowuje 80% minimalnego netto.
- `Minimalne100` = 11 — 100% minimalnego: Pracownik zachowuje 100% minimalnego netto.

**TypNaliczenia** (`Soneta.Place.TypNaliczenia`)

- `PłatnaZGóry` = 1 — Płatna z góry: Płacona przed przepracowaniem miesiąca; nieobecności rozliczane w kolejnej wypłacie.
- `PłatnaZDołu` = 2 — Płatna z dołu: Płacona po przepracowanym miesiącu (standard).

**TypOkresuNaliczania** (`Soneta.Place.TypOkresuNaliczania`)

- `Jednorazowa` = 1 — Jednorazowa: Wypłata jednorazowa.
- `Każda` = 2 — Każdy okres: Za każdy okres (miesiąc).
- `CoNMiesięcy` = 3 — Co n miesięcy
- `CoNLat` = 4 — Co n lat
- `CoNTygodni` = 5 — Co n tygodni: Tygodnie liczone od dnia tygodnia, w którym wypada 1 stycznia.
- `KażdaWypłata` = 6 — Każda wypłata: Z każdą wypłatą w miesiącu.

**TypPodstawyAlgorytmu** (`Soneta.Place.TypPodstawyAlgorytmu`)

- `Kwota` = 1 — Kwota: Stała kwota wpisana na definicji lub u pracownika.
- `Cecha` = 2 — Cecha: Kwota z cechy pracownika/etatu.
- `Wyrażenie` = 3 — Wyrażenie: Wyrażenie C# – tylko wdrożeniowiec.
- `KodMetody` = 4 — Kod metody: Kod C# – tylko wdrożeniowiec; opisz zasadę w uwagach.
- `Wynagrodzenia` = 10 — Wynagrodzenia: Wartość z konfiguracji (np. najniższe wynagrodzenie).
- `Zasiłki` = 11 — Zasiłki: Kwota zasiłku z konfiguracji.
- `Wskaźniki` = 12 — Wskaźniki: Wskaźnik z konfiguracji płac.
- `NajniższeWynagrodzenie1h` = 20 — Najniższe wynagrodzenie 1h
- `StawkaZaszeregowania1h` = 21 — Stawka zaszeregowania 1h: Zasadnicze miesięczne / norma z kalendarza pracownika.
- `MaksStawkaNajniższe1h` = 22 — Maks(stawka, najniższe) 1h: Większa z: najniższe 1h i stawka zaszeregowania 1h.
- `ZasadniczeNominalne` = 23 — Zasadnicze nominalne: Pełne zasadnicze bez pomniejszeń.
- `ZasadniczeRzeczywiste` = 24 — Zasadnicze rzeczywiste: Zasadnicze po pomniejszeniach za nieobecności.
- `ZasadniczeZNadgodzinami` = 25 — Zasadnicze z nadgodzinami
- `Brutto` = 26 — Brutto: Suma elementów opodatkowanych z zasiłkami.
- `BruttoBezZasiłków` = 27 — Brutto bez zasiłków
- `BruttoBezZasiłkówBezWynChor` = 28 — Brutto bez zasiłków i wynagrodzenia chorobowego
- `NominalneWOkresieZatrudnienia` = 29 — Nominalne w okresie zatrudnienia
- `StawkaNorma1h` = 30 — Stawka norma 1h: Zasadnicze / godziny pon.–pt. (ze świętami, bez sobót i niedziel).
- `NajniższeWynagrodzenieNormatywne1h` = 31 — Najniższe wynagrodzenie normatywne 1h
- `StawkaZaszeregowaniaNormatywna1h` = 32 — Stawka zaszeregowania normatywna 1h: Zasadnicze / norma kodeksowa.
- `ZaOkresNieobecności` = 40 — Za okres nieobecności: Pomniejszenie z innego elementu (bazowa definicja).
- `PodstawaUrlopuWypoczynkowego1h` = 50 — Podstawa urlopu wypoczynkowego 1h
- `PodstawaUrlopuOkolicznościowego1h` = 51 — Podstawa urlopu okolicznościowego 1h
- `PodstawaEkwiwalentu1h` = 52 — Podstawa ekwiwalentu 1h
- `PodstawaNadgodzin1h` = 53 — Podstawa nadgodzin 1h: Stawka zaszeregowania 1h albo 60% wynagrodzenia (wg kartoteki).
- `SumaElementów` = 60 — Suma elementów: Suma wybranych elementów (zakładka Suma elementów).

**TypPodstawyUrlopu** (`Soneta.Place.TypPodstawyUrlopu`)

- `NieWliczać` = 0 — Nie wliczać: Np. nagrody jubileuszowe, odprawy, premie uznaniowe.
- `WliczaćAktualnąWartość` = 1 — Wliczać aktualną wartość: Stałe składniki miesięczne – wartość z miesiąca urlopu.
- `WliczaćPoPrzeliczeniu` = 2 — Wliczać po przeliczeniu: Przeliczenie przy zmianie stawek – wymaga współczynnika.
- `Wliczać` = 3 — Wliczać: Składniki zmienne – z 3 (lub 12) miesięcy.
- `WliczaćJakZasadnicze` = 4 — Wliczać jak zasadnicze: Np. premia procentowa od zasadniczego.

**TypPodstawyZasilku** (`Soneta.Place.TypPodstawyZasilku`)

- `NieWliczać` = 0 — Nie wliczać: Składnik zachowany w okresie zasiłku.
- `WliczaćDla` = 1 — Wliczać dla: W kwocie faktycznie wypłaconej, bez uzupełnienia (np. nadgodziny).
- `DopełniaćWedługGodzinDla` = 2 — Dopełniać według godzin dla: Uzupełnienie z dokładnością do minut.
- `DopełniaćWedługDniDla` = 3 — Dopełniać według dni dla: Uzupełnienie proporcjonalnie do dni pracy.
- `DopełniaćJakZasadniczeDla` = 4 — Dopełniać jak zasadnicze dla: Jak wynagrodzenie zasadnicze (np. premia procentowa).
- `WliczaćWNominalnejWysokościDla` = 5 — Wliczać w nominalnej wysokości dla
- `DopełniaćWedług130Dla` = 6 — Dopełniać według 1/30 dla: Dla elementów pomniejszanych o 1/30 (np. służba zastępcza).

**TypPomniejszeniaFIS** (`Soneta.Place.TypPomniejszeniaFIS`)

- `NiePomniejsza` = 0 — Nie pomniejsza: Np. umowy na PIT-8AR.
- `Pomniejsza` = 1 — Pomniejsza: Pomniejsza zaliczkę bez ograniczenia (np. kontrakty).
- `PomniejszaDoFIS` = 2 — Pomniejsza do FIS: Ograniczona do wysokości zaliczki – składniki ze stosunku pracy.
- `PomniejszaWarunkowo` = 3 — Pomniejsza warunkowo do FIS: Jak „do FIS”, ale nie dla umów ryczałtowych.
- `PomniejszaWarunkowoBezFis` = 4 — Pomniejsza warunkowo: Pomniejsza bez ograniczenia, ale nie dla umów ryczałtowych.

**TypPomniejszeniaPodstawyFIS** (`Soneta.Place.TypPomniejszeniaPodstawyFIS`)

- `False` = 0 — Nie: Bez pomniejszenia (np. umowy ryczałtowe).
- `True` = 1 — Tak: Podstawa pomniejszona o składki ZUS.
- `Warunkowo` = 2 — Warunkowo: Umowy do 200 zł jak ryczałtowe, powyżej – zwykle.

**TypPracyNaDzień** (`Soneta.Place.TypPracyNaDzień`)

- `OstatniDzieńOkresu` = 0 — Ostatni dzień okresu
- `PierwszyDzieńOkresu` = 1 — Pierwszy dzień okresu
- `OstatniDzieńPoprzedniegoOkresu` = 2 — Ostatni dzień poprzedniego okresu

**TypSkładkiFGŚP** (`Soneta.Place.TypSkładkiFGŚP`)

- `Domyślna` = 0 — Domyślna: Wg ustawienia w kartotece pracownika.
- `NieNaliczać` = 1 — Nie naliczać: Np. udział w organach stanowiących.

**TypSkładkiFP** (`Soneta.Place.TypSkładkiFP`)

- `Domyślna` = 0 — Domyślna: Reguły wbudowane w enova.
- `BezPrzyrównania` = 1 — Bez przyrównania: Bez przeliczania na pełny miesiąc.
- `ZPrzyrównaniem` = 2 — Z przyrównaniem: Wartość przeliczana na pełny miesiąc.

**TypUbezpieczeniaSpolecznego** (`Soneta.Place.TypUbezpieczeniaSpolecznego`)

- `NieNaliczać` = 0 — Nie naliczać: Element poza podstawą składek.
- `Naliczać` = 1 — Naliczać: Element w podstawie składek społecznych.
- `NieMniejNiż` = 2 — Nie mniej niż: Tylko dochód deklarowany właściciela.

**TypUbezpieczeniaZdrowotnego** (`Soneta.Place.TypUbezpieczeniaZdrowotnego`)

- `NieNaliczać` = 0 — Nie naliczać
- `Naliczać` = 1 — Naliczać: Element w podstawie składki zdrowotnej.
- `NieMniejNiż` = 2 — Nie mniej niż: Tylko dochód deklarowany właściciela.
- `Warunkowo` = 3 — Warunkowo: Umowy ryczałtowe i zwykłe jednym elementem.

**TypUlgiPodatkowej** (`Soneta.Place.TypUlgiPodatkowej`)

- `NieNaliczać` = 0 — Nie naliczać
- `Naliczać` = 1 — Naliczać: Ulga (kwota zmniejszająca podatek) odliczana od zaliczki.

**TypUmowyCywilnoPrawnej** (`Soneta.Kadry.TypUmowyCywilnoPrawnej`)

- `NieDotyczy` = 0 — Nie dotyczy: Tytuł ubezpieczenia wskazany w polu „Domyślny tytuł ubezpieczenia”.
- `UdziałWOrganach` = 1 — Udział w organach stanowiących: Proponowany tytuł 2240.
- `UmowaODzieło` = 2 — Umowa o dzieło: Bez tytułu ubezpieczenia.
- `UmowaZlecenia` = 3 — Umowa zlecenia: Proponowany tytuł 0411.

**TypWartosciUmowyAlgorytm** (`Soneta.Place.TypWartosciUmowyAlgorytm`)

- `Dowolny` = 0 — Dowolny
- `Brutto` = 1 — Brutto
- `Netto` = 2 — Netto

**TypWspolczynnikaAlgorytmu** (`Soneta.Place.TypWspolczynnikaAlgorytmu`)

- `BezWspółczynnika` = 0 — Bez współczynnika: Mnożnik = 1.
- `ZależnyOdStażuPracy` = 1 — Zależny od stażu pracy: Procent z progów stażu (zakładka Staż pracy).
- `Ułamek` = 2 — Ułamek: Np. 1/2.
- `Procent` = 3 — Procent: Np. 10%.
- `Współczynnik` = 4 — Współczynnik: Liczba, np. 1,5.
- `ZależnyOdStażuPracyZwyczajowego` = 5 — Zależny od stażu pracy (liczonego zwyczajowo): Staż liczony jako kontynuacja zatrudnienia u poprzednich pracodawców.

**TypWspolczynnikaUrlopu** (`Soneta.Place.TypWspolczynnikaUrlopu`)

- `Wynagrodzenia` = 10 — Wynagrodzenia
- `Zasiłki` = 11 — Zasiłki
- `Wskaźniki` = 12 — Wskaźniki
- `NajniższeWynagrodzenie1h` = 20 — Najniższe wynagrodzenie1h
- `StawkaZaszeregowania1h` = 21 — Stawka zaszeregowania1h
- `MaksStawkaNajniższe1h` = 22 — Maks stawka najniższe1h
- `ZasadniczeNominalne` = 23 — Zasadnicze nominalne
- `StawkaZaszeregowania` = 24 — Stawka zaszeregowania
- `WZerowejWartości` = 25 — W zerowej wartości: Element w zerowej wartości.
- `NajniższeWynagrodzenie` = 26 — Najniższe wynagrodzenie

**TypWynagrodzenia** (`Soneta.Place.KreatorAlgorytmu+TypWynagrodzenia`)

- `PrzeciętneMiesięczne` = 1 — Przeciętne miesięczne
- `MinimalnyDochódDeklarowany` = 2 — Minimalny dochód deklarowany
- `Najniższe` = 3 — Najniższe
- `NajniższeUczniaIRoku` = 4 — Najniższe ucznia iroku
- `NajniższeUczniaIIRoku` = 5 — Najniższe ucznia iiroku
- `NajniższeUczniaIIIRoku` = 6 — Najniższe ucznia iiiroku
- `MinimalnaPodstawaZdrowotnego` = 7 — Minimalna podstawa zdrowotnego
- `NajniższeMłodocianyPrzyuczany` = 8 — Najniższe młodociany przyuczany

**TypZaliczkiPodatku** (`Soneta.Place.TypZaliczkiPodatku`)

- `NieNaliczać` = 0 — Nie naliczać: Element nieopodatkowany.
- `NaliczaćWgSkaliPodatkowej` = 1 — Naliczać wg skali podatkowej: Składniki pracownicze – progi skali z danych podatkowych.
- `Procentowe` = 2 — Procentowe: Stały procent wpisany na definicji (np. IFT-1, PIT-8AR).
- `ProcentoweJakUmowy` = 3 — Procentowe jak umowy: Procent z konfiguracji płac (umowy na PIT-11).

**TypZasiłku** (`Soneta.Place.KreatorAlgorytmu+TypZasiłku`)

- `Porodowy` = 1 — Porodowy
- `Pogrzebowy` = 2 — Pogrzebowy
- `Pielęgnacyjny` = 3 — Pielęgnacyjny
- `RodzinnyNa1I2Dziecko` = 4 — Rodzinny na1i2dziecko
- `RodzinnyNa3Dziecko` = 5 — Rodzinny na3dziecko
- `RodzinnyNaKolejneDziecko` = 6 — Rodzinny na kolejne dziecko
- `WychowawczyPodstawaER` = 7 — Wychowawczy podstawa er
- `Wychowawczy` = 8 — Wychowawczy
- `WychowawczySamotni` = 9 — Wychowawczy samotni
- `Wychowawczy3INastępne` = 10 — Wychowawczy3inastępne
- `RodzinnyJednorazowy` = 11 — Rodzinny jednorazowy
- `DodRodzinnyUrodzenieDziecka` = 12 — Dod rodzinny urodzenie dziecka
- `DodRodzinnyKształcenieDo5` = 13 — Dod rodzinny kształcenie do5
- `DodRodzinnyKształceniePow5` = 14 — Dod rodzinny kształcenie pow5
- `DodRodzinnyRokSzkolny` = 15 — Dod rodzinny rok szkolny
- `DodRodzinyNaukaPozaZamiesz` = 16 — Dod rodziny nauka poza zamiesz
- `DodRodzinyNaukaPozaDojazd` = 17 — Dod rodziny nauka poza dojazd
- `ŚwiadczeniePielęgnacyjne` = 18 — Świadczenie pielęgnacyjne

**TypZatrudnienia** (`Soneta.Place.TypZatrudnienia`)

- `Etat` = 1 — Etat: Pracownicy etatowi.
- `Umowa` = 2 — Umowa: Zleceniobiorcy (umowy cywilnoprawne).
- `InnaWyplata` = 3 — Inna wypłata: Osoby niezatrudnione, np. byli pracownicy.
- `Warunkowy` = 9 — Warunkowy: Pracownicy i zleceniobiorcy (np. delegacje, zasiłek rodzinny).
- `WarunkowyLubInna` = 10 — Warunkowy lub inna

**UwzgledniajZwrotSkladkiPPK** (`Soneta.Place.UwzgledniajZwrotSkladkiPPK`)

- `NieDotyczy` = 0 — Nie dotyczy
- `ZwrotPoStroniePracownika` = 1 — Zwrot po stronie pracownika
- `ZwrotPoStroniePracodawcy` = 2 — Zwrot po stronie pracodawcy

**UwzglednienieLukiPlacowe** (`Soneta.Place.UwzglednienieLukiPlacowe`)

- `Domyslnie` = 0 — Domyślnie: Wg reguł enova (do weryfikacji).
- `NieDotyczy` = 1 — Nie dotyczy: Pomijany w luce płacowej.
- `Stale` = 2 — Stałe: Składnik stały.
- `Zmienne` = 3 — Zmienne: Składnik zmienny.

**WliczajDoKosztowPlacyEnum** (`Soneta.Kadry.WliczajDoKosztowPlacyEnum`)

- `Domyślnie` = 0 — Domyślnie
- `Tak` = 1 — Tak
- `Nie` = 2 — Nie

**WliczajDoPodstawyWynagrodzeniaZaNadgodziny** (`Soneta.Place.WliczajDoPodstawyWynagrodzeniaZaNadgodziny`)

- `Nie` = 0 — Nie: Nie wlicza się (domyślnie).
- `WliczajZNominału` = 1 — Wliczaj z nominału: Składnik główny / czas nominalny.
- `WliczajWypłacone` = 2 — Wliczaj wypłacone: Wartość wypłacona / czas przepracowany.

## Pominięte w formularzu klienta

| Pole / sekcja | Ścieżka | Powód |
|---|---|---|
| Standard | `IsStandard` | informacja systemowa (definicja standardowa enova) – nie ustawia się |
| Prawa dostępu | `zakładka Prawa dostępu` | prawa do definicji wg wydziału ustawia administrator enova (licencja platynowa) – nie dotyczy opisu składnika |
|  | `PodlegaRozporzadzeniuMF070122` | tylko do odczytu w enova (wyliczane) |
| , po dopełnieniu | `Nieobecnosci.Urlop.Dopelnienie` | w enova pole „po dopełnieniu” jest aktywne tylko dla ekwiwalentu (NieobecnosciDefElementu włącza je wyłącznie dla Ekwiwalent) |
| Sposób wliczania do luki płacowej | `SposobWliczaniaDoLukiPlacowej` | tylko do odczytu w enova (wyliczane) |
| Nazwa | `Algorytm.Klasa.Nazwa` | klasa algorytmu z własnej biblioteki DLL – konfiguruje wdrożeniowiec |
| Wyrażenie | `Algorytm.KreatorAlgorytmu.Wyrażenie` | wyrażenie w języku C# – instrukcja: „wyłącznie dla autoryzowanego serwisu”; klient opisuje zasadę w uwagach |
| Nazwa log'a | `Algorytm.ZapisObliczen.Nazwa` | nazwa logu obliczeń – ustawienie techniczne |
| Algorytm/Edytor | `zakładka Algorytm/Edytor` | kod algorytmu w C# – pisze wdrożeniowiec na podstawie opisu klienta w polu „Uwagi dla wdrożeniowca” |
| Algorytm/Nazwy parametrów | `zakładka Algorytm/Nazwy parametrów` | nazwy parametrów edytowalnych na dodatku/elemencie wypłaty – ustawia wdrożeniowiec zgodnie z algorytmem (do decyzji, czy pokazywać klientowi) |
| Algorytm/Podgląd | `zakładka Algorytm/Podgląd` | podgląd wygenerowanego kodu – tylko do odczytu |
| Historia | `zakładka Historia` | historyczność definicji (tylko nadgodziny i nocne) – wskazanie innej definicji od daty robi wdrożeniowiec |
| Algorytm/Pomniejszenia nieobecności | `zakładka Algorytm/Pomniejszenia nieobecności` | pomniejszenia wg pojedynczych definicji nieobecności (licencja platynowa, lista z bazy klienta) – do decyzji, czy dodać do formularza |

## Ostrzeżenia generatora

- [DefinicjaElementuEdytor] grupa 'Edytor (roslyn)': nierozwinięte wyrażenie {new DefinicjaElementuEdytorExtender} - pominięta.
- [DefinicjaElementuPro] Include RuntimeDefinitionInfo.form.xml: brak zasobu w bibliotece UI - pominięty.

