# Analiza

Materiały do **analizy przedwdrożeniowej** — spotkań z klientem, na których
ustalamy docelową konfigurację systemu w obszarze kadr, płac i czasu pracy,
pokazujemy, co enova365 daje w standardzie, i wskazujemy wprost, co będzie
wymagało kodu pisanego pod klienta.

Folder trzyma **szablony**, nie ustalenia konkretnego wdrożenia. Notatki
z konkretnego klienta robimy jako kopię szablonu (nazwaną tak, by nie zdradzała
klienta) albo w folderze `Zadania/`.

## Zawartość

| Plik | Do czego służy |
|---|---|
| [Analiza przedwdrożeniowa - Kadry, Płace, Czas pracy.md](Analiza%20przedwdro%C5%BCeniowa%20-%20Kadry%2C%20P%C5%82ace%2C%20Czas%20pracy.md) | Cały materiał na warsztat w jednym dokumencie: **rozdział 1** to macierz zakresu (27 procesów, 482 pozycje funkcjonalne z numerem strony instrukcji producenta i wymaganym wariantem licencji), **rozdziały 2–18** to pytania warsztatowe z „czerwonymi flagami", **19–21** to podsumowanie, sygnały ostrzegawcze i uwagi metodyczne. |
| `Analiza przedwdrożeniowa - kwestionariusz.xlsx` | Ten sam materiał w formie arkusza roboczego do wypełniania na spotkaniu. |

## Arkusze w pliku xlsx

- **Instrukcja** — jak prowadzić warsztat, legenda klasyfikacji S/K/C/X.
- **Materiały od klienta** — checklista dokumentów do zebrania przed spotkaniem.
- **Zakres procesów** — 482 pozycje funkcjonalne z rozdziału 1; kolumny na
  wymaganie klienta, klasyfikację (wstępnie wypełnioną propozycją konsultanta),
  „czy na start", szacunek godzin, osobę decyzyjną i opis realizacji.
  Dodatkowo numer strony instrukcji producenta i wymagany wariant licencji.
- **Kwestionariusz** — 151 pytań; kolumny na odpowiedź, klasyfikację,
  „czy blokuje start", szacunek godzin i osobę decyzyjną.
- **Czerwone flagi** — ściąga: sytuacje, które zwykle oznaczają kod, nie konfigurację.
- **Sygnały ostrzegawcze** — zdania klienta i ich tłumaczenie na zakres prac.
- **Rejestr customizacji** — pusty, do wypełnienia tym, co wyszło jako „C".
- **Otwarte pytania** — pusty, na tematy nierozstrzygnięte na spotkaniu.

## Klasyfikacja tematów

Każdy temat z warsztatu oznaczamy jedną literą:

- **S** — standard, działa bez zmian;
- **K** — konfiguracja (definicje, cechy, kalendarze, prawa);
- **C** — customizacja: element płacowy z algorytmem, weryfikator, raport,
  worker albo dodatek DLL;
- **X** — poza zakresem / decyzja po stronie klienta.

Sens jest taki, żeby podział „standard vs. kod" powstawał na bieżąco, przy stole
z klientem, a nie po analizie — wtedy wycena i harmonogram wychodzą z warsztatu
gotowe, a nie są dopisywane później.

## Aktualizacja arkusza

Arkusz `.xlsx` jest **generowany** z pliku `.md` (macierz zakresu z rozdziału 1,
pytania z rozdziałów 2–18, checklista z rozdziału 0, czerwone flagi i tabela
sygnałów). Źródłem prawdy jest `.md` — treść poprawiamy tam, a arkusz odświeżamy
generatorem, inaczej oba pliki się rozjadą.

Generator: [`Generator/`](Generator/) — mała konsola .NET 8 budująca arkusz
biblioteką `DevExpress.Spreadsheet` z DLL-i serwera enova365 (nie wymaga
uruchomionej enovy ani bazy).

```bash
cd Analiza/Generator
dotnet build
dotnet bin/Debug/net8.0/GenAnaliza.dll
```

Bez argumentów czyta `.md` i nadpisuje `.xlsx` w folderze `Analiza`. Opcjonalnie
można podać własne ścieżki: `dotnet ... GenAnaliza.dll <plik.md> <plik.xlsx>`.
Po uruchomieniu wypisuje liczbę wczytanych pytań, flag, materiałów i sygnałów —
warto na nią zerknąć, bo spadek liczby oznacza, że zmiana formatowania w `.md`
rozjechała parser.

Uwagi:
- ścieżka do serwera enova365 jest w `GenAnaliza.csproj` (`EnovaDir`); przy innej
  wersji serwera: `dotnet build -p:EnovaDir="C:\enovaServer\<wersja>\Soneta.Products.Server.Standard"`;
- parser rozpoznaje w `.md`: nagłówki `## <nr>. <nazwa>`, w rozdziale 1 nagłówki
  obszarów `### XXX — nazwa`, procesów `#### XXX-00 · nazwa` i wiersze tabeli
  zaczynające się od identyfikatora `XXX-00-000`, numerowane pytania w rozdziałach
  2–18 (`1. …`, z wcięciem dla kontynuacji), pozycje `- [ ]` w rozdziale 0, akapity
  `**Czerwone flagi …:**` oraz wiersze tabeli sygnałów w rozdziale 20 —
  numery rozdziałów są stałymi na początku `Program.cs`, więc przy przenumerowaniu
  dokumentu trzeba je poprawić razem z nim;
- każde uruchomienie zmienia plik `.xlsx` binarnie (metadane), więc `git diff`
  pokaże zmianę nawet przy identycznej treści.

## Zakres a pytania

Dokument ma dwie części, które się nie zastępują:

- **rozdział 1 (zakres)** to lista funkcjonalności produktu, przy której
  zapisujemy ustalenia w formie nadającej się do umowy i wyceny;
- **rozdziały 2–18 (pytania)** to to, o co *pytamy*, żeby te ustalenia
  z klienta wydobyć — wyciągają rzeczy, których klient sam nie powie.

Praktycznie: pytaniami rozmawiamy, w rozdziale 1 notujemy. Pozycja zakresu
oznaczona **C** powinna mieć odpowiednik w arkuszu „Rejestr customizacji".

Lista funkcjonalności w rozdziale 1 pochodzi ze spisu treści instrukcji
producenta „Kadry Płace i HR" (2196 stron, 671 pozycji spisu treści), a kolumna
*Instr.* podaje numer strony. Kolumna *Lic.* jest wypełniona tylko tam, gdzie
instrukcja wprost stawia warunek licencyjny (72 pozycje platynowe, 28 złotych,
17 wymagających osobnej licencji na dodatek).

Przy nowej wersji instrukcji numery stron się przesuną — wtedy trzeba je
odświeżyć, a nie zakładać, że są wieczne. Sam PDF instrukcji nie leży w repo
(trafia do `Pobrane/`, która jest wykluczona z gita).
