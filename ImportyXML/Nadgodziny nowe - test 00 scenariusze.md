# Nadgodziny 50/100/okresowe/NSW nowe — dane testowe (baza `Claude`)

Pliki importu XML zasilające bazę testową **`Claude`** danymi pod scenariusze testowe
zestawu cech `Cechy/Nadgodziny 50 nowe`, `100 nowe`, `okresowe nowe`, `NSW nowe`
(`FeatureDefs` ID 13–16, `StrefaPracy`). Pełny opis scenariuszy, oczekiwanych wyników i
instrukcja sprawdzenia w GUI: [`Cechy/Nadgodziny nowe - scenariusze testowe.xlsx`](../Cechy/Nadgodziny%20nowe%20-%20scenariusze%20testowe.xlsx).

## Środowisko

Baza `Claude` na `localhost\SQLEXPRESS` (= `ET-097-vm2\SQLEXPRESS`), enova **2512.5.6**.

```
C:\enovaServer\2512.5.6\Soneta.Products.Server.Standard\dbmgr.exe importxml Claude "<plik>" --standard
```

**Kolejność importu jest wymagana** (twarde zależności FK, import wg rekordów nie
przestawia kolejności):

| # | Plik | Zawartość |
|---|---|---|
| 1 | `Nadgodziny nowe - test 01 wzorzec kalendarza.xml` | Nowy kalendarz **wzorcowy** „Test nadgodziny nowe” (Kalendarze ID 28) — kopia „Standard” z dopisanym oknem nocnym 22:00–6:00 (limit 8:00). Musi istnieć przed importem pracowników (Etat.Kalendarz to twardy FK). |
| 2 | `Nadgodziny nowe - test 02 pracownicy.xml` | 8 kompletnych pracowników etatowych `NG-01`…`NG-08`, zatrudnieni od 2026-01-01, `Etat.Kalendarz` = wzorzec z pliku 1. |
| 3 | `Nadgodziny nowe - test 03 plan pracy.xml` | Plan dnia (`DzienPlanu`) po jednym dniu na pracownika (2026-11-02…2026-11-09), dobrany pod każdy mechanizm cechy. |
| 4 | `Nadgodziny nowe - test 04 dane rzeczywiste.xml` | Rzeczywisty czas pracy (`DzienPracy`/`StrefyPracy`) — dane, które trzeba otworzyć w GUI, żeby zobaczyć wartości cech. |

Pliki 1–3 mają stałe GUID-y i są idempotentne. **Plik 4 NIE jest idempotentny** —
`DniPracy`/`StrefyPracy` nie są guidowane (jak `ImportyXML/ALDI RCP - dane RCP.xml`);
ponowne uruchomienie dopisze duplikaty zamiast je zaktualizować.

## Odkrycie po drodze: skąd cecha bierze normę/Nocne/Nadgodz50

`Etat.NormaDobowa` (surowe pole `PracHistorie.EtatNormaDobowa`) jest `0` dla **wszystkich**
pracowników w bazie `Claude`, także tych już istniejących (`TS-01`…`TS-09`, `0001`) —
efektywna norma dobowa używana przez silnik (i przez cechy „nowe”) pochodzi więc z
`Etat.Kalendarz` (kalendarz **wzorcowy**, np. „Standard” = 480 min, „Podstawowy -
Orzeczenie Niep.” = 420 min), NIE z indywidualnego „Kalendarza pracownika” (`Kalendarze.Typ
= KalendarzPracownika`) — ten drugi (potwierdzone SQL-em) ma pola `Nadgodziny*`/`Nocne*`
zerowe u wszystkich sprawdzonych pracowników i służy wyłącznie jako pojemnik na wyjątki
planu (`DzienPlanu`). Dlatego scenariusze wymagały **nowego kalendarza wzorcowego**
zamiast modyfikacji indywidualnych kalendarzy — modyfikacja wspólnego „Standard” zmieniłaby
zachowanie dla `TS-01`…`TS-09`/`0001`, więc powstał osobny wzorzec.

Indywidualny „Kalendarz pracownika” (`Kalendarze.Typ=KalendarzPracownika`) **nie jest**
tworzony ręcznie w tych plikach — powstaje **sam**, automatycznie (logika `OnAdded` przy
zapisie `Etat`), gdy tylko plik 2 zapisuje `PracHistoria.Etat`. Próba jawnego dopisania
własnego takiego kalendarza kończy się `DuplicateKeyException` na unikalnym indeksie
`Kalendarze_Podstawowy (Typ, Nazwa)`.

## Znaleziona rozbieżność (do decyzji, nie poprawiona)

Scenariusz `NG-08` (dzień „Pracy w święto” — `Typ=Świąteczny`, ale **bez** flagi
`NadgodzinySW`) pokazuje, że przy obecnej implementacji **wszystkie cztery** nowe cechy
zwracają `0` dla strefy „Praca poza normą” tego dnia: `50 nowe`/`100 nowe` wykluczają cały
`Typ==Świąteczny`, a `NSW nowe` przy `Config.Nadgodziny.Dobowe100=false` (stan bazy Claude)
sprawdza wyłącznie `NadgodzinySW`, której tu brak. Godziny „znikają” z rozbicia per-strefa.
Szczegóły w arkuszu, wiersz `NG-08` (podświetlony). Nie zmieniano kodu cech w ramach tego
zadania — do porównania ze starym zestawem cech i decyzji klienta.

## Stan weryfikacji

- Import wszystkich 4 plików przechodzi bez błędów; dane widoczne w `Pracownicy`,
  `PracHistorie`, `Kalendarze`, `DniKalendarza`/`StrefyKalandarza`, `DniPracy`/`StrefyPracy`
  (sprawdzone SQL-em co do minuty zgodnie z projektem scenariuszy — 2026-09-16).
- **NIE zweryfikowano przeliczeniem w GUI** — środowisko robocze nie ma buscall/GUI (patrz
  `project-srodowisko-lokalne`). Same cechy (`Cechy/Nadgodziny *.nowe`) są też oznaczone jako
  „NIEZWERYFIKOWANE” w swoich `.md` — dane testowe są gotowe, ale kod cech nie został jeszcze
  przeliczony na żywo. Następny krok: otworzyć każdy dzień z arkusza scenariuszy w GUI enova
  (`Kadry i płace / Kadry / Pracownicy / [NG-0X] / Kalendarz / Czas pracy`) i porównać wynik
  z kolumnami oczekiwanymi w arkuszu.

## Wariant: kalendarz równoważnego czasu pracy „4msc”

Drugi komplet scenariuszy sprawdza te same 4 cechy na kalendarzu **równoważnego czasu
pracy** „4msc” (`Kalendarze.ID=24`, `AlgorytmDobowa=RównoważnyCzasPracy`) — norma dobowa
= `MAX(plan dnia, Etat.NormaDobowa=8:00)`, więc dzień zaplanowany na 10h/12h nie generuje
nadgodzin, jeśli przepracowano dokładnie tyle, ile zaplanowano. Ten kalendarz **już
istniał** w bazie Claude (używany przez demo-pracownika `0001`) — pliki poniżej go NIE
tworzą ani nie modyfikują.

| # | Plik | Zawartość |
|---|---|---|
| 1 | `Nadgodziny nowe rownowazny - test 01 pracownicy.xml` | 8 pracowników `RC-01`…`RC-08`, `Etat.Kalendarz` = „4msc” (guid `14511BBF-2E5D-4A8E-9481-36141737639E`). |
| 2 | `Nadgodziny nowe rownowazny - test 02 plan pracy.xml` | Plan dnia (8h/10h/12h/4h) — guidy indywidualnych kalendarzy odczytane z bazy PO imporcie pliku 1 (powstają automatycznie, patrz [[reference_enova_kalendarz_wzorcowy_vs_indywidualny]]). |
| 3 | `Nadgodziny nowe rownowazny - test 03 dane rzeczywiste.xml` | Rzeczywisty czas pracy — jednorazowy, nieidempotentny (jak plik 4 powyżej). |

Scenariusze (arkusz „Rownowazny 4msc” w xlsx): `RC-01`/`RC-02` (plan 8h, baseline i +2h),
`RC-03`/`RC-04` (plan 10h, brak nadgodzin vs +2h ponad wydłużony dzień), `RC-05`/`RC-06`
(plan 12h, analogicznie), `RC-07`/`RC-08` (plan 4h — dzień kompensacyjny, „podłoga” normy
etatu 8h wygrywa nad krótkim planem → godziny do 8h to okresowe, nie 50%).

Zob. [[baza-claude-dodatek-roczny]], [[project-nadgodziny-50-100-nowe]],
[[reference-import-dzienplanu-xml]], [[reference-import-pracownika-xml]],
[[project_nadgodziny_nowe_scenariusze_testowe]].
