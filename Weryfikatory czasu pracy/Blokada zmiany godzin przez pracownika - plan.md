# Blokada zmiany godzin przez pracownika (dokument aktualizacji kalendarza)

Para weryfikatorów kalendarza do **wersjonowania kalendarzy**. Pracownik **na umowę o pracę**
może z pulpitu **złożyć nowy dokument aktualizacji kalendarza**, ale **nie może zmieniać na nim
godzin pracy**. Godziny zmienia przełożony (pulpit kierownika) albo kadry.

Cała logika jest w **Dodatkowym kodzie do kompilacji** (plik
[`BlokadaZmianyGodzinDAK`](BlokadaZmianyGodzinDAK), opis w `BlokadaZmianyGodzinDAK.md`).
Definicje weryfikatorów tylko go wywołują.

| Plik definicji | Rodzaj weryfikatora | Wywołanie |
|---|---|---|
| `Blokada zmiany godzin przez pracownika - plan` | `DzienPlanuAktualizacja` (dzień planu pracy na dokumencie) | `A1WeryfikatoryKalendarza.A1BlokadaGodzin(dzien.Session, dzien.Pozycja.ZrodloPlanu, dzien.Data, dzien.Pozycja)` |
| `Blokada zmiany godzin przez pracownika - czas pracy` | `DzienPracyAktualizacja` (dzień czasu pracy na dokumencie) | `A1WeryfikatoryKalendarza.A1BlokadaGodzin(dzien.Session, dzien.Pozycja.ZrodloPlanu, dzien.Data, dzien.Pozycja)` |

Jeśli dokumenty aktualizacji dotyczą wyłącznie planu pracy, wystarczy pierwsza definicja.

## Reguła

Błąd, gdy jednocześnie:
1. pozycja dokumentu dotyczy **pracownika na umowę o pracę**, czyli źródło planu to
   `Pracownik` (kalendarz etatu), a nie `Umowa` (cywilnoprawna z kalendarzem), i pracownik
   jest zatrudniony na etat w dniu zmiany (`JestZatrudnionyNaEtat`),
2. **zalogowany użytkownik pulpitu (web) to ten sam pracownik** (porównanie po `Guid`),
3. **dzień na dokumencie różni się od planu** pracownika (patrz „Porównanie z planem” niżej).

| Kto zmienia godziny | Wynik |
|---|---|
| Pracownik na UoP na swoim dokumencie (pulpit pracownika) | **blokada** |
| Zleceniobiorca (pozycja dla umowy cywilnoprawnej) | dozwolone |
| Pracownik w dniu poza okresem zatrudnienia na etat | dozwolone |
| Kierownik na dokumencie podwładnego (pulpit kierownika) | dozwolone |
| Kierownik na UoP na dokumencie dotyczącym jego samego | **blokada** (jest „tym samym pracownikiem”) |
| Operator kadr w programie (bez użytkownika pulpitu) | dozwolone |

## Dlaczego nie blokuje samego dodania dokumentu

Sprawdzone w kodzie enova 2512.5.6 (`DokumentAktualizacjiKalendarza.OnAdded`,
`AktualizacjaKalendarzaManager`):

- dodanie dokumentu „dla Składającego” tworzy nagłówek i **pozycję** pracownika, **bez dni**,
- dzień na dokumencie (`DzienPlanuAktualizacja` / `DzienPracyAktualizacja`) powstaje dopiero
  przy zmianie komórki w siatce, przez „Wstaw serię” albo „Wstaw z planu”,
- wbudowany `KontrolaDzienKalendarzaAktualizacjaVerifier` /
  `KontrolaDzienPracyAktualizacjaVerifier` uruchamia weryfikatory danego rodzaju z kalendarza
  dla każdego takiego dnia.

Sam wiersz dnia na dokumencie nie znaczy jednak, że godziny są inne. Po przywróceniu
pierwotnych godzin wiersz zostaje (tylko zamknięcie dokumentu bez zapisu go usuwa), dlatego
weryfikator porównuje dzień z planem.

## Porównanie z planem

| | Dzień na dokumencie | Dzień „pierwotny” |
|---|---|---|
| Plan pracy | `new KalkulatorAktualizacjiPlanu(pozycja)[data]` | `new KalkulatorPlanu(pracownik)[data]` |
| Czas pracy | `new KalkulatorAktualizacjiPracy(pozycja)[data]` | `new KalkulatorPracy(pracownik)[data]` |

Kalkulator aktualizacji liczy dzień tak, jak będzie wyglądał po zatwierdzeniu dokumentu
(z niezapisanymi zmianami w sesji). Zwykły kalkulator liczy dzień pracownika bez dokumentu.
Oba kalkulatory są tworzone od nowa przy każdym sprawdzeniu, bez buforowania.

Dni są takie same (`A1TakiSamDzien`), gdy zgadzają się: definicja dnia, godzina od, czas oraz
zestaw stref (bez względu na kolejność; strefy porównywane przez `Dzien.Strefa.Equals`:
definicja strefy, od–do, czas rozliczany, czynność, lokalizacja). Wtedy weryfikator nie
zgłasza błędu, czyli przywrócenie pierwotnych godzin zdejmuje błąd.

**Do sprawdzenia na żywo:** czy po przywróceniu godzin w siatce strefy wychodzą identyczne,
łącznie z czasem rozliczanym i czynnością. Jeśli pracownik przywróci godziny, a błąd zostanie,
przyczyną jest najpewniej któreś z tych pól.

## Wymagana konfiguracja

1. **System → Dodatkowy kod do kompilacji**: dodaj plik z zawartością
   `BlokadaZmianyGodzinDAK` (metoda `A1BlokadaGodzin` w klasie `A1.Runtime.KadryPlace.WeryfikatoryKalendarza.TblCodeFiles.A1WeryfikatoryKalendarza`, wspólnej dla weryfikatorów kalendarza).
   **Musi być dodany przed definicjami**, inaczej definicje się nie skompilują.
2. **Definicje weryfikatorów kalendarza**: dwie definicje rodzaju *Dzień planu aktualizacji*
   i *Dzień pracy aktualizacji* z kodem z plików (samo wywołanie).
3. **Powiązanie z kalendarzem** (`Kalendarz → Weryfikatory`) z **Typ = Error**, inaczej
   jest to tylko ostrzeżenie i zapis przejdzie. Weryfikator jest pobierany z kalendarza
   planu pracownika (`ZrodloPlanu.GetPlanKalendarz(data)`), czyli z **kalendarza
   wzorcowego z etatu**. Podpiąć go do **każdego** kalendarza wzorcowego, na którym są
   pracownicy korzystający z pulpitu.
4. **Rola pulpitowa**: pełne prawa do definicji dokumentu i do tabel dokumentu (dni
   i strefy też pełne, blokadę robi weryfikator, nie prawa).

## Jak to wygląda dla pracownika

Komórki w siatce wyglądają na edytowalne. Przy zapisie pojawia się błąd
„Nie możesz zmieniać godzin pracy na dokumencie aktualizacji kalendarza (dzień …)”
i zmiana się nie zapisze. Pracownik może dalej uzupełnić nagłówek (opis, okres)
i przekazać dokument do akceptacji.

## Status

- Kod do kompilacji i obie definicje kompilują się razem na bibliotekach serwera 2512.5.6
  (kompilacja testowa poza enovą).
- **Niesprawdzone na żywym pulpicie**: przetestować na bazie testowej (pracownik na UoP
  zakłada dokument → zmienia godziny → zapis ma zostać odrzucony; kierownik zmienia godziny
  podwładnego → zapis przechodzi).
