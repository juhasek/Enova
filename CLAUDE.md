# Konwencje pracy w repozytorium

## Branch roboczy

Domyślnym branchem, na który powinny trafiać zmiany w tym repozytorium, jest
`Testy` — nie `Produkcja`. Zmiany należy commitować i pushować bezpośrednio na
`Testy`, bez tworzenia dodatkowych branchy `claude/...` na potrzeby
pojedynczych zadań, chyba że użytkownik wyraźnie poprosi inaczej.

Branch `Produkcja` traktowany jest jako stabilny/produkcyjny. Przenoszenie zmian
z `Testy` do `Produkcja` odbywa się świadomie (np. przez Pull Request) i wymaga
wyraźnej zgody użytkownika za każdym razem.

### Podział Produkcja / Testy

- **`Produkcja`** zawiera wszystkie rozwiązania zaakceptowane – przeszły testy
  i użytkownik nie ma do nich zastrzeżeń.
- **`Testy`** zawiera tylko rozwiązania w trakcie prac/testów oraz pliki
  wspólne, które zostają na obu branchach: `CLAUDE.md`, `README.md`,
  `.claude/`, `.gitignore`, `.mcp.json`, `Addon/.gitignore`, `Pobrane/README.md`.
  Brak rozwiązania w `Testy` nie oznacza, że nie istnieje – szukać go na `Produkcja`
  (`git ls-tree -r --name-only origin/Produkcja`).
- Branche **nie mają wspólnej historii i nie scala się ich w całości**
  (merge `Testy` → `Produkcja` usunąłby z `Produkcja` zaakceptowane rozwiązania, a merge
  `Produkcja` → `Testy` przywróciłby je do `Testy`). Rozwiązania przenosi się
  pojedynczo.
- **Akceptacja rozwiązania** (tylko na wyraźne polecenie użytkownika):
  1. branch z `Produkcja`, np. `<temat>-do-Produkcja`;
  2. skopiowanie plików rozwiązania z `Testy`: `git checkout origin/Testy -- <ścieżki>`
     (cały folder rozwiązania wraz z dokumentacją `.md`, scenariuszami i opisami sesji);
  3. Pull Request do `Produkcja` i jego scalenie;
  4. po scaleniu usunięcie tych plików z `Testy` (`git rm`) – commit + push na `Testy`.
- **Poprawka zaakceptowanego rozwiązania:** przywrócić je do `Testy` z `Produkcja`
  (`git checkout origin/Produkcja -- <ścieżki>`), poprawić i przetestować na `Testy`,
  a po akceptacji ponownie przenieść jak wyżej.
- Rozwiązanie, którego choć jeden plik jest w testach, trzymać w `Testy` w całości
  (np. cały projekt `Addon/<nazwa>/`), żeby dało się je zbudować i testować.
- Opis struktury repo poniżej dotyczy całości (`Produkcja` + `Testy`); na `Testy`
  część folderów może być pusta/nieobecna.

## Struktura repo

Repozytorium przechowuje customizacje do systemu ERP **enova365** (Soneta).
Każdy folder odpowiada jednemu typowi artefaktu enova365:

- **Raporty/** — niestandardowe raporty (np. raport z odbić RCP).
- **Tuple/** — definicje krotek (kolumn/pól wyliczanych) używanych w widokach
  i raportach enova365.
- **Weryfikatory czasu pracy/** — weryfikatory reguł kadrowo-płacowych
  specyficznych dla modułu czasu pracy (np. normy okresu rozliczeniowego,
  okresu zatrudnienia). Zostaje wydzielony jako osobny folder ze względu na
  specyfikę domeny kadrowo-płacowej.
- **Weryfikatory/** — pozostałe weryfikatory pól/obiektów enova365,
  niezwiązane z czasem pracy (np. walidacje danych kadrowych ogólnych,
  handlowych itp.).
- **ElementyPlac/** — definicje elementów płacowych (składniki wynagrodzenia,
  reguły naliczania) enova365.
- **Rezerwy urlopowe/** — rezerwy urlopowe i budżety rezerw (moduł planowanych
  list płac enova), każde wdrożenie w osobnym podkatalogu: kod algorytmów
  elementów, cechy widoku, XML importu (generowany skryptem), scenariusze testowe.
- **Cechy/** — definicje cech (atrybutów) obiektów enova365.
- **Widoki/** — definicje/modyfikacje widoków (list, formularzy) enova365.
- **Taski/** — kod definicji zadań enova365 (Taski z algorytmem, tabela
  `TaskDefs`) i ich wyzwalaczy (`TaskTriggers`), np. automatyczne akcje
  przy zapisie danych. Nie mylić z folderem `Zadania/`.
- **Zadania/** — opisy/specyfikacje zadań wdrożeniowych, niekoniecznie
  powiązane 1:1 z pojedynczym artefaktem powyżej.
- **Analiza/** — szablony do analizy przedwdrożeniowej (kwestionariusz pytań
  na warsztat z klientem w wersji `.md` i `.xlsx`). Szablony, nie ustalenia
  konkretnego wdrożenia — notatki z konkretnego klienta trafiają do `Zadania/`.
- **ImportyXML/** — pliki XML importu danych/konfiguracji do enova365 wg
  schematu `<session xmlns="http://www.soneta.pl/schema/business">`
  (import wg rekordów lub przez logikę biznesową), np. `*.dbinit.xml`
  definiujące rekordy zamiast ręcznego wpisywania w GUI. Pola oznaczone
  w pliku jako TODO/UNVERIFIED wymagają potwierdzenia próbnym importem
  (`dbmgr importxml`) na bazie testowej przed użyciem produkcyjnym —
  środowisko robocze tego repo nie ma dostępu do DLL/live-testu, więc
  takie pliki nie są tworzone jako w pełni zweryfikowane.
- **Integracja/** — analizy, specyfikacje i materiały testowe integracji enova365
  z systemami zewnętrznymi (WebAPI, synchronizacje), np. kolekcje Postman i scenariusze
  testowe. Każda integracja w osobnym podkatalogu; wszystko dotyczące danej integracji
  trafia tam (nie do `Zadania/`). Uzupełnionych środowisk Postman (hasła) nie commitować.
- **Wzor_analizy/** — wzór dokumentu analizy przedwdrożeniowej KPCP (Kadry, Płace,
  Czas pracy): dokument główny i załączniki 1, 2, 3, 5 (`Wzor_*.docx`), kwestionariusz
  spotkań, `STYLE_GUIDE.md`, `raport_analizy_dokumentow.md`. Pliki `.docx` są
  **generowane** z `Wzor_analizy/Tresc/*.txt` generatorem `Wzor_analizy/Generator/` –
  zmiany wzoru wprowadza się w plikach treści, nie w `.docx`. `Wzor_analizy/_robocze/`
  (kopie dokumentów źródłowych, lista zakazanych słów) jest poza gitem.
- **Klienci/** — analizy konkretnych klientów utworzone ze wzoru (`Klienci/<Nazwa>/`),
  notatki ze spotkań w `Klienci/<Nazwa>/Notatki/`. Cały katalog jest **poza gitem**
  (`.gitignore`) – zawiera nazwy i dane klientów; nie commitować (np. przez `git add -f`).
- **Pobrane/** — pliki wejściowe przekazywane przez użytkownika do analizy
  (eksporty z enova365, wydruki, zrzuty ekranu, DLL-e klientów, logi).
  Katalog roboczy, a nie artefakt enova365 — wynik analizy trafia do
  właściwego folderu tematycznego powyżej. Gdy użytkownik mówi o pliku
  „do analizy” bez podania ścieżki, najpierw szukać go tutaj.
  Zawartość jest wykluczona z gita (`.gitignore`, poza `README.md`) —
  **nie commitować** plików z tego katalogu (np. przez `git add -f`), bo
  mogą zawierać dane osobowe/płacowe. Wnioski i wynikowe artefakty zapisywać
  w folderach tematycznych, bez kopiowania surowych danych.

### Ważne: format plików

Pliki bez rozszerzenia w powyższych folderach to **fragmenty kodu C#
wklejane bezpośrednio do wbudowanego edytora skryptów enova365** (np. w
oknie definicji weryfikatora, tupli, elementu płacowego itp.) — **nie** są
to skompilowane projekty .NET ani DLL-e, i nie mają samodzielnego pliku
`.csproj`/`.sln`. Nie należy traktować ich jak standardowego projektu C#
(np. nie próbować ich budować przez `dotnet build`), chyba że w repo pojawi
się osobny folder **Addon/** zawierający właściwy projekt `.csproj` — to
byłby sygnał, że mowa o skompilowanym dodatku enova365, a nie o wklejanym
skrypcie. Wyjątkiem jest **Integracja/*/Generator/** (konsolówka budująca kolekcję
Postman i arkusz scenariuszy), **Analiza/Generator/**, **Wzor_analizy/Generator/** oraz
**Zadania/Generator słownika rozwiązania umowy/** — to zwykłe narzędzia
konsolowe budujące arkusz (z dokumentu `.md` albo z DLL serwera i bazy), uruchamiane
poza enovą (korzystają tylko z bibliotek DevExpress z katalogu serwera), a nie dodatki do wgrania.

Plik `.md` obok pliku bez rozszerzenia (o tej samej nazwie) to dokumentacja
biznesowa danego artefaktu.

## Opis sesji – zapisywany automatycznie

Na koniec każdej sesji z istotnymi ustaleniami (zmiany w kodzie/bazie, decyzje,
wyniki testów, otwarte punkty) zapisz opis sesji **bez czekania na prośbę**.
Surowe transkrypty Claude Code są kasowane po 30 dniach, więc do ustaleń
wracamy przez te pliki.

- Plik: `Zadania/<temat> - sesja RRRR-MM-DD.md`. Jeśli temat ma własny folder
  (np. `Addon/<projekt>/`, `Integracja/<nazwa>/`), plik trafia tam jako
  `sesja-RRRR-MM-DD.md`.
- Gdy tego dnia był już opis sesji na ten sam temat, dopisz do niego nową sekcję
  zamiast tworzyć kolejny plik.
- Treść: cel, co zrobiono (pliki w repo, zmiany w bazie), ustalenia i decyzje,
  wyniki testów, otwarte punkty / następne kroki.
- Bez nazw klientów i danych osobowych/płacowych (zgodnie z zasadami dla
  `Klienci/` i `Pobrane/`).
- Po zapisaniu: commit + push na `Testy`. Krótkie wpisy w pamięci projektu
  aktualizuj jak dotąd, a opis sesji traktuj jako pełniejszy zapis.
- Sesje czysto informacyjne (pojedyncze pytanie, odczyt danych bez ustaleń)
  pomijaj.

## Analiza przedwdrożeniowa – „uzupełnij analizę”

Gdy użytkownik po spotkaniu z klientem przekazuje notatki i pisze „uzupełnij analizę”
(lub podobnie), postępuj według poniższych zasad.

**Położenie materiałów** (`C:\enovaServer\Projekty\Enova\Wzor_analizy`):
- wzory: `Wzor_Analiza_przedwdrozeniowa_KPCP.docx` (dokument główny),
  `Wzor_ZAL_01_Kadry_KPCP.docx`, `Wzor_ZAL_02_Czas_pracy_KPCP.docx`,
  `Wzor_ZAL_03_Place_KPCP.docx`, `Wzor_ZAL_05_Pulpity_KPCP.docx`;
- reguły formatowania i języka: `STYLE_GUIDE.md` – **obowiązkowo przeczytać przed edycją**;
- kwestionariusz: `Kwestionariusz_spotkan_KPCP.docx` (kolumna „Sekcja w analizie” wskazuje
  docelową sekcję: AN – dokument główny, Z1/Z2/Z3/Z5 – załączniki).

**Nowy klient:**
1. Utwórz `Klienci\<Nazwa>\` (nazwa bez spacji i polskich znaków) z podkatalogami `Notatki\`
   i `Archiwum\`. Gdy nazwa nie jest jasna – zapytaj.
2. Skopiuj wzory jako `Klienci\<Nazwa>\Analiza_<Nazwa>_v0.1.docx` oraz
   `Analiza_<Nazwa>_ZAL_01_Kadry_v0.1.docx`, `..._ZAL_02_Czas_pracy_...`, `..._ZAL_03_Place_...`,
   `..._ZAL_05_Pulpity_...`, a kwestionariusz jako `Notatki\Kwestionariusz_<Nazwa>.docx`.
3. Uzupełnij od razu nazwę klienta (strona tytułowa, klauzula, akceptacja), metrykę
   (data, wersja 0.1, autor, status „Roboczy”) i pierwszy wiersz historii zmian.

**Uzupełnianie po spotkaniu:**
1. Notatki zapisz w `Klienci\<Nazwa>\Notatki\` (np. `RRRR-MM-DD_<temat>.md`), bez modyfikowania
   ich treści merytorycznej.
2. Każdą informację z notatek przypisz do sekcji: najpierw przez kwestionariusz (kolumna
   „Sekcja w analizie”), a gdy pytania tam nie ma – przez spis treści właściwego dokumentu.
   Zastąp odpowiadające pole `[DO UZUPEŁNIENIA: …]` treścią zapisaną formułami z
   `STYLE_GUIDE.md` (rozdz. 8: „Na etapie analizy ustalono, że…”, „(mechanizm N)” itd.).
   W kwestionariuszu wpisz odpowiedź i status „Wyjaśnione”.
3. Sekcję opcjonalną usuń w całości (z nagłówkiem), jeśli notatki jednoznacznie wskazują,
   że nie dotyczy klienta; w przeciwnym razie usuń tylko znacznik `[SEKCJA OPCJONALNA – …]`
   po jej uzupełnieniu.
4. **Niczego nie zgaduj.** Informacji brakujących, niejednoznacznych lub sprzecznych nie
   wpisuj do treści – zostaw pole `[DO UZUPEŁNIENIA]` i dodaj wiersz do tabeli „Pytania
   otwarte” (dokument główny, rozdz. Pytania otwarte i ryzyka, lub „Pytania otwarte – <obszar>”
   w załączniku) z numerem sekcji i statusem „Otwarte”.
5. Formatuj wyłącznie istniejącymi stylami wzoru (Normalny, Nagłówek 1–3, Lista punktowana,
   Tekst tabeli, Legenda, Proposal Table, Pole do uzupełnienia) – bez ręcznych zmian czcionki,
   koloru, odstępów i numeracji. Tabele dodawaj w układach z `STYLE_GUIDE.md` (rozdz. 7).
6. Edycja `.docx` klienta: OpenXML (System.IO.Compression + XLinq) lub DevExpress RichEdit
   z katalogu serwera (`C:\enovaServer\2610\Soneta.Products.Server.Standard`, .NET 10 –
   `~/.dotnet10`); narzędzia pomocnicze w scratchpadzie, nie w katalogu klienta. Python,
   Word i LibreOffice nie są dostępne na tym komputerze.

**Wersjonowanie przy każdej zmianie:**
- poprzednią wersję przenieś do `Klienci\<Nazwa>\Archiwum\` (nie nadpisuj), nową zapisz
  z podniesionym numerem: robocze `0.2, 0.3…`, przekazana do akceptacji `1.0`, poprawki
  po uwagach Klienta `1.1, 1.2…`;
- dodaj wiersz w „Historii zmian dokumentu” (wersja, data, autor, co uzupełniono lub usunięto),
  zaktualizuj metrykę (wersja, ostatnia modyfikacja, status) i oznacz pola do aktualizacji
  (spis treści, wykazy tabel i rysunków odświeżą się przy otwarciu w Wordzie);
- przy zmianie wersji załącznika zaktualizuj „Wykaz załączników” w dokumencie głównym.

**Zakazy:**
- oryginałów w `Pobrane\` nie wolno modyfikować, przenosić ani usuwać – pracuj na kopiach;
- `Klienci\` nie trafia do gita; do wzorów (`Wzor_analizy\`) nie przenoś danych klienta
  (nazw, nazwisk, NIP, adresów, kwot);
- w czacie nie używaj nazw klientów (patrz pamięć projektu).

**Na koniec każdej sesji uzupełniania** podaj listę pól, które nadal pozostają do uzupełnienia:
dla każdego dokumentu klienta liczbę pól `[DO UZUPEŁNIENIA` i ich listę z numerem sekcji
(treść pola), pozostałe znaczniki sekcji opcjonalnych oraz nowe pozycje dopisane do „Pytań
otwartych”.

## Obsługa programu enova365 (MCP)

Do obsługi interfejsu programu używaj skilla `soneta-mcp-ui` (`.claude/skills/soneta-mcp-ui`).
Serwery MCP (`.mcp.json`, `buscall.exe mcp`) — każdy połączony z jedną bazą:

| Serwer MCP | Baza |
|---|---|
| `soneta_ui_al` | Al |
| `soneta_ui_claude` | Claude |

Gdy operator nie wskaże bazy, zapytaj, na której pracować — nie zakładaj domyślnej.
