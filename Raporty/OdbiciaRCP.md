# Raport z odbić RCP – dni zmodyfikowane i czas z planu

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy raport

Raport pokazuje, dla wybranych pracowników i wskazanego okresu, dni „Ewidencji czasu pracy”, które
zostały **zmodyfikowane** (mają zapisany rekord Dzień pracy — najczęściej w wyniku importu odbić z
systemu RCP lub ręcznej korekty), a zestawia je z dniami **niezmodyfikowanymi**, dla których czas
pracy jest przepisywany automatycznie z planu pracy.

Głównym celem raportu jest **wychwycenie dni zmodyfikowanych, dla których nie naliczono żadnych
godzin pracy** — czyli sytuacji, w której w systemie widać, że dzień był „ruszany” (np. przez import
odbić z czytnika RCP), a mimo to kalkulator czasu pracy nie wykazuje żadnego czasu. Taki dzień
w ewidencji wygląda jak nieobecność / brak czasu pracy, mimo że pracownik faktycznie rejestrował
wejścia i wyjścia. Pozostawiony bez korekty prowadzi do błędów w naliczeniu wynagrodzenia i błędnej
ewidencji czasu pracy.

Przyczyna techniczna (dla informacji): import odbić RCP zapisuje godziny w strefach dnia, a nie
bezpośrednio w polu „Praca” rekordu Dnia pracy — czas dnia jest więc wyliczany na nowo przez
kalkulator (KalkPracy). Jeśli z jakiegoś powodu kalkulator nie naliczy z tych stref żadnego czasu,
dzień zostaje „pusty”, mimo zarejestrowanych odbić.

## 2. Gdzie uruchomić raport

Raport uruchamiany jest dla zaznaczonych pracowników z poziomu:

- **Listy pracowników**, lub
- **Pulpitu kierownika**.

Jeśli w widoku, z którego uruchamiany jest raport, zaznaczone są wiersze historii zatrudnienia
(a nie sami pracownicy), raport automatycznie rozpoznaje pracownika, do którego dany wiersz należy.

Raport analizuje wyłącznie dni mieszczące się w **okresie zatrudnienia** danego pracownika — dni
poza zatrudnieniem (przed przyjęciem / po zwolnieniu) są pomijane.

## 3. Parametry raportu

| Parametr | Opis |
|---|---|
| **Okres** (wymagany) | Zakres dat, za jaki ma zostać przygotowany raport. Domyślnie podpowiadany jest bieżący miesiąc kalendarzowy, można go zmienić na dowolny inny zakres. |
| **Pokaż dni z godzinami z RCP** | Domyślnie **odznaczony** — raport pokazuje wtedy tylko dni wymagające uwagi (błędne i przepisane z planu). Po zaznaczeniu raport dodatkowo wypisuje dni zmodyfikowane, dla których poprawnie naliczono godziny z RCP — przydatne do pełnej weryfikacji odbić, nie tylko błędów. |

## 4. Jak czytać wynik – klasyfikacja dni (kolumna „Status”)

Raport klasyfikuje każdy dzień pracownika do jednej z trzech kategorii:

1. **„Zmodyfikowany – BRAK godzin pracy”** ⚠️
   Dzień posiada rekord Dnia pracy (czyli był modyfikowany, np. przez import RCP), ale naliczony
   czas pracy wynosi zero. **To są dni wymagające sprawdzenia i korekty przed naliczeniem płac** —
   główny cel raportu.

2. **„Niezmodyfikowany – czas z planu”**
   Brak rekordu Dnia pracy — dzień nie był w ogóle modyfikowany. Czas pracy wynika bezpośrednio
   z planu pracy pracownika (harmonogramu). To sytuacja normalna, wyświetlana informacyjnie, gdy
   dla danego dnia zarówno plan, jak i naliczony czas są niezerowe (dni nieobecności, wolne itp.
   są pomijane).

3. **„Zmodyfikowany – godziny z RCP”**
   Dzień posiada rekord Dnia pracy i poprawnie naliczony czas pracy > 0. Widoczne tylko przy
   zaznaczonym parametrze „Pokaż dni z godzinami z RCP”. Sytuacja poprawna — informacja
   pomocnicza, nie wymaga działania.

Wiersze ze statusem błędnym (pkt 1) są dodatkowo oznaczane wewnętrznie jako „błędne” i mogą być
wyróżnione graficznie w wydruku (np. innym kolorem/pogrubieniem), aby łatwo je odróżnić od
pozostałych.

## 5. Opis kolumn raportu

| Kolumna | Opis |
|---|---|
| Pracownik | Nazwisko i imię pracownika. |
| Kod | Kod pracownika w systemie. |
| Data | Data dnia, którego dotyczy wiersz. |
| Dzień | Skrót dnia tygodnia (Nd, Pn, Wt, Śr, Cz, Pt, So). |
| Plan od-do | Godziny pracy wynikające z planu (harmonogramu), w formacie „od–do”; puste, jeśli plan nie przewidywał pracy. |
| Plan | Suma godzin zaplanowanych na dany dzień. |
| Czas ewidencji | Faktyczny czas pracy naliczony przez kalkulator na podstawie zapisów w ewidencji / odbić RCP. |
| Odbicia | Lista zarejestrowanych wejść/wyjść w danym dniu wraz z godzinami (patrz oznaczenia w pkt 6); „brak odbić”, jeśli dzień jest zmodyfikowany, ale nie ma żadnych zarejestrowanych przejść. |
| RCP OK | Informacja, czy import odbić RCP oznaczył dzień jako poprawny („Tak”/„Nie”). |
| Status | Klasyfikacja dnia — patrz pkt 4. |

## 6. Skróty typów odbić w kolumnie „Odbicia”

| Skrót | Znaczenie |
|---|---|
| We | Wejście |
| Wy | Wyjście |
| WeS | Wejście służbowe |
| WyS | Wyjście służbowe |
| WeP | Wejście prywatne |
| WyP | Wyjście prywatne |

## 7. Podsumowanie na raporcie

Na górze wydruku wyświetlane jest podsumowanie zbiorcze dla wybranego okresu:

- liczba pracowników objętych raportem,
- liczba dni zmodyfikowanych **bez** godzin pracy (wymagających korekty),
- liczba dni z czasem przepisanym z planu,
- liczba dni zmodyfikowanych z poprawnie naliczonymi godzinami z RCP (uwzględniana tylko, gdy
  parametr „Pokaż dni z godzinami z RCP” jest zaznaczony).

## 8. Zalecany sposób pracy z raportem

1. Przed naliczeniem wynagrodzeń za dany okres uruchom raport dla wszystkich pracowników objętych
   RCP, z domyślnymi parametrami (bez zaznaczania „Pokaż dni z godzinami z RCP”).
2. Przeanalizuj wiersze ze statusem **„Zmodyfikowany – BRAK godzin pracy”** — dla każdego z nich
   sprawdź zarejestrowane odbicia (kolumna „Odbicia”) i ustal przyczynę braku naliczenia czasu
   (np. niesparowane wejście/wyjście, błąd importu, nietypowa strefa dnia).
3. Popraw dzień pracy w ewidencji pracownika lub ponów import, a następnie zweryfikuj wynik.
4. Wiersze ze statusem „Niezmodyfikowany – czas z planu” traktuj jako informacyjne — nie wymagają
   działania, o ile plan pracy dla danego dnia jest prawidłowy.
5. W razie potrzeby pełnej weryfikacji poprawnych odbić zaznacz „Pokaż dni z godzinami z RCP” i
   uruchom raport ponownie.
