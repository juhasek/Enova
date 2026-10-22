# Wynagrodzenie za nadgodziny i godziny ponadwymiarowe (dodatek automatyczny)

Jeden element płacący wynagrodzenie (100% stawki) za całą pracę ponad plan.
Dodatki 50%/100% za nadgodziny zostają w osobnych elementach, bez zmian.
Kod zastępuje wersję, w której `Składnik.Czas = zestNad.Razem` (zmiana z 04.09).

## Problem

Pracownik na 1/2 etatu, plan 4h, praca 12h. Od 04.09 element płacił tylko
`NadgodzinyDobaOkres(...).Razem` = 4h (nadgodziny ponad normę dobową 8h).
Godziny 4–8 (ponadwymiarowe, art. 151 § 5 KP) nie były płacone nigdzie,
a przy pensji miesięcznej wynagrodzenie zasadnicze obejmuje tylko plan.

## Stawka za 1h

Bez zmian: `max(najniższe wynagrodzenie / 1h; stawka zaszeregowania / 1h + dodatki z cechą WliczajDoNadgodzin / norma)`.

## Czas

| Pracownik | Czas składnika |
|---|---|
| Pełny etat | nadgodziny wg silnika (`zestNad.Razem`) – jak dotąd |
| Niepełny etat, stawka miesięczna | nadgodziny + godziny ponadwymiarowe = `max(odchyłki Plus − Minus; nadgodziny)` |
| Niepełny etat, stawka godzinowa | tylko nadgodziny (patrz niżej) |

Godziny ponadwymiarowe = `max(0; (odchyłki Plus − Minus) − nadgodziny wg silnika)`.
Dla pełnego etatu część ponadwymiarowa nie jest liczona, żeby nie zmieniać
wyników przy równoważnym czasie pracy i bilansowaniu okresowym.

## Zabezpieczenie przed podwójną wypłatą

Od wyliczonego czasu odejmowany jest czas składników **tej samej definicji**
z **innych wypłat**, których okres nachodzi na okres składnika
(`Pracownik.Elementy[okres]` – bez elementów wystornowanych i stornujących).
Drugie naliczenie za ten sam okres daje więc 0h, a korekta (storno + nowy
element) liczy się od nowa.

Ograniczenie: przy częściowym nachodzeniu okresów odejmowany jest cały czas
wcześniejszego składnika.

## Stawka godzinowa

Przy stawce godzinowej wynagrodzenie zasadnicze liczone jest od
przepracowanych godzin, więc godziny ponadwymiarowe powinny już być w nim
zapłacone. Element dopłaciłby je drugi raz, dlatego ich nie liczy.

**Niezweryfikowane:** trzeba sprawdzić na pracowniku testowym ze stawką
godzinową, ile godzin pokazuje element wynagrodzenia zasadniczego przy
pracy ponad plan:

- tylko plan → godziny ponadwymiarowe nie są płacone; trzeba usunąć
  warunek `stawkaMiesieczna`,
- plan + ponadwymiarowe (bez nadgodzin) → kod jest poprawny,
- cały przepracowany czas, razem z nadgodzinami → dublują się też same
  nadgodziny (problem także w poprzedniej wersji elementu).

## Do sprawdzenia przed użyciem

- Odbiór czasu wolnego za nadgodziny (przeniesienia do magazynu): czy
  odchyłki Plus nie zawierają godzin oddanych wolnym. Silnik nadgodzin je
  pomija, odchyłki – niesprawdzone.
- Nieobecności w okresie: czy nie zwiększają odchyłki Minus.
- Dodatek 50% za godziny ponadwymiarowe przy limicie z umowy (art. 151 § 5
  KP) nie jest liczony – element płaci tylko 100%.
- Scenariusze testowe: 1/2 etatu miesięczna (plan 4h / praca 12h → 8h),
  pełny etat (wynik bez zmian), druga lista za ten sam okres (→ 0h),
  korekta okresu (→ przeliczenie od nowa), stawka godzinowa.
