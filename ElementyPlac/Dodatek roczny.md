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
   z datą wypłaty listy płac: `Element.Wyplata.ListaPlac.DataWyplaty` (zmiana 2026-09-29).
   Wcześniej kod porównywał z `Składnik.Okres.To`, co przy okresie naliczania „co 12 miesięcy,
   płatna z dołu" mogło oznaczać koniec okresu naliczania (np. 31.12.2026), a nie dzień
   wypłaty — umowa rozwiązana z dniem 31.12.2026 przechodziła bramkę, choć regulamin każe
   dać 0 (scenariusz TS-19). Umowa kończąca się dokładnie w dniu wypłaty bramkę przechodzi
   (`>=`) — pracownik jest jeszcze zatrudniony tego dnia.
3. **Frekwencja 100% w każdym miesiącu wskazanego okresu** (§2 ust.3-4, §5 ust.5) — w całym
   wskazanym okresie nie może być nieobecności spoza listy dozwolonych wyjątków
   (`CzyDozwolonaNieobecnosc`); jedna łamiąca nieobecność łamie frekwencję za cały okres.
   Nieobecności czytane są z indeksu **`NieobecnosciIdx`** (`WgPracownik` + warunek na okres),
   a nie z `Pracownik.Nieobecnosci` (zmiana 2026-09-29, zgłoszenie klienta). To ta sama lista
   obowiązujących nieobecności, której używa sama enova (kalendarz, wnioski urlopowe).
   **Korekty nieobecności:** przy korekcie enova zostawia pierwotny rekord z flagą
   `Korygowana == true` i dodaje rekord `KorektaNieobecności` z poprawionymi danymi. Korekta ma
   jako źródło pierwotną nieobecność, nie pracownika, więc w `Pracownik.Nieobecnosci` jej nie
   widać (pierwsza wersja poprawki szukała jej tam i w teście u klienta nie znalazła). W indeksie
   enova trzyma pierwotną nieobecność tylko w części **niepokrytej** korektą, a korektę w jej
   okresie (`Nieobecnosc.UpdateIdx`, potwierdzone dekompilacją `Soneta.KadryPlace`). Działa to
   w obie strony:
   - urlop „na żądanie” → zwykły urlop wypoczynkowy: frekwencja zachowana (TS-20),
   - L4 (lub inna łamiąca) → urlop „na żądanie”: frekwencja nadal złamana, wynik 0 (TS-21),
   - korekta części okresu: niepoprawiona część oceniana jak pierwotna nieobecność.

   W zapisie obliczeń korekta ma dopisek „, korekta” (także gdy jest dozwolona).
   **Do potwierdzenia (TS-20, TS-21)** przeliczeniem wypłaty w GUI po wgraniu poprawki.

   **Nieobecność w dni wolne wg grafiku** (zmiana 2026-09-30, zgłoszenie klienta): nieobecność,
   która normalnie łamie frekwencję, jest **pomijana**, jeżeli w okresie dodatku przypada
   wyłącznie na dni, w których pracownik wg grafiku i tak nie miał pracować (zaplanowany czas
   pracy = 0). Przykład z regulaminu praktyki: pracownik ma grafikowo wolny poniedziałek
   i wtorek, nieobecność obejmuje tylko te dwa dni — dodatek nadal przysługuje. Wystarczy jeden
   dzień roboczy wg grafiku w okresie nieobecności, żeby frekwencja została złamana.
   Plan czytany jest z `Element.Pracownik.Czasy.KalkPlanu[data].Czas`
   (`Soneta.Kalend.KalkulatorPracownika` → `KalkulatorPlanu`: kalendarz wzorcowy pracownika
   plus wyjątki `DzienPlanu`) — to **plan pracy**, niezależny od wpisanych nieobecności, więc
   wpisanie nieobecności nie zmienia wyniku tego sprawdzenia. Oceniana jest część nieobecności
   wpadająca w okres dodatku (`i.Okres * rocznyOkres`); dni tej samej nieobecności wykraczające
   poza okres dotyczą frekwencji innego okresu rozliczeniowego i tu się nie liczą.
   Dzień bez planu (np. poza okresem zatrudnienia) traktowany jest zachowawczo jak dzień roboczy
   — o takim przypadku i tak rozstrzyga bramka 1.
   W zapisie obliczeń pominięta nieobecność ma wpis „pominięta, tylko dni wolne wg grafiku”,
   a łamiąca — wskazanie pierwszego dnia roboczego wg grafiku.
   **Do potwierdzenia (TS-22, TS-23)** przeliczeniem wypłaty w GUI po wgraniu poprawki.
