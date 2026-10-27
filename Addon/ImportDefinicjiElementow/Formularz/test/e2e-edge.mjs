// Test end-to-end zbudowanego formularza w Edge/Chrome headless (DevTools Protocol, bez zależności npm).
//   node Formularz/test/e2e-edge.mjs [katalog-wyników]
// Zapisuje zrzuty ekranu i pobrany plik JSON do katalogu wyników (domyślnie Formularz/test/wyniki).
import { spawn } from 'node:child_process';
import { mkdirSync, writeFileSync, readFileSync, existsSync, readdirSync, rmSync } from 'node:fs';
import { fileURLToPath, pathToFileURL } from 'node:url';
import path from 'node:path';
import assert from 'node:assert/strict';

const kat = path.dirname(fileURLToPath(import.meta.url));
const wyniki = path.resolve(process.argv[2] || path.join(kat, 'wyniki'));
const profil = path.join(wyniki, 'profil');
const pobrane = path.join(wyniki, 'pobrane');
rmSync(wyniki, { recursive: true, force: true });
mkdirSync(pobrane, { recursive: true });
const formularz = pathToFileURL(path.resolve(kat, '../dist/formularz-elementow-wynagrodzenia.html')).href;
const przegladarka = [
  'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
  'C:/Program Files/Google/Chrome/Application/chrome.exe',
].find(existsSync);

const PORT = 9333;
const proc = spawn(przegladarka, ['--headless=new', `--remote-debugging-port=${PORT}`, `--user-data-dir=${profil}`,
  '--window-size=1400,1000', '--no-first-run', '--disable-gpu', 'about:blank'], { stdio: 'ignore' });
const czekaj = (ms) => new Promise((r) => setTimeout(r, ms));

let ws, nr = 0;
const oczekujace = new Map();
async function polacz() {
  for (let i = 0; i < 50; i++) {
    try {
      const lista = await (await fetch(`http://127.0.0.1:${PORT}/json`)).json();
      const strona = lista.find((x) => x.type === 'page');
      if (strona) {
        ws = new WebSocket(strona.webSocketDebuggerUrl);
        await new Promise((r, e) => { ws.onopen = r; ws.onerror = e; });
        ws.onmessage = (m) => {
          const d = JSON.parse(m.data);
          if (d.id && oczekujace.has(d.id)) { oczekujace.get(d.id)(d); oczekujace.delete(d.id); }
        };
        return;
      }
    } catch { /* jeszcze startuje */ }
    await czekaj(200);
  }
  throw new Error('Nie udało się połączyć z przeglądarką');
}
function cdp(method, params = {}) {
  const id = ++nr;
  ws.send(JSON.stringify({ id, method, params }));
  return new Promise((r, e) => oczekujace.set(id, (d) => d.error ? e(new Error(method + ': ' + d.error.message)) : r(d.result)));
}
async function js(wyr) {
  const r = await cdp('Runtime.evaluate', { expression: wyr, awaitPromise: true, returnByValue: true });
  if (r.exceptionDetails) throw new Error('JS: ' + (r.exceptionDetails.exception?.description || r.exceptionDetails.text));
  return r.result.value;
}
async function zrzut(nazwa) {
  const r = await cdp('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
  writeFileSync(path.join(wyniki, nazwa + '.png'), Buffer.from(r.data, 'base64'));
}
const klik = (sel) => js(`(() => { const e = document.querySelector(${JSON.stringify(sel)}); if (!e) throw new Error('brak ${sel.replace(/'/g, '')}'); e.click(); return true; })()`);
const ustaw = (sciezka, wartosc) => js(`(() => {
  const e = document.querySelector('[data-sciezka="${sciezka}"]');
  if (!e) throw new Error('brak pola ${sciezka}');
  if (e.type === 'checkbox') e.checked = ${JSON.stringify(wartosc)}; else e.value = ${JSON.stringify(wartosc)};
  e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); return true; })()`);
const zakladka = (id) => klik(`[data-zakladka="${id}"]`);
const widoczne = (sciezka) => js(`!!document.querySelector('[data-pole="${sciezka}"]')`);

try {
  await polacz();
  await cdp('Page.enable');
  await cdp('Runtime.enable');
  await cdp('Browser.setDownloadBehavior', { behavior: 'allow', downloadPath: pobrane }).catch(() =>
    cdp('Page.setDownloadBehavior', { behavior: 'allow', downloadPath: pobrane }));
  await cdp('Page.navigate', { url: formularz });
  await czekaj(1500);
  await zrzut('01-start');
  assert.equal(await js('document.querySelectorAll("#definicje li.pusta-lista").length'), 1, 'pusta lista na starcie');

  // nowa definicja z szablonu „Premia procentowa”
  await klik('#btnNowa');
  await js(`[...document.querySelectorAll('#menuNowa button')].find(b => b.textContent.includes('Premia procentowa')).click()`);
  await czekaj(200);
  assert.ok(await js('document.querySelector(".podsumowanie.blad") !== null'), 'bez nazwy są błędy');
  await ustaw('Nazwa', 'Premia procentowa działu');
  await ustaw('Skrot', 'Prem.dz.');
  await czekaj(200);
  await zrzut('02-ogolne-z-szablonu');
  assert.equal(await js('document.querySelector(".podsumowanie").className'), 'podsumowanie ok', 'po nadaniu nazwy brak błędów');

  // algorytm: szablon ma kreator, wartość miesięczną i procent
  await zakladka('Algorytm');
  await czekaj(200);
  await zrzut('03-algorytm-kreator');
  assert.ok(await widoczne('Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent'), 'procent widoczny');
  assert.ok(await widoczne('Algorytm.KreatorAlgorytmu.Korekta.Chorobowe'), 'pomniejszenia widoczne (wartość miesięczna)');
  assert.equal(await widoczne('Algorytm.KreatorAlgorytmu.Korekta.ZUS'), false, 'pole ZUS ukryte przy licencji złotej');

  // zmiana algorytmu przełącza sekcje
  await ustaw('Algorytm.Typ', 'JakUrlopWypoczynkowy');
  await czekaj(200);
  assert.equal(await widoczne('Algorytm.KreatorAlgorytmu.PodstawaTyp'), false, 'kreator ukryty');
  assert.ok(await widoczne('Algorytm.JakUrlopWypoczynkowy.StdOkresPodstawy'), 'sekcja „jak urlop” widoczna');
  assert.equal(await js(`document.querySelector('[data-sciezka="Algorytm.JakUrlopWypoczynkowy.OkresPodstawy"]').disabled`), true, 'okres podstawy nieedytowalny przy standardowym');
  await ustaw('Algorytm.JakUrlopWypoczynkowy.StdOkresPodstawy', false);
  await czekaj(200);
  assert.equal(await js(`document.querySelector('[data-sciezka="Algorytm.JakUrlopWypoczynkowy.OkresPodstawy"]').disabled`), false, 'okres podstawy edytowalny');
  await zrzut('04-algorytm-jak-urlop');
  await ustaw('Algorytm.Typ', 'KreatorAlgorytmu');

  // nazwy parametrów: przy mnożniku procentowym aktywny tylko „Procent”
  await zakladka('NazwyParametrow');
  await czekaj(200);
  assert.equal(await js(`document.querySelector('[data-sciezka="Algorytm.DodProcent"]').value`), 'Procent premii', 'nazwa z szablonu');
  assert.equal(await js(`document.querySelector('[data-sciezka="Algorytm.DodUlamek"]').disabled`), true, 'ułamek niedostępny');
  await js(`window.scrollTo(0, 0)`);
  await zrzut('04b-nazwy-parametrow');
  await zakladka('Algorytm');

  // licencja srebrna: jedno pole ZUS zamiast czterech
  await js(`(() => { const s = document.getElementById('licencja'); s.value = 'srebrna'; s.dispatchEvent(new Event('change')); })()`);
  await czekaj(200);
  assert.ok(await widoczne('Algorytm.KreatorAlgorytmu.Korekta.ZUS'), 'pole ZUS przy licencji srebrnej');
  await js(`(() => { const s = document.getElementById('licencja'); s.value = 'zlota'; s.dispatchEvent(new Event('change')); })()`);

  // deklaracje: bez pozycji PIT zaliczka nieedytowalna
  await zakladka('Deklaracje');
  await ustaw('Deklaracje.PozycjaPIT', '');
  await czekaj(200);
  assert.equal(await js(`document.querySelector('[data-sciezka="Deklaracje.Zaliczka.Typ"]').disabled`), true);
  await ustaw('Deklaracje.PozycjaPIT', 'PIT-11 1/PIT-4R 1');
  await czekaj(200);
  await zrzut('05-deklaracje');

  // druga definicja z tą samą nazwą – błąd unikalności, pomoc „?”
  await klik('#btnDuplikuj');
  await zakladka('Ogolne');
  await ustaw('Nazwa', 'Premia procentowa działu');
  await czekaj(200);
  assert.ok(await js(`[...document.querySelectorAll('.komunikat.blad')].some(e => e.textContent.includes('innej definicji'))`), 'błąd unikalności nazwy');
  await zrzut('06-blad-unikalnosci');
  await js(`document.querySelector('[data-pole="RodzajZrodla"] .pomoc').click()`);
  await czekaj(200);
  await zrzut('07-pomoc-pola');
  await js(`document.getElementById('dialog').close()`);
  await ustaw('Nazwa', 'Premia procentowa produkcji');
  await czekaj(200);

  // uwagi dla wdrożeniowca
  await zakladka('__uwagi');
  await js(`(() => { const t = document.querySelector('[data-klucz="__uwagi"]'); t.value = 'Procent ustala kierownik co miesiąc.'; t.dispatchEvent(new Event('input')); })()`);
  await js(`(() => { const t = document.querySelector('[data-klucz="__informacja"]'); t.value = 'Sprawdźcie wysokość procentu.'; t.dispatchEvent(new Event('input')); })()`);

  // eksport pliku
  await klik('#btnEksport');
  for (let i = 0; i < 30 && !readdirSync(pobrane).some((f) => f.endsWith('.json')); i++) await czekaj(200);
  const plikJson = readdirSync(pobrane).find((f) => f.endsWith('.json'));
  assert.ok(plikJson, 'pobrany plik JSON');
  const dane = JSON.parse(readFileSync(path.join(pobrane, plikJson), 'utf8'));
  assert.equal(dane.format, 'enova-definicje-elementow-wynagrodzenia');
  assert.equal(dane.definicje.length, 2);
  assert.equal(dane.definicje[0].wartosci.Nazwa, 'Premia procentowa działu');
  assert.equal(dane.definicje[1].uwagi, 'Procent ustala kierownik co miesiąc.');
  assert.equal(dane.definicje[0].wartosci['Algorytm.KreatorAlgorytmu.Wspolczynnik.Typ'], 'Procent');
  assert.deepEqual(dane.definicje.map((d) => d.bledy.length), [0, 0]);
  writeFileSync(path.join(wyniki, 'elementy-wynagrodzenia.json'), JSON.stringify(dane, null, 2));

  // autozapis: po przeładowaniu lista wraca
  await czekaj(500);
  await cdp('Page.reload');
  await czekaj(1500);
  assert.equal(await js('document.querySelectorAll("#definicje li:not(.pusta-lista)").length'), 2, 'szkic przywrócony z localStorage');

  // wczytanie pliku: dołącz do listy
  const doc = await cdp('DOM.getDocument');
  const wej = await cdp('DOM.querySelector', { nodeId: doc.root.nodeId, selector: '#plik' });
  await cdp('DOM.setFileInputFiles', { nodeId: wej.nodeId, files: [path.join(pobrane, plikJson)] });
  await czekaj(500);
  await js(`[...document.querySelectorAll('#dialogPrzyciski button')].find(b => b.value === 'dolacz').click()`);
  await czekaj(300);
  assert.equal(await js('document.querySelectorAll("#definicje li").length'), 4, 'dołączono 2 definicje');
  await zrzut('08-po-wczytaniu');

  // dodatek stażowy z szablonu: zakładka Staż pracy z progami
  await klik('#btnNowa');
  await js(`[...document.querySelectorAll('#menuNowa button')].find(b => b.textContent.includes('Dodatek stażowy')).click()`);
  await czekaj(200);
  await zakladka('StazPracy');
  await czekaj(200);
  assert.equal(await js(`document.querySelectorAll('table.siatka tbody tr').length`), 17, 'progi stażu z szablonu');
  await js(`[...document.querySelectorAll('button')].find(b => b.textContent === '+ Dodaj wiersz').click()`);
  await czekaj(200);
  assert.ok(await js(`[...document.querySelectorAll('.komunikat.blad')].some(e => e.textContent.includes('Wiersz 18'))`), 'pusty wiersz progu = błąd');
  await js(`window.scrollTo(0, 0)`);
  await zrzut('09-staz-progi');

  // wdrożeniowiec: wybór pól dla klienta i pobranie jego formularza
  await klik('#btnKlient');
  await czekaj(200);
  // pytania ankiety: ukryte (wdrożeniowiec odpowie sam), wymagane (bez „Nie wiem”), pytanie o pole pokazane
  const konfPytanie = (id, v) => js(`(() => { const s = document.querySelector('[data-konf-pytanie="${id}"] select'); s.value = '${v}'; s.dispatchEvent(new Event('change')); return true; })()`);
  await konfPytanie('minimalne', 'ukryte');
  await konfPytanie('urlop', 'wymagane');
  await konfPytanie('OkresNaliczania.Opoznienie', 'edycja');
  await czekaj(200);
  assert.ok(await js(`document.querySelectorAll('[data-konf-pytanie]').length > 150`), 'lista pytań ankiety');
  assert.ok(await js(`document.querySelector('[data-konf-pytanie="minimalne"]').classList.contains('konf-ukryte')`), 'pytanie ukryte');
  await js(`window.scrollTo(0, 0)`);
  await zrzut('10a-konfiguracja-pytan');
  await klik('[data-konf-widok="pola"]');
  await czekaj(200);
  const konf = (sciezka, v) => js(`(() => { const s = document.querySelector('[data-konf="${sciezka}"] select'); s.value = '${v}'; s.dispatchEvent(new Event('change')); return true; })()`);
  await konf('Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent', 'edycja');
  await konf('Blokada', 'wymagane');
  await js(`(() => { const t = document.getElementById('wiadomoscKonf'); t.value = 'Prosimy o uzupełnienie do piątku.'; t.dispatchEvent(new Event('input')); })()`);
  await czekaj(400);
  assert.ok(await js(`document.querySelector('.konf-liczniki').textContent.includes('Klient musi uzupełnić: 3')`), 'liczniki konfiguracji');
  await js(`window.scrollTo(0, 0)`);
  await zrzut('10-konfiguracja-klienta');
  await klik('#btnPobierzKlienta');
  for (let i = 0; i < 30 && !readdirSync(pobrane).some((f) => f.endsWith('.html')); i++) await czekaj(200);
  const plikHtml = readdirSync(pobrane).find((f) => f.endsWith('.html'));
  assert.ok(plikHtml, 'pobrany formularz klienta');

  // klient: tylko wskazane pola, jedna strona bez zakładek
  await cdp('Page.navigate', { url: pathToFileURL(path.join(pobrane, plikHtml)).href });
  await czekaj(1500);
  assert.equal(await js('document.querySelectorAll("#definicje li").length'), 5, 'definicje od wdrożeniowca');
  assert.equal(await js('document.querySelector("nav.zakladki")'), null, 'bez zakładek');
  assert.equal(await js('document.getElementById("licencja").disabled && document.getElementById("btnKlient").hidden'), true, 'licencja zablokowana');
  assert.ok(await widoczne('Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent'), 'pole „może zmienić” widoczne');
  assert.equal(await widoczne('Deklaracje.PozycjaPIT'), false, 'pole ukryte');
  assert.equal(await js(`document.querySelector('[data-sciezka="RodzajZrodla"]').disabled`), true, 'pole do wglądu zablokowane');
  assert.equal(await js(`document.querySelector('[data-sciezka="Blokada"]').tagName + '=' + document.querySelector('[data-sciezka="Blokada"]').value`), 'SELECT=nie', 'wymagane Tak/Nie jako lista');
  await zrzut('11-klient');
  await ustaw('Blokada', '');
  await czekaj(200);
  assert.ok(await js(`[...document.querySelectorAll('[data-pole="Blokada"] .komunikat.blad')].some(e => e.textContent === 'Uzupełnij to pole.')`), 'Blokada do uzupełnienia');
  await ustaw('Blokada', 'tak');
  await ustaw('Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent', '12');
  await czekaj(200);
  assert.ok(await js(`document.getElementById('wiadomosc').textContent.includes('do piątku')`), 'wiadomość wdrożeniowca');

  // akceptacja: zablokowana przy błędach (zdublowana nazwa), cofana przy zmianie
  const akceptuj = async () => {
    await js(`(() => { const t = document.querySelector('[data-klucz="__akceptujacy"]'); t.value = 'Kierownik kadr'; t.dispatchEvent(new Event('input')); })()`);
    await klik('#btnAkceptuj');
    await czekaj(200);
  };
  await js(`(() => { const t = document.querySelector('[data-klucz="__akceptujacy"]'); t.value = 'Kierownik kadr'; t.dispatchEvent(new Event('input')); })()`);
  assert.equal(await js(`document.getElementById('btnAkceptuj').disabled`), true, 'akceptacja zablokowana przy błędach');
  await ustaw('Nazwa', 'Premia działu A');
  await czekaj(200);
  await akceptuj();
  assert.ok(await js(`document.querySelector('#definicje li.wybrana .znaczek.zaakc') !== null`), 'znaczek akceptacji');
  await ustaw('Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent', '13');
  await czekaj(200);
  assert.ok(await js(`document.querySelector('#btnAkceptuj') !== null && document.body.textContent.includes('akceptacja nieaktualna')`), 'zmiana cofa akceptację');
  await akceptuj();
  await js(`window.scrollTo(0, document.body.scrollHeight)`);
  await zrzut('12-klient-akceptacja');
  await js(`document.querySelectorAll('#definicje li')[1].click()`);
  await czekaj(200);
  assert.ok(await js(`document.querySelector('.tresc .wiadomosc').textContent.includes('wysokość procentu')`), 'informacja do definicji');
  rmSync(path.join(pobrane, plikJson)); // przeglądarka zapisze plik klienta pod tą samą nazwą
  await klik('#btnEksport');
  await czekaj(300);
  await js(`[...document.querySelectorAll('#dialogPrzyciski button')].find(b => b.value === 'tak')?.click()`); // dane testowe mają zdublowane nazwy
  for (let i = 0; i < 30 && !existsSync(path.join(pobrane, plikJson)); i++) await czekaj(200);
  const odKlienta = JSON.parse(readFileSync(path.join(pobrane, plikJson), 'utf8'));
  const d0 = odKlienta.definicje[0].wartosci;
  assert.equal(d0['Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent'], 13, 'wartość klienta');
  assert.equal(odKlienta.definicje[0].akceptacja.osoba, 'Kierownik kadr', 'akceptacja w pliku');
  assert.equal(odKlienta.definicje[1].akceptacja, null, 'bez akceptacji');
  assert.equal(d0.Blokada, true, 'wybór klienta Tak/Nie');
  assert.equal(d0['Deklaracje.PozycjaPIT'], 'PIT-11 1/PIT-4R 1', 'ukryte pola wdrożeniowca w pliku importu');

  // klient: nowa definicja zaczyna się od ankiety w rozdziałach
  const zaznacz = async (pytanie, odp) => { await klik(`[data-klucz="odp_${pytanie}_${odp}"]`); await czekaj(150); };
  const zaznaczPole = async (sciezka, odp) => { await klik(`[data-klucz="k_${sciezka}_${odp}"]`); await czekaj(150); };
  const dalej = async () => { await klik('#btnDalej'); await czekaj(200); };
  const rozdzial = () => js(`document.querySelector('.kreator .rozdzial-tytul').textContent`);
  const jest = (sel) => js(`!!document.querySelector(${JSON.stringify(sel)})`);
  await klik('#btnNowa');
  await czekaj(300);
  assert.ok(await js(`document.querySelector('.kreator') !== null`), 'kreator ankiety');
  assert.ok(await js(`document.querySelector('#definicje li.wybrana .znaczek').textContent === 'ankieta'`), 'znaczek ankiety na liście');
  assert.equal(await rozdzial(), 'Podstawowe informacje');
  await zrzut('13-ankieta-start');
  await dalej();
  assert.equal(await rozdzial(), 'Podstawowe informacje', 'bez odpowiedzi nie przechodzi dalej');
  assert.ok(await js(`document.getElementById('kreatorInfo').textContent.length > 0`), 'komunikat kreatora');
  assert.equal(await jest('[data-pytanie="Blokada"]'), true, 'pytanie o pole „musi uzupełnić” w ankiecie');
  assert.equal(await jest('[data-pytanie="Kod"]'), false, 'pytanie o pole ukryte');
  await ustaw('Nazwa', 'Premia za jakość');
  await ustaw('Skrot', 'Pr.jak.');
  await zaznacz('rodzaj', 'premia');
  await zaznacz('kogo', 'etat');
  await zaznaczPole('Blokada', 'true');
  await zrzut('14-ankieta-rozdzial-1');
  await dalej();
  assert.equal(await rozdzial(), 'Wysokość składnika');
  await zaznacz('kwota', 'procent');
  assert.ok(await jest('[data-pytanie="procentWartosc"]'), 'pytanie o procent po odpowiedzi „procent”');
  await ustaw('Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent', '15');
  await czekaj(150);
  await dalej();
  assert.equal(await rozdzial(), 'Kiedy jest wypłacany');
  await zaznacz('czestotliwosc', 'miesiac');
  await zaznacz('zaOkres', 'zDolu');
  assert.ok(await jest('[data-pytanie="OkresNaliczania.Opoznienie"]'), 'pytanie o pole pokazane przez wdrożeniowca');
  await dalej();
  assert.equal(await rozdzial(), 'Nieobecności');
  assert.equal(await jest('[data-klucz="odp_urlop_?"]'), false, 'pytanie wymagane bez „Nie wiem”');
  await zaznacz('choroba', 'inaczej');
  await zaznacz('urlop', 'staly');
  await zaznacz('nieplatne', 'tak');
  await zrzut('15-ankieta-nieobecnosci');
  await dalej();
  assert.equal(await rozdzial(), 'Podatek i składki ZUS', 'rozdział bez pytań dla klienta pominięty');
  await zaznacz('podatki', '?');
  await dalej();
  assert.equal(await rozdzial(), 'Uwagi', 'pytanie ukryte (minimalne) pominięte');
  await js(`(() => { const t = document.querySelector('[data-klucz="__uwagi"]'); t.value = 'Premia wg oceny jakości z systemu QMS.'; t.dispatchEvent(new Event('input')); })()`);
  await dalej();
  assert.ok(await js(`document.querySelector('.kreator').textContent.includes('Do ustalenia z wdrożeniowcem')`), 'podsumowanie: nie wiem');
  assert.equal(await js(`document.getElementById('btnZakonczAnkiete').disabled`), false, 'pytanie ukryte nie blokuje zakończenia');
  await zrzut('16-ankieta-podsumowanie');
  await klik('#btnZakonczAnkiete');
  await czekaj(300);
  // po ankiecie: pola ze „szczegółów” (choroba – inaczej) do uzupełnienia, ramka z odpowiedziami
  assert.ok(await js(`document.querySelector('details.ankieta-podsumowanie') !== null`), 'ramka ankiety');
  assert.equal(await widoczne('Nazwa'), false, 'pola z ankiety nie powtarzają się na stronie definicji');
  assert.ok(await widoczne('Algorytm.KreatorAlgorytmu.Korekta.Chorobowe'), 'szczegóły choroby widoczne u klienta');
  assert.equal(await js(`document.querySelector('[data-sciezka="Algorytm.KreatorAlgorytmu.Korekta.Chorobowe"]').disabled`), false, 'szczegóły edytowalne');
  await ustaw('Algorytm.KreatorAlgorytmu.Korekta.Chorobowe', 'ZaKażdyDzień');
  await czekaj(200);
  assert.ok(await js(`document.querySelector('[data-pole="Algorytm.KreatorAlgorytmu.Korekta.Chorobowe"] .pochodzenie.reczne') !== null`), 'zmiana ręczna oznaczona');
  await js(`document.querySelector('details.ankieta-podsumowanie').open = true; window.scrollTo(0, 0)`);
  await zrzut('17-po-ankiecie');
  // zmiana odpowiedzi (skok do rozdziału) nie nadpisuje zmiany ręcznej
  await klik('#btnZmienOdpowiedzi');
  await czekaj(200);
  await js(`[...document.querySelectorAll('.kroki button')].find(b => b.textContent === 'Kiedy jest wypłacany').click()`);
  await czekaj(200);
  await zaznacz('czestotliwosc', 'kwartal');
  await js(`[...document.querySelectorAll('.kroki button')].find(b => b.textContent === 'Podsumowanie').click()`);
  await czekaj(200);
  await klik('#btnZakonczAnkiete');
  await czekaj(300);
  rmSync(path.join(pobrane, plikJson));
  await klik('#btnEksport');
  await czekaj(300);
  await js(`[...document.querySelectorAll('#dialogPrzyciski button')].find(b => b.value === 'tak')?.click()`);
  for (let i = 0; i < 30 && !existsSync(path.join(pobrane, plikJson)); i++) await czekaj(200);
  const zAnkiety = JSON.parse(readFileSync(path.join(pobrane, plikJson), 'utf8')).definicje.find((x) => x.wartosci.Nazwa === 'Premia za jakość');
  const wa = zAnkiety.wartosci;
  assert.equal(wa['Algorytm.KreatorAlgorytmu.Wspolczynnik.Typ'], 'Procent', 'ankieta: procent');
  assert.equal(wa['Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent'], 15, 'ankieta: wartość procentu');
  assert.equal(wa['Algorytm.KreatorAlgorytmu.PodstawaTyp'], 'ZasadniczeNominalne', 'ankieta: od zasadniczego');
  assert.equal(wa['OkresNaliczania.Ilosc'], 3, 'ankieta: zmieniona odpowiedź – co kwartał');
  assert.equal(wa['Algorytm.KreatorAlgorytmu.Korekta.Chorobowe'], 'ZaKażdyDzień', 'zmiana ręczna zachowana');
  assert.equal(wa['Nieobecnosci.Urlop.Typ'], 'WliczaćJakZasadnicze', 'urlop: procent od zasadniczego');
  assert.equal(wa.Blokada, true, 'odpowiedź kafelkiem Tak/Nie');
  assert.deepEqual(zAnkiety.bledy, [], 'definicja z ankiety bez błędów');
  assert.equal(zAnkiety.uwagi, 'Premia wg oceny jakości z systemu QMS.');
  assert.equal(zAnkiety.ankieta.wersja, '2.0');
  assert.equal(zAnkiety.ankieta.odpowiedzi.podatki, '?');
  assert.deepEqual(zAnkiety.ankieta.doWyjasnienia, ['Czy od składnika pobiera się podatek i składki ZUS?']);
  assert.deepEqual(zAnkiety.ankieta.reczne.sort(), ['Algorytm.KreatorAlgorytmu.Korekta.Chorobowe', 'Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent', 'Blokada', 'Nazwa', 'Skrot']);
  writeFileSync(path.join(wyniki, 'elementy-wynagrodzenia-ankieta.json'), JSON.stringify(zAnkiety, null, 2));
  console.log('E2E OK – zrzuty i plik w ' + wyniki);
} catch (e) {
  await zrzut('99-blad').catch(() => {});
  console.error('E2E BŁĄD:', e.message);
  process.exitCode = 1;
} finally {
  ws?.close();
  proc.kill();
}
