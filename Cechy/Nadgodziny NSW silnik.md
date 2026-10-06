# Nadgodziny NSW silnik – dokumentacja biznesowa

Część zestawu „Nadgodziny … silnik”. Pełny opis – po co ten zestaw, jak dzieli pracę między
silnik i cechę, od jakiej konfiguracji zależy, jakie są ryzyka i status – znajduje się
w `Cechy/Nadgodziny 50 silnik.md`. Ta cecha korzysta z dokładnie tej samej mechaniki
i różni się tylko ostatnim krokiem.

Liczy wyłącznie w dniach niedzielno-świątecznych, w których **cała** nadwyżka dobowa jest
nadgodziną świąteczną (`ZestawienieNadgodzin.NSW`). Kwalifikacja dnia dokładnie jak
w silniku: przy wyłączonej konfiguracji „Dobowe 100” (tak jest w bazie Claude) decyduje
`DefinicjaDnia.NadgodzinySW`, przy włączonej – `DefinicjaDnia.Typ != Pracy`.

W pozostałych dniach zwraca 0 – rozliczają je `Nadgodziny 50 silnik`,
`Nadgodziny 100 silnik` i `Nadgodziny okresowe silnik`.
