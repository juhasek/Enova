# Integracja WebAPI – Kadry: analiza dodatku i biblioteki WCF.Core

Data analizy: 2026-10-08. Źródło: dwa pliki DLL przekazane do `Pobrane/` (dodatek WebAPI
z usługami kadrowymi oraz biblioteka bazowa `WCF.Core`). Analiza wyłącznie statyczna
(dekompilacja i metadane). **Nic nie zostało zweryfikowane na działającym serwerze.**
Do tego służą scenariusze: [Scenariusze testowe.xlsx](Scenariusze%20testowe.xlsx) oraz
kolekcja Postman w [Postman/](Postman/).

---

## 1. Co to jest

Skompilowany dodatek enova365 (wersja 2512.9.11, oznaczona **Alfa**, zbudowany pod
enova 2512 / .NET 8). Wystawia przez WebAPI dwie usługi, którymi system zewnętrzny
zasila kadry:

| Usługa | Metoda | Działanie |
|---|---|---|
| `IStaff` | `UpsertEmployee` | Dodanie lub aktualizacja pracownika (szukanie po **kodzie**): dane osobowe, 3 adresy, podatki, etat, kontakty, rachunki, cechy. |
| | `UpsertTermination` | Rozwiązanie umowy: koniec etatu, przyczyna, wyrejestrowanie z ZUS. |
| `IStrukturaWydzialowa` | `GetWydzialy`, `UpsertWydzial`, `DeleteWydzial`, `GetOddzialy` | Odczyt i zmiana drzewa wydziałów, odczyt oddziałów firmy. |

Konfiguracja: własna strona w **Narzędzia → Opcje** z jednym polem **„Data odcięcia”**
(zapis w `CfgNodes`/`CfgAttributes`, węzeł nazwany jak assembly dodatku → `Konfiguracja`).
`UpsertEmployee` odrzuca dane z `DataAktualizacji.From` wcześniejszą niż ta data.

Pełna nazwa typu usługi, potrzebna w polu `ServiceName` wywołania, ma postać
`<Namespace dodatku>.Interfaces.IStaff, <Nazwa assembly dodatku>` (analogicznie
`IStrukturaWydzialowa`). Wartości wpisuje się w środowisku Postman — patrz rozdział 6.

### Zależności

- Biblioteka bazowa `WCF.Core` — obsługa żądań (`HandleRequest2`), klasy `RowDTO`,
  `UpdateResult`, `PackableDataResponse`, `DateMapper`, zapis cech.
- Wymagane w bazie: cechy **„Typ pracownika”** (na historii pracownika) i **„Numer
  koncernowy”** (na pracowniku), definicje stanowisk (szukane po **nazwie**), wydziały
  (po **kodzie**), urzędy skarbowe (po kodzie), przyczyny rozwiązania umowy (po **nazwie**).
  Brak którejkolwiek pozycji przerywa wywołanie wyjątkiem — poza przyczyną rozwiązania
  (patrz 3.5).

---

## 2. Przebieg `UpsertEmployee`

1. Kontrola daty odcięcia (tylko gdy podano `DataAktualizacji.From`).
2. Pracownika **nie ma** → zakładany z kodem; `DataAktualizacji` jest **pomijana**
   (dane trafiają do jedynego zapisu historii).
3. Pracownik **istnieje** → wymagane `DataAktualizacji.From`; brany zapis historii z tego
   dnia albo tworzony nowy (`Historia.Update`). Gdy podano `To`, ustawiana jest aktualność
   zapisu `From…To`.
4. Zapis danych DTO do tego zapisu historii, a następnie **do wszystkich późniejszych
   zapisów historii** (w zakresie do `To`, jeśli podane).
5. Etat (`Zatrudnienie`): czyszczenie pól rozwiązania umowy, okres etatu, daty, wydział,
   podstawa, typ umowy, rodzaj zatrudnienia, stanowisko, wymiar, tytuł ubezpieczenia
   **0110**, cecha „Typ pracownika”.

Pola wymagane w JSON (`Required.Always`, czyli obecne i różne od null): `DataAktualizacji`,
`Kod`, `DataUrodzenia`, `Imie`, **`ImieDrugie`** (trzeba wysłać choćby `""`), `Nazwisko`;
w `Zatrudnienie`: `OkresZatrudnieniaOd`, `JednostkaOrg`, `TypUmowy`, `Stanowisko`, `Wymiar`;
w rozwiązaniu umowy wszystkie pola poza `RejestracjaZus` — także **`OkresUmowyDo`, którego
kod w ogóle nie czyta**.

---

## 3. Problemy znalezione w dodatku

### 3.1 Aktualizacja pracownika cofa rozwiązanie umowy — KRYTYCZNE
Każde `UpsertEmployee` z sekcją `Zatrudnienie`:
- wywołuje czyszczenie pól rozwiązania (przyczyna, kod zwolnienia, podstawa prawna,
  inicjatywa, data złożenia wypowiedzenia);
- nadpisuje `Etat.Okres` wartością z DTO; brak `OkresZatrudnieniaDo` = etat bez końca.

Jeśli po `UpsertTermination` przyjdzie zwykła aktualizacja danych, zwolnienie po cichu
znika — dodatkowo na wszystkich późniejszych zapisach historii (3.2). Scenariusz **R-02**.

### 3.2 Zmiana z datą wsteczną nadpisuje przyszłe zapisy historii — KRYTYCZNE
Dane wysłane z datą X trafiają do każdego zapisu historii po X: etat, wymiar, stanowisko,
wydział, adresy, kontakty. Zaplanowane późniejsze zmiany (np. wymiaru) są zastępowane.
Scenariusz **P-05a/P-05b**.

### 3.3 Tytuł ubezpieczenia zawsze 0110
`Tyub4` ustawiany na sztywno przy każdej aktualizacji etatu, bez względu na typ umowy
i rodzaj zatrudnienia.

### 3.4 Pola bez wartości nadpisują dane
- `Adres.Wojewodztwo` — zawsze wpisywane; brak w JSON = „nieokreślone” we wszystkich
  trzech adresach (**P-06**);
- `RodzajZatrudnienia` — zawsze wpisywany (brak = „Nie dotyczy”);
- `Blokada` rachunku — zawsze wpisywana (brak = odblokowanie) (**P-07**).

### 3.5 Przyczyna rozwiązania szukana po nazwie
`WgNazwy[...]` — gdy nazwa nie pasuje do słownika, przyczyna zostaje pusta, a wynik
i tak `Success = true` (**R-03**).

### 3.6 Wyrejestrowanie z ZUS
- daty `Do` ubezpieczeń = data rozwiązania **+ 1 dzień** — sprawdzić, czy enova nie doda
  kolejnego dnia na ZUS ZWUA (**R-01**);
- `RejestracjaZus = false` usuwa tytuł ubezpieczenia z całego zapisu historii (**R-06b**);
- rozwiązanie nie ustawia kodu zwolnienia, podstawy prawnej ani inicjatywy — tylko
  przyczynę;
- linijka „jeśli tytuł był pusty, ustaw pusty” nic nie robi (pozostałość).

### 3.7 Data odcięcia działa tylko częściowo
Nie dotyczy `UpsertTermination` (**R-08**) ani nowego pracownika wysłanego bez daty.

### 3.8 Wydziały
- `DeleteWydzial` usuwa dowolny wydział po ID, bez dodatkowych zabezpieczeń (**W-08**);
- `UpsertWydzial` i `DeleteWydzial` nie ustawiają `Success = true` (domyślnie `false`)
  — wynik może wyglądać na porażkę mimo zapisu (**W-03**, **W-09**);
- kod i nazwa wydziału zapisywane bez daty, a odczyt jest historyczny (`wydzial[data]`)
  — niesymetryczne (**W-04**);
- `"Parents": null` lub nieistniejące ID → `NullReferenceException` zamiast komunikatu
  (**W-06**, **W-07**);
- nieznana kategoria (definicja wydziału) jest po cichu pomijana, nieznany oddział
  (`CompanyBranch`) czyści oddział;
- do sprawdzenia: czy `Wydzial.OkresWorker` zmienia okres bez wywołania metody
  wykonującej.

### 3.9 Mniejsze
- `SprawdzLicencje()` nadpisane pustą metodą — kontrola licencji biblioteki wyłączona;
  `MinIntervalRequest = 0`.
- Kontakty i rachunki są tylko dopisywane, nigdy usuwane; kilka kontaktów tego samego
  rodzaju może być domyślnych; telefon stacjonarny nie trafia do pola kontaktowego;
  każdy rachunek dostaje priorytet 1 (**P-08**).
- `GetWydzialy`/`GetOddzialy` przy każdym wywołaniu odświeżają cache loginu
  (`ResyncCaches`) — koszt wydajnościowy przy stronicowaniu.
- Martwy kod: mapper obywatelstwa i podatnika zagranicznego nieużywany.

---

## 4. Biblioteka WCF.Core

### 4.1 Stan pliku
- Chroniona obfuskatorem **ConfuserEx** (anti-tamper): kod wszystkich metod jest
  zaszyfrowany i odszyfrowuje się dopiero przy uruchomieniu. Dekompilatory (ilspycmd 8.2
  i 9.1) nie odtwarzają kodu. Czytelne są nazwy typów, sygnatury, część tekstów oraz
  wbudowana dokumentacja (HTML/txt).
- Zdejmowania zabezpieczenia nie wykonywano — to ochrona cudzej biblioteki. Pełny kod
  należy uzyskać od dostawcy.

### 4.2 Niezgodność wersji — BLOKUJĄCE

| | Dodatek WebAPI | Dostarczony WCF.Core | Serwer lokalny |
|---|---|---|---|
| .NET | 8 | **10** | 8 |
| enova | 2512.9.11 | **2606.0.1** | 2512.5.6 |
| oczekiwany WCF.Core | **2510.1.1** | (assembly 8.1.0.0) | — |

Dostarczona para nie uruchomi się razem na enova 2512. Do testów potrzebny jest
`WCF.Core` **2510.x pod .NET 8**, z którym dodatek był budowany.

### 4.3 Co wynika z sygnatur dla dodatku (wnioski, nie kod)
- **Uprawnienia:** biblioteka sprawdza prawa operatora po pełnej nazwie metody i deklaruje
  `SimpleRight` dla swoich metod. Dodatek przekazuje jako nazwę metody `""` i nie deklaruje
  własnych praw → jego metody prawdopodobnie nie podlegają kontroli praw na poziomie
  metody (**W-08**).
- **Logi w plikach .txt:** klasa bazowa zapisuje żądanie/odpowiedź do plików (folder
  `Response`). Jeśli logowanie jest włączone, **PESEL-e, adresy i numery kont** trafiają
  na dysk serwera (**T-03**).
- **Cechy w `Features`:** zapis wymaga flagi „Dostęp przez WebAPI” (lub starszej
  „Obsługa WCF”) na definicji cechy; `null`/`""` czyści wartość (**P-11**). Cechy
  wpisywane przez dodatek bezpośrednio w kodzie tej flagi nie potrzebują.
- **Sposób wywołania:** biblioteka rejestruje własne usługi w Dynamic WebAPI
  (`/api/{usługa}/{metoda}` + token). Dodatek nie ma `DynamicApiControllerAttribute` →
  najpewniej dostępny tylko przez `POST /api/MethodInvoker/InvokeServiceMethod`
  (**D-02**).
- **Dublowanie funkcji:** `WCF.Core` ma własne `IKadryPlace2.UpdateEmployee`
  (`PracownikDTO` z `Aktualizuj`, `PowodAktualizacji`, `Zatrudnienie.Zwolniony`);
  dodatek pisze własny Upsert. Problemy z rozdziału 3 leżą w kodzie dodatku.

---

## 5. Rekomendacje

1. Pozyskać od dostawcy `WCF.Core` w wersji zgodnej z dodatkiem (2510.x / .NET 8),
   najlepiej niezaciemnioną lub z kodem źródłowym dla partnera.
2. Przed produkcją poprawić w dodatku 3.1 i 3.2 (nie czyścić rozwiązania i nie nadpisywać
   okresu etatu, gdy DTO ich nie niesie; nie propagować na przyszłe zapisy historii
   lub robić to tylko dla pól jawnie zmienionych).
3. Uzgodnić z systemem źródłowym: tytuł ubezpieczenia, województwo, blokadę rachunku,
   słownik przyczyn rozwiązania (lub mapowanie po kodzie).
4. Ustalić politykę logów (T-03) i uprawnień operatora integracyjnego (W-08).
5. Przejść scenariusze z arkusza na bazie testowej i wpisać wyniki.

---

## 6. Scenariusze testowe i Postman

- [Scenariusze testowe.xlsx](Scenariusze%20testowe.xlsx) — 37 scenariuszy (diagnostyka,
  wydziały, pracownik, rozwiązanie umowy, techniczne) z kolumnami na wynik; arkusz
  „Zmienne Postman” opisuje zmienne środowiska.
- [Postman/WebAPI-Kadry.postman_collection.json](Postman/WebAPI-Kadry.postman_collection.json)
  — 36 ramek (żądań) w folderach w kolejności wykonania; każde ma opis i testy
  (HTTP 200, brak wyjątku / `Success`, albo oczekiwane odrzucenie). W-03 zapisuje ID nowego
  wydziału w zmiennej kolekcji `testWydzialId`.
- [Postman/WebAPI-Kadry.postman_environment.json](Postman/WebAPI-Kadry.postman_environment.json)
  — szablon środowiska; pola `UZUPELNIJ` wypełnić przed uruchomieniem. Hasła wpisywać
  tylko lokalnie w Postmanie, **nie commitować** uzupełnionego środowiska.

Wszystkie ramki idą przez `POST {{baseUrl}}/api/MethodInvoker/InvokeServiceMethod`
z treścią `{ DatabaseHandle, Operator, Password, ServiceName, MethodName, MethodArgs: { pars } }`.
Daty `DataDTO` mają postać `{ "Year": 2026, "Month": 1, "Day": 1 }`; enumy wysyłane są
jako liczby (np. `TypUmowy` 1 = na czas nieokreślony, `Wojewodztwo` 6 = małopolskie,
`DaneKontaktowe.Rodzaj` 0 = tel. stacjonarny, 1 = komórkowy, 2 = e-mail).

Kolejność ma znaczenie: P-01 zakłada pracownika `{{kodTest}}`, na którym bazują kolejne
P-xx i R-01…R-04/R-08; R-06a zakłada drugiego pracownika do R-06b/R-07. Scenariusze
P-12 i R-08 wymagają ręcznego ustawienia i wyczyszczenia „Daty odcięcia”.

### Regeneracja

Arkusz i kolekcja są budowane z jednej listy w [Generator/Program.cs](Generator/Program.cs):

```
cd Integracja\WebAPI-Kadry\Generator
dotnet build -c Release -o bin\out
dotnet bin\out\GenScenariusze.dll ..
```

Zmiany scenariuszy wprowadzać w generatorze, nie ręcznie w plikach wynikowych.
