# Raport – Zestawienie na posiedzenie Komisji Socjalnej

Wzorzec wydruku Enova (`Raporty/ZestawienieKomisjaSocjalna.repx`, format
DevExpress XtraReports, orientacja pozioma), zasilany snippetem
`Raporty/ZestawienieKomisjaSocjalnaSnippet`.

## Kontekst uruchomienia

Wydruk uruchamiany jest z **Listy pracowników** (ew. **Pulpitu kierownika**).
Pracownicy sortowani po „Nazwisko i Imię”.

**Dobór pracowników** zależy od parametru „Tylko pracownicy zaznaczeni na
liście":

- **domyślnie (odznaczone)** – raport przegląda **elementy wypłat o definicji
  „zapomoga"** (`WypElementy.WgDefinicja`, filtr serwerowy po `WypElement.Data`)
  w oknie `[1.1.(R-4), 31.12.(R-1)]` i bierze **każdego pracownika, który
  dostał zapomogę** – także **zwolnionych** i **spoza bieżącego filtra
  listy / okresu**. Zaznaczenie jest ignorowane. To odpowiedź na scenariusz:
  „lista ma okres wrzesień, a na raporcie mają być też osoby zwolnione
  w styczniu, które dostały zapomogę".
- **zaznaczone** – klasycznie: tylko pracownicy zaznaczeni na liście
  (wiersz historii `PracHistoria` mapowany na pracownika).

W obu trybach pracownik bez zapomogi w oknach nie trafia na wydruk.

## Parametry wydruku

| Parametr | Pole | Domyślnie | Rola |
|---|---|---|---|
| Data posiedzenia komisji | `PrnParams.DataPosiedzenia` (`Date`) | dziś | Wyznacza rok odniesienia `R` (= rok tej daty) oraz służy jako data graniczna wniosku ZFŚS |
| Tylko pracownicy zaznaczeni na liście | `PrnParams.TylkoZaznaczeni` (`bool`) | `false` | `false` = skan wszystkich wypłat; `true` = tylko zaznaczeni |

Okna zapomóg to **całe lata kalendarzowe**, rok `R` (rok daty posiedzenia)
jest **pominięty**:

- **„z 2 lat”**: lata `R-2` i `R-1` → `[1.1.(R-2), 31.12.(R-1)]`
- **„z poprzednich 2 lat”**: lata `R-4` i `R-3` → `[1.1.(R-4), 31.12.(R-3)]`

Przykład: `R = 2026` → „z 2 lat” = 2024 + 2025; „z poprzednich 2 lat” =
2022 + 2023.

## Układ: master-detail, jeden wiersz = jedna zapomoga

**Jeden wiersz wydruku = jedna wypłacona zapomoga.** Pracownik z kilkoma
zapomogami zajmuje kilka kolejnych wierszy. Kolumny „poziomu pracownika”
(Lp, Nr ewid., Nazwisko, Data urodzenia, Jednostka obsługująca, Dochód na
członka rodziny) oraz obie sumy wypełniane są **tylko w pierwszym wierszu
grupy** pracownika; w kolejnych wierszach te komórki są puste. Dwie listy
zapomóg (okno bieżące / poprzednie) są wyrównane wg indeksu wiersza –
dłuższa wyznacza liczbę wierszy grupy, krótsza ma puste komórki w
nadmiarowych wierszach. **Pracownik bez żadnej zapomogi w obu oknach jest
pomijany** – nie trafia na wydruk i nie zużywa numeru `Lp` (numeracja
pozostaje ciągła dla osób z zapomogami).

Przykład ze wzoru papierowego (nr ewid. 82652, 2 wiersze detail):
`Kwota zapomogi z 2 lat` = 3 000,00 = suma kolumny `Kwota` z obu wierszy
(1 000 + 2 000); `Kwota zapomóg z poprzednich 2 lat` = 2 000,00 = suma
kolumny `Kwota` (poprzednia) z obu wierszy (1 000 + 1 000).

## Kolumny

| Nagłówek | Pole źródła | Zawartość |
|---|---|---|
| Lp. | `Lp` | Liczba porządkowa – tylko w 1. wierszu grupy pracownika |
| Nr ewid. | `NrEwid` | `Pracownik.Kod` – tylko 1. wiersz grupy |
| Nazwisko i Imię | `NazwiskoImie` | `Pracownik.NazwiskoImię` – tylko 1. wiersz grupy |
| Data urodzenia | `DataUrodzenia` | `Pracownik.Historia[Date.Today].Urodzony.Data` – tylko 1. wiersz grupy |
| Jednostka obsługująca | `JednostkaObslugujaca` | Cecha „Jednostka obsługująca” z Wydziału bieżącego etatu pracownika. Cecha typu „element słownika” → `(ElemSlownika) Wydzial.Features["Jednostka obsługująca"]`, wyświetlane `.Nazwa` (fallback `"brak"`). Tylko 1. wiersz grupy |
| Dochód na członka rodziny | `DochodNaCzlonkaRodziny` | Z aktualnego, zatwierdzonego wniosku ZFŚS pracownika (krotka `A1_ZFSS`, dodatek `AltOne.Skanska.Workflow`). Odwzorowuje worker `WniosekZFFSPracownikaWorker.GetProgDochodu`: wniosek przez `PracownikExt.GetAktualnyWniosekZFSS(pracownik, data posiedzenia, rok posiedzenia)`; dla `RodzajWskazywaniaDochodu == "Kwota"` → kwota dochodu na członka rodziny, w przeciwnym razie nazwa progu dochodowego (element słownika). Brak roli widoczności kwot u operatora → `(Brak praw do widoku)`. Puste = brak zatwierdzonego wniosku. Tylko 1. wiersz grupy |
| Kwota zapomogi z 2 lat | `KwotaZapomogiZ2Lat` | Suma `WypElement.Wartosc` zapomóg z okna bieżącego (= suma kolumny `Kwota` wszystkich wierszy grupy). Puste = 0. Tylko 1. wiersz grupy |
| Data / Kwota | `DataWyplaty` / `KwotaWyplaty` | i-ta zapomoga z okna bieżącego: `Wyplata.Data` / `WypElement.Wartosc` (`N2`). Puste, gdy w tym wierszu nie ma już zapomogi z tego okna |
| Kwota zapomóg z poprzednich 2 lat | `KwotaZapomogPoprzednich2Lat` | Suma zapomóg z okna poprzedniego (= suma kolumny `Kwota` poprzednia wszystkich wierszy grupy). Puste = 0. Tylko 1. wiersz grupy |
| Data / Kwota | `DataWyplatyPoprzedniej` / `KwotaWyplatyPoprzedniej` | i-ta zapomoga z okna poprzedniego. Puste, gdy w tym wierszu nie ma już zapomogi z tego okna |

Zapomoga = element wypłaty (`WypElement`), którego nazwa (lub nazwa
`Definicja`) zawiera „zapomog"/„zapomóg", niewystornowany
(`RozliczenieStorna == false`), z `WypElement.Data` w oknie. Pozycje w oknie
sortowane rosnąco po dacie.

**Wydajność:** raport nie iteruje wszystkich wypłat. Dobór pracowników
(tryb domyślny) idzie przez `WypElementy.WgDefinicja[def]` dla definicji
z „zapomog" w nazwie; wiersze pojedynczego pracownika – przez
`WypElementy.WgPracownik[pracownik]`; oba z warunkiem serwerowym po
`WypElement.Data`.

## Zależność: dodatek AltOne.Skanska.Workflow

Kolumna „Dochód na członka rodziny” korzysta z klas dodatku
`AltOne.Skanska.Workflow` (środowisko Skanska): `PracownikExt` i `A1ZfssTuple`
z przestrzeni `AltOne.Skanska.Workflow.Extensions` /
`AltOne.Skanska.Workflow.Procesy.WniosekZFSS.Tuples`. Snippet **nie skompiluje
się** w bazie, w której ten dodatek nie jest wczytany. Logika (metoda
`PobierzDochodNaCzlonkaRodziny`) jest 1:1 odwzorowaniem property
`WniosekZFFSPracownikaWorker.GetProgDochodu` – zweryfikowana przez dekompilację
DLL w wersjach `2512.7.8` i `2604.4.4`, **niesprawdzona na żywej aplikacji**
(repo nie ma dostępu do środowiska Skanska). Przed użyciem produkcyjnym:
uruchomić wydruk na bazie Skanska dla kilku pracowników z i bez wniosku ZFŚS.

Rok oświadczenia = rok z „Daty posiedzenia komisji”; data graniczna wniosku
(„nie później niż”) = data posiedzenia.

## TODO / do weryfikacji na żywej bazie

- **Cały snippet niesprawdzony na środowisku Skanska** – kompilacja
  zweryfikowana lokalnie (enova 2604.4.4 + `AltOne.Skanska.Workflow` +
  DevExpress), ale bez uruchomienia. Sprawdzić:
  - liczba wierszy grupy = `max(zapomogi w oknie bieżącym, w poprzednim)`,
    puste kolumny poziomu pracownika w wierszach 2..N;
  - sumy zgadzają się z sumą widocznych pozycji (jak we wzorze papierowym);
  - zmiana roku w „Dacie posiedzenia” przesuwa oba okna o rok kalendarzowy.
- **Definicja „zapomogi”** – dopasowanie po nazwie definicji elementu
  (`DefinicjaElementu.Nazwa` zawiera `"zapomog"`) oraz po nazwie samego
  elementu (`WypElement.Nazwa`). Potwierdzić, że wszystkie definicje zapomóg
  w bazie Skanska mają „zapomog” w nazwie. Uwaga: dobór pracowników (tryb
  domyślny) idzie **tylko po definicjach** – element z własną nazwą
  „zapomoga”, ale definicją bez tego słowa, nie doda pracownika do raportu
  (choć jego wiersze i tak by się nie pojawiły bez zapomogi wykrytej przez
  definicję – spójne).
- **`WypElement.Data`** – zakładamy, że to data wypłaty (jak `Wyplata.Data`).
  Potwierdzić na danych.
- **Cecha „Jednostka obsługująca”** na Wydziale jest typu **element
  słownika** (`Soneta.Ksiega.ElemSlownika`). Odczyt: nietypowany indeksator
  `Wydzial.Features["Jednostka obsługująca"]` → rzut na `ElemSlownika` →
  `.Nazwa`; brak wartości → `"brak"`. Nazwa cechy w konfiguracji to dokładnie
  „Jednostka obsługująca” (ze spacją). Wymaga `using Soneta.Ksiega;`.

## Błąd „DataComponentBase” przy wywołaniu z Pulpitu

**Ważne doprecyzowanie (2026-09-07):** ten raport (`ZestawienieKomisjaSocjalna`)
sam w sobie **nie jest wywoływany** z Pulpitów w tej instalacji. Poniższy błąd
pojawia się przy próbie wywołania **innego** raportu z **Pulpitu WWW**
(pulpit działa jako aplikacja przeglądarkowa, nie jako moduł w kliencie
desktopowym). Mimo to treść błędu wskazuje na plik/snippet
`A1ZestawienieKomisjaSocjalna` — czyli na **ten sam kod** (prawdopodobnie ten
inny raport w Enova przejął/skopiował ten sam „systemowy plik dodatkowy”
snippetu, zamiast dostać własny). To istotna poszlaka: błąd nie jest
specyficzny dla konkretnego `.repx` tego raportu ani dla ścieżki wywołania
„Lista pracowników → ten raport” — pojawia się przy kompilacji **tego kodu**
w procesie **Pulpitu WWW**, niezależnie od tego, z poziomu którego raportu
Enova akurat tę kompilację wyzwoliła. Wzmacnia to hipotezę „host Pulpitu WWW
nie ma dostępu do `DevExpress.DataAccess.v24.1.dll`”, a osłabia potrzebę
grzebania dalej w treści snippetu — problem wygląda na czysto
środowiskowy/wdrożeniowy po stronie serwera WWW obsługującego Pulpity.

Treść błędu:

```
Systemowy plik dodatkowy: Snippet: A1ZestawienieKomisjaSocjalna
A1ZestawienieKomisjaSocjalna.cs(155,21): error CS0012: Typ „DataComponentBase”
jest zdefiniowany w nieprzywoływanym zestawie. Musisz dodać odwołanie do
zestawu „DevExpress.DataAccess.v24.1, Version=24.1.5.0, Culture=neutral,
PublicKeyToken=b88d1754d700e49a”.
```

**Hipoteza przyczyny:** `DataComponentBase` (assembly `DevExpress.DataAccess.v24.1`)
to bazowa klasa `Soneta.Business.UI.DxReports.BusinessDataSource` — komponentów
`BusinessSource`/`BusinessSourceContext` osadzonych w `ComponentStorage` tego
`.repx` (patrz `Raporty/SzablonTabela6Kolumn.md`, sekcja „Historia usterki: brak
źródła danych w `.repx`” — identyczne komponenty). Przy kompilacji snippetu
Enova musi więc rozwiązać typ `BusinessDataSource` łącznie z jego klasą
bazową. W wersji okienkowej ta biblioteka jest już załadowana w procesie
klienta (m.in. przez sam projektant wydruków), więc kompilator dynamiczny ją
znajduje. Host obsługujący **Pulpity** to prawdopodobnie osobny proces/AppDomain
(np. serwis WWW dla pulpitów), w którym `DevExpress.DataAccess.v24.1.dll` nie
została jeszcze załadowana / nie jest automatycznie referencjonowana — stąd
`CS0012` tylko w tej ścieżce wywołania.

**Próba nr 1 (obalona):** dodano `using DevExpress.DataAccess;` na początku
pliku, w nadziei że jawny `using` wymusi dołączenie referencji przy
kompilacji. **Przetestowane na żywo — ten sam błąd nadal występuje.** To
wyklucza hipotezę „silnik dobiera referencje z `using`-ów w kodzie źródłowym
snippetu” — najwyraźniej lista zestawów dostępnych kompilatorowi w procesie
Pulpitów jest ustalana niezależnie od treści snippetu (np. z góry
skonfigurowana lista referencji dla tego hosta, albo zestaw assembly faktycznie
załadowanych w tym AppDomain). Skoro sama treść kodu tego nie zmienia, wniosek
jest taki, że **problem nie leży w snippecie, tylko w środowisku/hoście
obsługującym Pulpity** — brakuje mu dostępu do
`DevExpress.DataAccess.v24.1.dll` (lub kompilator w tym procesie ma z góry
ograniczoną listę referencjonowanych zestawów, do której ta biblioteka nie
należy).

**Próba nr 2 — ominięcie w kodzie źródłowym snippetu (nieprzetestowana):**
CS0012 pojawia się dokładnie tam, gdzie kod snippetu odwołuje się do
właściwości `CustomDataSource` na wyniku `DxReportHelpers.GetDataSourceEmpty(this)`
(zadeklarowanym jako `BusinessDataSource`) — takie odwołanie do właściwości
wymaga od kompilatora rozwiązania **pełnej hierarchii klas** `BusinessDataSource`
(w tym `DataComponentBase`), bo odczyt/zapis właściwości wymaga przeszukania
także składowych odziedziczonych. Sam fakt wywołania metody zwracającej ten
typ i przypisania wyniku do zmiennej `object` (bez dotykania jego składowych)
**nie powinien** wymagać tej samej pełnej hierarchii — rzutowanie w górę do
`object` jest zawsze legalne bez pełnego rozwiązania klasy bazowej.

Na tej podstawie zmieniono snippet: nowa prywatna metoda `UstawDaneRaportu(object dane)`
przechowuje wynik `GetDataSourceEmpty(this)` w zmiennej typu `object` i
ustawia `CustomDataSource` **przez refleksję** (`Type.GetProperty` +
`PropertyInfo.SetValue`), więc w kodzie źródłowym nigdzie nie ma już
statycznego odwołania do typu `BusinessDataSource` ani jego składowych.
Wszystkie 4 miejsca w `BeforePrint`, które wcześniej ustawiały
`CustomDataSource` bezpośrednio, przechodzą teraz przez tę metodę.

**To nadal hipoteza, nieprzetestowana na żywo** — dwa możliwe wyniki po
wgraniu:
- **Zadziała** (błąd zniknie, także przy wywołaniu tamtego innego raportu) —
  potwierdzi, że problem był ściśle w treści snippetu (odwołanie do
  `CustomDataSource`), a nie w samej obecności `.repx` z komponentami
  `BusinessDataSource` w `ComponentStorage`.
- **Nie zadziała / ten sam błąd** — oznaczać będzie, że CS0012 pochodzi z
  kodu generowanego automatycznie przez Enova na podstawie `ComponentStorage`
  tego `.repx` (pola dla komponentów `BusinessSource`/`BusinessSourceContext`
  łączonego przy kompilacji z treścią snippetu), a nie z treści snippetu
  wklejanej w „Kod źródłowy” — wtedy jedyną naprawą zostaje strona
  serwera/hosta (dodanie `DevExpress.DataAccess.v24.1.dll` do Pulpitu WWW),
  bo nic w treści snippetu tego już nie ominie.

Do ustalenia (wymaga dostępu do serwera/instalacji — poza zasięgiem tego
repo):
- sprawdzić, czy w folderze `bin` procesu obsługującego Pulpity (może to być
  osobny serwis/aplikacja WWW, nie ten sam katalog co klient desktopowy)
  znajduje się `DevExpress.DataAccess.v24.1.dll` obok innych bibliotek
  `DevExpress.XtraReports.v24.1` / `DevExpress.XtraPrinting.v24.1`;
- jeśli brakuje — skopiować ją tam z instalacji klienta (ta sama wersja
  `24.1.5.0`) i zrestartować usługę/proces Pulpitów;
- sprawdzić, czy **jakikolwiek** wydruk oparty o `.repx` z komponentem
  `BusinessDataSource` (wzorzec `Soneta.Business.UI.DxReports`) w ogóle działa
  wywołany z tej instalacji Pulpitów — jeśli żaden nie działa, problem jest
  po stronie hosta Pulpitów, a nie tego konkretnego raportu;
- w projektancie wydruków sprawdzić, czy raport, przy którego wywołaniu z
  Pulpitu WWW faktycznie wystąpił błąd, rzeczywiście **celowo** ma podpięty
  „systemowy plik dodatkowy” `A1ZestawienieKomisjaSocjalna` — jeśli to
  przypadkowe powielenie/pozostałość po kopiowaniu wydruku, warto to
  rozdzielić niezależnie od naprawy samego hosta.

**Kolejne doprecyzowanie (użytkownik, 2026-09-07):** ten inny raport
działał z Pulpitu WWW **bez problemu, dopóki nie wgrano snippetu
`ZestawienieKomisjaSocjalna`**. To zmienia obraz sprawy — sam akt wgrania
naszego snippetu (nie jego wywołanie!) jest tym, co zaczęło psuć inny,
wcześniej działający raport. To silnie sugeruje, że w tej instalacji Enova
kod źródłowy wydruków (wszystkie snippety/„systemowe pliki dodatkowe”) jest
w kliencie Pulpitu WWW **kompilowany razem, jako jedna wspólna jednostka
kompilacji** (typowy mechanizm Soneta dla „kodu w bazie” / rozszerzeń
modułu) — a nie osobno, per raport. Dopóki żaden snippet w tej wspólnej
paczce nie wymagał `DevExpress.DataAccess.v24.1`, paczka kompilowała się
poprawnie w tym hoście (mimo braku tej biblioteki — po prostu nie była
potrzebna). Nasz `.repx` ma w `ComponentStorage` komponenty
`BusinessDataSource` (dziedziczące po `DataComponentBase`) — ich dodanie do
wspólnej paczki wymusza rozwiązanie tego typu przy kompilacji **całej
paczki**, a skoro kompilacja jest prawdopodobnie „wszystko albo nic”, błąd w
naszym raporcie **wywala kompilację całej paczki**, więc przestają działać
też inne, niepowiązane raporty które akurat są w tej samej paczce — łącznie
z tym „innym raportem”, którego kod nigdy nie dotykał `DataComponentBase`.

**Przyczyna potwierdzona (użytkownik, 2026-09-07):** bez snippetu
`ZestawienieKomisjaSocjalna` w bazie tamten inny raport z Pulpitu WWW działa
bez problemu. To potwierdza teorię „wspólna paczka kompilacji” — nasz
`.repx` psuje kompilację całej paczki w hoście Pulpitu WWW, który nie ma
`DevExpress.DataAccess.v24.1.dll`. Użytkownik pamięta, że podobny problem z
innym raportem w Pulpitach już kiedyś wystąpił i został jakoś rozwiązany,
ale rozwiązanie nie zostało odnotowane w tym repo (przeszukane commity i
`.md` — brak śladu) — najpewniej rozwiązano to bezpośrednio po stronie
serwera (np. dograniem brakującej biblioteki), bez zapisu tutaj.

**Wniosek praktyczny:** dopóki `DevExpress.DataAccess.v24.1.dll` nie
znajdzie się w hoście Pulpitu WWW, obecność tego snippetu/raportu w bazie
**psuje inne, działające dotąd raporty wywoływane z tego Pulpitu** — to nie
jest tylko „ten raport nie działa”, tylko realna regresja funkcji, które
wcześniej działały.

**ROZWIĄZANE (użytkownik, 2026-09-07):** po wgraniu wersji snippetu z
`UstawDaneRaportu` (refleksja zamiast statycznego `.CustomDataSource` na
`BusinessDataSource`) błąd zniknął — potwierdzone na żywo, tamten inny
raport znów działa z Pulpitu WWW. To ostatecznie potwierdza „Próbę nr 2”
opisaną wyżej: `CS0012` pochodził z odwołania do właściwości
`CustomDataSource` w treści **naszego** snippetu (a nie z automatycznie
generowanego kodu wiążącego `ComponentStorage` tego `.repx`) — ominięcie go
w kodzie źródłowym wystarczyło, **bez** żadnej zmiany po stronie serwera/hosta
Pulpitu WWW i bez dodawania `DevExpress.DataAccess.v24.1.dll`. Ten sam wzorzec
(`UstawDaneRaportu` przez refleksję zamiast bezpośredniego
`DxReportHelpers.GetDataSourceEmpty(this).CustomDataSource = ...`) warto mieć
na uwadze przy innych raportach z tym samym `BusinessDataSource` w `.repx`
(np. `Raporty/SzablonTabela6Kolumn`), gdyby też miały być kiedyś wywoływane z
Pulpitu WWW w tej instalacji.

## Jak podpiąć snippet w Enova

1. Otwórz `ZestawienieKomisjaSocjalna` w projektancie wydruków Enova.
2. W „Kod źródłowy” wklej całą zawartość `Raporty/ZestawienieKomisjaSocjalnaSnippet`.
3. Zapisz – Enova skompiluje kod i podepnie klasę
   `ZestawienieKomisjaSocjalnaSnippet` pod wydruk.
4. Uruchom wydruk z Listy pracowników; w oknie parametrów podaj „Datę
   posiedzenia komisji” (domyślnie dziś). „Tylko pracownicy zaznaczeni na
   liście” zostaw odznaczone, aby raport sam znalazł wszystkich z zapomogą
   (także zwolnionych).

Zmiana nie wymaga modyfikacji `.repx` – pasmo `Detail` jest płaskie, a
„wygaszanie” powtórzonych kolumn i wyrównanie dwóch list zapomóg realizuje
snippet (puste stringi w wierszach 2..N grupy).

`.repx` zawiera już w `ComponentStorage` komponenty `BusinessContext`,
`BusinessSource` (`DataKind="Empty"`) i `BusinessSourceContext`
(`DataKind="Context"`) oraz atrybut `DataSource="#Ref-49"` na korzeniu –
analogicznie do `Raporty/SzablonTabela6Kolumn` (patrz tam „Historia usterki:
brak źródła danych w `.repx`”). Komórki wiersza danych są już powiązane z
polami `[Lp]`, `[NrEwid]`, `[NazwiskoImie]`, `[DataUrodzenia]`,
`[JednostkaObslugujaca]` itd. – layout nie wymaga zmian.

## Odporność na błędy

- Poszczególne komórki poziomu pracownika liczone są przez `Bezpiecznie(...)`
  – błąd w jednej daje `[BŁĄD: ...]` w tej komórce zamiast wywalenia wydruku.
- Grupa **każdego pracownika** budowana jest w osobnym `try/catch` – błąd
  (np. w pobraniu wypłat / wniosku ZFŚS) daje jeden wiersz `[BŁĄD: ...]` dla
  tego pracownika, reszta zestawienia się drukuje.
- Cała `BeforePrint` jest w `try/catch` – błąd poza pętlą daje jeden wiersz
  „BŁĄD” z etapem i treścią wyjątku.

Jeśli zobaczysz `[BŁĄD: ...]`, wklej treść – pozwoli poprawić logikę lub dane.
