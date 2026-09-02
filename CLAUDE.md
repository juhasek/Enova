# Konwencje pracy w repozytorium

## Branch roboczy

Domyślnym branchem, na który powinny trafiać zmiany w tym repozytorium, jest
`Testy` — nie `main`. Zmiany należy commitować i pushować bezpośrednio na
`Testy`, bez tworzenia dodatkowych branchy `claude/...` na potrzeby
pojedynczych zadań, chyba że użytkownik wyraźnie poprosi inaczej.

Branch `main` traktowany jest jako stabilny/produkcyjny. Przenoszenie zmian
z `Testy` do `main` odbywa się świadomie (np. przez Pull Request) i wymaga
wyraźnej zgody użytkownika za każdym razem.

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
- **Cechy/** — definicje cech (atrybutów) obiektów enova365.
- **Widoki/** — definicje/modyfikacje widoków (list, formularzy) enova365.
- **Zadania/** — opisy/specyfikacje zadań wdrożeniowych, niekoniecznie
  powiązane 1:1 z pojedynczym artefaktem powyżej.
- **ImportyXML/** — pliki XML importu danych/konfiguracji do enova365 wg
  schematu `<session xmlns="http://www.soneta.pl/schema/business">`
  (import wg rekordów lub przez logikę biznesową), np. `*.dbinit.xml`
  definiujące rekordy zamiast ręcznego wpisywania w GUI. Pola oznaczone
  w pliku jako TODO/UNVERIFIED wymagają potwierdzenia próbnym importem
  (`dbmgr importxml`) na bazie testowej przed użyciem produkcyjnym —
  środowisko robocze tego repo nie ma dostępu do DLL/live-testu, więc
  takie pliki nie są tworzone jako w pełni zweryfikowane.

### Ważne: format plików

Pliki bez rozszerzenia w powyższych folderach to **fragmenty kodu C#
wklejane bezpośrednio do wbudowanego edytora skryptów enova365** (np. w
oknie definicji weryfikatora, tupli, elementu płacowego itp.) — **nie** są
to skompilowane projekty .NET ani DLL-e, i nie mają samodzielnego pliku
`.csproj`/`.sln`. Nie należy traktować ich jak standardowego projektu C#
(np. nie próbować ich budować przez `dotnet build`), chyba że w repo pojawi
się osobny folder **Addon/** zawierający właściwy projekt `.csproj` — to
byłby sygnał, że mowa o skompilowanym dodatku enova365, a nie o wklejanym
skrypcie.

Plik `.md` obok pliku bez rozszerzenia (o tej samej nazwie) to dokumentacja
biznesowa danego artefaktu.
