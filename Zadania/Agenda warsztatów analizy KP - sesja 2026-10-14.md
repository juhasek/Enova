# Agenda warsztatów analizy KP — sesja 2026-10-14

## 1. Wejście

Agenda konsultanta (`Pobrane/agenda.xlsx`, poza repo) na 4 spotkania analityczne z klientem:

| Dzień | Termin | Temat w agendzie wyjściowej |
|---|---|---|
| 1 | 20.10.2026, 10:30–14:30 (4 h) | Kadry cz. 1 |
| 2 | 21.10.2026, 9:00–15:00 | Kadry cz. 2 & Płace cz. 1 |
| 3 | 23.10.2026, 9:00–15:00 | Płace cz. 2 |
| 4 | 26.10.2026, 9:00–15:00 | Obszar Czasu Pracy |

Komentarz w agendzie zawiera zakres z umowy: prezentacja standardu + matryca wymagań; struktura
organizacyjna, obszar kadrowy, systemy płacowe, składniki płacowe, zasiłki chorobowe, UCP, UoP, BHP,
e-deklaracje, zajęcia komornicze, role i uprawnienia operatorów.

## 2. Wynik — dwa warianty (folder `Agenda warsztatów analizy KP/`)

1. **Korekta agendy wyjściowej** (`Agenda warsztatów analizy KP.xlsx/.md`) — układ dni bez zmian,
   uzupełnione braki względem umowy (zasiłki, BHP, e-deklaracje, zajęcia komornicze, role
   i uprawnienia), rozdzielony czas pracy (dz. 2 = zasady, dz. 4 = proces), struktura na początek
   dnia 1, otwarcie i podsumowania, przerwy. Arkusz „Uwagi do agendy” — 10 uwag.
2. **Propozycja autorska** (`… - propozycja autorska.xlsx/.md`) — ułożona od zera wg zależności
   danych w enova365: dz. 1 fundamenty (struktura, zatrudnienie, kartoteka, wybór pracowników
   wzorcowych), dz. 2 cały czas pracy, dz. 3 nieobecności i składniki, dz. 4 proces miesięczny,
   deklaracje, pulpity, uprawnienia, migracja, matryca. Do tego: 10 pracowników wzorcowych
   (późniejsze scenariusze odbiorowe), pokaz na danych klienta (baza testowa 22.10 i 24–25.10),
   oznaczone decyzje trudne do zmiany po starcie, tabela „strefa czasu pracy → składnik”.

Oba warianty mają w każdym bloku: co pokazujemy w standardzie (strona instrukcji „Kadry Płace i HR”,
oznaczenie licencji P/Z), kluczowe pytania, odwołanie do `Analiza/` (rozdz., KAD-/CP-/PL-…),
ryzyka/decyzje i kolumnę na notatki S/K/C/X. Dodatkowe arkusze: „Pokrycie umowy”, „Materiały przed
spotkaniem”.

## 3. Generator

`Agenda warsztatów analizy KP/Generator/` — konsola .NET 8, DevExpress.Spreadsheet z katalogu serwera
(jak `Analiza/Generator`). Treść w `AgendaKorekta.cs` i `AgendaAutorska.cs`, rendering w `Program.cs`;
jedno uruchomienie odświeża oba `.xlsx` i `.md`:

```bash
cd "Zadania/Agenda warsztatów analizy KP/Generator"
dotnet build && dotnet bin/Debug/net8.0/GenAgenda.dll
```

Commity: `8f0e5fc` (korekta), `143debd` (propozycja autorska).

## 4. Do zrobienia

1. Obejrzeć arkusze w Excelu (wysokość wierszy, podgląd wydruku) — nie były oglądane wizualnie.
2. Przed 20.10: potwierdzić wariant licencji klienta (Złota/Platynowa) i przygotować bazę testową.
3. Wybrać wariant agendy i wysłać klientowi listę materiałów (arkusz „Materiały przed spotkaniem”).
