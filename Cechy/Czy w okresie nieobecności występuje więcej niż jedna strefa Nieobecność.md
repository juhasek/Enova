# Czy w okresie nieobecności występuje więcej niż jedna strefa Nieobecność – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy cecha

Cecha wyliczana (typu Tak/Nie), przypisana do tabeli **Nieobecnosci** (nieobecność pracownika,
`Soneta.Kalend.Nieobecnosc` / `NieobecnośćPracownika`).

Dla konkretnego zapisu nieobecności zwraca `Tak`, jeśli w **którymkolwiek dniu objętym okresem tej
nieobecności** (`Row.Okres`, od–do) na dniu pracy pracownika (`DzienPracy`) zaewidencjonowano
**więcej niż jeden** zapis strefy o nazwie definicji **„Nieobecność”**.

Jest odpowiednikiem cechy
[Czy więcej niż jedna strefa Nieobecność w dniu pracy](Czy%20więcej%20niż%20jedna%20strefa%20Nieobecność%20w%20dniu%20pracy.md)
(przypisanej do tabeli **DniPracy**), ale liczonym z poziomu samej nieobecności — dzięki temu nie
trzeba wchodzić na konkretny dzień pracy, tylko sprawdzić od razu na wpisie nieobecności, czy w jej
okresie taka sytuacja występuje.

## 2. Jak liczy

Cecha wyliczana (kod algorytmu w edytorze cech enova), oparta o zmienną `Row` (bieżący wiersz
tabeli Nieobecnosci, czyli `Nieobecnosc`):

```csharp
public bool Feature_WiecejNizJednaStrefaNieobecnosciWOkresie {
	get {
		for (Date dt = Row.Okres.From; dt <= Row.Okres.To; dt++)
		{
			DzienPracy dzien = Row.Pracownik.DniPracy[dt] as DzienPracy;
			if (dzien == null)
				continue;

			int liczbaStrefNieobecnosci = dzien.Strefy.Count(s => s.Definicja.Nazwa == "Nieobecność");
			if (liczbaStrefNieobecnosci > 1)
				return true;
		}
		return false;
	}
}
```

- Pętla `for (Date dt = Row.Okres.From; dt <= Row.Okres.To; dt++)` przechodzi po wszystkich dniach
  kalendarzowych okresu nieobecności (`Row.Okres`, typ `FromTo`) — ten sam sposób iteracji po
  zakresie dat jest już używany w repozytorium w
  [Weryfikatorze normy okresu rozliczeniowego](../Weryfikatory%20czasu%20pracy/Weryfikator%20normy%20okresu%20rozliczeniowego)
  (`for (Date dt = item.From; dt <= item.To; dt++)`).
- `Row.Pracownik.DniPracy[dt] as DzienPracy` — pobranie zapisu dnia pracy pracownika na daną datę.
  Ten sam sposób dostępu (`pracownik.DniPracy[data] as DzienPracy`) jest już używany w repozytorium
  w [Weryfikatorze wniosku o odbiór dnia wolnego za święto](../Tuple/OdbiórDniaWolnegoZaSwieto).
  Dzień pracy nie musi istnieć dla każdej daty okresu (może nie być jeszcze zainicjowany w
  Kalendarzu/Czasie pracy) — wtedy `dzien` jest `null` i taki dzień jest pomijany (`continue`).
- `dzien.Strefy.Count(s => s.Definicja.Nazwa == "Nieobecność")` — liczba zapisów strefy o nazwie
  definicji `"Nieobecność"` na danym dniu pracy. Ten sam sposób dostępu do stref dnia (`.Strefy`,
  `s.Definicja.Nazwa`) jest używany w cesze
  [Czy więcej niż jedna strefa Nieobecność w dniu pracy](Czy%20więcej%20niż%20jedna%20strefa%20Nieobecność%20w%20dniu%20pracy.md).
- Jeśli którykolwiek dzień okresu ma więcej niż jedną taką strefę (`> 1`), cecha natychmiast zwraca
  `Tak`. Jeśli żaden dzień okresu tego nie spełnia, wynikiem jest `Nie`.

**Do zweryfikowania w środowisku docelowym:**
- Nazwa definicji strefy nieobecności — kod zakłada dokładnie `"Nieobecność"` (analogicznie do
  cechy „Czy więcej niż jedna strefa Nieobecność w dniu pracy”). Jeśli w Twojej konfiguracji nazwa
  jest inna, dopasuj warunek `s.Definicja.Nazwa == "Nieobecność"`.
- Dla nieobecności wieloletnich/wielomiesięcznych pętla dzień po dniu może być kosztowna
  wydajnościowo — jeśli okresy nieobecności bywają bardzo długie, rozważ ograniczenie sprawdzania
  (np. tylko do bieżącego miesiąca/okresu rozliczeniowego) albo wcześniejsze wyjście z pętli po
  znalezieniu pierwszego pasującego dnia (już zaimplementowane przez `return true`).

## 3. Ograniczenia

- Cecha liczy wyłącznie **liczbę zapisów strefy** o danej nazwie na dniu pracy, niezależnie od
  godzin, czasu trwania czy tego, czy strefy się pokrywają/sąsiadują.
- Dni okresu nieobecności bez zainicjowanego zapisu `DzienPracy` (np. dzień jeszcze nie dotknięty w
  Kalendarzu/Czasie pracy) są pomijane — nie liczą się jako `Tak` ani `Nie`, po prostu nie mają
  jeszcze stref do sprawdzenia.
- Cecha nie rozróżnia rodzaju nieobecności bieżącego wpisu (`Row.Definicja`) — sprawdza wyłącznie,
  czy w jej okresie występują dni z wieloma strefami „Nieobecność”, niezależnie od typu tej
  konkretnej nieobecności.
