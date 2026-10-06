# Nadgodziny 100 silnik – dokumentacja biznesowa

Część zestawu „Nadgodziny … silnik”. Pełny opis – po co ten zestaw, jak dzieli pracę między
silnik i cechę, od jakiej konfiguracji zależy, jakie są ryzyka i status – znajduje się
w `Cechy/Nadgodziny 50 silnik.md`. Ta cecha korzysta z dokładnie tej samej mechaniki
i różni się tylko ostatnim krokiem.

Zwraca część nadwyżki dobowej przypadającą na tę strefę, która ma być płatna 100%:

- nadwyżka powyżej limitu `Kalendarz.Nadgodziny.Nadgodz50` – tylko przy włączonej
  konfiguracji „Dobowe 100” (w bazie Claude wyłączona, więc ta część wynosi 0),
- godziny nocne przeniesione z 50% – przy włączonej konfiguracji „Nocne 100” (w bazie Claude
  włączona), wg okna nocnego wyznaczonego przez `KalkulatorPracy.NocOkres`.

W dniu niedzielno-świątecznym zwraca 0 – takie dni rozlicza `Nadgodziny NSW silnik`.
