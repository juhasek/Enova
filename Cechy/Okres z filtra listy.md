# Okres z filtra listy – dokumentacja biznesowa

## 1. Do czego służy cecha

Cecha wyliczana typu **Tekst** na tabeli **Pracownicy**. Zwraca okres wybrany w filtrze
„Okres” listy pracowników, na której jest wyświetlana (np. `2026-10-01...2026-10-31`).
Przydaje się jako kolumna kontrolna widoku albo w wydrukach/eksportach, żeby było widać,
za jaki okres liczone są pozostałe kolumny.

## 2. Jak liczy

1. Odczytuje z kontekstu listy obiekt okresu (`FromTo`) – lista wkłada go tam, gdy widok ma
   filtr okresu.
2. Zwraca okres w formacie enova `rrrr-MM-dd...rrrr-MM-dd`.
3. Gdy w kontekście nie ma okresu (widok bez filtra okresu, np. lista z polem „Aktualność”
   – to jedna data, nie okres) albo okres jest pusty – zwraca pusty tekst.

Ten sam sposób odczytu okresu stosują cechy
[Zatwierdzona aktualizacja planu pracy](Zatwierdzona%20aktualizacja%20planu%20pracy.md)
i [Zatwierdzona aktualizacja czasu pracy](Zatwierdzona%20aktualizacja%20czasu%20pracy.md).

## 3. Konfiguracja w enova365

| Pole | Wartość |
|---|---|
| Tabela | Pracownicy |
| Nazwa | `OkresFiltra` (od nazwy zależy nazwa metody `Get_Feature_OkresFiltra` w kodzie) |
| Typ | Tekst |
| Algorytm | Wartość wyliczana algorytmem z parametrami |

Kod z pliku bez rozszerzenia wkleić do edytora algorytmu cechy. Jeśli cecha ma mieć inną
nazwę, trzeba zmienić też nazwę metody w kodzie.

## 4. Status

Kod przygotowany 2026-10-15, **niezałożony w żadnej bazie i niesprawdzony na żywo**.
Do sprawdzenia: dodać cechę jako kolumnę na liście pracowników z filtrem okresu, zmienić
okres w filtrze i odświeżyć listę – kolumna powinna pokazać nowy okres.
