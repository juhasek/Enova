# Ankieta elementu wynagrodzenia – założenia i plan

Propozycja rozbudowy narzędzia `Addon/ImportDefinicjiElementow/` (stan 2026-10-27): zamiast zaczynać od pól enova,
klient zaczyna od **krótkiej ankiety w języku kadrowym**. Odpowiedzi same wypełniają pola definicji. Dopiero gdy
składnik okaże się nietypowy, klient (albo wdrożeniowiec) przechodzi do szczegółowych pól.

## 1. Założenia

### Trzy poziomy szczegółowości

| Poziom | Kto | Co widzi | Kiedy |
|---|---|---|---|
| **1. Ankieta** | klient | 10–15 pytań biznesowych, jedno na ekran, odpowiedzi do wyboru + „Nie wiem” | zawsze, na start każdej definicji |
| **2. Szczegóły** | klient | obecna strona klienta, ale **tylko pola z obszarów, które ankieta oznaczyła jako nietypowe** | gdy odpowiedź wskazuje złożoność („inaczej”, „zależy od…”) |
| **3. Wdrożeniowiec** | wdrożeniowiec | pełny formularz (201 pól), kod algorytmu C#, uwagi | „Nie wiem”, algorytm nie da się kreatorem, sprzeczne odpowiedzi |

### Zasady

1. **Ten sam plik wynikowy.** Ankieta tylko ustawia wartości pól – `elementy-wynagrodzenia.json` ma ten sam format.
   Dochodzi jedna sekcja `ankieta` (odpowiedzi + wersja ankiety) – informacyjnie, dla wdrożeniowca i raportu.
   **Dodatek importu bez zmian w logice** (najwyżej pokazuje odpowiedzi w raporcie HTML).
2. **Jedno źródło prawdy.** Pytania i mapowania w nowym ręcznym pliku `schema/ankieta.json` (obok `opisy.json`,
   `zrodla.json`); generator wkleja je do schematu i **sprawdza** (każde mapowane pole istnieje, wartości enumów są
   dozwolone, warunki w DSL `warunki.js` się parsują). Nie piszemy drugiego silnika reguł.
3. **Ankieta wybiera punkt startu, nie liczy prawa.** Odpowiedź „rodzaj składnika” wybiera szablon (`szablony`) albo
   najbliższą definicję standardową enova (`definicjeStandardowe`), kolejne odpowiedzi nadpisują pojedyncze pola.
   Kolejność: szablon → odpowiedzi → ręczne zmiany w szczegółach (zawsze wygrywają, oznaczone „zmieniono ręcznie”).
4. **Każda wartość ma uzasadnienie.** Przy polu widać, skąd pochodzi: „z szablonu”, „z odpowiedzi na pytanie 5”,
   „wpisane ręcznie”, „domyślne enova”. Wdrożeniowiec widzi to samo w pełnym formularzu.
5. **„Nie wiem” nigdy nie blokuje.** Zostawia wartość z szablonu i dopisuje pytanie do listy „do wyjaśnienia
   z wdrożeniowcem”. Akceptacja klienta jest możliwa, ale import ostrzega (jak dziś przy braku akceptacji).
6. **Język klienta, skutki zamiast nazw pól.** Np. nie „Korekta → Usprawiedliwione: Proporcjonalnie”, tylko
   „Czy składnik jest pomniejszany, gdy pracownik jest na zwolnieniu lekarskim?”. Pod odpowiedzią podsumowanie
   „co to znaczy w enova” (rozwijane, dla dociekliwych).
7. **Obszary z ryzykiem prawnym (PIT, ZUS, podstawa zasiłku/urlopu)** – ankieta pyta, ale wynik zawsze oznacza
   „do potwierdzenia przez wdrożeniowca”, jeśli odpowiedź odbiega od szablonu.
8. **Wdrożeniowiec konfiguruje ankietę** tak jak dziś stany pól: może ukryć pytanie, odpowiedzieć z góry,
   zablokować odpowiedź. Konfiguracja nadal w `<script id="konfiguracja">` – jedna strona, offline.

### Robocza lista pytań (do warsztatu)

| # | Pytanie | Przykładowe odpowiedzi | Ustawia (obszar) |
|---|---|---|---|
| 1 | Jak nazywa się składnik u Was? | tekst | Nazwa, Skrót (propozycja) |
| 2 | Czym jest ten składnik? | wynagrodzenie zasadnicze / premia / dodatek / nagroda / potrącenie / świadczenie w naturze / inne | szablon, Rodzaj, Potrącenie |
| 3 | Kogo dotyczy? | pracownicy etatowi / zleceniobiorcy / obie grupy | Zatrudnienie, Umowy |
| 4 | Jak ustalana jest kwota? | stała kwota / % od innych składników / stawka × godziny / stawka × ilość / wpisywana co miesiąc / **inaczej** | Algorytm (kreator), Nazwy parametrów |
| 5 | (gdy %) Od czego liczony jest procent? | zasadnicze / zasadnicze + dodatki / wskazane składniki / **inaczej** | Suma elementów |
| 6 | Jak często wypłacany? | co miesiąc / co kwartał / raz w roku / jednorazowo | Okres naliczania |
| 7 | Za jaki okres? | za bieżący miesiąc (z góry) / za poprzedni (z dołu) | Naliczanie, opóźnienie |
| 8 | Czy pomniejszany za czas choroby? | nie / proporcjonalnie / tracony w całości / **inaczej** | Korekta – nieobecności |
| 9 | Czy pomniejszany za urlop / inne nieobecności? | jw. | Korekta, Nieobecności |
| 10 | Czy wliczany do podstawy chorobowego? | tak / nie / tylko gdy nie jest wypłacany w chorobie / nie wiem | Nieobecności – zasiłki |
| 11 | Czy wliczany do wynagrodzenia urlopowego / ekwiwalentu? | stały / zmienny / nie | Nieobecności – urlop |
| 12 | Opodatkowanie i ZUS | jak wynagrodzenie / zwolniony z ZUS / zwolniony z PIT / **inaczej** | Deklaracje |
| 13 | Na której liście płac? | z wynagrodzeniem / osobna lista / nie wiem | Definicja listy płac |
| 14 | Czy jest coś nietypowego? | nie / tak – opis | uwagi, eskalacja na poziom 3 |

Odpowiedź **„inaczej”** = eskalacja na poziom 2 dla danego obszaru (pola tego obszaru dostają stan „Klient może
zmienić”). Sprzeczne odpowiedzi albo konieczność kodu C# (np. warunki zależne od stażu i działu naraz) = poziom 3.

## 2. Plan

| Etap | Zakres | Wynik / kryterium odbioru |
|---|---|---|
| **0. Katalog pytań** | warsztat z Tobą (+ ewentualnie konsultant płacowy): lista pytań, odpowiedzi, mapowania na pola; podstawa – instrukcja s. 1668–1764 i istniejące szablony | `schema/ankieta.json` v0.1, tabela „odpowiedź → pola” zaakceptowana |
| **1. Schemat i walidacja** | generator wczytuje `ankieta.json`, sprawdza mapowania; test **pokrycia**: każda kombinacja odpowiedzi (bez „inaczej”) daje definicję bez błędów wg `warunki.js` | test w `node --test`, generator nie przechodzi przy złym mapowaniu |
| **2. Test odwrotny na bazie Claude** | dla ~15 definicji standardowych enova: wypełnić ankietę „jak by to zrobił klient”, porównać wynik z prawdziwą definicją | raport różnic; każda różnica = brakujące pytanie albo złe mapowanie → poprawka etapu 0 |
| **3. Interfejs ankiety** | kreator w formularzu klienta: pytanie na ekran, pasek postępu, „Wstecz”, „Nie wiem”, na końcu podsumowanie prostym językiem („Premia 10% od zasadniczego, co miesiąc, nie pomniejszana w chorobie, wliczana do podstawy zasiłku…”) | E2E w Edge: przejście ankiety → plik JSON identyczny z ręcznie wypełnionym |
| **4. Eskalacja i pochodzenie wartości** | „inaczej” odsłania pola obszaru; znaczniki pochodzenia wartości; ręczna zmiana pola ustawionego przez ankietę – oznaczenie i pytanie „zmienić też odpowiedź?” | test reguł pierwszeństwa szablon → ankieta → ręcznie |
| **5. Wdrożeniowiec** | konfiguracja ankiety w trybie „Formularz dla klienta” (ukryj/odpowiedz z góry/zablokuj pytanie); w pełnym formularzu zakładka „Ankieta” z odpowiedziami i listą „do wyjaśnienia” | konfiguracja w wygenerowanym HTML klienta |
| **6. Dodatek** | tylko raport: odpowiedzi ankiety i lista „Nie wiem” w raporcie HTML; ostrzeżenie przy „Nie wiem” w obszarze PIT/ZUS | xUnit + test importu na żywo w Claude |
| **7. Pilot** | 5–10 rzeczywistych składników od jednego klienta, obserwacja: ile definicji przeszło samą ankietą, ile poziom 2/3 | decyzja o dalszym rozwoju / poprawki pytań |

Kolejność etapów 0 → 2 jest celowa: **najpierw sprawdzamy, czy pytania w ogóle wystarczą** (test odwrotny),
dopiero potem budujemy interfejs.

## 3. Mocne strony

- **Próg wejścia dla klienta spada drastycznie** – 10–15 pytań zamiast 201 pól; pytania o skutki, które kadrowa zna
  z praktyki („czy zabieramy w chorobie?”), a nie o nazwy pól enova.
- **Spójność definicji** – podobne składniki dostają identyczne ustawienia (mapowanie deterministyczne), mniej
  przypadkowych różnic niż przy ręcznym klikaniu.
- **Bez zmian w imporcie** – plik JSON, walidacja, dodatek i akceptacja klienta działają jak dziś; ryzyko regresji małe.
- **Lepszy materiał na rozmowę** – odpowiedzi + lista „Nie wiem” to gotowa agenda spotkania z klientem; łatwo
  wychwycić, gdzie klient sam nie zna zasad (częste przy regulaminach wynagradzania).
- **Wiedza zamknięta w mapowaniach** – doświadczenie wdrożeniowca („premia uznaniowa → tak ustawiamy chorobowe”)
  zapisane raz, sprawdzone testem odwrotnym, użyteczne także dla nowych konsultantów.
- **Stopniowanie** – prosty składnik kończy się na ankiecie, złożony płynnie przechodzi dalej; nikt nie traci
  dotychczasowych możliwości.

## 4. Słabe strony i ryzyka

- **Uproszczenie może być mylące** – odpowiedź „tak, pomniejszany” ukrywa kilka pól enova (odchyłki, płatne,
  macierzyńskie, rehabilitacyjne…). Klient może być przekonany, że coś ustalił, a mapowanie przyjęło założenie.
  *Łagodzenie:* podsumowanie „co to znaczy w enova” + test odwrotny + oznaczenie „do potwierdzenia” w obszarach prawnych.
- **Utrzymanie dwóch warstw** – każda zmiana schematu enova (nowa wersja) może wymagać poprawy mapowań.
  *Łagodzenie:* walidacja w generatorze – zła ścieżka/enum zatrzymuje build.
- **Odpowiedzialność za PIT/ZUS/podstawy** – ankieta sugeruje „prosto i bezpiecznie”, a błąd tu ma skutki prawne
  i finansowe. Narzędzie nie może zastąpić weryfikacji wdrożeniowca – trzeba to jasno komunikować klientowi.
- **Koszt etapu 0** – dobranie pytań i mapowań to praca merytoryczna (płacowa), nie programistyczna; bez dobrego
  katalogu reszta nie ma sensu.
- **Konflikty odpowiedzi i ręcznych zmian** – klient zmienia pole w szczegółach, potem wraca do ankiety i zmienia
  odpowiedź: trzeba zdecydować, co wygrywa (proponuję: ręczna zmiana wygrywa, ale z wyraźnym ostrzeżeniem).
- **Większy formularz HTML** – kolejna warstwa logiki w jednym pliku offline; rośnie złożoność testów E2E.
- **Nie zastąpi algorytmów C#** – składniki z nietypową logiką i tak trafią do wdrożeniowca; ankieta tylko szybciej
  to wykryje.

## 5. Decyzje do podjęcia

1. Czy ankieta ma zastąpić obecną stronę klienta jako **start**, czy być opcją („Wypełnij ankietę” / „Wypełnij pola”)?
   Rekomendacja: start, ze skrótem do pól dla klientów, którzy znają enova.
2. Kto ustala katalog pytań (etap 0) – Ty sam, czy z konsultantem płacowym? Od tego zależy termin.
3. Czy w ankiecie pytamy o PIT/ZUS (pytanie 12), czy zostawiamy to w całości wdrożeniowcowi?
4. Pilot – który klient i które składniki.
