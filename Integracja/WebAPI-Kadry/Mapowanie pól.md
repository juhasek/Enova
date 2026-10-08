# Mapowanie pól systemu źródłowego na ramki WebAPI (kadry)

Data: 2026-10-08. Podstawa: kod dodatku WebAPI (atrybuty `Required.Always` w DTO + logika
zapisu), opis w [Analiza.md](Analiza.md). Lista pól źródłowych przekazana przez użytkownika.
**Niezweryfikowane na żywym serwerze** — potwierdzić scenariuszami z
[Scenariusze testowe.xlsx](Scenariusze%20testowe.xlsx).

Wersja do przekazania klientowi (z kolumnami na odpowiedzi): [Wymagalność pól.xlsx](Wymagal%C5%84o%C5%9B%C4%87%20p%C3%B3l.xlsx)
— generowana z `Generator/Mapowanie.cs`; zmiany wprowadzać tam i w tym pliku.

## Legenda statusów

| Status | Znaczenie |
|---|---|
| **WYMAGANE** | Pole musi być w JSON i nie może być `null` — inaczej całe żądanie jest odrzucane (błąd deserializacji). |
| **WYMAGANE + SŁOWNIK** | Jak wyżej, a dodatkowo wartość musi istnieć w słowniku enova — inaczej wyjątek i **nic się nie zapisuje** (cała metoda to jedna transakcja). |
| **WARUNKOWE** | Wymagane tylko w określonej sytuacji (opisanej w uwagach). |
| **OPCJONALNE** | Można pominąć; brak = pole w enova bez zmian. |
| **OPCJONALNE – NADPISUJE** | Można pominąć, ale brak **zeruje** wartość w enova. Wysyłać zawsze. |
| **NIEOBSŁUGIWANE** | Dodatek nie ma takiego pola — wartość nie trafi do enova, nawet jeśli zostanie wysłana. |

Ważne: kontrola `Required.Always` działa przy deserializacji Newtonsoft (wywołanie przez
`MethodInvoker`). Gdyby usługi były wołane przez Dynamic WebAPI (System.Text.Json), braki
nie zostaną odrzucone na wejściu, tylko skończą się błędem lub wartością domyślną w trakcie
zapisu.

## UpsertEmployee (`IStaff`)

| # | Pole źródłowe | Pole w ramce | Status | Uwagi / format |
|---|---|---|---|---|
| 1 | Effective as of | `DataAktualizacji.From` | **WYMAGANE** (obiekt `DataAktualizacji`) / **WARUNKOWE** (`From`) | `DataAktualizacji` musi być zawsze. `From` wymagane dla istniejącego pracownika (bez niego: „No employee updated/added”); przy nowym pracowniku ignorowane. Format `{Year, Month, Day}`. Data starsza niż „Data odcięcia” → odrzucenie. |
| 2 | Person ID (=PK-ID) | `Kod` + `PKID` | **WYMAGANE** (`Kod`) | `Kod` to klucz wyszukiwania pracownika (komunikat dodatku: „Employee code (PersonID)”). `PKID` (opcjonalne) zapisuje się do cechy „Numer koncernowy” — cecha musi istnieć. **Do potwierdzenia:** czy kodem ma być Person ID, User ID czy pole KOD. |
| 3 | User ID | — | NIEOBSŁUGIWANE | Chyba że zostanie uzgodnione jako `Kod` (pkt 2). |
| 4 | Date Of Birth | `DataUrodzenia` | **WYMAGANE** | ISO `"1985-03-15T00:00:00"` (nie `{Year…}`). |
| 5 | Place Of Birth | `MiejsceUrodzenia` | OPCJONALNE | |
| 6 | First Name | `Imie` | **WYMAGANE** | |
| 7 | Middle Name | `ImieDrugie` | **WYMAGANE** | Pole musi być obecne nawet bez drugiego imienia — wysłać `""` (nie `null`). |
| 8 | Last Name | `Nazwisko` | **WYMAGANE** | |
| 9 | Birth Name | `NazwiskoRodowe` | OPCJONALNE | |
| 10 | Gender | — | NIEOBSŁUGIWANE | Płeć liczona **tylko z PESEL**. Pracownik bez PESEL (cudzoziemiec) nie dostanie płci z integracji. |
| 11 | Nationality | `Obywatelstwo` | OPCJONALNE | Tekst (nazwa obywatelstwa, np. „polskie”). |
| 12 | Tax Office | `Podatki.KodUrzeduSkarbowego` | OPCJONALNE + SŁOWNIK | Jeśli wysłane — kod urzędu musi istnieć w enova, inaczej wyjątek. Wymaga mapowania kodów systemu źródłowego na kody enova. |
| 13 | National Id Card Type | (`Podatki.IdentyfikatorPodatkowy`) | NIEOBSŁUGIWANE / częściowo | Typu dokumentu dodatek nie zapisuje. Ewentualnie wybór identyfikatora podatkowego: 1 = PESEL, 2 = NIP. |
| 14 | National Id | `PESEL` (lub `NIP`) | OPCJONALNE | Wysyłać jako PESEL tylko gdy typ = PESEL. Paszportu / innego dokumentu dodatek nie obsługuje. |
| 15 | Country/Region | `Adres….KodKraju` / `Kraj` | OPCJONALNE | `KodKraju` = kod ISO (PL), `Kraj` = nazwa. |
| 16 | Street | `Adres….Ulica` | OPCJONALNE | |
| 17 | House Number | `Adres….NrDomu` | OPCJONALNE | |
| 18 | Apartment | `Adres….NrLokalu` | OPCJONALNE | |
| 19 | Municipality | `Adres….Gmina` | OPCJONALNE | |
| 20 | City | `Adres….Miejscowosc` | OPCJONALNE | |
| 21 | Postal Code | `Adres….KodPocztowyS` | OPCJONALNE | Format `00-000`. |
| 22 | Region | `Adres….Wojewodztwo` | **OPCJONALNE – NADPISUJE** | Liczba 0–16 (np. 6 = małopolskie, 7 = mazowieckie). Brak = „nieokreślone”. Wymaga tabeli mapowania nazw regionów na liczby. |
| 23 | Post | `Adres….Poczta` | OPCJONALNE | |
| — | (brak w źródle) | `Adres….Powiat` | OPCJONALNE | Źródło nie ma powiatu. |
| 24 | Email Address | `DaneKontaktowe[] { Rodzaj: 2, Kontakt }` | OPCJONALNE | W elemencie listy `Rodzaj` i `Kontakt` są wymagane; pusty `Kontakt` = element pominięty. `Domyslny: true` ustawia e-mail w kontakcie pracownika. |
| 25–27 | Country/Region Code, Area Code, Phone Number | `DaneKontaktowe[] { Rodzaj: 1, Kontakt }` | OPCJONALNE | Skleić w jeden tekst (np. „+48 600100200”). 1 = komórkowy, 0 = stacjonarny (stacjonarny nie trafia do pola Telefon). |
| 28 | Job country/region | — | NIEOBSŁUGIWANE | Ewentualnie przez `RodzajZatrudnienia` = 5 (pracownik za granicą) — wymaga uzgodnienia. |
| 29 | Pay Type | — | NIEOBSŁUGIWANE | |
| 30 | IBAN | `Rachunki[].Numer` | OPCJONALNE | Z prefiksem kraju lub bez; spacje dozwolone. Rachunki są tylko dopisywane, nigdy usuwane. |
| 31 | BIC | `Rachunki[].SWIFT` | OPCJONALNE | |
| — | (brak w źródle) | `Rachunki[].Blokada`, `Domyslne` | **OPCJONALNE – NADPISUJE** | Brak = `false` (odblokowanie, zdjęcie domyślności). Wysyłać jawnie `Domyslne: true` dla głównego rachunku. |
| 32 | Contract Start Date | `Zatrudnienie.OkresZatrudnieniaOd` | **WYMAGANE** (gdy jest sekcja `Zatrudnienie`) | Cała sekcja `Zatrudnienie` jest opcjonalna; jeśli jest — te pola są wymagane. |
| 33 | Contract End Date | `Zatrudnienie.OkresZatrudnieniaDo` | **OPCJONALNE – NADPISUJE** | Brak = umowa bez końca. **Uwaga:** cofa wcześniejsze rozwiązanie umowy (Analiza 3.1) — po rozwiązaniu wysyłać datę rozwiązania albo nie wysyłać sekcji `Zatrudnienie`. |
| 34 | Recruit Date | `Zatrudnienie.DataRozpoczeciaPracy` lub `DataZawarciaUmowy` | OPCJONALNE | Do uzgodnienia, które pole. Drugie z nich nie ma odpowiednika w źródle. |
| 35 | Contract Type | `Zatrudnienie.TypUmowy` | **WYMAGANE** | Liczba: 1 = na czas nieokreślony, 2 = na okres próbny, 3 = na czas określony, 5 = na zastępstwo… Wymaga tabeli mapowania. |
| 36 | KOD | `Zatrudnienie.JednostkaOrg` (+ `KodPrawaDostepu`) | **WYMAGANE + SŁOWNIK** | **Do potwierdzenia**, że KOD to kod jednostki organizacyjnej. Musi istnieć wydział o tym kodzie. `KodPrawaDostepu` (opcjonalne) — ten sam słownik, ustawia wydział pracownika do praw dostępu. |
| 37 | Positionh Title PL | `Zatrudnienie.Stanowisko` | **WYMAGANE + SŁOWNIK** | Dokładna **nazwa** definicji stanowiska w enova (moduł HR). Inna pisownia = wyjątek. |
| 38 | Job Title | — | NIEOBSŁUGIWANE | Chyba że ma zastąpić pkt 37. |
| 39 | Position | — | NIEOBSŁUGIWANE | Stanowisko szukane po nazwie, nie po kodzie pozycji. |
| 40 | Position Entry Date | — | NIEOBSŁUGIWANE | Zmiana stanowiska datowana przez `DataAktualizacji.From`. |
| 41 | Function Code | — | NIEOBSŁUGIWANE | |
| 42 | Standard Weekly Hours | `Zatrudnienie.Wymiar {Numerator, Denominator}` | **WYMAGANE** | Przeliczenie: godziny / 40 jako ułamek (40 → 1/1, 20 → 1/2, 30 → 3/4). `Denominator` = 0 → wymiar pominięty. |
| 43 | Employee Class | `Zatrudnienie.TypPracownika` | OPCJONALNE | Zapis do cechy „Typ pracownika” (na historii pracownika) — cecha musi istnieć. **Do potwierdzenia.** |
| — | (brak w źródle) | `Zatrudnienie.ZatrudnienieNaPodstawie` | OPCJONALNE | 1 = umowa o pracę. Zalecane wysyłać stałe 1. |
| — | (brak w źródle) | `Zatrudnienie.RodzajZatrudnienia` | **OPCJONALNE – NADPISUJE** | Brak = 0 („Nie dotyczy”). |
| — | (brak w źródle) | `NIP`, `ImieOjca`, `ImieMatki`, `Features` | OPCJONALNE | |

Zapisywane zawsze, bez względu na dane: tytuł ubezpieczenia **0110** (przy sekcji
`Zatrudnienie`), płeć z PESEL.

## UpsertTermination (`IStaff`)

| Pole źródłowe | Pole w ramce | Status | Uwagi |
|---|---|---|---|
| Person ID / KOD pracownika | `PracownikKod` | **WYMAGANE** | Ten sam klucz co `Kod` w UpsertEmployee; brak pracownika = błąd. |
| Contract Start Date | `OkresUmowyOd` | **WYMAGANE** | Musi być **dokładnie równe** początkowi etatu w enova, inaczej „Employee's contract was not updated” (nic się nie zmienia). |
| Contract End Date | `OkresUmowyDo` | **WYMAGANE** | Kod go nie używa, ale bez niego żądanie jest odrzucane — wysłać dowolną datę (np. Contract End Date lub Termination Date). |
| Termination Date | `TerminationDate` | **WYMAGANE** | |
| TerminationReason | `TerminationReason` | **WYMAGANE + SŁOWNIK** | Dokładna **nazwa** przyczyny rozwiązania z enova. Nieznana nazwa = przyczyna pusta, ale wynik `Success = true` (błąd po cichu). „NO SHOW” przy dacie rozwiązania = początku umowy kasuje okres etatu. |
| Effective as of: | — | NIEOBSŁUGIWANE | Rozwiązanie nie ma daty aktualizacji; nie podlega też „Dacie odcięcia”. |
| Event Reason | — | NIEOBSŁUGIWANE | Ewentualnie źródło dla `TerminationReason`, jeśli to tu jest przyczyna — do uzgodnienia. |
| (brak w źródle) | `RejestracjaZus` | OPCJONALNE | Brak = `true` (wyrejestrowanie). `false` usuwa tytuł ubezpieczenia (Analiza 3.6). |

## Słowniki enova, które muszą być gotowe przed integracją

Brak pozycji = wyjątek i odrzucenie całego żądania (oprócz przyczyny rozwiązania):

1. **Wydziały** — kody zgodne z polem KOD (JednostkaOrg, KodPrawaDostepu).
2. **Definicje stanowisk (HR)** — nazwy zgodne z „Positionh Title PL”.
3. **Urzędy skarbowe** — jeśli wysyłany Tax Office (mapowanie kodów).
4. **Przyczyny rozwiązania umowy** — nazwy zgodne z TerminationReason (błąd po cichu!).
5. **Cechy** „Numer koncernowy” (Pracownik) i „Typ pracownika” (historia pracownika) —
   gdy wysyłane PKID / Employee Class.
6. **Banki** — tylko gdy wysyłany `KodBanku` (źródło go nie ma).

## Tabele mapowania do przygotowania po stronie systemu źródłowego

- Region → `Wojewodztwo` (0–16),
- Contract Type → `TypUmowy` (0–10),
- Standard Weekly Hours → `Wymiar` (ułamek),
- Tax Office → kod urzędu skarbowego enova,
- TerminationReason → nazwa przyczyny w enova,
- Country/Region Code + Area Code + Phone Number → jeden tekst telefonu.

## Pytania do uzgodnienia

1. Które pole jest kluczem pracownika (`Kod`): Person ID, User ID czy KOD?
2. Czy KOD to kod jednostki organizacyjnej (wydziału)?
3. Stanowisko z „Positionh Title PL” czy „Job Title”?
4. Recruit Date → data rozpoczęcia pracy czy data zawarcia umowy?
5. Employee Class → cecha „Typ pracownika”?
6. Jeden adres ze źródła → który adres w enova (zamieszkania / zameldowania / korespondencyjny), czy wszystkie trzy?
7. Gender i National Id Card Type dla cudzoziemców bez PESEL — dodatek ich nie obsługuje; czy rozszerzyć?
8. Event Reason — czy to źródło przyczyny rozwiązania?
