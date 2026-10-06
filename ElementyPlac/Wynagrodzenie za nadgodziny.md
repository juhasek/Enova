# Wynagrodzenie za nadgodziny (dodatek automatyczny)

Algorytm z edytora C# definicji elementu (`_Param` + `_Wylicz`).

## Stawka za 1h

- **Fizyczni:** `StawkaZaszeregowaniaNorm1h(Element.Okres.To)`, czyli stawka
  z wynagrodzenia zasadniczego.
- **Umysłowi:** jak wyżej, plus suma `WartośćNominalna` elementów oznaczonych
  cechą definicji `WliczajDoWynNadg`, podzielona przez normę kodeksową miesiąca.
- Rodzaj pracownika jest odczytywany z cechy „Rodzaj pracownika” definicji
  stanowiska (słownik, wartość „Umysłowy”). Brak definicji stanowiska kończy
  się wyjątkiem.

## Czas

- Liczony tylko wtedy, gdy w okresie jest praca w strefie „Praca poza normą”.
- Oddział, którego symbol zawiera „SCE”: praca poza normą minus odchyłka minus.
- Pozostałe oddziały: odchyłka plus minus odchyłka minus.

## Norma kodeksowa – poprawka z 2026-10-05

Wcześniej norma była liczona dla `Element.OkresListy`, czyli okresu **listy
płac**. Przy korekcie zeszłego miesiąca (np. sierpnia 2026 na liście
wrześniowej) dawało to normę miesiąca listy (wrzesień: 176h) zamiast normy
miesiąca, którego dotyczy element (sierpień: 160h). Ilość godzin się nie
zmieniała, rosła tylko norma, przez którą dzielona jest stawka z dodatków.

Teraz norma liczona jest za **pełny miesiąc `Element.Okres.To`**. Pełny,
a nie sam `Element.Okres`, bo element może dotyczyć części miesiąca
(zatrudnienie lub zmiana historii w trakcie), a dotychczas zawsze dzielono
przez normę całego miesiąca.

## Korekty za zeszłe miesiące

Po poprawce wszystkie składowe są brane z okresu elementu:

- stawka zaszeregowania: na dzień `Element.Okres.To`,
- etat, wymiar, oddział: z `Element.PracHistoria` (historia na okres elementu),
- dodatki: `Element.Elementy[Element.Okres]` zbiera elementy pracownika za ten
  okres ze **wszystkich** wypłat (nie tylko z bieżącej listy). Pomija elementy
  wystornowane i stornujące, więc przy korekcie liczy się wersja po korekcie,
  bez podwójnego liczenia.
- czas: odchyłki i strefy za `Składnik.Okres`.

Warunki, które muszą być spełnione:

- Elementy z cechą `WliczajDoWynNadg` muszą mieć **niższy**
  `Algorytm.Priorytet` niż ten element. Inaczej `Element.Elementy[...]` ich
  nie zwróci.
- Jeśli dodatek za dany miesiąc zostanie skorygowany na **późniejszej** liście
  niż korekta nadgodzin, nadgodziny nie przeliczą się same. Trzeba je
  skorygować ponownie.
- Cecha „Rodzaj pracownika” jest na definicji stanowiska (nie historyczna),
  więc korekta weźmie aktualną wartość ze słownika stanowiska.
