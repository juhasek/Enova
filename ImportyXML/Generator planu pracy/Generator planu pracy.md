# Generator cyklicznego importu planu pracy (dnia planu)

## Co to jest

Narzędzie do cyklicznego generowania plików XML importu „dnia planu” do
enova365 — czyli zakładki **Kalendarz / Norma czasu pracy** w kartotece
pracownika (jego rzeczywisty grafik). Składa się z:

- `Generator planu pracy.xlsx` — arkusze Instrukcja / Konfiguracja / Plan,
- `modGeneratorPlanuPracy.bas` — makro VBA `GenerujPlanyPracy`.

Osoba wypełniająca arkusz „Plan” wpisuje: **kod pracownika**, **jedną
konkretną datę** i godziny 1–4 stref pracy tego dnia — **jeden wiersz =
jeden dzień dla jednego pracownika** (bez zakresu dat, bez zaznaczania
dni tygodnia — każdy dzień roboczy to osobny wiersz). Makro generuje
**jeden plik XML** w oficjalnym formacie enova (`Root/DniPlanu/
DzienPlanu/Strefy`), który importuje się w programie enova poleceniem
menu **Plik → Importuj zapisy → Import czasu pracy i wynagrodzeń**.

**Makro nie łączy się z bazą SQL w żaden sposób i nie używa żadnego GUID-u.**
Pracownik jest identyfikowany wyłącznie po kodzie (`<Pracownik>`) — importer
enova sam go wyszukuje po stronie serwera. To wymóg klienta (zakaz połączeń
SQL z poziomu makra Excela) i jednocześnie właściwy, natywny sposób importu
tego typu danych w enova365 — nie obejście, tylko udokumentowany mechanizm
platformy.

## Skąd wzięliśmy ten format — historia ustaleń

Wcześniejsze podejście (ten sam plik, wcześniejsze wersje tego dokumentu)
próbowało budować import przez ogólny mechanizm `dbmgr importxml`
(`<session xmlns="...soneta.pl/schema/business">`). Ustalono wtedy
empirycznie, że tabela `Kalendarze` (`KalendarzBase`) **nie ma żadnego pola
użytecznego w `where`/`key` poza wewnętrznym GUID-em** — więc ten mechanizm
*wymagał* GUID-u kalendarza, którego nie da się pozyskać bez SQL.

Użytkownik dostarczył wzorcowy plik z **oficjalnej dokumentacji/pomocy
enova** w formacie:

```xml
<Root xmlns:xsd="..." xmlns:xsi="...">
  <DniPlanu>
    <DzienPlanu>
      <Pracownik>006</Pracownik>
      <Data>02.01.2012</Data>
      <Definicja>Pracy</Definicja>
      <OdGodziny>9:00</OdGodziny>
      <Czas>8:00</Czas>
    </DzienPlanu>
  </DniPlanu>
</Root>
```

Próba wczytania tego pliku przez `dbmgr importxml` **dała błędne wyniki bez
zgłoszenia błędu** — trzy testy z różnymi kodami pracownika (`PP-01`,
`0001`, `TS-01`) za każdym razem zapisały dzień planu w kalendarzu
**kolejnego, niepowiązanego pracownika** (sekwencyjnie: NG-01, NG-02,
NG-03), z błędnymi godzinami. Wniosek: **ten format nie jest przeznaczony
dla `dbmgr importxml`** — `<Pracownik>` był całkowicie ignorowany. Błędne
wpisy testowe zostały natychmiast usunięte z bazy (SQL DELETE, zweryfikowane).

Użytkownik dostarczył kluczowy brakujący element: pliki
`Soneta.CzasPracy.Migrator.dll` + `Soneta.CzasPracy.Utils.dll`
(z paczki „Migracja_Soneta.CzasPracy od wersji 2406”) — **dodatek enova
zarejestrowany jako rozszerzenie bazy** (`dbmgr extlist Claude` potwierdza:
`soneta.czaspracy.migrator.dll` i `soneta.czaspracy.utils.dll`, „Use in
server”=True) — oraz oryginalny wzorcowy arkusz Soneta
`xml- Norma pracy.xlsm`, w którym wprost napisano:

> „Plik wczytujemy z pozycji Plik | Importuj zapisy | Import czasu pracy
> i wynagrodzeń (konieczna dllka czas pracy)”
> „Uruchamiamy makro Plan pracy, które generuje plik norma pracy.xml”

To wyjaśnia wszystko: ten format XML **nie jest czytany przez `dbmgr`**,
tylko przez dedykowaną pozycję menu w kliencie enova, zaimplementowaną
właśnie w dostarczonych DLL-ach.

## Potwierdzenie z kodu źródłowego (dekompilacja)

Zdekompilowano `Soneta.CzasPracy.Utils.dll` (`ilspycmd`) i przeanalizowano
klasę `Soneta.CzasPracy.Akordy.Document` (metoda `ImportPlanuPracy`) oraz
`Root` (definicje `[XmlElement]`/`[XmlAttribute]`). Ustalenia:

- Pracownik wyszukiwany **wyłącznie po `Kod`**:
  `kadry.Pracownicy.WgKodu[dzienPlanu.Pracownik]` — brak GUID-u, brak SQL
  (to zwykły klucz ORM, rozwiązywany przez sam silnik enova podczas
  importu w GUI, nie przez nasz kod).
- Dzień identyfikowany przez `pracownik.DniPlanu[data]` — jeśli istnieje,
  jest **aktualizowany** (strefy kasowane i wpisywane od nowa), jeśli nie
  — tworzony nowy. **Nie kasuje dni spoza podanych dat** (inaczej niż
  `dbmgr importxml` z `fromto`).
- `<Definicja>` dnia i `Definicja` strefy szukane **po nazwie**
  (`kalend.DefinicjeDni.WgNazwy` / `kalend.DefinicjeStref.WgNazwy`) —
  domyślnie „Pracy” / „Praca w normie” (standardowe nazwy systemowe).
- **`<Strefy>` z wieloma `<StrefaPracy>` jest w pełni obsługiwane** —
  każda strefa to jeden element z atrybutami `Definicja`, `OdGodziny`,
  `Czas`. Dokładnie to trzeba do dnia przerywanego (2 strefy: 8-12, 13-17).
- Kodowanie pliku: `Unicode` (UTF-16LE z BOM) w nagłówku XML — makro zapisuje
  tak przez `ADODB.Stream` z `Charset="Unicode"`.

Schemat klas (`Root.cs`): `Root.DniPlanu` (tablica `DzienPlanu`) →
`DzienPlanu : DzienPracy : Praca : PracownikHost` — pola `Pracownik`,
`Data`, `Definicja`, `OdGodziny`, `Czas`, `Strefy` (tablica `StrefaPracy`,
każda z atrybutami `Definicja`/`OdGodziny`/`Czas`).

## Wymaganie wstępne

- Pracownik musi już istnieć w enova (kartoteka założona, Kod zgodny) —
  import nie tworzy pracowników, zgłasza błąd „Pracownik o kodzie X nie
  został znaleziony” dla brakujących (reszta pliku importuje się dalej,
  błędy trafiają do logu „Import”).
- W docelowej bazie musi być zarejestrowane rozszerzenie
  `Soneta.CzasPracy.Migrator`/`Soneta.CzasPracy.Utils` — bez tego pozycja
  menu „Import czasu pracy i wynagrodzeń” jest niedostępna.

## Status weryfikacji

- **Mechanizm potwierdzony z kodu źródłowego** (dekompilacja, nie
  zgadywanie) — wysoka pewność co do poprawności schematu.
- **Rozszerzenie zarejestrowane w bazie `Claude`** — potwierdzone
  (`dbmgr extlist Claude`).
- **Import przez GUI enova NIE został jeszcze przetestowany na żywo**
  (to środowisko robocze nie ma dostępu do GUI/buscall) — plik testowy
  `Norma pracy - test PP-01 2026-11-05 dwie strefy.xml` (w tym samym
  folderze) czeka na test przez użytkownika: Plik → Importuj zapisy →
  Import czasu pracy i wynagrodzeń, na bazie `Claude`, pracownik PP-01,
  dzień 2026-11-05 z dwiema strefami 8-12/13-17.
- Samo makro VBA nie było uruchomione w prawdziwym Excelu (brak Excela w
  tym środowisku) — logika odzwierciedla dokładnie strukturę potwierdzoną
  dekompilacją, ale wymaga przetestowania w Excelu przed użyciem
  produkcyjnym.

**Do zrobienia po teście użytkownika:** potwierdzić w tym pliku wynik
próby importu (sukces/błąd, ewentualne poprawki formatu daty/nazw
definicji, jeśli w bazie klienta różnią się od „Pracy”/„Praca w normie”).

## 2026-09-17 — poprawka: domyślny folder docelowy

Pierwsza wersja makra miała w arkuszu „Konfiguracja” domyślną wartość
`C:\enovaServer\Projekty\Enova\ImportyXML\` — to ścieżka ze środowiska
deweloperskiego, w którym powstał ten generator, nie istniejąca na
komputerze użytkownika. Użytkownik uruchomił makro z tą wartością
niezmienioną i nie mógł znaleźć wygenerowanego pliku (prawdopodobnie
cichy błąd `MkDir`, który tworzy tylko jeden brakujący poziom folderu
naraz — dla wielopoziomowej nieistniejącej ścieżki zawodzi).

Poprawka:
- **Puste pole „Folder na plik XML” = zapis obok samego skoroszytu**
  (`ThisWorkbook.Path`) — zawsze istniejący, zawsze zapisywalny folder,
  nowa wartość domyślna w szablonie.
- Tworzenie folderu (`ZapewnijFolder`) obsługuje teraz **wszystkie
  brakujące poziomy** ścieżki, nie tylko jeden.
- Błędy tworzenia folderu i zapisu pliku pokazują teraz **czytelny
  komunikat z dokładną ścieżką i opisem błędu** zamiast cichego
  niepowodzenia lub nieobsłużonego wyjątku VBA.
- Dodana końcowa weryfikacja `Dir(nazwaPliku)` po zapisie — jeśli mimo
  braku zgłoszonego błędu pliku nie widać (częste przy OneDrive/
  antywirusie), makro o tym informuje zamiast milczeć.

## 2026-09-23 — uproszczenie arkusza „Plan”: jeden wiersz = jeden dzień

Użytkownik dostarczył własny wzorcowy plik Excela pokazujący oczekiwany
układ arkusza „Plan” — bez kolumn „Data do” i „Pn..Nd”, za to z jednym
wierszem na każdy pojedynczy dzień pracownika (np. 11 wierszy dla PP-01,
2026-09-01..2026-09-11). Wcześniejsza wersja (zakres dat + zaznaczanie
dni tygodnia, rozwijana przez makro pętlą `For d = dataOd To dataDo`)
została zastąpiona tym prostszym modelem:

- Kolumny „Plan” teraz: `Kod pracownika | Data | Strefa 1 od | Strefa 1
  czas | Strefa 2 od | Strefa 2 czas | Strefa 3 od | Strefa 3 czas |
  Strefa 4 od | Strefa 4 czas` (10 kolumn zamiast 19 — bez „Data do”,
  „Pn..Nd”, „Uwagi”).
- Makro (`modGeneratorPlanuPracy.bas`) już nie rozwija zakresów dat ani
  dni tygodnia — każdy wiersz arkusza generuje dokładnie jeden
  `<DzienPlanu>` w pliku wynikowym. Kto chce zaplanować cały miesiąc,
  musi mieć w arkuszu jeden wiersz na każdy dzień roboczy (Excel
  ułatwia to przez przeciągnięcie/serię dat).
- Format wynikowego XML (`Root/DniPlanu/DzienPlanu/Strefy`) się nie
  zmienił — zmiana dotyczy wyłącznie sposobu wypełniania arkusza
  „Plan” i logiki odczytu wierszy w makrze.
- Nadal **zero SQL, zero GUID-u** — bez zmian względem wcześniejszych
  ustaleń.
