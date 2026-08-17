# Raport – lista pracowników (6 kolumn)

Wzorzec wydruku Enova (`Raporty/SzablonTabela6Kolumn.repx`, format DevExpress
XtraReports) z tabelą o 6 kolumnach, zasilany snippetem
`Raporty/SzablonTabela6KolumnSnippet`.

## Kolumny

| # | Nagłówek | Zawartość |
|---|---|---|
| 1 | Lp. | Liczba porządkowa wiersza |
| 2 | Nr ewidencyjny | `Pracownik.Kod` |
| 3 | Nazwisko i Imię | `Pracownik.NazwiskoImię` |
| 4 | Data urodzenia | `pracownik.Historia[Date.Today].Urodzony.Data` |
| 5 | Jednostka obsługująca | Cecha „Jednostka obsługująca” z Wydziału bieżącego etatu pracownika — **na razie wyłączona w kodzie** (metoda `PobierzJednostkeObslugujaca` zwraca `""`), bo cecha jeszcze nie istnieje w tej bazie; po jej utworzeniu odkomentować linię z `wydzial.Cechy["JednostkaObslugujaca"]` i poprawić nazwę techniczną, jeśli będzie inna |
| 6 | (bez nagłówka) | Celowo pusta — do dalszej rozbudowy |

Raport działa na zaznaczeniu (np. z Listy pracowników) — analogicznie jak
`Raporty/OdbiciaRCP`.

## Jak podpiąć snippet w Enova

1. Otwórz `SzablonTabela6Kolumn` w projektancie wydruków Enova.
2. W oknie edycji wydruku znajdź opcję „Kod źródłowy” (edycja snippetu wydruku)
   i wklej tam całą zawartość pliku `Raporty/SzablonTabela6KolumnSnippet`.
3. Zapisz — Enova powinna sama skompilować kod i podpiąć go pod wydruk
   (uzupełniając w pliku `.repx` element `ComponentStorage/Snippet` ze
   wskazaniem na klasę `SzablonTabela6KolumnSnippet`).
4. Uruchom wydruk dla zaznaczonych pracowników i sprawdź wynik.

`.repx` już zawiera powiązania komórek wiersza danych z polami
`[Kolumna1]`…`[Kolumna6]` — to właśnie te pola wypełnia snippet (klasa
`Wiersz` z właściwościami `Kolumna1..Kolumna6`), więc nie trzeba nic zmieniać
w samym layoutcie.

## Historia usterki: brak źródła danych w `.repx`

Pierwsza wersja `.repx` miała w `ComponentStorage` tylko komponent
`BusinessContext` — bez żadnego `BusinessDataSource`. Snippet wywołuje
`DxReportHelpers.GetDataSourceEmpty(this).CustomDataSource = ...`, a ta
metoda szuka w `ComponentStorage` komponentu `BusinessDataSource` typu
„pusty” (`DataKind="Empty"`), do którego może podpiąć własną listę. Bez
niego `GetDataSourceEmpty(this)` zwracało `null`, więc każda próba wydruku
kończyła się `NullReferenceException` — niezależnie od tego, co działo się
w reszcie kodu snippetu (dlatego kolejne poprawki logiki snippetu nic nie
zmieniały).

Naprawione (częściowo) dodaniem do `ComponentStorage`:

```xml
<Item2 Ref="29" ObjectType="Soneta.Business.UI.DxReports.BusinessDataSource,Soneta.Business.UI.DxReports" Name="BusinessSource" DataKind="Empty" />
```

oraz atrybutu `DataSource="#Ref-29"` na korzeniu `XtraReportsLayoutSerializer`,
wskazującego na ten komponent. To naprawiło `GetDataSourceEmpty`, ale
diagnostyka etapowa w snippecie ujawniła kolejny, analogiczny brak: wywołanie
`DxReportHelpers.GetContext(this)` (na samym początku metody, jeszcze przed
pobraniem zaznaczenia) też zwracało `null`/rzucało `NullReferenceException` —
bo w `ComponentStorage` brakowało osobnego komponentu typu „Context”. Dodany
drugi komponent:

```xml
<Item3 Ref="30" ObjectType="Soneta.Business.UI.DxReports.BusinessDataSource,Soneta.Business.UI.DxReports" Name="BusinessSourceContext" DataKind="Context" />
```

**Ważne:** obie poprawki są w pliku `.repx`, nie w „Kod źródłowy” — trzeba
więc ponownie zaimportować/podmienić cały plik `.repx` w Enova (tak jak przy
pierwszym dodawaniu wzorca), samo wklejenie nowej treści snippetu nie
wystarczy.

## Odporność na błędy w danych (kolumny 2–5)

Obliczenie każdej z kolumn 2–5 jest opakowane w `Bezpiecznie(...)`: jeśli dla
konkretnego pracownika coś rzuci wyjątkiem w trakcie druku (np. brak
aktywnego etatu, brak przypisanego Wydziału), w tej jednej komórce pojawi
się `[BŁĄD: <treść wyjątku>]` zamiast wywalenia całego wydruku. Cała metoda
`BeforePrint` jest dodatkowo opakowana w try/catch — w razie błędu poza
pętlą po pracownikach wydruk pokaże jeden wiersz diagnostyczny zamiast się
wywalić.

Jeśli po uruchomieniu zobaczysz `[BŁĄD: ...]` w którejś kolumnie, wklej go
tutaj — pozwoli to precyzyjnie poprawić dane wejściowe albo logikę.
