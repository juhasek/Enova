# SEOP Enrollment File – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy raport

Raport generuje plik „Enrollment file" dla programu SEOP (Skanska Employee
Ownership Plan / akcje pracownicze). Zgodnie z wymaganiem klienta plik ten
ma zawierać **wyłącznie** osoby, które w raportowanym okresie:

- po raz pierwszy przystąpiły do programu SEOP, **lub**
- po raz pierwszy przystąpiły do nowej edycji SEOP.

Nie jest to lista wszystkich aktualnych uczestników programu — to plik
zgłoszeniowy nowych przystąpień, wysyłany do operatora planu.

## 2. Parametry raportu

| Parametr | Opis |
|---|---|
| **Okres** | Miesiąc (domyślnie bieżący), za który generowany jest plik. |

## 3. Poprawka: raport pokazywał wszystkich uczestników zamiast tylko nowo przystępujących

**Objaw:** w sierpniu do aktualnej edycji SEOP przystąpił jeden pracownik, a
raport wygenerował ponad 200 osób zamiast tylko tego jednego pracownika.

**Przyczyna:** pętla po pracownikach znajdowała edycję SEOP obowiązującą dla
oddziału pracownika w raportowanym okresie, a następnie sprawdzała jedynie,
czy pracownik **w ogóle** jest uczestnikiem tej edycji
(`edycja.Uczestnictwa.FirstOrDefault(u => u.Pracownik == p)`). Warunek
odsiewający po „aktualności" uczestnictwa był w kodzie zakomentowany. W
efekcie do raportu trafiał każdy aktywny uczestnik danej edycji, niezależnie
od tego, kiedy faktycznie do niej przystąpił — stąd 200+ osób zamiast
jednej.

**Poprawka:** każde przystąpienie do programu (czy to pierwsze w ogóle, czy
do kolejnej edycji) tworzy nowy wiersz `UczestnictwoWAkcji`, którego
`OkresOd` to data przystąpienia. Dodano warunek, że do raportu trafiają
tylko uczestnictwa, których `OkresOd` mieści się w raportowanym okresie
(`pars.Okres.Contains(uczestnictwo.OkresOd)`). Uczestnicy kontynuujący
wcześniej rozpoczęte uczestnictwo (z `OkresOd` sprzed raportowanego okresu)
są pomijani.

```csharp
if (!pars.Okres.Contains(uczestnictwo.OkresOd))
{
    log.WriteLine($"Uczestnictwo pracownika rozpoczęło się poza raportowanym okresem (OkresOd={uczestnictwo.OkresOd}) - pomijam");
    continue;
}
```

## 4. Poprawka: brak adresu zamieszkania

**Objaw:** adres zamieszkania pracownika (`historia.AdresZamieszkania`) nie zawsze jest
uzupełniony w kartotece, przez co kolumny adresowe raportu (`ResidentAdress1`,
`ResidentCity`, `ResidenPostalCode`, `ResidentCountry`) wychodziły puste.

**Poprawka:** jeśli adres zamieszkania jest pusty (brak miejscowości), dane adresowe do
raportu pobierane są z adresu zameldowania (`historia.AdresZameldowania`) — ten sam subrow
`Soneta.Core.Adres`, więc te same pola (`Linia1`/`Miejscowosc`/`KodPocztowy`/`KodKraju`).

```csharp
var adresZamieszkania = historia?.AdresZamieszkania;
var adresDoRaportu = string.IsNullOrWhiteSpace(adresZamieszkania?.Miejscowosc)
    ? historia?.AdresZameldowania
    : adresZamieszkania;
```

Wszystkie pola adresowe w wierszu raportu korzystają teraz z `adresDoRaportu` zamiast
bezpośrednio z `historia?.AdresZamieszkania`.

## 5. Synchronizacja z wersją produkcyjną (zmiany wprowadzone poza tą sesją)

Po poprawkach 3–4 plik `Raporty/SeopEnrollmentFile` został ręcznie zmieniony bezpośrednio w
edytorze skryptów Enova i zsynchronizowany z powrotem do repo. Zmiany:

- log wyłączony na stałe (`new Log("SEOP", false)`),
- cecha oddziału `SEOP_GlobalCode` → `SEOP_CompanyCode` (zmiana nazwy technicznej cechy),
- `KontoAktywne` liczone z `uczestnictwo.AktualnoscOd` zamiast na sztywno `"Tak"`,
- nowe kolumny `AsOfDate` (data zatrudnienia) i `PersonalIdentityCode` (PESEL),
- `InternalParticipantID` tymczasowo pusty (zakomentowany kod źródłowy — do uzupełnienia),
- kolumna `SubsidiaryCode` w wierszu zastąpiona kolumną `Company` (ta sama wartość —
  `companyCode` z cechy oddziału),
- `ResidentCountry` zapisywany na sztywno jako `"POL"` zamiast kodu kraju z adresu — świadoma
  decyzja biznesowa, spójna z `HostCountry = "POL"`.

## 6. Formatowanie arkusza Excel: pseudo-autofit szerokości kolumn + wysokość wierszy

**Potrzeba:** plik `.xlsx` ma mieć dopasowaną szerokość kolumn do treści oraz ustaloną
wysokość wierszy.

**Bez EPPlus** — sprawdziliśmy, że przy eksporcie z poziomu standardowego podglądu wydruku
Enova DevExpress zapisuje plik samodzielnie i nie ma udokumentowanego zdarzenia dającego
dostęp do gotowego pliku „po fakcie", więc post-processing biblioteką EPPlus by tu nie
zadziałał (patrz historia commitów — podejście z EPPlus/Workerem zostało wycofane). Zamiast
tego formatowanie jest robione **natywnie przez DevExpress**, w samym `Report_BeforePrint`,
bo to on i tak generuje ten plik przy eksporcie.

**Struktura wzorca `SeopEnrollmentFile.repx` (potwierdzona z rzeczywistego pliku):**
pasmo `Lista` (`DetailReportBand`) zawiera dwa podpasma — `ListaWiersz` (`DetailBand`) z tabelą
`table1` (wiersz danych) i `ListaNagłówek` (`GroupHeaderBand`, `RepeatEveryPage="true"`) z tabelą
`table2` (nagłówek). Obie tabele mają dokładnie **32 komórki w tej samej kolejności**, więc
dopasowanie kolumn po indeksie komórki jest w pełni poprawne. Komórki danych są bindowane przez
`ExpressionBindings` (`Expression="[Gid]"`, `"[FirstName]"` itd.), **nie** przez klasyczny
`DataBindings["Text"]` — to sposób, w jaki edytor wydruków Enova domyślnie generuje bindowanie.
Komórki nagłówka mają statyczny `Text` (np. `"First_Name"`, `"Statement_Currency_Preference"`).

**Jak to działa (`DopasujSzerokosciKolumn`, `UstawWysokoscWierszy`):**

1. Po ustawieniu `Lista.DataSource = wynik` kod rekurencyjnie zbiera wszystkie kontrolki
   `XRTable`/`XRTableRow`/`XRTableCell` z pasma `Lista` (czyli `table1` i `table2`).
2. Dla każdej kolumny (indeks komórki) `OdczytajNazwePola` odczytuje `ExpressionBindings["Text"]`
   (parsując `"[NazwaWlasciwosci]"` → `"NazwaWlasciwosci"`, z fallbackiem na klasyczny
   `DataBindings["Text"].DataMember` dla kompatybilności z innymi wydrukami) — jeśli komórka jest
   zbindowana, liczy długość najdłuższej wartości tej właściwości w `wynik`; jeśli to statyczna
   komórka nagłówkowa (jak w `table2`), bierze długość jej tekstu.
3. Na tej podstawie liczy nową szerokość (`długość znaków × ok. 6,5 + margines`, przycięte do
   sensownego zakresu 40–400) i ustawia ją na **wszystkich** komórkach tej kolumny, w obu
   tabelach (żeby nagłówek i wiersz danych zostały wyrównane — inaczej siatka kolumn w
   wyeksportowanym Excelu się rozjedzie). Z uwagi na długie techniczne nazwy w nagłówku (np.
   `Relationship_to_Company_Hire_Date`, 34 znaki) to zwykle nagłówek, a nie dane, będzie
   dominował szerokość większości kolumn.
4. `UstawWysokoscWierszy` ustawia stałą wysokość (`WysokoscWiersza = 18`) na obu `XRTableRow`.

**Ograniczenia w porównaniu do EPPlus:** to przybliżenie (stała szerokość znaku dla domyślnej
czcionki, a nie rzeczywisty pomiar renderowanego tekstu jak w `AutoFitColumns()`) — dla
większości kolumn tekstowych powinno dać wynik „wystarczająco dobry", ale nie jest identyczne
z prawdziwym auto-fit.

**Do zweryfikowania na żywej aplikacji:**

- `XRTableCell.Weight` i `WidthF` są ustawiane jednocześnie dla pewności (w zależności od
  wersji DevExpress i trybu layoutu tabeli o realnej szerokości kolumny może decydować jedno
  lub drugie) — warto to zweryfikować na jednym wygenerowanym pliku i w razie potrzeby usunąć
  zbędne przypisanie.
- Kolumna „LP." (`tableCell1`) nie ma żadnego bindowania tekstu — numerację generuje
  `<Summary Func="RecordNumber">`, a jej `Text="LP."` jest statycznym tekstem domyślnym z
  projektanta. `OdczytajNazwePola` zwróci dla niej `null`, więc szerokość liczy się z długości
  literału `"LP."` (3 znaki) — wystarczające dla numeracji do ok. 999 wierszy, ale warto to
  mieć na uwadze, gdyby lista miała być znacznie dłuższa.

## 7. Znane ograniczenia / do potwierdzenia

- Filtr „nowo przystępujący" opiera się na założeniu, że `UczestnictwoWAkcji.OkresOd` to
  rzeczywista data przystąpienia pracownika do danej edycji (a nie np. data
  początku samej edycji, jednakowa dla wszystkich jej uczestników). Warto
  zweryfikować to założenie na danych produkcyjnych — jeśli `OkresOd`
  wszystkich uczestników edycji jest identyczny (np. równy początkowi
  edycji), filtr nie zadziała poprawnie i trzeba będzie znaleźć inne pole
  (np. datę utworzenia wiersza `UczestnictwoWAkcji`) jako źródło daty
  przystąpienia.
- Pozostaje otwarty, niezwiązany z tą poprawką problem opisany w kodzie:
  pętla pobiera historię pracownika na dzień `pars.Okres.From` (początek
  miesiąca), co może pomijać pracownika, który zaczął pracę w trakcie
  raportowanego miesiąca.
- `InternalParticipantID` jest obecnie pusty (kod źródłowy zakomentowany w
  `Raporty/SeopEnrollmentFile`) — do uzupełnienia właściwą wartością przed wysyłką pliku do
  operatora SEOP.
