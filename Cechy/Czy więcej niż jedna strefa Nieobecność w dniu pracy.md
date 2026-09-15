# Czy więcej niż jedna strefa Nieobecność w dniu pracy – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy cecha

Cecha wyliczana (typu Tak/Nie), przypisana do tabeli **DniPracy** (Kalendarz/Czas pracy).
Dla konkretnego dnia pracy zwraca `Tak` tylko wtedy, gdy w tym dniu zaewidencjonowano
**więcej niż jeden** zapis strefy o nazwie definicji **„Nieobecność”**.

Dzień bez żadnej strefy „Nieobecność” albo dokładnie z jedną taką strefą daje wynik `Nie`.
Cecha powstała jako uzupełnienie cechy
[Czy nieobecność w dniu pracy](Czy%20nieobecność%20w%20dniu%20pracy.md) — tamta sprawdza
współwystępowanie nieobecności i czasu pracy w tym samym dniu, ta natomiast wykrywa dni,
na których strefa „Nieobecność” została zaewidencjonowana wielokrotnie (np. przez pomyłkę
przy ręcznym wprowadzaniu czasu pracy albo błąd importu RCP).

## 2. Jak liczy

Cecha wyliczana (kod algorytmu w edytorze cech enova), oparta o zmienną `Row` (bieżący wiersz
tabeli DniPracy, czyli `DzienPracy`):

```csharp
public bool Feature_WiecejNizJednaStrefaNieobecnosci {
	get {
		int liczbaStrefNieobecnosci = Row.Strefy.Count(s => s.Definicja.Nazwa == "Nieobecność");
		return liczbaStrefNieobecnosci > 1;
	}
}
```

- `Row.Strefy` — kolekcja stref (`StrefaPracy`) zaewidencjonowanych na danym dniu pracy. Ten sam
  sposób dostępu do stref dnia jest już używany w repozytorium m.in. w
  [Weryfikatorze wniosku o odbiór dnia wolnego za święto](../Tuple/OdbiórDniaWolnegoZaSwieto)
  (`dzien.Strefy`) oraz w cesze [Nadgodziny okresowe](Nadgodziny%20okresowe.md)
  (`Row.Dzien.Strefy`).
- `s.Definicja.Nazwa` — nazwa definicji strefy przypisanej do wiersza. Cecha zlicza tylko strefy,
  których definicja nosi nazwę dokładnie `"Nieobecność"`.
- Wynik `Tak` jest zwracany, gdy takich stref jest więcej niż jedna (`> 1`).

**Do zweryfikowania w środowisku docelowym:** kod zakłada, że definicja strefy nieobecności w
docelowej bazie nazywa się dokładnie `"Nieobecność"` (analogicznie do stałych nazw stref
używanych w [Nadgodziny okresowe](Nadgodziny%20okresowe.md), np. `"Praca poza normą"`). Jeśli w
Twojej konfiguracji nazwa jest inna albo istnieje kilka wariantów nazw stref nieobecności do
zliczenia, dopasuj warunek `s.Definicja.Nazwa == "Nieobecność"` odpowiednio (np. porównanie
`Contains` albo `HashSet<string>` z dozwolonymi nazwami, jak w
`Tuple/OdbiórDniaWolnegoZaSwieto`).

## 3. Ograniczenia

- Cecha liczy wyłącznie **liczbę zapisów strefy** o danej nazwie, niezależnie od godzin, czasu
  trwania czy tego, czy strefy się pokrywają/sąsiadują — dwie krótkie, rozłączne strefy
  „Nieobecność” w tym samym dniu również dadzą wynik `Tak`.
- Cecha nie rozróżnia rodzaju/powodu nieobecności — sprawdza jedynie nazwę definicji strefy, a nie
  np. konkretny typ nieobecności przypisany do pracownika (`Row.Pracownik.Nieobecnosci`).
