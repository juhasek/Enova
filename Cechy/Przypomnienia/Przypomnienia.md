# Przypomnienia — wariant bez DLL (cechy + widok listy)

Wersja 2 pulpitu przypomnień bez dodatku DLL: wszystko w bazie. Propozycja z DLL: `Addon/PulpitPrzypomnien/PROPOZYCJA.md`.

## Pliki

| Plik | Co to |
|---|---|
| `Przypomnienia.dbinit.xml` | kod wspólny (Dodatkowy kod do kompilacji **A1Przypomnienia**) + 10 cech kategorii **Przypomnienia** na Pracownicy — import: MCP `import_business_xml` albo `dbmgr importxml` |
| `Przypomnienia.viewform.xml` | widok listy „Przypomnienia” (Kadry i płace → Kadry → Pracownicy) |
| `dodaj-widok.ps1` | zakłada widok w konfiguracji bazy (kopia bazy przed zmianą); `-Baza <nazwa>` |
| `Przypomnienia - dane testowe.xml` | dane testowe bazy Claude (badania, BHP, orzeczenie, aneks) |

## Cechy (kategoria „Przypomnienia”, tabela Pracownicy)

| Cecha | Typ | Źródło |
|---|---|---|
| KoniecUmowy | Data | etat na dziś: koniec umowy (pusta przy czasie nieokreślonym) |
| KoniecUmowyCywilnej | Data | najbliższy koniec trwającej, nieanulowanej umowy cywilnoprawnej |
| BadanieWazneDo | Data | zaplanowane badanie → Termin; wykonane → WazneDo (bez zastąpionych nowszym tej samej definicji) |
| SzkolenieBHPTermin | Data | zaplanowane szkolenie BHP (wpis bez daty wykonania) → Termin |
| UprawnienieWazneDo | Data | TerminWaznosci nieutraconych uprawnień (bez zastąpionych nowszymi) |
| OrzeczenieDo | Data | orzeczenia PFRON i ZUS obowiązujące dziś lub później: koniec ciągłości; bezterminowe → puste |
| AneksCzasowyDo | Data | **ręczna** (bez algorytmu) — aneks czasowy nie ma jednoznacznego oznaczenia w historii |
| NajblizszyTermin | Data | najwcześniejsza z dat powyżej |
| TerminCzego | Tekst | nazwa najbliższego terminu |
| DniDoTerminu | Liczba | NajblizszyTermin − dziś; ujemne = po terminie; **9999 = brak terminu** |
| WOkresie | Liczba | 1 = najbliższy termin do końca okresu z filtra „Okres” listy (zaległe też), 0 = nie; bez okresu 30 dni — filtr widoku |

Nowy termin u klienta: cecha (ręczna albo z algorytmem w kodzie wspólnym) + dopisanie do metody
`Najblizszy` w A1Przypomnienia + kolumna w widoku.

Orzeczenia: kod używa `OrzeczeniaPFRON`/`OrzeczeniaZUS` z enova **2610** — w 2512 tych kolekcji nie ma
(stopień jest na zapisie historii), cecha OrzeczenieDo wymaga tam innej wersji.

## Widok

Kolumny: Kod, Nazwisko i imię, Wydział, Najbliższy termin, Czego dotyczy, Dni + kolumna na każdy termin.
Sortowanie po Kod — sortowanie po cesze z algorytmem enova blokuje komunikatem „kliknij TUTAJ” (posortować można ręcznie kolumną Dni). Kolor wiersza (`Appearance`):
czerwony < 0, pomarańczowy 0–7, żółty 8–30; 9999 wyszarzone. Filtry: standardowe filtry listy Pracownicy; **„Okres” wyznacza okres przypomnień** — domyślny `<UserFilter Value="[Features.WOkresie] = 1"/>` (bez CaptionHtml = włączany przy otwarciu, GridController.InitFilters) zostawia tylko pracowników z terminem do końca okresu, łącznie z zaległymi.
Strona startowa (web/SonetaFrame): Ulubione → „Startowy” = ten widok.

Po założeniu lub zmianie widoku SQL-em serwer nie widzi zmiany do restartu procesów 2610 (cache konfiguracji).

## Test (baza Claude, web 2610, 29.10.2026)

TEST LIVE OK: 10 scenariuszy z danych testowych — wartości cech zgodne, kolory wierszy (czerwony NG-01/NG-02,
pomarańczowy NG-03/NG-04/NG-05, żółty NG-06/LZ-01/LZ-02/LZ-03, bez koloru LZ-04 = 184 dni), 9999 wyszarzone.
Niesprawdzone: klient okienkowy.
