// Test odwrotny ankiety na definicjach standardowych enova (schemat → wzorceTestowe, zrodla.json):
// dla każdej definicji szuka odpowiedzi na pytania ogólne, które najlepiej ją odtwarzają, i wypisuje różnice,
// które klient musiałby poprawić pytaniami szczegółowymi. Wynik: schema/TEST-ODWROTNY.md (materiał do przeglądu).
//   node Formularz/test/test-odwrotny.mjs
import { readFileSync, writeFileSync } from 'node:fs';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const W = require('../src/warunki.js');
const A = require('../src/ankieta.js');
const czytaj = (p) => JSON.parse(readFileSync(new URL(p, import.meta.url), 'utf8'));
const schemat = czytaj('../../schema/definicja-elementu.schema.json');
const ankieta = czytaj('../../schema/ankieta.json');
const LIC = { licencja: 'zlota' };
const ID = { Nazwa: 'Wzorzec', Skrot: 'W' };

const eksport = (wartosci) => W.doEksportu(schemat, { wartosci: { ...wartosci, ...ID } }, LIC);
const roznice = (a, b) => [...new Set([...Object.keys(a), ...Object.keys(b)])].filter((k) => JSON.stringify(a[k]) !== JSON.stringify(b[k])).sort();

function wynikOdpowiedzi(odp, wzorzec) {
  const w = A.zastosuj(ankieta, schemat, { wartosci: {}, ankieta: { odpowiedzi: odp, reczne: {} } });
  // pytania „podstawowe” (kwota, procent, progi…) klient wpisuje – przyjmujemy wartości wzorca
  for (const p of A.widocznePytania(ankieta, odp)) if (p.podstawowe) for (const s of A.polaPytania(p)) if (s in wzorzec) w.wartosci[s] = wzorzec[s];
  return { eA: eksport(w.wartosci), w };
}

// odpowiedzi bez „Nie wiem” i „Inaczej” – test sprawdza, ile odtwarzają same pytania ogólne
const opcje = (p) => p.odpowiedzi.map((o) => o.id).filter((id) => id !== 'inaczej');
const wybor = ankieta.pytania.filter((p) => A.typ(p) === 'wybor');

function najlepsze(wzorzec) {
  const eT = eksport(wzorzec);
  const ocen = (odp) => roznice(wynikOdpowiedzi(odp, wzorzec).eA, eT).length;
  let najl = null;
  for (const r of opcje(wybor[0])) { // każdy rodzaj, potem kolejne pytania po kolei (3 przebiegi)
    const odp = { [wybor[0].id]: r };
    for (let przebieg = 0; przebieg < 3; przebieg++)
      for (const p of wybor.slice(1)) {
        if (!A.widocznePytania(ankieta, odp).includes(p)) { delete odp[p.id]; continue; }
        let best = null, bestN = Infinity;
        for (const o of opcje(p)) { const n = ocen({ ...odp, [p.id]: o }); if (n < bestN) { bestN = n; best = o; } }
        odp[p.id] = best;
      }
    const n = ocen(odp);
    if (!najl || n < najl.n) najl = { odp: { ...odp }, n };
  }
  const { eA } = wynikOdpowiedzi(najl.odp, wzorzec);
  return { ...najl, lista: roznice(eA, eT).map((s) => ({ s, ankieta: eA[s], wzorzec: eT[s] })) };
}

const pytanieOPole = {};
for (const p of ankieta.pytania) for (const s of A.polaPytania(p)) pytanieOPole[s] = p;
const tekst = (s, v) => v === undefined ? '—' : W.etykietaWartosci(schemat, s, v) + '';
const etykietaOdp = (id, o) => (ankieta.pytania.find((p) => p.id === id).odpowiedzi.find((x) => x.id === o) || {}).etykieta || o;

let md = `# Test odwrotny ankiety na definicjach standardowych enova\n\n`
  + `Wygenerowano: ${new Date().toISOString().slice(0, 10)} · ankieta ${ankieta.wersja} · enova ${schemat.wersjaEnova} · licencja złota.\n\n`
  + `Dla każdej definicji standardowej szukane są odpowiedzi na **pytania ogólne** (bez „Inaczej” i „Nie wiem”), które najlepiej ją odtwarzają. `
  + `Różnice to pola, które klient musiałby poprawić pytaniem szczegółowym – kolumna „Pytanie” mówi, czy i w jakim zestawie klient je zobaczy. `
  + `Materiał do przeglądu z konsultantem: duża liczba różnic = brakuje pytania ogólnego albo mapowania.\n\n`
  + `| Definicja | Pól różnych | Odpowiedzi |\n|---|---|---|\n`;
const wyniki = schemat.wzorceTestowe.map((wz) => ({ wz, r: najlepsze(wz.wartosci) }));
for (const { wz, r } of wyniki)
  md += `| ${wz.etykieta} | ${r.n} | ${Object.entries(r.odp).map(([k, v]) => etykietaOdp(k, v)).join(' · ')} |\n`;
for (const { wz, r } of wyniki) {
  md += `\n## ${wz.etykieta} – ${r.n} różnic\n\n`;
  if (!r.lista.length) { md += 'Pytania ogólne odtwarzają definicję w całości.\n'; continue; }
  md += `| Pole | Z ankiety | W enova | Pytanie (zestaw) |\n|---|---|---|---|\n`;
  for (const x of r.lista) {
    const p = pytanieOPole[x.s];
    const zestaw = !p ? '—' : p.tylkoWdrozeniowiec ? 'tylko wdrożeniowiec' : p.podstawowe ? 'podstawowy' : p.zestaw === 'rozszerzona' ? 'rozszerzony' : 'pełny';
    md += `| ${schemat.pola[x.s]?.etykieta || x.s} | ${tekst(x.s, x.ankieta)} | ${tekst(x.s, x.wzorzec)} | ${p ? p.pytanie.replace(/\|/g, '/') : '—'} (${zestaw}) |\n`;
  }
}
writeFileSync(new URL('../../schema/TEST-ODWROTNY.md', import.meta.url), md, 'utf8');
console.log(wyniki.map(({ wz, r }) => `${wz.etykieta}: ${r.n}`).join('\n'));
