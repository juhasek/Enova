# Kontrole planu z algorytmu kalendarza (Dodatkowy kod do kompilacji)

Fragment **Dodatkowego kodu do kompilacji** (System → Dodatkowy kod do kompilacji, tabela
`CodeFiles`, plik `A1WeryfikatoryKalendarza`) z kontrolami planu pracy przeniesionymi z
zakładki **Algorytm** definicji kalendarza do **definicji weryfikatorów kalendarza**.

Źródło: algorytm kalendarza „Podstawowy system_1_8:16” (baza testowa, `Kalendarze.ID` = 47).
Kalendarz „3 miesięczny” (ID 45) ma kopię tego algorytmu z innym zestawem włączonych kontroli
(patrz [Podpięcie do kalendarzy](#podpięcie-do-kalendarzy)).

## Dlaczego przenosimy

- Kod algorytmu jest skopiowany w każdym kalendarzu osobno (2 kopie, już się rozjechały).
  Po przeniesieniu kod jest w jednym miejscu, a kalendarz tylko wybiera kontrole.
- Poziom (Ostrzeżenie / Błąd) ustawia się przy podpięciu do kalendarza, a nie w kodzie
  (`!` na początku komunikatu).
- Usunięty błąd sklejania komunikatów, patrz [Słabe miejsca, pkt 1](#1-komunikaty-algorytmu-i-weryfikatorów-sklejają-się-a-błąd-staje-się-ostrzeżeniem).

enova woła algorytm kalendarza i definicje weryfikatorów w tym samym miejscu
(zdekompilowane `Soneta.KadryPlace.dll` 2512.5.6):

| Moment | Algorytm kalendarza | Definicje weryfikatorów |
|---|---|---|
| Zapis dnia planu (`KontrolaDniaVerifier`) | `SprawdźDzieńPlanu(dzień)` | rodzaj **Dzień planu** (30) |
| Zapis dnia planu, kontrola miesiąca tego dnia (`KontrolaPlanuVerifier`) | `SprawdźPlan(pracownik, miesiąc)` | rodzaj **Plan pracy** (40) |

Przeniesienie zachowuje więc moment uruchomienia 1:1.

## Metody

Wszystkie metody to `public static string` w klasie
`A1.Runtime.KadryPlace.WeryfikatoryKalendarza.TblCodeFiles.A1WeryfikatoryKalendarza`
i zwracają `null`, gdy nie ma uwag. Plik w repo zawiera **tylko** nowe metody (oraz
prywatne metody pomocnicze z prefiksem `A1`). Wkleić je do istniejącej klasy, nie zastępować
całego pliku.

| Metoda | Odpowiednik w algorytmie | Co sprawdza |
|---|---|---|
| `A1Przerwa11h(kp, data)` | `KontrolaPrzerwy11h` | 11h odpoczynku w obrębie doby |
| `A1DobaPracownicza(kp, data)` | `KontrolaDobaPracownicza` | praca nie zaczyna się wcześniej niż wczoraj / później niż jutro |
| `A1NormaDobowaNiepelnosprawnych(kp, data)` | `ControlWorkHoursForDisabled` | 7h / 8h dla osób z niepełnosprawnością |
| `A1Praca12h(kp, data)` | `ControlWork12h` | plan dnia ≤ 12:00 |
| `A1Przerwa11hWMiesiacu`, `A1DobaPracowniczaWMiesiacu`, `A1NormaDobowaNiepelnosprawnychWMiesiacu`, `A1Praca12hWMiesiacu` `(pracownik, miesiąc)` | pętla po dniach w `SprawdźPlan` | jw. dla każdego dnia miesiąca z efektywnym etatem |
| `A1NormaSredniotygodniowa48h(pracownik, miesiąc)` | `KontrolaNormyŚredniotygodniowej` | przeciętnie 48h/tydzień w okresie rozliczeniowym |
| `A1OdpoczynekTygodniowy35h(pracownik, miesiąc)` | `KontrolaPrzerwa35h` | 7 dni pracy w tygodniu |
| `A1DniWolneIswietaWMiesiacu(pracownik, miesiąc)` | `ControlRequiredCountOfFreeOrHolidayDays` | dni wolne + święta w planie ≥ w miesiącu kalendarzowym |
| `A1PlanPonadNormeOkresuRozliczeniowego(pracownik, miesiąc)` | `ControlToManyWorkDaysInPeriod` | plan okresu rozliczeniowego ≤ norma kodeksowa (`CzasPracyEtatWorker`) |

Nie przeniesiono: `KontrolaNormy8h` (w algorytmie zakomentowana) i `Trace` (nieużywana).

Zmiany względem algorytmu (poza podziałem na metody):
- usunięta linia diagnostyczna `try {throw new ArgumentNullException();} catch {}`,
- poprawione literówki w komunikatach („godzine” → „godzinie”, „przekaczający” →
  „przekraczający”, „normę czas” → „normę czasu”),
- każda kontrola miesiąca tworzy własny `KalkulatorPlanu` (w algorytmie był jeden wspólny).

## Definicje weryfikatorów

Ciało metody `Weryfikuj` w każdej definicji to jedno wywołanie. Prefiks klasy w skrócie
`K.` = `A1.Runtime.KadryPlace.WeryfikatoryKalendarza.TblCodeFiles.A1WeryfikatoryKalendarza.`
(w definicji trzeba wpisać pełną nazwę).

**Dzień planu** (rodzaj 30), sygnatura
`public override string Weryfikuj(Soneta.Kalend.DzienPlanu dzien, WeryfikujEventArgs args)`:

| Nazwa | Wywołanie |
|---|---|
| A1_Praca do 12h (dzień) | `return K.A1Praca12h(new KalkulatorPlanu(dzien.Kalendarz.Pracownik), dzien.Data);` |
| A1_Norma niepełnosprawnych (dzień) | `var pracownik = dzien.Kalendarz.Pracownik; if (pracownik == null) return null; return K.A1NormaDobowaNiepelnosprawnych(new KalkulatorPlanu(pracownik), dzien.Data);` |

**Plan pracy** (rodzaj 40), sygnatura
`public override string Weryfikuj(Soneta.Kadry.Pracownik pracownik, WeryfikujEventArgs args)`:

| Nazwa | Wywołanie |
|---|---|
| A1_Odpoczynek dobowy 11h | `return K.A1Przerwa11hWMiesiacu(pracownik, args.Miesiąc);` |
| A1_Doba pracownicza | `return K.A1DobaPracowniczaWMiesiacu(pracownik, args.Miesiąc);` |
| A1_Praca do 12h | `return K.A1Praca12hWMiesiacu(pracownik, args.Miesiąc);` |
| A1_Norma niepełnosprawnych | `return K.A1NormaDobowaNiepelnosprawnychWMiesiacu(pracownik, args.Miesiąc);` |
| A1_Norma średniotygodniowa 48h | `return K.A1NormaSredniotygodniowa48h(new KalkulatorPlanu(pracownik), args.Miesiąc.LastDay);` |
| A1_Odpoczynek tygodniowy 35h | `return K.A1OdpoczynekTygodniowy35h(pracownik, args.Miesiąc);` |
| A1_Dni wolne i święta w miesiącu | `return K.A1DniWolneIswietaWMiesiacu(pracownik, args.Miesiąc);` |
| A1_Plan ponad normę okresu rozl. | `return K.A1PlanPonadNormeOkresuRozliczeniowego(pracownik, args.Miesiąc);` |

## Podpięcie do kalendarzy

`Kalendarz → Weryfikatory`, `Typ`: 100 = Ostrzeżenie, 200 = Błąd.

| Definicja | Podstawowy system_1_8:16 (ID 47) | 3 miesięczny (ID 45) |
|---|---|---|
| A1_Praca do 12h (dzień) | Ostrzeżenie | Ostrzeżenie |
| A1_Norma niepełnosprawnych (dzień) | Ostrzeżenie | — (w algorytmie liczona, ale nie zwracana) |
| A1_Odpoczynek dobowy 11h | Ostrzeżenie | Ostrzeżenie |
| A1_Doba pracownicza | Ostrzeżenie | Ostrzeżenie |
| A1_Praca do 12h | Ostrzeżenie | Ostrzeżenie |
| A1_Norma niepełnosprawnych | Ostrzeżenie | — |
| A1_Norma średniotygodniowa 48h | Ostrzeżenie | Ostrzeżenie |
| A1_Odpoczynek tygodniowy 35h | Ostrzeżenie | Ostrzeżenie |
| A1_Dni wolne i święta w miesiącu | Ostrzeżenie | — (zakomentowana) |
| A1_Plan ponad normę okresu rozl. | **Błąd** | — (zwraca zawsze `null`) |

Tak odtwarza się dotychczasowe zachowanie obu kalendarzy. Do obu są już podpięte
`A1_Okres zatrudnienia`, `A1_Norma w okresie rozliczeniowym` i `A1_Ilość dni wolnych w okresie`
— zostają bez zmian (ale patrz pkt 2 słabych miejsc).

## Wgranie

1. **System → Dodatkowy kod do kompilacji**, plik `A1WeryfikatoryKalendarza`: wkleić metody
   z pliku `KontrolePlanuZAlgorytmuKalendarza` do klasy. **Przed definicjami**, inaczej
   definicje się nie skompilują.
2. Utworzyć 10 definicji weryfikatorów kalendarza z tabel powyżej.
3. Podpiąć je do kalendarzy wg tabeli podpięć.
4. Dopiero potem **wyczyścić zakładkę Algorytm** obu kalendarzy (zostawić puste
   `SprawdźDzieńPlanu` / `SprawdźPlan` zwracające `null` albo usunąć kod). Bez tego każda
   uwaga pojawi się dwa razy.
5. Test: pracownik na kalendarzu ID 47, dzień planu 13h → ostrzeżenie „przekraczający
   dopuszczalną normę godzinową (12:00)”; plan okresu ponad normę → zapis zablokowany.

Pole `Nazwa` definicji ma maksymalnie 40 znaków (stąd skróty „niepełnosprawnych”, „rozl.”).

Status: kod skompilowany lokalnie przeciw bibliotekom enova 2512.5.6 (cała klasa z bazy +
nowe metody + ciała wszystkich 10 definicji) — **bez błędów**. Nie sprawdzony na żywo w enova.

### Stan w bazie testowej (2026-10-06)

Wgrane bezpośrednio SQL-em, w jednej transakcji (kroki 1–4):
- `CodeFiles` ID 13 (`A1WeryfikatoryKalendarza`) — klasa uzupełniona o nowe metody,
- `DefWeryfKalend` ID **2038–2047** (kolejność jak w tabelach definicji, `RuntimeInfoProject` = 29),
- `WeryfKalend` — podpięcia do kalendarzy 47 i 45 wg tabeli podpięć,
- `Kalendarze.Algorytm` kalendarzy 45 i 47 wyczyszczony (= „Edycja algorytmu” wyłączona).
  Poprzednia treść: pliki `KontrolePlanuZAlgorytmuKalendarza - archiwum algorytmu kalendarza 47/45`.

**Aktualizacja (2026-10-06, po decyzji użytkownika):** w kodzie zostały tylko
`A1NormaDobowaNiepelnosprawnych` i `A1NormaSredniotygodniowa48h`. Reszta kontroli jest
zakomentowana na końcu pliku albo usunięta — zostaną zastąpione standardowymi weryfikatorami
enova albo nie są potrzebne. Skutki w bazie:
- aktywne i podpięte zostały: **2039** „A1_Norma niepełnosprawnych (dzień)” (kalendarz 47,
  Ostrzeżenie) i **2044** „A1_Norma średniotygodniowa 48h” (kalendarze 45 i 47, Ostrzeżenie),
- **zablokowane** (`Blokada` = 1): 2038, 2040–2043, 2045–2047. Wywołanie metody w ich kodzie
  jest zakomentowane, a definicja zwraca `null`. Robimy tak, bo enova generuje kod do
  kompilacji także dla zablokowanych definicji, a wołane metody już nie istnieją. Przywrócenie
  definicji: odkomentować metodę w klasie, odkomentować wywołanie, zdjąć blokadę i podpiąć ją.
- podpięcia zablokowanych definicji do kalendarzy usunięte (w GUI: Typ = „Brak”). Musi tak być,
  bo zakładka Weryfikatory kalendarza ładuje tylko aktywne definicje i szuka podpięć przez
  `Single(...)`. Zablokowana, a podpięta definicja wywróciłaby tę zakładkę wyjątkiem.

Uwaga: `dbmgr compile Al` zwracał kod 0 także wtedy, gdy definicje wołały nieistniejące
metody. Nie potwierdza więc kompilacji definicji weryfikatorów. Kompilację sprawdzono lokalnie.

Zdublowane weryfikatory (pkt 2 słabych miejsc) celowo zostawione. `dbmgr compile Al`
kończy się bez błędów. Test na żywo (krok 5) jeszcze nie wykonany. Przed testem trzeba
zrestartować serwer enova albo odświeżyć bazę, bo zmiany wprowadzone SQL-em omijają cache aplikacji.

## Słabe miejsca

### 1. Komunikaty algorytmu i weryfikatorów sklejają się, a błąd staje się ostrzeżeniem

Dotyczy stanu **przed** przeniesieniem. `KontrolaPlanuVerifier` robi
`text = SprawdźPlan(...)` i dokleja wyniki definicji. Algorytm nie kończy tekstu znakiem nowej
linii, więc pierwszy komunikat definicji przykleja się do ostatniej linii algorytmu. Poziom
błędu enova rozpoznaje **tylko po `!` na pierwszym znaku całego tekstu**. Gdy algorytm zwróci
dowolne ostrzeżenie, błąd z `A1_Norma w okresie rozliczeniowym` (podpiętej jako Błąd) ląduje
w środku tekstu: zapis **nie jest blokowany**, a w treści widać zbędny `!`. Po przeniesieniu
enova sortuje weryfikatory wg poziomu (najpierw Błąd), więc problem znika.

### 2. Te same reguły sprawdzane dwa lub trzy razy

| Reguła | Algorytm (teraz definicja) | Inne weryfikatory na tym samym kalendarzu / w systemie |
|---|---|---|
| Norma okresu rozliczeniowego | `A1PlanPonadNormeOkresuRozliczeniowego` (Błąd) | `A1_Norma w okresie rozliczeniowym` (Błąd, podpięty) + wbudowany „Norma w okresie rozliczeniowym” |
| Dni wolne | `A1DniWolneIswietaWMiesiacu` (miesiąc) | `A1_Ilość dni wolnych w okresie` (okres rozliczeniowy, podpięty) |
| 11h odpoczynku | `A1Przerwa11h` | wbudowany „Odpoczynek dobowy” + własny weryfikator `11-sto godzinny odpoczynek` w repo |
| 35h odpoczynku | `A1OdpoczynekTygodniowy35h` | wbudowany „Odpoczynek tygodniowy” |
| Limit dobowy | `A1Praca12h` | wbudowany „Norma dobowa” |

Przekroczenie normy okresu daje dziś **dwa błędy** o tym samym. Dni wolne liczą dwie
**różne** reguły (miesiąc vs okres rozliczeniowy, inna definicja wymaganej liczby) — mogą dać
sprzeczne komunikaty. Rekomendacja: zostawić po jednej kontroli na regułę. Najprościej:
nie podpinać `A1_Plan ponad normę okresu rozl.` i `A1_Dni wolne i święta w miesiącu`
tam, gdzie są `A1_Norma…` i `A1_Ilość dni wolnych…`.

### 3. Limit 12h w systemie podstawowym

Kalendarz ID 47 to system podstawowy (`RownowaznyCzasPracy` = 0). Tam plan powyżej normy
dobowej (8h) to zaplanowane nadgodziny, a kontrola sprawdza dopiero 12h. Dzień 10h przejdzie
bez uwag. Limit 12h pasuje do równoważnego systemu (kalendarz ID 45).

**Stan 2026-10-07:** definicja 2039 „A1_Norma niepełnosprawnych (dzień)” celowo zablokowana i odpięta od
kalendarza 47 (zablokowana, a podpięta wywracałaby zakładkę Weryfikatory). Przy odblokowaniu podpiąć
ponownie w GUI jako Ostrzeżenie. Podpięcia wstawione SQL-em są widoczne w GUI dopiero po restarcie
serwera enova — wcześniej zakładka pokazuje „Brak”, a próba włączenia kończy się błędem duplikatu klucza.

**Rozwiązane (2026-10-06):** `A1Praca12h` przeniesiona do sekcji zakomentowanej (definicja 2038
dalej zablokowana). Zastępuje ją nowy weryfikator `A1NormaDobowa` — definicja **2048**
„A1_Norma dobowa” (Dzień planu), podpięta do kalendarzy 45 i 47 jako Ostrzeżenie:
`var pracownik = dzien.Kalendarz.Pracownik; if (pracownik == null) return null;
return K.A1NormaDobowa(new KalkulatorPlanu(pracownik), dzien.Data);`.
Limit = `Etat.NormaDobowa` z zapisu historii obowiązującego w dniu (pole „Norma dobowa” na
etacie, a gdy puste — `Nadgodziny.WartoscDobowa` kalendarza etatu); gdy kalendarz etatu ma
`RownowaznyCzasPracy` — 12h. W odróżnieniu od wbudowanej „Normy dobowej” uwzględnia normę
z etatu i równoważny system (wbudowana na kalendarzu 45 z Wartością dobową 8:00 zgłaszałaby
każdy dzień 10–12h).

### 4. Okres rozliczeniowy liczony na dwa sposoby

**Rozwiązane dla 48h (2026-10-06).** `A1NormaSredniotygodniowa48h` przepisana na wzór
`A1WeryfikujNormeWOkresieRozliczeniowym`: wejście `(KalkulatorPlanuBase, Date)`, okres z
`pracownik.WyliczOkresRoliczeniowyNadgodzin(data)` przycięty do zatrudnienia przez
`ZrodloPlanu.GetPlanOkresZatrudnienia` (tak jak wbudowana „Norma w okresie rozliczeniowym”),
jeden komunikat przez `TranslateFormat` z „- {pracownik}” na końcu. Sekcja „Pomocnicze”
(`A1TeoretycznyOkresRozliczeniowy`, `A1OkresyZatrudnieniaWMiesiacu`, `A1NormaSredniotygodniowa`,
`A1ZPracownikiem`, `A1Dopisz`, `A1KontrolaDniMiesiaca`) usunięta. Definicja 2044 woła teraz
`A1NormaSredniotygodniowa48h(new KalkulatorPlanu(pracownik), args.Miesiąc.LastDay)`.
Zakomentowane stare wersje na końcu pliku nadal odwołują się do usuniętych helperów —
przy ewentualnym przywróceniu trzeba je przepisać tym samym wzorem.

Opis pierwotnego problemu (dotyczy już tylko zakomentowanej kontroli 35h):

48h i 35h liczą okres „teoretycznie”: od stycznia co `Nadgodziny.Okres` miesięcy
(`A1TeoretycznyOkresRozliczeniowy`). Norma okresu używa `WyliczOkresRoliczeniowyNadgodzin`
z enova. Skutki:
- przesunięcie okresu (`NadgodzinyPrzesuniecie`) i inny typ okresu są ignorowane — 48h/35h
  sprawdzą inny okres niż norma,
- `Nadgodziny.Okres` = 0 → dzielenie przez zero (wyjątek przy zapisie dnia),
- okres, który nie dzieli 12 (np. 5 mies.) → miesiąc 13+ w `new YearMonth(r, e)` → wyjątek.

W bazie oba kalendarze mają okres 1 i 3 bez przesunięcia, więc dziś to nie występuje.

### 5. Odpoczynek tygodniowy 35h nie mierzy 35h

Kontrola zgłasza tylko tydzień z **7 dniami pracy**. Tydzień z 6 dniami, ale z przerwą
krótszą niż 35h (np. noc z soboty na niedzielę i start w poniedziałek rano), przejdzie.
Dodatkowo sprawdza **cały** okres rozliczeniowy, nie tylko edytowany miesiąc. Przy okresie
3-miesięcznym ten sam tydzień jest zgłaszany przy edycji każdego z trzech miesięcy.

### 6. Odpoczynek dobowy 11h i doba pracownicza

- 11h liczone do godziny rozpoczęcia **tego samego** dnia w dobie następnej
  (`OdGodziny + 24h`), a nie do faktycznego startu następnego dnia. To się broni tylko razem
  z kontrolą doby pracowniczej — dlatego obie trzeba podpinać razem.
- Gdy dowolna strefa dnia nie ma definicji, kontrola 11h jest pomijana dla całego dnia.
- Doba pracownicza zgłasza ten sam konflikt dwa razy (przy dniu d jako „następna doba” i przy
  d+1 jako „poprzednia doba”).

### 7. Dni wolne w miesiącu

- `Date.FreeOrHoliday` = soboty + niedziele + święta. Święto w sobotę liczone jest raz, a
  pracownikowi przysługuje za nie dodatkowy dzień wolny — wymóg wychodzi zaniżony.
- Brak uwzględnienia zatrudnienia od/do połowy miesiąca i okresu wielomiesięcznego (stąd
  wyłączenie w kalendarzu ID 45).
- Typ dnia porównywany przez `ToString()` z tekstem „Świąteczny” / „Wolny” — działa (nazwy
  w enumie `TypDnia` się zgadzają), ale bezpieczniej porównać z `TypDnia.Wolny` /
  `TypDnia.Świąteczny`.

### 8. Norma niepełnosprawnych

Sprawdza tylko normę dobową. Nie ma limitu tygodniowego (35h / 40h) ani zakazu pracy w nocy
i w nadgodzinach.

**Poprawione (2026-10-06):** wejście `(KalkulatorPlanuBase, Date)` i komunikat przez `TranslateFormat`
(na wzór wbudowanej „Normy dobowej”). Stopień i zgoda na 8h czytane z zapisu historii
obowiązującego w dniu (`pracownik[data]`), a nie z pierwszego pasującego zapisu. Stopień porównywany
z enumem `StNiepełnosprawności`, a nie tekstem. Brany wyższy stopień z danych o niepełnosprawności
i z danych PFRON, okres `OkresExt` (jak enova przy limicie urlopu). Definicja 2039 pomija dzień
kalendarza bez pracownika.

**Nowy weryfikator (2026-10-13): definicja 2069 „A1_Brak zgody na pracę 8h”** (Dzień planu, priorytet 29),
podpięta do kalendarzy 45 i 47 jako **Błąd**, więc planu nie da się zapisać. Metoda `A1BrakZgodyNaPrace8h(kp, data)`
w tej samej klasie. Reguła: pracownik ze stopniem **umiarkowanym lub znacznym** (dane o niepełnosprawności
albo PFRON, wyższy z nich, okres `OkresExt`), który w zapisie historii obowiązującym w dniu nie ma
zaznaczonego na etacie pola „Zgoda na pracę 8h” (`Etat.PracownikNiepelnosprawnyZgodaNaPrace8h`),
nie może mieć zaplanowanych więcej niż 7:00 w dniu. Stopień lekki i brak stopnia nie są sprawdzane,
bo u osób bez niepełnosprawności to pole też jest puste. Definicja 2039 (Ostrzeżenie, także 8h dla stopnia
lekkiego) zostaje bez zmian, zablokowana; przy jej odblokowaniu oba komunikaty pojawią się razem.
Wywołanie w definicji:
`var pracownik = dzien.Kalendarz.Pracownik; if (pracownik == null) return null;
return K.A1BrakZgodyNaPrace8h(new KalkulatorPlanu(pracownik), dzien.Data);`.
Podpięcia wstawione SQL-em — widoczne po restarcie serwera enova.

**Test na żywo (2026-10-15, baza Al): 9 z 10 scenariuszy pozytywnie** — arkusz „Brak zgody 8h”
w `Scenariusze testowe weryfikatorow czasu pracy.xlsx`, pracownicy testowi NP1–NP9
(`ImportyXML/Weryfikator brak zgody 8h - pracownicy i plan.xml`, orzeczenia dodane w programie).
Działa: umiarkowany/znaczny bez zgody > 7:00 → Błąd, granica 7:00, zgoda 8h, stopień lekki, brak
orzeczenia, orzeczenie ZUS (odpowiadający stopień), kalendarz równoważny.

**Błąd (scenariusz 8):** w enova 2610 `PracHistoria.StopienNiepelnosp` i `PracHistoria.PFRON` są
przestarzałymi nakładkami (do usunięcia w 2710) na tabele `OrzeczeniaPFRON` / `OrzeczeniaZUS`
i zwracają **jedno** orzeczenie obowiązujące w okresie zapisu historii — to z najpóźniejszą datą
„Obowiązuje od”. Gdy stopień zmienia się w trakcie zapisu historii (np. umiarkowany do 15.11, lekki
od 16.11), dla dni sprzed zmiany weryfikator widzi orzeczenie późniejsze, którego okres nie obejmuje
dnia → stopień „Brak” → brak błędu. Dotyczy też definicji 2039. Poprawka: szukać orzeczenia, którego
okres (`OkresExt`) obejmuje dany dzień, w `pracownik.OrzeczeniaPFRON` i `pracownik.OrzeczeniaZUS`
(nie poprawione — do decyzji).

**Poprawione (2026-10-19):** w obu metodach (2039 i 2069) stopień liczony metodą enova
`pracownik.StopienNiepelnosprawnosciNaDzien(data).Stopien`. Metoda wybiera orzeczenie PFRON
i ZUS obowiązujące w danym dniu (`OkresExtObow`, a przy kilku kandydatach to, którego własny
okres obejmuje dzień) i zwraca wyższy stopień. Zgoda na 8h nadal z `pracownik[data].Etat`.
Kompilacja całej klasy na bibliotekach 2610 (.NET 10) bez błędów, kod wgrany do `CodeFiles` 13,
stamp `RuntimeProjects` 29 podbity. **Potwierdzone na żywo (2026-10-19, po restarcie serwera):**
NP8 13.11 = 8:00 → błąd 2069 ze stopniem „Umiarkowany”; 16.11 = 8:00 (lekki) → bez błędu 2069.
Scenariusz 8 w arkuszu „Brak zgody 8h” zmieniony na pozytywny (10/10).

**Uwaga z testu:** weryfikator normy okresu rozliczeniowego (definicja 37) liczy normę ogólną także
dla osób ze stopniem umiarkowanym/znacznym bez zgody (norma 7h/dobę, 35h/tydzień) — dla nich oba
weryfikatory się wykluczają (niedobór normy okresu każe wydłużyć dzień do 8:00, 2069 blokuje > 7:00).

**Decyzja (2026-10-19): bez zmian w kodzie.** Definicja 37 liczy normę metodą enova
`KalkulatorKodeksowyPracownika.Norma` = norma dobowa kalendarza z etatu × dni robocze × wymiar, a maksimum
dnia z „Nadgodziny – wartość dobowa” kalendarza. Klient ma kalendarze „8:15” z normą 7:00 i limitem
nadgodzin 7:00 / 35:00 (ID 1070, 1072, 1074, 1077). Na takim kalendarzu norma okresu wynosi 7h × dni
i konflikt z 2069 nie występuje. Zasada: osoba ze stopniem umiarkowanym/znacznym bez zgody na pracę 8h
ma kalendarz 8:15. Konflikt w teście wynikał z danych testowych (NP1–NP9 na kalendarzu 47, norma 8h).
Do wyjaśnienia z klientem: kalendarz 1080 „Zadaniowy system_1_8:15_N” ma normę dobową 8:00 przy limicie
nadgodzin 7:00. Na kalendarzach równoważnych 8:15 podpowiedź 37 może proponować wydłużenie dnia
do 12h (blokowane przez 2069) — tylko treść podpowiedzi, niezmieniane.

### 8a. Zgody osoby z niepełnosprawnością: nadgodziny i pora nocna (2026-10-19)

Trzy nowe weryfikatory, podpięte do tych samych 14 kalendarzy co 2069 (45, 47, 1070–1081).
Dotyczą **każdego** stopnia niepełnosprawności (orzeczenie PFRON/ZUS obowiązujące w dniu,
`pracownik.StopienNiepelnosprawnosciNaDzien`) — enova udostępnia pola zgód na etacie przy każdym
stopniu (`Etat.IsReadOnlyPracownikNiepelnosprawnyZgoda...` = brak orzeczenia o stopniu).
Zgody czytane z zapisu historii obowiązującego w dniu (`pracownik[data].Etat`).

| Def. | Nazwa | Rodzaj | Poziom | Metoda | Reguła |
|---|---|---|---|---|---|
| 2070 | A1_Brak zgody na nadgodziny | Dzień pracy | Ostrzeżenie | `A1BrakZgodyNaNadgodziny(dzien)` | brak `PracownikNiepelnosprawnyZgodaNaPraceNadgodziny`; czas pracy dnia (strefy `Wchodzi` i `Zwieksza`) > większa z wartości: plan dnia, `Etat.NormaDobowa`; w dniu wolnym wg planu — każda praca |
| 2071 | A1_Brak zgody na pracę w nocy (plan) | Dzień planu | **Błąd** | `A1BrakZgodyNaPraceWNocyPlan(kp, data)` | brak `PracownikNiepelnosprawnyZgodaNaPraceWPorzeNocnej`; strefy planu dnia przecinają porę nocną |
| 2072 | A1_Brak zgody na pracę w nocy (czas) | Dzień pracy | Ostrzeżenie | `A1BrakZgodyNaPraceWNocyCzasPracy(dzien)` | jw. dla stref czasu pracy |

Pora nocna: metoda enova `KalkulatorPracyBase.NocOkres(PracHistoria)` (Od/Do z `Kalendarz.Nocne`
etatu albo z konfiguracji, z obsługą przejścia przez północ), przecięta ze strefami wliczanymi do
czasu pracy; doliczana jawna strefa „Praca w godzinach nocnych” (`DefinicjaStrefy.Nocne`). Nie
korzysta z `KalkulatorPracy.Nocne`, bo ten przy `Nocne.Rozliczaj = false` widzi tylko jawną strefę.
Wspólny helper `A1CzasWPorzeNocnej` (prywatny).

Wgrane SQL-em w jednej transakcji (CodeFiles 13, DefWeryfKalend 2070–2072, 42 podpięcia WeryfKalend,
stamp RuntimeProjects 29). Kompilacja całej klasy na 2610 (.NET 10) bez błędów. Podpięcia widoczne
po restarcie serwera enova. Test na żywo: do zrobienia.

### 8b. Norma tygodniowa 35h / 40h osoby z niepełnosprawnością (2026-10-19)

Dwie nowe definicje podpięte do tych samych 14 kalendarzy co 2069 (45, 47, 1070–1081) oraz do
kalendarza testowego 1086. Poziom (Błąd / Ostrzeżenie) to ustawienie podpięcia na zakładce
„Weryfikatory kalendarza”, więc zmienia się go bez zmian w kodzie.

| Def. | Nazwa | Rodzaj | Poziom | Metoda |
|---|---|---|---|---|
| 3070 | A1_Norma tygodniowa niepełnosprawnych | Plan pracy | **Błąd** | `A1NormaTygodniowaNiepelnosprawnych(kp, args.Miesiąc)` |
| 3071 | A1_Doba niepełnosprawnych (równoważny) | Dzień planu | Ostrzeżenie | `A1DobaNiepelnosprawnegoWRownowaznym(kp, data)` |

**Norma osoby (d = dobowa, W = 5 × d):** umiarkowany/znaczny bez zgody na pracę 8h (pole na etacie)
→ 7h/35h; lekki oraz umiarkowany/znaczny ze zgodą 8h → 8h/40h. Stopień z orzeczeń PFRON/ZUS
obowiązujących w danym dniu (`pracownik.StopienNiepelnosprawnosciNaDzien`), zgoda z zapisu historii
obowiązującego w dniu. Brak orzeczenia → nic nie sprawdza.

**Tygodnie:** 7 kolejnych dni od pierwszego dnia okresu rozliczeniowego (`pracownik.WyliczOkresRoliczeniowyNadgodzin`,
art. 128 § 3 pkt 1 KP). Tak samo liczy standardowy weryfikator odpoczynku tygodniowego enova. Tygodnie
nie zaczynają się od nowa na granicy miesięcy, a ostatni tydzień okresu może być niepełny.

**Godziny planowane:** `KalkulatorPlanu(pracownik).Norma(...)`, czyli strefy planu dnia z bazy razem
ze zmianami z bieżącej sesji. Kalkulator planu nie odejmuje nieobecności (rozlicza je kalkulator pracy),
tak samo jak standardowa „Norma w okresie rozliczeniowym”.

**Kalendarz podstawowy** (`Kalendarz.RownowaznyCzasPracy = false`, także zadaniowy): każdy tydzień ≤ W,
liczone sztywno, a niepełny tydzień na końcu okresu też ≤ W. Dodatkowo cały okres ≤ wymiar.
**Kalendarz równoważny:** tylko cały okres ≤ wymiar (norma średnia).

**Wymiar** to metoda enova `KalkulatorKodeksowy.Norma` (art. 130 KP: 40h × pełne tygodnie + 8h × dni
pon–pt niepełnego tygodnia − 8h × święta poza niedzielą), policzona dla każdego tygodnia okresu
i przeskalowana przez d/8. Przykład: X 2026 to 4 × 35 + 2 × 7 = 154h (W = 35) albo 176h (W = 40).
IX–XI 2026 to 13 × 35 − 7 (11.11) = 448h. Dni poza zatrudnieniem są pomijane (jak w 37). Wymiar etatu
nie zmniejsza limitu.

**Zmiana orzeczenia w trakcie okresu:** norma liczona dla każdego dnia. Tydzień, w którym normy dni się
różnią, dostaje normę dobową równą średniej z dni pon–pt, np. pon–wt po 7h i śr–pt po 8h daje W = 38h.
Zmiana w weekend nie obniża limitu tygodnia. Dzień bez orzeczenia liczony jest normą ogólną 8h/40h,
a tydzień bez żadnego dnia z orzeczeniem nie jest sprawdzany.

**Zakres:** sprawdzane są okresy rozliczeniowe obejmujące zapisywany miesiąc (`args.Miesiąc`), a tygodnie
tylko te, które ten miesiąc przecinają. Komunikat jest jeden, dla pierwszego przekroczenia: najpierw tydzień,
potem wymiar okresu.

**3071 (Ostrzeżenie):** na kalendarzu równoważnym dzień planu dłuższy niż d. Przy umiarkowanym/znacznym
bez zgody i dniu powyżej 7h odzywa się też 2069 (Błąd).

**Ograniczenia:** weryfikatory kalendarza enova uruchamiają się przy zapisie planu, a nie przy zmianie
kalendarza na etacie ani przy zmianie orzeczenia. Po takiej zmianie kontrola zadziała dopiero przy
najbliższej edycji planu w danym miesiącu.

Wgrane SQL-em w jednej transakcji (CodeFiles 13, DefWeryfKalend 3070–3071, po 14 podpięć, stamp
RuntimeProjects 29). Kompilacja całej klasy na 2610 (.NET 10) bez błędów. Dane testowe: kalendarz
testowy 1086 „TEST Podstawowy_3_8:15_N” (kopia 1072, okres 3 mies. od 1.03.2024) dla NP3,
NP4 przeniesiony na 1076, a dni planu w `ImportyXML/Weryfikator norma tygodniowa niepelnosprawnych - plan.xml`.

**Test na żywo (2026-10-19, baza Al, po restarcie serwera): wszystkie scenariusze pozytywne** (arkusz
„Norma tygodniowa 35-40h” w `Scenariusze testowe weryfikatorow czasu pracy.xlsx`): podstawowy 35h OK /
tydzień 36h Błąd, tydzień 29.09–05.10 na przełomie miesięcy w okresie 3-mies., tydzień czw–śr (nie pon–nd),
równoważny tydzień 42h OK / okres 449 > 448 Błąd (11.11 odjęte), lekki 41 > 40 Błąd, zgoda 8h 177 > 176 Błąd
+ ostrzeżenie 3071, brak orzeczenia bez komunikatu, zmiana orzeczenia 16.11 (tydzień 15–21.11 40h OK, 1–7.11 36h Błąd).
Decyzja użytkownika 2026-10-19: przy kalendarzu podstawowym **Błąd**.

Uwagi z testu: kalendarze klienta „_N” nie generują planu z definicji kalendarza (dni bez wpisu = 0h),
więc dane testowe mają jawny plan każdego dnia. Kalendarz 1086 skopiowany SQL-em z niepustym polem `Algorytm`
dawał NRE w `Kalendarz.GetTools()` przy zapisie dnia planu; pole wyczyszczone (algorytm zawierał same `return null`).
Opis dla klienta: dwie nowe sekcje w `Instrukcja_Weryfikatory.docx`.

### 9. Wydajność

Kontrole rodzaju Plan pracy uruchamiają się przy zapisie **każdego** dnia planu i liczą cały
miesiąc (a 48h/35h — cały okres rozliczeniowy). Algorytm używał jednego `KalkulatorPlanu`.
Teraz każda z 8 definicji tworzy własny. Przy dużych grafikach (zapis wielu dni naraz) może to
być odczuwalne. W razie problemu: połączyć kontrole dzienne w jedną definicję.

### 10. Wspólny plik w Dodatkowym kodzie do kompilacji

Wszystkie definicje A1 wołają jedną klasę. Błąd kompilacji w tym pliku wyłącza **wszystkie**
weryfikatory A1 naraz. Repo trzyma tylko fragmenty tej klasy (np. `BlokadaZmianyGodzinDAK`
i ten plik), a nie całą klasę z bazy — przy wklejaniu łatwo coś zgubić lub zdublować metodę.
Wzorcem jest zawartość bazy (`CodeFiles`).

## Inwentaryzacja 2026-10-08: stary algorytm kalendarza a obecne weryfikatory (baza testowa)

Źródło „przed”: archiwa algorytmu kalendarzy 45 i 47 (`SprawdźDzieńPlanu` i `SprawdźPlan`;
`SprawdźDzieńKalendarza` i `SprawdźKalendarz` zwracały `null`). Kalendarz 1 „Standard” nie miał algorytmu.
W starym algorytmie wszystkie kontrole były ostrzeżeniami, poza normą okresu na kal. 47 (błąd).

| Kontrola w starym algorytmie | Kal. 45 (przed) | Kal. 47 (przed) | Obecnie kal. 45 | Obecnie kal. 47 |
|---|---|---|---|---|
| Odpoczynek dobowy 11h (`KontrolaPrzerwy11h`, przy zapisie planu) | Ostrz. | Ostrz. | **brak** | std. Odpoczynek dobowy (1 — dzień planu, 3 — dzień pracy), Ostrz. |
| Doba pracownicza (`KontrolaDobaPracownicza`, przy zapisie planu) | Ostrz. | Ostrz. | **brak** | **brak** (std. 11h tego nie obejmuje) |
| Praca do 12h (`ControlWork12h`) | Ostrz. | Ostrz. | 2048 A1_Norma dobowa (12h — równoważny), Ostrz. | 2048 A1_Norma dobowa (8h), **Błąd** |
| Norma niepełnosprawnych (`ControlWorkHoursForDisabled`) | liczona, niezwracana | Ostrz. | — | 2039 celowo zablokowana i odpięta |
| Norma średniotygodniowa 48h | Ostrz. | Ostrz. | 2044, Ostrz. | 2044, **Błąd** |
| Odpoczynek tygodniowy 35h (`KontrolaPrzerwa35h`) | Ostrz. | Ostrz. | **brak** | std. Odpoczynek tygodniowy (17 — plan), Ostrz. |
| Dni wolne i święta w miesiącu | wyłączona | Ostrz. | (1036 A1_Ilość dni wolnych w okresie, Błąd) | 1036, Błąd — inna reguła (okres zamiast miesiąca) |
| Plan ponad normę okresu (`ControlToManyWorkDaysInPeriod`) | zwracała zawsze `null` | **Błąd** | 37 A1_Norma w okresie rozl., Błąd | 37, Błąd |
| Norma 8h (`KontrolaNormy8h`) | zakomentowana | zakomentowana | — | — |

Poza starym algorytmem (już wcześniej lub nowe): 35 A1_Okres zatrudnienia, 37, 1036 (kal. 1/45/47, Błąd),
2049 A1_Praca w normie nie ponad plan (45/47, Błąd), 2051 A1_Magazyn nadgodzin - umowy cywilne (1/45/47, Błąd).

**Luki:** kal. 45 — brak 11h, 35h i doby pracowniczej; kal. 47 — brak doby pracowniczej.
**Zaostrzenia na kal. 47:** 2048 i 2044 jako Błąd (wcześniej ostrzeżenia), a limit dobowy 8h zamiast 12h.
