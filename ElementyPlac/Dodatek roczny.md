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

## 2. Charakterystyka

- **Rodzaj:** Dodatek, naliczany **jednorazowo**, płatny z dołu. Wypłacany na liście płac
  w miesiącu następującym po zakończeniu wskazanego okresu (dla okresu 2026 → wypłata 01/2027).
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

| Pole | Wartość |
|---|---|
| Nazwa | `Dodatek roczny` |
| Skrót | `Dod.roczny` |
| Rodzaj | Dodatek |
| Rodzaj wypłaty | Etat |
| Naliczanie | Płatna z dołu, jednorazowo |
| Lista płac | Lista płac-etaty (LPE) |
| Generuj zerowy element | **Tak** (element widoczny na wypłacie także z kwotą 0) |
| Korygowany | **Tak** |
| PIT | PIT-11 1a — Wynagrodzenia ze stosunku pracy |
| ZUS społeczne / zdrowotne | Naliczać (standardowo) |
| Podstawa urlopu wypoczynkowego | Nie wliczać (§6 ust.1 regulaminu) |
| Podstawa ekwiwalentu za urlop | Nie wliczać (§6 ust.1 regulaminu) |

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
2. **Zatrudnienie trwa na dzień wypłaty** (§5 ust.7) — porównanie z `Składnik.Okres.To`.
3. **Frekwencja 100% w każdym miesiącu wskazanego okresu** (§2 ust.3-4, §5 ust.5) — dla
   każdego miesiąca sprawdzane są nieobecności pracownika (`Pracownik.Nieobecnosci[miesiąc]`);
   każda nieobecność musi być na liście dozwolonych wyjątków (`CzyDozwolonaNieobecnosc`),
   inaczej łamie frekwencję za cały okres.

Każdy krok obliczeń zapisuje linię do `Element.ZapisObliczen.Add(...)` — widoczne na
formularzu elementu, zakładka **Zapis obliczeń**: wskazany okres, zatrudnienie przez cały
okres (tak/nie + okres zatrudnienia), aktywność na dzień wypłaty, frekwencja (tak/nie +
lista łamiących nieobecności z nazwą i okresem), wpisana kwota, wynik końcowy.

### Nazwy definicji nieobecności — zweryfikowane w bazie `Al`

Wprost z tabeli `DefNieobecnosci` (61 definicji na bazie `Al`), pięć wyjątków z §2 ust.4:

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
z intencją regulaminu, ale nie zostało potwierdzone testem na żywym przypadku (TS-07 poniżej).
Jeśli w Twoim systemie taki dzień JEST jednak rejestrowany jako `Nieobecność` pod inną nazwą,
trzeba dopisać dla niej osobny `case`.

## 6. Scenariusze testowe

Testy wymagają przeliczenia wypłaty na przykładowym pracowniku w GUI (element sam w sobie
nie jest walidowany przez `dbmgr importxml`/`compile` — to potwierdzone empirycznie, patrz
p. 7). Nazwy definicji nieobecności są już uzupełnione (p. 5) — **TS-07 (odbiór nadgodzin)
weryfikuje założenie**, że taki dzień nie generuje rekordu `Nieobecność` w tym systemie.

W scenariuszach przyjęto przykładowo **okres = rok 2026, wypłata 01/2027**; operator wskazuje
okres w polu „Okres" dodatku. „Dodatek = 0" oznacza element widoczny na wypłacie z kwotą 0
(`GenerujZerowy=Tak`) wraz z powodem w „Zapisie obliczeń".

| ID | Scenariusz | Warunki wejściowe | Oczekiwany wynik |
|---|---|---|---|
| TS-01 | Pełne uprawnienie | Zatrudniony cały 2026, brak nieobecności naruszających frekwencję, aktywny w 01/2027 | Dodatek = kwota wpisana operatorowi |
| TS-02 | Nieusprawiedliwiona nieobecność | Jak TS-01, plus 1 dzień nieobecności nieusprawiedliwionej w dowolnym miesiącu 2026 | Dodatek = 0, log wskazuje łamiącą nieobecność |
| TS-03 | Choroba (L4) | Jak TS-01, plus zwolnienie lekarskie w dowolnym miesiącu | Dodatek = 0 (choroba nie jest na liście wyjątków §2 ust.4) |
| TS-04 | Urlop na żądanie | Jak TS-01, plus 1 dzień urlopu wypoczynkowego „na żądanie" | Dodatek = 0 (wyjątek §2 ust.4a.i wyklucza „na żądanie") |
| TS-05 | Urlop planowy | Jak TS-01, plus urlop wypoczynkowy planowany (nie na żądanie) | Dodatek = kwota wpisana (dozwolony wyjątek) |
| TS-06 | Urlop okolicznościowy | Jak TS-01, plus urlop okolicznościowy | Dodatek = kwota wpisana |
| TS-07 | Odbiór nadgodzin | Jak TS-01, plus dzień wolny za nadgodziny | Dodatek = kwota wpisana — **weryfikuje założenie z p. 5**: jeśli w Twoim systemie taki dzień jednak generuje rekord `Nieobecność`, test wykaże 0 zamiast kwoty (sygnał, że trzeba dopisać `case`) |
| TS-08 | Opieka / badania | Jak TS-01, plus `Urlop opiekuńczy (art 188 kp, dni)` **albo** `(art 188 kp, godz.)` **albo** `Badania lekarskie` | Dodatek = kwota wpisana |
| TS-09 | Niepełny rok zatrudnienia | Zatrudniony od marca 2026, 100% frekwencji w okresie zatrudnienia | Dodatek = 0 (brak pełnego roku kalendarzowego) |
| TS-10 | Zwolnienie przed wypłatą | Rozwiązanie umowy w grudniu 2026, przed terminem wypłaty 01/2027 | Dodatek = 0, niezależnie od trybu/przyczyny zwolnienia |
| TS-11 | Zwolnienie po wypłacie | Rozwiązanie umowy w lutym 2027 (po wypłacie 01/2027), reszta warunków spełniona | Dodatek = kwota wpisana |
| TS-12 | Kategoria B — brak elementu | Pracownik ukarany karą porządkową w 2026 | Operator **nie dodaje** elementu — dodatek nieobecny na liście płac (test proceduralny, nie algorytmu) |
| TS-13 | Kwota zerowa wpisana operatorowi | Jak TS-01, ale operator wpisał 0,00 | Dodatek = 0, brak błędu obliczeń |
| TS-14 | Podstawa urlopu/ekwiwalentu | Naliczenie urlopu wypoczynkowego / ekwiwalentu w okresie, gdy dodatek był wypłacony | Dodatek roczny **nie wchodzi** do podstawy (§6 ust.1) |
| TS-15 | Log obliczeń — kompletność | Dowolny z powyższych scenariuszy | Zakładka „Zapis obliczeń" zawiera wszystkie linie (pełny rok, aktywność, frekwencja + ew. łamiące nieobecności, kwota, wynik) ze zgodnymi wartościami |
| TS-16 | Idempotencja importu | Dwukrotny `dbmgr importxml` tym samym plikiem na tej samej bazie | Jeden rekord definicji (bez duplikatu) — zweryfikowane, patrz p. 7 |
| TS-17 | Brak wskazanego okresu | Jak TS-01, ale operator nie wypełnił pola „Okres" na dodatku | Dodatek = 0; „Zapis obliczeń" zawiera linię „BŁĄD: nie wskazano okresu dodatku" |
| TS-18 | Okres inny niż 2026 | Operator ustawia „Okres" = 1.01.2025–31.12.2025, pracownik spełnia warunki w 2025 | Dodatek = kwota wpisana; log odwołuje się do okresu 2025 (nie 2026) |

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

**Niezweryfikowane / do zrobienia przed produkcją:**
- **dynamiczny `rocznyOkres`** — `Element.DodHistoria.Okres` jako źródło okresu wymaga
  potwierdzenia realnym przeliczeniem (TS-17, TS-18); pole `DodHistoria.Okres` (`FromTo`)
  potwierdzone w props `soneta-programming`, ale nie na żywym naliczeniu.
- import/kompilacja bazy (`dbmgr importxml`/`compile`) **nie waliduje poprawności kodu C#**
  algorytmu Edytora — potwierdzone eksperymentalnie (celowo zepsuty kod dał identyczny wynik
  sukcesu). Jedyna wiarygodna weryfikacja to realne przeliczenie wypłaty w GUI.
- założenie o „odbiorze dnia za nadgodziny" bez rekordu `Nieobecność` (p. 5) — wynika
  z braku pasującej definicji w `DefNieobecnosci`, ale nie zostało potwierdzone realnym
  przeliczeniem (TS-07).
- scenariusze TS-01 do TS-16 nieprzeprowadzone na żywym systemie.
