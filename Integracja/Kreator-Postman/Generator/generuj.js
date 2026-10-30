// Testuje logikę Kreator.html i zapisuje gotowe pliki Postman oraz listę scenariuszy dla arkusza.
// Uruchomienie: node Generator\generuj.js
const fs = require('fs'), path = require('path'), vm = require('vm'), assert = require('assert');

const katalog = path.join(__dirname, '..');
const html = fs.readFileSync(path.join(katalog, 'Kreator.html'), 'utf8');
const kod = html.match(/<script id="logika">([\s\S]*?)<\/script>/)[1];
const k = {};
vm.runInNewContext(`${kod}; Object.assign(wynik, { zbudujZapytanie, sprawdz, parsujPolecenie, fikcyjnyPesel, kolekcja, pakiet, srodowisko, SCENARIUSZE });`, { wynik: k });
const r = o => JSON.parse(JSON.stringify(o));
const blad = (f, tekst) => assert.ok(k.sprawdz(f).bledy.some(b => b.includes(tekst)), tekst);
const umowa = { od: '2026-11-01', typUmowy: 'NaCzasNieokreślony', stanowisko: 'S', wydzial: 'X' };

// Polecenie tekstowe
let f = k.parsujPolecenie('zatrudnij Jan Testowy, kod API-01, od 2026-11-01 do 31.10.2027, na czas określony, stanowisko Główny Specjalista, wydział 01, 1/2');
assert.deepStrictEqual(r(f), { operacja: 'zatrudnij', imie: 'Jan', nazwisko: 'Testowy', kod: 'API-01', od: '2026-11-01', do: '2027-10-31', stanowisko: 'Główny Specjalista', wydzial: '01', licznik: '1', mianownik: '2', typUmowy: 'NaCzasOkreślony' });
assert.strictEqual(k.parsujPolecenie('zatrudnij Anna Nowak na czas nieokreślony od 1.12.2026').typUmowy, 'NaCzasNieokreślony');
assert.strictEqual(k.parsujPolecenie('zwolnij pracownika Jan Testowy kod A1 od 2026-01-01 do 2027-03-31, przyczyna porzucenie pracy').przyczyna, 'porzucenie pracy');
assert.strictEqual(k.parsujPolecenie('przedłuż umowę Jan Testowy kod A1 do 2028-01-31').nazwisko, 'Testowy');
assert.strictEqual(k.parsujPolecenie('zmień dane Jan Testowy kod A1 z dniem 2027-01-01').dataZmiany, '2027-01-01');
f = k.parsujPolecenie('zmień rachunek kod A1 na 12 1050 0000 0000 0000 0000 0001');
assert.strictEqual(f.operacja, 'rachunek');
assert.strictEqual(f.rachunek, '12105000000000000000000001');
assert.strictEqual(k.parsujPolecenie('zatrudnij Jan Testowy od 2026-11-01 rachunek 12105000000000000000000001').operacja, 'zatrudnij');

// Budowa żądań wg DTO dodatku
const z = k.zbudujZapytanie({ operacja: 'zatrudnij', kod: 'A', imie: 'J', nazwisko: 'T', ...umowa, licznik: '1', mianownik: '2', pkid: 'P', typPracownika: 'U' });
assert.strictEqual(z.metoda, 'UpsertEmployee');
assert.strictEqual(z.cialo.TestMode, true);
assert.deepStrictEqual(r(z.cialo.DataAktualizacji), { From: { Year: 2026, Month: 11, Day: 1 } });
assert.deepStrictEqual(r(z.cialo.Zatrudnienie.OkresZatrudnieniaDo), { Year: 9999, Month: 12, Day: 31 });
assert.strictEqual(z.cialo.Zatrudnienie.JednostkaOrg, 'X');
assert.strictEqual(z.cialo.PKID, 'P');
assert.strictEqual(z.cialo.Zatrudnienie.TypPracownika, 'U');
const zw = k.zbudujZapytanie({ operacja: 'zwolnij', kod: 'A', od: '2026-11-01', do: '2027-03-31', przyczyna: 'P' });
assert.strictEqual(zw.metoda, 'UpsertTermination');
assert.strictEqual(zw.cialo.PracownikKod, 'A');
assert.strictEqual(zw.cialo.TerminationReason, 'P');
assert.deepStrictEqual(r(k.zbudujZapytanie({ operacja: 'rachunek', kod: 'A', rachunek: '1', dataZmiany: '2027-01-01' }).cialo), { TestMode: true, DataAktualizacji: { From: { Year: 2027, Month: 1, Day: 1 } }, Kod: 'A', Rachunki: [{ Numer: '1', Domyslne: true }] });
const adres = k.zbudujZapytanie({ operacja: 'zmien', kod: 'A', ulica: 'U', wojewodztwo: 'mazowieckie', dataZmiany: '2027-01-01' }).cialo;
assert.strictEqual(adres.AdresZamieszkania.Wojewodztwo, 'mazowieckie');
assert.strictEqual(adres.Zatrudnienie, undefined);
assert.strictEqual(adres.Imie, undefined);

// Walidacja
assert.strictEqual(k.sprawdz({ operacja: 'zatrudnij', imie: 'J', nazwisko: 'T', ...umowa }).bledy.length, 0);
blad({ operacja: 'zmien', dataZmiany: '2027-01-01' }, 'Kod');
blad({ operacja: 'zmien', kod: 'A' }, 'datę zmiany');
blad({ operacja: 'rachunek', kod: 'A' }, 'datę zmiany');
blad({ operacja: 'zatrudnij', imie: 'J', nazwisko: 'T', od: '2026-11-01', wydzial: 'X', stanowisko: 'S' }, 'typ umowy');
blad({ operacja: 'zatrudnij', imie: 'J', nazwisko: 'T', od: '2026-11-01', typUmowy: 'NaCzasOkreślony' }, 'stanowiska');
blad({ operacja: 'przedluz', kod: 'A', do: '2028-01-31', dataZmiany: '2027-01-01' }, 'typu umowy');
blad({ operacja: 'zmien', kod: 'A', dataZmiany: '2027-01-01', stanowisko: 'S' }, 'typu umowy');
blad({ operacja: 'zwolnij', kod: 'A', od: '2026-11-01', do: '2027-03-31' }, 'przyczynę');
assert.ok(k.sprawdz({ operacja: 'zmien', kod: 'A', ulica: 'U', dataZmiany: '2027-01-01' }).uwagi.some(u => u.includes('województwa')));
const pesel = k.fikcyjnyPesel('1990-05-15', 'Mężczyzna', 6);
assert.strictEqual(k.sprawdz({ operacja: 'zatrudnij', imie: 'J', nazwisko: 'T', ...umowa, pesel }).bledy.length, 0);
blad({ operacja: 'zatrudnij', imie: 'J', nazwisko: 'T', ...umowa, pesel: '90051500061' }, 'PESEL');

// Postman: wartości wpisane wprost, zmienne tylko dla połączenia i nazw usług
const kol = r(k.pakiet());
const zadania = kol.item.flatMap(i => i.item);
assert.ok(kol.info.schema.includes('v2.1.0'));
assert.strictEqual(zadania.length, k.SCENARIUSZE.length);
for (const q of zadania) {
  assert.ok(q.request.url.endsWith('/api/MethodInvoker/InvokeServiceMethod'), q.name);
  const b = JSON.parse(q.request.body.raw);
  assert.ok(!JSON.stringify(b.MethodArgs.pars).includes('{{'), `zmienna w danych: ${q.name}`);
}
assert.strictEqual(new Set(k.SCENARIUSZE.map(s => s.id)).size, k.SCENARIUSZE.length);

const zapisz = (plik, o) => {
  fs.mkdirSync(path.dirname(path.join(katalog, plik)), { recursive: true });
  fs.writeFileSync(path.join(katalog, plik), JSON.stringify(o, null, 2) + '\n');
};
zapisz('Postman/Kadry-dodatek.postman_collection.json', k.pakiet());
zapisz('Postman/Kadry-TEST.postman_environment.json', k.srodowisko());
zapisz('Generator/scenariusze.json', k.SCENARIUSZE.map(s => ({ Id: s.id, Grupa: s.grupa, Nazwa: s.nazwa, Metoda: s.z.metoda, Oczekiwane: s.oczekiwane, Podstawa: s.podstawa })));
console.log(`OK: ${k.SCENARIUSZE.length} scenariuszy`);
