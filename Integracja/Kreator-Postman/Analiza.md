# Kreator Postman: metody kadrowe WebAPI

Narzędzie do budowania żądań kadrowych (zatrudnienie, zwolnienie, przedłużenie umowy, zmiana danych)
i kolekcji Postman. Podstawa: dokumentacja WebAPI v. 5.1 oraz testy 30.10.2026 na serwerze 2610, baza `Integracja`,
`AltOne.WCF.Core` 2606.0.1, wywołania przez MethodInvoker.

## Jak używać

1. Otwórz [Kreator.html](Kreator.html) w przeglądarce (działa bez serwera i bez internetu).
2. Wpisz polecenie, np. `zatrudnij Jan Testowy, kod API-01, od 2026-11-01 do 2027-10-31, na czas określony,
   stanowisko Specjalista, wydział ADM, 1/1`, i kliknij **Uzupełnij pola**.
3. Popraw pola, kliknij **Dodaj do kolekcji**, potem **Pobierz kolekcję** i **Pobierz środowisko**.
4. W Postmanie: Import obu plików, wybór środowiska, uzupełnienie zmiennych.

Gotowy pakiet 34 scenariuszy: [Postman/](Postman/). Arkusz z wynikami testów:
[Scenariusze testowe.xlsx](Scenariusze%20testowe.xlsx).

Regeneracja po zmianie `Kreator.html` (testy logiki + pliki Postman + lista scenariuszy, potem arkusz):

```
node Generator\generuj.js
cd Generator
dotnet build -c Release -o bin\out
dotnet bin\out\GenArkusz.dll ..
```

Arkusz bierze wyniki z `Generator/wyniki.json` (przebieg newman z 30.10.2026).

## Zmienne środowiska

| Zmienna | Znaczenie |
|---|---|
| `baseUrl` | Adres WebAPI bez prefiksu, np. `http://localhost:9010` |
| `appToken` | Token aplikacji (tylko Dynamic API) |
| `sessionToken` | Ustawia go żądanie Logowanie |
| `database`, `operator`, `password` | Tylko MethodInvoker |

Uzupełnionego środowiska nie commitować.

Dane żądań (stanowisko, wydział, `TestMode` itd.) są wpisane wprost w każde żądanie, bo klient wywołuje API poza Postmanem.
Zmienne dotyczą tylko połączenia. Na innej bazie podmień `Stanowisko`, `JednostkaOrg` i `NAZWA_CECHY` (Z-06).

`TestMode = true`: serwer sprawdza dane i wycofuje zapis. Grupy 1, 2 i 7 są próbne. **Grupy 3–6 mają `TestMode = false` i zapisują dane**,
bo przedłużenia i zwolnienia działają na pracownikach założonych w grupie 3: uruchamiaj je tylko na bazie testowej.
W kreatorze tryb próbny jest domyślnie włączony (pole wyboru).

## Sposoby wywołania

**MethodInvoker** (`POST /api/MethodInvoker/InvokeServiceMethod`, sprawdzone): wymaga
`WebApi:LegacyControllers:MethodInvoker = true` i operatora z hasłem. `ServiceName` =
`AltOne.WCF.Core.Interfaces.IKadryPlace2, AltOne.WCF.Core`, parametr `pars`. Enova oznacza tę metodę jako przestarzałą.

Operator do API potrzebuje dokładnie jednej roli i włączonego `AllowEnova365` (dostęp do wersji multi).

**Dynamic API** (wg dokumentacji, nieprzetestowane end-to-end). Logowania dokumentacja nie opisuje. Z kodu `webapi.dll` 2610:

1. W enova utwórz token aplikacji typu WebApi (lista tokenów w **Narzędzia → Opcje**), dodaj usługi
   `KadryPlaceWebAPI` i `OperationFunctionsWebAPI`, użyj **Generuj token**.
2. `POST /api/LoginApi` z tokenem aplikacji jako Bearer zwraca `Token` (token sesji).
3. Pozostałe wywołania idą z tokenem sesji. Sesja wygasa po 5 minutach bezczynności.

Licencja: oprócz `AltOne.Integracje` (dla `WCF.Core`) potrzebny jest moduł `Soneta.WebApi` albo `Soneta.HarmZadan`.
Tryb demo pomija obie kontrole.

## Minimalne dane (potwierdzone)

| Cel | Wymagane pola |
|---|---|
| Kartoteka bez umowy | `Imie`, `Nazwisko` |
| Umowa | jak wyżej + `Zatrudnienie`: `OkresOd`, `OkresDo`, `Stanowisko`, `JednostkaOrg` (kod wydziału) |
| Czas nieokreślony | jak umowa + `TypUmowy`, a `OkresDo` = `{9999,12,31}` (w bazie 2099-12-31) |
| Przedłużenie | `Kod`, `Imie`, `Nazwisko`, `Aktualizuj: true`, `DataAktualizacji`, `Zatrudnienie.OkresDo` |
| Zwolnienie | jak przedłużenie + `Zatrudnienie.Zwolniony: true` |
| Zmiana danych | `Kod`, `Imie`, `Nazwisko`, `Aktualizuj: true`, `DataAktualizacji` + zmieniane pola |

Bez `OkresDo` serwer zwraca „Błędny zakres dat”, także dla umowy na czas nieokreślony. Bez `TypUmowy` i `Wymiar` zapis
przechodzi. Schemat w dokumentacji tych wymagań nie pokazuje: są to reguły enova na etacie.

## Zachowanie (potwierdzone)

- **`Kod`:** brak kodu albo kod spoza bazy zakłada nową kartotekę. Enova nadaje numer zgodnie ze schematem albo przepisuje kod z żądania.
  Kod istniejący aktualizuje kartotekę.
- **`TestMode`** działa: `Success` i komunikaty jak przy zapisie, w bazie bez zmian. Ponieważ zapis jest wycofywany,
  aktualizacje nie widzą pracownika założonego wcześniejszym żądaniem próbnym.
- **Przedłużenie:** powstaje nowy zapis historii od `DataAktualizacji`. Wcześniejszy zapis zachowuje stary koniec umowy.
- **Nowa umowa po przerwie:** `OkresOd` + `OkresDo` w aktualizacji działają, stara umowa zostaje w historii.
- **Częściowa aktualizacja:** pominięte pola i całe sekcje zostają bez zmian.
- **Zmiana z datą wsteczną** nie nadpisuje późniejszych zapisów historii (inaczej niż w poprzednim dodatku WebAPI). Późniejszy
  zapis zachowuje stare wartości, więc aneks działa tylko do następnej zmiany.
- **Bez `Aktualizuj`** zmiana nadpisuje bieżący zapis historii, bez nowego wpisu.
- **Zwolnienie:** `OkresDo` kończy etat, a `Zwolniony: true` ustawia znacznik od `DataAktualizacji`. Bez `Zwolniony`
  etat też się kończy, ale znacznik zostaje `false`.

## Pułapki

1. **Zmiana po końcu umowy tworzy zapis bez etatu** (R-03). Zwolnienie zostaje w historii, ale nowy zapis ma puste
   zatrudnienie i `Zwolniony = false`.
2. **API nie zapisuje przyczyny rozwiązania ani kodu zwolnienia.** Pola zostają puste, DTO ich nie ma. Nie sprawdzano
   wyrejestrowania ZUS.
3. **Nieznane pole w JSON nie powoduje błędu**, więc literówka w nazwie pola przejdzie po cichu. Cecha bez nazwy też (N-04).
4. **`ZatrudnienieDTO` nie ma stawki ani zaszeregowania.** `UpsertDodatkiPracownika` obsługuje tylko dodatki.

## Nieustalone

- **Cechy (pola dodatkowe):** baza demo nie ma definicji cechy pracownika, więc zapisu cech nie sprawdzono (Z-06).
  Potwierdzono tylko błąd braku definicji (N-05). Plik testów klienta był pusty, lista pól nie jest znana.
- **Dynamic API:** pełne logowanie tokenem nieprzetestowane.
- **Serwer 2606.2.3:** testy przeszły na 2610.

## Rozbieżności w dokumentacji

- Wstęp pokazuje `UpdateDate` cechy jako `{Rok,Miesiac,Dzien}`, a schemat `DataDTO` jako `{Year,Month,Day}`.
  Kreator używa schematu.
- Lista bibliotek zawiera `AltOne.WebAPI.ObslugaKadrowa`, której nie ma w dostarczonych plikach
  (dodatki, podzielniki, dokumenty dodatkowe; kreator z niej nie korzysta).
- Dostarczony `AltOne.WCF.Core` to wersja `2606.0.1.80200-Alfa`, a dokumentacja opisuje `2606.0.1.80001`.
  Biblioteka zawiera też tekst „Biblioteka przeznaczona tylko na środowisko testowe".
