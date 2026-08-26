# Aktualizacja OkresuDo w UczestnictwieWAkcji wg daty zakończenia umowy – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy task

Task (metoda `IsEnable`, uruchamiana przy zapisie kartoteki pracownika) automatycznie zamyka
uczestnictwo pracownika w akcjach pracowniczych (np. **SEOP**), gdy pracownikowi kończy się umowa
o pracę — tak, żeby `OkresDo` uczestnictwa nie sięgał dalej niż faktyczny koniec zatrudnienia.

Obejmuje dwa poziomy danych na wierszu **UczestnictwoWAkcji**:

- główny okres uczestnictwa w edycji akcji,
- powiązany z nim okres w ramach kategorii SEOP (grid „Parametry").

## 2. Kiedy się uruchamia

Przy zapisie pracownika. Task przegląda tylko wiersze `UczestnictwoWAkcji` **tego** pracownika
(`WgPracownik`), a wśród nich tylko wiersz, którego okres obejmuje dzisiejszą datę — czyli aktualnie
obowiązującą edycję akcji. Historyczne edycje (już zakończone wcześniej) nie są ruszane.

## 3. Jak liczy docelowy koniec okresu

1. **Pełny (domyślny) koniec** wynika z samej edycji akcji, do której pracownik przystąpił
   (`EdycjaAkcji.Okres.To`) — to wartość, jaką `OkresDo` ma domyślnie, dopóki nic go nie skraca.
2. **Data końca umowy** jest brana pod uwagę **tylko wtedy, gdy pracownik jest oznaczony jako
   zwolniony** (`Row.PracownikZwolniony`). Samo wpisanie daty końca umowy (np. kontrakt terminowy,
   który może zostać przedłużony) nie oznacza jeszcze zwolnienia i nie skraca uczestnictwa.
3. **Docelowy koniec** = wcześniejsza z dwóch dat: pełny koniec edycji albo data końca umowy (o ile
   dotyczy i jest wcześniejsza).

Ta sama reguła (ta sama funkcja `ObliczDocelowyKoniec`) jest stosowana zarówno do głównego wiersza
uczestnictwa, jak i do jego parametrów SEOP.

## 4. Rozróżnienie zamknięcia automatycznego od ręcznego

Wiersz `UczestnictwoWAkcji` ma cechę **AutomatyczneZakonczenie**, którą task ustawia na `true`
zawsze, gdy sam skraca `OkresDo`. Dzięki temu:

- jeśli `OkresDo` jest już krótszy niż pełny koniec edycji, a wiersz **nie** ma tej cechy — to
  była świadoma, ręczna decyzja (np. wcześniejsza rezygnacja z akcji) i task jej nie rusza,
- jeśli wiersz **ma** tę cechę, task może go dalej swobodnie korygować — również **otworzyć z
  powrotem** (wydłużyć `OkresDo` do pełnego końca edycji), jeśli ktoś odznaczy
  `PracownikZwolniony` (np. cofnięcie wypowiedzenia).

Innymi słowy: task nigdy nie nadpisuje ręcznej ingerencji, ale w pełni panuje nad tym, co sam
wcześniej zamknął — w obie strony.

## 5. Diagnostyka błędów

Każda iteracja po wierszu uczestnictwa jest opakowana w `try/catch` — w razie wyjątku (np. znany w
Enova komunikat „Błędny zakres dat") task rzuca nowy wyjątek z pełnym kontekstem: Guid wiersza,
pracownik, edycja akcji, daty przed próbą zapisu, stan cechy `AutomatyczneZakonczenie` oraz
wyliczone daty. Ułatwia to zdiagnozowanie, na którym dokładnie wierszu i przy jakich datach
wystąpił problem, bez konieczności odtwarzania scenariusza krok po kroku.

## 6. Znane ograniczenia / do potwierdzenia

- **Nazwa pola `PracownikZwolniony` nie została jeszcze potwierdzona** — w kodzie jest komentarz
  „Do potwierdzenia: dokładna nazwa/lokalizacja pola (przyjąłem `Row.PracownikZwolniony`)". Przed
  użyciem na produkcji warto zweryfikować, że pole o tej dokładnie nazwie istnieje na kartotece
  pracownika i faktycznie odzwierciedla fakt zwolnienia (a nie np. samą obecność daty rozwiązania
  umowy).
- Task honoruje tylko **aktualną** (obejmującą dzisiejszą datę) edycję akcji na danym wierszu
  `UczestnictwoWAkcji` — jeśli pracownik ma kilka historycznych edycji, wcześniejsze nie są
  przeliczane.
- Jeśli data końca umowy wypada **po** pełnym końcu edycji akcji, nie ma efektu — `OkresDo`
  pozostaje ograniczony przez naturalny koniec edycji (task nigdy nie wydłuża okresu ponad ten
  limit).
