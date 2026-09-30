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
| [Analiza przedwdrożeniowa - Kadry, Płace, Czas pracy.md](Analiza%20przedwdro%C5%BCeniowa%20-%20Kadry%2C%20P%C5%82ace%2C%20Czas%20pracy.md) | Pełny zestaw pytań na warsztat, podzielony na 17 obszarów, z „czerwonymi flagami" (co zwykle oznacza customizację) i uwagami metodycznymi dla konsultanta. |
| `Analiza przedwdrożeniowa - kwestionariusz.xlsx` | Ten sam zestaw pytań w formie arkusza roboczego do wypełniania na spotkaniu. |
| [Rozdział 1 - Zakres procesów objętych analizą i wdrożeniem.md](Rozdzia%C5%82%201%20-%20Zakres%20proces%C3%B3w%20obj%C4%99tych%20analiz%C4%85%20i%20wdro%C5%BCeniem.md) | Macierz zakresu: 27 procesów i 482 pozycje funkcjonalne obszaru Kadry/Płace/Czas pracy, z numerem strony instrukcji producenta, wymaganym wariantem licencji i wstępną klasyfikacją S/K/D/X. Rozdział otwierający dokument analizy — to w nim zamykamy zakres. |

## Arkusze w pliku xlsx

- **Instrukcja** — jak prowadzić warsztat, legenda klasyfikacji S/K/C/X.
- **Materiały od klienta** — checklista dokumentów do zebrania przed spotkaniem.
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

Arkusz `.xlsx` jest **generowany** z pliku `.md` (pytania z rozdziałów 1–17,
checklista z rozdziału 0, czerwone flagi i tabela sygnałów). Źródłem prawdy jest
`.md` — pytania poprawiamy tam, a arkusz odświeżamy generatorem, inaczej oba
pliki się rozjadą.

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
- parser rozpoznaje w `.md`: nagłówki `## <nr>. <nazwa>`, numerowane pytania
  (`1. …`, z wcięciem dla kontynuacji), pozycje `- [ ]` w rozdziale 0, akapity
  `**Czerwone flagi …:**` oraz wiersze tabeli sygnałów w rozdziale 19 —
  przy większej zmianie układu dokumentu trzeba poprawić też generator;
- każde uruchomienie zmienia plik `.xlsx` binarnie (metadane), więc `git diff`
  pokaże zmianę nawet przy identycznej treści.

## Rozdział 1 a kwestionariusz

Oba pliki służą do czego innego i nie zastępują się wzajemnie:

- **kwestionariusz** (151 pytań) to *pytania*, które zadajemy na warsztacie —
  prowadzi rozmowę i wyciąga z klienta to, czego sam nie powie;
- **Rozdział 1** to *zakres* — lista funkcjonalności produktu, przy której
  zapisujemy odpowiedzi w formie nadającej się do umowy i wyceny.

Praktycznie: kwestionariuszem rozmawiamy, w Rozdziale 1 notujemy ustalenia.
Pozycja z Rozdziału 1 oznaczona **D** powinna mieć odpowiednik w „Rejestrze
customizacji" arkusza kwestionariusza.

Lista funkcjonalności w Rozdziale 1 pochodzi ze spisu treści instrukcji
producenta „Kadry Płace i HR" (2196 stron, 671 pozycji spisu treści), a kolumna
*Instr.* podaje numer strony. Przy nowej wersji instrukcji numery stron się
przesuną — wtedy trzeba je odświeżyć, a nie zakładać, że są wieczne.
