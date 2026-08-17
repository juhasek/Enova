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
| 4 | Data urodzenia | `Pracownik.DataUrodzenia` |
| 5 | Jednostka obsługująca | Cecha „Jednostka obsługująca” z Wydziału bieżącego etatu pracownika |
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

## Odporność na błędy w danych (kolumny 2–5)

Nazwy właściwości użyte w snippecie (`Pracownik.Kod`, `.NazwiskoImię`,
`.DataUrodzenia`, `.Historia`, `Etat.Wydzial`, `wydzial.Cechy[...]`)
skompilowały się poprawnie w tej instalacji Enova. Mimo to obliczenie
każdej z kolumn 2–5 jest opakowane w `Bezpiecznie(...)`: jeśli dla
konkretnego pracownika coś rzuci wyjątkiem w trakcie druku (np. brak
aktywnego etatu, brak przypisanego Wydziału, brak cechy o danej nazwie
technicznej na tym Wydziale), w tej jednej komórce pojawi się
`[BŁĄD: <treść wyjątku>]` zamiast wywalenia całego wydruku.

Jeśli po uruchomieniu zobaczysz taki komunikat w którejś kolumnie, wklej go
tutaj — pozwoli to precyzyjnie poprawić dane wejściowe albo logikę (np.
zmienić nazwę techniczną cechy w `wydzial.Cechy["JednostkaObslugujaca"]`,
jeśli w tej bazie nazywa się inaczej — sprawdź w Konfiguracja > Cechy dla
Wydziału).
