// Testy ankiety (schema/ankieta.json + src/ankieta.js): node --test Formularz/test
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const W = require('../src/warunki.js');
const A = require('../src/ankieta.js');
const czytaj = (p) => JSON.parse(readFileSync(new URL(p, import.meta.url), 'utf8'));
const schemat = czytaj('../../schema/definicja-elementu.schema.json');
const ankieta = czytaj('../../schema/ankieta.json');

// Wartości wpisywane przez klienta w pytaniach o pola (odpowiedź = zmiana pola, zapamiętana w „reczne”).
const WPISANE = {
  Nazwa: 'Test ankiety', Skrot: 'TA',
  'Algorytm.KreatorAlgorytmu.Podstawa': 250,
  'Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent': 10,
  PodstawyDefinicji: [{ ElementPodstawy: 'Wynagrodzenie zasadnicze mies.', RodzajPodstawy: 'WartoscNominalna' }],
};

/** Definicja po ankiecie – tak jak w formularzu: wynik zastosuj() staje się wartościami definicji. */
function poAnkiecie(odpowiedzi, wpisane = WPISANE) {
  const d = { id: 'a', wartosci: {}, ankieta: { odpowiedzi, reczne: {} } };
  for (const p of A.widocznePytania(ankieta, odpowiedzi))
    for (const s of A.polaPytania(p)) if (s in wpisane) { d.wartosci[s] = wpisane[s]; d.ankieta.reczne[s] = true; }
  const w = A.zastosuj(ankieta, schemat, d);
  d.wartosci = w.wartosci;
  return { d, w };
}

const bledy = (d, licencja) => W.waliduj(schemat, d, [d], { licencja }).filter((b) => b.poziom === 'blad');

test('ankieta zgodna ze schematem (pola, enumy, szablony, warunki, rozdziały)', () => {
  assert.deepEqual(A.sprawdz(ankieta, schemat), []);
});

test('ankieta pyta o każde pole schematu', () => {
  assert.deepEqual(A.niepokryte(ankieta, schemat), []);
});

test('każda ścieżka odpowiedzi daje definicję bez błędów (srebrna, złota, platynowa)', () => {
  // ścieżki: dla każdej odpowiedzi każdego pytania – wariant „pierwsze odpowiedzi + ta jedna”, plus losowe kombinacje
  let ziarno = 12345;
  const los = (n) => { ziarno = (ziarno * 1103515245 + 12345) % 2147483648; return ziarno % n; };
  const wybor = ankieta.pytania.filter((p) => A.typ(p) === 'wybor');
  const opcje = (p) => p.odpowiedzi.map((o) => o.id).concat(p.nieWiem === false ? [] : [A.NIE_WIEM]);
  const sciezki = [];
  for (const r of opcje(wybor[0])) {
    const baza = { rodzaj: r };
    for (const p of wybor.slice(1)) baza[p.id] = p.odpowiedzi[0].id;
    sciezki.push(baza);
    for (const p of wybor.slice(1)) for (const o of opcje(p)) sciezki.push({ ...baza, [p.id]: o });
  }
  for (let i = 0; i < 3000; i++) {
    const o = {};
    for (const p of wybor) { const op = opcje(p); o[p.id] = op[los(op.length)]; }
    sciezki.push(o);
  }
  const problemy = [];
  for (const odp of sciezki) {
    const { d } = poAnkiecie(odp);
    for (const lic of ['srebrna', 'zlota', 'platynowa']) {
      // szablony zasadniczego mają podstawę „Wyrażenie” – niedostępna w srebrnej (ankieta kieruje to do wdrożeniowca)
      if (lic === 'srebrna' && /^zasadnicze/.test(odp.rodzaj)) continue;
      const b = bledy(d, lic);
      if (b.length) problemy.push(`${lic} ${JSON.stringify(odp)}: ${b.map((x) => x.sciezka + ' – ' + x.komunikat).join('; ')}`);
    }
  }
  assert.deepEqual(problemy.slice(0, 5), [], `${problemy.length} błędnych ścieżek`);
});

// Test odwrotny: odpowiedzi „jak klient opisałby składnik standardowy enova” mają odtworzyć jego szablon.
// Dopuszczone różnice są świadome – opis przy każdej.
const WZORCE = [
  { szablon: 'premiaUznaniowa', odp: { rodzaj: 'premia', kogo: 'etat', kwota: 'coMiesiac', czestotliwosc: 'miesiac', zaOkres: 'zDolu', choroba: 'przelozony', urlop: 'zmienny', nieplatne: 'nie', podatki: 'tak', minimalne: 'tak' }, roznice: {} },
  { szablon: 'premiaProcentowa', odp: { rodzaj: 'premia', kogo: 'etat', kwota: 'procent', czestotliwosc: 'miesiac', zaOkres: 'zDolu', choroba: 'proporcjonalnie', urlop: 'staly', nieplatne: 'tak', podatki: 'tak', minimalne: 'tak' },
    roznice: {
      'OkresNaliczania.Typ': 'co miesiąc = co 1 miesiąc (CoNMiesięcy), standard ma „Każdy okres” – działa tak samo',
      'OkresNaliczania.Ilosc': 'j.w.', 'OkresNaliczania.DlaKazdejUmowy': 'j.w. – pole aktywne przy CoNMiesięcy, wartość domyślna',
      GenerujZerowy: 'kosmetyka – szablon premii (premiaUznaniowa) nie pokazuje zerowych', Kolejnosc: 'kolejność na liście płac – ustala wdrożeniowiec',
      'Algorytm.ElPodstawa1': 'nazwa parametru z dwukropkiem', 'Algorytm.ElProcent': 'nazwa parametru z dwukropkiem',
    } },
  { szablon: 'dodatekFunkcyjny', odp: { rodzaj: 'dodatek', kogo: 'etat', kwota: 'indywidualna', czestotliwosc: 'miesiac', zaOkres: 'zDolu', choroba: 'proporcjonalnie', urlop: 'bezZmian', nieplatne: 'tak', podatki: 'tak', minimalne: 'tak' },
    roznice: {
      'Algorytm.KreatorAlgorytmu.Korekta.Chorobowe': 'choroba „pomniejszany” = jak zasadnicze (1/30 przy stawce miesięcznej, art. 92 KP); standard: proporcjonalnie do godzin',
      'Algorytm.KreatorAlgorytmu.Korekta.Macierzynske': 'j.w.', 'Algorytm.KreatorAlgorytmu.Korekta.Rehabilitacyjne': 'j.w.', 'Algorytm.KreatorAlgorytmu.Korekta.Opiekuncze': 'j.w.',
      'Nieobecnosci.ZasilkiPracownicy.Typ': 'składnik pomniejszany za chorobę – do podstawy zasiłku z uzupełnieniem (art. 42 ustawy zasiłkowej); standard: w kwocie wypłaconej',
      'Nieobecnosci.ZasilkiInni.Typ': 'j.w.',
      'Algorytm.DodPodstawa': 'nazwa parametru wg rodzaju („Kwota dodatku”)',
    } },
  { szablon: 'dodatekStazowy', odp: { rodzaj: 'dodatekStazowy', choroba: 'zachowany', urlop: 'bezZmian', nieplatne: 'tak', minimalne: 'tak' }, roznice: {} },
  { szablon: 'potracenie', odp: { rodzaj: 'potracenie', kogo: 'obie', kwota: 'indywidualna', czestotliwosc: 'miesiac' }, roznice: { 'Algorytm.DodPodstawa': 'nazwa parametru wg rodzaju („Kwota potrącenia”)' } },
  { szablon: 'zasadnicze', odp: { rodzaj: 'zasadnicze' }, roznice: {} },
  { szablon: 'zasadniczeGodz', odp: { rodzaj: 'zasadniczeGodz' }, roznice: {} },
];

test('test odwrotny: odpowiedzi odtwarzają szablony standardowe (poza opisanymi różnicami)', () => {
  for (const wz of WZORCE) {
    const t = schemat.szablony.find((s) => s.id === wz.szablon);
    const wpisane = { ...WPISANE };
    for (const s of Object.keys(wpisane)) if (t.wartosci[s] !== undefined) wpisane[s] = t.wartosci[s];
    const { d, w } = poAnkiecie(wz.odp, wpisane);
    assert.deepEqual(w.brak, [], `${wz.szablon}: pytania bez odpowiedzi`);
    const dt = { wartosci: { ...t.wartosci, Nazwa: wpisane.Nazwa, Skrot: wpisane.Skrot } };
    const eA = W.doEksportu(schemat, d, { licencja: 'zlota' });
    const eT = W.doEksportu(schemat, dt, { licencja: 'zlota' });
    const rozne = [...new Set([...Object.keys(eA), ...Object.keys(eT)])]
      .filter((k) => JSON.stringify(eA[k]) !== JSON.stringify(eT[k])).sort();
    assert.deepEqual(rozne, Object.keys(wz.roznice).sort(), `${wz.szablon}: różnice względem szablonu`);
  }
});

test('pierwszeństwo: szablon → odpowiedzi → odpowiedzi o pola / zmiany ręczne', () => {
  const odp = { rodzaj: 'premia', kwota: 'stala', choroba: 'zachowany' };
  const d = { wartosci: { 'Algorytm.KreatorAlgorytmu.Podstawa': 300, 'Nieobecnosci.Urlop.Typ': 'Wliczać', 'Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent': 5 },
    ankieta: { odpowiedzi: odp, reczne: { 'Algorytm.KreatorAlgorytmu.Podstawa': true, 'Nieobecnosci.Urlop.Typ': true, 'Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent': true } } };
  const w = A.zastosuj(ankieta, schemat, d);
  assert.equal(w.szablon, 'premiaUznaniowa');
  assert.equal(w.pochodzenie.RodzajZrodla, 'szablon');
  assert.equal(w.wartosci['Algorytm.KreatorAlgorytmu.Korekta.Chorobowe'], 'NiePomniejsza');
  assert.equal(w.pochodzenie['Algorytm.KreatorAlgorytmu.Korekta.Chorobowe'], 'choroba');
  assert.equal(w.wartosci['Algorytm.KreatorAlgorytmu.Podstawa'], 300);
  assert.equal(w.pochodzenie['Algorytm.KreatorAlgorytmu.Podstawa'], 'kwotaStala');
  assert.equal(w.wartosci['Nieobecnosci.Urlop.Typ'], 'Wliczać');
  assert.equal(w.pochodzenie['Nieobecnosci.Urlop.Typ'], 'Nieobecnosci.Urlop.Typ');
  // pytanie ukryte warunkiem (procent przy stałej kwocie) – zmiana zostaje jako ręczna
  assert.equal(w.pochodzenie['Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent'], 'reczne');
  // pole bez zmiany użytkownika nie przechodzi ze starych wartości (zmiana odpowiedzi nie zostawia śladów)
  const s = A.zastosuj(ankieta, schemat, { wartosci: { 'Algorytm.DodProcent': 'stare' }, ankieta: { odpowiedzi: odp, reczne: {} } });
  assert.equal(s.wartosci['Algorytm.DodProcent'], undefined);
  // null w „ustaw” usuwa wartość szablonu
  const u = A.zastosuj(ankieta, schemat, { wartosci: {}, ankieta: { odpowiedzi: { rodzaj: 'dodatek', kwota: 'stala' } } });
  assert.equal('Algorytm.DodPodstawa' in u.wartosci, false);
});

test('wypłata jednorazowa: bez urlopu i podstawy zasiłku (pytania o chorobę i urlop ukryte)', () => {
  const { d, w } = poAnkiecie({ rodzaj: 'nagroda', kogo: 'etat', kwota: 'indywidualna', czestotliwosc: 'jednorazowo', podatki: 'tak', minimalne: 'nie' });
  assert.deepEqual(w.brak, []);
  const e = W.doEksportu(schemat, d, { licencja: 'zlota' });
  assert.equal(e['Nieobecnosci.Urlop.Typ'], 'NieWliczać');
  assert.equal(e['Nieobecnosci.ZasilkiPracownicy.Typ'], 'NieWliczać');
  assert.equal(e.MinimalneWynagrodzenie, false);
  assert.equal(e['OkresNaliczania.Typ'], 'Jednorazowa');
});

test('nie wiem, inaczej, wdrożeniowiec, do potwierdzenia', () => {
  const { w } = poAnkiecie({ rodzaj: 'dodatek', kogo: 'umowa', kwota: 'stala', czestotliwosc: 'miesiac', zaOkres: A.NIE_WIEM, choroba: 'inaczej', urlop: 'zmienny', nieplatne: 'tak', podatki: 'tak', minimalne: 'nie' });
  assert.deepEqual(w.nieWiem, ['zaOkres']);
  assert.deepEqual(w.wdrozeniowiec.map((x) => x.pytanie), ['kogo']);
  assert.deepEqual(w.doPotwierdzenia, ['choroba', 'urlop', 'minimalne']);
  assert.ok(w.szczegoly.includes('Algorytm.KreatorAlgorytmu.Korekta.Chorobowe'));
  assert.ok(w.szczegoly.includes('Nieobecnosci.ZasilkiPracownicy.Typ'));
  assert.ok(w.szczegoly.includes('Nieobecnosci.WahaniaWysokosci'));
  // pytania ukryte dla rodzaju nie są wymagane
  assert.deepEqual(poAnkiecie({ rodzaj: 'zasadnicze' }).w.brak, []);
});

test('pytania ukryte przed klientem i „Nie wiem” przy pytaniu o pole', () => {
  const d = { wartosci: {}, ankieta: { odpowiedzi: { rodzaj: 'premia', 'Deklaracje.KodRSA': A.NIE_WIEM }, reczne: {} } };
  const w = A.zastosuj(ankieta, schemat, d, (p) => !['rodzaj', 'Deklaracje.KodRSA'].includes(p.id));
  assert.deepEqual(w.brak, []);
  assert.deepEqual(w.nieWiem, ['Deklaracje.KodRSA']);
  assert.ok(A.zastosuj(ankieta, schemat, d).brak.includes('kogo'));
});

test('pytania ogólne a szczegółowe, zestawy, pytania tylko dla wdrożeniowca', () => {
  const p = (id) => ankieta.pytania.find((x) => x.id === id);
  assert.ok(A.polaUstawiane(p('choroba')).includes('Algorytm.KreatorAlgorytmu.Korekta.Chorobowe'));
  assert.ok(A.polaUstawiane(p('kwota')).includes('Algorytm.DodPodstawa'), 'także „ustawGdy”');
  assert.equal(A.szczegolowe(p('Algorytm.KreatorAlgorytmu.Korekta.Chorobowe')), true);
  assert.equal(A.szczegolowe(p('kwotaStala')), false, 'pytanie podstawowe uzupełnia odpowiedź ogólną');
  assert.equal(A.szczegolowe(p('nazwa')), false);
  assert.equal(A.szczegolowe(p('choroba')), false);
  for (const q of ankieta.pytania.filter((x) => x.zestaw)) assert.ok(A.szczegolowe(q), `zestaw tylko dla pytań szczegółowych: ${q.id}`);
  assert.deepEqual([...new Set(ankieta.pytania.filter((x) => x.tylkoWdrozeniowiec).map((x) => x.rozdzial))], ['techniczne']);
});
