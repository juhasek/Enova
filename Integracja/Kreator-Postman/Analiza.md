# Kreator Postman: metody kadrowe WebAPI

Narzędzie do budowania żądań kadrowych (zatrudnienie, zwolnienie, przedłużenie umowy, zmiana danych)
i kolekcji Postman. Podstawa: dokumentacja WebAPI v. 5.1 z `AltOne.WCF.Core` 2606.0.1
oraz testy na serwerze lokalnym 2610 (baza `Integracja`).

## Jak używać

1. Otwórz [Kreator.html](Kreator.html) w przeglądarce (działa bez serwera i bez internetu).
2. Wpisz polecenie, np. `zatrudnij Jan Testowy, kod API-01, od 2026-11-01 do 2027-10-31, na czas określony,
   stanowisko Specjalista, wydział ADM, 1/1`, i kliknij **Uzupełnij pola**.
3. Popraw pola, kliknij **Dodaj do kolekcji**, potem **Pobierz kolekcję** i **Pobierz środowisko**.
4. W Postmanie: Import obu plików, wybór środowiska, uzupełnienie zmiennych.

Gotowy pakiet 29 scenariuszy: [Postman/](Postman/), arkusz wyników:
[Scenariusze testowe.xlsx](Scenariusze%20testowe.xlsx).

Regeneracja po zmianie `Kreator.html` (testy logiki + pliki Postman + lista scenariuszy, potem arkusz):

```
node Generator\generuj.js
cd Generator
dotnet build -c Release -o bin\out
dotnet bin\out\GenArkusz.dll ..
```

## Zmienne środowiska

| Zmienna | Znaczenie |
|---|---|
| `baseUrl` | Adres WebAPI bez prefiksu, np. `http://localhost:9010` |
| `testMode` | `true`: tryb próbny, serwer ma nie zapisywać zmian. Zmień na `false` dopiero po ocenie wyników. |
| `appToken` | Token aplikacji (tylko Dynamic API) |
| `sessionToken` | Ustawia go żądanie Logowanie |
| `database`, `operator`, `password` | Tylko MethodInvoker |
| `stanowisko`, `wydzial` | Wartości z bazy, na której testujesz. `wydzial` to kod wydziału. |

Uzupełnionego środowiska nie commitować.

## Sposoby wywołania

**Dynamic API** (wg dokumentacji). Dokumentacja nie opisuje logowania. Z kodu `webapi.dll` 2610 wynika:

1. W enova utwórz token aplikacji typu WebApi (lista tokenów w **Narzędzia → Opcje**), dodaj do niego usługi
   `KadryPlaceWebAPI` i `OperationFunctionsWebAPI`, użyj **Generuj token**. Serwer odrzuca wywołanie kontrolera
   spoza listy na tokenie.
2. `POST /api/LoginApi` z tokenem aplikacji jako Bearer zwraca `Token` (token sesji).
3. Pozostałe wywołania idą z tokenem sesji. Token aplikacji nie działa na kontrolerach.
4. Sesja wygasa po 5 minutach bezczynności.

**MethodInvoker** (`POST /api/MethodInvoker/InvokeServiceMethod`): wymaga `WebApi:LegacyControllers:MethodInvoker = true`
i operatora z ustawionym hasłem. `ServiceName` = `AltOne.WCF.Core.Interfaces.IKadryPlace2, AltOne.WCF.Core`,
parametr `pars`. Metoda jest oznaczona przez enova jako przestarzała.

## Co ustalono

| Ustalenie | Podstawa |
|---|---|
| Zatrudnienie, zwolnienie, przedłużenie i zmiana danych to jedna metoda: `KadryPlaceWebAPI/UpdateEmployee` | dokumentacja |
| Schemat wymaga tylko `Imie` i `Nazwisko`, także przy aktualizacji | dokumentacja |
| `JednostkaOrg` to kod wydziału; nieznany kod daje błąd | komunikat w bibliotece |
| Enumy idą jako tekst z polskimi znakami (`NaCzasOkreślony`), daty umowy jako `{Year,Month,Day}`, `DataAktualizacji` jako data z godziną | dokumentacja, test na serwerze |
| Wszystkie 29 żądań pakietu przechodzi przez prawdziwe DTO; zły enum i zła data są odrzucane | test na serwerze 2610 |
| Nieznane pole w JSON nie powoduje błędu: literówka w nazwie pola przejdzie po cichu | test na serwerze 2610 |
| `TestMode` to tryb próbny: biblioteka ma komunikat o braku trwałych zmian | komunikat w bibliotece |
| Cecha musi mieć włączony dostęp przez API; cecha historyczna wymaga `UpdateDate` | dokumentacja, komunikat w bibliotece |

## Czego nie ustalono

Lokalnie `UpdateEmployee` zatrzymuje się na kontroli licencji dodatku: baza `Integracja` nie ma licencji
`AltOne.WCF.Core` („Do testów modułu wymagana jest czasowa licencja Demo"). Kod biblioteki jest zaszyfrowany.
Dlatego poniższe wymaga testu na serwerze z licencją (kolumna „Podstawa” = hipoteza w arkuszu):

- **Faktyczne minimum danych.** Sondy M-01…M-06 dodają pola po kolei. Pierwsza z `Success = true` wyznacza minimum.
- **Zwolnienie.** Jest tylko pole `Zwolniony`. DTO nie ma przyczyny, trybu, kodu zwolnienia ani wyrejestrowania ZUS,
  a dokumentacja nie opisuje skutku tego pola (R-01, R-02).
- **Przedłużenie.** Nie ma osobnej metody ani opisu; kreator zakłada `Aktualizuj = true` + `DataAktualizacji` +
  nowy `OkresDo` (P-01). Niewiadoma: czy brak `OkresDo` czyści datę końca (P-03).
- **Częściowa aktualizacja.** Czy pominięte pola są czyszczone (C-02).
- **Zwolnienie po aktualizacji.** W poprzednim dodatku WebAPI aktualizacja cofała rozwiązanie umowy; tu sprawdza to R-03.
- **Wynagrodzenie.** `ZatrudnienieDTO` nie ma stawki ani zaszeregowania. `UpsertDodatkiPracownika` obsługuje tylko dodatki.

## Rozbieżności w dokumentacji

- Wstęp pokazuje `UpdateDate` cechy jako `{Rok,Miesiac,Dzien}`, a schemat `DataDTO` jako `{Year,Month,Day}`.
  Kreator używa schematu.
- Lista bibliotek zawiera `AltOne.WebAPI.ObslugaKadrowa`, której nie ma w dostarczonych plikach
  (dotyczy dodatków, podzielników, dokumentów dodatkowych; kreator z niej nie korzysta).
- Dostarczony `AltOne.WCF.Core` to wersja `2606.0.1.80200-Alfa`, a dokumentacja opisuje `2606.0.1.80001`.
  Biblioteka zawiera też tekst „Biblioteka przeznaczona tylko na środowisko testowe".
