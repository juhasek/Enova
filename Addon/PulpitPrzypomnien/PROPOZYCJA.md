# Pulpit przypomnień — propozycja rozwiązania

Cel: po zalogowaniu operator widzi jeden czytelny ekran z terminami w wybranym okresie
(koniec umów, badania, szkolenia BHP, uprawnienia, dokumenty, własne daty), zamiast Terminarza.
Działa w enova okienkowej i w SonetaFrame/web (2610).

Szablon widoku: [`szablon/pulpit.html`](szablon/pulpit.html) — dane fikcyjne, sprawdzony w przeglądarce enovy
(MCP `show_content`, web 2610, 29.10.2026).

## Dlaczego dodatek, a nie standard

| Opcja | Ocena |
|---|---|
| Terminarz (standard) | nieczytelny, brak kategorii/kolorów/podsumowań |
| Kokpity | tylko TRIVA — w enova365 folder niewidoczny |
| Ekran modułu (FolderMenu) | rysuje tylko kafelki — `Html`, `Grid`, lista aktualności się nie rysują |
| Panel BI | możliwy, ale bez osi czasu i bez prostego „kto, co, za ile dni”; reguły w modelu BI trudne do samodzielnej konfiguracji |
| **Dodatek: okno z kartą HTML + reguły w Opcjach** | ten sam wzorzec co Metryczka klienta (działa w web i okienkowej) |

## Elementy

1. **Okno „Ogólne / Pulpit przypomnień”** — `FolderView(ObjectType=..., ObjectPage=...)` (ObjectPage wymagany
   przez okienkową), w pageform:
   - nad kartą pola: **Okres** (7 / 30 / 60 / 90 dni / zakres dat, domyślnie z Opcji), **Zakres** (wszyscy /
     mój wydział), **Kategorie** (wybór wielu);
   - `<Html Height="*">` z kartą jak w szablonie: nagłówek, 5 kafelków KPI, oś czasu (Po terminie / Ten tydzień /
     Później), podsumowanie wg kategorii, notatki działu;
   - czynność **Pokaż listę** — te same pozycje jako zwykła lista (sortowanie, eksport, wejście w kartotekę).
2. **Reguły w Narzędzia → Opcje → Pulpit przypomnień** (węzeł konfiguracji, jak w Metryczce) — operator sam
   dodaje/zmienia, bez programisty. Jedna reguła = jeden rodzaj terminu:

   | Pole | Przykład |
   |---|---|
   | Nazwa | Badanie okresowe — ważność |
   | Kategoria + kolor | Badania lekarskie, turkus |
   | Źródło | lista wbudowanych (poniżej) albo **Cecha typu Data** (tabela + nazwa cechy) |
   | Ostrzegaj dni przed | 30 |
   | Pokazuj po terminie (dni) | 60 |
   | Aktywna | Tak |

   Źródła wbudowane (do potwierdzenia nazw pól w DLL przy implementacji): koniec umowy o pracę na czas
   określony/próbny (historia etatu), koniec umowy cywilnoprawnej, ważność badania lekarskiego, ważność szkolenia
   BHP, ważność uprawnień/kwalifikacji, ważność dokumentu tożsamości/pobytu.
   **„Inne informacje”** = źródło *Cecha typu Data* — dowolna nowa data (ocena okresowa, przegląd auta służbowego…)
   to nowa cecha + nowa reguła, bez zmiany kodu. Notatki działu = osobna lista w Opcjach (data + tekst).
3. **Start po zalogowaniu** (ustalone dekompilacją 2610):
   - web / SonetaFrame: Ulubione → „Startowy” (`FavoritesData.HomeFolder`) — operator wskazuje folder pulpitu,
     zero kodu;
   - kod (obie aplikacje, do sprawdzenia w okienkowej): `[assembly: Service(typeof(ILoginListenerUI), typeof(PulpitPoZalogowaniu), ServiceScope.Login)]`
     + `DataFormStyle(UseDialog = true)` — tak jak standardowe `BetaModeInformation`; w `AfterLoginResult`
     dodać obiekt do `args.Values`, gdy są pozycje po terminie lub w 7 dni (opcja w Opcjach: zawsze / tylko gdy
     coś pilnego / nigdy).
4. **Kafelek na ekranie Kadr i płac** „Przypomnienia: 3 po terminie · 4 w 7 dni” otwierający pulpit.
   Uwaga: zasób `FolderMenu-KadryIPlace.form.xml` ma już Metryczka klienta — jeden zasób na ekran, więc kafelek
   dopisać w tym samym miejscu (albo przenieść układ ekranu do wspólnego dodatku).

## Nowy termin u klienta (np. koniec aneksu czasowego, koniec orzeczenia)

Dodatek nie musi znać miejsca, w którym leży data — czyta ją z **cechy typu Data**. Trzy poziomy:

| Gdzie jest data | Co robimy | Kod w dodatku |
|---|---|---|
| Klient wpisuje ją ręcznie | zwykła cecha Data + reguła | nie |
| Jest w danych enovy (historia etatu, orzeczenia) | **cecha z algorytmem** zwracająca datę + reguła | nie — kilkanaście linii w algorytmie cechy |
| Potrzebna u wielu klientów | przeniesienie do źródeł wbudowanych dodatku | tak |

Zalety cechy z algorytmem: różnice wersji schowane w jednym miejscu (orzeczenie: w 2512 na zapisie historii
`StopienNiepelnosp`/`PFRON`, w 2610 `OrzeczeniaPFRON`/`OrzeczeniaZUS` z okresem obowiązywania), cecha jest też
kolumną/filtrem na liście pracowników, a pulpit tylko ją czyta.

- **Koniec orzeczenia** — algorytm: orzeczenie obowiązujące dziś (lub najbliższe), zwraca datę końca; brak → pusta.
- **Koniec aneksu czasowego** — w enova aneks to zapis historii pracownika z datą końca. Do ustalenia u klienta,
  po czym poznać aneks czasowy (rodzaj/przyczyna zmiany, opis), żeby nie łapać zwykłych zmian warunków.

## Wariant bez DLL (gdy klient nie zgodzi się na dodatek)

Makieta: [`szablon/lista-bez-dll.html`](szablon/lista-bez-dll.html). Wszystko w bazie:
- cechy pracownika z datami (jak wyżej) + cechy z algorytmem `NajblizszyTermin`, `TerminCzego`, `DniDoTerminu`;
- własny widok „Przypomnienia” pod Kadry → Pracownicy (viewform w konfiguracji): kolumny `{Features.…}`,
  kolory `<Appearance Condition="{?…}">` (czerwony < 0, pomarańczowy ≤ 7, żółty ≤ 30), filtr „Dni ≤ 30” zapisany w widoku;
- start: Ulubione → „Startowy” (web/SonetaFrame); w okienkowej — ręcznie z Ulubionych.

Ograniczenia: wiersz = pracownik (nie pojedynczy termin), brak kafelków KPI, osi czasu i notatek, nowy termin
wymaga też dopisania kolumny w widoku. Do sprawdzenia na żywo: `Appearance` i filtr na cesze z algorytmem.

## Ograniczenia HTML w enovie (zastosowane w szablonie)

- bez `var(--x)` i bez `style=` — tylko klasy w `<style>`, kolory na stałe (kolor reguły = jedna z gotowych klas);
- okienkowa = WebBrowser (IE): `<meta X-UA-Compatible IE=edge>`, układ na `<table>`, bez flex/gap/grid;
- HTML nie jest interaktywny — filtry i nawigacja to pola i czynności formularza nad kartą.

## Kolejność wdrożenia

1. Akceptacja wyglądu szablonu (kolory, sekcje, co w kafelkach).
2. Dodatek: reguły w Opcjach + okno z kartą + 3 źródła (umowy, badania, BHP) + źródło Cecha Data.
3. Start po zalogowaniu (Startowy w web; listener — test w okienkowej).
4. Kafelek na ekranie Kadr i płac, pozostałe źródła, zakres „mój wydział”.

Otwarte: dokładne tabele/pola badań, szkoleń i uprawnień w bazie klienta; czy pulpit ma być per rola
(np. inny zestaw reguł dla kierownika).
