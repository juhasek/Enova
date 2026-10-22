# Widok „Ewidencja zatwierdzeń (PIP)” – dokumentacja biznesowa

Podwidok listy **Kadry i płace → Kadry → Czas pracy → Dokumenty aktualizacji kalendarzy** – raport
dla PIP: kto i kiedy złożył (przekazał do akceptacji) i kto i kiedy zatwierdził **czas pracy**
i **plan pracy** pracownika. Bez DLL – same cechy (kod w bazie) i widok listy.

Filtry jak lista standardowa (Definicja – czas / plan, Zakres dat, Stan). Kolumny:

Pracownik · Kod pracownika · Dział · Okres · Rodzaj (definicja) · Numer · Stan · Aktualny ·
Złożył · Złożono · Złożył jako · Zatwierdził · Zatwierdzono · Zatwierdził jako · Przełożony

Wydruk / Excel: standardowy wydruk listy albo eksport listy do arkusza w programie.

## Jak czytać

| Scenariusz | Złożył / jako | Zatwierdził / jako |
|---|---|---|
| Czas pracy – pracownik składa na pulpicie, kierownik zatwierdza | pracownik / Pracownik | kierownik / Przełożony |
| Plan pracy – kierownik przygotowuje i akceptuje | kierownik / Przełożony | kierownik / Przełożony |
| Admin składa za pracownika (w programie) | operator admina / **Inny operator** | wg tego, kto zatwierdził |

„Aktualny = Nie” – dokument zastąpiony później zatwierdzonym dokumentem tego samego rodzaju
(obowiązuje nowszy). Dla PIP zwykle filtr Aktualny = Tak, ale historię widać po zdjęciu filtra.

Źródło danych i zasady cech: [Cechy/Dokument aktualizacji kalendarza.md](../../Cechy/Dokument%20aktualizacji%20kalendarza.md).

## Instalacja

`dodaj-widok.ps1 -Baza <nazwa bazy SQL>` – jak przy widoku „Aktualne dokumenty” (kopia bazy
`COPY_ONLY`, potem w transakcji wpis widoku i `viewform.xml`; gdy widok już jest – podmiana kolumn).
Wymaga cech `Zlozyl*` / `Zatwierdzil*` i kodu wspólnego. Program widzi widok po restarcie procesu serwera.

## Status

Baza Claude, 2026-10-22: skrypt gotowy, **niezainstalowany** – kopia bazy przed zmianą nie powstała
(brak miejsca na dysku C:, 0,22 GB wolne).
