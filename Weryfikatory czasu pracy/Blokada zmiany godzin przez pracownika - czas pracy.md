# Blokada zmiany godzin przez pracownika – czas pracy

Weryfikator kalendarza rodzaju **`DzienPracyAktualizacja`** (dzień czasu pracy na dokumencie
aktualizacji kalendarza). Bliźniak weryfikatora dla planu pracy: ta sama reguła (tylko
pracownicy na umowę o pracę), inny typ wiersza (`Soneta.Kalend.DzienPracyAktualizacja`).

Definicja tylko wywołuje `BlokadaZmianyGodzinDAK.Weryfikuj(source)` z Dodatkowego kodu do
kompilacji (plik `BlokadaZmianyGodzinDAK`).

Pełny opis (reguła, konfiguracja, status testów) jest w
[Blokada zmiany godzin przez pracownika - plan.md](Blokada%20zmiany%20godzin%20przez%20pracownika%20-%20plan.md).

Potrzebny tylko wtedy, gdy na pulpicie używane są dokumenty aktualizacji **czasu pracy**.
