# Zatwierdzona aktualizacja planu pracy – dokumentacja biznesowa

## 1. Do czego służy cecha

Cecha wyliczana typu **Tak/Nie** na tabeli **Pracownicy**. Zwraca `Tak`, jeśli pracownik ma
w sprawdzanym okresie **zatwierdzony dokument aktualizacji kalendarza** dotyczący **planu pracy**
(pozycja pracownika na dokumencie typu „Aktualizacja planu pracy”, tabela `PozAktKalend`).

Odpowiednik dla drugiego rodzaju dokumentu: [Zatwierdzona aktualizacja czasu pracy](Zatwierdzona%20aktualizacja%20czasu%20pracy.md).

## 2. Jak liczy

1. **Okres sprawdzania** – jeśli kontekst, w którym liczona jest cecha, zawiera okres (`FromTo`,
   np. filtr okresu na liście), brany jest ten okres. W przeciwnym razie – **cały miesiąc daty
   aktualności** (`ActualDate`, np. pole „Aktualność” na liście pracowników).
2. Przeglądane są wszystkie pozycje pracownika na dokumentach aktualizacji kalendarza
   (`PozAktKalend.WgZrodloPlanu[pracownik]`).
3. `Tak`, gdy którykolwiek dokument ma stan **Zatwierdzony** i jego okres **zachodzi** na okres
   sprawdzania (wystarczy częściowe nakładanie się). Dokumenty Wypełniany / Do akceptacji /
   Anulowany są pomijane.

## 3. Konfiguracja w enova365

| Pole | Wartość |
|---|---|
| Tabela | Pracownicy |
| Nazwa | `PlanPracy` (od nazwy zależy nazwa metody `Get_Feature_PlanPracy` w kodzie) |
| Typ | Tak/Nie (bool) |
| Algorytm | Wartość wyliczana algorytmem z parametrami |

Kod z pliku bez rozszerzenia wkleić do edytora algorytmu cechy. Jeśli cecha ma mieć inną nazwę,
trzeba zmienić też nazwę metody w kodzie.

## 4. Status

Kod skompilowany na bibliotekach enova 2512.5.6 – **niesprawdzony na żywych danych** (w bazie
testowej brak dokumentów aktualizacji kalendarza). Do sprawdzenia: czy lista pracowników, na
której będzie używana cecha, przekazuje okres w kontekście – jeśli nie, cecha liczy miesiąc daty
aktualności.
