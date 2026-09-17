# Dodatek roczny – konfiguracja elementu wynagrodzenia

Dokumentacja biznesowa i techniczna dla operatora enova365 oraz osoby wdrażającej.

## 1. Podstawa

Element realizuje **Dodatek Roczny** z „Regulaminu premii frekwencyjnej" (regulamin wszedł
w życie dla roku 2026). Regulamin obejmuje też osobną **Premię** kwartalną — ten element
**jej nie dotyczy**, obsługuje wyłącznie Dodatek Roczny.

**Element nie jest przypisany do konkretnego roku.** Okres, za jaki dodatek przysługuje,
wskazuje operator w polu **„Okres"** dodatku (kartoteka pracownika → Etat → Dodatki), a
algorytm bierze go z `Element.DodHistoria.Okres`. Dla dodatku za rok 2026 operator ustawia
okres `1.01.2026–31.12.2026`, dla kolejnego roku — odpowiednio.

Plik importu XML (definicja gotowa do wczytania przez `dbmgr importxml`):
[`ImportyXML/Dodatek roczny.dbinit.xml`](../ImportyXML/Dodatek%20roczny.dbinit.xml)
(GUID rekordu: `c973afe6-a410-4cb8-9428-030ab2213957`).

Scenariusze testowe:
[`ElementyPlac/Dodatek roczny - scenariusze testowe.xlsx`](Dodatek%20roczny%20-%20scenariusze%20testowe.xlsx)
(p. 6).

## 2. Charakterystyka

- **Rodzaj:** Dodatek, okres naliczania **co 12 miesięcy**, płatny z dołu. Wypłacany na liście
  płac w miesiącu następującym po zakończeniu wskazanego okresu (dla okresu 2026 → wypłata 01/2027).
  (Do 2026-09-10 dokumentacja błędnie podawała „jednorazowo" — patrz p. 7.)
- **Kwota:** wpisywana ręcznie przez operatora (pole „Podstawa"/„Kwota" dodatku) — regulamin
  przewiduje stałe 1500,00 PLN brutto, ale element nie ma tego zaszytego na sztywno w kodzie.
- **Okres:** operator ustawia pole „Okres" dodatku na przedział, za jaki dodatek przysługuje
  (np. cały 2026). Bez wskazanego okresu algorytm zwraca 0 z komunikatem błędu w „Zapisie obliczeń".
- **Kto dodaje element:** operator (Dział Kadr i Płac) **ręcznie**, w kartotece pracownika
  (Etat → Dodatki), na listę płac miesiąca wypłaty — wyłącznie pracownikom, dla których
  **wstępnie** ocenił, że dodatek się należy.
- **Algorytm mimo to weryfikuje uprawnienie automatycznie** — jeśli mimo dodania elementu
  pracownik nie spełnia warunków (frekwencja, zatrudnienie przez cały okres, aktywne
  zatrudnienie na dzień wypłaty), wynik jest zerowany. Element **pojawia się na wypłacie
  z kwotą 0** (`GenerujZerowy=Tak`) — czytelny ślad, że naliczenie wykonano; powód zera
  jest w zakładce „Zapis obliczeń".

### Dwie kategorie utraty prawa

| Kategoria | Sprawdzana przez | Warunki |
|---|---|---|
| **A — automatyczna** | algorytm elementu | zatrudnienie przez cały wskazany okres, zatrudnienie trwa na dzień wypłaty, 100% frekwencji w każdym miesiącu wskazanego okresu |
| **B — decyzyjna** | operator, **poza systemem** | alkohol/środki odurzające, zwolnienie dyscyplinarne, naruszenie BHP/ppoż., zawinione narażenie na szkodę majątkową, kara porządkowa, naruszenie dobrego imienia pracodawcy — jeśli wystąpiły, operator **po prostu nie dodaje elementu** |

## 3. Ścieżka konfiguracji

`Narzędzia → Opcje → Kadry i płace → Płace → Elementy wynagrodzenia` (import definicji z pliku XML: komenda `dbmgr importxml` — szczegóły w skillu `soneta-tools`).

## 4. Ustawienia elementu

Stan z eksportu definicji z bazy `Al` (rekord ID 262, eksport 2026-09-10); ten sam stan jest
w bazie `Claude` (ID 263) — porównane kolumna po kolumnie i kod algorytmu.

| Pole | Wartość |
|---|---|
| Nazwa | `Dodatek roczny` |
| Skrót | `Dod.roczny` |
| Rodzaj | Dodatek |
| Rodzaj wypłaty | Etat |
| Naliczanie | Płatna z dołu, **co N miesięcy, N = 12** (`OkresNaliczania`: `Typ=CoNMiesięcy`, `Ilosc=12`, opóźnienie 0) |
| Lista płac | Lista płac-etaty (LPE) |
| Generuj zerowy element | **Tak** (element widoczny na wypłacie także z kwotą 0) |
| Korygowany | **Tak** |
| Do wypłaty | Tak |
| PIT | `PIT-11 1/PIT-4R 1` — Wynagrodzenia ze stosunku: pracy, służbowego, spółdzielczego i z pracy nakładczej (stan bazy `Al` 2026-09-07; wcześniej „PIT-11 1a") |
| Koszty uzyskania przychodu | Ze stosunku pracy |
| Zaliczka podatku | Naliczać wg skali podatkowej, pomniejszona o ZUS; ulga podatkowa — naliczać |
| ZUS społeczne / zdrowotne | Naliczać (standardowo) |
| Podstawa urlopu wypoczynkowego | Nie wliczać (§6 ust.1 regulaminu) |
| Podstawa ekwiwalentu za urlop | Nie wliczać (§6 ust.1 regulaminu) |
| Podstawa zasiłków (pracownicy / inni) | Nie wliczać |
| Algorytm | Edytor algorytmu (kod w p. 5), zapis obliczeń: „Algorytmy płacowe" |

## 5. Algorytm (Edytor algorytmu)

Okres rozliczeniowy dodatku (`rocznyOkres`) algorytm pobiera z `Element.DodHistoria.Okres`
— czyli z pola „Okres" wpisanego przez operatora na dodatku w kartotece. Nie ma tu żadnego
roku zaszytego na sztywno.

Kod w `_Param` sprawdza po kolei bramki i zeruje `Składnik.Podstawa1`, jeśli którakolwiek
nie jest spełniona:

0. **Wskazany okres** — jeśli pole „Okres" jest puste, wynik = 0 i komunikat błędu w logu
   („Zapis obliczeń"), dalsze bramki się nie wykonują.
1. **Zatrudnienie przez cały wskazany okres** (§3 ust.1b) — `Etat.OkresZatrudnienia`
   pokrywa cały `rocznyOkres`.
2. **Zatrudnienie trwa na dzień wypłaty** (§5 ust.7) — porównanie końca zatrudnienia
   z `Składnik.Okres.To`. **Do potwierdzenia (scenariusz TS-19):** przy okresie naliczania
   „co 12 miesięcy, płatna z dołu" `Składnik.Okres` może oznaczać okres naliczania
   (do 31.12.2026), a nie miesiąc wypłaty. Wtedy umowa rozwiązana z dniem 31.12.2026
   przejdzie tę bramkę, choć regulamin każe dać 0.
3. **Frekwencja 100% w każdym miesiącu wskazanego okresu** (§2 ust.3-4, §5 ust.5) — dla
   każdego miesiąca sprawdzane są nieobecności pracownika (`Pracownik.Nieobecnosci[miesiąc]`);
   każda nieobecność musi być na liście dozwolonych wyjątków (`CzyDozwolonaNieobecnosc`),
   inaczej łamie frekwencję za cały okres.

Każdy krok obliczeń zapisuje linię do `Element.ZapisObliczen.Add(...)` — widoczne na
formularzu elementu, zakładka **Zapis obliczeń**: wskazany okres, zatrudnienie przez cały
okres (tak/nie + okres zatrudnienia), aktywność na dzień wypłaty, frekwencja (tak/nie +
lista łamiących nieobecności z nazwą i okresem), wpisana kwota, wynik końcowy.

### Nazwy definicji nieobecności — zweryfikowane w bazach `Al` i `Claude`

Wprost z tabeli `DefNieobecnosci` (61 definicji, te same nazwy w obu bazach), pięć wyjątków z §2 ust.4:

| Wyjątek regulaminowy | Definicja `Nazwa` w `DefNieobecnosci` |
|---|---|
| urlop wypoczynkowy planowany (nie na żądanie) | `Urlop wypoczynkowy` — rozróżnienie „na żądanie" przez pole `Urlop.Przyczyna` na konkretnym zapisie, **nie** osobna definicja |
| urlop okolicznościowy | `Urlop okolicznościowy` |
| zwolnienie art. 188 KP (opieka nad dzieckiem) | `Urlop opiekuńczy (art 188 kp, dni)` **oraz** `Urlop opiekuńczy (art 188 kp, godz.)` — dwa warianty, oba w kodzie |
| badania medycyny pracy | `Badania lekarskie` |
| odbiór dnia wolnego za nadgodziny | **brak dedykowanej definicji Nieobecność** w tym systemie — patrz niżej |

**Ważne rozróżnienie:** `Urlop opiekuńczy (art 188 kp, ...)` **to nie to samo** co
`Zwolnienie opieka (ZUS)` — to drugie jest płatnym z ZUS zasiłkiem opiekuńczym (art. 32-35
ustawy zasiłkowej), inna podstawa prawna niż cytowany w regulaminie art. 188 KP. Kod celowo
używa definicji „Urlop opiekuńczy", nie „Zwolnienie opieka".

**Założenie do potwierdzenia:** „odbiór dnia wolnego za nadgodziny" (§2 ust.4a.iii) nie ma
odpowiednika w `DefNieobecnosci` tej bazy — prawdopodobnie w tym systemie realizowany jest
jako korekta harmonogramu/grafiku pracy (nie generuje rekordu `Nieobecność`), więc **nie
pojawi się** w pętli po `Pracownik.Nieobecnosci` i nie złamie frekwencji — co jest zgodne
z intencją regulaminu, ale nie zostało potwierdzone testem na żywym przypadku (TS-07).
Jeśli w Twoim systemie taki dzień JEST jednak rejestrowany jako `Nieobecność` pod inną nazwą,
trzeba dopisać dla niej osobny `case`.

## 6. Scenariusze testowe

**Scenariusze prowadzone są w arkuszu Excel:**
[`Dodatek roczny - scenariusze testowe.xlsx`](Dodatek%20roczny%20-%20scenariusze%20testowe.xlsx)
(obok tego pliku). Arkusze:

- **Scenariusze** — TS-01…TS-19: obszar (bramka algorytmu), warunki wejściowe, kroki testu,
  oczekiwany wynik, oczekiwana treść „Zapisu obliczeń", pracownik w bazie `Claude`, pliki
  danych testowych oraz kolumny do wypełnienia przy teście: **Status** (lista: Nieprzeprowadzony /
  OK / Błąd / Zablokowany), **Data testu**, **Wynik rzeczywisty / uwagi**.
- **Dane testowe (Claude)** — pięciu pracowników z bazy `Claude` (`Kod` = numer scenariusza),
  ich nieobecności w 2026 i oczekiwany wynik wypłaty 01/2027.
- **Informacje** — konfiguracja istotna dla testów, bramki algorytmu, sposób testowania, legenda.

Testy wymagają przeliczenia wypłaty w GUI (element nie jest walidowany przez
`dbmgr importxml`/`compile` — patrz p. 7). W scenariuszach przyjęto **okres = rok 2026,
wypłata 01/2027**. „Dodatek = 0" oznacza element widoczny na wypłacie z kwotą 0
(`GenerujZerowy=Tak`) wraz z powodem w „Zapisie obliczeń".

Nowe scenariusze dopisuje się w arkuszu, nie w tym pliku. TS-19 (zwolnienie z dniem 31.12.2026)
dodano 2026-09-10 w związku z korektą okresu naliczania (p. 5, bramka 2).

## 7. Stan weryfikacji (na dziś)

Zweryfikowane próbnym importem (`dbmgr importxml`) na bazie testowej `Al`:
- pola `RodzajZrodla`, `DefinicjaListyPlac`, `Deklaracje.PozycjaPIT`, `OkresNaliczania`,
  `Nieobecnosci.Urlop/Ekwiwalent` — poprawnie zmapowane na wartości z bazy,
  idempotencja importu (TS-16) potwierdzona,
- kod używa `Element.ZapisObliczen.Add(...)` (potwierdzone przez użytkownika jako
  działające na żywym systemie — dokumentacja skilla znała wcześniej tylko wzorzec `=`).

Nazwy definicji nieobecności (p. 5) zweryfikowane wprost w tabeli `DefNieobecnosci` bazy `Al`
(zapytanie SQL, 61 definicji) — kod algorytmu zaktualizowany i ponownie zaimportowany.

**Aktualizacja 2026-09-02 (synchronizacja z bazą `Al`):**
- konfiguracja odczytana wprost z `dbo.DefElementow` (ID 262) — operator zmienił w GUI
  `GenerujZerowy` → Tak oraz `Korygowany` → Tak; pozostałe pola (PIT poz. 72 = ten sam GUID,
  ZUS, lista płac, okres naliczania) bez zmian. Plik XML doprowadzony do tego stanu.
- algorytm: `rocznyOkres` pobierany z `Element.DodHistoria.Okres` (pole „Okres" na dodatku
  w kartotece) zamiast `new FromTo(2026-01-01, 2026-12-31)`; dodana bramka 0 (brak okresu → 0).

**Aktualizacja 2026-09-07 (ponowna synchronizacja z bazą `Al`):**
- odczyt wprost z `dbo.DefElementow` (ID 262, guid `C973AFE6-…`) wraz z kolumną `Tekst`
  (kod Edytora algorytmu).
- **Jedyna zmiana operatora:** pozycja PIT. Element wskazywał `PIT-11 1a`
  (`PozycjePIT` ID 72, guid `…-0004-0021-…`); operator przestawił na `PIT-11 1/PIT-4R 1`
  (`PozycjePIT` ID 1, guid `…-0004-0001-…`, „Wynagrodzenia ze stosunku: pracy, służbowego,
  spółdzielczego i z pracy nakładczej…") — standardowa pozycja dla oskładkowanego
  i opodatkowanego dodatku pieniężnego. Plik XML doprowadzony do tego stanu.
- **Kod algorytmu — bez zmian.** Treść w bazie `Al` jest semantycznie identyczna z wersją
  w pliku XML: te same bramki (0–3), ta sama lista dozwolonych nieobecności, ten sam warunek
  „na żądanie" (`PrzyczynaUrlopu.NaŻądanie`). Baza trzyma wariant z tokenami `%NAZWA%`/`%TYP%`
  (podstawiane przez enova) i bez komentarzy; plik XML zachowuje wersję opisaną komentarzami.
- pozostałe kolumny konfiguracji (`RodzajZrodla`=Dodatek, `Zatrudnienie`=Etat,
  `DefinicjaListyPlac`=LPE, `OkresNaliczania` (opisany wtedy błędnie jako Jednorazowa — patrz
  2026-09-10)/PłatnaZDołu, `GenerujZerowy`=True, `Korygowany`=True, ZUS społeczne/zdrowotne=Naliczać,
  zaliczka wg skali, `Nieobecnosci.Urlop/Ekwiwalent`=NieWliczać) — bez zmian względem 2026-09-02.

**Aktualizacja 2026-09-10 (baza `Claude` zsynchronizowana z `Al`):**
- użytkownik wyeksportował definicję z bazy `Al` (plik `DefElementow_20260910082825.xml`,
  rekord `DefinicjaElementu_262`) i wczytał ją do bazy `Claude` (zapis 2026-09-10 8:29).
  Wcześniej `Claude` miał definicję z importu pliku repo (2026-09-03).
- porównanie programowe: wszystkie 193 kolumny `DefElementow` i kod `Tekst` w `Claude` (ID 263)
  są identyczne z `Al` (ID 262) oraz ze stanem odczytanym 2026-09-07. **Logika algorytmu
  bez zmian** (bramki 0–3, lista wyjątków, warunek „na żądanie").
- **korekta dokumentacji i pliku XML — okres naliczania:** kolumna `OkresNaliczaniaTyp = 3`
  to `CoNMiesięcy` (enum `Soneta.Place.TypOkresuNaliczania`: `Jednorazowa` = 1,
  `CoNMiesięcy` = 3), `OkresNaliczaniaIlosc = 12`. Eksport enova potwierdza to wprost
  (`<Typ>CoNMiesięcy</Typ><Ilosc>12</Ilosc>`). Od 2026-09-02 dokumentacja i `dbinit.xml`
  błędnie podawały `Jednorazowa` — ponowny import starego pliku cofnąłby ustawienie w bazie.
  Plik XML poprawiony; opis w p. 2 i 4 poprawiony; dopisany scenariusz TS-19.
- scenariusze testowe przeniesione z tabeli w tym pliku do arkusza Excel (p. 6).

**Niezweryfikowane / do zrobienia przed produkcją:**
- **dynamiczny `rocznyOkres`** — `Element.DodHistoria.Okres` jako źródło okresu wymaga
  potwierdzenia realnym przeliczeniem (TS-17, TS-18); pole `DodHistoria.Okres` (`FromTo`)
  potwierdzone w props `soneta-programming`, ale nie na żywym naliczeniu.
- **bramka 2 przy okresie „co 12 miesięcy, z dołu"** — nie wiadomo, jaki okres enova podaje
  w `Składnik.Okres` (okres naliczania czy miesiąc wypłaty); rozstrzyga TS-19.
- import/kompilacja bazy (`dbmgr importxml`/`compile`) **nie waliduje poprawności kodu C#**
  algorytmu Edytora — potwierdzone eksperymentalnie (celowo zepsuty kod dał identyczny wynik
  sukcesu). Jedyna wiarygodna weryfikacja to realne przeliczenie wypłaty w GUI.
- założenie o „odbiorze dnia za nadgodziny" bez rekordu `Nieobecność` (p. 5) — wynika
  z braku pasującej definicji w `DefNieobecnosci`, ale nie zostało potwierdzone realnym
  przeliczeniem (TS-07).
- scenariusze TS-01…TS-19 (poza TS-16) nieprzeprowadzone na żywym systemie — statusy
  prowadzone w arkuszu `Dodatek roczny - scenariusze testowe.xlsx`.
