# Rozwiązanie odprawa emerytalno-rentowa

## Cel biznesowy

Automatyczne naliczenie na **planowanej liście płac** (moduł rezerw bilansowych
enova — "Definicje planowanych list płac", tabela `DefPlanListPlac`) pozycji
odwracającej ("rozwiązanie") wcześniej rozliczoną na **liście głównej** (realnej
liście płac) odprawę emerytalno-rentową (element systemowy **"Odprawa
emerytalna"**, `DefElementow` ID 111 w bazach testowych, GUID
`C955AB11-B500-4D90-933D-0A0FB2EE7BE8` — stały GUID standardowego elementu
kreatorowego, identyczny w bazach Claude i Al, więc traktowany jako przenośny).

Reguła (potwierdzona z użytkownikiem):
- Wyzwalacz: pracownik ma na liście głównej niezerowo rozliczony element
  "Odprawa emerytalna".
- Wynik: na liście planowanej (rezerwowej) powstaje pozycja "Rozwiązanie
  odprawa emerytalno-rentowa" o **tej samej kwocie ze znakiem minus**.
- "Lista planowana" to **moduł rezerw** enova (naliczanie rezerw
  urlopowych/na odprawy do księgowości), NIE zwykła "druga" lista płac
  uruchamiana na próbę — użytkownik to jednoznacznie potwierdził.

## Mechanizm enova (ustalony przez dekompilację, nie przez dokumentację)

Ten mechanizm nie jest opisany w żadnym lokalnie dostępnym skillu ani w bazach
testowych (obie bazy sandboxowe — Claude i Al — mają pustą tabelę
`DefPlanListPlac`/`PlanListyPlac`, więc nie było gotowego przykładu do
naśladowania). Poniższe ustalono dekompilując (ilspycmd) assembly serwera:
`Soneta.KadryPlace.dll` (klasy `Soneta.Place.DefinicjaPlanowanejListyPłac`,
`Soneta.Place.PlanowanaWypłata`, `Soneta.Place.PlanowanyElementWypłaty`,
`Soneta.Place.SourceFilterArgs`) i `Soneta.Ksiega.dll` (klasa bazowa
`Soneta.Ksiega.Płace.AlgorytmDefinicjiPlanowanejListyPłac`), wersja serwera
2512.5.6 (`C:\enovaServer\2512.5.6\Soneta.Products.Server.Standard`).

Ustalenia:

1. **`DefPlanListPlac`** ("Definicja planowanej listy płac") to SZABLON
   generowania rezerwy dla JEDNEGO elementu-źródła (pole `Element`,
   ograniczone przez `GetListElement()` do elementów `RodzajZrodla=Dodatek`).
   Dla naszego przypadku `Element` = **"Odprawa emerytalna"** (istniejący
   element #111) — to on jest źródłem skanowanym przez silnik generowania
   planu, NIE nowy element "Rozwiązanie...".
2. Generowanie planu wywołuje (dla każdej kwalifikującej się realnej
   `Wyplata`) `IAlgorytmDefinicji.KopiujWypłatę(dest, src)`, gdzie `src` to
   już ZNALEZIONA przez silnik enova realna wypłata z listy głównej — algorytm
   NIE musi sam przeszukiwać `WypElementy`/okresów, dostaje gotowy obiekt.
3. `KopiujWypłatę` (niewirtualna, nie nadpisywać) woła po kolei trzy metody
   `protected virtual`, które WOLNO nadpisać: `KopiujNagłówek`,
   `KopiujElementy`, `KopiujOpis`.
4. Domyślna implementacja `KopiujElementy` kopiuje WSZYSTKIE elementy wypłaty
   1:1 (ta sama `Definicja`, ta sama wartość, bez zmiany znaku) do
   `PlanElementyWyp`. Żeby dostać:
   - tylko element "Odprawa emerytalna" (nie całą wypłatę),
   - pod INNĄ definicją ("Rozwiązanie odprawa emerytalno-rentowa"),
   - ze znakiem odwróconym (rezerwa "rozwiązana"),

   trzeba nadpisać `KopiujElementy` + `KopiujElement` (patrz kod niżej).
5. `Podatki.KopiujNaMinus(from)` neguje WSZYSTKIE pola składkowo-podatkowe.
   Bazowy kod woła ją DWA razy (`dest.Podatki.KopiujNaMinus(src.Podatki)`,
   potem `dest.Podatki.KopiujNaMinus(dest.Podatki)`) — podwójna negacja daje
   zwykłą kopię bez zmiany znaku. W naszym override wołamy ją RAZ, żeby
   faktycznie odwrócić znak (rezerwa jest "rozwiązywana").
6. Nagłówek (`PlanowanaWypłata.Wartosc`) trzeba przeliczyć samodzielnie —
   domyślnie `KopiujNagłówek` kopiuje `src.Wartosc` (sumę CAŁEJ wypłaty), a
   nam zależy tylko na sumie skopiowanych (odwróconych) pozycji.

## PRZEBUDOWA 2026-09-22 (druga część dnia): właściwy wzorzec od klienta

Po dwóch poprawkach błędów kompilacji (patrz sekcje niżej) mechanizm w końcu
się kompilował i uruchamiał bez błędu — ale **nic się nie naliczało**.
Użytkownik pokazał wtedy kod działającego u niego analogicznego elementu
"Rozwiązanie Rezerwy Urlopowej", co ujawniło, że cała moja architektura
(customowy `KopiujNagłowek`/`KopiujElementy`/`KopiujElement` w
`DefinicjaPlanowanejListyPłac` odwracający znak) była **złą drogą** — działa,
ale nie robi tego co trzeba, bo filtr/kopiowanie nie trafiał w oczekiwany
sposób w praktyce (dokładna przyczyna niezdiagnozowana — przebudowano zamiast
debugować, bo pojawił się sprawdzony wzorzec).

**Właściwy, potwierdzony u klienta wzorzec** jest dużo prostszy:
- Cała logika "rozwiązania" siedzi w zwykłym **Dodatku automatycznym**
  (`RodzajZrodla=DodatekAutomatyczny`, klasa `WypElementDodatekAutomatyczny`),
  DOKŁADNIE tak samo jak każdy inny element wynagrodzenia — zwykłe
  `_Param`/`_Wylicz`, żadnej specjalnej integracji z `DefPlanListPlac`.
- `_Param` sumuje `Element.Elementy[Element.Okres]` (elementy pracownika w
  danym okresie, NIEZALEŻNIE z której listy płac — czyli widzi też historyczny,
  już zatwierdzony element z listy głównej sprzed miesięcy) po
  `e.Definicja.Nazwa.Contains("Odprawa emerytalna")`, i wstawia sumę do
  `Składnik.Podstawa1` **bez zmiany znaku** (wzorzec klienta też nie neguje —
  najwyraźniej efekt "rozwiązania" w księgowości zależy od schematu
  księgowego/konta, a nie od znaku kwoty w WypElement).
- `DefinicjaPlanowanejListyPłac` zostaje z DOMYŚLNYM, niezmienionym
  algorytmem enova (sam szablon `FiltrNaliczania` zwracający `true`) — jej
  jedyna rola to być "wyzwalaczem": `Element` = "Odprawa emerytalna" każe
  silnikowi przeliczyć pracownika dla tego dodatku w podanym okresie, a przy
  okazji (jak każdy Dodatek automatyczny) naliczy się też "Rozwiązanie...",
  które domyślny (nienadpisany) `KopiujElementy` skopiuje na plan tak jak
  jest.

**Ważna konsekwencja dla klasyfikacji podatkowej:** ponieważ to zwykły Dodatek
automatyczny, nalicza się przy KAŻDYM przeliczeniu pracownika obejmującym ten
okres — nie tylko na planie, ale też na REALNEJ liście płac (np. w miesiącu,
w którym faktycznie wypłacana jest odprawa, skoro wtedy "Odprawa emerytalna"
też jest w `Element.Elementy` tego okresu). Dlatego zmieniono klasyfikację na
`NieNaliczać` dla ZUS i PIT (poprzednia wersja miała PIT wg skali, co przy tym
mechanizmie oznaczałoby REALNE podwójne opodatkowanie tej samej kwoty).

**Scenariusz testowy od użytkownika:** pracownik zwolniony 01/2026, odprawa
wypłacona na liście głównej w 08/2026 — licząc planowaną listę płac z
okresem 08/2026 powinno pojawić się "Rozwiązanie odprawa emerytalno-rentowa"
o tej samej (dodatniej) kwocie.

## Czwarta iteracja tego samego dnia: brakujący Priorytet algorytmu

Po przebudowie na Dodatek automatyczny (sekcja wyżej) kod kompilował się i
uruchamiał bez błędu, ale **nadal nic się nie liczyło** nawet dla dokładnie
opisanego scenariusza (odprawa 08/2026, plan liczony na 08/2026). Przyczyna,
ustalona dekompilacją `Soneta.KadryPlace.dll` (`WypElement.ItElementy`,
klasa obsługująca indekser `Element.Elementy[Okres]` użyty w `_Param`):

```csharp
int priorytet = element.Definicja.Algorytm.Priorytet;
foreach (WypElement item in subTable)
    if (item.Definicja.Algorytm.Priorytet < priorytet && !item.RozliczenieStorna)
        arrayList.Add(item);
```

**`Element.Elementy[Okres]` zwraca WYŁĄCZNIE elementy o priorytecie NIŻSZYM
niż priorytet elementu, który go odpytuje** — to mechanizm kolejności
naliczania w silniku enova (element o wyższym priorytecie liczy się później i
"widzi" wyniki elementów o niższym priorytecie, nigdy odwrotnie). "Odprawa
emerytalna" ma `AlgorytmPriorytet=98` (sprawdzone w bazie Claude), a nowo
utworzony element domyślnie dostał `AlgorytmPriorytet=0` — więc warunek
`98 < 0` był fałszywy i pętla `foreach` w `_Param` nigdy nie widziała
"Odprawy emerytalnej", niezależnie od tego, czy dana wypłata faktycznie ją
zawierała.

**Poprawka:** dodano `<Algorytm><Priorytet>200</Priorytet></Algorytm>` do
definicji "Rozwiązanie odprawa emerytalno-rentowa" — wartość dobrana
analogicznie do innych automatycznych dodatków w bazie, które też sumują już
policzone elementy (np. "Przychód od skł. pracod. PPK (etat, W)" też ma
`Priorytet=200`; dla porównania "Potrącenie OPP", liczone jako ostatnie, ma
999). Zweryfikowano zapis w bazie Claude (`dbmgr importxml`, exit 0).

**Wniosek na przyszłość:** przy KAŻDYM dodatku automatycznym, który w swoim
`_Param` odczytuje `Element.Elementy[Okres]` żeby zsumować/odczytać wartość
INNEGO elementu — trzeba pamiętać o ustawieniu `Algorytm.Priorytet` na
wartość WYŻSZĄ niż priorytet tego innego (źródłowego) elementu, inaczej kod
skompiluje się i uruchomi bez żadnego błędu, ale pętla zawsze będzie pusta.
To nie jest widoczne ani przy kompilacji, ani przy pierwszym spojrzeniu na
kod — ujawnia się tylko przy realnym teście z danymi.

Plik XML w całości przepisany na ten wzorzec + poprawka priorytetu.

## Piąta iteracja tego samego dnia: Element wskazywał na złą definicję

Po poprawce priorytetu użytkownik zgłosił: nadal się nie liczy, tym razem BEZ
komunikatu o błędzie. Dekompilacja `NaliczanieWypłat.AddŹródło` (metoda, przez
którą przechodzi KAŻDY kandydat na element wypłaty — także Dodatki
automatyczne, patrz `WypElementDodatekAutomatyczny.Nalicz.GenerujElement`,
linia `Naliczanie.AddŹródło(wypElementDodatekAutomatyczny, item3)`) ujawniła
twardy warunek wykluczający:

```csharp
if ((dodatek != null && element.Definicja != dodatek) || ...)
    return; // element pomijany całkowicie
```

`dodatek` to pole ustawiane przez `NaliczaniePlanowanychListPłac.Nalicz`
wprost z `DefinicjaPlanowanejListyPłac.Element`
(`pracownikParams.Dodatek = definicja.Element` → `nw.DodajDodatek(...)`).
Skoro `Element` = "Odprawa emerytalna", to warunek `element.Definicja !=
dodatek` był PRAWDZIWY dla "Rozwiązanie odprawa emerytalno-rentowa" (inna
definicja) — **element był całkowicie wykluczany z przeliczenia, zanim
dotarł do własnego `_Param`**, niezależnie od priorytetu czy czegokolwiek
innego w jego kodzie. To była prawdziwa przyczyna, głębsza niż priorytet z
poprzedniej sekcji (priorytet i tak trzeba było poprawić, ale sam nie
wystarczał).

**Poprawka:** `DefinicjaPlanowanejListyPłac.Element` zmieniony z "Odprawa
emerytalna" na WŁASNY element "Rozwiązanie odprawa emerytalno-rentowa" (ID
276 w bazie Claude). Wtedy `element.Definicja == dodatek` jest prawdziwe dla
"Rozwiązanie..." (bo to ONO jest teraz scopingiem), więc DOCIERA do swojego
`_Param`. Ten z kolei i tak czyta "Odprawa emerytalna" NIEZALEŻNIE od tego
scopingu — `Element.Elementy[Okres]` to osobne zapytanie do historycznej
tabeli `WypElementy` (`WypElement.ItElementy`), niezwiązane z polem `dodatek`.

`GetListElement()` (podpowiedź w GUI) filtruje kandydatów do pola `Element`
po `RodzajZrodla=Dodatek`, więc "Rozwiązanie..." (RodzajZrodla=DodatekAutomatyczny)
formalnie nie pasuje do tej podpowiedzi — ale nie znaleziono żadnego
walidatora wymuszającego to przy zapisie. **NIEZWERYFIKOWANE, czy edycja tej
definicji przez GUI będzie się nadal dawała normalnie otworzyć/zapisać**
(pole Element może pokazywać się jako puste/nieprawidłowe, skoro wskazywany
element nie jest na liście podpowiedzi).

**Techniczna uwaga:** `dbmgr importxml` przy tej poprawce zaczął się
konsekwentnie zawieszać na `TimeoutDatabaseSqlException` (kilka prób pod
rząd) — okazało się to przejściowym obciążeniem/blokadą serwera SQL
(potwierdzone: `dbmgr compile Claude` chwilę później przeszło normalnie, z
komunikatem "Waiting for the end of administrative operation..."), nie
błędem samej zmiany. Żeby nie czekać na ustąpienie obciążenia, pole
`DefPlanListPlac.Element` zaktualizowano wprost przez SQL
(`UPDATE DefPlanListPlac SET Element = 276 WHERE ID = 3`) — baza i plik XML
są teraz zgodne.

Nadal NIEZWERYFIKOWANE żywym testem w tej (piątej) wersji.

## Ważne uzupełnienie (2026-09-22, po dalszej dekompilacji): jak NAPRAWDĘ działa generowanie planu

Pierwsza wersja tego dokumentu zakładała, że `KopiujWypłatę` dostaje "gotową,
już zatwierdzoną" realną wypłatę z listy głównej. To nieprecyzyjne. Klasa
faktycznie wywołująca algorytm to `Soneta.Place.NaliczaniePlanowanychListPłac`
(`NaliczPracownika` → `Nalicz`):

1. Silnik ustawia `pracownikParams.Dodatek = definicja.Element` (czyli
   "Odprawa emerytalna") i woła `nw.DodajDodatek(...)` — to standardowy
   mechanizm **naliczania seryjnego scopowanego do jednego dodatku** (ten sam
   co przy zwykłym przeliczaniu wybranego dodatku dla grupy pracowników).
2. **PRZED** przeliczeniem silnik kasuje niezatwierdzone wypłaty pracownika
   danego `TypWypłaty` (`wyp.Delete()`), chyba że wywołanie ma
   `UwzgledniajNieZatwierdzoneListyPlac=true` (wtedy tylko je zatwierdza).
   **To wbudowane zachowanie enova, nie coś wprowadzonego tym plikiem — ale
   ważne ryzyko operacyjne, o którym trzeba poinformować klienta przed
   użyciem na produkcji: generowanie planu może skasować inne, niezwiązane
   niezatwierdzone wypłaty tego pracownika.**
3. Woła `NaliczanieSeryjne.Pracownika(pracownikParams).Nalicz()` — to
   PRAWDZIWY silnik liczący wypłaty (ten sam co przy zwykłym przeliczaniu
   listy płac), NIE odczyt historii. Wynik (`Wyplata`) to świeże przeliczenie
   dla okresu/parametrów podanych przez operatora przy generowaniu planu, a
   NIE odczytana z bazy już zatwierdzona wartość.
4. Dopiero ten świeżo przeliczony wynik trafia jako `src` do
   `algorytmDefinicji.KopiujWypłatę(planowanaWypłata, item)`.

**Konsekwencja dla naszej logiki (raczej pozytywna):** ponieważ "Odprawa
emerytalna" jest dodatkiem jednorazowym przypisanym przez `DodHistoria` do
konkretnego okresu, przeliczenie scopowane do tego dodatku dla okresu spoza
jej `DodHistoria.Okres` powinno naturalnie zwrócić zero — silnik SAM pilnuje
"czy to należy się w tym okresie", więc nasz kod (filtr po nazwie +
`Wartosc != 0`) nie musi ręcznie przeszukiwać historii realnych list. Warunek
użytkownika "jeżeli na liście głównej ma rozliczoną odprawę" powinien być
więc spełniony automatycznie, POD WARUNKIEM że operator generuje plan dla
tego samego okresu, w którym odprawa faktycznie została/zostanie naliczona —
to wymaga potwierdzenia w GUI, nie jest to już tylko teoria z dekompilacji
klas kopiujących, tylko z całego łańcucha wywołań.

## Co jest POTWIERDZONE, a co NIEZWERYFIKOWANE

**Aktualizacja 2026-09-22 (test w GUI przez użytkownika, wersja serwera
2512.5.6):** próba realnego "Nalicz" na planowanej liście płac ujawniła błąd
kompilacji, którego `dbmgr importxml`/`dbmgr compile` NIE wykryły:

```
DefPlanListPlac\Rozwiązanie odpr.emeryt.\Rozwiązanie odpr.emeryt..cs(73,63):
error CS0104: 'Element „Wyplata” to niejednoznaczne odwołanie między
elementem „Soneta.Kasa.Wyplata” i „Soneta.Place.Wyplata”
```

Stos wywołań z błędu potwierdza WPROST ścieżkę ustaloną wcześniej
dekompilacją: `NaliczaniePlanowanychListPłacWorker.Nalicz()` →
`NaliczaniePlanowanychListPłac.NaliczPracownika` →
`DefPlanListPlac.PobierzAlgorytm(definicja)` →
`Session._AssemblyCache.GetType(...)` — czyli algorytm `DefPlanListPlac`
kompiluje się DOPIERO na żądanie, przy faktycznym "Nalicz" w GUI, a NIE przy
ogólnej kompilacji bazy (`dbmgr importxml`/`dbmgr compile` przechodzą bez
błędu mimo tego buga — **nie są wystarczającym testem dla tego mechanizmu**).

Przyczyna: sygnatury `KopiujNagłówek`/`KopiujElementy` używały samego
`Wyplata`, a w kontekście kompilacji tej definicji w zasięgu są jednocześnie
`Soneta.Kasa.Wyplata` i `Soneta.Place.Wyplata` — poprawka: pełna nazwa
`Soneta.Place.Wyplata` w obu sygnaturach.

**Druga iteracja tego samego dnia:** po poprawce powyżej pojawił się KOLEJNY
błąd, tym razem `CS0115: nie znaleziono odpowiedniej metody do
przesłonięcia` dla `KopiujNagłówek`. Przyczyna okazała się być głębsza:
**metoda bazowa w kodzie enova ma literówkę — nazywa się `KopiujNagłowek`
(bez „ó”), nie `KopiujNagłówek`** (poprawna polska pisownia to "nagłówek", ale
tak akurat nie napisali w Soneta). Ja tę literówkę "naprawiłem" nieświadomie,
bo pierwsza dekompilacja tej klasy (`Soneta.Ksiega.Płace.AlgorytmDefinicjiPlanowanejListyPłac`)
była zrobiona przez bezpośredni wydruk `ilspycmd -t` na konsolę Bash, która
**zniekształca polskie znaki diakrytyczne** (wychodzą jako „�") — musiałem
je wtedy rekonstruować "na oko" i pomyliłem się przy tym jednym słowie.

**Wniosek na przyszłość (ważne dla kolejnych podobnych zadań):** `ilspycmd -t
Typ plik.dll` wypisywany bezpośrednio na konsolę Bash NIE jest wiarygodnym
źródłem nazw z polskimi znakami — zawsze dekompilować do plików
(`ilspycmd -p plik.dll -o katalog`) i czytać przez narzędzie Read (poprawne
UTF-8), nigdy nie ufać rekonstrukcji nazw z zniekształconego wydruku
konsoli. Po tym zdarzeniu wszystkie użyte w tym pliku nazwy
(`SourceFilterArgs`, `PlanowanyElementWypłaty`, `Podatki.KopiujNaMinus`,
`DefElementow.WgNazwy`, `SourceFilterDelegate`, `KopiujElementy`,
`KopiujElement`) zostały ręcznie zweryfikowane znak-po-znaku względem
poprawnie zdekompilowanych plików (`Soneta.KadryPlace.dll` → `/tmp/kadryplace_src`,
`Soneta.Ksiega.dll` → `/tmp/ksiega_src`) — zgadzają się.

Zaimportowano obie poprawki i zweryfikowano `dbmgr importxml`/`dbmgr compile`
(exit 0 za każdym razem) — ale to, jak ustalono już wcześniej, NIE gwarantuje
braku kolejnych błędów w tej samej, kompilowanej leniwie ścieżce (uruchamianej
dopiero przy realnym "Nalicz" w GUI, czego `dbmgr` nie wywołuje). Mimo
dokładnej weryfikacji nazw — wciąż nie było żywego testu wykonania kodu.

**Potwierdzone próbnym importem na bazie Claude** (`dbmgr importxml`,
2026-09-22, exit code 0):
- Oba rekordy (`DefinicjaElementu` ID 276, `DefinicjaPlanowanejListyPłac` ID 3)
  zapisały się poprawnie, `DefPlanListPlac.Element` wskazuje ID 111 (Odprawa
  emerytalna) — referencja przez GUID zadziałała.
- **Kod C# algorytmu (`FiltrNaliczania`/`KopiujNagłówek`/`KopiujElementy`/
  `KopiujElement`) skompilował się bez błędów** w ramach `dbmgr importxml`
  (etap "Compiling database ... Kompilacja projektu: KadryPlace") — więc
  wszystkie użyte typy/pola/metody (`SourceFilterArgs`, `PlanowanyElementWypłaty`,
  `Podatki.KopiujNaMinus`, `Module.DefElementow.WgNazwy`,
  `Module.PlanElementyWyp.AddRow`, `SkładnikGłówny`, `DoOpodatkowania` itd.)
  faktycznie istnieją i mają zgodne typy w wersji serwera 2512.5.6. To NIE jest
  jeszcze test wykonania (runtime) — tylko test kompilacji.
- Limity długości kolumn wymusiły skrócenie: `DefElementow.Skrot` (12 zn.) →
  "Rozw.odpr.em"; `DefPlanListPlac.Symbol` (12 zn.) → "ROZWODPREM";
  `DefPlanListPlac.Nazwa` (30 zn.) → "Rozwiązanie odpr.emeryt.".
- `DefPlanListPlac.DefinicjaED` **NIE** utworzyła się samoczynnie przy imporcie
  wg rekordów (zostało `NULL`) — hipoteza z pierwszej wersji tego dokumentu
  (że `OnImported`/`CreateDocumentDefinitionOADuringImport` wypełni to pole)
  się nie potwierdziła w tym trybie importu. Do sprawdzenia w GUI: czy trzeba
  to uzupełnić ręcznie (np. otwierając definicję w
  Ustawienia → Kadry i płace → Płace → Definicje planowanych list płac i
  zapisując ją ponownie), zanim funkcja zadziała.

**Potwierdzone dekompilacją** (wysoka pewność — to literalny kod z DLL serwera
klienta, wersja 2512.5.6):
- Sygnatury i kolejność wywołań `KopiujWypłatę` → `KopiujNagłówek` →
  `KopiujElementy` → `KopiujOpis`.
- Typy i pola `SourceFilterArgs`, `PlanowanyElementWypłaty` (konstruktor
  `(PlanowanaWypłata, DefinicjaElementu, WypElement)`), `Podatki.KopiujNaMinus`.
- Że `Element` w `DefPlanListPlac` musi być typu `Dodatek` (nie `Dodatek
  automatyczny`) — stąd nowy element "Rozwiązanie..." NIE może być typu
  "Dodatek automatyczny", mimo że użytkownik pierwotnie tak go nazwał
  (potocznie — "automatyczny" = "nie wymaga ręcznego wpisywania na liście
  planowanej", a nie dosłowny `Rodzaj=DodatekAutomatyczny` z enova).

**NIEZWERYFIKOWANE — wymaga próbnego importu (dbmgr importxml) i realnego
wygenerowania planu w GUI**, bo środowisko robocze tego repo nie ma dostępu do
żywego testu/buscall:
- Czy `FiltrNaliczania`/`SourceFilterArgs.Element` faktycznie dostaje
  wyłącznie elementy zgodne z polem `Element` definicji (czyli czy sama
  obecność `Element="Odprawa emerytalna"` już wystarczająco zawęża co trafia
  do `KopiujWypłatę`, czy silnik i tak przegląda też inne elementy wypłaty).
- Czy `dest.Module.DefElementow.WgNazwy["Rozwiązanie odprawa
  emerytalno-rentowa"]` faktycznie ZWRACA poprawny wiersz W RUNTIME (kod się
  skompilował — patrz wyżej — ale to nie dowodzi, że wywołanie się powiedzie,
  np. gdyby `Module` z `dest` był innym modułem niż ten z `DefElementow`).
- Czy brak `DefinicjaED` (patrz wyżej — pole zostało `NULL` po imporcie)
  faktycznie blokuje generowanie planu w GUI, czy jest uzupełniane przy
  pierwszym użyciu/zapisie w GUI.
- Klasyfikacja PIT/ZUS nowego elementu "Rozwiązanie..." (skopiowana 1:1 z
  "Odprawa emerytalna": ZUS nie nalicza, PIT wg skali) — wartości na
  rezerwie i tak są ustawiane wprost w kodzie (`KopiujElement`), więc
  Deklaracje nowego elementu mają znaczenie tylko dla ewentualnych raportów/
  filtrów czytających klasyfikację definicji, nie dla samego wyniku.
- Ogólnie: cały mechanizm `DefPlanListPlac` nie ma w tym repo ani w bazach
  testowych żadnego działającego precedensu (w przeciwieństwie np. do
  "Dodatek roczny", gdzie kod korygowano po kolejnych importach na bazie Al) —
  traktować jako **pierwszą wersję do zweryfikowania**, nie jako gotowe
  rozwiązanie.

## Szósta iteracja (2026-09-23): zwolniony pracownik "znika" z listy przy generowaniu planu

Użytkownik zgłosił nowy przypadek dla tego samego scenariusza testowego
(pracownik zwolniony 01/2026, odprawa wypłacona na liście głównej 08/2026):
licząc plan dla okresu 08/2026, mimo że pracownika nie ma już na "liście
pracowników", rozwiązanie odprawy i tak powinno mu się naliczyć.

Zdekompilowano `Soneta.KadryPlace.dll` (`NaliczaniePlanowanychListPłacWorker`,
`NaliczaniePlanowanychListPłac`, `NaliczanieSeryjne.Pracownika`) w wersji
serwera 2512.5.6 — ustalono, że **silnik naliczania (`NaliczanieSeryjne.Pracownika`
→ `NaliczanieWypłat`) nie ma żadnego wbudowanego filtra "tylko aktualnie
zatrudnieni"**: liczy dla dowolnego przekazanego mu obiektu `Pracownik`, dla
podanego okresu, niezależnie od statusu zatrudnienia (aktywny/zwolniony).

Akcja `NaliczaniePlanowanychListPłacWorker.Nalicz()` (przycisk "Nalicz
planowane listy płac...") działa na tablicy `Pracownik[] pracownicy`
wstrzykiwanej przez `[Context] Pracownik` z **aktualnie zaznaczonych wierszy
listy Pracownicy**, z której akcja jest wywoływana:

```csharp
[Context]
public Pracownik[] Pracownik { set { pracownicy = value; } }
...
public NaliczaniePlanowanychListPłac Nalicz()
{
    if (pracownicy.Length == 0) throw new CancelException();
    ...
    foreach (Pracownik pracownik in pracownicy)
        naliczaniePlanowanychListPłac.NaliczPracownika(pracownik, kontekstPłac);
```

**Wniosek: to NIE jest błąd/luka w naszym Dodatku automatycznym ani w
mechanizmie `DefPlanListPlac`.** Zwolniony pracownik "znika" wyłącznie dlatego,
że domyślny filtr listy Kadry → Pracownicy w GUI pokazuje tylko aktualnie
zatrudnionych — pracownik zwolniony w 01/2026 nie jest widoczny/zaznaczalny na
tej liście, więc nigdy nie trafia do `pracownicy[]`, a silnik w ogóle go nie
przelicza (nie dlatego, że coś go odrzuca wewnątrz naliczania).

**Rozwiązanie operacyjne (bez zmiany kodu):** w GUI przełączyć filtr listy
Pracownicy tak, żeby pokazywał też zwolnionych ("Wszyscy" zamiast domyślnych
"Aktualnie zatrudnieni"), zaznaczyć tego pracownika i z takiego zaznaczenia
uruchomić "Nalicz planowane listy płac...". Silnik policzy wtedy dla niego
okres 08/2026 normalnie, a Dodatek automatyczny "Rozwiązanie odprawa
emerytalno-rentowa" (priorytet 200, `Element` wskazujący na samego siebie)
powinien zadziałać zgodnie z projektem.

**Nadal NIEZWERYFIKOWANE żywym testem** (ani ta poprawka podejścia w GUI, ani
sam mechanizm rozwiązania odprawy) — do potwierdzenia przy najbliższym
kontakcie z klientem: wynik testu po zaznaczeniu zwolnionego pracownika z
filtrem "Wszyscy".

## Siódma iteracja (2026-09-23): operacyjnie to za mało — trzeba automatycznego przycisku

Użytkownik słusznie zwrócił uwagę, że rozwiązanie z poprzedniej sekcji (filtr
"Wszyscy" + ręczne zaznaczenie zwolnionego pracownika) jest nierealistyczne w
praktyce: generując plan na dany miesiąc, operator standardowo stoi na liście
pracowników zatrudnionych AKTUALNIE (w momencie liczenia planu), a nie
"zatrudnionych w miesiącu, dla którego liczy plan" — nie ma powodu, żeby co
miesiąc pamiętać o ręcznym doszukiwaniu zwolnionych. Wymóg użytkownika wprost:
**kod ma sam sprawdzać, komu w danym okresie wypłacono odprawę, i naliczać
rozwiązanie automatycznie — bez polegania na ręcznym zaznaczeniu pracownika**.

Ponieważ silnik (`NaliczaniePlanowanychListPłacWorker.Nalicz`) sam w sobie nie
ma haka pozwalającego Dodatkowi automatycznemu "dociągnąć" innego pracownika
spoza przekazanej tablicy `Pracownik[]` (dodatek liczy się TYLKO w kontekście
pracownika, dla którego silnik już został wywołany — nie może sam wywołać
przeliczenia dla kogoś innego), jedynym miejscem, w którym można to naprawić,
jest to, SKĄD bierze się `Pracownik[]` — czyli własny, skompilowany worker
zamiast polegania na zaznaczeniu listy Pracownicy w GUI.

**Rozwiązanie: nowy przycisk w skompilowanym dodatku**
(`Addon/A1RozwiazanieOdprawWorker.cs`, ten sam DLL co "Pełna lista płac XLSX",
`[assembly: Worker<A1RozwiazanieOdprawWorker, DefPlanListPlac>]` — przycisk na
widoku "Definicje planowanych list płac"):

1. Operator wybiera definicję planu (u nas: "Rozwiązanie odprawa
   emerytalno-rentowa") i okres/parametry przez STANDARDOWE okienko
   (`Soneta.Place.NaliczaniePlanowanychListPłacWorker.Params` — klasa
   wbudowana w enova, ponownie użyta wprost, bez przepisywania; publiczna,
   więc referencyjna z zewnętrznego dodatku).
2. Worker sam odpytuje `PlaceModule.WypElementy.WgDefinicja[odprawa]`
   (indeks tabeli historycznej `WypElementy` po polu `Definicja` — zwraca
   elementy WSZYSTKICH pracowników, niezależnie od statusu zatrudnienia) i
   filtruje po `Okres == Pars.Okres` oraz `Wartosc != 0` — dokładnie ten sam
   warunek, jaki już i tak sprawdza `_Param` Dodatku automatycznego
   (`Element.Elementy[Okres]` + `Nazwa.Contains("Odprawa emerytalna")`), tylko
   odwrócony: zamiast "dla TEGO pracownika sprawdź, czy ma odprawę w okresie"
   pytamy "dla TEGO okresu, którzy pracownicy mają odprawę".
3. Znaleziona lista `Pracownik[]` (aktywni i zwolnieni razem, bez rozróżniania)
   trafia do zwykłego, WBUDOWANEGO `NaliczaniePlanowanychListPłacWorker` przez
   jego publiczne właściwości `Pracownik`/`Pars`, po czym wywołujemy jego
   `Nalicz()` — **świadomie NIE odtworzono ręcznie wewnętrznej logiki
   sesji/transakcji tej metody** (tworzenie osobnej sesji, `Context.Clone`,
   dwie zagnieżdżone transakcje na różnych sesjach — patrz kod w
   `NaliczaniePlanowanychListPłac.Nalicz`/`NaliczPracownika` ustalony
   dekompilacją) — ryzyko subtelnego błędu w tak niedokumentowanym
   mechanizmie uznano za zbyt duże po już 5 rundach poprawek. Zamiast tego
   worker tylko DOSTARCZA inną listę pracowników do tej samej, sprawdzonej
   metody.

Lookup elementu źródłowego: `PlaceModule.DefElementow.WgNazwy["Odprawa
emerytalna"]` (indeks po dokładnej nazwie — potwierdzony jako istniejący i
kompilujący się typ w tej wersji serwera, patrz sekcja "Potwierdzone próbnym
importem" wyżej, gdzie `Module.DefElementow.WgNazwy` był już częścią
zweryfikowanego kodu).

**Skompilowano lokalnie** (`dotnet build -c Release` w `Addon/`) — exit 0,
`A1PelnaListaPlacAddon.dll` zbudowany bez błędów. Po drodze doszła jedna
poprawka projektu: `PlaceModule.GetInstance(Session)` wymaga typu
`ISupportRequiredService` z `Microsoft.Extensions.DependencyInjection.Abstractions.dll`
(referencja dodana do `.csproj`, `Private=false`, DLL z folderu serwera —
wcześniej niepotrzebna, bo `A1PelnaListaPlacWorker` nie wołał tej przeciążonej
metody).

**NIEZWERYFIKOWANE żywym testem** (jak cały mechanizm `DefPlanListPlac` w tym
zadaniu) — do zrobienia przed produkcją:
1. Wgrać `A1PelnaListaPlacAddon.dll` (przez `ExtPath`, restart usług — patrz
   `Addon/README.md`) na bazę testową.
2. W GUI: "Definicje planowanych list płac" → zaznaczyć "Rozwiązanie odprawa
   emerytalno-rentowa" → nowy przycisk "Nalicz plan wg wypłaconych odpraw
   (także zwolnieni)..." → podać okres 08/2026 → sprawdzić, czy pracownik
   zwolniony 01/2026 (z odprawą rozliczoną na liście głównej w 08/2026)
   pojawia się na wygenerowanym planie z pozycją "Rozwiązanie odprawa
   emerytalno-rentowa", MIMO że nie był zaznaczony na żadnej liście
   Pracownicy.
3. Przypadek negatywny: uruchomić dla okresu, w którym żaden pracownik nie ma
   rozliczonej odprawy — powinien pojawić się komunikat "Brak pracowników...",
   bez tworzenia pustego planu.
4. Sprawdzić przypadek pracownika AKTYWNEGO z odprawą w tym samym okresie —
   powinien zostać znaleziony i policzony tak samo jak zwolniony (zapytanie
   nie rozróżnia statusu zatrudnienia).

## Pliki

- `ImportyXML/Rozwiązanie odprawa emerytalno-rentowa.dbinit.xml` — import wg
  rekordów: `DefinicjaElementu` ("Rozwiązanie odprawa emerytalno-rentowa",
  Dodatek automatyczny) + `DefinicjaPlanowanejListyPłac` (`Element` wskazuje
  na SAM SIEBIE, patrz "Piąta iteracja" wyżej).
- `Addon/A1RozwiazanieOdprawWorker.cs` — skompilowany worker: przycisk "Nalicz
  plan wg wypłaconych odpraw (także zwolnieni)..." na widoku "Definicje
  planowanych list płac", omijający ograniczenie wbudowanej czynności enova do
  aktualnie zaznaczonych/zatrudnionych pracowników (patrz "Siódma iteracja"
  wyżej). Część tego samego `A1PelnaListaPlacAddon.dll` co funkcja "Pełna
  lista płac (XLSX)" — wgrywane i wdrażane razem, patrz `Addon/README.md`.

## Kolejne kroki przed produkcją

1. Import próbny na bazie testowej (Claude lub Al) przez `dbmgr importxml` —
   **zrobione** (2026-09-22, zobacz sekcję "Potwierdzone próbnym importem"
   wyżej), ale to tylko test kompilacji, nie zachowania.
2. Zbudować i wgrać `Addon/A1PelnaListaPlacAddon.dll` (`dotnet build -c
   Release` — **zrobione, exit 0**, patrz "Siódma iteracja" wyżej — wgranie
   przez `ExtPath` + restart usług na bazie testowej NIEZROBIONE).
3. **UWAGA przed testem w GUI:** generowanie planu kasuje niezatwierdzone
   wypłaty pracownika danego typu, chyba że zaznaczona jest opcja
   uwzględniania niezatwierdzonych list — testować WYŁĄCZNIE na danych
   testowych, nigdy na produkcyjnych bez zrozumienia tej opcji.
4. W GUI: rozliczyć testowemu pracownikowi (najlepiej ZWOLNIONEMU, jak w
   scenariuszu klienta) "Odprawa emerytalna" na liście głównej za dany okres,
   potem z widoku "Definicje planowanych list płac" uruchomić nowy przycisk
   "Nalicz plan wg wypłaconych odpraw..." dla TEGO SAMEGO okresu — bez
   zaznaczania kogokolwiek na liście Pracownicy — i sprawdzić, czy powstaje
   pozycja "Rozwiązanie odprawa emerytalno-rentowa" z poprawną kwotą.
5. Sprawdzić też przypadek negatywny (okres bez żadnej odprawy → komunikat,
   brak planu) i przypadek pracownika aktywnego (patrz punkty 3–4 w sekcji
   "Siódma iteracja" wyżej).
6. Po weryfikacji — zaktualizować ten plik z wynikiem (analogicznie do historii
   zmian w `ImportyXML/Dodatek roczny.dbinit.xml`).
