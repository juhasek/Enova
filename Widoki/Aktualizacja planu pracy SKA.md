# Aktualizacja planu pracy SKA – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy

Definicja zestawienia czasu (`DefinicjaZestawieniaCzasu`, tabela `DefZestawCzasu`, w bazie
`Claude` **ID = 11**, nazwa w enova: „Aktualizacja planu pracy SKA”), powiązanie:
`DokumentAktualizacjiKalendarza` (Rodzaj = `Zestawienie`, Powiazanie =
`DokumentAktualizacjiPlanuPracy`). To jest **siatka (grid)**, w której edytuje się plan pracy
(dzień po dniu: Typ dnia, Od, Do, Czas) dla pracowników objętych dokumentem aktualizacji planu.

Dwie dodatkowe kolumny wierszowe (`[YAxis]`), widoczne raz na wiersz pracownika:

- **„Norma / KP (okres)”** (`NormaO`) — norma z kalkulatora aktualizacji planu i (po ` / `) norma
  kodeksowa (`KalkulatorKodeksowyPracownika`) dla okresu przekazanego do zestawienia (typowo
  miesiąc widoczny w siatce).
- **„Norma / KP (rozl)”** (`NormaR`) — to samo, ale dla okresu rozliczeniowego nadgodzin
  (`Pracownik.WyliczOkresRoliczeniowyNadgodzin(Data)`).

## 2. Zgłoszony błąd i poprawka (17.09.2026) — kolumny normy nie odświeżały się na żywo

**Zgłoszenie klienta:** po zmianie godzin (Od/Do/Czas) na poszczególnych dniach w siatce,
wartości w kolumnach „Norma / KP (okres)” i „Norma / KP (rozl)” nie aktualizowały się na bieżąco.

**Diagnoza:** wyliczenie samej normy było poprawne — `GetKalk(pak)` w części `Source` unieważnia
swój cache kalkulatorów przy każdej zmianie w sesji (`dokument.Session.TouchCounter`), więc
`NormaOkres()` zawsze liczy na aktualnych danych, gdyby została odpytana ponownie. Problem leżał
w tym, że **nic nie mówiło siatce (UI), że ma tę kolumnę odpytać ponownie** po edycji innej
komórki w tym samym wierszu.

Klasa bazowa `DefinicjaZestawieniaCzasu.Extender` ma wbudowany mechanizm: jej `OnChanged()`
(wywoływane automatycznie przy zmianie własnych pól Extendera, np. pól grupy „Wstaw serię” —
`Okres`, `DefDnia`, `OdGodziny`, `Czas`, `Pomin`, `Nadpisz`) ustawia statyczną flagę
`Extender.ForceReloadRows = true`. Edycja **komórki dnia** (`Definicja`, `OdGodziny`,
`DoGodziny`, `Czas` we właściwościach klasy `Cell`) nie przechodzi jednak przez ten mechanizm —
to zwykłe settery zapisujące dane w sesji, bez żadnego wywołania `OnChanged`.

**Pierwsza próba poprawki (17.09.2026, w oparciu o historyczny, zakomentowany kod w pokrewnym
pliku `Widoki/Zestawienie aktualizacji czasu pracy SKA`) ustawiała jawnie
`Extender.ForceReloadRows = true` + `Session.InvokeChanged()` w setterach `Cell`. Testy
użytkownika na żywej aplikacji (klient webowy enova) pokazały, że to NIE wystarcza — wartość
nie odświeżała się nawet po nawigacji między dniami w tej samej siatce** (w odróżnieniu od
natywnej zakładki „Planowanie”, gdzie analogiczne pole odświeża się poprawnie — ale tamto pole
korzysta z całkiem innego mechanizmu formularza, nie z tego zestawienia).

**Właściwa przyczyna (potwierdzona przez użytkownika na innej, działającej definicji
zestawienia w tym środowisku):** klient webowy enova odświeża siatkę zestawienia w reakcji na
`OnChanged(EventArgs.Empty)` wywołane **na instancji Extendera** (metoda odziedziczona z
`ContextBase`, patrz `references/contextbase.md` w skillu `soneta-programming` — to ten sam
mechanizm, którego używają parametry wydruków/czynności). Sama statyczna flaga
`ForceReloadRows` i `Session.InvokeChanged()` — mimo że to realne, istniejące API platformy
(zweryfikowane dekompilacją `Soneta.KadryPlace.dll`) — nie są tym, co ten konkretny web-grid
faktycznie obserwuje.

**Poprawka (wersja finalna):** `OnChanged` jest `protected`, więc `Cell` (osobna klasa, nie
dziedziczy z `Extender`) nie może go wywołać bezpośrednio. Rozwiązanie:

1. `Source.GetCells` przekazuje referencję do żywej instancji Extendera (`ext`) do konstruktora
   każdej `Cell` (dodatkowy parametr).
2. W `Extender` dodano publiczny wrapper:
   ```csharp
   public void OdswiezZestawienie() {
       OnChanged(EventArgs.Empty);
   }
   ```
3. W `Cell` prywatna metoda `OdswiezZestawienie()` woła `ext.OdswiezZestawienie();` — wywoływana
   na końcu setterów `Definicja`, `OdGodziny`, `DoGodziny`, `Czas`.
4. Akcja `WstawSerię` w `Extender` (zmienia godziny wielu dni na raz tym samym mechanizmem sesji
   co edycja pojedynczej komórki) na końcu woła `OdswiezZestawienie();` (bezpośrednio, ma dostęp
   do `OnChanged` jako część tej samej klasy).

## 3. Do potwierdzenia

- Ta wersja (przekazanie `ext` do `Cell` + `OnChanged` na instancji Extendera) **jeszcze nie
  została przetestowana na żywo** — czeka na potwierdzenie użytkownika w GUI. Pierwsza wersja
  (statyczny `ForceReloadRows`/`Session.InvokeChanged()`) była testowana i **nie zadziałała** —
  zob. wyżej.
- Środowisko robocze tego repo nie ma dostępu do buscall/GUI, więc kolejne iteracje tej
  poprawki są testowane wyłącznie przez użytkownika w żywej aplikacji.

## 4. Sekcja „Norma pracownika” w panelu na dole (24.09.2026) — zamiast kolumn w siatce

**Zgłoszenie klienta:** kolumny „Norma / KP (okres)” i „Norma / KP (rozl)” (opisane w punkcie 1
jako `[YAxis]` w klasie `Cell`) mają zniknąć z siatki. W ich miejsce ma powstać osobna sekcja na
dole ekranu, w której te same informacje o normie pracownika są widoczne na bieżąco.

**Zmiana:**

- Usunięto z `Cell` właściwości `NormaO`/`NormaR` oraz pomocniczą metodę `NormaOkres(FromTo)`.
- W `Extender` dodano grupę pól **„Norma pracownika”**:
  - `NormaOkresPracownika` („Norma / KP (okres)”) — norma z kalkulatora aktualizacji planu i (po
    ` / `) norma kodeksowa, liczona dla okresu całego dokumentu (`dokument.Okres`).
  - `NormaRozlPracownika` („Norma / KP (rozl)”) — to samo, ale dla okresu rozliczeniowego
    nadgodzin wyliczonego względem początku okresu dokumentu
    (`Pracownicy.WyliczOkresRoliczeniowyNadgodzin(dokument.Okres.From)`).
- Obie właściwości liczą normę dla **jednego** pracownika — tego wybranego w natywnym,
  wbudowanym filtrze „Pracownicy” (pole `Pracownicy` w `Extender`, już wcześniej używane do
  filtrowania listy wierszy siatki), a nie dla każdego wiersza siatki z osobna jak poprzednio.
  Odpowiadającą pozycję dokumentu (`PozycjaAktualizacjiKalendarza`) szuka się w
  `dokument.PozycjePlan` po `ZrodloPlanu.GetPracownik() == Pracownicy` (metoda `PakPracownika()`).
- Logika samego wyliczenia (`KalkulatorAktualizacjiPlanu` + `KalkulatorKodeksowyPracownika`,
  format `"wartość / wartość"`) jest przeniesiona 1:1 z usuniętych kolumn — patrz historyczny
  kod w punkcie 1 tego dokumentu.

**Odświeżanie na żywo:** nowa sekcja nie potrzebuje osobnego mechanizmu odświeżania — to zwykłe
właściwości tylko do odczytu na tej samej instancji `Extender`, więc automatycznie przeliczają się
przy każdym przerysowaniu panelu. Przerysowanie już następuje przez `OdswiezZestawienie()`
(`OnChanged(EventArgs.Empty)` na instancji Extendera) wywoływane z setterów `Definicja`/
`OdGodziny`/`DoGodziny`/`Czas` w `Cell` (patrz punkt 2) — w przeciwieństwie do usuniętych kolumn
`[YAxis]`, które musiały pożyczać referencję do Extendera (`ext`), żeby w ogóle mieć dostęp do
`OnChanged`. Dzięki przeniesieniu do samego Extendera ten krok pośredni znika.

**Do potwierdzenia:** ta sekcja **nie została przetestowana na żywo** (środowisko robocze bez
dostępu do buscall/GUI) — w szczególności:
- czy sam mechanizm `OdswiezZestawienie()` z punktu 2 rzeczywiście już działa (jeszcze
  niepotwierdzony przez użytkownika w chwili pisania tej zmiany),
- czy zmiana wybranego pracownika w filtrze „Pracownicy” sama w sobie odświeża tę sekcję (powinna,
  bo zmiana filtra przeładowuje wiersze siatki, ale nie zweryfikowano tego na żywo dla tego
  konkretnego panelu),
- czy `dokument.PozycjePlan` zawiera pozycję dla pracownika, zanim jakikolwiek wiersz zostanie
  wyrenderowany (sekcja odwołuje się do `dokument` ustawianego w `ListaPracownicy`, wywoływanego
  przez natywny mechanizm listy pracowników — powinno być już ustawione, ale niezweryfikowane).

**Błąd kompilacji przy pierwszym otwarciu (24.09.2026):** `FromTo` nie ma właściwości `Od`/`Do` —
poprawny odczyt granic okresu to `.From`/`.To` (tak jak już wcześniej w tym pliku, np.
`Row.Okres.From`/`Row.Okres.To` w innych plikach repo). Poprawiono
`Pracownicy.WyliczOkresRoliczeniowyNadgodzin(dokument.Okres.Od)` na `...(dokument.Okres.From)`.
