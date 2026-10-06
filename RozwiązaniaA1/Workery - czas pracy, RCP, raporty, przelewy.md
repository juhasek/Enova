# Dodatek „Workery” — opis biznesowy

Dokument opisuje skompilowany dodatek enova365 (`*.Workery.dll`, autor w metadanych: AltOne),
przekazany do analizy w katalogu `Pobrane/`. To **pakiet kilkunastu niezależnych funkcji**
dla działu kadr i płac sieci handlowej (sklepy + centra dystrybucyjne). Łączy je tylko to, że
siedzą w jednym DLL. Funkcje dzielą się na pięć obszarów:

1. **Czas pracy — reguły i blokady** przy wpisywaniu planu i czasu pracy (strefy
   „nieproduktywne”, limity godzin, zgody pracownika, norma okresu rozliczeniowego).
2. **Import zdarzeń RCP** z zewnętrznego systemu rejestracji czasu pracy.
3. **Uzupełnianie MPK** (centrum kosztów) na strefach, zestawieniach i nieobecnościach.
4. **Raporty i eksporty** — produktywność, BZU, użytkownicy platformy szkoleniowej, raporty
   wysyłane e-mailem.
5. **Kontrole kadrowe i eksport przelewów** — zwolnienie, urlop okolicznościowy, konto
   pracownika w enova365 Web, przelewy ISO 20022.

Analizowana wersja: `2512.9.11` (oznaczenie informacyjne `0.1.1.1Alfa`, kompilacja **Debug**),
zbudowana pod enova365 **2512.9.11**, .NET 8.

## 0. Metodologia i stopień pewności

DLL został w całości zdekompilowany do C# (`ilspycmd`, z referencjami do enova 2512.5.6
dostępnej lokalnie), a nazwy pozycji menu odczytano wprost z atrybutów w IL. Opis wynika
**z lektury kodu, nie z uruchomienia** — nie było bazy klienta ani dodatków, od których ten
DLL zależy (patrz §7). Wszystko, co jest wnioskiem, a nie wynika wprost z kodu, jest oznaczone
jako *(wniosek)*. Szczegóły zachowania w bazie trzeba potwierdzić na środowisku klienta.

---

## 1. Czas pracy — reguły wpisywania planu i czasu pracy

Najobszerniejsza część dodatku. Nie ma tu żadnego przycisku. Reguły włączają się same
(przez inicjalizator programu), gdy użytkownik zmienia godzinę „od” lub czas w planie albo
w czasie pracy pracownika, i działają jak weryfikatory przy zapisie.

### 1.1. Pojęcia, na których opierają się reguły

- **Strefa nieproduktywna („zdarzenie”)** — każda definicja strefy czasu pracy, która ma
  wypełnioną cechę **`Kategoria`**. Pozostałe strefy (bez kategorii) to **strefy
  produktywne**. Na tym podziale opierają się weryfikatory i raporty produktywności.
- **Limit** — cecha **`Limit`** (historyczna, typ czas) na definicji strefy. Określa, ile godzin
  danej strefy wolno wpisać w okresie dostępności. Wartość jest brana na dzień, którego
  dotyczy wpis.
- **MPK strefy** — cecha **`Projekt`** na strefie czasu pracy. Gdy jest pusta, przyjmuje się MPK
  wydziału z etatu pracownika (u zleceniobiorców: z wydziału umowy).
- **Stanowiska sklepowe**, na których opiera się ścieżka awansu: Sprzedawca → Starszy Sprzedawca
  → Zastępca Kierownika Sklepu (ZKS) → Kierownik Sklepu (KS). Porównanie nazw stanowisk jest
  tekstowe (bez rozróżniania wielkości liter).

### 1.2. Limity i okna dostępności stref nieproduktywnych (blokada zapisu)

Gdy reguła jest naruszona, zapis jest **blokowany** (weryfikator typu *Błąd*), a komunikat
kończy się zdaniem „Zablokowano możliwość wpisania czasu do wybranego zdarzenia”. Okno
dostępności to czas, w którym strefę w ogóle wolno wpisać. Limit to suma godzin tej strefy
w oknie.

| Strefa (nazwa definicji) | Od kiedy liczone okno | Długość okna | Dodatkowe warunki |
|---|---|---|---|
| On-Boarding Sprzedawca | data zatrudnienia | 2 mies. | stanowisko = Sprzedawca |
| On-Boarding Starszy Sprzedawca | data zatrudnienia | 3 mies. + 14 dni | stanowisko = Starszy Sprzedawca |
| On-Boarding ZKS/KS | data zatrudnienia | 8 mies. + 14 dni | stanowisko = KS lub ZKS |
| Awans na Starszy Sprzedawca | data zmiany stanowiska | 2 mies. + 14 dni | w historii wykryty awans na to stanowisko |
| Awans na ZKS | data zmiany stanowiska | 5 mies. | j.w. |
| Awans na KS | data zmiany stanowiska | 6 mies. (dostępność), limit liczony w 1 mies. | j.w. |
| Powrót po dł. nieob. (4-6mies) | dzień po końcu nieobecności | 30 **przepracowanych** dni | nieobecność trwała 4–6 mies. |
| Powrót po dł. nieob. (6+ mies) | j.w. | j.w. | nieobecność > 6 mies. |
| Szkolenie BHP dla kier. perso. | pierwszy wpis w cyklu | cykl 3 mies., wpis tylko w pierwszych 7 dniach cyklu | obowiązuje od 01.03.2025 |
| Szkolenie inne | pierwszy wpis w cyklu | cykl 3 mies. | obowiązuje od 01.03.2025 |
| pozostałe z kategorią „Szkolenie” | dzień wpisu | ±3 mies. | — |
| FLB | rok kalendarzowy | 1 rok | patrz uwaga w §8 |
| Roczna rozmowa okresowa KS / ZKS | ostatnia zmiana stanowiska, a gdy jej nie ma — rok wstecz | 1 rok, jedna rozmowa | stanowisko = odpowiednio KS / ZKS |
| Wyjście służbowe | dzień wpisu | 1 dzień | tylko gdy Limit ≠ 0 |
| Przebudowa sklepu, Zdarzenie losowe, Inne | dzień wpisu | ±6 mies. | tylko gdy Limit ≠ 0 |
| Mycie chłodni | pierwszy wpis **dla MPK sklepu** | cykl 6 mies. | limit wspólny dla sklepu, nie per pracownik; zakaz wpisu z wyprzedzeniem |
| Wdrożenie w sklepie KRS,KS,MD | pierwszy wpis **dla MPK sklepu** | 14 dni | j.w. |

Dodatkowe blokady dla stref on-boardingu i awansu:

- pracownik **ponownie zatrudniony mniej niż 30 dni** po poprzednim zatrudnieniu nie dostaje
  ponownie on-boardingu,
- po **wręczeniu wypowiedzenia** (data złożenia wypowiedzenia na etacie) nie można wpisać
  on-boardingu,
- jeśli w międzyczasie nastąpiła **aktualizacja stanowiska zgodna ze ścieżką awansu**,
  on-boarding jest zamykany („Nastąpiła aktualizacja stanowiska dnia …”),
- strefa awansu wymaga, by historia pracownika pokazywała faktyczny awans w dozwolonym kierunku.
  Zmiana „w dół” albo brak zmiany blokuje zapis.

**Powrót po długiej nieobecności wyklucza inne zdarzenia tego dnia.** Gdy w dniu jest strefa
„Powrót…”, a obok niej on-boarding, awans albo wdrożenie w sklepie, zapis pozostałych stref
jest blokowany. Długość nieobecności liczy się w miesiącach jako dni/30, przy czym sąsiadujące
nieobecności są łączone w jeden okres.

**Mycie chłodni i Wdrożenie w sklepie** to limity na poziomie **sklepu (MPK)**. Dodatek
prowadzi dla nich własne tabele godzin oraz tabele historii (dodanie, zmiana, usunięcie,
archiwizacja). Po upływie cyklu 6 miesięcy stare wpisy mycia chłodni są archiwizowane i liczenie
zaczyna się od nowa. Tabele te pochodzą z innego dodatku klienta (§7), a ten DLL tylko je
zasila.

### 1.3. Pozostałe kontrole dnia pracy

| Kontrola | Kiedy | Skutek |
|---|---|---|
| **Czas stref nieproduktywnych wypełniony** | każda strefa z kategorią musi mieć wpisany czas | blokada („Nie wszystkie strefy mają uzupełniony czas pracy”) |
| **Pełnienie funkcji kierowniczej** (strefa o kodzie `PFK`) | godziny PFK nie mogą przekroczyć sumy godzin pozostałych stref pracy w dniu | blokada z podaniem nadwyżki |
| **MPK w strefach nieproduktywnych** | przy zapisie sesji: dla każdego MPK suma godzin nieproduktywnych ≤ suma produktywnych na tym samym MPK, a MPK strefy nieproduktywnej musi występować wśród stref produktywnych dnia | blokada (wyjątek przy zapisie) |
| **Zgoda na pracę w nocy** (plan) | zaplanowane godziny nachodzą na porę nocną z kalendarza, a pracownik nie ma cechy `Zgoda na pracę w nocy` | blokada |
| **Zgody w czasie pracy** (czas rzeczywisty) | czas rzeczywisty w porze nocnej bez zgody na pracę w nocy **lub** dłuższy niż plan bez cechy `Zgoda na pracę w nadgodzinach` | ostrzeżenie (zapis możliwy) |

### 1.4. Norma okresu rozliczeniowego przy planowaniu

Przy zmianie planu pracy dodatek sprawdza, czy zaplanowany czas w **okresie rozliczeniowym
nadgodzin** nie przekracza normy. Gdy przekracza, rzuca błąd z kwotą dozwoloną
i zaplanowaną. Kontrola działa tylko:

- od miesiąca **06.2023**,
- dla operatorów, którzy mają rolę zawierającą w nazwie **„Kadry NET”**. Inni użytkownicy
  (np. kierownicy na pulpicie) nie są tą kontrolą objęci *(wniosek: celowo — chodzi
  o kontrolę planów zatwierdzanych przez kadry)*,
- przy wstawianiu nowego dnia dla etatu niepełnego jest korekta o różnicę 8 h × (1 − wymiar).

---

## 2. Import zdarzeń RCP

**Menu:** lista Pracownicy → *Import RCP (baza źródłowa)*.

Pobiera odbicia (wejścia/wyjścia) z bazy **Oracle systemu RCP** (tabela zdarzeń systemu GFOS)
i zapisuje je w enova jako **zdarzenia wejścia/wyjścia** pracownika (ewidencja
`WejsciaWyjsciaO`).

- **Parametry:** okres (od–do) **albo** „Importuj ostatnie 2 godz” oraz opcjonalnie wybrani
  pracownicy. Tryb „ostatnie 2 godziny” jest przeznaczony do cyklicznego uruchamiania z
  harmonogramu zadań *(wniosek z konstrukcji parametru)*.
- **Dopasowanie pracownika** — po **kodzie pracownika** w enova = numer osobowy w RCP.
  Nieznany kod jest pomijany z wpisem w logu.
- **Typ zdarzenia** — 1 = wejście, 2 = wyjście. Pobierane są też typy 3–5, ale zapisują się
  jako **wejście** (patrz §8).
- **Bez duplikatów** — zdarzenie o tej samej dacie i godzinie dla pracownika jest pomijane, więc
  import można bezpiecznie powtarzać.
- Każdy pracownik jest zapisywany w osobnej sesji, więc błąd u jednego nie blokuje pozostałych.
  Podsumowanie (pobrano / dodano / pominięto) trafia do okna logu „ImportRCP”.

---

## 3. Uzupełnianie MPK

Trzy czynności na liście Pracownicy w menu **inSolutions**. Wpisują MPK (nazwę centrum kosztów
z wydziału etatu) do cechy **`Projekt`**, z której korzystają weryfikatory z §1 i raporty z §4.

| Pozycja menu | Gdzie wpisuje MPK | Okres | Skąd MPK |
|---|---|---|---|
| Uzupełnij MPK do stref czasu pracy | strefy bazowe w rzeczywistym czasie pracy, dzień po dniu | zakres dat | wydział etatu obowiązującego **w danym dniu** |
| Uzupełnij MPK do zestawień czasu pracy | strefy zestawień czasu pracy za miesiąc | miesiąc | wydział z **ostatniego** zapisu historii |
| Uzupełnij MPK do nieobecności | nieobecności zawarte w miesiącu | miesiąc | wydział z **ostatniego** zapisu historii |

Wspólne opcje: **„Zamieniaj zdefiniowane”** (nadpisuje istniejący MPK, domyślnie uzupełniane są
tylko puste) i **„Tylko zaznaczone wiersze”** (domyślnie przetwarzani są **wszyscy** pracownicy
w bazie).

---

## 4. Raporty i eksporty

### 4.1. Raporty CSV na SFTP (produktywność, BZU)

**Menu:** lista Pracownicy → *Generuj drugi raport produktywności* / *Generuj raport BZU*
(parametr: okres). Pliki CSV trafiają na serwer **SFTP**, którego adres i dane logowania są
w ustawieniach innego dodatku klienta (§7).

Zakres danych to zawsze sklepy: wydziały z „sklep ” w nazwie i MPK zaczynające się od **„41”**.

- **Raport produktywności** (`/Produktywnosc/produktywnosc_<data>_M.csv`) — dla każdego MPK i dnia:
  wszystkie przepracowane godziny, godziny nieproduktywne (strefy z kategorią) i produktywne.
- **Raport BZU** (`/BZU/bzu_<data>_M.csv`) — zbiorczo per **MPK** (format `PL-001-<MPK>`)
  i **miesiąc**: godziny produktywne i nieproduktywne, godziny nieobecności, koszty chorobowe
  (dni opłacone elementem „Wynagr.chorobowe”), godziny nominalne, nadgodziny (praca ponad
  8 h lub ponad normę dnia), wykorzystany urlop i limit urlopu (bieżący + zaległy z limitu
  „Urlop wypoczynkowy”), saldo odchyłek, godziny nocne (jako „dopłaty”), godziny nadgodzin
  odebranych/rozliczonych (element „Rozliczenie odchyłek” z list płac). Domyślny okres to
  ostatnie 35 dni.
- W kodzie jest też **pierwszy** i **trzeci raport produktywności** — szczegółowe zestawienia
  per pracownik, dzień i strefę nieproduktywną, ze spółką regionalną, kierownikiem sklepu (KS)
  i kierownikiem rejonu (KRS). Nie mają pozycji w menu (nieaktywne w tej wersji).

### 4.2. Raport użytkowników platformy szkoleniowej (Litmos)

**Menu:** lista Pracownicy → *Utwórz raport Userdata_Litmos_PL*.

Buduje plik `Userdata_Litmos_PL_<ddMMyyyy>.csv` z kartoteką pracowników dla platformy e-learningowej
(Litmos) i wysyła go do **SharePoint** (Microsoft Graph). Opcjonalnie zapisuje go też na dysk.

- **Kto trafia do pliku** — pracownicy zatrudnieni w dniu raportu oraz pracownicy wyrejestrowani
  z ubezpieczeń w ciągu ostatnich 7 dni (każdy tylko raz, co pamięta cecha
  `LitmosLastReportWyrejData`). Pomijane są stanowiska magazynowe „Forklift Truck Driver”
  i „Logistics Support Team”.
- **Co zawiera** — login, e-mail, imię, nazwisko, status, data urodzenia, data zatrudnienia,
  data objęcia stanowiska, organizacja (sklep / centrum dystrybucyjne / biuro), ID sklepu,
  przełożony (wg struktury organizacyjnej „Struktura podległościowa” i osobnych reguł dla obu
  centrów dystrybucyjnych), czy jest kierownikiem, **angielski tytuł stanowiska**
  (mapowanie kilkudziesięciu polskich nazw stanowisk), informacja o długotrwałej nieobecności
  i jej długości w dniach.
- Datę ostatniego eksportu zapisuje w tabeli parametrów klienta w bazie.

### 4.3. Raport wysyłany e-mailem

**Menu:** lista Pracownicy → *ReportViaEmail* (parametry: okres, nazwa szablonu).

Odczytuje szablon wysyłki z tabeli konfiguracyjnej (dodatek z §7): ścieżkę raportu, ustawienia
w JSON, temat, treść i adresatów. Generuje raport do **PDF** i wysyła go jako załącznik pocztą
enova. Obsługiwane są trzy raporty:

- Ewidencja pracowników / **Zestawienie do nagrody jubileuszowej** (podstawa stażu, wymagany staż),
- Ewidencja pracowników / **Ewidencja wypadków**,
- **Zatrudnienia w okresie** (raport klienta; filtr: jednostka organizacyjna, stanowisko, MPK).

*(wniosek)* Funkcja jest pomyślana do uruchamiania z harmonogramu zadań, bo nie wymaga zaznaczenia
pracowników — raport obejmuje wszystkich.

### 4.4. Rozliczenie ujemnego czasu z „magazynu nadgodzin”

**Menu:** lista Pracownicy → *KiP serwisowe / Rozliczenie ujemnego czasu* (parametr: okres,
wymagane zaznaczenie pracowników).

Woła procedurę SQL **`sp_SettleWorkTime`** w bazie enova dla zaznaczonych pracowników. **Cała
logika rozliczenia jest w procedurze, nie w DLL**, więc z samego dodatku nie da się powiedzieć,
co dokładnie robi z saldem. Trzeba ją przeczytać w bazie klienta.

---

## 5. Kontrole kadrowe

| Funkcja | Kiedy działa | Co robi |
|---|---|---|
| **Kompletność danych przy zwolnieniu** | zaznaczenie „Pracownik zwolniony” na etacie | ostrzeżenie, jeśli brak: przyczyny wypowiedzenia, podstawy prawnej, kodu zwolnienia, inicjatywy rozwiązania umowy albo wyrejestrowania ze **wszystkich** ubezpieczeń (emerytalne, rentowe, chorobowe, wypadkowe, zdrowotne) |
| **Urlop okolicznościowy — „Inna przyczyna” zablokowana** | edycja przyczyny lub okresu wniosku o urlop okolicznościowy | blokada zapisu, jeśli wybrano przyczynę „Inna przyczyna” |
| **Konto pracownika w enova365 Web** | utworzenie użytkownika Web dla pracownika | login = **kod pracownika**, hasło pierwszego logowania = **PESEL** |

---

## 6. Eksport przelewów (ISO 20022)

**Menu:** lista Przelewy → *[nazwa klienta] Eksport przelewów*.

Zastępuje standardowy eksport przelewów wersją dostosowaną do banku klienta. Działa **tylko** dla
formatu wymiany **ISO 20022** (inny format kończy się błędem). Kolejno:

1. Przygotowuje plik standardowym serializerem enova i sprawdza rachunki na białej liście VAT.
2. Poprawia XML pod wymagania banku:
   - usuwa cudzysłowy z nazw zleceniodawcy,
   - dopisuje **adres siedziby firmy** (z konfiguracji *Podstawowe/Firma/Adres siedziby*),
   - zamienia identyfikator rozliczeniowy banku na **kod BIC/SWIFT** (rachunku firmy
     i rachunku każdego odbiorcy),
   - ustawia **`BtchBookg = false`**, czyli każdy przelew osobno na wyciągu, oraz liczbę i sumę
     kontrolną transakcji.
     *(wniosek: istotne dla wypłat — pojedyncze pozycje zamiast jednej zbiorczej kwoty).*
3. Oznacza przelewy jako **wyeksportowane i zatwierdzone** i zapisuje numer paczki.

W kodzie jest też wariant eksportu „przez API” oraz pomocnik zwracający pojedynczy przelew
w Base64. Nie są podpięte do menu *(wniosek: przygotowanie pod przyszłą integrację z bankiem
przez API)*.

---

## 7. Zależności — czego dodatek wymaga do działania

DLL **nie działa samodzielnie**. Wymaga:

- **innego dodatku klienta z ustawieniami** — konfiguracja SFTP, tabela szablonów raportów
  wysyłanych e-mailem,
- **dodatku z tabelami klienta** — godziny i historia „Mycia chłodni” oraz „Wdrożenia
  w sklepie” (dla tabel wdrożenia dodatek sam nadaje pełne prawa i zdejmuje tylko-do-odczytu,
  żeby weryfikator mógł je zapisywać niezależnie od uprawnień operatora),
- **obiektów SQL w bazie enova:** procedura `sp_SettleWorkTime`, procedura zwracająca KS/KRS
  dla pracownika na dzień (raporty produktywności), tabela parametrów klienta,
- **konfiguracji w enova:**
  - cechy definicji strefy `Kategoria` i `Limit` (historyczna),
  - cecha strefy, zestawienia i nieobecności `Projekt`,
  - cechy pracownika `Zgoda na pracę w nocy`, `Zgoda na pracę w nadgodzinach`
    i `LitmosLastReportWyrejData`,
  - strefa o kodzie `PFK`,
  - definicje stref o **dokładnie takich nazwach jak w §1.2**,
  - struktura organizacyjna z elementami typu „Spółka”,
  - rola operatora „Kadry NET”,
  - definicje: nieobecność „Urlop wypoczynkowy”, limit „Urlop wypoczynkowy”, elementy
    „Wynagr.chorobowe” i „Rozliczenie odchyłek”,
- **dostępu sieciowego** z serwera enova do bazy Oracle systemu RCP, serwera SFTP i Microsoft
  Graph (SharePoint).

Większość reguł opiera się na **nazwach** (stref, stanowisk, wydziałów, elementów), a nie na
identyfikatorach. Zmiana nazwy w konfiguracji po cichu wyłącza regułę — bez błędu.

---

## 8. Uwagi i ryzyka wychwycone w kodzie

1. **Dane logowania zaszyte w kodzie.** W DLL są wprost: login i hasło do bazy Oracle systemu
   RCP oraz identyfikator i sekret aplikacji Azure (SharePoint) jako domyślne wartości
   parametrów. Każdy, kto ma plik DLL, może je odczytać. Zalecenie: przenieść do konfiguracji
   i **zmienić te hasła/sekrety**. Wartości celowo nie są tu przytoczone.
2. **Hasło pierwszego logowania = PESEL** (§5) — znane współpracownikom i kadrom, więc słabe.
3. **Kompilacja Debug, wersja „Alfa”** — nie wygląda na wydanie produkcyjne.
4. **FLB — limit roczny prawdopodobnie nie działa.** Okno roczne liczone jest od roku „pustej”
   daty, więc suma godzin w oknie zawsze wynosi zero i limit nie jest przekraczany. Wymaga
   potwierdzenia testem.
5. **Awans na KS** — dostępność strefy to 6 miesięcy, a limit liczony jest tylko w pierwszym
   miesiącu po awansie. Możliwa niespójność z założeniami biznesowymi.
6. **Import RCP — typy zdarzeń 3–5** są pobierane, ale zapisywane jako „wejście”. Jeśli
   w systemie RCP oznaczają np. wyjście służbowe lub przerwę, ewidencja może być zafałszowana.
7. **Import RCP — wybrani pracownicy** są wstawiani do zapytania jako surowy tekst kodu.
   Działa tylko dla kodów numerycznych.
8. **Rozliczenie ujemnego czasu** — logika w procedurze SQL poza DLL, niewidoczna w tej analizie.
9. **Uzupełnianie MPK bez zaznaczenia** przetwarza **wszystkich** pracowników w bazie (także
   zwolnionych), co przy dużej bazie bywa długie.
10. Raport e-mail obsługuje tylko trzy raporty wymienione w §4.3. Inny szablon wygeneruje
    raport bez parametrów.
