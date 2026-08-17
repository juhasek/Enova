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

## Ważne zastrzeżenie — do zweryfikowania przy pierwszej kompilacji

Kod snippetu został napisany na podstawie wzorców widocznych w
`Raporty/OdbiciaRCP` (bez dostępu do rzeczywistych assembly Soneta przy jego
tworzeniu), więc może wymagać drobnych poprawek nazw. Jeśli kompilacja w
Enova zgłosi błąd, wklej go — poprawię. Miejsca najbardziej niepewne:

- **`Pracownik.DataUrodzenia`** — jeśli taka właściwość nie istnieje wprost na
  `Pracownik`, sprawdź w IntelliSense projektanta właściwy odpowiednik (dane
  osobowe bywają pod osobną właściwością).
- **`Etat.Wydzial`** — właściwość Wydziału na etacie pracownika.
- **`Historia.Last`** — pobranie ostatniego wpisu historii zatrudnienia, gdy
  brak wpisu obejmującego dzisiejszą datę.
- **Nazwa techniczna cechy** w `wydzial.Cechy["JednostkaObslugujaca"]` — musi
  być zgodna z nazwą techniczną tej cechy zdefiniowaną w bazie
  (Konfiguracja > Cechy dla Wydziału), która może się różnić od widocznego
  podpisu „Jednostka obsługująca”. Jeśli cecha ma inną nazwę techniczną,
  popraw ciąg w `Cechy["..."]`.
