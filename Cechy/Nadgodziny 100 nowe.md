# Nadgodziny 100 nowe – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy

Cecha siostrzana `Cechy/Nadgodziny 50 nowe` — pełny kontekst, cel biznesowy i szczegóły
algorytmu (norma dobowa, chronologiczne przypisanie nadwyżki do strefy, limit `Nadgodz50`,
godziny nocne) opisuje `Cechy/Nadgodziny 50 nowe.md`. Ten dokument opisuje tylko różnicę.

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`), dla stref „Praca poza normą” / „Praca poza normą awaria”. Liczy tę
część dobowej nadwyżki nad normą, która ma być rozliczona jako **nadgodziny 100%**, czyli
dopełnienie do „Nadgodziny 50 nowe”:

- część nadwyżki **ponad limit** `Kalendarz.Nadgodziny.Nadgodz50` — tylko gdy globalna
  konfiguracja `Config.Nadgodziny.Dobowe100` jest włączona (w bazie Claude: wyłączona, więc
  ten składnik obecnie zawsze wynosi `0`),
- część nadwyżki przypadająca na **godziny nocne** kalendarza (`Kalendarz.Nocne.Od`/`Do`) —
  aktywne w bazie Claude (`Config.Nadgodziny.Nocne100 = true`), okno nocne wyznacza sam
  system (`KalkulatorPracy.NocOkres`).

## 2. Uwaga: to jest realna zmiana w praktyce, nie tylko techniczna symetria

Stara cecha „Nadgodziny 50 test” (i jej domyślny odpowiednik „Nadgodziny 100” w bazie
Claude) **nigdy nie miała napisanego kodu** dla 100% — pole `Nadgodziny 100` w
`FeatureDefs` istniało jako pusty stub (`Algorithm = 0`, brak kodu). Efekt: godziny pracy
poza normą przypadające w nocy były dotąd **w całości liczone jako 50%**, mimo że globalna
konfiguracja bazy (`Nocne 100 = true`) mówi, że godziny nocne nadgodzin mają być rozliczane
jako 100%. „Nadgodziny 100 nowe” domyka tę lukę — nie jest to tylko refaktoryzacja, ale
realne poprawienie wyniku dla zmian nocnych z pracą „poza normą”.

## 3. Status: NIEZWERYFIKOWANE

Patrz `Cechy/Nadgodziny 50 nowe.md` pkt 6 — te samo ograniczenie (brak żywego testu w
edytorze skryptów, niesprawdzone bezpośrednio użycie `FromTimes`/`FromTime` i
`KalkulatorPracy.NocOkres`) dotyczy tej cechy identycznie, bo współdzieli tę samą logikę.
