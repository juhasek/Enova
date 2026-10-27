// Arkusz przeglądu pytań ankiety dla konsultanta płacowego: schema/przeglad-pytan.xlsx
//   node Formularz/przeglad-pytan.mjs
// Jedno pytanie = jeden wiersz (rozdział, treść, pola enova, możliwe odpowiedzi, zestaw, obszar prawny) + kolumny na uwagi.
// XLSX składany ręcznie (SpreadsheetML + zip) – bez zależności; wymaga Node 22+ (zlib.crc32).
import { readFileSync, writeFileSync } from 'node:fs';
import { deflateRawSync, crc32 } from 'node:zlib';

const kat = new URL('./', import.meta.url);
const czytaj = (p) => JSON.parse(readFileSync(new URL(p, kat), 'utf8'));
const schemat = czytaj('../schema/definicja-elementu.schema.json');
const ankieta = czytaj('../schema/ankieta.json');

const rozdzial = Object.fromEntries(ankieta.rozdzialy.map((r) => [r.id, r]));
const polaPytania = (p) => p.pola || (p.pole ? [p.pole] : []);

function odpowiedzi(p) {
  if (!p.typ || p.typ === 'wybor') return p.odpowiedzi.map((o) => o.etykieta).join('\n');
  if (p.typ === 'uwagi') return '(tekst)';
  return polaPytania(p).map((s) => {
    const pole = schemat.pola[s];
    const et = p.etykiety || {};
    if (pole.typ === 'bool') return (polaPytania(p).length > 1 ? pole.etykieta + ': ' : '') + [et.true || 'Tak', et.false || 'Nie'].join(' / ');
    if (pole.enum) return schemat.enumy[pole.enum].wartosci.filter((w) => !w.ukryta)
      .map((w) => (p.typowe && p.typowe.includes(w.nazwa) ? '★ ' : '') + (et[w.nazwa] || w.etykieta)).join('\n');
    return `${pole.etykieta}: wpisywane (${pole.typ}${pole.slownik ? ', słownik' : ''})`;
  }).join('\n');
}

const naglowek = ['Nr', 'Rozdział', 'Pytanie', 'Opis dla klienta', 'Rodzaj pytania', 'Pola enova', 'Możliwe odpowiedzi (★ = kafelek typowej wartości)',
  'Zestaw', 'Obszar prawny', 'Uwagi konsultanta', 'Decyzja (OK / zmienić / usunąć)'];
const szerokosci = [5, 18, 45, 40, 18, 32, 45, 14, 10, 40, 16];
const wiersze = ankieta.pytania.map((p, i) => [
  i + 1,
  rozdzial[p.rozdzial].tytul,
  p.pytanie,
  p.opis || '',
  !p.typ || p.typ === 'wybor' ? 'ogólne (kilka pól)' : p.typ === 'uwagi' ? 'uwagi' : p.podstawowe ? 'pole – podstawowe' : 'pole – szczegółowe',
  polaPytania(p).map((s) => `${schemat.pola[s].etykieta} [${s}]`).join('\n'),
  odpowiedzi(p),
  p.tylkoWdrozeniowiec ? 'tylko wdrożeniowiec' : !p.typ || p.typ === 'wybor' || p.typ === 'uwagi' || p.podstawowe ? 'podstawowy' : p.zestaw === 'rozszerzona' ? 'rozszerzony' : 'pełny',
  p.prawne || rozdzial[p.rozdzial].prawne ? 'tak' : '',
  '', ''
]);

const xml = (t) => String(t).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
const kol = (i) => String.fromCharCode(65 + i);
const komorka = (v, r, c, styl) => typeof v === 'number'
  ? `<c r="${kol(c)}${r}" s="${styl}"><v>${v}</v></c>`
  : `<c r="${kol(c)}${r}" s="${styl}" t="inlineStr"><is><t xml:space="preserve">${xml(v)}</t></is></c>`;
const ostatnia = kol(naglowek.length - 1);
const arkusz = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
<sheetViews><sheetView workbookViewId="0"><pane ySplit="1" topLeftCell="A2" activePane="bottomLeft" state="frozen"/></sheetView></sheetViews>
<cols>${szerokosci.map((w, i) => `<col min="${i + 1}" max="${i + 1}" width="${w}" customWidth="1"/>`).join('')}</cols>
<sheetData>
<row r="1">${naglowek.map((h, c) => komorka(h, 1, c, 1)).join('')}</row>
${wiersze.map((w, i) => `<row r="${i + 2}">${w.map((v, c) => komorka(v, i + 2, c, c >= 9 ? 2 : 0)).join('')}</row>`).join('\n')}
</sheetData>
<autoFilter ref="A1:${ostatnia}${wiersze.length + 1}"/>
</worksheet>`;
const style = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
<fonts count="2"><font><sz val="10"/><name val="Calibri"/></font><font><b/><sz val="10"/><name val="Calibri"/></font></fonts>
<fills count="4"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill>
<fill><patternFill patternType="solid"><fgColor rgb="FFDCE6F2"/></patternFill></fill><fill><patternFill patternType="solid"><fgColor rgb="FFFFF4D6"/></patternFill></fill></fills>
<borders count="2"><border/><border><left style="thin"><color rgb="FFBFBFBF"/></left><right style="thin"><color rgb="FFBFBFBF"/></right><top style="thin"><color rgb="FFBFBFBF"/></top><bottom style="thin"><color rgb="FFBFBFBF"/></bottom></border></borders>
<cellStyleXfs count="1"><xf/></cellStyleXfs>
<cellXfs count="3">
<xf fontId="0" fillId="0" borderId="1" applyBorder="1" applyAlignment="1"><alignment vertical="top" wrapText="1"/></xf>
<xf fontId="1" fillId="2" borderId="1" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment vertical="center" wrapText="1"/></xf>
<xf fontId="0" fillId="3" borderId="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment vertical="top" wrapText="1"/></xf>
</cellXfs>
</styleSheet>`;
const pliki = {
  '[Content_Types].xml': `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>`,
  '_rels/.rels': `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>`,
  'xl/workbook.xml': `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Pytania ankiety" sheetId="1" r:id="rId1"/></sheets><definedNames><definedName name="_xlnm._FilterDatabase" localSheetId="0" hidden="1">'Pytania ankiety'!$A$1:$${ostatnia}$${wiersze.length + 1}</definedName></definedNames></workbook>`,
  'xl/_rels/workbook.xml.rels': `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>`,
  'xl/styles.xml': style,
  'xl/worksheets/sheet1.xml': arkusz,
};

/** Minimalny zip (deflate) – tylko to, czego potrzebuje xlsx. */
function zip(pliki) {
  const lokalne = [], centralny = [];
  let przes = 0;
  for (const [nazwa, tresc] of Object.entries(pliki)) {
    const dane = Buffer.from(tresc, 'utf8'), spak = deflateRawSync(dane), n = Buffer.from(nazwa, 'utf8'), crc = crc32(dane);
    const lh = Buffer.alloc(30);
    lh.writeUInt32LE(0x04034b50, 0); lh.writeUInt16LE(20, 4); lh.writeUInt16LE(0x0800, 6); lh.writeUInt16LE(8, 8);
    lh.writeUInt32LE(crc, 14); lh.writeUInt32LE(spak.length, 18); lh.writeUInt32LE(dane.length, 22); lh.writeUInt16LE(n.length, 26);
    const ch = Buffer.alloc(46);
    ch.writeUInt32LE(0x02014b50, 0); ch.writeUInt16LE(20, 4); ch.writeUInt16LE(20, 6); ch.writeUInt16LE(0x0800, 8); ch.writeUInt16LE(8, 10);
    ch.writeUInt32LE(crc, 16); ch.writeUInt32LE(spak.length, 20); ch.writeUInt32LE(dane.length, 24); ch.writeUInt16LE(n.length, 28); ch.writeUInt32LE(przes, 42);
    lokalne.push(lh, n, spak);
    centralny.push(ch, n);
    przes += lh.length + n.length + spak.length;
  }
  const cd = Buffer.concat(centralny), koniec = Buffer.alloc(22);
  koniec.writeUInt32LE(0x06054b50, 0); koniec.writeUInt16LE(centralny.length / 2, 8); koniec.writeUInt16LE(centralny.length / 2, 10);
  koniec.writeUInt32LE(cd.length, 12); koniec.writeUInt32LE(przes, 16);
  return Buffer.concat([...lokalne, cd, koniec]);
}

const cel = new URL('../schema/przeglad-pytan.xlsx', kat);
writeFileSync(cel, zip(pliki));
console.log(`Zapisano ${cel.pathname} (pytań: ${wiersze.length})`);
