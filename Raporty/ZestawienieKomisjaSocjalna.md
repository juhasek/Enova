# Raport – Zestawienie na posiedzenie Komisji Socjalnej

Wzorzec wydruku Enova (`Raporty/ZestawienieKomisjaSocjalna.repx`, format
DevExpress XtraReports, orientacja pozioma), zasilany snippetem
`Raporty/ZestawienieKomisjaSocjalnaSnippet`.

## Kontekst uruchomienia (2026-09-22 — zmiana)

Wydruk uruchamiany jest z **listy Świadczeń socjalnych** (ZFŚS, obiekt
`Soneta.Kadry.SwiadczSocjalne`, tabela `SwiadczeniaSoc`) — **nie** z listy
pracowników, jak w poprzedniej wersji. Na tej liście mogą znajdować się
zarówno aktualni pracownicy, jak i osoby już niezatrudnione, którym
przyznano zapomogę (kwota świadczenia może wynosić nawet **0**).

**Dobór pracowników** zależy od parametru „Tylko świadczenia zaznaczone na
liście":

- **domyślnie (odznaczone)** – raport skanuje **wszystkie** świadczenia
  socjalne (`session.GetKadry().SwiadczeniaSoc`) i bierze każdego
  pracownika, którego świadczenie typu „zapomoga" ma cechę
  **„PosiedzenieKomisji"** (typ `Data`) **równą** dacie z parametru „Data
  posiedzenia komisji" — dopasowanie jest **dokładne** (nie oknem
  czasowym, jak poprzednio). Dotyczy to także pracowników zwolnionych i
  spoza bieżącego filtra listy. Zaznaczenie jest ignorowane.
- **zaznaczone** – tylko wiersze świadczeń zaznaczone na liście (bez
  sprawdzania cechy — ufamy zaznaczeniu operatora).

W obu trybach pracownik bez zapomogi w oknach sum (patrz niżej) nie trafia
na wydruk.

## Parametry wydruku

| Parametr | Pole | Domyślnie | Rola |
|---|---|---|---|
| Data posiedzenia komisji | `PrnParams.DataPosiedzenia` (`Date`) | dziś | **Klucz doboru pracowników** — musi się dokładnie zgadzać z cechą „PosiedzenieKomisji" świadczenia; wyznacza też rok odniesienia `R` dla okien sum i jest datą graniczną wniosku ZFŚS |
| Tylko świadczenia zaznaczone na liście | `PrnParams.TylkoZaznaczeni` (`bool`) | `false` | `false` = skan wszystkich świadczeń wg cechy; `true` = tylko zaznaczone wiersze świadczeń |

Okna **sum** (nie doboru pracowników!) to nadal **całe lata kalendarzowe**,
rok `R` (rok daty posiedzenia) jest **pominięty**:

- **„z 2 lat"**: lata `R-2` i `R-1` → `[1.1.(R-2), 31.12.(R-1)]`
- **„z poprzednich 2 lat"**: lata `R-4` i `R-3` → `[1.1.(R-4), 31.12.(R-3)]`

Przykład: `R = 2026` → „z 2 lat" = 2024 + 2025; „z poprzednich 2 lat" =
2022 + 2023.

## Układ: master-detail, jeden wiersz = jedna zapomoga

**Jeden wiersz wydruku = jedna zapomoga (świadczenie socjalne).**
Pracownik z kilkoma zapomogami zajmuje kilka kolejnych wierszy. Kolumny
„poziomu pracownika" (Lp, Nr ewid., Nazwisko, Data urodzenia, Jednostka
obsługująca, Dochód na członka rodziny) oraz obie sumy wypełniane są
**tylko w pierwszym wierszu grupy** pracownika; w kolejnych wierszach te
komórki są puste. Dwie listy zapomóg (okno bieżące / poprzednie) są
wyrównane wg indeksu wiersza – dłuższa wyznacza liczbę wierszy grupy,
krótsza ma puste komórki w nadmiarowych wierszach. **Pracownik bez żadnej
zapomogi jest pomijany** – nie trafia na wydruk i nie zużywa numeru `Lp`
(numeracja pozostaje ciągła dla osób z zapomogami). Od 2026-09-22 warunek
pomijania sprawdza **trzy** źródła naraz (aktualne świadczenie + oba okna
sum), nie tylko oba okna — bo rok posiedzenia `R` jest celowo pominięty w
oknach sum, więc pracownik z wyłącznie „aktualną" zapomogą (typowy
przypadek — to ona go tu sprowadziła) miałby wcześniej `0` wierszy mimo
realnych danych do pokazania.

**Kwota 0 jest wartością poprawną (2026-09-22):** świadczenie może być
przyznane w kwocie 0 i mimo to jest realną zapomogą — sumy „z 2 lat" /
„z poprzednich 2 lat" pokazują się zawsze (nawet `0,00`), gdy w oknie jest
choć jedna zapomoga; **pusta komórka** oznacza wyłącznie **brak
jakiejkolwiek zapomogi** w danym oknie, nie zerową kwotę. To zmiana
względem poprzedniej wersji, gdzie `FormatKwota` chowała zerowe sumy.

## Kolumny

| Nagłówek | Pole źródła | Zawartość |
|---|---|---|
| Lp. | `Lp` | Liczba porządkowa – tylko w 1. wierszu grupy pracownika |
| Nr ewid. | `NrEwid` | `Pracownik.Kod` – tylko 1. wiersz grupy |
| Nazwisko i Imię | `NazwiskoImie` | `Pracownik.NazwiskoImię` – tylko 1. wiersz grupy |
| Data urodzenia | `DataUrodzenia` | `Pracownik.Historia[Date.Today].Urodzony.Data` – tylko 1. wiersz grupy |
| Jednostka obsługująca | `JednostkaObslugujaca` | Cecha „Jednostka obsługująca" z Wydziału bieżącego etatu pracownika. Cecha typu „element słownika" → `(ElemSlownika) Wydzial.Features["Jednostka obsługująca"]`, wyświetlane `.Nazwa` (fallback `"brak"`). Tylko 1. wiersz grupy |
| Dochód na członka rodziny | `DochodNaCzlonkaRodziny` | Z aktualnego, zatwierdzonego wniosku ZFŚS pracownika (krotka `A1_ZFSS`, dodatek `AltOne.Skanska.Workflow`). Odwzorowuje worker `WniosekZFFSPracownikaWorker.GetProgDochodu`; niezmienione względem poprzedniej wersji. Tylko 1. wiersz grupy |
| Kwota zapomogi z aktualnego świadczenia | `KwotaZapomogiAktualnej` | **Nowa kolumna (2026-09-22).** Suma `Rozliczenie.Kwota` świadczeń, które sprowadziły pracownika na TO zestawienie — czyli tych z cechą `PosiedzenieKomisji` równą parametrowi (albo, w trybie „Tylko zaznaczone", zaznaczonych wierszy). Rok posiedzenia `R` jest celowo pominięty w oknach „z 2 lat"/„z poprzednich 2 lat", więc to zwykle osobna liczba, nie składowa żadnej z tych dwóch sum. Puste = brak takiego świadczenia (nie powinno się zdarzyć, bo to właśnie ono kwalifikuje pracownika do wydruku); `0,00` = świadczenie jest, w kwocie zero. Tylko 1. wiersz grupy |
| Kwota zapomogi z 2 lat | `KwotaZapomogiZ2Lat` | Suma `Rozliczenie.Kwota` zapomóg (świadczeń) z okna bieżącego. Puste = brak zapomogi w oknie; `0,00` = zapomoga jest, ale w kwocie zero. Tylko 1. wiersz grupy |
| Data / Kwota | `DataWyplaty` / `KwotaWyplaty` | i-ta zapomoga z okna bieżącego: `SwiadczSocjalne.Data` (data przyznania) / `Rozliczenie.Kwota` (`N2`). Puste, gdy w tym wierszu nie ma już zapomogi z tego okna |
| Kwota zapomóg z poprzednich 2 lat | `KwotaZapomogPoprzednich2Lat` | Analogicznie, okno poprzednie. Tylko 1. wiersz grupy |
| Data / Kwota | `DataWyplatyPoprzedniej` / `KwotaWyplatyPoprzedniej` | i-ta zapomoga z okna poprzedniego |

**Zapomoga** = `Soneta.Kadry.SwiadczSocjalne`, którego `Definicja` (rodzaj
świadczenia, słownik `DefSwiadczSocjal`) ma w nazwie „zapomog"/„zapomóg" —
odróżnia zapomogi od innych rodzajów świadczeń socjalnych na tym samym
obiekcie (np. „Dopłata do wypoczynku", „Paczka"), które **nie** powinny
wchodzić do tego zestawienia. Data pozycji = `SwiadczSocjalne.Data` (data
przyznania świadczenia — **nie** `Rozliczenie.Data`, zob. TODO niżej).
Kwota = `Rozliczenie.Kwota` (`Currency` → `.Value` na `decimal`).
Świadczenia socjalne nie mają odpowiednika „storna" jak `WypElement` —
nie ma tu filtra analogicznego do `RozliczenieStorna`.

**Wydajność:** dobór pracowników (tryb domyślny) skanuje **całą** tabelę
`SwiadczeniaSoc` (nie ma tu odpowiednika `WgDefinicja`/`WgPracownik` z
filtrem serwerowym po cesze — cechy nie są indeksowane jak pola
bazodanowe). Sumy per pracownik czytają `pracownik.Swiadczenia`
(`SubTable<SwiadczSocjalne>` tego pracownika) — to jest tanie, bo nie
iteruje całej firmy.

## Zależność: dodatek AltOne.Skanska.Workflow

Kolumna „Dochód na członka rodziny" korzysta z klas dodatku
`AltOne.Skanska.Workflow` (środowisko Skanska): `PracownikExt` i `A1ZfssTuple`
z przestrzeni `AltOne.Skanska.Workflow.Extensions` /
`AltOne.Skanska.Workflow.Procesy.WniosekZFSS.Tuples`. Snippet **nie skompiluje
się** w bazie, w której ten dodatek nie jest wczytany. Ta część **nie
zmieniła się** w tej rewizji.

Rok oświadczenia = rok z „Daty posiedzenia komisji"; data graniczna wniosku
(„nie później niż") = data posiedzenia.

## TODO / do weryfikacji na żywej bazie

- **CAŁA nowa logika doboru wg cechy „PosiedzenieKomisji" jest
  niezweryfikowana na żywo** — kompilacja lokalna OK (enova 2512.5.6 +
  `Soneta.Kadry.SwiadczSocjalne` ze standardowych DLL serwera, stuby zamiast
  `AltOne.Skanska.Workflow`), ale bez uruchomienia. Sprawdzić:
  - czy cecha `PosiedzenieKomisji` rzeczywiście istnieje na `SwiadczSocjalne`
    w bazie Skanska i jest typu `Data` (kod ma zabezpieczenie —
    `wartoscCechy is Date` — więc brak/zły typ cechy da **pominięcie**
    świadczenia, nie błąd, ale to wymaga potwierdzenia, że to pożądane
    zachowanie);
  - czy `session.GetKadry().SwiadczeniaSoc` zwraca też świadczenia
    pracowników zwolnionych (guided root — powinno, ale niepotwierdzone
    na żywo);
  - czy `Definicja.Nazwa` zapomóg w bazie Skanska rzeczywiście zawiera
    „zapomog" (ten sam warunek, co poprzednio dla `DefinicjaElementu`, ale
    teraz na `DefinicjaŚwiadczeniaSocjalnego`) — **do potwierdzenia
    osobno**, to inny słownik niż poprzednio;
  - liczba wierszy grupy, puste komórki, sumy — jak poprzednio.
- **Pole daty pozycji `SwiadczSocjalne.Data` vs `Rozliczenie.Data`** —
  przyjęto `Data` (data przyznania świadczenia) jako odpowiednik dawnego
  `WypElement.Data` (data wypłaty). To **założenie, nie potwierdzone** —
  `Rozliczenie.Data` (data rozliczenia płacowego) może być bliższym
  odpowiednikiem „daty wypłaty" z poprzedniej wersji. Do ustalenia z
  klientem, które pole ma znaczenie biznesowe dla kolumn „Data" w
  zestawieniu.
- **Cecha „Jednostka obsługująca"** na Wydziale — bez zmian względem
  poprzedniej wersji (patrz opis kolumny wyżej).

## Błąd „DataComponentBase” przy wywołaniu z Pulpitu — ROZWIĄZANE

Rozwiązane i potwierdzone na żywo 2026-09-07 (commit `e897407`) — patrz
historia commitów. `CustomDataSource` jest ustawiane przez refleksję
(`UstawDaneRaportu`), nie bezpośrednio, żeby uniknąć `CS0012` w hoście
Pulpitu WWW (brak `DevExpress.DataAccess.v24.1.dll` w tamtym procesie). Ten
mechanizm **nie zmienił się** w tej rewizji.

## Jak podpiąć snippet w Enova

1. Otwórz `ZestawienieKomisjaSocjalna` w projektancie wydruków Enova.
2. W „Kod źródłowy" wklej całą zawartość `Raporty/ZestawienieKomisjaSocjalnaSnippet`.
3. Zapisz – Enova skompiluje kod i podepnie klasę
   `ZestawienieKomisjaSocjalnaSnippet` pod wydruk.
4. Uruchom wydruk **z listy Świadczeń socjalnych (ZFŚS)**; w oknie
   parametrów podaj „Datę posiedzenia komisji" — musi się dokładnie
   zgadzać z cechą „PosiedzenieKomisji" na świadczeniach, które mają się
   pojawić na wydruku. „Tylko świadczenia zaznaczone na liście" zostaw
   odznaczone, aby raport sam znalazł wszystkich pracowników wg cechy
   (także zwolnionych).

Przełączenie źródła danych na Świadczenia socjalne nie wymagało modyfikacji
`.repx` – pasmo `Detail` jest płaskie, a „wygaszanie” powtórzonych kolumn i
wyrównanie dwóch list zapomóg realizuje snippet (puste stringi w wierszach
2..N grupy). Struktura `.repx` (komponenty
`BusinessContext`/`BusinessSource`/`BusinessSourceContext`) jest niezależna
od zmiany źródła danych i nie zmieniła się.

**Dodanie kolumny „Kwota zapomogi z aktualnego świadczenia" (2026-09-22)
WYMAGAŁO edycji `.repx`** — nowa komórka nagłówka (`cellNaglAktualnaZapomoga`,
`rowNaglowek`) i nowa komórka danych (`cellKwotaZapomogiAktualnej`,
bindowanie `[KwotaZapomogiAktualnej]`, `rowDane`) wstawione między kolumny
„Dochód na członka rodziny" i „Kwota zapomogi z 2 lat", `Weight="0.95"`
(jak sąsiednie kolumny sum). `Ref` nowych elementów: `51` (nagłówek), `52`
(komórka danych), `53` (jej `ExpressionBindings`) — najwyższe dotąd
niewykorzystane numery w pliku, żeby nie renumerować istniejących
elementów. Kolejne komórki w obu wierszach przesunięte o 1 pozycję
(`Item7`→`Item8` itd.), ich `Ref` pozostały bez zmian. Zweryfikowane:
plik parsuje się jako poprawny XML, brak duplikatów `Ref`.

## Odporność na błędy

- Poszczególne komórki poziomu pracownika liczone są przez `Bezpiecznie(...)`
  – błąd w jednej daje `[BŁĄD: ...]` w tej komórce zamiast wywalenia wydruku.
- Grupa **każdego pracownika** budowana jest w osobnym `try/catch` – błąd
  (np. w pobraniu świadczeń / wniosku ZFŚS) daje jeden wiersz `[BŁĄD: ...]`
  dla tego pracownika, reszta zestawienia się drukuje.
- Cała `BeforePrint` jest w `try/catch` – błąd poza pętlą daje jeden wiersz
  „BŁĄD” z etapem i treścią wyjątku.

Jeśli zobaczysz `[BŁĄD: ...]`, wklej treść – pozwoli poprawić logikę lub dane.
