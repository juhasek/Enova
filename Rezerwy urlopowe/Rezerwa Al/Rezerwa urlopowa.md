# Rezerwa urlopowa i budżet rezerwy – dokumentacja

## 1. Cel

Miesięczna rezerwa urlopowa sprawdzana przez Zarząd oraz budżet rezerwy liczony w sierpniu
na stan urlopu na 01.01 roku następnego. Rozwiązanie korzysta z wbudowanego w enova365
modułu **planowanych list płac** (moduł rezerw, licencja **Płace Platynowe**), dzięki czemu
narzuty pracodawcy liczy standardowy silnik płac, a rezerwa nie trafia na żadną listę płac.

## 2. Założenia klienta → realizacja

| # | Założenie | Realizacja |
|---|---|---|
| 1 | Rezerwa sprawdzana miesięcznie | Planowana lista płac „Rezerwa urlopowa” (REZURL) naliczana co miesiąc po zatwierdzeniu list płac. |
| 2 | Urlop zaległy + bieżący proporcjonalny − wykorzystany do miesiąca | Z limitów urlopu wypoczynkowego i dodatkowego: zaległy = przeniesienie z poprzedniego roku; bieżący = limit roczny × liczba miesięcy do miesiąca rezerwy / liczba miesięcy okresu limitu; wykorzystany = nieobecności pomniejszające limit od 01.01 do końca miesiąca. |
| 3 | Podstawa 1 – zasadnicze + zmienne do ekwiwalentu, średnia z 3 miesięcy (luty → II, I, XII) | Zasadnicze nominalne z kartoteki za każdy miesiąc + elementy z flagą „wliczać do ekwiwalentu” wg okresu „za”; średnia z miesięcy zatrudnienia (max 3). |
| 3 | Podstawa 2 – standard enova (podstawa ekwiwalentu) | Ten sam mechanizm co element „Ekwiwalent za czas urlopu” (`NaliczanieEkwiwalent`). |
| 3 | Współczynnik do ekwiwalentu | Z konfiguracji (Nieobecności → średnia norma miesięczna) × wymiar etatu; gdy brak – dni robocze roku / 12 (jak standard). |
| 4 | Widok: Kod, Imię, Nazwisko, MPK, zaległy, bieżący, wykorzystany, podstawa, kwota, narzuty | Lista **Pracownicy** + cechy z kategorii „Rezerwa urlopowa” (kolumny), miesiąc = data aktualności listy. |
| 5 | Budżet w sierpniu – symulacja limitu na 01.01 roku następnego, dwie podstawy | Planowana lista „Budżet rezerwy urlopowej” (BUDREZURL): symulacja „Limity nieobecności / Nalicz” na rok następny w sesji, która NIE jest zapisywana; zaległy na 01.01 + limit należny na rok następny; podstawy jak w pkt 3. |

## 3. Elementy rozwiązania (w bazie Al)

| Obiekt | Nazwa | Uwagi |
|---|---|---|
| Element wynagrodzenia (Dodatek automatyczny) | **Rezerwa urlopowa** | Edytor algorytmu – plik `Rezerwa urlopowa`. Priorytet 200, Rodzaj naliczania = **Tylko planowane**, Do wypłaty = Nie, PIT = nie naliczać, ZUS = naliczać (dla narzutów). |
| Element wynagrodzenia (Dodatek automatyczny) | **Budżet rezerwy urlopowej** | Ten sam kod; tryb budżetu rozpoznawany po nazwie zaczynającej się od „Budżet”. |
| Definicja planowanej listy płac | **REZURL – Rezerwa urlopowa** | Element = „Rezerwa urlopowa”, algorytm domyślny. |
| Definicja planowanej listy płac | **BUDREZURL – Budżet rezerwy urlopowej** | Element = „Budżet rezerwy urlopowej”. |
| Cechy (tabela Pracownicy, kategoria „Rezerwa urlopowa”) | Rezerwa MPK, Rezerwa urlop zaległy / bieżący / wykorzystany, Rezerwa godziny, Rezerwa podstawa 1 / 2, Rezerwa kwota, Rezerwa narzuty; Budżet urlop zaległy / należny, Budżet godziny, Budżet podstawa 1 / 2, Budżet kwota, Budżet narzuty | Pliki `Cecha widoku rezerwy MPK` i `Cecha widoku rezerwy (wzorzec)`. |

Pola elementu na planowanej wypłacie: Podstawa 1, Podstawa 2 (miesięcznie), Podstawa 3/4/5 =
urlop zaległy / bieżący / wykorzystany w dniach (budżet: zaległy na 01.01 / limit roku
następnego / 0), Czas = godziny rezerwy, Wartość = kwota, Narzuty = narzuty pracodawcy.
Pełne wyliczenie (limity, miesiące podstawy, współczynnik, log standardowego ekwiwalentu)
jest w **Zapisie obliczeń** elementu.

Wzór: `kwota = godziny rezerwy × podstawa / (współczynnik × wymiar etatu) / godzin w dniu urlopu (8 h
lub norma dobowa z kalendarza – wg konfiguracji ekwiwalentu)`.

Przełączniki na początku kodu algorytmu:
- `WARIANT_PODSTAWY` – 1 = podstawa 1 (domyślnie), 2 = podstawa 2, 3 = wyższa z obu;
- `ZASADNICZE_NOMINALNE` – `true` = stawka z kartoteki (domyślnie), `false` = zasadnicze naliczone na listach (pomniejszone o nieobecności).

## 4. Instalacja

1. `bash generuj-xml.sh` – buduje `Rezerwa urlopowa.dbinit.xml` z plików kodu (nie edytować XML ręcznie).
2. `dbmgr importxml <baza> "Rezerwa urlopowa.dbinit.xml" --standard` (import wg rekordów; GUID-y stałe,
   ponowny import aktualizuje te same rekordy). Po imporcie zrestartować serwer enova / przelogować się.
3. Uwaga: `dbmgr importxml` kończy się kodem 0 także przy błędzie w kodzie algorytmu – kod sprawdzono
   osobną kompilacją kontrolną na bibliotekach enova 2512.5.6 (algorytm + 16 cech: bez błędów).
4. Widok: Kadry i płace → Pracownicy → Ustawienia kolumn: dodać Kod, Imię, Nazwisko oraz cechy z kategorii
   „Rezerwa urlopowa”; zapisać jako widok „Rezerwa urlopowa” (i osobno „Budżet rezerwy”).

## 5. Obsługa (miesięcznie)

1. Zatwierdzić listy płac za miesiąc rezerwy (**ważne** – silnik planu USUWA niezatwierdzone wypłaty
   etatowe pracownika przed naliczeniem planu; zachowanie standardowe enova).
2. Kadry i płace → Pracownicy (zatrudnieni) → zaznaczyć wszystkich → Czynności → **Nalicz planowane listy płac**:
   definicja „Rezerwa urlopowa”, okres = miesiąc rezerwy, typ wypłaty Etat.
3. Pracownicy → Aktualność = ostatni dzień miesiąca → widok „Rezerwa urlopowa” → eksport do Excela dla Zarządu.
4. Ponowne naliczenie za ten sam miesiąc jest bezpieczne – widok pokazuje ostatnie naliczenie
   (stare planowane listy można usunąć w Płace → Planowane listy płac).

**Budżet (sierpień):** jak wyżej, definicja „Budżet rezerwy urlopowej”, okres 08/RRRR; widok z cechami „Budżet …”.
Symulacja limitu uwzględnia urlopy już wprowadzone na wrzesień–grudzień (jak standardowe naliczenie limitu);
żadne limity nie są zapisywane do bazy.

## 6. Do potwierdzenia z klientem

1. Która podstawa decyduje o kwocie rezerwy (1, 2 czy wyższa)? Obecnie: podstawa 1.
2. Zasadnicze w podstawie 1: nominalne z kartoteki (obecnie) czy faktycznie naliczone (pomniejszone o chorobowe itp.)?
3. Ujemny stan urlopu (urlop wykorzystany „na zapas”): obecnie rezerwa = 0; czy dopuszczać wartości ujemne?
4. Budżet: czy „stan urlopu na 01.01” to zaległy + pełny limit roku następnego (obecnie), czy tylko zaległy?
5. „Urlop wykorzystany proporcjonalny” – przyjęto: wykorzystany od 01.01 do końca miesiąca rezerwy.
6. Narzuty: składki ZUS pracodawcy + FP + FGŚP + FEP wg silnika płac. Wpłata pracodawcy PPK **nie** jest
   doliczana (element PPK nie liczy się na planie zawężonym do rezerwy) – czy ma być?
7. Urlop dodatkowy (niepełnosprawni) – wliczony razem z wypoczynkowym; czy to właściwe?
8. Licencja Płace Platynowe – wymagana dla planowanych list płac (do sprawdzenia u klienta).

## 7. Status (2026-10-09)

- Zaimportowane do lokalnej bazy **Al** (elementy ID 268/269, DefPlanListPlac ID 1/2, cechy FeatureDefs 4–19),
  kopia bazy przed zmianą: `Al_przed_rezerwa_20261009.bak` (domyślny katalog backupów SQL Server).
- Kod skompilowany kontrolnie bez błędów. **NIESPRAWDZONE na żywych danych** – lokalna baza Al nie ma
  wypłat ani limitów; test wg `Rezerwa urlopowa - scenariusze testowe.xlsx` (21 scenariuszy + kalkulator
  kwot) do wykonania w GUI na bazie z danymi.
- Elementy krytyczne do pierwszego testu: (a) czy dodatek automatyczny pojawia się na planowanej liście
  (RU-01), (b) zgodność podstawy 2 ze standardem (RU-11), (c) symulacja budżetu bez zapisu limitów (RU-16/17).

## 8. Wycena (starszy konsultant enova, bez wsparcia AI)

| Etap | Godziny |
|---|---|
| Doprecyzowanie założeń z klientem, decyzje z pkt 6 | 4 |
| Rozpoznanie i konfiguracja modułu planowanych list płac (rezerwy), licencja | 4 |
| Algorytm – stan urlopu (zaległy / bieżący proporcjonalny / wykorzystany, 2 limity) | 6 |
| Podstawa 1 (średnia 3 mies.) + współczynnik do ekwiwalentu | 3 |
| Podstawa 2 (standard ekwiwalentu) + narzuty (deklaracje ZUS, weryfikacja) | 5 |
| Budżet – symulacja limitu na 01.01 w niezapisywanej sesji | 5 |
| Widok – 16 cech + konfiguracja widoku listy | 5 |
| Definicje planowanych list, import/instalacja na bazie testowej | 2 |
| Testy (21 scenariuszy, dane testowe, ~2 rundy poprawek) | 12 |
| Dokumentacja, instrukcja, szkolenie (kadry/płace, prezentacja dla Zarządu) | 4 |
| Wdrożenie produkcyjne + asysta przy 1. zamknięciu miesiąca i 1. budżecie | 3 |
| **Razem** | **53** |

Rekomendacja do oferty: **ok. 55–60 h** (53 h + 10% bufor na ryzyka: mało używany moduł planowanych list
płac, decyzje z pkt 6). Dolna granica przy konsultancie znającym już moduł rezerw: ok. 40 h.
