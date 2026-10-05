# BlokadaZmianyGodzinDAK (Dodatkowy kod do kompilacji)

Fragment **Dodatkowego kodu do kompilacji** (System → Dodatkowy kod do kompilacji, tabela
`CodeFiles`) z logiką weryfikatorów „Blokada zmiany godzin przez pracownika” (plan i czas
pracy). Reguła, konfiguracja i status testów:
[Blokada zmiany godzin przez pracownika - plan.md](Blokada%20zmiany%20godzin%20przez%20pracownika%20-%20plan.md).

## Gdzie żyje w bazie

Metoda `A1BlokadaGodzin` jest w klasie
`A1.Runtime.KadryPlace.WeryfikatoryKalendarza.TblCodeFiles.A1WeryfikatoryKalendarza`.
To wspólna klasa weryfikatorów kalendarza, w której są też inne metody (np.
`A1OkresZatrudnienia`, `A1WeryfikujNormeWOkresieRozliczeniowym`). Plik w repo zawiera
**tylko** metodę blokady godzin. Przy wgrywaniu trzeba ją wkleić do istniejącej klasy,
nie zastępować całego pliku.

## Metoda

```csharp
public static string A1BlokadaGodzin(Session session, IZrodloPlanu zrodlo, Date data)
```

Wywoływana z obu definicji weryfikatorów:

```csharp
return A1.Runtime.KadryPlace.WeryfikatoryKalendarza.TblCodeFiles.A1WeryfikatoryKalendarza.A1BlokadaGodzin(dzien.Session, dzien.Pozycja.ZrodloPlanu, dzien.Data);
```

Zwraca `null`, gdy nie ma blokady, albo komunikat błędu.

## Uwagi do kodu

- Umowa o pracę: `zrodlo as Pracownik` (pozycja dla umowy cywilnoprawnej ma źródło `Umowa`)
  + `Pracownik.JestZatrudnionyNaEtat(FromTo.Day(data))`.
- Zalogowany pracownik: `Session.Login.WebUserOperatingInstance?.Host as Pracownik`
  (`null` poza pulpitem). Porównanie po `Guid`, bo Host pochodzi z innej sesji.
- Komunikat przez `string.Format`. `TranslateFormat` też zadziała, jeśli plik ma
  `using Soneta.Tools;`, jak reszta klasy `A1WeryfikatoryKalendarza`.
