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
`Soneta.Place.Wyplata` w obu sygnaturach. Zaimportowano poprawkę i
zweryfikowano `dbmgr compile Claude` (exit 0) — ale to, jak wyżej, NIE
gwarantuje braku kolejnych błędów w tej samej, kompilowanej leniwie ścieżce;
wymaga ponownej próby "Nalicz" w GUI.

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

## Pliki

- `ImportyXML/Rozwiązanie odprawa emerytalno-rentowa.dbinit.xml` — import wg
  rekordów: nowy `DefinicjaElementu` ("Rozwiązanie odprawa
  emerytalno-rentowa", typ Dodatek, algorytm edytora praktycznie nieużywany —
  wartości i tak nadpisuje kod planu) + nowa `DefinicjaPlanowanejListyPłac`
  wskazująca `Element` = "Odprawa emerytalna" i zawierająca właściwy algorytm
  odwrócenia opisany wyżej.

## Kolejne kroki przed produkcją

1. Import próbny na bazie testowej (Claude lub Al) przez `dbmgr importxml` —
   **zrobione** (2026-09-22, zobacz sekcję "Potwierdzone próbnym importem"
   wyżej), ale to tylko test kompilacji, nie zachowania.
2. **UWAGA przed testem w GUI:** generowanie planu kasuje niezatwierdzone
   wypłaty pracownika danego typu, chyba że zaznaczona jest opcja
   uwzględniania niezatwierdzonych list — testować WYŁĄCZNIE na danych
   testowych, nigdy na produkcyjnych bez zrozumienia tej opcji.
3. W GUI: rozliczyć testowemu pracownikowi "Odprawa emerytalna" na liście
   głównej, wygenerować plan listy płac dla TEGO SAMEGO okresu (to ważne —
   plan przelicza dodatek na nowo dla podanego okresu, nie odczytuje historii
   — patrz sekcja wyżej) i sprawdzić, czy powstaje pozycja "Rozwiązanie
   odprawa emerytalno-rentowa" z poprawną (ujemną, równą) kwotą.
4. Sprawdzić też przypadek negatywny: wygenerować plan dla okresu, w którym
   pracownik NIE miał odprawy — pozycja "Rozwiązanie..." nie powinna w ogóle
   powstać.
5. Po weryfikacji — zaktualizować ten plik z wynikiem (analogicznie do historii
   zmian w `ImportyXML/Dodatek roczny.dbinit.xml`).
