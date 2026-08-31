# Zestawienie aktualizacji czasu pracy SKA – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy

Algorytm definicji zestawienia czasu (`DefinicjaZestawieniaCzasu`, nazwa w enova: „Aktualizacja
czasu pracy SKA”), powiązanie: `DokumentAktualizacjiCzasuPracy`. To jest **siatka (grid)**, w
której pracownicy samodzielnie wypełniają swój czas pracy dzień po dniu — dla każdego dnia okresu
pokazywana jest stała liczba wierszy/kolumn „Strefa” (np. 3), z polami: Definicja, Od, Do, Czas,
Projekt (Cecha), Task (Cecha) itd. Liczba stref na dzień wynika z maksymalnej liczby rzeczywistych
stref pracy występujących u danego pracownika w okresie (`GetRows`/`GetCells`).

Klasa `Cell` odpowiada za wyliczenie zawartości pojedynczej komórki siatki (jeden pracownik, jeden
dzień, jedna „Strefa” = kolejny wiersz/kolumna). Właściwość `Osoba` pokazuje nazwisko pracownika
tylko dla pierwszej strefy (`Strefa == 0`), dla pozostałych zwraca pusty string — to jest wzorzec, z
którego siatka korzysta, żeby nie powtarzać tych samych danych w każdej kolumnie.

## 2. Zgłoszony błąd i poprawka (31.08.2026) — strefa „Nieobecność” dublowała się w siatce (mechanizm, nie cecha)

**Zgłoszenie klienta:** mimo poprawek w cesze inicjującej dni (`Cechy/Inicjacja pozycji
aktualizacji czasu pracy`, ograniczonej do dodawania jednego wiersza „NB” na dzień bez źródła),
strefa „Nieobecność” w siatce nadal wyświetlała się po kilka razy (np. trzy identyczne wiersze
„Nieobecność” dla dnia 04.08), podczas gdy dzień z normalną pracą wyświetlał strefę tylko raz mimo
tej samej liczby wierszy w siatce. Pytanie klienta: czy problem leży w cesze, czy w mechanizmie
dodawania/wyświetlania wierszy.

**Diagnoza:** problem leży w **tym pliku** (mechanizm renderowania siatki), nie w cesze
`GetInitValueInicjacja`. Właściwości `Definicja`, `OdGodziny`, `DoGodziny` i `Czas` w klasie `Cell`
miały postać:

```csharp
public DefinicjaStrefy Definicja {
    get {
        INieobecnosc nb = getKalk(pak).Nieobecnosc(Data);
        if (nb != null)
            return KalendModule.GetInstance(Pracownik).DefinicjeStref.WgKodu["NB"].First();

        object ds = DaneStrefy(TypDanych.Definicja);
        return (DefinicjaStrefy)ds;
    }
    ...
}
```

`Cell` jest tworzona osobno dla **każdej** strefy/kolumny dnia (`Strefa` = 0, 1, 2, ...). Warunek
`if (nb != null)` sprawdzał tylko, czy w tym dniu istnieje jakakolwiek nieobecność — **nie**
sprawdzał, dla której kolumny (`Strefa`) jest aktualnie liczona wartość. W efekcie, jeśli dzień miał
nieobecność, to każda z 3 kolumn niezależnie „widziała” tę samą nieobecność i **każda** zwracała
„NB” (analogicznie `OdGodziny`/`DoGodziny` zwracały kod nieobecności, np. „U WYP”, w każdej
kolumnie) — stąd trzy identyczne wiersze „Nieobecność” w siatce, **niezależnie od tego, ile
faktycznie wierszy `StrefaPracyAktualizacja` istnieje w bazie**.

Dla dnia z normalną pracą ten warunek nigdy nie był prawdziwy (`nb == null`), więc każda kolumna
poprawnie sięgała do `DaneStrefy(...)`, która indeksuje **rzeczywiste** strefy dnia po numerze
`Strefa` — stąd tylko pierwsza kolumna (z rzeczywistą strefą „Praca w normie”) miała dane, a
pozostałe dwie były puste. To wyjaśnia różnicę zaobserwowaną przez klienta między dniem roboczym a
dniem nieobecności.

**Poprawka:** do każdego z czterech warunków dodano `&& Strefa == 0`, analogicznie do sposobu, w
jaki właściwość `Osoba` ogranicza się do pierwszej kolumny:

```csharp
if (nb != null && Strefa == 0)
    return KalendModule.GetInstance(Pracownik).DefinicjeStref.WgKodu["NB"].First();
```

Dzięki temu nieobecność wyświetla się tylko raz — w pierwszej kolumnie/wierszu strefy — a pozostałe
kolumny normalnie spadają do `DaneStrefy(...)`, zwracając pusto (tak jak dla dnia roboczego bez
danych w danej kolumnie). Zmiana dotyczy właściwości `Definicja` (linia z `WgKodu["NB"]`),
`OdGodziny`, `DoGodziny` i `Czas` (linie zwracające `nb.Definicja.Kod`).

## 3. Zależność od poprawki w cesze

Ta poprawka jest **niezależna** od wcześniejszej poprawki w
`Cechy/Inicjacja pozycji aktualizacji czasu pracy` (ograniczenie do jednego wiersza „NB” na dzień
bez źródła) i obie są potrzebne:

- cecha inicjująca odpowiada za to, ile **rzeczywistych wierszy** `StrefaPracyAktualizacja` trafia
  do bazy przy zakładaniu dokumentu,
- ten plik (algorytm zestawienia) odpowiada za to, co **wyświetla się w siatce** — i to on
  duplikował widok nieobecności niezależnie od liczby rzeczywistych wierszy w bazie, bo w ogóle nie
  patrzył na numer kolumny (`Strefa`).

## 4. Do potwierdzenia / obserwacje

- Pozostałe właściwości `Cecha*` (Projekt, Task, DodatekBryg, EkwiwalentZaPranie,
  Expenditure_organization) w gettery **nie** miały tego problemu — nie mają wczesnego zwrotu na
  podstawie `nb`, tylko od razu indeksują rzeczywiste dane przez `DaneStrefy(...)`, więc pokazują
  się poprawnie tylko dla kolumn z rzeczywistymi wierszami.
- Setterach tych samych właściwości (`Definicja`, `OdGodziny`, `DoGodziny`, `Czas` i pól `Cecha*`)
  pozostał osobny warunek `if (nb != null) nb.Delete();`, uruchamiany po skasowaniu wartości w danej
  kolumnie (`value == string.Empty` / `value == null`) — to inna funkcja (usuwanie nieobecności przy
  czyszczeniu wiersza przez użytkownika), nie została zmieniona, bo nie była przedmiotem zgłoszenia.
- Plik zawiera duży blok zakomentowanego, historycznego kodu (m.in. alternatywne wersje pól
  `Definicja`/`OdGodziny`/`Czas`, właściwości `Umowa`/`NormaO`/`NormaR`/`Info`) — pozostał bez zmian,
  nieaktywny, tak jak w oryginalnym pliku.
