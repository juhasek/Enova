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
`Extender.ForceReloadRows = true`, którą framework siatki sprawdza przy najbliższym odświeżeniu i
wymusza ponowne `GetRows`/`GetCells` (czyli przeliczenie wszystkich kolumn, w tym `[YAxis]`).
Edycja **komórki dnia** (`Definicja`, `OdGodziny`, `DoGodziny`, `Czas` we właściwościach klasy
`Cell`) nie przechodzi jednak przez ten mechanizm — to zwykłe settery zapisujące dane w sesji,
bez żadnego wywołania `OnChanged`/`ForceReloadRows`/`Session.InvokeChanged()`.

Ten sam wzorzec naprawy (ustawienie `ForceReloadRows` + `Session.InvokeChanged()` po edycji
komórki) istniał już wcześniej, w formie zakomentowanego, historycznego kodu w pokrewnym pliku
`Widoki/Zestawienie aktualizacji czasu pracy SKA` (setter `Czas`, wersja przed obecnym
mechanizmem `DaneStrefy`) — potwierdza to, że jest to znany, rzeczywisty sposób wymuszenia
odświeżenia tego typu zestawień, nie tylko domysł.

**Poprawka:** w klasie `Cell` dodano prywatną metodę pomocniczą:

```csharp
void OdswiezZestawienie() {
    DefinicjaZestawieniaCzasu.Extender.ForceReloadRows = true;
    pak.Session.InvokeChanged();
}
```

i wywołanie `OdswiezZestawienie();` na końcu setterów `Definicja`, `OdGodziny`, `DoGodziny` i
`Czas` — po każdej faktycznej zmianie dnia planu. Dodatkowo to samo wywołanie (przez
`context.Session.InvokeChanged()`) dodano na końcu akcji `WstawSerię` w `Extender` — ta akcja
zmienia godziny wielu dni na raz tym samym mechanizmem sesji co edycja pojedynczej komórki, więc
miała identyczny problem z nieodświeżaniem kolumn normy.

## 3. Do potwierdzenia

- Poprawka nie została jeszcze zweryfikowana na żywo (środowisko robocze tego repo nie ma
  dostępu do buscall/GUI) — do potwierdzenia przez użytkownika po wklejeniu kodu do edytora
  definicji zestawienia (zakładki „Cell” i „Extender”) i przeliczeniu w enova.
- `ForceReloadRows` i `Session.InvokeChanged()` zweryfikowano jako rzeczywiste, istniejące
  elementy API platformy (deasemblacja `Soneta.KadryPlace.dll`, klasa
  `Soneta.Kalend.DefinicjaZestawieniaCzasu.Extender` — `public static bool ForceReloadRows { get; set; }`
  ustawiane w `OnChanged()`; `Session.InvokeChanged()` używane analogicznie w natywnym kodzie
  `DokumentAktualizacjiKalendarza`/`KalkulatorDokumentuAktualizacji`), nie z domysłu.
