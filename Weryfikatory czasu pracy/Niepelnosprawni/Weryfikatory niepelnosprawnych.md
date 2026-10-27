# Weryfikatory osób z niepełnosprawnością

Wydzielone z `../KontrolePlanuZAlgorytmuKalendarza.md` (sekcje 8, 8a, 8b). Kod metod jest w `../KontrolePlanuZAlgorytmuKalendarza` (jeden plik = CodeFiles 13 w bazie). W tym katalogu: instrukcja dla klienta `Instrukcja_Weryfikatory_niepelnosprawnych.docx`, scenariusze `Scenariusze testowe weryfikatorow niepelnosprawnych.xlsx` (arkusze „Brak zgody 8h”, „Zgody nadgodziny i noc”, „Norma tygodniowa 35-40h”) i dane testowe XML. Te same sekcje i arkusze są nadal we wspólnych `../Instrukcja_Weryfikatory.docx` i `../Scenariusze testowe weryfikatorow czasu pracy.xlsx`.

## 8. Norma niepełnosprawnych

Sprawdza tylko normę dobową. Nie ma limitu tygodniowego (35h / 40h) ani zakazu pracy w nocy
i w nadgodzinach.

**Poprawione (2026-10-06):** wejście `(KalkulatorPlanuBase, Date)` i komunikat przez `TranslateFormat`
(na wzór wbudowanej „Normy dobowej”). Stopień i zgoda na 8h czytane z zapisu historii
obowiązującego w dniu (`pracownik[data]`), a nie z pierwszego pasującego zapisu. Stopień porównywany
z enumem `StNiepełnosprawności`, a nie tekstem. Brany wyższy stopień z danych o niepełnosprawności
i z danych PFRON, okres `OkresExt` (jak enova przy limicie urlopu). Definicja 2039 pomija dzień
kalendarza bez pracownika.

**Nowy weryfikator (2026-10-13): definicja 2069 „A1_Brak zgody na pracę 8h”** (Dzień planu, priorytet 29),
podpięta do kalendarzy 45 i 47 jako **Błąd**, więc planu nie da się zapisać. Metoda `A1BrakZgodyNaPrace8h(kp, data)`
w tej samej klasie. Reguła: pracownik ze stopniem **umiarkowanym lub znacznym** (dane o niepełnosprawności
albo PFRON, wyższy z nich, okres `OkresExt`), który w zapisie historii obowiązującym w dniu nie ma
zaznaczonego na etacie pola „Zgoda na pracę 8h” (`Etat.PracownikNiepelnosprawnyZgodaNaPrace8h`),
nie może mieć zaplanowanych więcej niż 7:00 w dniu. Stopień lekki i brak stopnia nie są sprawdzane,
bo u osób bez niepełnosprawności to pole też jest puste. Definicja 2039 (Ostrzeżenie, także 8h dla stopnia
lekkiego) zostaje bez zmian, zablokowana; przy jej odblokowaniu oba komunikaty pojawią się razem.
Wywołanie w definicji:
`var pracownik = dzien.Kalendarz.Pracownik; if (pracownik == null) return null;
return K.A1BrakZgodyNaPrace8h(new KalkulatorPlanu(pracownik), dzien.Data);`.
Podpięcia wstawione SQL-em — widoczne po restarcie serwera enova.

**Test na żywo (2026-10-15, baza Al): 9 z 10 scenariuszy pozytywnie** — arkusz „Brak zgody 8h”
w `Scenariusze testowe weryfikatorow czasu pracy.xlsx`, pracownicy testowi NP1–NP9
(`Weryfikator brak zgody 8h - pracownicy i plan.xml` (ten katalog), orzeczenia dodane w programie).
Działa: umiarkowany/znaczny bez zgody > 7:00 → Błąd, granica 7:00, zgoda 8h, stopień lekki, brak
orzeczenia, orzeczenie ZUS (odpowiadający stopień), kalendarz równoważny.

**Błąd (scenariusz 8):** w enova 2610 `PracHistoria.StopienNiepelnosp` i `PracHistoria.PFRON` są
przestarzałymi nakładkami (do usunięcia w 2710) na tabele `OrzeczeniaPFRON` / `OrzeczeniaZUS`
i zwracają **jedno** orzeczenie obowiązujące w okresie zapisu historii — to z najpóźniejszą datą
„Obowiązuje od”. Gdy stopień zmienia się w trakcie zapisu historii (np. umiarkowany do 15.11, lekki
od 16.11), dla dni sprzed zmiany weryfikator widzi orzeczenie późniejsze, którego okres nie obejmuje
dnia → stopień „Brak” → brak błędu. Dotyczy też definicji 2039. Poprawka: szukać orzeczenia, którego
okres (`OkresExt`) obejmuje dany dzień, w `pracownik.OrzeczeniaPFRON` i `pracownik.OrzeczeniaZUS`
(nie poprawione — do decyzji).

**Poprawione (2026-10-19):** w obu metodach (2039 i 2069) stopień liczony metodą enova
`pracownik.StopienNiepelnosprawnosciNaDzien(data).Stopien`. Metoda wybiera orzeczenie PFRON
i ZUS obowiązujące w danym dniu (`OkresExtObow`, a przy kilku kandydatach to, którego własny
okres obejmuje dzień) i zwraca wyższy stopień. Zgoda na 8h nadal z `pracownik[data].Etat`.
Kompilacja całej klasy na bibliotekach 2610 (.NET 10) bez błędów, kod wgrany do `CodeFiles` 13,
stamp `RuntimeProjects` 29 podbity. **Potwierdzone na żywo (2026-10-19, po restarcie serwera):**
NP8 13.11 = 8:00 → błąd 2069 ze stopniem „Umiarkowany”; 16.11 = 8:00 (lekki) → bez błędu 2069.
Scenariusz 8 w arkuszu „Brak zgody 8h” zmieniony na pozytywny (10/10).

**Wersja klienta 2512.10.12 (2026-10-20) — stopień znowu z zapisu historii.** Klient ma enova
2512.10.12: tam **nie ma** `Pracownik.StopienNiepelnosprawnosciNaDzien` ani tabel
`OrzeczeniaPFRON` / `OrzeczeniaZUS` — kod z 19.10 nie kompilował się (6 wywołań, a to jeden plik
`CodeFiles` z wszystkimi weryfikatorami kalendarza). W 2512 stopień przechowywany jest w każdym
zapisie historii pracownika: `PracHistoria.StopienNiepelnosp` (dane ubezpieczeniowe) i
`PracHistoria.PFRON` (`Stopien`, `OkresExt`), bez oznaczenia „przestarzałe”. Wszystkie weryfikatory
liczą teraz stopień jedną metodą pomocniczą `A1StopienNiepelnosprawnosciNaDzien(pracownik, data)`:
zapis historii obowiązujący w dniu, wyższy z obu stopni, jeśli `OkresExt` obejmuje dzień (tak jak
przed 19.10). W 2512 każdy zapis historii ma własne dane, więc problem scenariusza 8 nie występuje;
w bazie testowej 2610 (nakładka) scenariusz 8 znowu jest negatywny. Kompilacja całej klasy z bazy:
2512.10.12 (pakiety NuGet) — 0 błędów, 0 ostrzeżeń; 2610 („Sprawdź kod” w Al) — 0 błędów,
5 ostrzeżeń „przestarzały”. Wgrane do `CodeFiles` 13 + stamp 29 + „Wymuś rekompilację”.

**Uwaga z testu:** weryfikator normy okresu rozliczeniowego (definicja 37) liczy normę ogólną także
dla osób ze stopniem umiarkowanym/znacznym bez zgody (norma 7h/dobę, 35h/tydzień) — dla nich oba
weryfikatory się wykluczają (niedobór normy okresu każe wydłużyć dzień do 8:00, 2069 blokuje > 7:00).

**Decyzja (2026-10-19): bez zmian w kodzie.** Definicja 37 liczy normę metodą enova
`KalkulatorKodeksowyPracownika.Norma` = norma dobowa kalendarza z etatu × dni robocze × wymiar, a maksimum
dnia z „Nadgodziny – wartość dobowa” kalendarza. Klient ma kalendarze „8:15” z normą 7:00 i limitem
nadgodzin 7:00 / 35:00 (ID 1070, 1072, 1074, 1077). Na takim kalendarzu norma okresu wynosi 7h × dni
i konflikt z 2069 nie występuje. Zasada: osoba ze stopniem umiarkowanym/znacznym bez zgody na pracę 8h
ma kalendarz 8:15. Konflikt w teście wynikał z danych testowych (NP1–NP9 na kalendarzu 47, norma 8h).
Do wyjaśnienia z klientem: kalendarz 1080 „Zadaniowy system_1_8:15_N” ma normę dobową 8:00 przy limicie
nadgodzin 7:00. Na kalendarzach równoważnych 8:15 podpowiedź 37 może proponować wydłużenie dnia
do 12h (blokowane przez 2069) — tylko treść podpowiedzi, niezmieniane.

## 8a. Zgody osoby z niepełnosprawnością: nadgodziny i pora nocna (2026-10-19)

Trzy nowe weryfikatory, podpięte do tych samych 14 kalendarzy co 2069 (45, 47, 1070–1081).
Dotyczą **każdego** stopnia niepełnosprawności (z zapisu historii obowiązującego w dniu,
`A1StopienNiepelnosprawnosciNaDzien` — zob. „Wersja klienta 2512.10.12” w sekcji 8) — enova udostępnia
pola zgód na etacie przy każdym stopniu (w 2512: `Etat.IsReadOnlyPracownikNiepelnosprawnyZgoda...` =
`Historia.StopienNiepelnosp.Kod == Brak`).
Zgody czytane z zapisu historii obowiązującego w dniu (`pracownik[data].Etat`).

| Def. | Nazwa | Rodzaj | Poziom | Metoda | Reguła |
|---|---|---|---|---|---|
| 2070 | A1_Brak zgody na nadgodziny | Dzień pracy | Ostrzeżenie | `A1BrakZgodyNaNadgodziny(dzien)` | brak `PracownikNiepelnosprawnyZgodaNaPraceNadgodziny`; czas pracy dnia (strefy `Wchodzi` i `Zwieksza`) > większa z wartości: plan dnia, `Etat.NormaDobowa`; w dniu wolnym wg planu — każda praca |
| 2071 | A1_Brak zgody na pracę w nocy (plan) | Dzień planu | **Błąd** | `A1BrakZgodyNaPraceWNocyPlan(kp, data)` | brak `PracownikNiepelnosprawnyZgodaNaPraceWPorzeNocnej`; strefy planu dnia przecinają porę nocną |
| 2072 | A1_Brak zgody na pracę w nocy (czas) | Dzień pracy | Ostrzeżenie | `A1BrakZgodyNaPraceWNocyCzasPracy(dzien)` | jw. dla stref czasu pracy |

Pora nocna: metoda enova `KalkulatorPracyBase.NocOkres(PracHistoria)` (Od/Do z `Kalendarz.Nocne`
etatu albo z konfiguracji, z obsługą przejścia przez północ), przecięta ze strefami wliczanymi do
czasu pracy; doliczana jawna strefa „Praca w godzinach nocnych” (`DefinicjaStrefy.Nocne`). Nie
korzysta z `KalkulatorPracy.Nocne`, bo ten przy `Nocne.Rozliczaj = false` widzi tylko jawną strefę.
Wspólny helper `A1CzasWPorzeNocnej` (prywatny).

Wgrane SQL-em w jednej transakcji (CodeFiles 13, DefWeryfKalend 2070–2072, 42 podpięcia WeryfKalend,
stamp RuntimeProjects 29). Kompilacja całej klasy na 2610 (.NET 10) bez błędów. Podpięcia widoczne
po restarcie serwera enova. Test na żywo: do zrobienia.

## 8b. Norma tygodniowa 35h / 40h osoby z niepełnosprawnością (2026-10-19)

Dwie nowe definicje podpięte do tych samych 14 kalendarzy co 2069 (45, 47, 1070–1081) oraz do
kalendarza testowego 1086. Poziom (Błąd / Ostrzeżenie) to ustawienie podpięcia na zakładce
„Weryfikatory kalendarza”, więc zmienia się go bez zmian w kodzie.

| Def. | Nazwa | Rodzaj | Poziom | Metoda |
|---|---|---|---|---|
| 3070 | A1_Norma tygodniowa niepełnosprawnych | Plan pracy | **Błąd** | `A1NormaTygodniowaNiepelnosprawnych(kp, args.Miesiąc)` |
| 3071 | A1_Doba niepełnosprawnych (równoważny) | Dzień planu | Ostrzeżenie | `A1DobaNiepelnosprawnegoWRownowaznym(kp, data)` |

**Norma osoby (d = dobowa, W = 5 × d):** umiarkowany/znaczny bez zgody na pracę 8h (pole na etacie)
→ 7h/35h; lekki oraz umiarkowany/znaczny ze zgodą 8h → 8h/40h. Stopień i zgoda z zapisu historii
obowiązującego w danym dniu (`A1StopienNiepelnosprawnosciNaDzien`, zob. „Wersja klienta 2512.10.12”
w sekcji 8). Brak stopnia → nic nie sprawdza.

**Tygodnie:** 7 kolejnych dni od pierwszego dnia okresu rozliczeniowego (`pracownik.WyliczOkresRoliczeniowyNadgodzin`,
art. 128 § 3 pkt 1 KP). Tak samo liczy standardowy weryfikator odpoczynku tygodniowego enova. Tygodnie
nie zaczynają się od nowa na granicy miesięcy, a ostatni tydzień okresu może być niepełny.

**Godziny planowane:** `KalkulatorPlanu(pracownik).Norma(...)`, czyli strefy planu dnia z bazy razem
ze zmianami z bieżącej sesji. Kalkulator planu nie odejmuje nieobecności (rozlicza je kalkulator pracy),
tak samo jak standardowa „Norma w okresie rozliczeniowym”.

**Kalendarz podstawowy** (`Kalendarz.RownowaznyCzasPracy = false`, także zadaniowy): każdy tydzień ≤ W,
liczone sztywno, a niepełny tydzień na końcu okresu też ≤ W. Dodatkowo cały okres ≤ wymiar.
**Kalendarz równoważny:** tylko cały okres ≤ wymiar (norma średnia).

**Wymiar** to metoda enova `KalkulatorKodeksowy.Norma` (art. 130 KP: 40h × pełne tygodnie + 8h × dni
pon–pt niepełnego tygodnia − 8h × święta poza niedzielą), policzona dla każdego tygodnia okresu
i przeskalowana przez d/8. Przykład: X 2026 to 4 × 35 + 2 × 7 = 154h (W = 35) albo 176h (W = 40).
IX–XI 2026 to 13 × 35 − 7 (11.11) = 448h. Dni poza zatrudnieniem są pomijane (jak w 37). Wymiar etatu
nie zmniejsza limitu.

**Zmiana orzeczenia w trakcie okresu:** norma liczona dla każdego dnia. Tydzień, w którym normy dni się
różnią, dostaje normę dobową równą średniej z dni pon–pt, np. pon–wt po 7h i śr–pt po 8h daje W = 38h.
Zmiana w weekend nie obniża limitu tygodnia. Dzień bez orzeczenia liczony jest normą ogólną 8h/40h,
a tydzień bez żadnego dnia z orzeczeniem nie jest sprawdzany.

**Zakres:** sprawdzane są okresy rozliczeniowe obejmujące zapisywany miesiąc (`args.Miesiąc`), a tygodnie
tylko te, które ten miesiąc przecinają. Komunikat jest jeden, dla pierwszego przekroczenia: najpierw tydzień,
potem wymiar okresu.

**3071 (Ostrzeżenie):** na kalendarzu równoważnym dzień planu dłuższy niż d. Przy umiarkowanym/znacznym
bez zgody i dniu powyżej 7h odzywa się też 2069 (Błąd).

**Ograniczenia:** weryfikatory kalendarza enova uruchamiają się przy zapisie planu, a nie przy zmianie
kalendarza na etacie ani przy zmianie orzeczenia. Po takiej zmianie kontrola zadziała dopiero przy
najbliższej edycji planu w danym miesiącu.

Wgrane SQL-em w jednej transakcji (CodeFiles 13, DefWeryfKalend 3070–3071, po 14 podpięć, stamp
RuntimeProjects 29). Kompilacja całej klasy na 2610 (.NET 10) bez błędów. Dane testowe: kalendarz
testowy 1086 „TEST Podstawowy_3_8:15_N” (kopia 1072, okres 3 mies. od 1.03.2024) dla NP3,
NP4 przeniesiony na 1076, a dni planu w `Weryfikator norma tygodniowa niepelnosprawnych - plan.xml` (ten katalog).

**Test na żywo (2026-10-19, baza Al, po restarcie serwera): wszystkie scenariusze pozytywne** (arkusz
„Norma tygodniowa 35-40h” w `Scenariusze testowe weryfikatorow czasu pracy.xlsx`): podstawowy 35h OK /
tydzień 36h Błąd, tydzień 29.09–05.10 na przełomie miesięcy w okresie 3-mies., tydzień czw–śr (nie pon–nd),
równoważny tydzień 42h OK / okres 449 > 448 Błąd (11.11 odjęte), lekki 41 > 40 Błąd, zgoda 8h 177 > 176 Błąd
+ ostrzeżenie 3071, brak orzeczenia bez komunikatu, zmiana orzeczenia 16.11 (tydzień 15–21.11 40h OK, 1–7.11 36h Błąd).
Decyzja użytkownika 2026-10-19: przy kalendarzu podstawowym **Błąd**.

Uwagi z testu: kalendarze klienta „_N” nie generują planu z definicji kalendarza (dni bez wpisu = 0h),
więc dane testowe mają jawny plan każdego dnia. Kalendarz 1086 skopiowany SQL-em z niepustym polem `Algorytm`
dawał NRE w `Kalendarz.GetTools()` przy zapisie dnia planu; pole wyczyszczone (algorytm zawierał same `return null`).
Opis dla klienta: dwie nowe sekcje w `Instrukcja_Weryfikatory.docx`.

