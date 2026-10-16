// Generator danych testowych "Kalkulator podstawy zasiłku" (baza Claude, enova 2610).
// Uruchomienie: node generuj.js  -> pliki XML w katalogu nadrzędnym (ImportyXML).
// Import: dbmgr importxml Claude "<plik>" (tryb wg rekordów), kolejno test 01..05.
// GUID-y stałe => pliki 01, 02, 04, 05 idempotentne; 03 (zmiany historii) jednorazowy.
'use strict';
const fs = require('fs');
const path = require('path');

const OUT = path.join(__dirname, '..');
const PREFIKS = 'Kalkulator podstawy zasilku - ';

const ZASADNICZE_MIES = '00000000-0007-0002-0002-000000000000';
const PREMIA = 'c9c3dc52-be71-4b8f-b89b-728d83cd4a9a';
const PREMIA_KWARTALNA = 'd5000000-0000-0007-0000-000000000003';
const PREMIA_ROCZNA = 'd5000000-0000-0007-0000-000000000012';
const ZWOLNIENIE_CHOROBOWE = '00000000-0006-0005-0003-000000000000';

// Scenariusze (kod = numer scenariusza; data niezdolności do kalkulatora w opisie .xlsx).
const pracownicy = [
  { nr: 1,  nazwisko: 'Pełny',      imie: 'Roman',     plec: 'M', ur: '1985-04-12', od: '2024-01-01', stawka: 6000, premia: 500 },
  { nr: 2,  nazwisko: 'Krótki',     imie: 'Ewa',       plec: 'K', ur: '1990-02-03', od: '2026-05-01', stawka: 7000 },
  { nr: 3,  nazwisko: 'Nowicjusz',  imie: 'Adam',      plec: 'M', ur: '1995-07-21', od: '2026-10-15', stawka: 6500 },
  { nr: 4,  nazwisko: 'Uzupełniony',imie: 'Beata',     plec: 'K', ur: '1987-11-30', od: '2024-01-01', stawka: 6000, premia: 500,
    nieobecnosci: [['2026-06-08', '2026-06-12']] },
  { nr: 5,  nazwisko: 'Wyłączony',  imie: 'Cezary',    plec: 'M', ur: '1983-09-09', od: '2024-01-01', stawka: 6000,
    nieobecnosci: [['2026-03-02', '2026-03-27']] },
  { nr: 6,  nazwisko: 'Premiowy',   imie: 'Dorota',    plec: 'K', ur: '1980-01-15', od: '2024-01-01', stawka: 6000,
    kwartalna: 3000, roczna: 12000 },
  { nr: 7,  nazwisko: 'Ciągły',     imie: 'Edward',    plec: 'M', ur: '1979-06-06', od: '2024-01-01', stawka: 6000,
    nieobecnosci: [['2026-09-14', '2026-09-18']], zmiana: { od: '2026-10-01', stawka: 7000 } },
  { nr: 8,  nazwisko: 'Przerwany',  imie: 'Felicja',   plec: 'K', ur: '1984-03-03', od: '2024-01-01', stawka: 6000,
    nieobecnosci: [['2026-06-15', '2026-06-19']], zmiana: { od: '2026-08-01', stawka: 7000 } },
  { nr: 9,  nazwisko: 'Półetatowy', imie: 'Grzegorz',  plec: 'M', ur: '1982-08-18', od: '2024-01-01', stawka: 8000,
    zmiana: { od: '2026-08-01', stawka: 4000, wymiar: '1/2' } },
  { nr: 10, nazwisko: 'Minimalny',  imie: 'Halina',    plec: 'K', ur: '1992-12-01', od: '2024-01-01', stawka: 3500 },
  { nr: 11, nazwisko: 'Limitowy',   imie: 'Igor',      plec: 'M', ur: '1975-10-10', od: '2024-01-01', stawka: 30000 },
  { nr: 12, nazwisko: 'Starszy',    imie: 'Jadwiga',   plec: 'K', ur: '1970-05-10', od: '2024-01-01', stawka: 6000 },
];

const nn = n => String(n).padStart(2, '0');
const kod = p => 'ZP-' + nn(p.nr);
const hex = n => n.toString(16).padStart(2, '0');
const guidPrac = p => `d5000000-0000-0000-0000-0000000000${hex(p.nr)}`;
const guidHist = p => `d5000000-0000-0001-0000-0000000000${hex(p.nr)}`;
const kwota = n => n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' PLN';

// PESEL z poprawną sumą kontrolną (dane fikcyjne).
function pesel(ur, plec, nr) {
  const [r, m, d] = ur.split('-').map(Number);
  const mm = r >= 2000 ? m + 20 : m;
  const seria = String(100 + nr * 7).padStart(3, '0');
  const pl = plec === 'K' ? 2 * (nr % 5) : 2 * (nr % 5) + 1;
  const cyfry = (nn(r % 100) + nn(mm) + nn(d) + seria + pl).split('').map(Number);
  const wagi = [1, 3, 7, 9, 1, 3, 7, 9, 1, 3];
  const suma = cyfry.reduce((s, c, i) => s + c * wagi[i], 0);
  return cyfry.join('') + ((10 - (suma % 10)) % 10);
}

function ubezpieczenia(od) {
  const u = t => `<${t}><Typ>Obowiazkowe</Typ><Od>${od}</Od><Do>(max)</Do><Przyczyna>NieDotyczy</Przyczyna>${t === 'Zdrowotne' ? '<Skladka>0.00</Skladka>' : ''}</${t}>`;
  return ['Emerytalne', 'Rentowe', 'Chorobowe', 'Wypadkowe', 'Zdrowotne'].map(u).join('\n            ');
}

function pracownikXml(p, i) {
  const ps = pesel(p.ur, p.plec, p.nr);
  return `
  <Pracownik id="P${i}" guid="${guidPrac(p)}" class="Soneta.Kadry.PracownikFirmy,Soneta.KadryPlace">
    <Wydzial />
    <Wlasciciel />
    <Typ>Pracownik</Typ>
    <Kod>${kod(p)}</Kod>
    <Nazwisko>${p.nazwisko}</Nazwisko>
    <Imie>${p.imie}</Imie>
    <PESEL>${ps}</PESEL>
    <Wieloetatowosc>NieDotyczy</Wieloetatowosc>
    <Net><WidocznyWNet>False</WidocznyWNet></Net>
    <Historia addnew="true">
      <PracHistoria id="PH${i}" guid="${guidHist(p)}">
        <Pracownik>P${i}</Pracownik>
        <Aktualnosc>(wszystko)</Aktualnosc>
        <Nazwisko>${p.nazwisko}</Nazwisko>
        <Imie>${p.imie}</Imie>
        <Urodzony><Data>${p.ur}</Data><Miejsce>Warszawa</Miejsce></Urodzony>
        <PESEL>${ps}</PESEL>
        <IdentyfikatorPodatkowy>PESEL</IdentyfikatorPodatkowy>
        <ImieOjca>Jan</ImieOjca>
        <ImieMatki>Maria</ImieMatki>
        <Plec>${p.plec}</Plec>
        <Podatki>
          <TypProgow>Standardowe</TypProgow>
          <KosztyRodzaj>JedenStosPracy</KosztyRodzaj>
          <KosztyMnoznik>1.00</KosztyMnoznik>
          <UlgaMnoznik>1.00</UlgaMnoznik>
          <UlgaCzesc>Ulga112</UlgaCzesc>
          <NaPITAdresZameldowania>False</NaPITAdresZameldowania>
        </Podatki>
        <StPokrewienstwa>Brak</StPokrewienstwa>
        <KasaChorych><Kod>07R</Kod><OdDnia>${p.od}</OdDnia></KasaChorych>
        <OddzialNFZ><KodGminy /></OddzialNFZ>
        <PodstawaFP><Naliczaj>Warunkowo</Naliczaj></PodstawaFP>
        <PodstawaFGSP><Naliczaj>Zawsze</Naliczaj></PodstawaFGSP>
        <Etat>
          <Wydzial>00000000-0005-0001-0001-000000000000</Wydzial>
          <DataZawarcia>${p.od}</DataZawarcia>
          <DataRozpPracy>${p.od}</DataRozpPracy>
          <Okres>${p.od}...</Okres>
          <Podstawa>UmowyOPrace</Podstawa>
          <TypUmowy>NaCzasNieokreślony</TypUmowy>
          <RodzajZatrudnienia>NieDotyczy</RodzajZatrudnienia>
          <Stanowisko>Specjalista</Stanowisko>
          <Funkcja>Specjalista</Funkcja>
          <Kalendarz>00000000-0006-0003-0001-000000000000</Kalendarz>
          <InterpretacjaKalendarza>WgPlanu</InterpretacjaKalendarza>
          <InterpretacjaKalendarzaPozaOkrZatr>Domyślnie</InterpretacjaKalendarzaPozaOkrZatr>
          <TypStawki>Dowolna</TypStawki>
          <Zaszeregowanie>
            <Wymiar>1/1</Wymiar>
            <Stawka>${kwota(p.stawka)}</Stawka>
            <RodzajStawki>Miesieczna</RodzajStawki>
            <Element>${ZASADNICZE_MIES}</Element>
          </Zaszeregowanie>
          <Ubezpieczenia>
            <Tyub4>00000000-0005-0002-0110-000000000000</Tyub4>
            <ObowiazkoweOd>${p.od}</ObowiazkoweOd>
            ${ubezpieczenia(p.od)}
          </Ubezpieczenia>
          <PracownikZwolniony>False</PracownikZwolniony>
        </Etat>
        <Nieobecnosci><OkresZasilkowy>Standardowy_6_9</OkresZasilkowy></Nieobecnosci>
        <GUS>
          <GlowneMiejscePracy>True</GlowneMiejscePracy>
          <PopMiejsceZatrudnienia>NieDotyczy</PopMiejsceZatrudnienia>
          <KodWyksztalcenia>Wyższe</KodWyksztalcenia>
        </GUS>
        <Dodatkowe_historii>
          <PracHistoria2 id="PH${i}_2">
            <Host>PH${i}</Host>
            <Dokument><Rodzaj>Niezdefiniowany</Rodzaj></Dokument>
            <Kontakt><EMAIL /><TelefonKomorkowy /></Kontakt>
            <Powiadomic />
            <Wojsko><Stosunek>0</Stosunek></Wojsko>
            <Obywatelstwo>
              <Nazwa>polskie</Nazwa>
              <PodstZdrowCudzoziemca>Brak</PodstZdrowCudzoziemca>
              <RodzajNumeruPodatnika>Nieokreślony</RodzajNumeruPodatnika>
            </Obywatelstwo>
            <StanRodzinny><StanCywilny>nieokreślony</StanCywilny></StanRodzinny>
            <Wyksztalcenie><Kod>NieDotyczy</Kod></Wyksztalcenie>
            <PFRON>
              <MiejsceZamieszkania>0</MiejsceZamieszkania>
              <PoziomWyksztalcenia>BezWykształcenia</PoziomWyksztalcenia>
              <TypSchorzenia>NieDotyczy</TypSchorzenia>
              <TypSchorzeniaSOD>Niezdefiniowany</TypSchorzeniaSOD>
              <Stopien>Brak</Stopien>
              <Okres>(pusty)</Okres>
              <PomocPubliczna>WgKonfiguracji</PomocPubliczna>
            </PFRON>
            <PIT40><Naliczaj>False</Naliczaj></PIT40>
          </PracHistoria2>
        </Dodatkowe_historii>
        <Adresy>
          <AdresExt id="PH${i}_AZAM">
            <Host>PH${i}</Host><Typ>Zameldowania</Typ>
            <Adres>
              <KodPocztowy>0</KodPocztowy>
              <Miejscowosc>Warszawa</Miejscowosc>
              <Wojewodztwo>mazowieckie</Wojewodztwo>
              <Kraj>Polska</Kraj>
              <Ulica>Testowa</Ulica>
              <NrDomu>${p.nr}</NrDomu>
            </Adres>
          </AdresExt>
          <AdresExt id="PH${i}_AZAMIESZK">
            <Host>PH${i}</Host><Typ>Zamieszkania</Typ>
            <Adres><KodPocztowy>0</KodPocztowy><Wojewodztwo>nieokreślone</Wojewodztwo></Adres>
          </AdresExt>
          <AdresExt id="PH${i}_AKOR">
            <Host>PH${i}</Host><Typ>Korespondencyjny</Typ>
            <Adres><KodPocztowy>0</KodPocztowy><Wojewodztwo>nieokreślone</Wojewodztwo></Adres>
          </AdresExt>
        </Adresy>
      </PracHistoria>
    </Historia>
  </Pracownik>`;
}

// Definicja premii okresowej: kopia ustawień standardowej "Premii" (DefElementow ID 29) z okresem co N miesięcy,
// bez proporcjonalnego pomniejszania. Wliczana do podstawy zasiłków ("Wliczać dla" wszystkich świadczeń).
function premiaXml(guid, nazwa, skrot, miesiecy, kolejnosc) {
  const zasilki = `<Typ>WliczaćDla</Typ><Chorobowych>True</Chorobowych><Macierzynskich>True</Macierzynskich><Rehabilitacyjnych>True</Rehabilitacyjnych><Opiekunczych>True</Opiekunczych>`;
  return `
  <DefinicjaElementu guid="${guid}">
    <Nazwa>${nazwa}</Nazwa>
    <Skrot>${skrot}</Skrot>
    <Blokada>False</Blokada>
    <DoDnia>2099-12-31</DoDnia>
    <Kolejnosc>${kolejnosc}</Kolejnosc>
    <Zatrudnienie>Etat</Zatrudnienie>
    <RodzajZrodla>Dodatek</RodzajZrodla>
    <DefinicjaListyPlac>00000000-0007-0001-0001-000000000000</DefinicjaListyPlac>
    <MinimalneWynagrodzenie>True</MinimalneWynagrodzenie>
    <DoWyplaty>True</DoWyplaty>
    <OkresNaliczania>
      <Naliczanie>PłatnaZDołu</Naliczanie>
      <Opoznienie>0</Opoznienie>
      <Typ>CoNMiesięcy</Typ>
      <Ilosc>${miesiecy}</Ilosc>
    </OkresNaliczania>
    <Algorytm>
      <Typ>KreatorAlgorytmu</Typ>
      <KreatorAlgorytmu>
        <PodstawaTyp>Kwota</PodstawaTyp>
        <Proporcjonalnie>False</Proporcjonalnie>
      </KreatorAlgorytmu>
      <DodPodstawa>Kwota premii</DodPodstawa>
      <ElPodstawa1>Wartość premii</ElPodstawa1>
    </Algorytm>
    <Deklaracje>
      <PozycjaPIT>00000000-0007-0004-0001-000000000000</PozycjaPIT>
      <Koszty><Typ>ZeStosunkuPracy</Typ></Koszty>
      <Zaliczka><Typ>NaliczaćWgSkaliPodatkowej</Typ><PomniejszonaZUS>True</PomniejszonaZUS></Zaliczka>
      <Ulga><Typ>Naliczać</Typ></Ulga>
      <PozycjaRP7>SkładnikiZmienne</PozycjaRP7>
      <Spoleczne><Typ>Naliczać</Typ></Spoleczne>
      <Zdrowotne><Typ>Naliczać</Typ><PomniejszaFIS>PomniejszaDoFIS</PomniejszaFIS><PomniejszoneZUS>True</PomniejszoneZUS></Zdrowotne>
      <PodstawaSkladek><UrlopWychowawczy>WliczaćDla</UrlopWychowawczy></PodstawaSkladek>
    </Deklaracje>
    <Nieobecnosci>
      <Urlop><Typ>Wliczać</Typ></Urlop>
      <Ekwiwalent><Typ>Wliczać</Typ></Ekwiwalent>
      <ZasilkiPracownicy>${zasilki}</ZasilkiPracownicy>
      <ZasilkiInni>${zasilki}</ZasilkiInni>
    </Nieobecnosci>
    <Zaokraglenie><Precyzja>DoPełnegoGrosza</Precyzja></Zaokraglenie>
    <Dodatkowe><DodatekDoEtatu>True</DodatekDoEtatu></Dodatkowe>
  </DefinicjaElementu>`;
}

function dodatekXml(p, nr, nazwa, element, okres, podstawa) {
  const g = `d5000000-0000-0004-00${hex(nr)}-0000000000${hex(p.nr)}`;
  const gh = `d5000000-0000-0005-00${hex(nr)}-0000000000${hex(p.nr)}`;
  return `
    <Dodatki addnew="true">
      <Dodatek guid="${g}">
        <Nazwa>${nazwa}</Nazwa>
        <Historia addnew="true">
          <DodHistoria guid="${gh}">
            <Aktualnosc>(wszystko)</Aktualnosc>
            <Element>${element}</Element>
            <Okres>${okres}</Okres>
            <Podstawa>${kwota(podstawa)}</Podstawa>
            <Ulamek>1/1</Ulamek>
          </DodHistoria>
        </Historia>
      </Dodatek>
    </Dodatki>`;
}

const naglowek = opis => `<?xml version="1.0" encoding="utf-8" ?>
<!--
  ${opis}
  Wygenerowane: ImportyXML/Generator kalkulatora podstawy zasilku/generuj.js (nie edytować ręcznie).
-->
<session xmlns="http://www.soneta.pl/schema/business" fromto="(wszystko)">`;

function zapisz(nazwa, tresc) {
  fs.writeFileSync(path.join(OUT, PREFIKS + nazwa), tresc + '\n</session>\n', 'utf8');
  console.log('zapisano: ' + PREFIKS + nazwa);
}

// 01 definicje premii okresowych
zapisz('test 01 definicje premii.xml',
  naglowek('Definicje premii okresowych do scenariusza ZP-06 (premia kwartalna co 3 mies., roczna co 12 mies.).\n  Import wg rekordów, stałe GUID-y - idempotentne.')
  + premiaXml(PREMIA_KWARTALNA, 'Premia kwartalna (test ZP)', 'Prem.kw.ZP', 3, 51)
  + premiaXml(PREMIA_ROCZNA, 'Premia roczna (test ZP)', 'Prem.rocz.ZP', 12, 52));

// 02 pracownicy
zapisz('test 02 pracownicy.xml',
  naglowek('12 pracowników ZP-01..ZP-12 pod scenariusze kalkulatora podstawy zasiłku (baza Claude).\n  Struktura wg ImportyXML/Dodatek roczny - test 01 pracownicy.xml. Import wg rekordów - idempotentne.')
  + pracownicy.map((p, i) => pracownikXml(p, i + 1)).join('\n'));

// 03 zmiany w historii (podwyżki, zmiana etatu) - jednorazowy
zapisz('test 03 zmiany historii.xml',
  naglowek('Aktualizacje historyczne kartotek (ZP-07, ZP-08 podwyżka; ZP-09 zmiana etatu 1/1 -> 1/2).\n  <PracHistoria date=...> tnie okres. Plik JEDNORAZOWY (ponowny import: DateDuplicateException).')
  + pracownicy.filter(p => p.zmiana).map(p => `
  <Pracownik where="Kod=${kod(p)}">
    <Historia addnew="true">
      <PracHistoria date="${p.zmiana.od}">
        <Etat><Zaszeregowanie>${p.zmiana.wymiar ? `<Wymiar>${p.zmiana.wymiar}</Wymiar>` : ''}<Stawka>${kwota(p.zmiana.stawka)}</Stawka></Zaszeregowanie></Etat>
      </PracHistoria>
    </Historia>
  </Pracownik>`).join('\n'));

// 04 dodatki (premie)
zapisz('test 04 dodatki.xml',
  naglowek('Premie w kartotekach: ZP-01, ZP-04 premia miesięczna 500 (standardowa "Premia");\n  ZP-06 premia kwartalna 3000 i roczna 12000 (definicje z pliku test 01). Import wg rekordów - idempotentne.')
  + pracownicy.filter(p => p.premia || p.kwartalna).map(p => `
  <Pracownik where="Kod=${kod(p)}">`
    + (p.premia ? dodatekXml(p, 1, 'Premia', PREMIA, '2025-11-01...2026-10-31', p.premia) : '')
    + (p.kwartalna ? dodatekXml(p, 2, 'Premia kwartalna (test ZP)', PREMIA_KWARTALNA, '2025-01-01...2026-12-31', p.kwartalna) : '')
    + (p.roczna ? dodatekXml(p, 3, 'Premia roczna (test ZP)', PREMIA_ROCZNA, '2025-01-01...2026-12-31', p.roczna) : '')
    + `
  </Pracownik>`).join('\n'));

// 05 nieobecności (zwolnienia chorobowe w okresie podstawy)
zapisz('test 05 nieobecnosci.xml',
  naglowek('Zwolnienia chorobowe w okresie podstawy: ZP-04 (5 dni w 06/2026, uzupełnienie), ZP-05 (26 dni w 03/2026,\n  miesiąc poniżej połowy), ZP-07 (09/2026, przerwa < 3 mies.), ZP-08 (06/2026, przerwa >= 3 mies.).\n  Import wg rekordów, stałe GUID-y - idempotentne. Muszą być w bazie PRZED naliczeniem list za te miesiące.')
  + pracownicy.filter(p => p.nieobecnosci).map(p => `
  <Pracownik where="Kod=${kod(p)}">
    <Nieobecnosci addnew="true">` + p.nieobecnosci.map((n, j) => `
      <Nieobecnosc class="Soneta.Kalend.NieobecnośćPracownika,Soneta.KadryPlace" guid="d5000000-0000-0006-00${hex(j + 1)}-0000000000${hex(p.nr)}">
        <TypZrodla>Pracownik</TypZrodla>
        <Definicja>${ZWOLNIENIE_CHOROBOWE}</Definicja>
        <Okres>${n[0]}...${n[1]}</Okres>
        <Zwolnienie>
          <Przyczyna>ZwolnienieLekarskie</Przyczyna>
          <KodChoroby>J06</KodChoroby>
        </Zwolnienie>
      </Nieobecnosc>`).join('') + `
    </Nieobecnosci>
  </Pracownik>`).join('\n'));

module.exports = { pracownicy };
