# Zatwierdzona aktualizacja planu pracy – dokumentacja biznesowa

## 1. Do czego służy cecha

Cecha wyliczana typu **Tak/Nie** na tabeli **Pracownicy**. Zwraca `Tak`, jeśli pracownik ma
w sprawdzanym okresie **zatwierdzony dokument aktualizacji kalendarza** dotyczący **planu pracy**
(pozycja pracownika na dokumencie typu „Aktualizacja planu pracy”, tabela `PozAktKalend`).

Odpowiednik dla drugiego rodzaju dokumentu: [Zatwierdzona aktualizacja czasu pracy](Zatwierdzona%20aktualizacja%20czasu%20pracy.md).

## 2. Jak liczy

1. **Okres sprawdzania** – okres wybrany w filtrze listy. Listy kadrowe (np. Kadry → Pracownicy)
   trzymają go w obiekcie parametrów widoku dziedziczącym po `OkresContext` (cecha czyta go przez
   `cx.Get<OkresContext>(out ...)`); inne widoki mogą wkładać do kontekstu sam okres (`FromTo`).
   Gdy okresu nie ma – **cały miesiąc daty aktualności** (`ActualDate`).
2. Przeglądane są wszystkie pozycje pracownika na dokumentach aktualizacji kalendarza
   (`PozAktKalend.WgZrodloPlanu[pracownik]`).
3. `Tak`, gdy którykolwiek dokument ma stan **Zatwierdzony** i jego okres **zachodzi** na okres
   sprawdzania (wystarczy częściowe nakładanie się). Dokumenty Wypełniany / Do akceptacji /
   Anulowany są pomijane.

## 3. Konfiguracja w enova365

| Pole | Wartość |
|---|---|
| Tabela | Pracownicy |
| Nazwa | `PlanPracyZatwierdzony` (od nazwy zależy nazwa metody `Get_Feature_PlanPracyZatwierdzony` w kodzie) |
| Typ | Tak/Nie (bool) |
| Algorytm | Wartość wyliczana algorytmem z parametrami |

Kod z pliku bez rozszerzenia wkleić do edytora algorytmu cechy. Jeśli cecha ma mieć inną nazwę,
trzeba zmienić też nazwę metody w kodzie.

## 4. Status

Kod skompilowany na bibliotekach enova 2512.5.6 – **niesprawdzony na żywych danych** (w bazie
testowej brak dokumentów aktualizacji kalendarza). 2026-10-15: odczyt okresu przerobiony na `cx.Get<T>(out ...)` (bez zmiennych `object`) i rozszerzony
o parametry listy (`OkresContext`) – wcześniej na liście Kadry → Pracownicy cecha nie widziała
okresu z filtra i liczyła miesiąc daty aktualności. Ten sam odczyt sprawdzony na żywo w cesze
[Okres z filtra listy](Okres%20z%20filtra%20listy.md) (baza Al). Baza Claude (2026-10-15): nowa wersja założona jako `PlanPracyZatwierdzony` (kategoria „Aktualizacja
kalendarza”) **obok** starej roboczej cechy `PlanPracy`, której nie ruszano. Kompiluje się i liczy
na liście Kadry → Pracownicy z filtrem okresu (wszędzie „Nie” – w bazie brak dokumentów
aktualizacji kalendarza).
