# Benefity w enova365 — dokument powdrożeniowy

Zestawienie wymagań z analizy przedwdrożeniowej (rozdz. **1.8 Benefity**, dot. Medicover, UNUM,
Multisport dla Skanska S.A., SPP, SRD) ze stanem faktycznie potwierdzonym w kodzie dodatku
`AltOne.Skanska.Ext.dll` (moduł `BenefityA1` — szczegółowy opis mechanizmu w
[Benefity.md](Benefity.md)).

**Metoda weryfikacji**: statyczna analiza metadanych .NET i odczyt bajtkodu IL kluczowych metod
dodatku (bez dostępu do bazy danych ani środowiska testowego enova). Oznacza to, że **mechanizm
programistyczny** (workery, walidacje, struktura danych) można potwierdzić wprost z kodu, natomiast
**wartości konfiguracyjne** (konkretne stawki, progi wiekowe, treści cech wyliczanych, zawartość
słowników pakietów) są danymi w bazie enova, poza zasięgiem tej analizy — oznaczone jako „do
weryfikacji w środowisku".

## Legenda statusów

| Symbol | Znaczenie |
|---|---|
| ✅ | Mechanizm potwierdzony wprost w kodzie (konkretna klasa/metoda/warunek IL) |
| 🟡 | Generyczny mechanizm/hook jest obecny, ale konkretna reguła biznesowa jest danymi (cechami) w bazie — nie do zweryfikowania statycznie |
| ⚠️ | **Nie znaleziono** odpowiadającego kodu w analizowanym DLL — do wyjaśnienia (patrz §5) |
| ❓ | Nie da się rozstrzygnąć metodą statyczną (zależy od danych/UI, którego nie widać w metadanych) |

## 1. Widok Benefity (Administrator) — s. 32

| Wymaganie | Status | Dowód / uwaga |
|---|---|---|
| Oddzielny widok dla Administratora Benefitów | ✅ | `BenefityA1ViewInfo`, menu **Kadry i płace/Kadry/Benefity**, prawo `Kadry.BenefityA1` — [Benefity.md §6](Benefity.md#6-raportlista-benefitów--benefitya1viewinfo) |
| Dodawanie abonamentów/kart/ubezpieczeń | ✅ | `DodajZestawKartaSportowaA1Worker` / `DodajZestawOpiekaA1Worker` / `DodajZestawUbezpieczenieA1Worker`, akcje masowe z listy pracowników — [Benefity.md §2](Benefity.md#2-przyznawanie-benefitu-pracownikowi--workery-dodajzestawa1worker) |
| Zamykanie abonamentów/kart/ubezpieczeń | ⚠️ | Brak dedykowanego workera „Zamknij" w analizowanym DLL. Formularz historii dodatku ma standardowe pola enova **`Dodatek.DataZakonczeniaWyplaty`** i **`Dodatek.PrzyczynaZakonczenia`** ([Benefity.md §5](Benefity.md#5-zakładka-historii-dodatku--historiabenefitua1extender)) — najprawdopodobniej zamykanie odbywa się przez ręczne uzupełnienie tych standardowych pól enova, a nie przez custom akcję z tego dodatku. **Do potwierdzenia w środowisku**, czy istnieje osobna akcja zamykania poza tym DLL. |
| Konfiguracja: wysokość dofinansowania, nazewnictwo pakietów, dodawanie/usuwanie pakietów, podział kosztów pracodawca/pracownik | ⚠️ | To operacje na słownikach `ZestawDodatków` / `ElementZestawuDodatków` / `DefElementow` (parametry algorytmu: Podstawa/Procent/Ułamek/Współczynnik — [Benefity.md §3](Benefity.md#3-rdzeń-zapisu--dodajdodatekbasea1workertdodajdodatek)). W analizowanym DLL **nie ma** dedykowanego edytora tych słowników — prawdopodobnie odbywa się to na standardowych listach słownikowych enova (edycja rekordów wprost), bez custom UI z tego dodatku. |
| Rozszerzalny zestaw kolumn widoku „zgodnie z wymaganiami klienta" | ❓ | Widok ma ustalony zestaw kolumn w `BenefityA1.viewform.xml` (Kod, Nazwisko i imię, Aktualność, Podstawa, Współczynnik, Procent, Czas, Dni). Możliwość dodania kolumn to standardowa personalizacja list w enova (dostępna zawsze), a nie dedykowana funkcja tego dodatku. |

## 2. Opieka medyczna Medicover

| # | Wymaganie | Status | Dowód / uwaga |
|---|---|---|---|
| 1 | Ręczne wprowadzenie tylko przez Administratora, na podstawie wniosku papierowego | ✅ | Brak w DLL jakiejkolwiek ścieżki samoobsługowej dla Medicover (w odróżnieniu od Multisport, gdzie samoobsługa przez pulpit jest opisana wprost w analizie) — spójne z wymaganiem |
| 2 | 3 poziomy refundacji wg poziomu zatrudnienia, pełna dla stopnia 4 i 5 | 🟡 | Mechanizm parametryzacji kwoty istnieje (`ParametryDodatkuParams` per element pakietu), ale konkretne stawki i ich powiązanie ze stopniem zaszeregowania to dane słownikowe — do weryfikacji w środowisku |
| 3a | Zmiana abonamentu | 🟡 | Logika reużycia dodatku (§3 w Benefity.md) pozwala na zmianę, ale brak w DLL wymuszenia „zamknij stary, otwórz nowy" jako osobnego kroku — patrz walidacja nr 8 w tabeli §5 poniżej |
| 3b | Dodawanie członków rodziny | ✅ | `DodajOsobeDoPakietuA1Worker`, `Osoba` = `Pracownik.Rodzina` (`CzlonekRodziny`) — [Benefity.md §4](Benefity.md#4-dopisywanie-osoby-towarzyszącej-do-pakietu--dodajosobedopakietua1worker) |
| 3c | Rozszerzenie o CMD | ⚠️ | Pola `DodatkowyPakiet1`/`DodatkowyPakiet2` (mechanizm „pakietów dodatkowych") są w kodzie **widoczne tylko dla `RodzajZestawu == Ubezpieczenie`** (`IsVisibleDodatkowyPakiet1/2`, sprawdzone w IL) — **nie dla `Opieka`**. Oznacza to, że CMD dla Medicover nie korzysta z tego samego mechanizmu co warianty dodatkowe UNUM. Możliwe, że CMD jest modelowany jako osobny `RodzajZestawu`/pakiet w słowniku Opieki, a nie jako „DodatkowyPakiet" — **rozbieżność do wyjaśnienia**, patrz §5 |
| 3d | Karta Senior (zapisy historyczne) | ✅ | `RodzajOsobyA1.Senior` jako jedna z dwóch wartości enuma osoby dodatkowej; historia zachowana strukturalnie (każdy wpis to osobna `DodHistoria`) |
| 3e | Zamykanie abonamentów | ⚠️ | j.w. (§1) |
| 4 | Blokada dopisania dziecka > 25 r.ż. | 🟡 | Hook istnieje (cecha wyliczana `WeryfikacjaOsoby` na `Zestaw`, wywoływana z wybraną osobą, wynik = komunikat błędu blokujący zapis — [Benefity.md §4](Benefity.md#4-dopisywanie-osoby-towarzyszącej-do-pakietu--dodajosobedopakietua1worker)), próg wieku (25 lat) jest danymi w cesze, nie kodem |
| 5 | Dopisywanie po numerze PESEL | ❓ | `Osoba` wybierana z listy `Pracownik.Rodzina` (kartoteka rodziny pracownika w enova ma pole PESEL) — czy wybór/wyszukiwanie odbywa się „po numerze PESEL" to kwestia UI formularza wyboru, nieczytelna z analizy IL |
| 6 | Karta Senior automatycznie wyłączana przy zwolnieniu pracownika | ⚠️ | Nie znaleziono automatycznego triggera/taska zamykającego wpisy benefitowe przy zwolnieniu w analizowanym DLL |
| — | Koszty: medycyna pracy / koszt pracodawcy / koszt pracownika (osobne składniki) | ✅ | Zgodne z modelem — jeden pakiet (`Zestaw`) może zawierać kilka `ElementZestawuDodatków`, każdy tworzący osobny Dodatek na liście płac ([Benefity.md §1](Benefity.md#1-model-danych--jak-benefit-jest-reprezentowany-w-enova)) — dokładnie pasuje do podziału na medycynę pracy / dopłatę pracodawcy / dopłatę pracownika jako osobne składniki |
| — | Blokada dodania CMD do 2 podstawowych wariantów | ⚠️ | Patrz pkt 3c — mechanizm `DodatkowyPakiet` nie obejmuje Opieki w kodzie; jeśli CMD faktycznie idzie tą ścieżką, blokada dla podstawowych wariantów nie jest widoczna w analizowanych metodach |

### Walidacje Medicover (11 pozycji z analizy przedwdrożeniowej)

| # | Walidacja | Status |
|---|---|---|
| 1 | Refundacja zgodna z poziomem zarządczym | 🟡 dane |
| 2 | CMD przypisane do rodzaju karty (pojedyncza/partnerska/rodzinna) | ⚠️ nie potwierdzone |
| 3 | Auto-zamknięcie z końcem miesiąca rozwiązania umowy | ⚠️ nie potwierdzone |
| 4 | Ręczne zamknięcie przez administratora | 🟡 prawdopodobnie std. pole `DataZakonczeniaWyplaty` |
| 5 | Dzieci do 25 r.ż. (wg PESEL) | 🟡 hook `WeryfikacjaOsoby` obecny, próg = dane |
| 6 | 2 rodzaje karty senior (do/powyżej 85 r.ż., wg PESEL) | 🟡 j.w. |
| 7 | Karta Senior wyłączana przy zwolnieniu | ⚠️ nie potwierdzone |
| 8 | Wymuszenie zamknięcia starego abonamentu przy zmianie (brak dwóch naraz) | ⚠️ patrz §5.2 |
| 9 | Powiadomienie e-mail po zmianie abonamentu | ⚠️ nie potwierdzone (patrz §5.3) |
| 10 | Data urodzenia i PESEL wymagane do rejestracji | ❓ standardowa walidacja kartoteki rodziny, poza tym DLL |
| 11 | Brak możliwości usunięcia abonamentu, jeśli zaciągnięty na listę płac | ❓ prawdopodobnie standardowy mechanizm enova (blokada kasowania Dodatku/DodHistorii powiązanej z rozliczoną wypłatą), nie custom kod tego dodatku |

## 3. Ubezpieczenie grupowe UNUM

| # | Wymaganie | Status | Dowód / uwaga |
|---|---|---|---|
| — | Rejestracja/zmiana/rezygnacja ręcznie przez Administratora na podstawie raportów ubezpieczyciela | ✅ | Ten sam generyczny mechanizm co pozostałe benefity (`DodajZestawUbezpieczenieA1Worker`) |
| — | 6 wariantów podstawowych + 2 warianty dodatkowe (U chroni Serce, U chroni onkologicznie) | ✅ | **Dokładne trafienie**: pola `DodatkowyPakiet1`/`DodatkowyPakiet2` istnieją **wyłącznie** dla `DodajZestawUbezpieczenieA1Worker`/`Params` i są widoczne tylko przy `RodzajZestawu == Ubezpieczenie` (`IsVisibleDodatkowyPakiet1/2`, potwierdzone w IL) — [Benefity.md §2](Benefity.md#2-przyznawanie-benefitu-pracownikowi--workery-dodajzestawa1worker) |
| — | Wariant I finansowany w całości przez pracodawcę; dopłata 14,90 zł do pozostałych; warianty dodatkowe potrącane z wynagrodzenia netto | 🟡 | Struktura parametrów (Podstawa/Procent/Ułamek) pozwala to zamodelować per element zestawu, konkretne kwoty to dane |
| — | Ubezpieczenie rodziny w całości opłacane przez pracownika | 🟡 | j.w. — element dla `RodzajOsoby` (osoba towarzysząca) to osobny wpis z własnymi parametrami; brak dopłaty pracodawcy to kwestia konfiguracji tego elementu, nie osobny mechanizm w kodzie |
| — | Rejestracja członków rodziny po numerze PESEL | ❓ | j.w. jak dla Medicover |

### Walidacje UNUM (6 pozycji)

| # | Walidacja | Status |
|---|---|---|
| 1 | Refundacja pracodawcy identyczna dla wszystkich wariantów/pracowników | 🟡 dane |
| 2 | Auto-wyłączenie z końcem miesiąca rozwiązania umowy | ⚠️ nie potwierdzone |
| 3 | Ręczne zamknięcie przez administratora | 🟡 std. pole Dodatku |
| 4 | Rejestracja dzieci pełnoletnich (wg PESEL) | ❓ |
| 5 | Data urodzenia i PESEL wymagane | ❓ poza tym DLL |
| 6 | Brak możliwości usunięcia, jeśli zaciągnięte na listę płac | ❓ standardowy mechanizm enova |

## 4. Dofinansowanie do kart Multisport

### 4a. Skanska S.A.

| Wymaganie | Status | Dowód / uwaga |
|---|---|---|
| Ręczne wprowadzenie na podstawie wniosku papierowego (Administrator) | ✅ | `DodajZestawKartaSportowaA1Worker` |
| Wniosek przez pulpit pracownika | ⚠️ | Ten DLL **nie referencuje `Soneta.Workflow`** (w odróżnieniu od drugiego przeanalizowanego dodatku, `AltOne.EnovaExt.dll`) — mechanizm samoobsługi pracownika przez Pulpit + Workflow (składanie/wycofywanie wniosku, limit do 20. dnia miesiąca) **nie jest częścią tego DLL**. Analiza przedwdrożeniowa wprost odsyła do osobnej sekcji „Workflow" — to potwierdza, że jest to inny komponent/warstwa wdrożenia, poza zakresem tej analizy. |
| Blokada wprowadzenia karty bez złożonego oświadczenia o dochodach ZFŚS | ⚠️ | Nie znaleziono w analizowanym DLL powiązania z oświadczeniem ZFŚS |
| Karta zawsze na czas określony, max rok kalendarzowy | 🟡 | Workery przyjmują dowolny zakres dat (`OdDnia`/`DoDnia`) — ograniczenie do roku kalendarzowego nie jest wymuszone w kodzie (brak walidacji zakresu w analizowanych metodach); może być pilnowane wyłącznie proceduralnie/w warstwie Workflow |
| Wysokość dofinansowania zależna od oświadczenia o dochodach | ⚠️ | Brak w tym DLL odwołania do oświadczenia o dochodach |
| Karty dla os. towarzyszących/dzieci/seniorów niedofinansowane (pracownik pokrywa 100%) | 🟡 | Model (osobne elementy per `RodzajOsoby`) to umożliwia, konkretne stawki (0% dopłaty) to dane |
| Dziecko do 15 r.ż. | 🟡 | Hook `WeryfikacjaOsoby` obecny, próg to dane |
| Student 15–26 r.ż. | 🟡 | `RodzajOsobyA1` w kodzie ma tylko **dwie** wartości: `OsTowarzysząca` i `Senior` — **brak odrębnej wartości „Student"**. Jeśli specyfikacja wymaga osobnej kategorii Studenta, w analizowanym DLL nie ma dla niej dedykowanej wartości enuma — do wyjaśnienia (§5.4) |
| Administrator może czasowo zablokować składanie wniosków (np. ~10 dni w grudniu) | ✅ (częściowo) | Pole **`ZestawDodatków.Blokada`** istnieje i jest sprawdzane w `IsEnabledDodajZestaw` (akcja Administratora wyłącza się, gdy brak niezablokowanych zestawów) — potwierdza istnienie flagi blokady na poziomie pakietu. **Uwaga**: to sprawdzenie dotyczy akcji Administratora w tym DLL; czy ta sama flaga blokuje też wnioski składane przez pracownika przez Pulpit/Workflow (poza tym DLL) — do potwierdzenia w środowisku |

### 4b. SPP i SRD (przez zewnętrzną Kafeterię)

| Wymaganie | Status | Dowód / uwaga |
|---|---|---|
| Wprowadzanie ręczne na podstawie raportu z zewnętrznej Kafeterii | ✅ | Ten sam generyczny mechanizm (`DodajZestawKartaSportowaA1Worker`) — spółka nie zmienia kodu, tylko proces wejściowy (dane z Kafeterii zamiast wniosku papierowego) |
| Pełna refundacja karty pracownika, 100% po stronie pracownika dla os. towarzyszącej/dzieci | 🟡 | Dane |

### Walidacje Multisport (15 pozycji)

| # | Walidacja | Status |
|---|---|---|
| 1 | Rejestracja tylko na obowiązujący rok kalendarzowy | 🟡 nie wymuszone w kodzie workera |
| 2 | Auto-wyłączenie z końcem miesiąca rozwiązania umowy | ⚠️ nie potwierdzone |
| 3 | Ręczne zamknięcie przez administratora | 🟡 std. pole Dodatku |
| 4 | Pracownik zarządza kartami do 20. dnia miesiąca | ⚠️ poza tym DLL (Workflow) |
| 5 | Administrator zarządza bez ograniczeń czasowych | ✅ (pośrednio — brak takiego ograniczenia w kodzie workerów Administratora) |
| 6 | Próg dofinansowania wg oświadczenia o dochodach | ⚠️ nie potwierdzone |
| 7 | Komunikat dla administratora ZFŚS przy zmianie oświadczenia wpływającej na dofinansowanie | ⚠️ nie potwierdzone |
| 8 | Blokada rejestracji karty bez oświadczenia o dochodach | ⚠️ nie potwierdzone |
| 9 | Dzieci do 15 r.ż. (Kids, Kids Aqua) | 🟡 hook obecny, próg = dane |
| 10 | Student 15–26 r.ż. | ⚠️ brak wartości „Student" w enumie `RodzajOsobyA1` — patrz §5.4 |
| 11 | Karta dodatkowa wymaga aktywnej karty głównej pracownika | 🟡 nie potwierdzone wprost w kodzie (brak takiego warunku w `DodajOsobeDoPakietuA1Worker` poza wymogiem istnienia `Zestaw` i cechy `ZestawOsoby`) |
| 12 | Powiadomienie e-mail po zmianie karty | ⚠️ nie potwierdzone (patrz §5.3) |
| 13 | Data urodzenia wymagana | ❓ poza tym DLL |
| 14 | Wymóg załączonej zgody PDF (pierwsza karta lub >9 mies. od ostatniej aktywacji) | ⚠️ nie potwierdzone — brak logiki w analizowanym DLL; jest ogólny mechanizm załączników (`AttachmentExtenderA1` — [Pozostale-funkcje.md §7](Pozostale-funkcje.md)), ale bez wymuszenia go dla Multisport |
| 15 | Brak możliwości usunięcia, jeśli zaciągnięte na listę płac | ❓ standardowy mechanizm enova |

Wymaganie „Pracownik umysłowy powinien mieć możliwość podglądu swoich benefitów na swoim pulpicie"
→ ⚠️ nie potwierdzone w tym DLL (brak `Soneta.Web.Business`/pulpitowego widoku benefitów wśród
zarejestrowanych typów w analizowanym kodzie poza ogólną obsługą pulpitu kierownika/pracownika z
[Pozostale-funkcje.md §7](Pozostale-funkcje.md)).

## 5. Kluczowe rozbieżności / elementy do wyjaśnienia z zespołem wdrożeniowym

### 5.1 Automatyczne zamykanie benefitu przy rozwiązaniu umowy o pracę

Wymaganie pojawia się dla **wszystkich trzech** benefitów (Medicover, UNUM, Multisport) i jest jedną
z częściej wymienianych walidacji w analizie przedwdrożeniowej. W analizowanym DLL **nie ma** kodu,
który nasłuchiwałby zmiany statusu zatrudnienia i automatycznie zamykał wpisy benefitowe
(`DodHistoria`/Dodatek) z końcem miesiąca rozwiązania umowy.

Prawdopodobne wyjaśnienia (do zweryfikowania z zespołem wdrożeniowym / w środowisku):
- funkcjonalność zaimplementowana jest innym mechanizmem enova (standardowy task/weryfikator
  dotyczący `PracHistoria`/`Etat`), poza tym konkretnym DLL,
- w repozytorium **jest już** analogiczny wzorzec dla innego obiektu — zob.
  `Zadania/Aktualizacja OkresuDo w UczestnictwieWAkcji wg daty zakończenia umowy.md` (task
  skracający `OkresDo` na podstawie `Row.PracownikZwolniony`) — możliwe, że dla benefitów
  potrzebny jest analogiczny task na `DodHistoria`, którego jeszcze nie ma w tym DLL,
- funkcjonalność jest zaplanowana, ale nie została jeszcze dostarczona w wersji DLL, którą
  przeanalizowano.

**Rekomendacja**: potwierdzić w środowisku testowym, czy zwolnienie pracownika z aktywnym
benefitem faktycznie skraca `Okres`/`DataZakonczeniaWyplaty` automatycznie, czy wymaga ręcznej
interwencji Administratora Benefitów.

### 5.2 Wymuszenie zamknięcia starego abonamentu przy zmianie (Medicover, walidacja nr 8)

Zaimplementowana logika reużycia dodatku (§3 w [Benefity.md](Benefity.md)) **przedłuża** istniejący
wpis, gdy poprzedni okres historii już się zakończył — to nie jest to samo, co „wymuszenie zamknięcia
starego i otworzenia nowego" wprost przy zmianie wariantu w trakcie trwania abonamentu. Kod nie
zawiera jawnej walidacji blokującej równoczesne istnienie dwóch aktywnych abonamentów tego samego
typu. Do potwierdzenia, czy to wymaganie jest pokryte przez standardowy mechanizm okresów
`DodHistoria` (nowy wpis automatycznie kończy poprzedni), czy wymaga dodatkowej logiki.

### 5.3 Powiadomienia e-mail po zmianie abonamentu

Wymaganie powtarza się dla Medicover i Multisport („powiadomienie wysyłane automatycznie przez
system do pracownika po dokonaniu zmiany abonamentu"). W DLL istnieje gotowa infrastruktura
mailingowa (`WysylkaEmailExt`, `SzablonWiadomosciExt`, `KontoPocztoweExt` —
[Pozostale-funkcje.md §7](Pozostale-funkcje.md)) używana przez **inne** powiadomienia
(`PowiadomienieBadaniaLekarskie`, `PowiadomienieOkresZasilkowy`), ale **żaden analizowany worker
benefitowy jej nie wywołuje**. Powiadomienie o zmianie benefitu albo nie jest jeszcze zaimplementowane,
albo jest realizowane przez zdarzenie/mechanizm poza zakresem tego DLL (np. wyzwalacz na poziomie
bazowego enova, event na `DodHistoria`).

### 5.4 Brak kategorii „Student" w enumie `RodzajOsobyA1`

Wymaganie Multisport wprost wymienia kartę **Student (15–26 r.ż.)** jako osobną kategorię, obok
Kids/Kids Aqua i osoby towarzyszącej/seniora. W kodzie `RodzajOsobyA1` ma tylko dwie wartości:
`OsTowarzysząca` i `Senior`. Możliwe wyjaśnienia:
- studenci są traktowani jako „dzieci" (`OsTowarzysząca`?) z inną definicją wieku w cesze
  `WeryfikacjaOsoby`/`RodzajOsoby` na poziomie elementu zestawu (mechanizm elementów jest
  wystarczająco elastyczny, żeby to obsłużyć bez osobnej wartości enuma — patrz
  [Benefity.md §4](Benefity.md#dodajzestawdodatkow--filtrowanie-elementów-wg-rodzaju-osoby)),
- albo enum wymaga rozszerzenia o wartość `Student` w kolejnej iteracji dodatku.

**Rekomendacja**: zweryfikować w słowniku pakietów Multisport, czy karta Student rzeczywiście działa
poprawnie (osobny próg wieku 15–26 vs. 15 dla dziecka), czy to zidentyfikowana luka do zgłoszenia.

### 5.5 CMD (Centrum Medyczne Damiana) a mechanizm „DodatkowyPakiet"

Jak w §2 pkt 3c — mechanizm `DodatkowyPakiet1`/`DodatkowyPakiet2` jest w kodzie ograniczony
wyłącznie do `RodzajZestawu == Ubezpieczenie` (UNUM). Rozszerzenie o CMD dla Medicover, wymienione
wprost w analizie przedwdrożeniowej, nie korzysta z tego samego mechanizmu w kodzie — prawdopodobnie
jest modelowane inaczej (osobny `Zestaw`/wariant pakietu Opieki w słowniku). Do potwierdzenia, czy
działa to zgodnie z oczekiwaniem („blokada dodania CMD dla 2 podstawowych wariantów").

## 6. Podsumowanie i rekomendowane następne kroki

Rdzeń mechanizmu — model danych (Dodatek + cechy `BenefityZestawDodatkow`/`BenefityOsobaDodatkowa`),
przyznawanie pakietów masowo z listy pracowników, dopisywanie osób towarzyszących/seniorów z
walidacją kwalifikowalności, wspólny widok administracyjny z filtrami i proracją — jest
zaimplementowany spójnie i dobrze odwzorowuje ogólną architekturę wymaganą w analizie
przedwdrożeniowej, w tym nietrywialne, specyficzne dopasowania (2 warianty dodatkowe tylko dla
UNUM, blokada pakietu jako pole na słowniku).

Rzeczy do potwierdzenia z zespołem wdrożeniowym / w testach UAT, zanim dokument powdrożeniowy
zostanie uznany za kompletny:

1. **Automatyczne zamykanie benefitu przy zwolnieniu pracownika** (§5.1) — najważniejsza luka,
   dotyczy wszystkich trzech benefitów.
2. **Powiadomienia e-mail o zmianach abonamentu** (§5.3).
3. **Blokada/limit powiązania z oświadczeniem o dochodach ZFŚS** dla Multisport Skanska S.A.
4. **Kategoria „Student"** w mechanizmie osób dodatkowych Multisport (§5.4).
5. **CMD jako rozszerzenie Opieki** — czy działa analogicznie do wariantów dodatkowych UNUM (§5.5).
6. Potwierdzić, czy funkcje **konfiguracyjne** dla Administratora (stawki, nazwy pakietów, podział
   kosztów) rzeczywiście odbywają się na standardowych słownikach enova, czy istnieje dedykowany
   ekran spoza tego DLL.
7. Zweryfikować, czy **Pulpit Pracownika** (podgląd benefitów, samoobsługa Multisport do 20. dnia
   miesiąca) jest zrealizowany w osobnym komponencie (Workflow) — poza zakresem tej analizy.

Powyższa lista nie oznacza, że funkcje nie istnieją — oznacza, że **nie zostały potwierdzone w
`AltOne.Skanska.Ext.dll`** tą metodą analizy i wymagają albo przeglądu innego komponentu wdrożenia
(np. warstwy Workflow/Pulpitu), albo bezpośredniej weryfikacji w środowisku testowym enova.
