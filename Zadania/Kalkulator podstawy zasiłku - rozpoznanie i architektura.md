# Kalkulator podstawy wymiaru zasiłku — etap 1: rozpoznanie i architektura

**Status:** raport z etapu 1, czeka na akceptację. Kodu dodatku jeszcze nie ma.
**Źródło ustaleń:** dekompilacja (ILSpy 8.2) bibliotek **enova365 2512.5.6**, czyli tej samej wersji
co serwer klienta. Paczki `Soneta.KadryPlace`, `Soneta.Business`, `Soneta.Types` i `Soneta.Core`
w wersji 2512.5.6 są publicznie dostępne na nuget.org. Odwołania typu `PodstawaZasiłku.cs:1521`
wskazują pliki zdekompilowanego `Soneta.KadryPlace.dll` (przestrzeń `Soneta.Place`, chyba że
podano inaczej). Ustalenia nie były sprawdzane na żywej bazie.

---

## 1. Kalkulator wynagrodzenia — jak jest zbudowany

| Element | Ustalenie |
|---|---|
| Rejestracja | `[assembly: Worker(typeof(KalkulatorWynagrodzenMultiWorker), typeof(Pracownik / PracHistoria / Pracownicy / Umowa / ListaPlac / Dodatek …))]` |
| Worker | `KalkulatorWynagrodzenWorker.GetKalkulator(Login)` tworzy **nową sesję `login.CreateSession(readOnly: true, config: false)`** i zwraca `new KalkulatorWynagrodzenParams(Context.Empty.Clone(s))` |
| Parametry + wynik | `KalkulatorWynagrodzenParams : ContextBase`. Jeden obiekt trzyma parametry wejściowe i pola wyniku (`NalBrutto`, `NalNetto`, `Miesiące` …), a formularz pokazuje oba. Metoda `Przelicz()` jest podpięta pod przycisk. |
| Symulacja | `Przelicz()` tworzy **osobną sesję `CreateSession(readOnly: false, config: true)`** i otwiera `using (session.Logout(editMode: true))`. Wewnątrz buduje **fikcyjnego pracownika** (`new PracownikFirmy()`), etat, umowę, a nawet zmienia konfigurację składek. Potem nalicza przez `NaliczanieWypłat.NaliczPracownikowiInt()` i przepisuje wynik do pól parametrów. |
| Wycofanie | Nie ma `Commit()` ani `Save()`. `Dispose()` transakcji i sesji (`finally`) wycofuje wszystko. |
| Uprawnienia | Całość działa w `session.ExecuteSpecialAccessInternal(AccessRights.Granted, …)`. Ta metoda jest **`internal`** i dodatek nie może jej użyć. |

**Wzorzec do powtórzenia:** osobna sesja edycyjna, transakcja `Logout(true)` bez `Commit`
i `Dispose` dają gwarantowane wycofanie. Parametry i wynik żyją w jednej klasie `ContextBase`.
Różnica: my nie budujemy fikcyjnego pracownika. Liczymy na **rzeczywistym** pracowniku
pobranym do tej sesji przez `session.Get(pracownik)`.

## 2. Natywne liczenie podstawy zasiłku

**Klasa `Soneta.Place.PodstawaZasiłku` jest publiczna i ma publiczne konstruktory.** Używa jej
sam algorytm elementów chorobowych. Szablon algorytmu „Jak chorobowe” (`JakChorobowe.cs:159`):

```csharp
PodstawaZasiłku podstawa = new PodstawaZasiłku(Element);
Składnik.Podstawa1 = podstawa.Podstawa;
Składnik.Procent   = podstawa.Procent;
```

Najważniejsze dla kalkulatora jest to, że **enova sama liczy podstawę „na niby”, bez zapisu**.
Robi to `NaliczanieZUS2022.PodstawaWychowawczego2012` (`NaliczanieZUS2022.cs:597-611`):

```csharp
using (pracownik.Session.Logout(editMode: true))          // brak Commit => rollback
{
    var n = new NieobecnośćPracownika(pracownik);
    KalendModule.GetInstance(pracownik).Nieobecnosci.AddRow(n);
    n.Definicja = …DefNieobecnosci[ZwolnienieChorobowe];
    var el = new WypElementNieobecność(pracownik, n);
    el.Definicja = …DefElementow[DefinicjaElementu.ZasMacierzyński];
    el.Okres = new FromTo(from, from);
    PlaceModule.GetInstance(pracownik).WypElementy.AddRow(el);
    decimal podstawa = new PodstawaZasiłku(el, new FromTo(from, from), miesiąc.LastDay, onlyLog: false) {…}.Podstawa;
}
```

**Wniosek:** natywny mechanizm da się wywołać z zewnątrz, dla dowolnej daty i bez zapisu, tym
samym wzorcem, którego używa enova. Kalkulator nie będzie więc implementował reguł od nowa.

Publiczne API `PodstawaZasiłku`, które wykorzystamy:

| Składowa | Znaczenie |
|---|---|
| `decimal Podstawa` | Pełny algorytm: przejęcie podstawy z poprzedniej nieobecności → średnia z miesięcy → składniki okresowe → limit dobrowolnego chorobowego → przyrównanie do minimalnej |
| `Percent Procent` | Procent zasiłku z elementu/definicji/konfiguracji |
| `Queue<PoliczMiesiąc> Miesiące` / `MiesiąceOkresowe` | Miesiące okresu podstawy. Każdy ma `Miesiąc`, `Norma` (CzasDni), `RNA` (przepracowane), `Suma` (po składkach), `Info` (powód wyłączenia) oraz `ElementyPodstawy` |
| `ElementPodstawyZasilku` | `Element` (WypElement), `Sposób` (`BezDopełniania` / `Nominalna` / `Dopełnione`), `KwotaDopełniana` (brutto przed uzupełnieniem), `KwotaDopełniona` (po uzupełnieniu i składkach), `Mnożnik`, `MnożnikOpis`, `Odchyłki`, `Opis` |
| `NaliczanieOd` / `NaliczanieDo` | Zakres miesięcy faktycznie wziętych |
| `string Info` | Pełny log naliczenia, ten sam co „Pokaż log naliczania podstawy” (`LogDoPodstawWorker`) |
| `WypSkladnikGłówny SkładnikPodstawyZNieobecności` | Składnik poprzedniej nieobecności, z której przejęto podstawę |
| `bool PomińPodstawęZPoprzedniejNieobecności` | **Gotowy przełącznik trybu diagnostycznego** („wymuś ponowne ustalenie”) |
| `bool PomińPrzyrównanieDoMinimalnegoWynagrodzenia` | Pozwala pokazać podstawę przed i po przyrównaniu do minimalnej |
| `Func<bool, WypElement, IPodstawaZasilkow> PodstawyZasiłkówFunc` | **Publiczny hook** pytany o ustawienie „podstawa zasiłków” każdego elementu. Wykorzystamy go do opcji „tylko zatwierdzone listy” i do zbierania elementów pominiętych z powodem. |

Konstruktor docelowy:
`new PodstawaZasiłku(WypElementNieobecność el, FromTo nieobecnośćOkres, Date dataWypłaty, bool onlyLog)`.
Ten konstruktor czyta też `Nieobecność.Zwolnienie.PonownieUstalPodstawe` i `.Przyczyna`.

Kwota zasiłku (stawka dzienna, kwota dzienna, zaokrąglenia) **nie** jest liczona w
`PodstawaZasiłku`, tylko w metodzie `_Wylicz` algorytmu elementu (zależy od
`Config.Chorobowy.ZmianaWyliczeniaStawkiDziennejOd`, `ZaokrąglenieZasiłkuDzień/Precyzja`).
Publiczne `NaliczanieWypłat.Nalicz()` + `DodajŹródło(IBazaZrodlaWyplaty)` pozwalają opcjonalnie
zrobić pełne symulowane naliczenie i odczytać kwotę 1:1 z enovą (patrz pytanie 4).

## 3. Konfiguracja definicji elementu wynagrodzenia

| Wymaganie | Gdzie w enova (`DefinicjaElementu`) | Uwagi |
|---|---|---|
| Wchodzi do podstawy chorobowego / opiekuńczego / macierzyńskiego / rehabilitacyjnego | `Nieobecnosci.ZasilkiPracownicy` (etat) i `Nieobecnosci.ZasilkiInni` (zleceniobiorcy), typ `PodstawaZasilkow`: `Chorobowych`, `Opiekunczych`, `Macierzynskich`, `Rehabilitacyjnych` (bool) | Wybór etat/umowa: `PoliczDefinicjePodstawyZasiłków(umowa, element)` |
| Czy i jak uzupełniany (art. 37/38) | to samo pole `.Typ` (`TypPodstawyZasilku`): `NieWliczać`, `WliczaćDla` (bez uzupełniania), `DopełniaćWedługGodzinDla`, `DopełniaćWedługDniDla`, `DopełniaćWedług130Dla`, `DopełniaćJakZasadniczeDla`, `WliczaćWNominalnejWysokościDla` (kwota z umowy) | Jedno ustawienie łączy „czy wchodzi” i „metodę uzupełnienia” |
| Pomniejszany za czas nieobecności | `Algorytm.…Korekta` (`KorektaDefElementu`): `Chorobowe`, `Opiekuncze`, `Macierzynskie`, `Rehabilitacyjne`, `Usprawiedliwione`, `Platne`, `Nieusprawiedliwione` (`RodzajPomniejszenia`) + odchyłki | `PodstawaZasiłku` czyta to pośrednio (mnożnik uzupełnienia, odchyłki) |
| Okres, za jaki jest wypłacany | `OkresNaliczania` (`Typ`, `Ilosc`, `IlośćMiesięcy`, `Opoznienie`, `Naliczanie`) | `IlośćMiesięcy > 1` oznacza składnik okresowy (1/3, 1/12 …). Algorytm może to zmienić przez `ParametryOkresuArgs` |
| Wypłacany za okres zasiłku (art. 42) | **Nie ma jednej flagi.** Działają: (a) `Typ = NieWliczać`; (b) `Nieobecnosci.WliczanieDoPodstawyChorobowego` (`Domyślnie` / `Zawsze` / `DoKońcaOkresuWypłaty` / `DoDatyZakończeniaWypłaty`) razem z `Config.Chorobowy.KontrolaKońcaDodatków` i `KontrolaWgPolaDataZakończonia`, które wyłączają dodatki zakończone przed nieobecnością | `Algorytm.ZaNieobecnosc.Chorobowego/Opiekunczego/…` istnieje, ale **`PodstawaZasiłku` go nie czyta** — patrz rozbieżność R3 |
| Inne | `Nieobecnosci.DoMinimalnej` — element dodawany przy przyrównaniu do minimalnej. `Deklaracje.PodstawaSkladek.UrlopWychowawczy`. Algorytm elementu może mieć własną metodę `PodstawaZasiłku(element, PodstawaZasiłkuArgs)`, która nadpisuje sposób wliczenia | |

Element musi też być oskładkowany (`Podatki.OrgTyUb` niepusty) i pasować tytułem
(pracownik/zleceniobiorca). W przeciwnym razie jest pomijany.

## 4. Dane źródłowe

| Dane | Źródło natywne |
|---|---|
| Elementy wypłat | `pracownik.Elementy` = `WypElementy.WgPracownik`, filtrowane **wg `WypElement.Okres` (za okres)**, nie wg daty wypłaty. Składki pracownika: z `Podatki.KosztyZUS / Chorobowa.Podstawa` faktycznie naliczonych w danym miesiącu (`wyliczSkładkiZus`). To automatycznie obsługuje **przekroczenie rocznej podstawy em.-rent.** |
| Status listy | `Wyplata.Zatwierdzona` / `Wyplata.Bufor`, `ListaPlac.Zatwierdzona` / `Bufor`. **Natywny algorytm nie filtruje po statusie** i bierze także elementy z list w buforze. |
| Czas pracy | `pracownik.Czasy.Norma(…)`, `Czasy.KalkPracy.PracaRozliczana/Odchylki/NormaNie(…)`. Wariant wg `Config.Chorobowy.AlternatywnaPodstawaOdchyłek`; od `MiesiąceWPodstawieZasiłkuWgDni` połowa miesiąca liczona jest **w dniach**, wcześniej w godzinach |
| Nieobecności | `pracownik.Czasy.Nieobecnosci(okres)`, `ZbiegiPracyIRodzicielstwa` |
| Etat / umowy | `pracownik[data]` (`PracHistoria`: `Etat.Wymiar`, `Etat.Ubezpieczenia.Chorobowe.UbezpieczenieOd`, `RodzajZatrudnienia`), `pracownik.Umowy` |
| Poprzednie zasiłki | Nieobecności ZUS + ich `WypElement`/składnik główny (`podstawaZNieobecności`) |
| Ręczne podstawy | `PodstawyNieobec[pracownik, TypyPodstawNieobecnosci.Chorobowa, miesiąc]` („Podstawa nieobecności” wpisana ręcznie, np. od poprzedniego pracodawcy lub z migracji) oraz `PracHistoria.ChorobowyBO` (bilans otwarcia podstawy) — **mają pierwszeństwo przed listami płac** |
| Liczba miesięcy okresu | `historia.Nieobecnosci.PoliczPodstawęZasiłku(...)`, czyli ustawienie kadrowe pracownika albo `Config.Chorobowy.PodstawaPracownicy` / `PodstawaPozostali`. Nie jest to stała 12. |

## 5. Parametry konfiguracyjne wpływające na podstawę

- `Config.Chorobowy`: `PodstawaPracownicy`, `PodstawaPozostali`, `PodstawaOkresowe`,
  `AlternatywnaPodstawaOkresowych` (wybór algorytmu składników okresowych), `DopełniajElementyOkresowe`,
  `IndywidAlgorytmMigrowanychOkresowych`, `KontrolaKońcaDodatków`, `KontrolaWgPolaDataZakończonia`,
  `ZaOkresWypStałeNaDni`, `NieobecnośćUsprawiedliwionaPłatna`, `AlternatywnaPodstawaOdchyłek`,
  `PodzielonePodstawyZasiłkówZUS` (osobne podstawy dla chorobowego, macierzyńskiego, opiekuńczego
  i rehabilitacyjnego), `MiesiąceWPodstawieZasiłkuWgDni`, `OgraniczPodstZasiłkuDoKorygowanego`,
  `WaloryzacjaPodstawyPo`, stawki `ProcentPłaciFirma`, `ProcentPłaciZUS`, `ProcentWypadek`,
  `ProcentCiąży`, `ProcentOpiekuńczego`, `ProcentSzpital…`, `PłaciFirma`, `PłaciFirma50Lat`,
  `ZmianaWyliczeniaStawkiDziennejOd`, `ZaokrąglenieZasiłku…`.
- `Config.SkładkiZUS`: `EmerytalnaUbezpieczonego`, `RentowaUbezpieczonego`, `Chorobowa` (historyczne,
  wg daty), `MaksymalnaRocznaPodstawa`, `OgraniczenieDobrChorobowego`.
- `Config.Wynagrodzenia`: `Najniższe` (minimalne), `PrognozowanePrzeciętneMiesięczne`.
- `Config.WaloryzacjaPodstawy`.
- Etat: `Etat.Wymiar`, `Etat.ProcentMinimalnegoNaDzień(data)`, `Etat.RodzajZatrudnienia`
  (młodociani i uczniowie nie są przyrównywani do minimalnej).

Wszystkie wartości kalkulator odczyta pośrednio, bo liczy je natywny algorytm. Żadna nie będzie
wpisana w kod.

---

## Rozbieżności do rozstrzygnięcia

Zgodnie z poleceniem zgłaszam je bez rozstrzygania. Numery artykułów podaję z pamięci: ISAP
był niedostępny z tego środowiska, więc trzeba je potwierdzić w aktualnym tekście jednolitym.

| # | Specyfikacja | Ustawa (stan wg mojej wiedzy) | enova 2512.5.6 |
|---|---|---|---|
| R1 | Przerwa < **3** mies. (art. 36 ust. 3) | Od 1.01.2022 przerwa krótsza niż **1 miesiąc kalendarzowy** ([gazetaprawna.pl](https://www.gazetaprawna.pl/praca/emerytury-i-renty/artykuly/10917282,ustalenie-wymiaru-zasilku-chorobowego-przerwa-miedzy-chorobami.html), [poradnikprzedsiebiorcy.pl](https://poradnikprzedsiebiorcy.pl/-wplyw-przerwy-w-zatrudnieniu-na-obliczenie-wymiaru-zasilku-chorobowego)) | `poprzednieNieobecności()`: **1 miesiąc** dla elementów od 2022-01-01, 3 miesiące wcześniej; okno podwojone przy obniżeniu wymiaru (COVID). Scenariusz testowy „< 3 mies.” trzeba zmienić na „< 1 mies.”. |
| R2 | Wyłączenie miesiąca < połowy = art. 37; brak pełnego miesiąca = art. 38 ust. 2 | Uzupełnianie = art. 37; wyłączenie miesiąca przepracowanego < połowy = **art. 38 ust. 1**; brak pełnego miesiąca = art. 38 ust. 2 (do weryfikacji w ISAP) | Wyłączenie: `time2 + time2 >= time` (≥ połowa normy) |
| R3 | Art. 42 „wg ustawień definicji” | — | Brak dedykowanej flagi; działa przez `TypPodstawyZasilku = NieWliczać` i kontrolę końca dodatku. Flagi `Algorytm.ZaNieobecnosc.*` nie wpływają na podstawę. Kalkulator pokaże je tylko informacyjnie. |
| R4 | Okres podstawy = 12 mies. | 12 mies. | Liczba miesięcy z konfiguracji. Pętla **zatrzymuje się na pierwszym odrzuconym miesiącu** (sprzed ubezpieczenia lub zmiany wymiaru), a także na miesiącu **bez żadnego wliczanego elementu** („Miesiąc nie wpływa na podstawę zasiłku”) i nie sięga dalej wstecz. |
| R5 | Tylko naliczone listy płac | — | Pierwszeństwo mają ręczne `PodstawyNieobec` i `ChorobowyBO`. Kalkulator pokaże, że miesiąc pochodzi z „wprowadzonej podstawy nieobecności”. |
| R6 | Opcja „tylko zatwierdzone listy” | — | enova bierze **także bufor**. Opcja „tylko zatwierdzone” celowo da wynik **inny niż enova**. Proponuję domyślnie „wszystkie (jak enova)”. |
| R7 | Podstawa minimalna proporcjonalnie do etatu | art. 45 | `Najniższe × Wymiar × ProcentMinimalnegoNaDzień − składki pracownika` + elementy `DoMinimalnej`. Pomijani: zleceniobiorcy, młodociani i uczniowie, tymczasowi przy przejętej podstawie. |
| R8 | — | — | Dla zleceniobiorców z dobrowolnym chorobowym enova dodatkowo ogranicza podstawę: `PrognozowanePrzeciętneMiesięczne × OgraniczenieDobrChorobowego`. |

## Proponowana architektura

```
Addon/KalkulatorZasilku/                      (osobny projekt, obok A1PelnaListaPlacAddon)
├─ A1KalkulatorZasilku.csproj                 net8.0; PackageReference Soneta.KadryPlace 2512.5.6
│                                              (nuget.org, ExcludeAssets=runtime) — zamiast HintPath
├─ KalkulatorPodstawyZasilkuWorker.cs         [assembly: Worker(.., typeof(Pracownik))]  menu Czynności
│                                              (+ wariant na Pracownicy dla wielu osób)
├─ KalkulatorPodstawyZasilkuParams.cs         ContextBase: Data, RodzajŚwiadczenia, WymuśPonowneUstalenie,
│                                              TylkoZatwierdzone; wynik (DTO) + akcja „Przelicz”
├─ SymulatorPodstawyZasilku.cs                rdzeń (bez UI): sesja rollback → PodstawaZasiłku → DTO
├─ Wynik/*.cs                                 POCO: WynikPodstawy, MiesiącPodstawy, ElementPodstawy,
│                                              RegułaSpecjalna, PorównanieZEnova
└─ KalkulatorPodstawyZasilkuParams.*.pageform.xml   zakładki: Parametry | Podsumowanie | Miesiące |
                                               Elementy | Log naliczenia (soneta-form-xml)
```

**Przebieg `SymulatorPodstawyZasilku.Licz(...)`:**
1. `using var s = login.CreateSession(readOnly: false, config: false, "KalkulatorZasilku")`.
   Nigdy `Save()`.
2. `using (s.Logout(editMode: true))`, **bez `Commit`**:
   - `p = s.Get(pracownik)`;
   - `NieobecnośćPracownika` z `Definicja` wg rodzaju świadczenia (systemowe GUID-y enovy, np.
     `DefinicjaNieobecnosci.ZwolnienieChorobowe`, `ZwolnienieOpiekaZUS`, `UrlopRehabilitacyjny`,
     macierzyński), `Okres = [data, data]`;
   - `WypElementNieobecność` z `Definicja` wg rodzaju (`DefinicjaElementu.WynagrChorobowe`,
     `ZasChorobowy`, `ZasOpiekuńczy`, `ZasMacierzyński`, …). Dla wynagrodzenia chorobowego (art. 92 KP)
     algorytm podstawy jest ten sam, inny jest tylko element i procent;
   - `var pz = new PodstawaZasiłku(el, okres, data, onlyLog:false) { PomińPodstawęZPoprzedniejNieobecności = wymuś }`;
     `PodstawyZasiłkówFunc` opakowana tak, że rejestruje decyzje dla elementów i opcjonalnie
     odrzuca listy niezatwierdzone;
   - odczyt `Podstawa`, `Procent`, `Miesiące`, `MiesiąceOkresowe`, `Info`,
     `SkładnikPodstawyZNieobecności`. Drugie wywołanie z `PomińPrzyrównanieDoMinimalnegoWynagrodzenia`
     pokaże podstawę przed przyrównaniem;
   - **kopiowanie wszystkiego do POCO**, zanim sesja zostanie zamknięta.
3. Dispose wycofuje dodane wiersze. Wynik nie trzyma referencji do `Row` z zamkniętej sesji.
4. Porównanie: w sesji formularza (tylko odczyt) szukam istniejącego `WypElementNieobecność`
   pracownika z nieobecnością obejmującą datę i biorę `SkładnikGłówny.Podstawa1`
   (to samo pole, które wypełnia szablon „Jak chorobowe”). Pokazuję różnicę.

**Stawki:** `stawka dzienna = podstawa / 30` i kwoty dla procentów z `Procent` oraz z
konfiguracji `Config.Chorobowy.Procent…`. Dokładna kwota zgodna z naliczeniem enovy (zaokrąglenia)
jest możliwa tylko przez pełną symulację `NaliczanieWypłat` — patrz pytanie 4.

**Bezpieczeństwo (safe-code):** sesja jednowątkowa, utworzona i zamknięta w obrębie jednej
akcji. Brak `Save`/`Commit`. Brak `ExecuteSpecialAccessInternal` (to API wewnętrzne).
Serwerowe zdarzenia i TaskTriggery uruchamiają się tylko na `Save`, więc się nie wykonają.
Ryzyko do sprawdzenia na bazie testowej: weryfikatory lub zdarzenia sesyjne odpalane przy
`AddRow` nieobecności (np. kontrola limitów) mogą rzucić wyjątek. Przechwycę go i pokażę jako
komunikat.

**Testy (etap 3):** projekt testów na `TestBase` (rollback dwupoziomowy), po jednym scenariuszu
na przypadek. Każdy test: (a) wynik kalkulatora, (b) faktyczne naliczenie nieobecności na liście
płac w transakcji wycofywanej, (c) asercja różnicy 0,00 zł. **Testów nie da się uruchomić w tym
środowisku**: nie ma serwera ani bazy enova. Uruchomienie u Ciebie albo przez `dbmgr` na bazie
testowej.

## Pytania przed implementacją

1. **„W bazie Claude”** — czy chodzi o bazę testową enova o nazwie *Claude*, na której mają iść
   testy z etapu 3? Jak je uruchamiamy (Twój serwer 2512.5.6, `TestBase` lub `dbmgr`)?
2. **R1:** przyjmuję stan prawny i enovy (przerwa **< 1 miesiąca** od 2022, wcześniej < 3)?
3. **R6:** domyślnie „wszystkie listy, jak enova”, a „tylko zatwierdzone” jako opcja z ostrzeżeniem
   o możliwej różnicy?
4. **Kwota zasiłku:** wystarczy podstawa + stawka dzienna `/30` × procent (wynik „informacyjny”)
   czy robimy pełne symulowane naliczenie przez `NaliczanieWypłat` (cięższe, ale kwota 1:1 z enovą)?
5. **Uprawnienia:** symulacja dodaje (i wycofuje) nieobecność i element wypłaty, więc użytkownik
   kalkulatora musi mieć prawo do ich edycji, nawet jeśli nic nie zostanie zapisane. Akceptowalne?
6. **Projekt:** referencje przez `PackageReference Soneta.KadryPlace 2512.5.6` z nuget.org
   (sprawdziłem, że paczka istnieje; projekt da się wtedy zbudować także tutaj) zamiast HintPath
   do `C:\enovaServer`. `Soneta.Sdk 1.2.0` celuje w nowsze wersje (net10), dlatego dla serwera
   2512 / net8 proponuję samą paczkę. OK?
7. **Branch:** CLAUDE.md wskazuje `Testy`, ale ten branch nie istnieje na zdalnym repo. Na razie
   pracuję na `claude/gifted-goldberg-3z9lm2`.
