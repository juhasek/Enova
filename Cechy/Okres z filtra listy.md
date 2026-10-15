# Okres z filtra listy – dokumentacja biznesowa

## 1. Do czego służy cecha

Cecha wyliczana typu **Tekst** na tabeli **Pracownicy**. Zwraca okres wybrany w filtrze
„Okres” listy pracowników, na której jest wyświetlana (np. `1...31.10.2026`).
Przydaje się jako kolumna kontrolna widoku albo w wydrukach/eksportach, żeby było widać,
za jaki okres liczone są pozostałe kolumny.

## 2. Jak liczy

1. Szuka w kontekście obiektu parametrów listy dziedziczącego po `OkresContext` i bierze jego
   pole `Okres`. Tak działają listy kadrowe: np. Kadry → Pracownicy wkłada do kontekstu
   `KadryPracownicyViewInfo.PrParams` (→ `StdParams` → `OkresContext`), a sam okres trzyma
   w polu tego obiektu – **nie** jako osobny `FromTo` w kontekście. Kontekst enova znajduje
   obiekt także po typie bazowym (`Context.FindInterface` → `IsAssignableFrom`).
2. Gdy takiego obiektu nie ma – szuka w kontekście samego okresu (`FromTo`), który wkładają
   niektóre inne widoki.
3. Zwraca okres w formacie enova, skróconym jak w filtrze, np. `1...31.10.2026`,
   `1.07.2026...30.09.2026`.
4. Gdy okresu nie ma (widok bez filtra okresu) albo jest pusty – zwraca pusty tekst.

Uwaga: cechy [Zatwierdzona aktualizacja planu pracy](Zatwierdzona%20aktualizacja%20planu%20pracy.md)
i [Zatwierdzona aktualizacja czasu pracy](Zatwierdzona%20aktualizacja%20czasu%20pracy.md) szukają
tylko `FromTo` w kontekście, więc na liście Kadry → Pracownicy nie widzą okresu z filtra
i liczą miesiąc daty aktualności.

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

- Baza Al (2026-10-15): cecha założona przez GUI (Ustawienia → Kadry i płace → Cechy →
  Pracownicy), kompiluje się.
- **Sprawdzona na liście Kadry i płace → Kadry → Pracownicy**: przy domyślnym filtrze okresu
  cecha zwraca `1...31.10.2026`, po zmianie filtra na 1.07–30.09.2026 zwraca
  `1.07.2026...30.09.2026` (ta sama wartość dla każdego pracownika na liście).
