# Zestawienie na posiedzenie Komisji Socjalnej

Raport DevExpress (`ZestawienieKomisjaSocjalna.repx`) odtwarzający układ
istniejącego zestawienia papierowego przygotowywanego na posiedzenia Komisji
Socjalnej (zapomogi z ZFŚS).

## Status

Etap 1 (ten commit): **układ kolumn**. Komórki w paśmie `Detail` są związane
przez `ExpressionBindings` z polami placeholderowymi (`[Lp]`, `[NrEwid]`,
itd.) — źródło danych (`BusinessSource`) ma na razie `DataKind="Empty"`,
czyli raport nie pobiera jeszcze żadnych danych. Logika wypełniania danych
(krotka/tupla, grupowanie master-detail, wygaszanie powtórzonych kolumn
pracownika w kolejnych wierszach wypłat) zostanie dodana w kolejnym etapie.

## Lista kolumn

Poziom pracownika (jedna wartość na pracownika, docelowo pokazywana tylko
w pierwszym wierszu grupy):

| # | Kolumna (nagłówek) | Pole | Uwagi |
|---|---|---|---|
| 1 | Lp. | `Lp` | numer porządkowy |
| 2 | Nr ewid. | `NrEwid` | numer ewidencyjny pracownika/emeryta |
| 3 | Nazwisko i Imię | `NazwiskoImie` | |
| 4 | Data urodzenia | `DataUrodzenia` | |
| 5 | Jednostka obsługująca | `JednostkaObslugujaca` | dział/oddział |
| 6 | Dochód na członka rodziny | `DochodNaCzlonkaRodziny` | |
| 7 | Kwota zapomogi z 2 lat | `KwotaZapomogiZ2Lat` | suma kwot wypłat z ostatnich 2 lat (kol. 9 wszystkich wierszy grupy) |

Poziom wypłaty — dwie równoległe listy wyrównane wierszami, po jednym
wierszu detail na wypłatę (dłuższa z list wyznacza liczbę wierszy grupy,
krótsza ma puste komórki w nadmiarowych wierszach):

| # | Kolumna (nagłówek) | Pole | Uwagi |
|---|---|---|---|
| 8 | Data | `DataWyplaty` | data wypłaty zapomogi z bieżącego okna 2 lat |
| 9 | Kwota | `KwotaWyplaty` | kwota tej wypłaty (bieżące 2 lata) |
| 10 | Kwota zapomóg z poprzednich 2 lat | `KwotaZapomogPoprzednich2Lat` | suma kwot wypłat z poprzednich 2 lat (kol. 12 wszystkich wierszy grupy) |
| 11 | Data | `DataWyplatyPoprzedniej` | data wypłaty z poprzednich 2 lat |
| 12 | Kwota | `KwotaWyplatyPoprzedniej` | kwota tej wypłaty (poprzednie 2 lata) |

Zweryfikowane na przykładowych danych ze screena: dla pracownika nr ewid.
82652 kol. 7 (3 000,00) = suma kol. 9 z obu wierszy detail (1 000,00 +
2 000,00), kol. 10 (2 000,00) = suma kol. 12 z obu wierszy (1 000,00 +
1 000,00).

## Otwarte pytania (do etapu wypełniania danych)

- Źródło danych: która krotka/obiekt biznesowy dostarcza listę
  pracowników/emerytów wraz z historią wypłat zapomóg.
- Definicja okna "2 lata" / "poprzednie 2 lata" względem daty posiedzenia
  (parametr raportu).
- Mechanizm wygaszania powtórzonych kolumn 1–7 w kolejnych wierszach
  wypłat tego samego pracownika (np. porównanie z poprzednim rekordem w
  `BeforePrint`).
- Tytuł raportu — czy ma być parametryzowany datą posiedzenia i czy
  wariant "Emeryci" / "Pracownicy" to jeden raport, czy dwa osobne.
