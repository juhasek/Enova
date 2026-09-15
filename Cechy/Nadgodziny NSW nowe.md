# Nadgodziny NSW nowe – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Kontekst i cel

Czwarta, ostatnia cecha z „nowego” zestawu — kontekst biznesowy opisuje
`Cechy/Nadgodziny 50 nowe.md` pkt 1. Ten dokument opisuje tylko to, co specyficzne dla NSW.

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`), dla stref „Praca poza normą” / „Praca poza normą awaria”. Liczy
godziny takiej pracy przypadające w dniu niedzielno-świątecznym — rozliczane jako
**nadgodziny NSW**, nie jako dobowe 50%/100% ani okresowe.

Odpowiednik istniejącej cechy `Cechy/Nadgodziny NSW` — **ta cecha nie jest i nigdy nie była
zgłoszona jako błędna**. „Nadgodziny NSW nowe” powstaje mimo to, żeby dopełnić spójny,
równoległy zestaw czterech cech (50/100/okresowe/NSW nowe) korzystających z tej samej,
jednolitej definicji dnia i normy co reszta zestawu — i żeby dało się porównać wynik ze
starą cechą na tych samych danych.

## 2. Algorytm

1. Ustalenie, czy dzień kwalifikuje się jako niedzielno-świąteczny dla nadgodzin —
   **dokładnie tak, jak robi to sam silnik systemu**
   (`KalkulatorPracownika.Nadgodziny`, zdekompilowany, patrz
   `reference_enova_nadgodziny_kalkulator`): zależnie od globalnej konfiguracji
   `Config.Nadgodziny.Dobowe100`:
   - gdy **włączona** → dzień typu `Wolny` lub `Świąteczny` kwalifikuje się,
   - gdy **wyłączona** (stan w bazie Claude) → liczy się wyłącznie flaga
     `Definicja.NadgodzinySW` dnia (węższy warunek niż stara cecha — patrz pkt 3).
2. Jeśli dzień się kwalifikuje i praca w dniu (`dzienPracy.Czas`) przekracza plan
   (`dzienPlanu.Czas`, zwykle `0` w takim dniu) — **cała** wartość `Row.Czas` (nie tylko
   nadwyżka) liczy się jako NSW. Tak jak w silniku: w dniu bez planu każda godzina pracy
   „poza normą” to nadgodziny NSW, nie tylko część ponad jakąś normę.

## 3. Różnica względem starej cechy „Nadgodziny NSW”

Stara cecha kwalifikowała dzień warunkiem `Typ == Świąteczny || NadgodzinySW` (suma logiczna
obu warunków, zawsze, niezależnie od konfiguracji `Dobowe100`). Silnik systemu w
rzeczywistości **przełącza się** między tymi dwoma warunkami zależnie od `Dobowe100` — przy
`Dobowe100 = false` (stan bazy Claude) używa **wyłącznie** `NadgodzinySW`, nie sprawdza typu
dnia wprost. W praktyce różnica ujawni się tylko, jeśli w kalendarzu istnieje dzień typu
`Świąteczny`, który **nie** ma ustawionej flagi `NadgodzinySW` — dla takiego dnia stara
cecha zwróci NSW, nowa (zgodnie z silnikiem) zwróci `0` (godziny trafią wtedy do
`Nadgodziny okresowe nowe`, bo tam wykluczenie jest po samym `Typ == Świąteczny`, szerzej
niż tu). W bazie Claude nie sprawdzono, czy taki przypadek (Świąteczny bez NadgodzinySW)
w ogóle występuje w danych klienta — do zweryfikowania przy porównaniu wyników obu cech.

## 4. Status: NIEZWERYFIKOWANE

Patrz `Cechy/Nadgodziny 50 nowe.md` pkt 6 — te same ograniczenia środowiska (brak
buscall/GUI, brak kompilacji na żywo) dotyczą tej cechy.
