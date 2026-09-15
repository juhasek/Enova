# Czy nieobecność w dniu pracy – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy cecha

Cecha wyliczana (typu Tak/Nie), przypisana do tabeli **DniPracy** (Kalendarz/Czas pracy).
Dla konkretnego dnia pracy zwraca `Tak` tylko wtedy, gdy w tym dniu **jednocześnie**:

- pracownikowi wprowadzono jakąkolwiek nieobecność (np. urlop, zwolnienie lekarskie), **oraz**
- w tym samym dniu zaewidencjonowano też jakiś czas pracy (dzień „mieszany", np. część dnia
  urlopu, część przepracowana).

Dzień z samą nieobecnością (bez godzin pracy) albo z samym czasem pracy (bez nieobecności) daje
wynik `Nie`.

## 2. Jak liczy

Cecha wyliczana (kod algorytmu w edytorze cech enova), oparta o zmienną `Row` (bieżący wiersz
tabeli DniPracy, czyli `DzienPracy`):

```csharp
public bool Feature_SprawdzenieNieobecnosci {
	get {
		bool jestNieobecnosc = Row.Pracownik.Nieobecnosci.Any(n => n.Okres.Contains(Row.Data));
		bool jestCzasPracy = Row.Czas > Time.Zero;
		return jestNieobecnosc && jestCzasPracy;
	}
}
```

- `jestNieobecnosc` — sprawdza, czy wśród nieobecności przypisanych do pracownika
  (`Row.Pracownik.Nieobecnosci`) istnieje taka, której okres (`Okres`) obejmuje datę bieżącego dnia
  (`Row.Data`). Ten sam sposób sprawdzania nieobecności na konkretny dzień jest już używany w
  repozytorium w [Weryfikatorze wniosku o odbiór dnia wolnego za święto](../Tuple/OdbiórDniaWolnegoZaSwieto)
  (metoda `CzyJestNieobecnosc`) — cecha celowo powiela tę samą logikę, żeby wynik był spójny
  niezależnie od tego, w którym miejscu systemu jest liczony.
- `jestCzasPracy` — sprawdza, czy zagregowany czas pracy zapisany na wierszu DniPracy (`Row.Czas`,
  suma stref pracy dnia) jest większy od zera.

**Do zweryfikowania w środowisku docelowym:** kod zakłada, że `DzienPracy` udostępnia zagregowane
pole `Czas` (analogicznie do `DzienPlanu.Czas`, używanego w tym repo w
`Weryfikatory czasu pracy/Weryfikator normy okresu rozliczeniowego`). Jeśli w Twojej wersji enova
pole nazywa się inaczej albo trzeba je liczyć jako sumę `Row.Strefy.Sum(s => s.Czas)`, popraw
`jestCzasPracy` odpowiednio — reszta kodu się nie zmienia.

## 3. Ograniczenia

- Cecha nie rozróżnia rodzaju nieobecności (urlop, L4, opieka itd.) — zwraca tylko `Tak`/`Nie`.
  Jeśli potrzebny jest konkretny typ nieobecności, trzeba sięgnąć bezpośrednio do
  `Row.Pracownik.Nieobecnosci` i pola `Definicja`.
- Nieobecność jest sprawdzana na poziomie całego dnia (`Okres.Contains(Data)`), a nie godzinowo —
  nieobecność obejmująca tylko część dnia (np. wyjście prywatne na 2 godziny) liczy się tak samo
  jak nieobecność całodniowa.
