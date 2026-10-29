# Metryczka – NazwaKlienta

Cecha wyliczana (tekst) na tabeli **Pracownicy**, kategoria „Metryczka klienta”. Zwraca wartość pola
**Nazwa klienta** z metryczki (Narzędzia → Opcje → A1Testy → Metryczka klienta). Wartość jest jedna dla całej bazy,
cecha udostępnia ją na listach, wydrukach i w kodzie kadrowo-płacowym.

## Jak liczy

Pole metryczki to węzeł konfiguracji `A1Testy → Metryczka klienta → NazwaKlienta`, wartość w atrybucie `Wartosc`
(dodatek `Addon/MetryczkaKlienta`). Klucz `NazwaKlienta` nie zmienia się przy zmianie nazwy pola w Opcjach.
Puste pole albo brak dodatku/węzła → pusty tekst. Kod nie wymaga referencji do DLL dodatku.

Inne pole = ten sam kod z innym kluczem (lista kluczy: `Addon/MetryczkaKlienta/README.md`, sekcja
„Odczyt pola metryczki w cesze / kodzie”).

## Instalacja

`dbmgr importxml <baza> "Metryczka - NazwaKlienta.dbinit.xml"` (definicja z GUID – ponowny import aktualizuje).

## Test (baza Claude, 29.10.2026)

| Krok | Wynik |
|---|---|
| Import, kompilacja bazy | OK (FeatureDefs ID 2165) |
| Lista pracowników, kolumna cechy przy pustym polu | OK – pusto |
| Wartość po wpisaniu nazwy klienta w Opcjach | do sprawdzenia |
