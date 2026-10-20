# Zgodność wzorów analizy z instrukcją enova365 „Kadry Płace i HR”

Data: 2026-10-20. Źródło: instrukcja producenta (PDF, 2196 stron), tekst zrzucony lokalnie do
`_robocze/instrukcja.txt` (poza gitem). „s.” = numer strony PDF. Oznaczenia dokumentów:
AN – dokument główny, Z1 Kadry, Z2 Czas pracy, Z3 Płace, Z5 Pulpity (numery sekcji jak we wzorach).

Sprawdzono ok. 70 stwierdzeń o standardzie programu (nazwy zakładek, folderów i opcji, zachowanie
systemu, definicje standardowe, wymagania licencyjne).

## 1. Do poprawy – niezgodne z instrukcją lub niepełne

| # | Miejsce | Jest we wzorze | Instrukcja | Poprawka |
|---|---|---|---|---|
| 1 | Z2 1.10 Nadgodziny | „dodatek 100% za nadgodziny średniotygodniowe wypłacany po zakończeniu okresu” jako zasada ogólna | Wysokość dopłaty za przekroczenie normy średniotygodniowej to ustawienie kalendarza „Rozliczanie”: Zawsze 50% (najczęstsza interpretacja) / Zawsze 100% / Do wysokości limitu 50% (s.1608) | Opis neutralny + wiersz „Dopłata za przekroczenie normy średniotygodniowej” w tabeli ustaleń |
| 2 | Z1 1.14 Oddelegowani | „standardowa funkcjonalność obsługi pracowników oddelegowanych” | „Pracownicy Eksportowi” to funkcjonalność dodatkowa: wymaga Płac w wersji złotej lub platynowej **i wykupienia dodatkowej licencji**, instalacji dodatku i plików xml (s.2179–2180) | Opis jako dodatek licencjonowany; pozycja w tabeli licencji (AN 3.2.1) |
| 3 | AN 3.2.1 UWAGA o licencji | W wariancie złotym brak m.in. RCP, rozliczania czasu pracy, weryfikatorów, planowania kalendarzy, magazynu nadgodzin, ZFŚS **i ochrony danych osobowych** | Ochrona danych osobowych jest dostępna w wersji złotej i platynowej (s.402). „Planowanie kalendarzy” – brak potwierdzenia. Tylko w platynie: Dane z RCP i Oryginalne dane z RCP, Rozliczenie czasu pracy, Dokumenty aktualizacji kalendarzy (s.375), Wersjonowanie kalendarzy (s.1568), Weryfikatory kalendarza (s.679), Magazyn nadgodzin (s.231), Planowane nieobecności (s.268), Planowane listy płac/rezerwy (s.896), ZFŚS (s.15), Pracownicy zewnętrzni B2B/APT (s.1317), listy Ewidencje (s.375). Od złotej: e-dokumentacja, lista importu z PUE, Struktura organizacyjna, definiowanie dni (s.375, 397, 1609) | Lista w uwadze zgodna z instrukcją, z numerami stron |
| 4 | Z3 1.15 Import z PUE | Import ręczny przez plik csv i Czynność/Import nieobecności z PUE; opcje „nie nadpisywać / nadpisywać tylko nierozliczone / nadpisywać i korygować wszystkie” | Konfiguracja: Narzędzia/Opcje/Kadry i płace/Kalendarze/Import z PUE; opcje: **Nie edytuj / Tylko nierozliczone / Koryguj rozliczone i nierozliczone** (s.684). E-zwolnienia pobiera się przyciskiem na liście Kadry/Czas pracy/Import nieobecności z PUE bez logowania do PUE (s.686; lista od wersji złotej – s.439). Import automatyczny wymaga dodatku **Harmonogram Zadań** i zadania AutoImportNieobecnosciPUE (s.685–686). Opieka: Zwolnienie opieka (ZUS) – domyślnie / (ZUS bez zasiłku 1/30) / (ZUS bez zasiłku prop.) (s.684–685); w wielooddziałowej konfiguracja per oddział | Przepisany opis zgodnie z instrukcją; Harmonogram Zadań w tabeli licencji |
| 5 | Z1 1.18 Tabela definicji nieobecności | Lista 56 definicji z analizy 2025 (część nazw historycznych) | Aktualna lista standardowa (s.256–257) zawiera dodatkowo m.in.: Urlop doktorancki, Urlop kombatancki, Urlop weterana, Uzupełniający urlop macierzyński, Zwolnienie chorobowe (bez zasiłku 1/30); Urlop radcowski i Urlop wypoczynkowy prac. tymcz. są domyślnie zablokowane | Tabela zastąpiona listą z instrukcji w jej grupach (nieusprawiedliwione / usprawiedliwione niepłatne / usprawiedliwione płatne) |
| 6 | Z1 1.10 Powiadomienia | 6 powiadomień, w tym „Skierowanie na badania kontrolne (5 dni przed końcem zwolnienia > 30 dni)” | Powiadomienia (taski): Przypomnienie o końcu umowy, Termin wykonania badania, Termin szkolenia BHP, Termin ważności uprawnienia, Badania lekarskie / Szkolenia BHP / Uprawnienia na stanowisku – braki (s.220) oraz Powiadomienie o autozapisie do PPK (Systemowe/Definicje zadań, co 4 lata, s.769–772); parametr „Zakończono wszystkie czynności związane ze zwolnieniem pracownika” wyłącza powiadomienia (s.220). Powiadomienia o badaniach kontrolnych w instrukcji brak | Tabela wg instrukcji; badania kontrolne jako „do potwierdzenia w programie” |
| 7 | AN 3.8 PIT-11 | zakładka „Elektroniczna dokumentacja pracownika – Pozostała dokumentacja” | „Elektroniczna dokumentacja pracownicza / Pozostała dokumentacja” (s.196); „Potwierdź i wyświetl” na Wydruku PIT-11 (s.875) | Poprawiona nazwa |
| 8 | Z5 1.2 Publikacja wypłat | „parametr Publikuj w pulpitach” na liście płac | Czynność „Publikuj w pulpitach” / „Wycofaj publikację” na zatwierdzonej liście płac – dostępna, gdy w konfiguracji Pulpity/Pulpity HR parametr Publikacja w pulpitach = „Po publikacji w pulpitach”; wypłaty widoczne w Pulpicie pracownika w Dane finansowe/Wypłaty (s.500) | Opis zgodny z instrukcją |
| 9 | Z3 1.10.1 Rezerwa urlopowa | mechanizm planowanych list płac bez warunków | Wersja platynowa; włączenie: Narzędzia/Opcje/Kadry i płace/Płace/Planowane listy płac; elementy z „Rodzaj naliczania: Tylko planowane”; czynność „Nalicz planowane listy płac…” (s.896–897) | Dopisane warunki i kroki |
| 10 | Z5 1.4 Zastępstwa | bez warunku konta www | Zastępujący musi mieć konto www (Kadry/Dostęp www); zastępstwa w pulpicie: Ogólne/Moje zastępstwa; licencje zgodne z opisem (s.2058–2059) | Dopisany warunek |
| 11 | Z1 1.2.9, 1.8 Listy ewidencji | „Ewidencje/Nagrody i kary”, „Ewidencje/Badania lekarskie” | Kadry/Ewidencje/… – listy w wersji platynowej (s.375, 464, 466) | Pełna ścieżka + wariant licencji |
| 12 | Z1 1.18 Limit po świadectwie pracy | „usunąć limit i naliczyć ponownie” | Instrukcja: po zmianie danych (np. dacie końca umowy) należy naliczyć limit jeszcze raz (s.248); naliczanie seryjne: Czynności/Limity nieobecności/Nalicz (s.254) | Opis zgodny z instrukcją |

## 2. Niepotwierdzone w instrukcji (praktyka wdrożeniowa – zostają z oznaczeniem lub do sprawdzenia w programie)

| Miejsce | Stwierdzenie | Uwagi |
|---|---|---|
| AN 3.3.1 | nazwa jednostki maks. 80 znaków; problemy raportów ze znakiem „&” | 80 znaków instrukcja podaje dla innych pól (s.184, 1481), nie dla wydziału |
| Z3 1.4 | raport „Lista nierozliczonych elementów” | brak w instrukcji – sprawdzić nazwę wydruku w programie |
| Z1 1.5 | rozszerzony zapis zmian (logowania, operator, zakres zmian) | funkcja platformy, poza instrukcją KP |
| Z3 1.6.1 | trzy sposoby obsługi ujemnej podstawy ZUS | instrukcja potwierdza tylko, że Płatnik nie przyjmuje ujemnych podstaw (s.18, 572) |
| AN 3.2.1, Z5 1.7 | licencja „Logowanie serwisowe”, konfiguracja SMTP | poza instrukcją KP |
| Z2 1.6, Z5 1.1.1, 1.2.2, 1.3.1 | Weryfikatory czasu pracy, 2FA, Dashboard, Planowanie urlopów (Alt One) | rozwiązania wykonawcy – poza instrukcją producenta |

## 3. Zgodne (wybrane potwierdzenia)

Zakładki kartoteki i Wieloetatowość (s.2, 80); Elektroniczna dokumentacja pracownicza (s.193, 420); Ukończone szkolenia (s.106); Okresy pracy w szczególnych warunkach, ZSWA (s.3, 18, 577); KEDU do Płatnika (s.18, 567); Definicje stanowisk i import kodów zawodów GUS (s.201, 388); praca zdalna okazjonalna 24 dni (s.279); Obsługa B2B i APT (s.484, 1317); limity art. 188 dni/godziny (s.254–255); wynagrodzenie chorobowe 33/14 dni (s.239); okres zasiłkowy 182/270 dni (s.1819); urlop na żądanie (s.121–122); Kalendarze/Czas pracy, Definicje dni, Serie dni, Kalendarz/Norma czasu pracy (s.214, 1576, 1581, 266); dokumenty aktualizacji planu i czasu pracy (s.701–705); wersjonowanie i zakładka Wersje (s.246, 1568); Miesięcy wstecz (s.300, 399); roczny limit nadgodzin (s.210); Karta ewidencji czasu pracy (s.865); Dodatki i potrącenia (s.422); planowane listy płac (s.896); KZP, ZFM i pożyczki (s.312–318); ZFŚS/Świadczenia socjalne (s.513); PIT-8AR, PIT-R, IFT-1R (s.595, 615, 622); FP, FGŚP i FEP (s.1796); LPE/LPU (s.502); minimalna stawka godzinowa zleceniobiorców (s.1074); e-wniosek Z-15 (s.1394); Zasobnik dokumentów (s.1959); Zaświadczenie o zatrudnieniu (s.891).

## 4. Decyzje i wprowadzone zmiany (2026-10-20)

- Punkt 1 (nadgodziny średniotygodniowe) – **bez zmian** z decyzji użytkownika: we wdrożeniach przyjmujemy zawsze dopłatę 100% (w kalendarzu ustawienie Rozliczanie = „Zawsze 100%”).
- Punkty 2–12 – wprowadzone w plikach `Tresc/*.txt` (dokument główny, Z1, Z3, Z5, kwestionariusz) i wygenerowane ponownie; kontrola generatora bez błędów.
- Pozycje z rozdziału 2 pozostają jako praktyka wdrożeniowa – do potwierdzenia w programie przy najbliższej okazji.

## 5. Weryfikacja pozycji z rozdziału 2 w plikach programu 2610 (2026-10-20)

Sprawdzone bez logowania do bazy – w DLL-ach i raportach serwera `C:\enovaServer\2610\Soneta.Products.Server.Standard`
(metadane tabel, formularze konfiguracji, rejestracja wydruków).

| Miejsce | Wynik | Źródło | Zmiana we wzorze |
|---|---|---|---|
| AN 3.3.1 – długość nazwy wydziału | **Niezgodne:** Nazwa wydziału maks. **100** znaków (Kod 20) | `Soneta.KadryPlace.dll`, `WydzialRow.Nazwa` – MaxLength(100), `Kod` – MaxLength(20) | „nie może przekroczyć 100 (kod wydziału – 20 znaków)”; uwaga o znaku „&” zostaje jako praktyka |
| Z3 1.4 – raport „Lista nierozliczonych elementów” | Potwierdzone – wydruk na liście pracowników, grupa „Wynagrodzenia i zasiłki” (wersje aspx i repx) | `Aspx/place/Lista nierozliczonych elementów.aspx`, `Soneta.KadryPlace.Reports.dll` | dopisana ścieżka Wydruki/Wynagrodzenia i zasiłki |
| Z3 1.6.1 – ujemna podstawa ZUS | Potwierdzone – wariant zaliczki: parametr w Kadry i płace/Płace/Składki ZUS (domyślnie wyłączony); zapis wypłaty z ujemną podstawą blokuje parametr w Kadry i płace/Płace/Ogólne (domyślnie włączony) | `Soneta.KadryPlace.UI.dll` (CfgSkładkiZUSPage, CfgOgolnePage), domyślne wartości w `Soneta.KadryPlace.dll` | dopisane parametry przy wariantach 2 i 3 |
| Z1 1.5 – rozszerzony zapis zmian | Potwierdzone – Systemowe/Zmiany ustawienia (import, eksport, per tabela: log listy, formularza, zapisów, raportów), lista Systemowe/Zmiany rekordów; typy wpisów m.in. Login, Logout, LoginFailed, Created, Modified, Deleted | `Soneta.Business.UI.dll` (ChangeInfosConfigPage, ChangeInfosPage), enum `Soneta.Business.ChangeInfoType` | dopisane miejsce konfiguracji i przeglądania |
