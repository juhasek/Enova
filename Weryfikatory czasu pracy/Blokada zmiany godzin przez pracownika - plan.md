# Blokada zmiany godzin przez pracownika (dokument aktualizacji kalendarza)

Para weryfikatorów kalendarza do **wersjonowania kalendarzy**. Pracownik **na umowę o pracę**
może z pulpitu **złożyć nowy dokument aktualizacji kalendarza**, ale **nie może zmieniać na nim
godzin pracy**. Godziny zmienia przełożony (pulpit kierownika) albo kadry.

Cała logika jest w **Dodatkowym kodzie do kompilacji** (plik
[`BlokadaZmianyGodzinDAK`](BlokadaZmianyGodzinDAK), opis w `BlokadaZmianyGodzinDAK.md`).
Definicje weryfikatorów tylko go wywołują.

| Plik definicji | Rodzaj weryfikatora | Wywołanie |
|---|---|---|
| `Blokada zmiany godzin przez pracownika - plan` | `DzienPlanuAktualizacja` (dzień planu pracy na dokumencie) | `BlokadaZmianyGodzinDAK.Weryfikuj(source)` |
| `Blokada zmiany godzin przez pracownika - czas pracy` | `DzienPracyAktualizacja` (dzień czasu pracy na dokumencie) | `BlokadaZmianyGodzinDAK.Weryfikuj(source)` |

Jeśli dokumenty aktualizacji dotyczą wyłącznie planu pracy, wystarczy pierwsza definicja.

## Reguła

Błąd, gdy jednocześnie:
1. pozycja dokumentu dotyczy **pracownika na umowę o pracę**, czyli źródło planu to
   `Pracownik` (kalendarz etatu), a nie `Umowa` (cywilnoprawna z kalendarzem), i pracownik
   jest zatrudniony na etat w dniu zmiany (`JestZatrudnionyNaEtat`),
2. **zalogowany użytkownik pulpitu (web) to ten sam pracownik** (porównanie po `Guid`).

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

Istnienie dnia na dokumencie oznacza więc, że ktoś zmienia godziny. Weryfikator nie porównuje
wartości z planem.

## Wymagana konfiguracja

1. **System → Dodatkowy kod do kompilacji**: dodaj plik z zawartością
   `BlokadaZmianyGodzinDAK` (klasa `Soneta.Runtime.Database.Business.TblCodeFiles.BlokadaZmianyGodzinDAK`).
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
