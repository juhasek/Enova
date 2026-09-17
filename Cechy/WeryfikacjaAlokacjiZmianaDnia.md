# WeryfikacjaAlokacjiZmianaDnia (cecha KartaCzasuPracy)

Cecha (kod C#) na tabeli **DniPracy**, kategoria `KartaCzasuPracy`. Zwraca
`string` — komunikat walidacyjny (pusty gdy brak błędu), wyświetlany w PL/EN
wg `PracownikWebUserInfoWorker.UICulture`.

- **ID w bazie Claude:** `17`
- **GUID:** `E2DDC390-27A1-4E66-BBD9-1385FFDE0349`
- **Zweryfikowano na żywo:** Nie — kod przeniesiony z bazy do repo po analizie
  i korektach na życzenie klienta, bez testu w GUI.

Niemal identyczny duplikat kodu z [[WeryfikacjaAlokacjiZapis]] (ID 18) —
różnią się tylko nazwą metody i drobną kolejnością instrukcji. Nazwy sugerują
dwa różne triggery w UI (zmiana dnia w widoku vs zapis rekordu).

## Logika

1. Jeśli `Row.Features.GetBool("Zatwierdzono")` → `""` (dzień zatwierdzony,
   bez walidacji).
2. Liczy normę dnia (`Pracownik.Czasy.Norma`) + czas ze stref z cechami
   `ZwiekszajCzas`/`PomniejszajCzas` na definicji strefy + czas nieobecności
   (`Pracownik.Czasy.NormaNie`).
3. Jeśli zaalokowany czas ≠ norma dnia → komunikat o niezgodności z normą.
4. Jeśli strefa „Przerwa bezpłatna" > 0:30 → komunikat o przekroczeniu
   przerwy.
5. Sprawdzenie godziny rozpoczęcia pracy (`Row.OdGodziny`):
   - **dzień zmodyfikowany w planie** (`DzienPlanu` istnieje dla tej daty) —
     porównanie **na równość** z `dzienPlanu.OdGodziny`. Każda inna godzina
     niż wpisana w polu „Rozpoczęcie pracy pomiędzy" tego konkretnego dnia
     → błąd, komunikat pokazuje tylko tę jedną godzinę (bez przedziału).
   - **dzień niemodyfikowany** (brak indywidualnego `DzienPlanu`) —
     porównanie z przedziałem `dzien.Definicja.Praca.OdGodziny` –
     `dzien.Definicja.WejścieDo` (pole „Rozpoczęcie pracy pomiędzy" na
     wzorcu dnia — `WejścieDo = OdGodziny + TolerancjaWe`, potwierdzone
     dekompilacją `Soneta.KadryPlace.dll`/`.UI.dll`).
6. Jeśli `DzienPracy` nie istnieje dla tej daty → `""`.

## Historia ustaleń z klientem

- Pierwotny komunikat dla dnia zmodyfikowanego pokazywał przedział
  (`{OdGodziny} - {WejścieDo}`) — na życzenie klienta zmieniono na
  pokazywanie **tylko** `OdGodziny`.
- Warunek dla dnia zmodyfikowanego pierwotnie sprawdzał przedział
  (`> WejścieDo || < OdGodziny`, tj. tolerancja `TolerancjaWe`) — na
  życzenie klienta zmieniono na **ścisłą równość** z `OdGodziny`: dowolna
  godzina inna niż wpisana w „Rozpoczęcie pracy pomiędzy" (dolna granica)
  dla tego dnia jest błędem, tolerancja z przedziału już się nie liczy.
- Gałąź dla dnia niemodyfikowanego (przedział na wzorcu dnia) pozostaje bez
  zmian — tam nadal obowiązuje przedział.

## Ograniczenia edytora skryptów (patrz pamięć repo)

Cała logika w jednej metodzie (właściwość `get`), zgodnie z ograniczeniami
wbudowanego edytora skryptów enova (brak `int?`/`Nullable<T>`, zawodne
metody pomocnicze).
