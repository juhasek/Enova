# Dodatek A1.KalkulatorPodstawyZasilku (enova365 2610)

Kalkulator podstawy wymiaru zasiłku (chorobowego, opiekuńczego, macierzyńskiego, świadczenia rehabilitacyjnego
i wynagrodzenia chorobowego z art. 92 KP) dla wskazanego pracownika i daty powstania niezdolności.
Rozpoznanie i architektura: `Addon/Plan KalkulatorPodstawyZasilku/Etap1-Rozpoznanie.md`.

## Jak liczy (wariant A, zaakceptowany)

Kalkulator **nie implementuje reguł ustawy** — podstawę liczy natywny silnik enova `Soneta.Place.PodstawaZasiłku`,
dokładnie ten, którego używa algorytm „Jak chorobowe” na liście płac.

1. Otwiera osobną sesję (`readOnly: false`) i transakcję `Logout(editMode: true)` — **bez `Commit` i bez `Save`**.
2. Jeżeli pracownik **nie ma** nieobecności ZUS w dniu D: dodaje ją tylko w pamięci (wybrana definicja nieobecności,
   kod zwolnienia, leczenie szpitalne, opcjonalnie „ponownie ustal podstawę”).
3. Uruchamia standardowe naliczanie wypłat enova (`NaliczanieWypłat`) tylko dla tej nieobecności, za miesiąc dnia D.
   Wynikiem są elementy, które pojawiłyby się na liście płac (wynagrodzenie chorobowe i/lub zasiłek, procent, dni, kwota).
4. Na pierwszym elemencie tworzy `new PodstawaZasiłku(element)` i odczytuje rozbicie: miesiące (nominał, przepracowane,
   ≥ połowy, kwoty przed/po uzupełnieniu), elementy (sposób ujęcia, mnożnik, kwota w podstawie po składkach),
   składniki okresowe, okres podstawy, przejęcie podstawy z poprzedniego zasiłku, podsumowanie i szczegółowy log
   (kategoria „Chorobowe” z `System.Diagnostics.Trace`, przechwytywana tylko z własnego wątku).
5. Zamyka sesję — wszystkie zmiany znikają. Formularz dostaje wyłącznie wartości (DTO).

Gdy nieobecność w dniu D **już jest naliczona** w bazie, kalkulator przelicza podstawę na tym samym elemencie
(jak natywna czynność „Pokaż log naliczania podstawy”) i pokazuje **różnicę do podstawy zapisanej w bazie**.

Bez opcji „tylko zatwierdzone listy” (decyzja 2026-10-16): silnik enova bierze także wypłaty z bufora, więc taki
filtr dawałby wynik inny niż enova.

## Użycie

Kadry i płace → Pracownicy → (formularz pracownika lub zaznaczeni na liście) → **Czynności → Kalkulator podstawy zasiłku**.

| Pole | Znaczenie |
|---|---|
| Pracownik | z kontekstu; przy kilku zaznaczonych — wybór spośród nich |
| Data powstania niezdolności / Nieobecność do | okres symulowanej nieobecności (dłuższy okres pokaże np. przejście z wynagrodzenia chorobowego na zasiłek) |
| Definicja nieobecności | lista definicji typu „Nieobecność ZUS” z bazy (choroba, opieka, macierzyński, rehabilitacja) |
| Rodzaj świadczenia | wynika z definicji (tylko do odczytu) |
| Kod zwolnienia | kody enova pasujące do definicji (np. ciąża, wypadek, szpital zmieniają procent) |
| Wymuś ponowne ustalenie podstawy | diagnostyka: rozbicie bez przejęcia podstawy z poprzedniego zasiłku; element naliczony z flagą zwolnienia „Ponownie ustal podstawę” |

Przycisk **Przelicz**. Zakładki: Kalkulator (podstawa, stawka dzienna 100%, kwoty dzienne wg procentów z konfiguracji,
naliczone elementy, porównanie z bazą), Miesiące, Składniki, Reguły i log, Zaznaczeni pracownicy
(„Przelicz zaznaczonych” — zestawienie).

## Budowanie

- Serwer 2610 działa na **.NET 10** → potrzebny SDK 10 (lokalnie: `~/.dotnet10`, `DOTNET_ROOT=~/.dotnet10`).
- Referencje **wprost do DLL-i serwera** `C:\enovaServer\2610\Soneta.Products.Server.Standard` (`SonetaDir`),
  `Private=false`. Paczki Soneta.Sdk 2610 z nuget.org (z zależnościami ~1 GB) nie zmieściły się na dysku tej maszyny;
  DLL-e serwera to dokładnie wersja 2610.0.0-beta.241094, z którą dodatek działa.
- `dotnet build A1.KalkulatorPodstawyZasilku -c Release` → `bin/Release/A1.KalkulatorPodstawyZasilku.dll`.

## Instalacja (baza Claude, 2026-10-16)

1. `dbmgr extupdate Claude <bin>\A1.KalkulatorPodstawyZasilku.dll --common --server` (dbmgr z katalogu serwera 2610).
2. Restart serwera enova (orchestrator) — rozszerzenia z bazy wczytują się przy starcie.
3. Bez `convert`: dodatek nie ma własnych tabel ani danych inicjujących.

Aktualizacja: ponowny `extupdate` + restart. Wycofanie: `dbmgr extremove Claude A1.KalkulatorPodstawyZasilku.dll`.

## Dane testowe (baza Claude)

Generator: `ImportyXML/Generator kalkulatora podstawy zasilku/generuj.js` → `ImportyXML/Kalkulator podstawy zasilku - test 01..05 *.xml`
(zaimportowane `dbmgr importxml` 2026-10-16). Listy płac 11/2025–10/2026 dla ZP-01…ZP-12 nalicza się standardowo
(Pracownicy → zaznaczyć ZP-* → Nalicz wypłaty, definicja LPE), miesiąc po miesiącu.

| Kod | Scenariusz | Dane | Data niezdolności w kalkulatorze |
|---|---|---|---|
| ZP-01 | pełne 12 miesięcy | od 2024-01-01, 6000 + premia mies. 500 | 2026-11-02 |
| ZP-02 | zatrudnienie krótsze niż 12 mies. | od 2026-05-01, 7000 | 2026-11-02 |
| ZP-03 | brak pełnego miesiąca (podstawa z umowy) | od 2026-10-15, 6500 | 2026-11-16 (po okresie wyczekiwania) |
| ZP-04 | miesiąc z chorobą — uzupełnienie | 6000 + premia 500, choroba 8–12.06.2026 | 2026-11-02 |
| ZP-05 | miesiąc poniżej połowy czasu — wyłączony | 6000, choroba 2–27.03.2026 | 2026-11-02 |
| ZP-06 | premia kwartalna i roczna | 6000, kwartalna 3000 (co 3 mies.), roczna 12000 (co 12 mies.) | 2026-11-02 |
| ZP-07 | przerwa < 3 mies. (przejęcie podstawy) | 6000 → 7000 od 10/2026, choroba 14–18.09.2026 | 2026-11-02 |
| ZP-08 | przerwa ≥ 3 mies. (nowa podstawa) | 6000 → 7000 od 08/2026, choroba 15–19.06.2026 | 2026-11-02 |
| ZP-09 | zmiana wymiaru etatu (art. 40) | 1/1 8000 → 1/2 4000 od 08/2026 | 2026-11-02 |
| ZP-10 | podstawa minimalna (art. 45) | 1/1, 3500 (poniżej minimalnego) | 2026-11-02 |
| ZP-11 | przekroczenie rocznej podstawy składek E-R | 30000/mies. (limit przekroczony w 10/2026) | 2026-11-02 |
| ZP-12 | 50+: wynagrodzenie chorobowe 14 dni, potem zasiłek | ur. 1970, 6000 | 2026-11-02 … 2026-11-27 |

## Weryfikacja (etap 3)

Dla każdego scenariusza: wynik kalkulatora vs podstawa po faktycznym naliczeniu nieobecności na liście płac.
Wyniki i różnice: arkusz `Addon/KalkulatorPodstawyZasilku/Scenariusze testowe.xlsx` (po przebiegu testów).

## Przegląd kodu (safe-code, soneta-programming)

- Sesja symulacji w `using`, transakcja bez `Commit`, brak `Save()` — nic nie trafia do bazy.
- Sesja okna celowo bez `using` (żyje tyle co formularz, tylko do odczytu) — ten sam wzorzec co `KalkulatorWynagrodzenWorker` enova.
- Brak obiektów sesyjnych poza sesją symulacji (wynik = DTO), brak pól statycznych z obiektami sesji.
- Listener logu: wspólne `Trace.Listeners`, ale zapis tylko z wątku obliczenia; odpinany w `Dispose`.
- `catch (Exception)` tylko na granicy UI (komunikat w polu „Błąd wyliczenia”), z komentarzem.
- Dane czytane w zakresie pracownika (`pracownik.Nieobecnosci`, `pracownik.Elementy[okres]`); pełny skan tylko tabeli
  konfiguracyjnej `DefNieobecnosci` (lista wyboru).
- Odstępstwo świadome: kwoty w DTO jako `decimal` (silnik zwraca `decimal`, zawsze PLN).
