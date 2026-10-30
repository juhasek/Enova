# Kreator Postman: metody kadrowe dodatku klienta

Narzędzie do budowania żądań kadrowych (zatrudnienie, zwolnienie, przedłużenie umowy, zmiana danych)
i kolekcji Postman. Klient korzysta z API dodatku klienta: `IStaff.UpsertEmployee` i `IStaff.UpsertTermination`.
Podstawa: kod dodatku w wersji 2512.9.11 oraz testy 30.10.2026 na serwerze 2610, baza `Integracja`, wywołania przez MethodInvoker.

## Jak używać

1. Otwórz [Kreator.html](Kreator.html) w przeglądarce (działa bez serwera i bez internetu).
2. Wpisz polecenie, np. `zatrudnij Jan Testowy, kod API-01, od 2026-11-01 do 2027-10-31, na czas określony,
   stanowisko Specjalista, wydział ADM, 1/1`, i kliknij **Uzupełnij pola**.
3. Popraw pola, kliknij **Dodaj do kolekcji**, potem **Pobierz kolekcję** i **Pobierz środowisko**.
4. W Postmanie: Import obu plików, wybór środowiska, uzupełnienie zmiennych.

Gotowy pakiet 47 scenariuszy: [Postman/](Postman/). Arkusz z wynikami testów:
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
| `database`, `operator`, `password` | Logowanie MethodInvoker |
| `serviceStaff` | Pełna nazwa usługi `IStaff` dodatku |
| `serviceWydzialy` | Pełna nazwa usługi `IStrukturaWydzialowa` dodatku |

Uzupełnionego środowiska nie commitować. Nazwa dodatku jest zmienną, żeby nie trafiła do repo.

Dane żądań są wpisane wprost w każde żądanie, bo klient wywołuje API poza Postmanem. Na innej bazie podmień `Stanowisko`,
`JednostkaOrg` i przyczynę rozwiązania.

`TestMode = true`: serwer sprawdza dane i wycofuje zapis. Grupy 1, 2 i 8 są próbne. **Grupy 3–7 mają `TestMode = false` i zapisują dane**,
bo pracują na pracownikach z grupy 3: uruchamiaj je tylko na bazie testowej i tylko na pustych kodach. W kreatorze tryb próbny jest domyślnie włączony.

## Wymagania po stronie enova

- Dodatek dostępny tylko przez MethodInvoker: `WebApi:LegacyControllers:MethodInvoker = true`.
- Licencja: `AltOne.Integracje` oraz `Soneta.WebApi` albo `Soneta.HarmZadan`.
- Operator: dokładnie jedna rola i włączone `AllowEnova365`.
- Cechy: `Numer koncernowy` (tabela Pracownicy) i `Typ pracownika` (tabela PracHistorie), typ tekstowy, przechowywane w bazie (algorytm `DB`).
  Dodatek zapisuje je bezpośrednio, więc nie potrzebują flagi dostępu przez WebAPI.
- Definicje stanowisk: `Zatrudnienie.Stanowisko` to **nazwa definicji**, nie wolny tekst.
- Przyczyny rozwiązania: `TerminationReason` to nazwa ze słownika przyczyn.

## Minimalne dane (potwierdzone)

Atrybuty `Required` w DTO nie są egzekwowane przy wywołaniu przez MethodInvoker. O wymaganiach decyduje enova:

| Cel | Wymagane pola |
|---|---|
| Kartoteka bez umowy | `Imie`, `Nazwisko` |
| Umowa | + `Zatrudnienie`: `OkresZatrudnieniaOd`, `JednostkaOrg`, `TypUmowy`, `Stanowisko` |
| Umowa na czas określony | + `OkresZatrudnieniaDo` |
| Przedłużenie | `Kod`, `DataAktualizacji.From`, `Zatrudnienie`: `OkresZatrudnieniaOd`, nowy `OkresZatrudnieniaDo`, `TypUmowy` |
| Zmiana danych | `Kod`, `DataAktualizacji.From` + zmieniane pola; przy sekcji `Zatrudnienie` także `TypUmowy` |
| Zmiana rachunku | `Kod`, `DataAktualizacji.From`, `Rachunki: [{ Numer, Domyslne: true }]` |
| Zwolnienie | `UpsertTermination`: `PracownikKod`, `TerminationDate`, `OkresUmowyOd`, `TerminationReason` |

`pars` to pojedynczy obiekt. `Kod` jest opcjonalny przy zatrudnieniu: bez niego enova nadaje numer zgodnie ze schematem,
a kod spoza bazy trafia do nowej kartoteki. Aktualizacja wymaga kodu z bazy. Czas nieokreślony: kreator wysyła `OkresZatrudnieniaDo` = 9999-12-31.

**Różnica względem `UpdateEmployee`:** w dodatku zmiana rachunku też wymaga daty i tworzy zapis historii. Bez `DataAktualizacji.From`
wynik to „No employee updated/added”. Nie ma tu pola `Aktualizuj`: aktualizację wyznacza istniejący `Kod`.

## Ryzyka (potwierdzone testami)

1. **Aktualizacja z sekcją `Zatrudnienie` cofa zwolnienie** (R-05). Czyści dane rozwiązania i nadpisuje okres etatu. Zwolnienie znika bez komunikatu.
2. **Zmiana bez `DataAktualizacji.To` nadpisuje późniejsze zapisy historii** (C-07, S-01). Zmiana stanowiska z datą wsteczną cofnęła przedłużenie
   umowy, gdy `OkresZatrudnieniaDo` nie był aktualny. `To` ogranicza zmianę (C-08).
3. **Przyczyna spoza słownika** daje `Success = true`, ale nie zapisuje się (R-03).
4. **Adres bez województwa** ustawia „nieokreślone” (C-04).
5. **Zmiana po końcu umowy tworzy zapis bez etatu** (R-04). Zwolnienie zostaje w historii.
6. **Rachunek jest dopisywany**, poprzednie zostają. `Domyslne: true` ustawia go jako domyślny.
7. **Nieznane pole w JSON jest ignorowane**, więc literówka w nazwie pola nie daje błędu.
8. **Tytuł ubezpieczenia jest zawsze ustawiany na 0110** przy każdej aktualizacji etatu (kod dodatku, nietestowane na innych tytułach).
9. **Zapis trwał lokalnie 2–20 s.** Kilka razy pojawił się timeout SQL albo „Wystąpił błąd podczas modyfikacji zapisu w bazie”, a ponowienie przechodziło.
   Blokady SQL nie zaobserwowano, przyczyna nieznana. System wywołujący powinien ponawiać takie żądania.

`GetEmployee` (biblioteka `WCF.Core`) pokazuje `Zwolniony = false` po `UpsertTermination`: koniec umowy widać w `OkresDo`.

## Testy klienta a ten DLL

Kolekcja klienta (9 żądań) **nie pasuje do dostarczonego DLL**:

- ma `pars` jako tablicę, a DLL przyjmuje obiekt (N-04);
- używa angielskich nazw pól (`EmployeeCode`, `FirstName`, `EffectiveDate`, `EmploymentContract`, `BankAccount`, `ContactData`), a DLL polskich
  (`Kod`, `Imie`, `DataAktualizacji`, `Zatrudnienie`, `Rachunki`, `DaneKontaktowe`). Nieznane pola są ignorowane, więc żądanie wygląda na puste;
- zawiera pola, których w tym DLL nie ma: `FunctionalCode`, `BaseOfEmployment` (w DTO `ZatrudnienieNaPodstawie`), `TypeOfEmployment` (w DTO `RodzajZatrudnienia`).

Kreator buduje żądania wg DLL, bo tylko to da się sprawdzić. Testy klienta pochodzą z innej wersji dodatku.

## Definicje stanowisk niehistoryczne

Klient ma wersję, w której definicje stanowisk nie są historyczne, a baza testowa 2610 ma definicje historyczne. Dodatek szuka definicji po nazwie,
więc scenariusze grupy 7 obejmują: zmianę stanowiska od daty, kontrolę przed i po zmianie, nieistniejącą definicję i inną wielkość liter.
Na wersji niehistorycznej nie były uruchamiane.

## Nieustalone

- Zachowanie na wersji klienta (niehistoryczne definicje stanowisk, angielskie nazwy pól, tablica `pars`).
- Pola `BaseOfEmployment`, `TypeOfEmployment` i `FunctionalCode` z testów klienta.
- Wyrejestrowanie z ZUS przy `UpsertTermination` nie było sprawdzane.

## Biblioteka `WCF.Core`

Metoda `KadryPlaceWebAPI/UpdateEmployee` z dokumentacji WebAPI v. 5.1 działa na tej samej bazie, ale klient z niej nie korzysta. Kreator używa z biblioteki
tylko `GetEmployee` do kontroli i `GetWebServiceInfo` do diagnostyki. Dokumentacja nie opisuje dodatku klienta.
