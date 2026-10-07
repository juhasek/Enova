# A1_Praca w normie nie ponad plan

## Cel

Przy ręcznym uzupełnianiu czasu pracy przełożony nie może wpisać w strefie **Praca w normie**
więcej godzin, niż wynika z planu pracy na ten dzień. Nadwyżkę musi rozpisać na strefy
**Praca poza normą** i **Nadgodziny do przeniesienia**.

## Działanie

Kontrola uruchamia się przy zapisie dnia pracy (rodzaj definicji **Dzień pracy**). Enova
uruchamia ją także po dodaniu, zmianie lub usunięciu strefy, bo każda zmiana strefy przelicza dzień.

1. Sumuje czas wszystkich stref **Praca w normie** w dniu. Strefa rozpoznawana jest po stałym
   identyfikatorze definicji enova (`DefinicjaStrefy.Pracy`), więc zmiana nazwy strefy jej nie psuje.
2. Gdy w dniu nie ma Pracy w normie — brak uwag.
3. Porównuje z planem dnia (`KalkulatorPlanu`). Dzień wolny w planie = plan 0:00, czyli każda
   Praca w normie w taki dzień zostanie zgłoszona.
4. Gdy Praca w normie > plan, zwraca:

   > Dla dnia (data) w strefie Praca w normie wpisano (X), a plan przewiduje (Y). Nadwyżkę (X−Y)
   > wpisz jako Praca poza normą i Nadgodziny do przeniesienia - pracownik

Podpięta jako **Błąd** — zapis dnia jest blokowany, dopóki nadwyżka nie zostanie przeniesiona.

## Konfiguracja w bazie testowej

- Kod metody: Dodatkowy kod do kompilacji `A1WeryfikatoryKalendarza` (CodeFiles ID 13).
- Definicja: `DefWeryfKalend` ID **2049** „A1_Praca w normie nie ponad plan”, rodzaj Dzień pracy (50).
- Podpięcie: kalendarze „3 miesięczny” (45) i „Podstawowy system_1_8:16” (47), Typ = Błąd.
- Wgrane SQL-em — widoczne w programie dopiero po restarcie serwera enova.

## Czego kontrola nie robi / do decyzji

- Nie sprawdza, czy nadwyżka faktycznie została wpisana jako Praca poza normą i Nadgodziny do
  przeniesienia — tylko blokuje nadmiar w Pracy w normie.
- Nie obejmuje dokumentów aktualizacji czasu pracy (rodzaj **Dzień pracy aktualizacja**). Jeśli czas
  pracy wpisuje się także przez wersjonowanie kalendarzy, potrzebna osobna definicja tego rodzaju.
- Limitem jest plan dnia. Odbiór nadgodzin obniżający normę dnia nie jest uwzględniany — kontrola
  pilnuje tylko górnej granicy.
- Kontrola działa na każdym zapisie dnia, także z importu i z RCP. Importy, które dopisują Pracę w normie
  z planu, nie przekraczają planu, więc nie powinny jej uruchamiać.
- Niesprawdzone na żywo.
