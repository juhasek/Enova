# Analiza przedwdrożeniowa – Kadry, Płace, Czas pracy (enova365)

Szablon do prowadzenia spotkania analitycznego z klientem. Nie jest to ankieta
do wysłania mailem – to lista pytań, które konsultant zadaje na warsztacie,
notując odpowiedzi i od razu klasyfikując każdy temat:

| Klasyfikacja | Znaczenie |
|---|---|
| **S** – standard | działa „z pudełka", wystarczy pokazać |
| **K** – konfiguracja | standard, ale wymaga ustawienia (definicje, cechy, kalendarze, prawa) |
| **C** – customizacja | wymaga kodu: element płacowy z algorytmem, weryfikator, raport, worker, dodatek DLL |
| **X** – poza zakresem / do decyzji klienta | proces zostaje poza systemem albo wymaga zmiany po stronie klienta |

Każdy punkt oznaczony **C** musi na koniec analizy mieć: opis funkcjonalny,
wskazanie osoby decyzyjnej po stronie klienta, szacunek pracochłonności
i informację, czy jest krytyczny dla startu (blokuje go), czy może powstać po starcie.

---

## 0. Przygotowanie – dokumenty do zebrania PRZED spotkaniem

Bez tych materiałów analiza zamienia się w zgadywanie. Prosić o nie z góry:

- [ ] Regulamin pracy (rozkłady, okresy rozliczeniowe, pory nocne, dyżury).
- [ ] Regulamin wynagradzania / układ zbiorowy / porozumienia zbiorowe.
- [ ] Regulamin ZFŚS wraz z tabelą progów dochodowych.
- [ ] Regulamin premiowania (jeśli osobny) i przykładowe wyliczenia premii.
- [ ] Wzory umów o pracę, aneksów, świadectw pracy, zaświadczeń (jeśli klient ma własne).
- [ ] 2–3 **przykładowe listy płac** z poprzedniego systemu – najlepiej „trudny" miesiąc
      (nadgodziny, chorobowe, korekta, wypłata po ustaniu zatrudnienia).
- [ ] Przykładowa ewidencja czasu pracy / grafik / plik z RCP (surowy eksport, nie raport).
- [ ] Schemat organizacyjny i lista stanowisk.
- [ ] Lista raportów i wydruków, których faktycznie używają (nie „wszystkich, jakie mamy").
- [ ] Wykaz systemów, z którymi enova ma się wymieniać danymi + przykładowe pliki.
- [ ] Informacja o posiadanej licencji (moduły, wariant Srebro/Złoto/Platyna, liczba pracowników).

> **Uwaga:** wariant licencji rozstrzyga o tym, czy część tematów jest w ogóle
> dostępna (m.in. dodatkowe algorytmy płacowe, rozbudowana ewidencja czasu pracy,
> pulpity, wielooddziałowość). Ustalić to na starcie – inaczej ryzykujemy obiecanie
> funkcji, której klient nie kupił.

---

## 1. Kontekst i zakres wdrożenia

1. Ile podmiotów (baz) obsługujemy? Jedna firma czy grupa kapitałowa?
2. Ile pracowników na etacie, ilu zleceniobiorców, ilu emerytów/byłych pracowników
   nadal obsługiwanych (ZFŚS, PIT)?
3. Jaka jest **data startu produkcyjnego** i od którego miesiąca pierwsza wypłata z enovy?
4. Czy równolegle działa stary system (praca podwójna) i jak długo?
5. Kto w firmie jest właścicielem procesu kadrowego, a kto płacowego? Kto podpisuje odbiór?
6. Czy kadry i płace robi ten sam zespół? Czy płace są outsourcowane?
7. Czego klient oczekuje jako największej zmiany na plus (to jest kryterium sukcesu wdrożenia
   i warto je zapisać dosłownie, jego słowami).

**Na co zwrócić uwagę:** jeśli klient nie umie nazwać kryterium sukcesu, wdrożenie skończy się
sporem o zakres. Zapisać jedno–dwa zdania i wrócić do nich przy odbiorze.

---

## 2. Struktura organizacyjna

1. Jak wygląda struktura: oddziały, wydziały, działy, projekty, MPK?
2. Czy struktura służy tylko do raportowania, czy też rozdziela koszty na konta księgowe?
3. Czy pracownik może być przypisany do więcej niż jednej jednostki jednocześnie
   (np. praca na dwóch wydziałach, rozdzielnik kosztów procentowy)?
4. Czy jest zdefiniowana **struktura podległości** (kto jest przełożonym kogo)
   i czy ma sterować akceptacją wniosków na pulpitach?
5. Jak często struktura się zmienia? Czy potrzebna jest jej historia (stan na dzień)?
6. Czy pracownicy zmieniają oddział/wydział w trakcie miesiąca – i jak wtedy dzielimy koszt?

**Czerwone flagi (→ C):** rozdzielnik kosztów procentowy per pracownik, potrzeba
raportowania „stan struktury na dzień X" wstecz, dwie równoległe hierarchie
(służbowa i kosztowa), podległość inna niż wynikająca z wydziału.

---

## 3. Kadry – kartoteka pracownika i zatrudnienie

1. Jakie dane muszą być na kartotece poza standardem? (numer karty RCP, numer szafki,
   identyfikator z systemu produkcyjnego, dane do ubezpieczenia grupowego)
2. Jakie rodzaje umów: o pracę (na czas określony/nieokreślony/zastępstwo), zlecenie,
   dzieło, B2B, umowa z uczniem, kontrakt menedżerski?
3. Czy występuje zatrudnianie na kilku umowach jednocześnie u tego samego pracodawcy?
4. Kto i kiedy tworzy kartotekę – kadry po podpisaniu umowy, czy wcześniej (rekrutacja)?
5. Jak numerowani są pracownicy? Czy numer/kod ma znaczenie biznesowe (np. zawiera dział)?
6. Czy trzeba pilnować terminów: badania lekarskie, szkolenia BHP, uprawnienia, zezwolenia
   na pracę cudzoziemców, ważność dokumentów? Kto ma być o nich powiadamiany i jak?
7. Jak wygląda proces zakończenia zatrudnienia? Kto wystawia świadectwo pracy,
   kto liczy ekwiwalent, kto odbiera sprzęt?
8. Czy są pracownicy zagraniczni / oddelegowani / z ograniczoną rezydencją podatkową?
9. Czy potrzebne są własne wydruki kadrowe (umowa, aneks, zaświadczenie o zarobkach)
   w layoucie klienta?

**Czerwone flagi (→ C):** własne szablony umów z klauzulami zależnymi od warunków
zatrudnienia, automatyczne powiadamianie mailem o terminach, walidacja „nie zapisz
pracownika bez X" (to weryfikator – czyli kod), obsługa cudzoziemców z pełną kontrolą
ważności zezwoleń.

---

## 4. Czas pracy – plan (kalendarze, normy, rozkłady)

1. Jakie systemy czasu pracy występują? (podstawowy, równoważny, ruchomy, weekendowy,
   praca w ruchu ciągłym, zadaniowy, skrócony tydzień)
2. Jakie są **okresy rozliczeniowe** – 1, 3, 4, 12 miesięcy? Czy różne dla różnych grup?
3. Kto tworzy grafiki i w jakim narzędziu? Excel czy system? Na ile tygodni do przodu?
4. Czy grafik jest zatwierdzany? Przez kogo? Czy po zatwierdzeniu wolno go zmieniać?
5. Ile jest zmian i jak są oznaczane? (I/II/III, nocka, dyżur, „12-godzinna")
6. Czy praca zmianowa przechodzi przez północ – i do której doby przypisujemy godziny?
7. Jak wyliczana jest **norma** dla pracownika (pełny etat, część etatu, zmiana etatu
   w trakcie miesiąca, zatrudnienie/zwolnienie w trakcie okresu rozliczeniowego)?
8. Jak traktowane są święta wypadające w dniu wolnym pracownika?
9. Czy występują strefy czasu pracy inne niż praca podstawowa (szkolenie, delegacja,
   przestój, praca zdalna, dyżur, gotowość)?
10. Czy praca zdalna/hybrydowa jest ewidencjonowana i rozliczana (ryczałt, ekwiwalent)?

**Czerwone flagi (→ C):** własne reguły wyznaczania normy przy zmianie etatu w trakcie
okresu, grafik generowany automatycznie z reguł (obsada minimalna), kontrola
11-godzinnego odpoczynku dobowego i 35-godzinnego tygodniowego, zakaz edycji grafiku
po zamkniętym okresie, import grafików z Excela w formacie klienta.

---

## 5. Czas pracy – rejestracja rzeczywista i RCP

1. Czy jest czytnik RCP? Jaki system, jaki format eksportu (plik CSV/XML, baza, API)?
2. Czy odbicia wchodzą do enovy automatycznie (harmonogram) czy ręcznym importem?
3. Czy pracownik odbija wejście i wyjście, czy tylko wejście? Co przy braku odbicia?
4. Jak traktujemy **wczesne wejście i późne wyjście** – jako nadgodziny, czy tylko
   przekroczenie ponad plan liczy się po akceptacji przełożonego?
5. Czy istnieje tolerancja (np. 5 minut spóźnienia bez konsekwencji)? Zaokrąglenia
   (do 15 minut w górę/dół)?
6. Czy przerwy są odbijane (przerwa, wyjście prywatne, wyjście służbowe)?
7. Kto poprawia błędne odbicia i na jakiej podstawie? Czy zostaje ślad korekty?
8. Czy odbicia mają być widoczne dla pracownika na pulpicie?
9. Jak rozliczamy pracowników nieodbijających (kierownictwo, pracownicy mobilni)?
10. Kiedy zamykamy miesiąc czasu pracy i czy po zamknięciu można coś zmienić
    (blokada wstecz)?

**Czerwone flagi (→ C):** własna logika „odbicie → dzień pracy" (podział na strefy,
tolerancje, zaokrąglenia, odcinanie nadgodzin nieakceptowanych), integracja z RCP,
raport rozbieżności plan/wykonanie, blokada edycji dni wstecz, przypisywanie godzin
do doby poprzedniej przy pracy nocnej.

---

## 6. Nadgodziny, dodatki za czas pracy, dyżury

1. Kto i na jakim etapie **akceptuje nadgodziny**? Czy bez akceptacji nie są płatne?
2. Jak dzielone są nadgodziny na 50% i 100%? Czy stosowane są reguły ustawowe,
   czy własne (np. wszystkie nadgodziny 100%)?
3. Czy nadgodziny dobowe i średniotygodniowe rozliczane są zgodnie z kodeksem,
   czy klient ma tu swoje odstępstwa (korzystniejsze)?
4. Czy pracownik może wybrać **odbiór nadgodzin czasem wolnym** zamiast wypłaty?
   Na wniosek pracownika (1:1) czy polecenie pracodawcy (1:1,5)? Jaki jest termin odbioru?
5. Jak rozliczamy pracę w dniu wolnym (niedziela, święto, dzień harmonogramowo wolny)?
6. Jakie dodatki za warunki pracy? (nocny, szkodliwe, uciążliwe, wysokościowe,
   za pracę w brygadzie, dodatek zmianowy)
7. Czy dodatek nocny to stawka ustawowa, czy stała kwota/procent ustalony u klienta?
8. Jak wyglądają dyżury – domowe, pod telefonem, w zakładzie? Jak płatne?
9. Czy występuje delegacja/podróż służbowa i czy ma wpływ na czas pracy i wynagrodzenie?
10. Czy występują przestoje i jak są płatne?

**Czerwone flagi (→ C):** własny podział nadgodzin na strefy, dodatki liczone od innej
podstawy niż standard, limity nadgodzin z kontrolą przy zapisie, dodatek zmianowy
zależny od liczby przepracowanych zmian w miesiącu, rozliczanie odbiorów nadgodzin
z kontrolą terminu wykorzystania.

---

## 7. Nieobecności i urlopy

1. Jakie rodzaje nieobecności występują poza standardowymi? Czy klient ma własne
   (np. urlop dodatkowy branżowy, wolne za krew, dzień na poszukiwanie pracy)?
2. Jak liczony jest wymiar urlopu – w dniach czy w godzinach?
3. Czy są urlopy dodatkowe (niepełnosprawność, staż, układ zbiorowy)?
4. Jak obsługiwane są urlopy zaległe i ich przedawnienie? Czy system ma pilnować limitu?
5. Jak wygląda ścieżka **wniosku urlopowego**: papier, mail czy pulpit pracownika?
   Ile poziomów akceptacji? Co, gdy przełożony jest nieobecny (zastępstwo)?
6. Czy urlop na żądanie ma odrębny limit i odrębną ścieżkę (np. zgłoszenie tego samego dnia)?
7. Kto wprowadza zwolnienia lekarskie – czy pobierane są z **e-ZLA (ZUS PUE)**?
8. Czy firma jest płatnikiem zasiłków? Kto liczy zasiłki – kadry czy ZUS?
9. Jak obsługiwane są korekty nieobecności (zwolnienie wpłynęło po wypłacie)?
10. Czy potrzebna jest kontrola kolizji nieobecności z grafikiem i z obsadą działu?

**Czerwone flagi (→ C):** własne rodzaje nieobecności z własnym wymiarem, e-wniosek
z niestandardową logiką (np. „dzisiaj = automatycznie na żądanie"), akceptacja
wielopoziomowa z macierzą zastępstw, kontrola obsady minimalnej przy akceptacji urlopu,
limit liczony inaczej niż w standardzie.

---

## 8. Płace – składniki wynagrodzenia

Najważniejszy blok. Przejść **po kolei przez każdą pozycję z przykładowej listy płac**
klienta i dla każdej ustalić: podstawę, sposób liczenia, wpływ na ZUS/PIT, wpływ na
podstawy urlopu/chorobowego/nadgodzin i sposób wykazania na pasku.

1. Jakie formy wynagrodzenia zasadniczego: miesięczne, godzinowe, akordowe, prowizyjne?
2. Jakie premie? Dla każdej: **od czego liczona** (podstawa), za jaki okres,
   kto podaje dane wejściowe, czy jest uznaniowa, czy pomniejszana za nieobecności,
   czy wchodzi do podstawy urlopu i chorobowego.
3. Czy występują dodatki stażowe, funkcyjne, za znajomość języka, za mentoring?
4. Czy są nagrody jubileuszowe? Jaka tabela, jaki staż jest liczony (tylko u tego
   pracodawcy czy cały?), jak ustalana podstawa?
5. Odprawy: emerytalno-rentowa, z tytułu zwolnień grupowych, pośmiertna – jakie tabele,
   jaka podstawa, czy staż liczony ze wszystkich świadectw pracy?
6. Wynagrodzenie za czas niedostępności: jak liczony jest urlop, przestój, opieka?
7. Czy są świadczenia rzeczowe / benefity doliczane do podstawy (samochód, mieszkanie,
   ubezpieczenie, karta sportowa, pakiet medyczny)? Kto dostarcza dane i w jakim formacie?
8. Czy występuje **wyrównanie do minimalnego wynagrodzenia** i jak liczone przy części etatu?
9. Czy są wypłaty niestandardowe: wyrównania wsteczne, podwyżka z datą wsteczną,
   wypłata po ustaniu zatrudnienia, świadczenia po śmierci pracownika?
10. Czy są wypłaty pozalistowe i zaliczki? Jak rozliczane?

**Czerwone flagi (→ C):** każda premia z regulaminu, którą da się opisać wzorem – to
element płacowy z własnym algorytmem. Każdy składnik zależny od danych z innego systemu.
Nagroda jubileuszowa i odprawa emerytalna – prawie zawsze kod, bo tabele i sposób
liczenia stażu są indywidualne.

**Praktyczna wskazówka:** przy każdym nietypowym składniku zapytać, czy w obecnym
systemie działa automatycznie, czy księgowa wpisuje kwotę ręcznie. Jeśli ręcznie –
często najtaniej zostawić to jako wpis ręczny (S/K) niż kodować (C). Decyzja klienta,
ale trzeba mu pokazać różnicę w koszcie.

---

## 9. Płace – proces miesięczny i listy płac

1. Ile jest **list płac w miesiącu** i jakich rodzajów? (etatowa, umowy cywilnoprawne,
   premiowa, korygująca, zasiłkowa, ZFŚS, nagrody)
2. Jakie są terminy: dzień wypłaty, dzień zamknięcia czasu pracy, dzień przekazania
   danych do płac?
3. Czy wynagrodzenie jest wypłacane w miesiącu przepracowanym, czy do 10. następnego?
   (to determinuje rozliczanie nadgodzin i nieobecności „do przodu")
4. Kto dostarcza dane zmienne (premie, godziny, potrącenia) i w jakiej formie?
5. Jak wygląda ścieżka akceptacji listy płac przed przelewem?
6. Jak robione są **korekty** – lista korygująca, storno, rozliczenie w kolejnym miesiącu?
7. Czy potrzebna jest symulacja / plan listy płac (rezerwy, budżet wynagrodzeń)?
8. Jak wygląda przekazanie do banku – plik w jakim formacie, jaki bank, przelewy
   zbiorcze czy indywidualne?
9. Jak wyglądają **paski wypłaty** – papier, mail, pulpit pracownika? Czy zaszyfrowane?
   Czy klient wymaga własnego layoutu?
10. Jak wygląda księgowanie płac – schemat dekretacji, rozdzielnik na MPK/projekty,
    czy księgowość jest w enovie czy w innym systemie?

**Czerwone flagi (→ C):** własny format pliku bankowego, własny layout paska,
paski wysyłane mailem z hasłem, wieloetapowa akceptacja listy płac,
rozdzielnik kosztów wg przepracowanych godzin na projektach.

---

## 10. Potrącenia, PPK, ZFŚS, ubezpieczenia grupowe

1. Jakie potrącenia dobrowolne? (kasa zapomogowa, pożyczki, związki zawodowe, ubezpieczenia)
2. Jak obsługiwane są **zajęcia komornicze** – ile ich jest, czy trzeba pilnować
   kwoty wolnej i kolejności zajęć, czy potrzebna jest korespondencja z komornikiem?
3. Pożyczki pracownicze: z ZFŚS czy z zakładowej kasy? Raty, oprocentowanie, harmonogram,
   zawieszenie spłaty, rozliczenie przy zwolnieniu?
4. PPK: kto jest instytucją finansową, jak wygląda plik do PPK, kto obsługuje rezygnacje
   i automatyczne ponowne zapisy?
5. ZFŚS: jakie świadczenia (wczasy pod gruszą, paczki, zapomogi, dofinansowanie
   wypoczynku dzieci)? Jaki jest **próg dochodowy** i jak liczony dochód na członka rodziny?
6. Czy ZFŚS obsługuje też emerytów i byłych pracowników?
7. Kto zatwierdza wnioski socjalne (komisja socjalna)? Czy potrzebny jest raport dla komisji?
8. Ubezpieczenia grupowe: ile polis, jakie warianty, kto przekazuje listę do ubezpieczyciela?
9. Czy są świadczenia opodatkowane i nieopodatkowane (limity zwolnień) – kto pilnuje limitów?

**Czerwone flagi (→ C):** tabela progów ZFŚS z własnym sposobem liczenia dochodu
na członka rodziny, raport dla komisji socjalnej, plik do ubezpieczyciela,
obsługa zajęć z wieloma tytułami i kwotą wolną liczoną nietypowo.

---

## 11. Deklaracje, sprawozdawczość, rozliczenia zewnętrzne

1. Kto wysyła deklaracje ZUS – enova → Płatnik, czy bezpośrednio e-Płatnik?
2. Jakie deklaracje PIT i kiedy? Czy PIT-11 przekazywany jest pracownikom elektronicznie?
3. Czy firma rozlicza PFRON (Wn-D, INF)? Czy potrzebne są dane o stopniu
   niepełnosprawności i schorzeniach szczególnych?
4. Jakie sprawozdania GUS są robione? (Z-03, Z-05, Z-06, Z-12, DG-1)
5. Czy są raporty dla instytucji zewnętrznych: PIP, urząd pracy, ubezpieczyciel, audyt?
6. Czy występuje raportowanie do grupy kapitałowej / zagranicznej centrali
   (headcount, FTE, koszty w podziale na kategorie)? W jakim formacie i języku?
7. Czy obowiązuje raportowanie jawności wynagrodzeń / luki płacowej?
8. Czy wymagane są zestawienia do budżetowania i planowania kosztów osobowych?

**Czerwone flagi (→ C):** każdy raport do centrali w obcym formacie, raporty
w innej walucie/układzie niż polski, luka płacowa (wymaga uzgodnienia metodologii),
sprawozdania łączone z wielu baz.

---

## 12. Pulpity i samoobsługa pracownicza

1. Czy kupione są Pulpity: Pracownika, Kierownika, Kadrowy (HR)? Ilu użytkowników?
2. Jak pracownik loguje się do pulpitu (własne konto, AD/SSO, telefon)?
   Czy pracownicy produkcyjni mają komputery/telefony?
3. Co pracownik ma widzieć: paski, limity urlopu, czas pracy, dane osobowe,
   zaświadczenia, PIT-11, e-teczka?
4. Które wnioski mają być elektroniczne: urlop, nadgodziny, delegacja, zmiana danych,
   zaświadczenie, ZFŚS, PPK?
5. Jaka jest ścieżka akceptacji każdego wniosku i kto jest zastępcą przy nieobecności?
6. Czy pracownik może sam edytować swoje dane (adres, konto bankowe) – i czy zmiana
   wymaga akceptacji kadr?
7. Czy kierownik ma planować grafik na pulpicie? Czy ma widzieć wynagrodzenia podwładnych?
8. Czy potrzebne są powiadomienia (mail) o wniosku do akceptacji?

**Czerwone flagi (→ C):** własne typy e-wniosków, akceptacja wg macierzy innej niż
struktura podległości, ograniczanie widoczności danych na pulpicie wg cechy pracownika,
własne zakładki na pulpicie.

---

## 13. Migracja danych i bilans otwarcia

1. Z jakiego systemu migrujemy? W jakiej formie da się dane wyeksportować?
2. Co migrujemy, a co zostaje w starym systemie (dostęp „do wglądu")?
3. Zakres minimalny: kartoteki pracowników aktywnych, historia zatrudnienia, etaty,
   dane do ZUS/PIT, **limity urlopowe**, dane do podstaw chorobowego (12 miesięcy),
   nierozliczone pożyczki i zajęcia, staże do jubileuszy i odpraw.
4. Czy migrujemy **historyczne wypłaty**? Ile lat wstecz? Po co (PIT, zaświadczenia,
   podstawy zasiłkowe, Rp-7)?
5. Czy migrujemy historię czasu pracy? Czy wystarczy saldo okresu rozliczeniowego?
6. Czy migrujemy pracowników zwolnionych? Ilu i jak głęboko?
7. Kto odpowiada za jakość danych źródłowych i kto potwierdza poprawność migracji?
8. Jak wygląda **test migracji**: równoległe naliczenie tego samego miesiąca w obu
   systemach i uzgodnienie co do złotówki. Kto to robi i ile iteracji zakładamy?
9. Co z e-teczkami / dokumentacją skanowaną?

**Na co zwrócić uwagę:** migracja jest najczęstszą przyczyną opóźnień wdrożenia.
Zapisać wprost: kto przygotowuje pliki, w jakim terminie, w jakim formacie,
ile rund poprawek jest w cenie. Podstawy chorobowego i limity urlopowe to dwa
miejsca, w których braki wyjdą dopiero przy pierwszej wypłacie.

---

## 14. Raporty i wydruki

1. Które raporty klient realnie drukuje/wysyła co miesiąc? (poprosić o egzemplarze)
2. Które z nich pokrywa standard enovy, a które wymagają własnego wydruku?
3. Jaki format wyjściowy: PDF, XLSX, CSV? Czy plik ma być dalej przetwarzany
   (wtedy potrzebna jest „płaska" tabela, nie ładny wydruk)?
4. Czy wydruki mają mieć logo/pieczątkę/stopkę klienta i dane oddziału?
5. Czy raporty mają być uruchamiane z listy z zaznaczeniem wielu rekordów?
6. Kto ma mieć dostęp do których raportów (raporty płacowe = dane wrażliwe)?
7. Czy potrzebne są zestawienia czasu pracy w układzie „pracownik × dni miesiąca"?
8. Czy dane mają iść do narzędzia BI / hurtowni?

**Czerwone flagi (→ C):** każdy raport w layoucie klienta, eksport do XLSX
z formatowaniem (scalenia, szerokości kolumn, hasło na pliku), zestawienia z własnymi
kolumnami wyliczanymi, raporty łączące dane kadrowe z zewnętrznymi.

---

## 15. Integracje

Dla **każdej** integracji ustalić: kierunek, zakres danych, format, częstotliwość,
mechanizm (plik/API/baza), stronę odpowiedzialną za wykonanie, obsługę błędów
i sposób testowania.

1. RCP / kontrola dostępu.
2. Bank (przelewy wynagrodzeń, zaliczek, PIT/ZUS).
3. Płatnik / e-Deklaracje / e-ZLA.
4. PPK – instytucja finansowa.
5. Systemy HR: rekrutacja, ocena pracownicza, szkolenia, benefity.
6. System produkcyjny / MES / projektowy (godziny na zlecenia i projekty).
7. Księgowość, jeśli inna niż enova.
8. Active Directory / SSO / zakładanie i blokowanie kont pracowniczych.
9. Hurtownia danych / BI / raportowanie do centrali.
10. Kto jest właścicielem technicznym po drugiej stronie i czy jest dostępny w projekcie?

**Na co zwrócić uwagę:** integracja bez wskazanej osoby technicznej po stronie
drugiego systemu jest ryzykiem harmonogramu, nie zakresu. Zapisać kontakt (rolą)
i zażądać przykładowego pliku albo dostępu do API na etapie analizy, nie na etapie testów.
Osobno ustalić, co się dzieje, gdy integracja zawiedzie – kto to zauważy.

---

## 16. Uprawnienia, bezpieczeństwo, RODO

1. Jakie role użytkowników i kto ma widzieć wynagrodzenia (całej firmy / swojego działu)?
2. Czy kierownik widzi dane kadrowe podwładnych? W jakim zakresie?
3. Czy potrzebne jest ograniczenie dostępu do danych wg oddziału/wydziału
   (prawa do danych, nie tylko do funkcji)?
4. Kto ma prawo modyfikować definicje (elementy płacowe, kalendarze) na produkcji?
5. Czy wymagany jest **dziennik zmian / audyt** kto co zmienił i kiedy?
6. Jakie są zasady retencji danych (usuwanie po okresie przechowywania)?
7. Jak realizowane są prawa osób: wgląd, sprostowanie, przeniesienie danych?
8. Jak wygląda polityka backupów i kto za nią odpowiada (klient czy my)?
9. Środowiska: czy jest baza testowa/szkoleniowa i jak często odświeżana z produkcji?
   Czy dane są wtedy anonimizowane?

**Czerwone flagi (→ C):** prawa do danych w układzie macierzowym, audyt zmian poza
standardowym logiem, anonimizacja bazy testowej.

---

## 17. Organizacja projektu

1. Kto po stronie klienta jest kierownikiem projektu i kto decyduje o zakresie?
2. Kto jest **kluczowym użytkownikiem** kadr, płac i czasu pracy (osoby, które będą testować)?
3. Ile czasu klient realnie przeznaczy na testy? (najczęstsza przyczyna poślizgu)
4. Jak wygląda ścieżka zgłaszania uwag i kto je priorytetyzuje?
5. Kiedy i w jakim zakresie potrzebne są szkolenia? Ilu użytkowników, w jakich grupach?
6. Jakie są kamienie milowe i jaki jest warunek odbioru każdego etapu?
7. Jak dokumentujemy ustalenia – kto pisze protokół z warsztatu i kto go akceptuje?
8. Co zostaje na etap 2 (po starcie)? Świadome odłożenie tematów jest lepsze
   niż wciśnięcie wszystkiego przed start.

---

## 18. Podsumowanie analizy – co musi być na wyjściu

Dokument wyjściowy z analizy powinien zawierać:

1. **Rejestr ustaleń** – każda decyzja z datą i osobą decydującą.
2. **Rejestr customizacji (C)** – lista z opisem funkcjonalnym, szacunkiem pracochłonności
   i oznaczeniem, czy blokuje start.
3. **Rejestr otwartych pytań** – z terminem odpowiedzi i właścicielem.
4. **Zakres migracji** – co, skąd, kto przygotowuje, w jakim formacie, do kiedy.
5. **Listę integracji** z osobami kontaktowymi.
6. **Listę rzeczy poza zakresem** – wypisaną wprost, żeby nie wróciła jako
   „przecież o tym mówiliśmy".
7. **Harmonogram** z datami testów i startu produkcyjnego.

---

## 19. Zbiorcza lista sygnałów ostrzegawczych

Zdania, które na warsztacie oznaczają, że temat najprawdopodobniej jest
customizacją – warto reagować na nie od razu, dopytując o szczegół:

| Klient mówi | Co to zwykle znaczy |
|---|---|
| „U nas to działa inaczej niż w kodeksie, ale korzystniej" | własny algorytm płacowy lub reguła czasu pracy |
| „Mamy to w Excelu, wystarczy przenieść" | logika niespisana nigdzie; trzeba ją odtworzyć z formuł |
| „To liczy nasza pani z kadr, ona wie jak" | wiedza nieudokumentowana; ryzyko, że wyjdzie po starcie |
| „Chcemy dokładnie taki sam wydruk jak teraz" | własny raport/layout |
| „System ma pilnować, żeby nie dało się wpisać…" | weryfikator = kod |
| „Chcemy, żeby to się liczyło samo z danych z…" | integracja + logika |
| „Nasz zarząd dostaje takie zestawienie co miesiąc" | własny raport, często w XLSX |
| „Potrzebujemy tego wstecz, za poprzednie lata" | rozszerzony zakres migracji |
| „Kierownik nie może widzieć wynagrodzeń, ale musi widzieć…" | prawa do danych, nie do funkcji |
| „To się zmienia co miesiąc" | parametr w konfiguracji albo cecha, nie wartość zaszyta w kodzie |

---

## 20. Uwagi metodyczne dla konsultanta

- **Prezentuj standard, zanim zapytasz o wymagania.** Klient opisujący proces „jak dziś"
  nieświadomie zamawia customizację. Pokazanie standardowego mechanizmu enovy często
  kończy temat.
- **Nie obiecuj na warsztacie.** Na każde „a czy da się…" odpowiadaj „da się,
  sprawdzę jakim kosztem" – i zapisuj.
- **Dopytuj o wyjątki.** Główny scenariusz zwykle jest prosty; koszt wdrożenia robią
  wyjątki (część etatu, zmiana w trakcie miesiąca, zatrudnienie od 15., zwolnienie
  z wyrównaniem, korekta po wypłacie).
- **Proś o liczby, nie o opisy.** „Ilu pracowników tego dotyczy?" często przesuwa temat
  z C na X – nie warto kodować obsługi trzech przypadków w roku.
- **Każdą regułę licz na przykładzie.** Weź konkretnego pracownika z listy płac klienta
  i policz z nim wynik ręcznie. To najszybszy sposób wykrycia, że regulamin mówi
  co innego niż praktyka.
- **Rozdziel „musi być na start" od „byłoby dobrze".** Zapisz to w dokumencie,
  bo przy poślizgu to będzie pierwsze pytanie.
- **Zapisuj także to, czego nie robimy.** Lista wykluczeń chroni obie strony.
