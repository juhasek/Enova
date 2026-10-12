# Ekwiwalent za urlop — opieka art. 188 na godziny liczona jako dzień przepracowany

## Zgłoszenie

Pracownik ma w planie dzień 12h, na cały ten dzień wpisana jest opieka art. 188.
Przy wyliczaniu ekwiwalentu za urlop system nie traktuje tego dnia jako nieobecności,
tylko liczy go jako dzień przepracowany. Przez to składniki zmienne są dopełniane
za mało albo wcale.

## Gdzie system liczy dni (enova 2512.5.6, Soneta.KadryPlace.dll)

`NaliczanieEkwiwalent` → `NaliczanieUrlop.WyliczCzasyDoDopełnienia` (dla okresów od 2004 r.):

```
norma = KalkPlanu.Norma(miesiąc).Dni
praca = Norma(okres zatrudnienia).Dni − NormaNie(okres).Dni      // konfiguracja bez „wg czasu pracy”
praca = KalkPracy.PracaRozliczana(okres).Dni                     // z „wg czasu pracy” (= norma − NormaNie ± odchyłki)
współczynnik dopełnienia = norma / praca
```

Liczba dni nieobecności (`NormaNie`) to suma `OkresNieobecności.Norma()`:

```csharp
CzasDni czasDni = plan.Norma(okres, def, condition);
Time time = nieobecność?.Norma ?? Time.Empty;
if (time != Time.Empty)
    czasDni = new CzasDni(Time.Min(czasDni.Czas, time), 0);   // <-- dni = 0
```

**Nieobecność wpisana na godziny (wypełnione pole Czas / `Nieobecnosc.Norma`) ma zawsze
0 dni, nawet jeśli obejmuje cały plan dnia (12h z 12h).** Godziny są odejmowane poprawnie,
ale dni nie. W dopełnieniu ekwiwalentu ten dzień zostaje więc w liczbie dni przepracowanych.

Opieka art. 188 rozliczana w godzinach (limit 16h) przy zmianach 12h jest zawsze wpisywana
na godziny, więc problem pojawia się za każdym razem.

Przykład: w miesiącu norma 14 dni × 12h, jeden dzień opieki 12h na godziny.

| | System | Poprawnie |
|---|---|---|
| norma / praca (dni) | 14 / 14 | 14 / 13 |
| współczynnik dopełnienia | 1,0000 | 1,0769 |

Godzinowa podstawa „~godz” składników zmiennych (dzielenie przez przepracowane godziny)
jest liczona dobrze, bo odejmuje 12h. Błąd dotyczy tylko dziennego dopełnienia.

## Jak sprawdzić u klienta

1. Zapis obliczeń elementu ekwiwalentu: wiersz `Norma/praca (dni): X/Y` za miesiąc z opieką.
   Jeśli opieka nie zmniejsza „praca”, potwierdza to diagnozę.
2. Nieobecność opieki: czy ma wypełnione pole Czas (godziny). Jeśli tak, to jest ta przyczyna.
3. Na wszelki wypadek definicja nieobecności „Opieka art. 188”: czy ma włączone „Pomniejsza czas pracy”.
   Bez tej opcji nieobecność nie jest odejmowana w ogóle, ani w dniach, ani w godzinach.

## Możliwe rozwiązania

1. **Dane**: gdy opieka obejmuje cały dzień, wpisywać ją jako całodniową, bez godzin.
   Trzeba sprawdzić, czy limit godzinowy art. 188 przyjmie wtedy 12h z planu (do przetestowania).
2. **Algorytm ekwiwalentu**: enova wywołuje z algorytmu definicji elementu ekwiwalentu
   opcjonalne metody `WartośćNieobecnościCzasDni(WypElement, WyliczCzasDniArgs)`
   (ścieżka bez „wg czasu pracy”) oraz `WartośćPracaCzasDni(...)` (ścieżka „wg czasu pracy”).
   Można w nich policzyć jako pełny dzień każdą nieobecność godzinową, która pokrywa cały
   plan dnia, i poprawić `args.Dni`. Wymaga to definicji ekwiwalentu z algorytmem w kodzie,
   a `PobierzMetode` musi tę metodę znaleźć. Niesprawdzone na żywo.
3. Zgłoszenie do producenta: dni nieobecności godzinowej obejmującej cały plan dnia.
