# Generator cyklicznego importu planu pracy (dnia planu)

## Co to jest

Narzędzie do cyklicznego generowania plików XML importu „dnia planu” do
enova365 — czyli zakładki **Kalendarz / Norma czasu pracy** w kartotece
pracownika (jego rzeczywisty grafik), tabela `DniKalendarza`, wiersze typu
`Soneta.Kalend.DzienPlanu`. Składa się z dwóch niezależnych kroków:

1. **Excel + makro VBA** (`Generator planu pracy.xlsx` +
   `modGeneratorPlanuPracy.bas`) — osoba wypełniająca arkusz „Plan” wpisuje
   kod pracownika, zakres dat, dni tygodnia i godziny 1–4 stref pracy.
   Makro rozwija to na konkretne dni i zapisuje **jeden plik XML na
   pracownika**. **Makro NIE łączy się z bazą SQL** — element
   `<KalendarzBase>` dostaje placeholder `guid="KOD:<Kod pracownika>"`
   zamiast prawdziwego GUID-u.
2. **Skrypt PowerShell** (`Importuj-PlanyPracy.ps1`), uruchamiany **poza
   Excelem** przez osobę wykonującą import — dla każdego placeholdera
   `KOD:xxx` odpytuje bazę SQL (Windows Auth) o GUID indywidualnego
   kalendarza tego pracownika (`Kalendarze.Typ=2`, **bez znaczenia, jaki
   konkretnie kalendarz ma pracownik** — liczy się tylko, że ma już
   założony Etat), podmienia placeholder i (z `-Importuj`) od razu woła
   `dbmgr importxml`.

**Dlaczego dwa kroki:** wymóg klienta — makro w Excelu (VBA) nie może
nawiązywać połączeń z SQL. Rozdzielenie generowania treści (Excel, offline)
od rozwiązania adresu kalendarza i importu (PowerShell, poza Excelem,
uruchamiane przez tę samą osobę, która i tak wykonuje `dbmgr importxml`)
spełnia ten wymóg w 100% — macro nigdy nie dotyka bazy.

Pełna instrukcja krok po kroku jest w arkuszu **„Instrukcja”** w samym
skoroszycie.

## Dlaczego nie da się zaadresować kalendarza inaczej niż przez GUID

Sprawdzone empirycznie (próbne importy na bazie `Claude`) i potwierdzone
strukturą tabeli (`scan-props` na `Soneta.Kalend.KalendarzBase`, tabela
`Kalendarze`): ten obiekt **nie ma żadnego pola zarejestrowanego jako klucz
użyteczny w atrybucie `where`/`key`** poza `Guid` — próby `where="Nazwa=..."`,
`where="Typ=... and Nazwa=..."`, `where="Pracownik=..."` kończą się błędem
`Klucz dla pola/pól 'X' nieznaleziony w tabeli 'Kalendarze'`. Właściwość
`Pracownik.DniPlanu` (bezpośrednio na obiekcie `Pracownik`) też nie działa w
imporcie wg rekordów (`CollectionConverter cannot convert from (null)` —
to właściwość obliczana, nie prosta kolekcja ORM). Zagnieżdżenie
`Pracownik → Kalendarze → KalendarzBase` bez GUID-u też się nie udaje:
domyślne zastępowanie kolekcji próbuje skasować kalendarz pracownika, co
jest zablokowane regułą biznesową („Nie można skasować kalendarza
pracownika”). Stąd GUID pozostaje jedynym działającym sposobem adresowania
— dlatego go rozwiązujemy, tylko robimy to poza Excelem.

(Istnieje osobny, cięższy mechanizm `DokumentAktualizacjiKalendarza` —
dokument z obiegiem zatwierdzania, adresowalny przez pracownika bez GUID-u
— ale to inny, dużo bardziej złożony obiekt biznesowy [workflow z
zatwierdzaniem], używany w tym repo do zupełnie innego celu, patrz
`Widoki/Aktualizacja planu pracy SKA.md`; nie nadaje się jako prosty
zamiennik zwykłego importu wg rekordów.)

## Mechanizm importu dnia planu (dla kogo rozwija to narzędzie)

Patrz `[[reference-import-dzienplanu-xml]]` (pamięć projektu) i przykład
`ImportyXML/Plan pracy przerywany 7-11 13-17 - TS-01 pazdziernik 2026.xml`.
Kluczowe punkty:

- `DniKalendarza` **nie jest guidowana** → dni importuje się zagnieżdżone
  w kolekcji `<Dni>` kalendarza pracownika, a atrybut `fromto` na
  `<session>` ogranicza kasowanie/zastępowanie do zadanego okresu.
- Dzień przerywany (np. 8-12 i 13-17) to **dwie strefy** „Praca w normie” —
  przerwa (12:00-13:00) to luka między strefami, nie osobna strefa.
- GUID-y `00000000-0006-0002-0001-...` (definicja dnia „Pracy”) i
  `00000000-0006-0001-0001-...` (definicja strefy „Praca w normie”) to
  **systemowe stałe enova365**, jednakowe w każdej instalacji.
- Import jest **wg rekordów** — pola `<Praca>` dnia trzeba wypełniać
  jawnie (robi to makro: `OdGodziny` = najwcześniejsza strefa, `Czas` =
  suma czasów stref).
- Komentarze w XML nie mogą zawierać `--` (błąd `XmlException`).

## Wymaganie wstępne: pracownik musi już istnieć w enova

Ani makro, ani skrypt PowerShell nie tworzą pracowników — skrypt szuka
tylko GUID-u kalendarza już istniejącego pracownika po jego `Kod`. Nowego
pracownika trzeba najpierw założyć w enova (GUI albo osobny import XML
kartoteki, patrz `[[reference-import-pracownika-xml]]`) — dopiero wtedy
jego indywidualny kalendarz istnieje i skrypt go znajdzie. Pracownik bez
kalendarza powoduje pominięcie **całego jego pliku** z ostrzeżeniem — nie
przerywa importu pozostałych plików.

## Status weryfikacji (2026-09-17, na żywo w bazie testowej `Claude`)

1. Pracownik testowy **PP-01** (Nowicka Marta, od 2026-09-01) + 22 dni
   planu przerywanego 8:00-12:00 / 13:00-17:00 na wrzesień 2026,
   zaimportowane **bezpośrednio** (GUID znany z SQL) — `DniKalendarza`=22,
   `StrefyKalandarza`=44. Pliki: `Plan pracy przerywany 8-12 13-17 - PP-01
   pracownik.xml` i `... PP-01 wrzesien 2026.xml`.
2. **Pełny pipeline z placeholderem** przetestowany end-to-end: plik XML z
   `guid="KOD:PP-01"` dla dnia 2026-10-01 → `Importuj-PlanyPracy.ps1
   -SqlServer localhost\SQLEXPRESS -Baza Claude -Importuj` → skrypt
   rozwiązał placeholder na `3c2929f3-e527-4e10-8188-b647b9183323`
   (kalendarz PP-01), wywołał `dbmgr importxml` bezbłędnie. Zweryfikowane
   SQL-em: `DniKalendarza` dla 2026-10-01 ma `PracaOdGodziny`=480,
   `PracaCzas`=480 (8:00), 2 wiersze `StrefyKalandarza`.
3. Próby adresowania `KalendarzBase` bez GUID-u (`where`/`key` na
   `Nazwa`/`Typ`/`Pracownik`, właściwość `Pracownik.DniPlanu`, zagnieżdżenie
   `Pracownik→Kalendarze` bez `addnew`/z `addnew`) — **wszystkie
   nieudane**, błędy udokumentowane wyżej. To ustalenie, nie luka do
   dopracowania — GUID + rozwiązanie poza Excelem to ostateczny mechanizm.

**Niezweryfikowane:** samo makro VBA (`GenerujPlanyPracy`) nie było
uruchomione w prawdziwym Excelu (brak Excela w tym środowisku) — logika
generowania placeholdera odzwierciedla dokładnie to, co ręcznie
przetestowano w kroku 2 powyżej, ale wymaga przetestowania w Excelu przed
użyciem produkcyjnym: zaimportować `.bas`, wygenerować plik dla przykładowego
wiersza PP-01, porównać wynik z plikiem testowym opisanym w punkcie 2.
