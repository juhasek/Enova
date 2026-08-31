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

**Pierwsza próba poprawki (31.08.2026):** do każdego z czterech warunków dodano `&& Strefa == 0`,
analogicznie do sposobu, w jaki właściwość `Osoba` ogranicza się do pierwszej kolumny:

```csharp
if (nb != null && Strefa == 0)
    return KalendModule.GetInstance(Pracownik).DefinicjeStref.WgKodu["NB"].First();
```

To usunęło trojenie się wiersza „Nieobecność”, ale ujawniło **kolejny błąd** — patrz punkt 3.

## 3. Zgłoszony błąd i poprawka (31.08.2026, cd.) — dzień mieszany (praca + nieobecność) tracił pierwszą realną strefę

**Zgłoszenie klienta (zrzuty ekranu):** dzień w kalendarzu pracownika miał 3 realne strefy: „Praca
zdalna regulaminowa” 7:00–8:00 (1h), „Praca zdalna regulaminowa” 12:00–15:00 (3h) i „Nieobecność”
(bez godzin, typ OPIEKA). W zestawieniu ten sam dzień pokazywał: kolumna 1 — „Nieobecność”
OPIEKA/OPIEKA/OPIEKA (**błędnie** — powinna być pierwsza „Praca zdalna” 7:00–8:00), kolumna 2 —
„Praca zdalna” 12:00–15:00 (poprawnie), kolumna 3 — „Nieobecność” 0:00 (poprawnie).

**Przyczyna:** warunek `if (nb != null && Strefa == 0)` z poprzedniej poprawki nadal sprawdzał się
**przed** sięgnięciem po realne dane strefy (`DaneStrefy`). Działa to poprawnie dla dnia z samą
nieobecnością (bo tam kolumna 0 i tak nie ma nic innego do pokazania), ale dla dnia **mieszanego**
(praca + nieobecność, wszystko jako realne wiersze `StrefaPracyAktualizacja` — tak jak w
kalendarzu na zrzucie) kolumna 0 bywa realną strefą pracy. Wymuszone „NB” bezwarunkowo przesłaniało
tę realną, pierwszą strefę pracy — kolumny 1 i 2 wypadały poprawnie tylko dlatego, że warunek już
ich nie dotyczył (`Strefa != 0`), więc normalnie spadały do `DaneStrefy(...)`.

**Poprawka:** odwrócono kolejność sprawdzania w `Definicja`, `OdGodziny`, `DoGodziny` i `Czas` —
najpierw próba pobrania **realnych** danych strefy (`DaneStrefy(...)`), a dopiero gdy dla danej
kolumny nie ma żadnych realnych danych (`ds == null`), fallback do „NB”/kodu nieobecności — nadal
ograniczony do `Strefa == 0`, żeby nie duplikować:

```csharp
public DefinicjaStrefy Definicja {
    get {
        object ds = DaneStrefy(TypDanych.Definicja);
        if (ds != null)
            return (DefinicjaStrefy)ds;

        INieobecnosc nb = getKalk(pak).Nieobecnosc(Data);
        if (nb != null && Strefa == 0)
            return KalendModule.GetInstance(Pracownik).DefinicjeStref.WgKodu["NB"].First();

        return null;
    }
    ...
}
```

Dzięki temu:

- dzień mieszany (praca + nieobecność, wszystko jako realne wiersze) — każda kolumna pokazuje
  swoje realne dane, bez przesłaniania; nieobecność pokazuje się tam, gdzie faktycznie jest w
  danych (tak jak kolumna 3 na zrzucie), nie wymuszona na kolumnie 0,
- dzień z samą nieobecnością, dla którego cecha inicjująca dodała już jeden realny wiersz „NB”
  (patrz `Cechy/Inicjacja pozycji aktualizacji czasu pracy`) — kolumna 0 i tak dostaje ten wiersz
  przez `DaneStrefy`, fallback praktycznie się nie uruchamia,
- fallback na „NB” w kolumnie 0 zostaje jako zabezpieczenie na wypadek dnia, dla którego naprawdę
  nie ma jeszcze żadnych realnych danych (np. całkowicie niezainicjowana komórka), żeby użytkownik
  widział choć sygnał nieobecności zamiast pustki.

## 4. Zgłoszony błąd i poprawka (31.08.2026, cd.) — Od/Do/Czas realnej strefy „Nieobecność” miały pokazywać rodzaj nieobecności

**Zgłoszenie klienta:** po poprawce z punktu 3 realna strefa „Nieobecność” (np. kolumna 3 na
zrzucie z punktu 3 — dzień 10.08, `Czas = 0:00`, puste Od/Do) pokazywała pustkę/zero w Od, Do i
Czas. Klient chce, żeby dla **każdej** strefy „Nieobecność” (nie tylko fallbacku kolumny 0) w tych
trzech kolumnach był widoczny rodzaj nieobecności (np. „OPIEKA”) — tak jak wcześniej pokazywał to
tylko fallback (kolumna 0 na zrzucie z punktu 3).

**Poprawka:** w `OdGodziny`, `DoGodziny` i `Czas` dodano, przed sięgnięciem po surowe dane
godzinowe strefy, sprawdzenie, czy realna definicja tej kolumny (`DaneStrefy(TypDanych.Definicja)`)
oznacza nieobecność (`DefinicjaStrefy.OznaczaNieobecnosc` — ta sama flaga, której już używa
`GetBackColor` do kolorowania stref nieobecności). Jeśli tak, kolumna zwraca kod konkretnej
nieobecności z kalendarza pracownika (`getKalk(pak).Nieobecnosc(Data).Definicja.Kod`, np.
„OPIEKA”), zamiast realnych (pustych/zerowych) godzin tej strefy:

```csharp
object dsDef = DaneStrefy(TypDanych.Definicja);
if (dsDef is DefinicjaStrefy defStrefy && defStrefy.OznaczaNieobecnosc) {
    INieobecnosc nbStrefa = getKalk(pak).Nieobecnosc(Data);
    if (nbStrefa != null)
        return nbStrefa.Definicja.Kod;
}
```

Dotyczy to teraz **każdej** kolumny z realną strefą „Nieobecność”, niezależnie od jej numeru
(`Strefa`) — nie tylko fallbacku dla kolumny 0 z punktu 2/3, który zostaje bez zmian jako
zabezpieczenie dla dnia bez żadnych realnych danych.

## 5. Zależność od poprawki w cesze

Ta poprawka jest **niezależna** od wcześniejszej poprawki w
`Cechy/Inicjacja pozycji aktualizacji czasu pracy` (ograniczenie do jednego wiersza „NB” na dzień
bez źródła) i obie są potrzebne:

- cecha inicjująca odpowiada za to, ile **rzeczywistych wierszy** `StrefaPracyAktualizacja` trafia
  do bazy przy zakładaniu dokumentu,
- ten plik (algorytm zestawienia) odpowiada za to, co **wyświetla się w siatce** — i to on
  duplikował widok nieobecności niezależnie od liczby rzeczywistych wierszy w bazie, bo w ogóle nie
  patrzył na numer kolumny (`Strefa`).

## 6. Do potwierdzenia / obserwacje

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
