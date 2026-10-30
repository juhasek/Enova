// Testuje logikę Kreator.html i zapisuje gotowe pliki Postman oraz listę scenariuszy dla arkusza.
// Uruchomienie: node Generator\generuj.js
const fs = require('fs'), path = require('path'), vm = require('vm'), assert = require('assert');

const katalog = path.join(__dirname, '..');
const html = fs.readFileSync(path.join(katalog, 'Kreator.html'), 'utf8');
const kod = html.match(/<script id="logika">([\s\S]*?)<\/script>/)[1];
const k = {};
vm.runInNewContext(`${kod}; Object.assign(wynik, { zbudujZapytanie, sprawdz, parsujPolecenie, parsujCechy, fikcyjnyPesel, doPostmana, kolekcja, pakiet, srodowisko, SCENARIUSZE });`, { wynik: k });
const r = o => JSON.parse(JSON.stringify(o));

// Polecenie tekstowe
let f = k.parsujPolecenie('zatrudnij Jan Testowy, kod API-01, od 2026-11-01 do 31.10.2027, na czas określony, stanowisko Główny Specjalista, wydział 01, 1/2');
assert.deepStrictEqual(r(f), { operacja: 'zatrudnij', imie: 'Jan', nazwisko: 'Testowy', kod: 'API-01', od: '2026-11-01', do: '2027-10-31', stanowisko: 'Główny Specjalista', wydzial: '01', licznik: '1', mianownik: '2', typUmowy: 'NaCzasOkreślony' });
assert.strictEqual(k.parsujPolecenie('zatrudnij Anna Nowak na czas nieokreślony od 1.12.2026').typUmowy, 'NaCzasNieokreślony');
assert.strictEqual(k.parsujPolecenie('zwolnij pracownika Jan Testowy kod A1 do 2027-03-31').operacja, 'zwolnij');
assert.strictEqual(k.parsujPolecenie('przedłuż umowę Jan Testowy kod A1 do 2028-01-31').nazwisko, 'Testowy');
assert.strictEqual(k.parsujPolecenie('zmień dane Jan Testowy kod A1 z dniem 2027-01-01').dataZmiany, '2027-01-01');

// Budowa żądań
const zatrudnij = k.zbudujZapytanie({ operacja: 'zatrudnij', kod: 'A', imie: 'J', nazwisko: 'T', od: '2026-11-01', typUmowy: 'NaCzasNieokreślony', licznik: '1', mianownik: '1', wydzial: 'X', stanowisko: 'S' });
assert.strictEqual(zatrudnij.metoda, 'UpdateEmployee');
assert.deepStrictEqual(r(zatrudnij.cialo.Zatrudnienie.OkresOd), { Year: 2026, Month: 11, Day: 1 });
assert.deepStrictEqual(r(zatrudnij.cialo.Zatrudnienie.OkresDo), { Year: 9999, Month: 12, Day: 31 });
assert.strictEqual(zatrudnij.cialo.Aktualizuj, undefined);
const zwolnij = k.zbudujZapytanie({ operacja: 'zwolnij', kod: 'A', imie: 'J', nazwisko: 'T', od: '2026-11-01', do: '2027-03-31', powod: 'P' });
assert.strictEqual(zwolnij.cialo.Aktualizuj, true);
assert.strictEqual(zwolnij.cialo.Zatrudnienie.Zwolniony, true);
assert.strictEqual(zwolnij.cialo.DataAktualizacji, '2027-03-31T00:00:00');

// Cechy
assert.deepStrictEqual(r(k.parsujCechy('A=1\nB=x@2026-02-10\nzly')), [{ Name: 'A', Value: '1' }, { Name: 'B', Value: 'x', UpdateDate: { Year: 2026, Month: 2, Day: 10 } }, { blad: 'zly' }]);

// PESEL i walidacja
const pesel = k.fikcyjnyPesel('1990-05-15', 'Mężczyzna', 6);
assert.strictEqual(pesel.length, 11);
assert.strictEqual(k.sprawdz({ operacja: 'zatrudnij', kod: 'A', imie: 'J', nazwisko: 'T', od: '2026-11-01', stanowisko: 'S', wydzial: 'X', pesel }).bledy.length, 0);
assert.ok(k.sprawdz({ operacja: 'zatrudnij', kod: 'A', imie: 'J', nazwisko: 'T', od: '2026-11-01', pesel: '90051500061' }).bledy.some(b => b.includes('PESEL')));
assert.ok(k.sprawdz({ operacja: 'zatrudnij', imie: 'J', nazwisko: 'T', od: '2026-11-01' }).bledy.some(b => b.includes('Kod')));
assert.ok(k.sprawdz({ operacja: 'zatrudnij', kod: 'A', imie: 'J', nazwisko: 'T', od: '2026-11-01', typUmowy: 'NaCzasOkreślony' }).bledy.some(b => b.includes('końca')));
assert.ok(k.sprawdz({ operacja: 'zwolnij', kod: 'A', imie: 'J', nazwisko: 'T' }).bledy.length > 0);
assert.ok(k.sprawdz({ operacja: 'zatrudnij', kod: 'A', imie: 'J', nazwisko: 'T', od: '2026-11-01' }).bledy.some(b => b.includes('stanowiska')));
assert.ok(k.sprawdz({ operacja: 'zmien', kod: 'A', imie: 'J', nazwisko: 'T' }).bledy.some(b => b.includes('datę zmiany')));
assert.strictEqual(k.zbudujZapytanie({ operacja: 'przedluz', kod: 'A', imie: 'J', nazwisko: 'T', do: '2028-01-31', dataZmiany: '2027-01-01' }).cialo.Zatrudnienie.OkresOd, undefined);

// Postman: oba tryby, poprawny JSON po podstawieniu testMode
for (const tryb of ['dynamic', 'invoker']) {
  const kol = r(k.pakiet(tryb));
  const zadania = kol.item.flatMap(i => i.item || [i]);
  assert.ok(kol.info.schema.includes('v2.1.0'));
  assert.strictEqual(zadania.length, k.SCENARIUSZE.length + (tryb === 'dynamic' ? 2 : 0));
  for (const z of zadania) {
    assert.ok(z.request.url.startsWith('{{baseUrl}}/api/'), z.name);
    if (z.request.body) JSON.parse(z.request.body.raw.replaceAll('{{testMode}}', 'true'));
  }
  const pierwszy = zadania.find(z => z.name.startsWith('Z-01'));
  const cialo = JSON.parse(pierwszy.request.body.raw.replaceAll('{{testMode}}', 'true'));
  if (tryb === 'dynamic') {
    assert.ok(pierwszy.request.url.endsWith('/api/KadryPlaceWebAPI/UpdateEmployee'));
    assert.strictEqual(cialo.TestMode, true);
    assert.strictEqual(zadania[0].request.url, '{{baseUrl}}/api/LoginApi');
  } else {
    assert.ok(pierwszy.request.url.endsWith('/api/MethodInvoker/InvokeServiceMethod'));
    assert.strictEqual(cialo.ServiceName, 'AltOne.WCF.Core.Interfaces.IKadryPlace2, AltOne.WCF.Core');
    assert.strictEqual(cialo.MethodName, 'UpdateEmployee');
    assert.strictEqual(cialo.MethodArgs.pars.TestMode, true);
  }
}
assert.strictEqual(new Set(k.SCENARIUSZE.map(s => s.id)).size, k.SCENARIUSZE.length);

const zapisz = (plik, o) => {
  fs.mkdirSync(path.dirname(path.join(katalog, plik)), { recursive: true });
  fs.writeFileSync(path.join(katalog, plik), JSON.stringify(o, null, 2) + '\n');
};
zapisz('Postman/Kadry-scenariusze-DynamicAPI.postman_collection.json', k.pakiet('dynamic'));
zapisz('Postman/Kadry-scenariusze-MethodInvoker.postman_collection.json', k.pakiet('invoker'));
zapisz('Postman/Kadry-TEST.postman_environment.json', k.srodowisko());
zapisz('Generator/scenariusze.json', k.SCENARIUSZE.map(s => ({ Id: s.id, Grupa: s.grupa, Nazwa: s.nazwa, Metoda: s.z.metoda, Oczekiwane: s.oczekiwane, Podstawa: s.podstawa })));
console.log(`OK: ${k.SCENARIUSZE.length} scenariuszy`);
