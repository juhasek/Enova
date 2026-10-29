# Metryczka klienta – instrukcja dla użytkownika

Metryczka klienta zbiera w jednym miejscu najważniejsze informacje o kliencie: zakres usług, listy płac,
integracje, wydruki, bank, kontakty, instytucje. Dane uzupełniasz w **Opcjach**, a program sam układa z nich
czytelną **kartę metryczki** z kolorowymi sekcjami i licznikiem, ile pozycji jest już uzupełnionych.

Pracujesz w dwóch miejscach:

| Co robisz | Gdzie |
|---|---|
| Ustalasz listę pól i wpisujesz wartości | **Narzędzia → Opcje → A1Testy → Metryczka klienta** |
| Oglądasz gotową kartę | kafelek **Metryczka klienta** na ekranie modułu **Kadry i płace** albo menu **Ogólne → Metryczka klienta** |

---

## Krok 1. Otwórz stronę metryczki w Opcjach

1. Wybierz **Narzędzia → Opcje**.
2. W drzewie po lewej rozwiń **A1Testy** i kliknij **Metryczka klienta**.

Na górze strony zobaczysz stan, np. **„Uzupełniono 23 z 43”**, a pod nim przyciski:

- **+ Dodaj pole** – nowa pozycja metryczki,
- **Zmień nazwę** – poprawienie nazwy istniejącej pozycji,
- **Usuń pola** – usunięcie jednej lub kilku pozycji,
- **Wczytaj brakujące pola z metryczki** – widoczny tylko wtedy, gdy brakuje którejś z pozycji standardowej metryczki.

Niżej pola są pogrupowane w sekcje według kategorii (każda kategoria = osobna sekcja na karcie).

> **Ważne:** wszystkie zmiany na tej stronie (także dodanie, zmiana nazwy i usunięcie pól) zostają zapisane
> dopiero po kliknięciu **Zapisz** w oknie Opcji. Zamknięcie okna bez zapisu = zmiany przepadają.

---

## Krok 2. Przygotuj listę pól

### Pierwsze uruchomienie – wczytaj standardowe pola

Jeśli strona jest pusta albo widzisz przycisk **Wczytaj brakujące pola z metryczki**:

1. Kliknij **Wczytaj brakujące pola z metryczki**.
2. Program dopisze brakujące pozycje standardowej metryczki (nazwa klienta, numer klienta, FTE, liczba kartotek,
   specjalista, zakres usług itd.). Pól, które już masz, nie zmienia.
3. Kliknij **Zapisz**.

### Dodanie własnego pola

1. Kliknij **+ Dodaj pole**.
2. Uzupełnij okno:
   - **Nazwa pola** – tekst, który zobaczysz na karcie, np. *„Czy klient korzysta z e-Teczki?”* (wymagane),
   - **Kategoria (karta)** – wybierz istniejącą sekcję z listy, **albo**
   - **albo nowa kategoria** – wpisz nazwę nowej sekcji, np. *„Testowa kategoria”*
     (jeśli wpiszesz nową kategorię, ma ona pierwszeństwo przed wyborem z listy),
   - **Typ pola**:
     - **Tak/Nie** – na stronie pojawi się pole do zaznaczenia, na karcie plakietka *✓ Tak* albo *Nie*,
     - **Tekst** – pole do wpisania dowolnej wartości (liczba, data, nazwisko, opis).
3. Kliknij **OK**. Pole pojawi się od razu na końcu swojej sekcji.
4. Kliknij **Zapisz**.

### Zmiana nazwy pola

1. Kliknij **Zmień nazwę**.
2. W polu **Pole** wybierz pozycję z listy, w polu **Nowa nazwa** wpisz nową nazwę.
3. Kliknij **OK**, a potem **Zapisz**. Wpisana wartość pola zostaje bez zmian.

### Usunięcie pól

1. Kliknij **Usuń pola**.
2. W kolumnie **Usuń** zaznacz wszystkie pozycje, których nie potrzebujesz (możesz kilka naraz).
3. Kliknij **OK**, a potem **Zapisz**.

> Usunięcie pola usuwa też jego wartość. Nie usuwaj pól z nagłówka karty (patrz niżej), bo zielony pasek
> pokaże wtedy „brak” albo „–”.

---

## Krok 3. Uzupełnij wartości

Na stronie **A1Testy → Metryczka klienta**:

- przy polach **Tak/Nie** zaznacz pole, jeśli odpowiedź brzmi „tak”; zostaw puste, jeśli „nie”,
- przy polach tekstowych wpisz wartość, np. *500*, *październik 2026*, *ING*, *Jan Kowalski*.

Na koniec kliknij **Zapisz**. Licznik „Uzupełniono X z Y” zaktualizuje się po zapisie.

**Pola w zielonym nagłówku karty.** Te pozycje standardowej metryczki nie trafiają do sekcji, tylko do nagłówka:

| Pole | Gdzie na karcie |
|---|---|
| Nazwa klienta | duży tytuł |
| Nr klienta, Nazwa bazy SQL, JG SAP | linia pod tytułem |
| FTE, Liczba kartotek, Specjalista | trzy kafelki pod tytułem |

---

## Krok 4. Wygeneruj (otwórz) kartę metryczki

Karta tworzy się automatycznie z aktualnych danych za każdym razem, gdy ją otwierasz – nie trzeba niczego
uruchamiać ani przeliczać.

1. Przejdź do modułu **Kadry i płace**.
2. Na samej górze ekranu kliknij kafelek **Metryczka klienta** (pod nazwą widać stan, np. *„Uzupełniono 23 z 43 pozycji”*).
   Możesz też wybrać z menu **Ogólne → Metryczka klienta**.

Na karcie zobaczysz:

- **zielony nagłówek** – nazwa i numer klienta, baza SQL, JG SAP, kafelki FTE / Kartotek / Specjalista,
  pasek uzupełnienia w procentach,
- **kolorowe sekcje** – po jednej na kategorię, w prawym górnym rogu licznik uzupełnionych pozycji (np. *3/5*),
- **plakietki wartości**:
  - *✓ Tak* – zaznaczone,
  - *Nie* – niezaznaczone / nie dotyczy,
  - *brak* – pole tekstowe nieuzupełnione (do dopisania).

---

## Najczęstsze pytania

**Dodałem pole, ale nie widzę go na karcie.**
Sprawdź, czy kliknąłeś **Zapisz** w oknie Opcji. Jeśli karta była otwarta wcześniej, zamknij ją i otwórz ponownie.

**Karta pokazuje „brak” przy polu, które uzupełniłem.**
Wartość nie została zapisana – wróć do Opcji, wpisz ją jeszcze raz i kliknij **Zapisz**.

**Pole Tak/Nie pokazuje „Nie”, a licznik go nie liczy.**
Pole nigdy nie było zapisane. Zaznacz je (lub zaznacz i odznacz) i kliknij **Zapisz** – wtedy liczy się jako uzupełnione.

**Jak zmienić kolejność sekcji na karcie?**
Sekcje są ułożone w kolejności pierwszego pola każdej kategorii. Nowa kategoria pojawia się na końcu.

**Dwie osoby edytują metryczkę jednocześnie.**
Druga osoba nie widzi niezapisanych zmian pierwszej. Wprowadzajcie zmiany po kolei i zawsze kończcie przyciskiem **Zapisz**.
