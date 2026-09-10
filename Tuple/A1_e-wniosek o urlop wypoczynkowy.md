# A1_e-wniosek o urlop wypoczynkowy

Kopia standardowej definicji krotki **e-wniosek o urlop wypoczynkowy**
(`Soneta.KadryPlace.Kadry.EWnioski.EWniosekOUrlopWypoczynkowy`), rozszerzona o reguły A1.
Pracownik składa nią wniosek o urlop wypoczynkowy (Pulpit pracownika / enova365).

- Baza `Claude`: `TuplesDefs` ID **201**, Guid `C1D91709-E8C5-463F-954B-2D46E8A6DE85`,
  Nazwa „e-wniosek o urlop wypoczynkowy (1)”, wyświetlana jako „A1_e-wniosek o urlop
  wypoczynkowy”. Klasa w kodzie: `EWniosek_o_urlop_wypoczynkowy_1` (nazwa wynika z pola Nazwa —
  po zmianie nazwy definicji trzeba poprawić nazwy klas `..._1` / `..._1_Calc` w kodzie).
- Plik bez rozszerzenia obok = sekcja **Class** kodu definicji. Sekcja **Calc** (klasa
  `Parametry` z ustawieniami: mail, powiadomienie, akceptacja kadr itd.) jest bez zmian
  względem standardu i nie jest wersjonowana.

## Reguła 1 — wniosek na dzień dzisiejszy = urlop na żądanie (etap 1)

Jeżeli okres wniosku to **jeden dzień równy dzisiejszej dacie** (`Okres.From == Okres.To ==
Date.Today`), przyczyna urlopu jest ustawiana automatycznie na **Na żądanie** (pole „Urlop na
żądanie” zaznaczone), a pole jest tylko do odczytu — pracownik nie może go odznaczyć.

Szczegóły działania:

- Reguła uruchamia się przy każdej zmianie **Okresu** na formularzu oraz przy tworzeniu
  wniosku, gdy okres jest podany od razu (np. kliknięcie dnia w kalendarzu Pulpitu).
- Jeśli „Na żądanie” zostało ustawione **przez regułę**, a okres zostanie potem zmieniony na
  inny niż dzisiejszy — przyczyna wraca do **Planowy**, a pole staje się znów edytowalne.
- Jeśli pracownik **sam** zaznaczył „Urlop na żądanie” (np. na jutro), a potem zmienia okres,
  jego wybór nie jest cofany.
- Znacznik „ustawione przez regułę” żyje tylko w trakcie edycji — po ponownym otwarciu
  zapisanego wniosku reguła niczego nie cofa.
- „Dzisiaj” = data serwera enova (`Date.Today`), a nie pole „Data złożenia”.

**Poza zakresem etapu 1:** okres wielodniowy zaczynający się dzisiaj (np. dziś–jutro) —
reguła go nie obejmuje, przyczyna zostaje domyślna (Planowy).

### Skutki uboczne wynikające ze standardu (nie z tej zmiany)

- Limit urlopu na żądanie (4 dni w roku) sprawdza standardowe `SprawdzLimitUrlop` przy
  przekazaniu wniosku do zatwierdzenia — wniosek na dziś przy wyczerpanym limicie zostanie
  zablokowany.
- `WfKierownik`: gdy w konfiguracji (Kadry → Czas pracy → Wnioski urlopowe) wyłączona jest
  opcja „Urlop na żądanie”, wniosek na żądanie **nie trafia do przełożonego** (zadanie bez
  kierownika), a przełożony dostaje tylko wiadomość po realizacji. Automatyczne ustawienie
  przyczyny zmienia więc ścieżkę obiegu dla wniosków na dziś.

## Wdrożenie

1. Konfiguracja → Definicje krotek → „A1_e-wniosek o urlop wypoczynkowy” → edycja kodu.
2. Podmienić sekcję klasy zawartością pliku `A1_e-wniosek o urlop wypoczynkowy`
   (sekcji parametrów nie ruszać), skompilować, zapisać.

Scenariusze testowe: `A1_e-wniosek o urlop wypoczynkowy - scenariusze testowe.xlsx`.
