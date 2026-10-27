// Buduje samodzielny formularz HTML (offline, jeden plik) ze schematu i źródeł w src/.
//   node Formularz/build.mjs
// Wynik: Formularz/dist/formularz-elementow-wynagrodzenia.html
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';

const kat = new URL('./', import.meta.url);
const czytaj = (p) => readFileSync(new URL(p, kat), 'utf8');
const schemat = JSON.parse(czytaj('../schema/definicja-elementu.schema.json'));
const ankieta = JSON.parse(czytaj('../schema/ankieta.json'));

// Do formularza trafia tylko to, czego potrzebuje klient – bez reguł z kodu enova, typów .NET i uwag technicznych.
const POLA_POLA = ['sciezka', 'etykieta', 'etykietaEnova', 'typ', 'enum', 'slownik', 'wymagane', 'wymaganeGdy', 'maxDlugosc',
  'domyslna', 'widocznyGdy', 'edytowalnyGdy', 'powodNieaktywnosci', 'walidacje', 'dozwoloneWartosci', 'licencja', 'opis', 'podpowiedz',
  'opisDoWeryfikacji', 'zrodloOpisu', 'kolumny'];
const wybierz = (o, klucze) => Object.fromEntries(klucze.filter((k) => o[k] !== undefined && o[k] !== null).map((k) => [k, o[k]]));

const klient = {
  wersjaSchematu: schemat.wersjaSchematu,
  wersjaEnova: schemat.wersjaEnova,
  wygenerowano: schemat.wygenerowano,
  zakladki: schemat.zakladki.map((z) => ({
    ...wybierz(z, ['id', 'etykieta', 'opis', 'widocznaGdy']),
    sekcje: z.sekcje.map((s) => wybierz(s, ['id', 'etykieta', 'opis', 'widocznaGdy', 'pola'])),
  })),
  pola: Object.fromEntries(Object.entries(schemat.pola).map(([k, p]) => [k, wybierz(p, POLA_POLA)])),
  enumy: Object.fromEntries(Object.entries(schemat.enumy).map(([k, e]) => [k, {
    wartosci: e.wartosci.map((w) => wybierz(w, ['nazwa', 'etykieta', 'opis', 'ukryta'])),
  }])),
  slowniki: Object.fromEntries(Object.entries(schemat.slowniki).map(([k, s]) => [k, {
    etykieta: s.etykieta,
    wartosciStandardowe: (s.wartosciStandardowe || []).map((w) => wybierz(w, ['klucz', 'opis'])),
  }])),
  definicjeStandardowe: schemat.definicjeStandardowe.map((d) => ({ nazwa: d.nazwa })),
  szablony: schemat.szablony.map((s) => wybierz(s, ['id', 'etykieta', 'opis', 'uwagi', 'wartosci'])),
  ankieta,
};

// JSON wewnątrz <script>: zabezpieczenie przed "</script>" i znakami końca linii JS
const BS = String.fromCharCode(92); // ukośnik wsteczny
const jsonSchematu = JSON.stringify(klient)
  .replace(/</g, BS + 'u003c')
  .replace(new RegExp(String.fromCharCode(0x2028), 'g'), BS + 'u2028')
  .replace(new RegExp(String.fromCharCode(0x2029), 'g'), BS + 'u2029');

const html = czytaj('src/index.html')
  .replace('/*__STYLE__*/', () => czytaj('src/style.css'))
  .replace('/*__SCHEMAT__*/', () => jsonSchematu)
  .replace('/*__WARUNKI__*/', () => czytaj('src/warunki.js'))
  .replace('/*__ANKIETA__*/', () => czytaj('src/ankieta.js'))
  .replace('/*__APP__*/', () => czytaj('src/app.js'));

mkdirSync(new URL('dist/', kat), { recursive: true });
const cel = new URL('dist/formularz-elementow-wynagrodzenia.html', kat);
writeFileSync(cel, html, 'utf8');
console.log(`Zapisano ${cel.pathname} (${Math.round(html.length / 1024)} KB, pól: ${Object.keys(klient.pola).length}, szablonów: ${klient.szablony.length}, pytań ankiety: ${ankieta.pytania.length})`);
