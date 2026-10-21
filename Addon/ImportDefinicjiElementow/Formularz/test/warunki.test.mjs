// Testy silnika reguł formularza: node --test Formularz/test
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const W = require('../src/warunki.js');
const schemat = JSON.parse(readFileSync(new URL('../../schema/definicja-elementu.schema.json', import.meta.url), 'utf8'));
const zlota = { licencja: 'zlota' };

function def(wartosci) { return { id: 'x', wartosci: { RodzajZrodla: 'Dodatek', Nazwa: 'Test', Skrot: 'T', ...wartosci } }; }
function stan(d, s, ust = zlota) { return new W.Kontekst(schemat, d, ust).stan(s); }
function wart(d, s, ust = zlota) { return new W.Kontekst(schemat, d, ust).wartosc(s); }
function bledy(d, wszystkie, ust = zlota) { return W.waliduj(schemat, d, wszystkie || [d], ust).filter(b => b.poziom === 'blad'); }

test('operatory warunków', () => {
  const ctx = { licencja: 'zlota', wartosc: s => ({ a: 'X', b: 3, c: '' })[s] };
  assert.equal(W.ocen({ pole: 'a', rowne: 'X' }, ctx), true);
  assert.equal(W.ocen({ pole: 'a', rozne: 'X' }, ctx), false);
  assert.equal(W.ocen({ pole: 'a', wsrod: ['Y', 'X'] }, ctx), true);
  assert.equal(W.ocen({ pole: 'a', poza: ['X'] }, ctx), false);
  assert.equal(W.ocen({ pole: 'c', pusty: true }, ctx), true);
  assert.equal(W.ocen({ pole: 'b', wiekszeOd: 1 }, ctx), true);
  assert.equal(W.ocen({ wszystkie: [{ licencja: 'zlota' }, { nie: { licencja: 'platynowa' } }] }, ctx), true);
  assert.equal(W.ocen({ dowolny: [{ pole: 'a', rowne: 'Z' }, { pole: 'b', rowne: 3 }] }, ctx), true);
  assert.throws(() => W.ocen({ cos: 1 }, ctx));
});

test('wartości domyślne potwierdzone w enova', () => {
  const d = def({});
  assert.equal(wart(d, 'OkresNaliczania.Naliczanie'), 'PłatnaZDołu');
  assert.equal(wart(d, 'OkresNaliczania.Typ'), 'CoNMiesięcy');
  assert.equal(wart(d, 'DoWyplaty'), true);
  assert.equal(wart(d, 'Zatrudnienie'), 'Etat');
  assert.equal(wart(def({ RodzajZrodla: 'Umowa' }), 'Zatrudnienie'), 'Umowa');
  assert.equal(wart(d, 'Algorytm.Typ'), 'KreatorAlgorytmu');
  assert.equal(wart(d, 'Zaokraglenie.Precyzja'), 'DoPełnegoGrosza');
});

test('sekcje kreatora zależą od typu algorytmu i podstawy', () => {
  const d = def({});
  assert.equal(stan(d, 'Algorytm.KreatorAlgorytmu.Podstawa').aktywne, true);
  assert.equal(stan(def({ 'Algorytm.KreatorAlgorytmu.PodstawaTyp': 'Wskaźniki' }), 'Algorytm.KreatorAlgorytmu.Podstawa').aktywne, false);
  const jak = def({ 'Algorytm.Typ': 'JakUrlopWypoczynkowy' });
  assert.equal(stan(jak, 'Algorytm.KreatorAlgorytmu.PodstawaTyp').aktywne, false);
  assert.equal(stan(jak, 'Algorytm.JakUrlopWypoczynkowy.StdOkresPodstawy').aktywne, true);
  assert.equal(stan(jak, 'Algorytm.JakUrlopWypoczynkowy.OkresPodstawy').widoczne, true);
  assert.equal(stan(jak, 'Algorytm.JakUrlopWypoczynkowy.OkresPodstawy').edytowalne, false);
});

test('pomniejszenia widoczne tylko dla wartości miesięcznej, licencja steruje polem ZUS', () => {
  const nie = def({});
  assert.equal(stan(nie, 'Algorytm.KreatorAlgorytmu.Korekta.Chorobowe').aktywne, false);
  const mies = def({ 'Algorytm.KreatorAlgorytmu.PodstawaZa': 'Miesięcznie' });
  assert.equal(stan(mies, 'Algorytm.KreatorAlgorytmu.Korekta.Chorobowe').aktywne, true);
  assert.equal(stan(mies, 'Algorytm.KreatorAlgorytmu.Korekta.ZUS').aktywne, false);
  assert.equal(stan(mies, 'Algorytm.KreatorAlgorytmu.Korekta.ZUS', { licencja: 'srebrna' }).aktywne, true);
  assert.equal(stan(mies, 'Algorytm.KreatorAlgorytmu.Korekta.Chorobowe', { licencja: 'srebrna' }).aktywne, false);
});

test('dozwolone wartości algorytmu zależą od rodzaju elementu', () => {
  assert.deepEqual(stan(def({ RodzajZrodla: 'Umowa' }), 'Algorytm.Typ').dozwolone, ['KreatorAlgorytmu', 'EdytorAlgorytmu']);
  assert.ok(stan(def({}), 'Algorytm.Typ').dozwolone.includes('ZaOkresNieobecności'));
  const b = bledy(def({ RodzajZrodla: 'Umowa', 'Algorytm.Typ': 'ZaOkresNieobecności' }));
  assert.ok(b.some(x => x.sciezka === 'Algorytm.Typ'));
});

test('podatek: procent wymagany przy zaliczce procentowej, pola zależne od pozycji PIT', () => {
  const bezPit = def({ 'Deklaracje.Zaliczka.Typ': 'Procentowe' });
  assert.equal(stan(bezPit, 'Deklaracje.Zaliczka.Typ').edytowalne, false);
  assert.equal(wart(bezPit, 'Deklaracje.Zaliczka.Typ'), 'NieNaliczać', 'nieedytowalne pole bierze wartość domyślną');
  const pit = def({ 'Deklaracje.PozycjaPIT': 'PIT-11 1/PIT-4R 1', 'Deklaracje.Zaliczka.Typ': 'Procentowe' });
  assert.equal(stan(pit, 'Deklaracje.Zaliczka.Procent').wymagane, true);
  assert.ok(bledy(pit).length === 0, 'domyślny procent 0 jest wartością – brak błędu wymagalności');
  const zly = def({ 'Deklaracje.PozycjaPIT': 'PIT-11 1/PIT-4R 1', 'Deklaracje.Zaliczka.Typ': 'Procentowe', 'Deklaracje.Zaliczka.Procent': 150 });
  assert.ok(bledy(zly).some(x => x.sciezka === 'Deklaracje.Zaliczka.Procent'));
});

test('walidacja: wymagane, długość, unikalność, nazwa standardowa', () => {
  assert.ok(bledy({ id: 'a', wartosci: { RodzajZrodla: 'Dodatek' } }).some(b => b.sciezka === 'Nazwa'));
  assert.ok(bledy(def({ Skrot: 'x'.repeat(13) })).some(b => b.sciezka === 'Skrot'));
  const a = { id: 'a', wartosci: { RodzajZrodla: 'Dodatek', Nazwa: 'Premia X', Skrot: 'PX' } };
  const b = { id: 'b', wartosci: { RodzajZrodla: 'Dodatek', Nazwa: 'premia x ', Skrot: 'PX' } };
  const wb = W.waliduj(schemat, b, [a, b], zlota);
  assert.ok(wb.some(x => x.sciezka === 'Nazwa' && x.poziom === 'blad'));
  assert.ok(wb.some(x => x.sciezka === 'Skrot' && x.poziom === 'ostrzezenie'));
  assert.ok(bledy(def({ Nazwa: 'Premia procentowa' })).some(x => x.komunikat.includes('standardowa')));
});

test('szablony z definicji standardowych przechodzą walidację po nadaniu nazwy', () => {
  assert.ok(schemat.szablony.length >= 6);
  for (const s of schemat.szablony) {
    const d = { id: s.id, wartosci: { ...s.wartosci, Nazwa: 'Szablon ' + s.id, Skrot: s.id.slice(0, 12) } };
    const b = bledy(d).filter(x => !(s.id === 'zasadnicze' && x.sciezka.startsWith('Algorytm.KreatorAlgorytmu')));
    assert.deepEqual(b, [], 'szablon ' + s.id);
  }
});

test('eksport zawiera tylko aktywne pola', () => {
  const w = W.doEksportu(schemat, def({ 'Algorytm.Typ': 'JakUrlopWypoczynkowy', 'Algorytm.KreatorAlgorytmu.Podstawa': 100 }), zlota);
  assert.equal(w['Algorytm.Typ'], 'JakUrlopWypoczynkowy');
  assert.equal('Algorytm.KreatorAlgorytmu.Podstawa' in w, false);
  assert.equal(w['Algorytm.JakUrlopWypoczynkowy.StdOkresPodstawy'], true);
});

test('nazwy parametrów: dostępne tylko dla parametrów używanych przez algorytm', () => {
  const procent = def({ 'Algorytm.KreatorAlgorytmu.Wspolczynnik.Typ': 'Procent' });
  assert.equal(stan(procent, 'Algorytm.DodProcent').aktywne, true);
  assert.equal(stan(procent, 'Algorytm.DodUlamek').aktywne, false);
  assert.equal(stan(def({ RodzajZrodla: 'Etat', 'Algorytm.KreatorAlgorytmu.Wspolczynnik.Typ': 'Procent' }), 'Algorytm.DodProcent').aktywne, false, 'parametry dodatku tylko dla rodzaju Dodatek');
  const edytor = def({ 'Algorytm.Typ': 'EdytorAlgorytmu', 'Algorytm.DodProcent': 'Procent' });
  assert.equal(stan(edytor, 'Algorytm.DodUlamek').aktywne, false, 'tylko jeden z: ułamek, procent, współczynnik');
  assert.equal(stan(edytor, 'Algorytm.ElPodstawa5').aktywne, true);
  assert.equal(stan(procent, 'Algorytm.ElPodstawa5').aktywne, false);
  assert.equal(stan(procent, 'Algorytm.ElPodstawa1').aktywne, true);
});
