# Zestaw „Nadgodziny … silnik” – dokumentacja biznesowa

Dotyczy czterech cech: `Nadgodziny 50 silnik`, `Nadgodziny 100 silnik`,
`Nadgodziny NSW silnik`, `Nadgodziny okresowe silnik` (wszystkie na wierszu strefy pracy,
`Row` = `StrefaPracy`, typ wartości liczbowy – godziny). Dotychczasowy zestaw
(`Nadgodziny 50`, `100`, `NSW`, `okresowe` oraz wersje `_old`) **zostaje nietknięty** – oba
pracują równolegle, żeby dało się porównać wyniki przed przełączeniem elementów
wynagrodzenia.

## 1. Po co ten zestaw

enova liczy nadgodziny poprawnie i pokazuje je w `Kalendarz / Statystyka`, ale **jako jedną
liczbę na dzień/okres**. Klient potrzebuje wiedzieć, **ile z tych nadgodzin to nadgodziny
awaryjne, a ile zwykłe** – bo zasilają różne elementy wynagrodzenia. Dotychczasowe cechy
odpowiadały na to, licząc nadgodziny **od nowa** (własna norma dobowa, własne przeniesienia,
własna reguła dnia wolnego) – i każda różnica względem silnika objawiała się jako błąd
w rozbiciu (zgłoszenia 14.09.2026, 01.10.2026, 02.10.2026).

Ten zestaw odwraca podział pracy:

- **ile** godzin to nadgodziny – mówi **silnik** (`KalkulatorPracownika.Nadgodziny`, ten sam
  kod, który wypełnia Statystykę),
- **która strefa** je wypracowała – liczy cecha.

Normy dobowej, przeniesień, nieobecności częściowych, limitu `Nadgodz50`, dni świątecznych
ani reguły „godziny między dobami pracowniczymi” nie ma więc w kodzie cechy (jeden wyjątek,
pkt 4).

## 2. Jak to działa

### 2.1 Ile – z silnika

```csharp
ZestawienieNadgodzin zn = pracownik.Czasy.Nadgodziny(
        new FromTo(data, data),
        KalkulatorNadgodzin.TrybRozliczaniaNadgodzin.TylkoDobowe);
Time dobowe = zn.N50 + zn.N100 + zn.NSW;
```

`TylkoDobowe` + zakres jednego dnia daje czystą nadwyżkę dobową tego dnia, bez bilansowania
całego okresu rozliczeniowego (tryby silnika: `Pełny`, `TylkoDobowe`, `TylkoOkresowe`).

### 2.2 Które godziny – ostatnie `dobowe` godzin czasu pracy dnia

```csharp
FromTimes pracaDnia = dzienPracy.Praca;                 // suma przedziałów pracy dnia
Time doOdciecia = pracaDnia.Time - dobowe;              // gdy > 0
FromTimes nadgodzinyDnia = pracaDnia.Sub(doOdciecia);   // ostatnie "dobowe" godzin
```

`Dzien.Praca` to `FromTimes`, czyli **suma przedziałów** stref zwiększających czas pracy –
godziny nachodzące na siebie nie dublują się (tak samo liczy je silnik).
`FromTimes.Sub(Time)` odrzuca pierwsze N godzin, `Intersection(Time)` bierze pierwsze N –
tych samych metod używa `KalkulatorNadgodzin.NadgodzinyDobowe`, gdy musi ustalić, które
godziny nadwyżki wypadły w porze nocnej
(`dzien.Praca.Sub(norma).Intersection(n50).Intersection(nocOkres)`).

Jeżeli kalendarz ma `Rozliczanie dobowych wg: stref`
(`Kalendarz.Nadgodziny.RozliczanieDobowych = WgStref`), pierwszeństwo mają godziny **poza
oknem planu** – czyli praca przed rozpoczęciem zmiany i w „czarnej dziurze”. W bazie Claude
wszystkie kalendarze mają `wg czasu`, więc gałąź `wg stref` jest napisana, ale
nieprzetestowana.

### 2.3 Udział strefy – i stąd podział awaria / zwykłe

```csharp
return (decimal)okna.Intersection(new FromTime(Row.OdGodziny, Row.Czas)).Time.TotalHours;
```

Każda godzina dnia należy do dokładnie jednej strefy, więc cecha na strefie „Praca poza
normą awaria” zwraca swój kawałek nadgodzin, a cecha na „Praca poza normą” – swój. Nie ma
tu reguły kolejności ani dzielenia proporcjonalnego: wynik wychodzi z godzin zegarowych.

### 2.4 Podział 50 / 100 / NSW / okresowe

Reguły przepisane 1:1 z silnika (`KalkulatorNadgodzin.NadgodzinyDobowe`):

- **dzień NSW** – przy wyłączonym „Dobowe 100” (flaga `b2003` silnika; w bazie Claude tak
  jest) decyduje `DefinicjaDnia.NadgodzinySW`, przy włączonym – `Typ != Pracy`. Cała
  nadwyżka idzie wtedy do `Nadgodziny NSW silnik`, a pozostałe trzy cechy zwracają 0.
- **limit 50%** (`Kalendarz.Nadgodziny.Nadgodz50`) – stosowany tylko przy włączonym
  „Dobowe 100”. W bazie Claude jest wyłączone, więc cała nadwyżka dobowa jest 50%.
- **godziny nocne** – przy „Nocne 100” część nadwyżki wypadająca w oknie nocnym
  (`KalkulatorPracy.NocOkres`) przechodzi z 50% do 100%.
- **okresowe** – praca poza oknem planu dnia, która **nie** weszła do nadgodzin dobowych:
  godziny przed rozpoczęciem zmiany i w „czarnej dziurze” (silnik przy ustawieniu
  `Nadgodziny między dobami pracowniczymi: warunkowo` odejmuje je od czasu pracy dnia, więc
  nie są dobowe – rozlicza je okres) oraz nadwyżka nad planem, która nie wyszła ponad normę
  dobową. To odtwarza potwierdzony przez klienta przypadek z 14.09.2026 (strefa 6:00–7:00
  przed zmianą zaplanowaną od 19:00 → 1:00 okresowych) **bez** osobnej reguły w kodzie.

## 3. Konfiguracja, od której to zależy (baza Claude)

| Ustawienie | Wartość | Skutek |
| --- | --- | --- |
| `Config.Nadgodziny.Dobowe 100` | false | cała nadwyżka dobowa jako 50%, limit `Nadgodz50` nieaktywny; dzień NSW wg `NadgodzinySW` |
| `Config.Nadgodziny.Nocne 100` | true | nocna część nadwyżki przechodzi do 100% |
| `Config.Nadgodziny.Nadgodziny między dob jako okr 100 ext` | `Warunkowo` | godziny przed startem zmiany („czarna dziura”) nie są dobowe – idą do okresowych |
| `Kalendarz.Nadgodziny.RozliczanieDobowych` | `wg czasu` (wszystkie kalendarze) | nadgodzinami są ostatnie godziny pracy dnia |

## 4. Jedyne odstępstwo od silnika: magazyn nadgodzin

Godziny odłożone do magazynu (strefa „Nadgodziny do przeniesienia”, rozliczenie
„W kolejnych miesiącach”) silnik **odejmuje** od czasu pracy dnia, więc dla takiego dnia
pokazuje mniej nadgodzin albo 0:00. Przykład z bazy: `NG-07` / 10.10.2026 – 8:00 w strefach
„Godziny do odbioru” przeniesione do magazynu + 2:00 „Praca poza normą”; silnik dla tego
dnia poda **0:00 nadgodzin dobowych**.

Na cechach pracują elementy wynagrodzenia, więc potrzebny jest obraz „jak przepracowano”.
Dlatego **tylko dla dni, w których `Dzien.DoPrzeniesienia > 0`**, cecha liczy nadwyżkę tym
samym wzorem co silnik (`czas pracy + ZPrzeniesienia − czas w czarnej dziurze` wobec normy
dobowej z `WyliczNormęDobową`), ale **bez** odjęcia `DoPrzeniesienia`, i bierze wynik
większy. Dla `NG-07`/10.10 daje to 2:00 nadwyżki → cała przypada na strefę „Praca poza
normą” 20:00–22:00.

**Do rozstrzygnięcia z klientem:** czy to jest pożądane. Jeśli po przeniesieniu godzin do
magazynu element wynagrodzenia **nie** ma już nic płacić, ten wyjątek należy usunąć – wtedy
cecha pokaże 0:00, zgodnie ze Statystyką.

## 5. Oczekiwane wyniki na danych testowych

| Pracownik / dzień | 50 silnik | 100 silnik | NSW silnik | okresowe silnik |
| --- | --- | --- | --- | --- |
| `NG-07` / 10.10.2026 – 8:00 „do odbioru” (magazyn) + 2:00 poza normą 20:00–22:00 | 2:00 | 0:00 | 0:00 | 0:00 |
| `NG-08` / 14.10.2026 – odbiór 2:00 + 6:00 w normie + 3:00 poza normą | 3:00 | 0:00 | 0:00 | 0:00 |
| `NG-01` / 02.11.2026 – 8:00 w normie + 2:00 poza normą | 2:00 | 0:00 | 0:00 | 0:00 |
| `NG-07` / 08.11.2026 – Święto, praca bez planu | 0:00 | 0:00 | 4:00 | 0:00 |

Główny test tego zestawu: suma `50 silnik + 100 silnik + NSW silnik` po wszystkich strefach
dnia powinna być równa nadgodzinom dobowym tego dnia **z zakładki Statystyka** (poza dniami
z przeniesieniem do magazynu – patrz pkt 4).

## 6. Ryzyka i status

**Kompilują się i działają** – potwierdzone przez użytkownika na bazie `Claude`
(02.10.2026). Tym samym działają w edytorze skryptów enova elementy użyte tu po raz
pierwszy w tym repozytorium (wcześniej potwierdzone tylko dekompilacją
`Soneta.KadryPlace.dll`):

- `pracownik.Czasy.Nadgodziny(FromTo, KalkulatorNadgodzin.TrybRozliczaniaNadgodzin)` oraz
  `ZestawienieNadgodzin` (`N50`, `N100`, `NSW`, `N100Doba`, `N100Okres`, `Razem`),
- `Dzien.Praca` (`FromTimes`), `Dzien.DoPrzeniesienia`, `Dzien.ZPrzeniesienia`,
  `Dzien.CzasPomiędzyDobamiPracowniczymi`,
- `FromTimes.Sub`, `Add`, `Intersection`, `ToFlat`, `Time` i enumeracja po `FromTime`,
- enumy `RozliczanieDobowych`, `TrybNadgodzMiedzyDobamiPrac`, `AlgorytmNorma`, `TypDnia`.

Do sprawdzenia na danych klienta pozostaje zgodność liczb: suma `50 + 100 + NSW silnik` po
wszystkich strefach dnia wobec zakładki Statystyka (pkt 5) oraz decyzja o wyjątku na magazyn
nadgodzin (pkt 4).

**Wydajność:** każda z czterech cech woła silnik dla swojego wiersza strefy, więc dzień
z trzema strefami „poza normą” to kilkanaście przeliczeń dnia. `TylkoDobowe` + zakres
jednego dnia to najtańsze wejście, ale na dużych listach i w raportach będzie to wolniejsze
niż dotychczasowy zestaw (który silnika nie woła wcale). Jeśli okaże się za wolno, następnym
krokiem jest skompilowany dodatek (`Addon/`) z jednym przeliczeniem dnia w cache zamiast
czterech niezależnych cech.
