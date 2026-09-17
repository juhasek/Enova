# Generator cyklicznego importu planu pracy (dnia planu)

## Co to jest

Narzędzie Excel (`Generator planu pracy.xlsx`) + makro VBA
(`modGeneratorPlanuPracy.bas`) do cyklicznego generowania plików XML importu
„dnia planu” do enova365 — czyli zakładki **Kalendarz / Norma czasu pracy**
w kartotece pracownika (jego rzeczywisty grafik), tabela `DniKalendarza`,
wiersze typu `Soneta.Kalend.DzienPlanu`.

Zamiast ręcznie pisać XML dla każdego pracownika i okresu, osoba
wypełniająca arkusz „Plan” wpisuje: kod pracownika, zakres dat, które dni
tygodnia mają być dniami pracy i godziny 1–4 stref pracy. Makro:

1. łączy się z bazą SQL enova (Windows Auth) i dla każdego kodu pracownika
   odnajduje GUID jego **indywidualnego kalendarza** (`Kalendarze.Typ=2`,
   FK `Pracownik`) — ten kalendarz musi już istnieć (powstaje automatycznie
   przy zapisie `Etat` pracownika w enova, patrz niżej);
2. rozwija zakresy dat na konkretne dni wg zaznaczonych dni tygodnia;
3. dla każdego dnia liczy `<Praca>` na poziomie dnia z zadanych stref
   (`OdGodziny` = najwcześniejsza strefa, `Czas` = suma czasów stref —
   import wg rekordów nie przelicza tego sam);
4. zapisuje **jeden plik XML na pracownika** (z `fromto` = zakres
   najwcześniejszej..najpóźniejszej wygenerowanej daty dla tego
   pracownika), gotowy do `dbmgr importxml <baza> "<plik>"` (tryb standard).

Pełna instrukcja krok po kroku (import makra, uzupełnianie arkuszy,
uruchomienie) jest w arkuszu **„Instrukcja”** w samym skoroszycie.

## Mechanizm importu (dla kogo edytuje/rozwija to narzędzie)

Patrz `[[reference-import-dzienplanu-xml]]` (pamięć projektu) i przykład
`ImportyXML/Plan pracy przerywany 7-11 13-17 - TS-01 pazdziernik 2026.xml`.
Kluczowe punkty:

- `DniKalendarza` **nie jest guidowana** → dni importuje się zagnieżdżone
  w kolekcji `<Dni>` kalendarza pracownika (`KalendarzBase.Dni`), a atrybut
  `fromto` na `<session>` ogranicza kasowanie/zastępowanie do zadanego
  okresu — stąd jeden plik na pracownika, żeby `fromto` miało jednoznaczny
  sens.
- Dzień przerywany (np. 8-12 i 13-17) to **dwie strefy** „Praca w normie” —
  przerwa (tu 12:00-13:00) to po prostu luka między strefami, nie osobna
  strefa.
- GUID-y `00000000-0006-0002-0001-...` (definicja dnia „Pracy”) i
  `00000000-0006-0001-0001-...` (definicja strefy „Praca w normie”) to
  **systemowe stałe enova365**, jednakowe w każdej instalacji — nie trzeba
  ich wyszukiwać per baza klienta.
- Import jest **wg rekordów** (`business="false"`) — jedyny tryb, który
  działa z CLI `dbmgr`. Nie uruchamia logiki kreatora/workerów, dlatego
  pola `<Praca>` dnia trzeba wypełniać jawnie (robi to za nas makro).
- Komentarze w wygenerowanym XML nie mogą zawierać `--` (błąd
  `XmlException` przy imporcie) — makro tego pilnuje, generując proste
  komentarze bez podwójnych myślników.

## Wymaganie wstępne: pracownik musi już istnieć w enova

Makro **nie tworzy pracowników** — szuka tylko GUID-u kalendarza już
istniejącego pracownika po jego `Kod`. Nowego pracownika trzeba najpierw
założyć w enova (GUI albo osobny import XML kartoteki, patrz
`[[reference-import-pracownika-xml]]`) — dopiero wtedy jego indywidualny
kalendarz istnieje w `Kalendarze` i makro go znajdzie.

## Status weryfikacji

Zweryfikowane na żywo w bazie testowej `Claude` (`localhost\SQLEXPRESS`)
2026-09-17: nowy pracownik testowy **PP-01** (Nowicka Marta, zatrudniona
od 2026-09-01) + import 22 dni planu przerywanego 8:00-12:00 / 13:00-17:00
na wszystkie dni robocze września 2026 — `DniKalendarza` = 22 wiersze,
`StrefyKalandarza` = 44 wiersze (po 2 na dzień), godziny w bazie 480 minut
(8:00) start i czas, zgodnie z oczekiwaniem. Pliki referencyjne:
`Plan pracy przerywany 8-12 13-17 - PP-01 pracownik.xml` i
`Plan pracy przerywany 8-12 13-17 - PP-01 wrzesien 2026.xml` w tym samym
folderze `ImportyXML/`.

Sam **generator Excel+VBA** (ten folder) nie był jeszcze uruchomiony na
żywo w Excelu (środowisko robocze repo nie ma zainstalowanego Excela) —
kod makra odzwierciedla dokładnie logikę ręcznie zweryfikowanego importu
PP-01 powyżej, ale wymaga przetestowania w prawdziwym Excelu przed
użyciem produkcyjnym: zaimportować `.bas`, uzupełnić „Konfiguracja”,
wygenerować plik dla przykładowego wiersza PP-01 i porównać z plikiem
referencyjnym `Plan pracy przerywany 8-12 13-17 - PP-01 wrzesien 2026.xml`.
