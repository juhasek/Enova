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

Arkusz `.xlsx` jest wyciągiem z pliku `.md` (pytania z rozdziałów 1–17, checklista
z rozdziału 0, czerwone flagi i tabela sygnałów). Przy zmianie pytań najpierw
poprawiamy `.md`, potem odświeżamy arkusz — inaczej oba pliki się rozjadą.
