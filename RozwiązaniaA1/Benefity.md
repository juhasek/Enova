# Moduł Benefity (BenefityA1) — dokumentacja biznesowa

Analiza dodatku `AltOne.Skanska.Ext.dll` (przestrzenie nazw `AltOne.Skanska.Ext.*`), część
dotycząca benefitów pozapłacowych. Ustalenia poniżej pochodzą z odczytu bajtkodu IL kluczowych
metod (nie tylko z nazw klas/pól), więc opisują faktyczny mechanizm zapisu, a nie tylko strukturę.

## 1. Model danych — jak benefit jest reprezentowany w enova

Benefit **nie jest** osobnym obiektem biznesowym — jest to **Dodatek** (element listy płac,
`Soneta.Kadry.Dodatek` / historia `DodHistoria`) przypisany do pracownika. Dodatek dostaje jedynie
dwie dodatkowe **cechy** (Features), które łączą go z warstwą "benefitową":

| Cecha | Co przechowuje |
|---|---|
| **`BenefityZestawDodatkow`** | Wiersz `ZestawDodatków` (pakiet/zestaw benefitowy), z którego dodatek pochodzi — to jest właśnie pole „Definicja benefitu" widoczne na zakładce historii dodatku |
| **`BenefityOsobaDodatkowa`** | Wiersz `CzlonekRodziny` (osoba towarzysząca/senior), jeśli dodatek dotyczy nie samego pracownika, ale osoby z niego korzystającej w ramach pakietu; `null` dla dodatku samego pracownika |

Pakiet (**Zestaw**, `Soneta.Kadry.ZestawDodatków`) to słownikowa definicja — zestaw kilku
**elementów** (`ElementZestawuDodatków`, kolekcja `ZestawDodatków.ElementyZestawu`), z których
każdy wskazuje na konkretną **definicję dodatku** (`ElementZestawuDodatków.Definicja` — element
algorytmu naliczeń, tabela `DefElementow`). Innymi słowy: jeden pakiet typu np. "Karta Multisport
Silver" może w praktyce oznaczać kilka osobnych składników na liście płac pracownika (np. wartość
świadczenia, dopłata pracownika, podatek od świadczenia) — worker tworzy je wszystkie naraz, po
kolei, dla każdego elementu zestawu.

Same pakiety są skategoryzowane cechą **`Pakiet`** (tekst), przyjmującą jedną z trzech wartości —
dokładnie zgodnie z enumem `RodzajZestawu`:

- `KartaSportowa`
- `Opieka`
- `Ubezpieczenie`

## 2. Przyznawanie benefitu pracownikowi — workery `DodajZestaw*A1Worker`

Trzy workery, każdy wywoływany z listy pracowników (`Soneta.Kadry.Pracownicy`, akcja masowa na
zaznaczeniu):

- **`DodajZestawKartaSportowaA1Worker`**
- **`DodajZestawOpiekaA1Worker`**
- **`DodajZestawUbezpieczenieA1Worker`** (dodatkowo obsługuje **`DodatkowyPakiet1`**/**`DodatkowyPakiet2`**
  — rozszerzenia/warianty polisy dokładane obok podstawowego ubezpieczenia)

Wspólna logika (`DodajZestaw`, zweryfikowana w IL identycznie dla wszystkich trzech):

1. Otwiera transakcję (`Session.Logout(editMode: true)`).
2. Pobiera `KadryModule`.
3. Dla **każdego elementu wybranego Zestawu** (`Zestaw.ElementyZestawu`) i **każdego zaznaczonego
   pracownika** (`Rows`) — czyli iloczyn kartezjański elementy × pracownicy:
   - loguje do konsoli progresu `"{Definicja}/{Pracownik}"`,
   - wywołuje `DodajDodatek(kadryModule, pracownik, DodajKolejny, Zestaw, null)` (ostatni argument
     `null` = brak osoby towarzyszącej, dodatek jest dla samego pracownika).
4. Commituje transakcję na końcu (jedna transakcja na całą operację masową).

## 3. Rdzeń zapisu — `DodajDodatekBaseA1Worker<T>.DodajDodatek`

To jest właściwy silnik tworzący/aktualizujący wpis na liście płac. Sygnatura (odtworzona z IL):
`DodajDodatek(KadryModule km, Pracownik pracownik, bool dodajKolejny, object zestaw, object osoba)`.

Logika (dokładnie wg odczytanego bajtkodu):

1. **Jeżeli `dodajKolejny == false`** (czyli checkbox „Dodaj kolejny" niezaznaczony — to jest
   wariant domyślny): worker szuka wśród istniejących dodatków pracownika (`GetDodatki(pracownik)`)
   takiego, który na dzień `Okres.From` ma ten sam `Element` (tę samą definicję algorytmu), co
   żądany. Jeśli trafi — sprawdza dodatkowo, że:
   - jego aktualna historia jest **otwarta** (`Aktualnosc.To == Date.MaxValue`), oraz
   - **`Historia.Okres.To < Okres.From`** żądanego okresu (czyli poprzedni okres historii kończy
     się przed nowym) —

     tylko wtedy **ponownie wykorzystuje ten sam obiekt Dodatku**, dokładając mu nowy wpis
     historii (`Historia.Update(Okres.From)`), zamiast tworzyć nowy Dodatek od zera. Efekt: kolejne
     przyznanie tego samego typu benefitu temu samemu pracownikowi **wydłuża/aktualizuje** istniejący
     wpis, zamiast go duplikować.
2. **Jeżeli `dodajKolejny == true`** (checkbox zaznaczony): warunek reużycia jest pomijany w
   całości — worker **zawsze** tworzy nowy, osobny obiekt Dodatku przez `CreateDodatek(pracownik)`
   (`new Dodatek(pracownik)`) i dodaje go do `KadryModule.Dodatki`. Czyli „Dodaj kolejny" oznacza
   dosłownie: nie łącz z istniejącym, zawsze zakładaj nowy, równoległy dodatek (np. gdy pracownik ma
   dostać dwie niezależne karty sportowe naraz).
3. Na docelowym wierszu historii (nowym lub doklejonym do istniejącego dodatku):
   - `Element` = definicja algorytmu z elementu zestawu,
   - parametry naliczenia (Podstawa/Czas/Dni/Procent/Ułamek/Współczynnik) — przez
     `ParametryDodatkuParams.Update(historia)`,
   - `Okres` = żądany zakres dat,
   - cecha **`BenefityZestawDodatkow`** = przekazany `zestaw` (wiąże wpis z definicją pakietu),
   - cecha **`BenefityOsobaDodatkowa`** = przekazany `osoba` (wiąże wpis z osobą towarzyszącą, jeśli
     dotyczy).

`DodajDodatekA1Worker` (bazowa, ogólna implementacja `GetDodatki`/`CreateDodatek` użyta przez
powyższe workery) po prostu odpytuje kolekcję `Pracownik.Dodatki` i tworzy `new Dodatek(pracownik)`
— czyli benefit trafia na standardową listę dodatków płacowych pracownika, tak jak każdy inny
składnik.

## 4. Dopisywanie osoby towarzyszącej do pakietu — `DodajOsobeDoPakietuA1Worker`

Osobny worker, uruchamiany na już istniejącym dodatku-pakiecie pracownika, żeby dołożyć do niego
osobę z jego rodziny (np. partnera życiowego albo seniora objętego opieką/ubezpieczeniem). Parametry
(`DodajOsobeDoPakietuA1Params`): `RodzajZestawu` (Opieka/Ubezpieczenie/KartaSportowa), `Zestaw`
(bazowy pakiet pracownika, filtrowany przez cechę `Pakiet` jak w §1), `Osoba` (wybierana z
`Pracownik.Rodzina` — czyli **`Soneta.Kadry.CzlonekRodziny`**), `RodzajOsoby` (enum `RodzajOsobyA1`:
`OsTowarzysząca` / `Senior`), opcjonalnie `DodatkowyPakiet1`/`DodatkowyPakiet2`.

Walidacje wykonywane przed zapisem (dokładnie wg IL, w tej kolejności):

1. **`Osoba` musi być wskazana** — inaczej wyjątek: *„Wymagane jest wprowadzenie osoby."*
2. Bazowy `Zestaw` pracownika musi mieć ustawioną cechę **`ZestawOsoby`** wskazującą **inny**
   `ZestawDodatków` — czyli osobny, „lustrzany" pakiet przeznaczony dla osób towarzyszących (np.
   pakiet pracownika "Ubezpieczenie – pracownik" ma cechę `ZestawOsoby` wskazującą na
   "Ubezpieczenie – osoba towarzysząca"). Brak tej cechy → wyjątek: *„Brak przypisanego zestawu
   osoby. Cecha 'ZestawOsoby' na zestawach dodatków."*
3. Cecha **`WeryfikacjaOsoby`** na tym `Zestaw` jest wywoływana z parametrem `Osoba`
   (`FeatureCollection.GetString(..., new[] { osoba })`) — to jest **cecha wyliczana zwracająca
   komunikat błędu** (pusty string = OK). Jeśli zwróci niepusty tekst, worker rzuca go wprost jako
   treść wyjątku. To jest miejsce, w którym enova sprawdza reguły biznesowe kwalifikowalności osoby
   do benefitu (np. wiek seniora, typ pokrewieństwa dla osoby towarzyszącej) — konkretna treść
   reguły jest zdefiniowana w cesze na danych, nie w kodzie dodatku.

Po przejściu walidacji: dla `ZestawOsoby` (i analogicznie dla `DodatkowyPakiet1`/`DodatkowyPakiet2`,
jeśli podane) worker woła `DodajZestawDodatkow(zestaw, log, kadryModule, transakcja, workerBazowy,
elementyZestawu, Osoba)`.

### `DodajZestawDodatkow` — filtrowanie elementów wg rodzaju osoby

Elementy zestawu „dla osób" nie są jednolite — jeden pakiet `ZestawOsoby` może zawierać elementy
przeznaczone zarówno dla osób towarzyszących, jak i dla seniorów. Rozróżnienie odbywa się **per
element**: każdy `ElementZestawuDodatków` ma cechę **`RodzajOsoby`** (tekst), porównywaną (jako
string) z wybranym w parametrach `RodzajOsobyA1` (`OsTowarzysząca`/`Senior`). **Tylko elementy, dla
których cecha się zgadza, są faktycznie dodawane** — reszta jest pomijana. Dla dopasowanych
elementów wywoływany jest ten sam `DodajDodatek(...)`, tym razem z piątym argumentem = wybrana
`Osoba` — stąd taki wpis historii dodatku dostaje wypełnioną cechę `BenefityOsobaDodatkowa`
(patrz §3), co pozwala później rozróżnić na liście/raporcie, czy dany wpis dotyczy pracownika, czy
konkretnej osoby towarzyszącej.

## 5. Zakładka historii dodatku — `HistoriaBenefituA1Extender`

Extender na `Soneta.Kadry.DodHistoria`, dokładający pole **`DefinicjaBenefitu`** — to po prostu
odczyt cechy `BenefityZestawDodatkow` z bieżącego wiersza historii (patrz §1/§3). Widoczny na
formularzu jako zakładka **„Ogólne benefit"** (zasób `DodHistoria.BenefitA1.pageform.xml`), razem z
danymi standardowego dodatku: pracownik, powiązanie, nazwa, okres, data/przyczyna zakończenia
wypłaty, parametry algorytmu (podstawa/czas/dni/procent/ułamek/współczynnik).

`GetListDefinicjaBenefitu` (lista wyboru definicji benefitu) filtruje dostępne definicje wg cechy
**`DozwoloneDefElementow`** ustawionej na roli operatora (`Session.Login.Entitle.Features`) —
jeżeli operator ma tam wpisaną listę dozwolonych ID (inną niż `"0"` = brak ograniczenia), lista jest
zawężana warunkiem `ID In (...)`. To jest **mechanizm uprawnień per rola** — różne role mogą widzieć
różny zestaw definicji benefitów (np. HR Skanska widzi wszystkie, kierownik działu tylko wybrane).

## 6. Raport/lista benefitów — `BenefityA1ViewInfo`

Rejestrowany w menu przez `[FolderView]` na poziomie assembly, z dokładnymi parametrami (odczytane
wprost z atrybutu, nie z domysłu):

| Parametr | Wartość |
|---|---|
| Ścieżka w menu | **Kadry i płace / Kadry / Benefity** |
| GroupIndex | 2 |
| Priority | 10000 |
| Description | „Benefity" |
| TableName | Dodatki |
| ViewType | `BenefityA1ViewInfo` |
| Kontekst licencyjny | `Soneta.Business.Licence.LicencjeModułu` (wartość enuma 6 — moduł Kadry/Płace) |
| RightName | `Kadry.BenefityA1` |

Widok (`BenefityA1.viewform.xml`) ma panel filtrów: **Definicja** (benefitu), **Zakres**,
**Jednostka organizacyjna**, **Z zależnymi** (czy uwzględniać wpisy dot. osób towarzyszących),
**Okres**, **Data** (stan na dzień), **Pracownik** — oraz siatkę z danymi pracownika i **proracją
wpisu** (Aktualność, Podstawa, Ułamek, Procent, Czas, Dni), czyli tym, jak benefit jest naliczany w
niepełnym okresie (np. pracownik dołączył/zwolnił się w trakcie miesiąca). Wiersze pracowników
nieaktywnych są podświetlone na czerwono (`Appearance` warunkowy na
`Pracownik.Workers.ArchiwumInfo.Aktywny = False`).

Parametry widoku (`BenefityA1Params`) są **trwałe** (metody `Save`/`Load` — parametry filtrów
zapisywane per operator, żeby przy kolejnym otwarciu listy nie trzeba było ustawiać filtrów od
nowa).

## 7. Podsumowanie przepływu biznesowego

1. Dział HR definiuje w słowniku pakiety (`ZestawDodatków`) trzech typów (cecha `Pakiet`):
   Karta Sportowa / Opieka / Ubezpieczenie, każdy złożony z jednego lub kilku elementów-dodatków
   (`ElementZestawuDodatków` → `Definicja`). Pakiety "dla osoby towarzyszącej/seniora" są powiązane
   z pakietem pracownika cechą `ZestawOsoby`, a ich elementy dodatkowo otagowane cechą `RodzajOsoby`.
2. HR przyznaje benefit zaznaczonym pracownikom (`DodajZestaw*A1Worker`) na wybrany okres — powstają
   dodatki na liście płac, powiązane cechą `BenefityZestawDodatkow` z definicją pakietu.
3. W razie potrzeby HR dopisuje do już przyznanego pakietu osobę towarzyszącą lub seniora
   (`DodajOsobeDoPakietuA1Worker`) — po walidacji zgodności osoby z regułami pakietu (cecha
   `WeryfikacjaOsoby`) i typu osoby (`RodzajOsoby` na elemencie) powstają dodatkowe wpisy, powiązane
   cechą `BenefityOsobaDodatkowa` z konkretną osobą.
4. Kolejne przyznania tego samego typu benefitu temu samemu pracownikowi domyślnie **przedłużają**
   istniejący wpis (a nie duplikują go) — chyba że użytkownik świadomie zaznaczy „Dodaj kolejny".
5. Cały czas trwania benefitów, dla wszystkich pracowników, jest widoczny i filtrowalny w jednym
   miejscu — liście **Kadry i płace / Kadry / Benefity**, z uwzględnieniem proracji za niepełne
   okresy oraz (opcjonalnie) wpisów dotyczących osób towarzyszących.

## 8. Do potwierdzenia w środowisku docelowym

- Dokładna treść i próg cechy `WeryfikacjaOsoby` (np. minimalny wiek dla `Senior`, dozwolone stopnie
  pokrewieństwa dla `OsTowarzysząca`) nie jest zapisana w DLL — jest to cecha wyliczana zdefiniowana
  na danych w bazie enova, poza kodem dodatku. Do zweryfikowania bezpośrednio w konfiguracji cech.
- Konkretne definicje pakietów (jakie dokładnie kwoty/algorytmy kryją się pod „Karta Sportowa
  Silver" itp.) są danymi słownikowymi, nie kodem — nie są widoczne w DLL.
- Wartość 6 kontekstu licencyjnego (`LicencjeModułu`) odczytana z atrybutu `[FolderView]` nie została
  zweryfikowana względem pełnej listy wartości tego enuma (enum pochodzi z `Soneta.Business`, poza
  tym DLL) — prawdopodobnie odpowiada licencji modułu Kadry/Płace.
