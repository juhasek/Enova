# Blokada zmiany godzin przez pracownika (dokument aktualizacji kalendarza)

Para weryfikatorów kalendarza do **wersjonowania kalendarzy**. Pracownik może z pulpitu
**złożyć nowy dokument aktualizacji kalendarza**, ale **nie może zmieniać na nim godzin
pracy**. Godziny zmienia przełożony (pulpit kierownika) albo kadry.

| Plik | Rodzaj weryfikatora | Typ wiersza |
|---|---|---|
| `Blokada zmiany godzin przez pracownika - plan` | `DzienPlanuAktualizacja` | `DzienKalendarzaAktualizacja` (dzień planu pracy na dokumencie) |
| `Blokada zmiany godzin przez pracownika - czas pracy` | `DzienPracyAktualizacja` | `DzienPracyAktualizacja` (dzień czasu pracy na dokumencie) |

Logika obu plików jest identyczna, różni się tylko typ wiersza. Jeśli dokumenty aktualizacji
dotyczą wyłącznie planu pracy, wystarczy pierwszy.

## Reguła

Błąd, gdy **zalogowany użytkownik pulpitu (web) to ten sam pracownik, którego dotyczy pozycja
dokumentu** (porównanie po `Guid`; źródło planu = pracownik albo umowa → `Umowa.Pracownik`).

| Kto zmienia godziny | Wynik |
|---|---|
| Pracownik na swoim dokumencie (pulpit pracownika) | **blokada** |
| Kierownik na dokumencie podwładnego (pulpit kierownika) | dozwolone |
| Kierownik na dokumencie dotyczącym jego samego | **blokada** (jest „tym samym pracownikiem”) |
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

1. **Definicje weryfikatorów kalendarza**: dodaj dwie definicje rodzaju
   *Dzień planu aktualizacji* i *Dzień pracy aktualizacji* i wklej kod z plików.
2. **Powiązanie z kalendarzem** (`Kalendarz → Weryfikatory`) z **Typ = Error**, inaczej
   jest to tylko ostrzeżenie i zapis przejdzie. Weryfikator jest pobierany z kalendarza
   planu pracownika (`ZrodloPlanu.GetPlanKalendarz(data)`), czyli z **kalendarza
   wzorcowego z etatu**. Podpiąć go do **każdego** kalendarza wzorcowego, na którym są
   pracownicy korzystający z pulpitu.
3. **Rola pulpitowa**: pełne prawa do definicji dokumentu i do tabel dokumentu (dni
   i strefy też pełne, blokadę robi weryfikator, nie prawa).

## Jak to wygląda dla pracownika

Komórki w siatce wyglądają na edytowalne. Przy zapisie pojawia się błąd
„Nie możesz zmieniać godzin pracy na dokumencie aktualizacji kalendarza (dzień …)”
i zmiana się nie zapisze. Pracownik może dalej uzupełnić nagłówek (opis, okres)
i przekazać dokument do akceptacji.

## Status

- Kod kompiluje się na bibliotekach serwera 2512.5.6 (kompilacja testowa poza enovą).
- **Niesprawdzone na żywym pulpicie**: przetestować na bazie testowej (pracownik zakłada
  dokument → zmienia godziny → zapis ma zostać odrzucony; kierownik zmienia godziny
  podwładnego → zapis przechodzi).

## API użyte w skrypcie

- `source.Session.Login.WebUserOperatingInstance` (`IWebUser`, `null` poza pulpitem) → `.Host`
  (`IWebOperator`, dla pracownika = `Pracownik`)
- `source.Pozycja.ZrodloPlanu` (`IZrodloPlanu`: `Pracownik` albo `Umowa`), `Umowa.Pracownik`
- `Pracownik.Guid`, `source.Data`, `TranslateFormat`
