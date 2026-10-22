# A1_Nadgodziny 100% w niedziele (kontrola)

## Cel

Kontrola do Taska „Nadgodziny 100% w niedziele” (`Taski/NadgodzinyNiedziela100`).
Task dopisuje strefę „Nadgodziny 100%” tylko przy zapisie dnia przez operatora,
więc niedziela może zostać bez strefy albo ze złą strefą:

- dzień zapisany przed założeniem Taska,
- zapis bez operatora (usługi w tle, część importów),
- zmiana normy, etatu albo planu po zapisie dnia.

Wtedy silnik po cichu liczy nadwyżkę jak w zwykły dzień (50%/100%).

## Działanie (rodzaj Dzień pracy)

Te same warunki co w Tasku:

1. Dzień wg planu typu Świąteczny bez „Nadgodziny świąteczne” (Niedziela).
2. Dni ze strefami przeniesień, „niewliczanymi/bez dopłaty” albo innymi
   systemowymi strefami nadgodzin — pomijane (rozliczane ręcznie).
3. Nadwyżka = czas pracy (strefy Wchodzi + Zwiększa) − norma dobowa z etatu.
4. Gdy suma stref „Nadgodziny 100%” ≠ nadwyżka:

   > Dla dnia (data) praca ponad normę dobową wynosi (X), a strefa Nadgodziny
   > 100% ma (Y). Zapisz dzień ponownie, żeby Task przeliczył strefę - pracownik

## Podpięcie: Ostrzeżenie, nie Błąd

Enova uruchamia weryfikatory przy zapisie **przed** Taskami
(`Saver.ClientSave` → `Verifiers.VerifyAllErrors`), a potem jeszcze raz po
nich. Jako Błąd kontrola zablokowałaby każdy zapis niedzieli, zanim Task
zdąży dopisać strefę.

## Konfiguracja

- Plik zawiera metodę do klasy `A1WeryfikatoryKalendarza` (Dodatkowy kod do
  kompilacji), jak pozostałe weryfikatory w tym folderze.
- Baza Claude: kod wpisany wprost w definicję, `DefWeryfKalend` ID **1040**,
  podpięty jako Ostrzeżenie (100) do kalendarza „Test nadgodziny nowe” (28).
- Skompilowany lokalnie na DLL serwera 2512.5.6. Niesprawdzony na żywo.
