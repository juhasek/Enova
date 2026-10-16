# Kalkulator podstawy wymiaru zasiłku — Etap 1: rozpoznanie i architektura

Stan: 2026-10-16. Źródło: dekompilacja `Soneta.KadryPlace.dll` serwera **2610**
(`C:\enovaServer\2610\Soneta.Products.Server.Standard`), namespace `Soneta.Place`.
Nazwy klas/metod przepisane z plików dekompilacji (UTF-8), nie z konsoli.

## Wniosek główny

enova ma **publiczny** natywny silnik podstawy zasiłku (`Soneta.Place.PodstawaZasiłku`), który
zwraca nie tylko kwotę, ale całe rozbicie (miesiące, normy, czas przepracowany, elementy, sposób
wliczenia, kwoty przed i po uzupełnieniu, składniki okresowe). Co więcej, **sama enova** wywołuje go
na „fantomowej” nieobecności, której nigdy nie zapisuje (`NaliczanieZUS2022.PodstawaWychowawczego2012`).
Kalkulator **nie implementuje reguł ustawowych od nowa**. Odtwarza ten wzorzec w sesji, której nie
zapisujemy. Wynik jest więc z definicji zgodny z naliczeniem na liście płac.

## 1. Kalkulator wynagrodzeń — jak jest zbudowany

| Element | Klasa / mechanizm |
|---|---|
| Akcja | `KalkulatorWynagrodzenMultiWorker.Kalkulator(Login)` z `[Action("Kalkulator wynagrodzeń", Menu\|ToolbarWithText)]`. Zarejestrowany `[assembly: Worker]` na `Pracownik`, `PracHistoria`, `Pracownicy`, `Umowa`, `ListaPlac`, `Dodatek`… |
| Parametry/wynik | `KalkulatorWynagrodzenParams : ContextBase`: pola wejściowe z `OnChanged()`, wyniki jako właściwości `Nal*`, lista `Miesiące`, metoda `Przelicz()` (przycisk na formularzu). |
| Sesja formularza | `login.CreateSession(readOnly: true, config: false)` → `Context.Empty.Clone(s)`. |
| Symulacja | W `Przelicz()` **osobna** sesja `Login.CreateSession(readOnly:false, config:true)` → `using (session.Logout(editMode:true))` → `ExecuteSpecialAccessInternal(...)`: tworzy fikcyjnego pracownika, zmienia konfigurację, uruchamia `NaliczanieWypłat` i czyta `NaliczaneElementy`. **Brak `Save()`**, sesja jest `Dispose()` w `finally`, więc zmiany giną. |

**Wzorzec do powtórzenia:** formularz parametrów na sesji tylko do odczytu, a obliczenie w osobnej,
zapisywalnej sesji w trybie edycji, bez `Save`, z `Dispose` w `finally`.
Różnica: my nie tworzymy fikcyjnego pracownika i nie zmieniamy konfiguracji. Do **realnego**
pracownika dodajemy w pamięci tylko jedną nieobecność.
Uwaga: `ExecuteSpecialAccessInternal` jest `internal`, więc dodatek go nie użyje. Działamy
na prawach operatora, co i tak jest pożądane, bo widzi tylko to, do czego ma prawa.

## 2. Natywne liczenie podstawy zasiłku

| Klasa | Rola (publiczne API) |
|---|---|
| `PodstawaZasiłku` | Silnik. Publiczne konstruktory: `(WypElementNieobecność)`, `(WypElementNieobecność, bool onlyLog)`, `(WypElementNieobecność, FromTo nieobecnośćOkres, Date dataWypłaty, bool onlyLog[, bool waloryzacje])`, `(WypElementZbiegPracyIRodzicielstwa)`. |
| `.Podstawa` | Kwota (getter liczy wszystko: okres ubezpieczenia/ciągłość, przejęcie z poprzedniej nieobecności, `sumujMiesiące`, okresowe, waloryzacja, minimalne). |
| `.Miesiące` / `.MiesiąceOkresowe` | `Queue<PoliczMiesiąc>`: `Miesiąc`, `Norma` i `RNA` (`CzasDni`: czas+dni; RNA = przepracowane), `Suma`, `ElementyPodstawy`, `Info`. |
| `ElementPodstawyZasilku` | `Element` (WypElement), `Miesiąc`, `Sposób` (BezDopełniania/Nominalna/Dopełnione), `KwotaDopełniana`, `KwotaDopełniona`, `Mnożnik`, `MnożnikOpis`, `Odchyłki`. |
| `.NaliczanieOd/.NaliczanieDo`, `.SkładnikPodstawyZNieobecności`, `.Info`, `.Procent` | Okres podstawy, **przejęcie podstawy z poprzedniego zasiłku** (składnik źródłowy), log podsumowania, procent. |
| Flagi publiczne | `PomińPodstawęZPoprzedniejNieobecności` (= tryb „wymuś ponowne ustalenie”), `PomińPrzyrównanieDoMinimalnegoWynagrodzenia`, `DataKonfiguracji`, `PrzeliczajZPrzyczyną` (np. liczenie podstawy opiekuńczego z tej samej nieobecności). |
| `LogDoPodstawWorker` | Natywna akcja „Pokaż log naliczania podstawy” na elemencie wypłaty (`onlyLog:true`). Szczegółowy log tabelaryczny idzie do `Trace` (kategoria „Chorobowe”), a nie do `.Info`. |
| Stawka dzienna | `AlgorytmyPłacowe.WyliczPodstawęZasiłkuZaDzień` (protected): przed `Config.Chorobowy.ZmianaWyliczeniaStawkiDziennejOd` = `Podstawa1*Procent*Ilosc/30`, od tej daty = `Round(Podstawa1/30, 2)*Procent*Ilosc`; zaokrąglenie wg `ZaokrąglenieZasiłkuDzień/Precyzja`. |

**Czy da się wywołać z zewnątrz dla zadanej daty bez zapisu: tak.** Precedens w samej enova
(`NaliczanieZUS2022`, wyliczenie podstawy wychowawczego):
```
using (pracownik.Session.Logout(editMode: true)) {
  var n = new NieobecnośćPracownika(pracownik); KalendModule.Nieobecnosci.AddRow(n); n.Definicja = ...;
  var e = new WypElementNieobecność(pracownik, n); e.Definicja = ...; e.Okres = ...; PlaceModule.WypElementy.AddRow(e);
  decimal p = new PodstawaZasiłku(e, okres, data, onlyLog:false) { ... }.Podstawa;
}   // bez Commit -> wycofane
```
Wszystkie użyte tu typy i konstruktory są publiczne.

Silnik czyta też **algorytm definicji**: jeżeli definicja elementu wchodzącego do podstawy ma w kodzie
metodę `PodstawaZasiłku(WypElement, PodstawaZasiłkuArgs)`, silnik ją wywołuje. Własne reguły klienta
są więc uwzględnione automatycznie, a w kalkulatorze od zera byłyby nie do odtworzenia.

## 3. Konfiguracja definicji elementu (`DefinicjaElementu`)

| Wymaganie | Gdzie w enova |
|---|---|
| Wchodzi do podstawy chorobowego/opiekuńczego/macierzyńskiego/rehab. | `Definicja.Nieobecnosci.ZasilkiPracownicy` (etat) / `.ZasilkiInni` (umowy): `PodstawaZasilkow` → `Typ` + `Chorobowych/Opiekunczych/Macierzynskich/Rehabilitacyjnych` |
| Uzupełnianie i metoda | `Typ` (`TypPodstawyZasilku`): `NieWliczać`, `WliczaćDla` (bez uzupełnienia), `DopełniaćWedługGodzinDla`, `DopełniaćWedługDniDla`, `DopełniaćWedług130Dla`, `DopełniaćJakZasadniczeDla`, `WliczaćWNominalnejWysokościDla` (kwota z umowy) |
| Pomniejszany za nieobecność | Kreator: `Algorytm.KreatorAlgorytmu.Korekta` (`Chorobowe`, `Opiekuncze`, `Macierzynskie`, `Rehabilitacyjne`… : `RodzajPomniejszenia`). Algorytm „potrącenie podstawowego”: `Algorytm.ZaNieobecnosc` (`Chorobowego`, `Opiekunczego`…). Edytor C#: zaszyte w kodzie, nieczytelne z konfiguracji. |
| Okres, za jaki wypłacany | `Definicja.OkresNaliczania` (`Typ`: `TypOkresuNaliczania`, `Ilosc`, `IlośćMiesięcy`); silnik: `PodstawaZasiłku.IlośćMiesięcy(element)` |
| Wypłacany za okres zasiłku (ustawa art. 41) | `Nieobecnosci.WliczanieDoPodstawyChorobowego` (`SposobyWliczaniaDoPodstawyChorobowego`: Domyślnie/Zawsze/DoKońcaOkresuWypłaty/DoDatyZakończeniaWypłaty) + konfiguracja `Chorobowy.KontrolaKońcaDodatków`, `KontrolaWgPolaDataZakończonia`. Składniki zachowywane za czas choroby wyłącza się w praktyce przez `Typ = NieWliczać`. |
| Do podstawy minimalnej | `Nieobecnosci.DoMinimalnej` |
| Podstawa urlopowa (nie dotyczy) | `Nieobecnosci.Urlop`, `.Ekwiwalent` |

## 4. Dane źródłowe

- **Elementy wypłat** są pobierane wg **okresu, za który przysługują** (`WypElement.Okres`), a nie daty wypłaty:
  `new SubTable(pracownik.Elementy, miesiącFT)`. Pomijane są storna i elementy nieoskładkowane chorobowo/wypadkowo.
- **Zatwierdzone a bufor:** silnik **nie filtruje** po `Wyplata.Bufor`, czyli bierze wszystkie naliczone elementy
  (filtr `filtrElementów` jest `internal`). Zob. pytanie 3.
- **Czas pracy:** `PoliczMiesiąc.Norma` (z kalendarza historii etatu, `NormaCD`) i `RNA` (przepracowane w okresie
  ubezpieczenia ∩ zatrudnienia, z odjęciem postojów/zbiegów).
- **Nieobecności i ciągłość:** `pracownik.Czasy.Nieobecność(...)`, `Nieobecnosc.Zwolnienie.PonownieUstalPodstawe`
  (ręczna flaga ponownego ustalenia na zwolnieniu), `Pracownik.DataPoczątkuZasiłku/DataKońcaZasiłku`.
- **Historia etatu i umowy:** `pracownik[data]` (`PracHistoria.Etat.Wymiar`, `Ubezpieczenia.Chorobowe.UbezpieczenieOd`), `pracownik.Umowy`.
- **Poprzednie zasiłki (przerwa < 3 mies.):** `poprzednieNieobecności()` szuka wcześniejszego naliczonego
  `WypElementNieobecność` i przejmuje jego podstawę (+ tabela `PodstawyNieobec` przy migracji).
- **Istniejąca podstawa w bazie (porównanie):** `WypElementNieobecność` pracownika → składnik główny `Podstawa1`.

## 5. Konfiguracja firmy/płac (czytana, nie wpisywana)

`PlaceModule.Config.Chorobowy`: `PodstawaPracownicy` (12), `PodstawaOkresowe`, `PodstawaPozostali`,
`MaksPrzerwaWUbezpieczeniu`, `ZaOkresWypStałeNaDni`, `AlternatywnaPodstawaOkresowych`, `AlternatywnaPodstawaOdchyłek`,
`DopełniajElementyOkresowe`, `NieobecnośćUsprawiedliwionaPłatna`, `PodzielonePodstawyZasiłkówZUS`,
`MiesiąceWPodstawieZasiłkuWgDni`, `ZmianaWyliczeniaStawkiDziennejOd`, `ZaokrąglenieZasiłkuDzień/Precyzja`,
procenty (`ProcentPłaciFirma`, `ProcentPłaciZUS`, `ProcentCiąży`, `ProcentWypadek`, `ProcentSzpital*`, `ProcentOpiekuńczego`),
`OgraniczPodstZasiłkuDoKorygowanego`, `WaloryzacjaPodstawyPo`. `Config.Wynagrodzenia.Najniższe` (minimalne),
`Config.SkładkiZUS.*` + `PodatkiUtil.ProcentSkładki(...)` (składki pracownika, w tym limit 30-krotności przez
`wyliczSkładkiZus`). `JakChorobowe.OkresPodstawy` w definicji (std. = konfiguracja). Silnik czyta to sam,
a kalkulator tylko pokazuje te wartości w zakładce „Konfiguracja”.

## Kluczowa decyzja architektoniczna

**Delegacja do natywnego `PodstawaZasiłku`, bez reimplementacji reguł.** Własny kod ogranicza się do:
1. zbudowania sytuacji „co by było, gdyby od dnia D pracownik był chory” w sesji niezapisywanej,
2. przepisania struktur silnika na formularz (DTO) i zamknięcia sesji,
3. stawki dziennej i kwot 80/100/70% (wzór z `WyliczPodstawęZasiłkuZaDzień` + procenty z wyniku naliczenia),
4. porównania z podstawą już naliczoną w bazie.

### Przebieg obliczenia (sesja symulacji)
```
Session s = login.CreateSession(readOnly:false, config:false)
try { using (s.Logout(editMode:true)) {          // NIE Commit, NIE Save
    pracownik = s.Get(pracownikZFormularza)
    nieob = new NieobecnośćPracownika(pracownik); AddRow; Definicja = <def. nieobecności wg rodzaju>; Okres = D..D+n
    if (WymuśPonowne) nieob.Zwolnienie.PonownieUstalPodstawe = true / pz.PomińPodstawęZPoprzedniejNieobecności = true
    Wariant A (domyślny, pełna zgodność): NaliczanieWypłat { Pracownik, Okres = miesiąc D, DataWypłaty/DataListy, MiesiącZUS }
       .DodajŹródło(nieob).Nalicz() → WypElementNieobecność z naliczonym składnikiem (Podstawa1, Procent, Dni, Wartość)
       → new PodstawaZasiłku(element, onlyLog:false) dla rozbicia
    Wariant B (gdy A się nie uda): element ręcznie jak w NaliczanieZUS2022 → PodstawaZasiłku(...)
    → kopiuj wynik do DTO (POCO, bez referencji do wierszy sesji)
}} finally { s.Dispose() }
```
Wariant A daje dokładnie to, co pojawiłoby się na liście płac (także podział wynagrodzenie chorobowe/zasiłek,
33/14 dni, procenty, ewentualny własny algorytm definicji). Ta sama ścieżka jest **testem z etapu 3**
(„naliczenie w transakcji wycofywanej”), więc kalkulator i test porównawczy nie rozjadą się.

### Komponenty dodatku (`Addon/KalkulatorPodstawyZasilku/`, Soneta.Sdk jak `Addon/RezerwyUrlopowe`)
| Plik | Zawartość |
|---|---|
| `KalkulatorPodstawyZasilkuWorker.cs` | `[assembly: Worker]` na `Pracownik` (+ `Pracownicy` dla zaznaczonych), akcja „Kalkulator podstawy zasiłku” (Menu\|ToolbarWithText), zwraca parametry na sesji read-only |
| `KalkulatorPodstawyZasilkuParams.cs` | `ContextBase`: Pracownik(-cy), Data niezdolności, Rodzaj świadczenia (enum → `PrzyczynaNieobecnosci` + wybór definicji nieobecności z bazy), Wymuś ponowne ustalenie, [Tylko zatwierdzone: zob. pytanie 3], `Przelicz()` |
| `SymulacjaPodstawy.cs` | Sesja symulacji (powyżej), bez stanu statycznego (thread-safe) |
| `WynikPodstawy.cs` | DTO: podstawa, stawka dzienna, kwoty dzienne, okres od–do, miesiące (norma/RNA/½/kwota przed/po), elementy (definicja, kwota, okres, sposób, mnożnik, po składkach), reguły specjalne, różnica do bazy, log |
| `KalkulatorPodstawyZasilkuParams.pageform.xml` | Zakładki: Parametry · Wynik · Miesiące · Elementy · Reguły i log · Porównanie |
| Brak `business.xml` | Bez własnych tabel (zgodnie z założeniem) |

Formularz: oznaczenie miesiąca wyłączonego = `RNA < Norma/2` z danych silnika plus brak elementów w kolejce.
Powód wyłączenia pochodzi z `Info` silnika, nie z własnej logiki.

## Rozbieżności: Twój opis, ustawa i enova (do decyzji, niczego nie rozstrzygam sam)

1. **Numeracja artykułów:** składniki zachowywane za okres zasiłku to **art. 41** (nie 42), a składniki za okresy
   dłuższe niż miesiąc to **art. 42** (nie 41). Brak pełnego miesiąca to wg mojej wiedzy **art. 37 ust. 1**
   (opis: art. 38 ust. 2), a wyłączenie miesięcy < ½ czasu i uzupełnianie to **art. 38** (opis: art. 37).
   Art. 37/38 do potwierdzenia w ISAP. Art. 40 (zmiana wymiaru) zgodny.
2. **Kwartalne:** wg art. 42 ust. 2 to **1/12 kwot wypłaconych za 4 kwartały** poprzedzające miesiąc niezdolności,
   a nie „1/3 z kwartału”. Roczne to 1/12 kwoty za rok poprzedzający. enova liczy okresowe w `SumujOkresowe2`
   (okno `Chorobowy.PodstawaOkresowe`, podział wg `IlośćMiesięcy`). Kolumnę „1/3 kwartalny” proponuję zastąpić
   „okresowy (1/12 z N mies.)” z mnożnikiem z silnika.
3. **Przerwa < 3 mies., wyjątki:** enova nie wykrywa sama zmiany składników w przerwie. Robi to ręczna flaga
   `Zwolnienie.PonownieUstalPodstawe` na nieobecności. Automatycznie obsługuje zmianę wymiaru etatu (`ZmienićWymiar`).
4. **„Tylko zatwierdzone listy”:** silnik bierze elementy także z wypłat w buforze. Opcja „tylko zatwierdzone” dałaby
   wynik **inny** niż enova.

## Pytania przed implementacją

1. **Baza i wersja docelowa:** Al (2610) czy Claude? Dodatek budujemy pod 2610 (`Soneta.*` 2610 z nuget.org)?
2. **Dane testowe:** w bazie Claude są tylko 2 elementy „Wynagr. chorobowe” i 6 wypłat. Bez ~12 miesięcy naliczonych
   list scenariuszy z etapu 3 nie da się porównać. Przygotować dane (pracownicy testowi + naliczenie list 13 mies.)
   w Claude czy jest baza z historią płac? (Zapytanie SQL do Al zwróciło teraz brak pamięci serwera SQL.)
3. **Opcja „tylko zatwierdzone”:** pominąć ją (zgodność z enova), czy zrobić jako tryb diagnostyczny z wyraźnym
   ostrzeżeniem „wynik inny niż enova”? Wymagałoby to w sesji symulacji usunięcia w pamięci wypłat z bufora.
4. **Definicja nieobecności:** rodzaj świadczenia mapuję na definicję z bazy po `PrzyczynaNieobecnosci`
   (np. pierwsza aktywna „Zwolnienie chorobowe” z kodem zwolnienia A). Wolisz, żeby użytkownik wybierał definicję
   nieobecności i kod zwolnienia (ciąża/szpital/wypadek zmieniają procent)?
5. **Wariant A (pełne naliczenie w pamięci):** akceptujesz? Daje 100% zgodności i od razu procent/kwotę, ale trwa
   dłużej (naliczenie miesiąca pracownika). Wariant B jest szybszy, ale procent trzeba by brać z konfiguracji.

## Ryzyka do sprawdzenia w testach
- Brak skutków ubocznych naliczenia w niezapisanej sesji (numeratory, logi): kontrola SQL liczności tabel przed i po.
- `WypElement.Data` (data wypłaty) w wariancie B jest pusta, co ma wpływ na `CzyObniżenie` (obniżenie warunków).
  To argument za wariantem A.
- Własny algorytm (edytor C#) definicji „chorobowe”: jeżeli nie używa `new PodstawaZasiłku(Element)`, to tylko
  wariant A zwróci tę samą wartość co lista płac.
