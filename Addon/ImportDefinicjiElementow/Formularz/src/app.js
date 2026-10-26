/*
 * Formularz definicji elementów wynagrodzenia – interfejs.
 * Schemat (wbudowany przy buildzie) opisuje zakładki, sekcje, pola i reguły; reguły ocenia Warunki (warunki.js).
 */
(function () {
  'use strict';

  // Kopia strony sprzed pierwszego renderu – z niej powstaje formularz dla klienta.
  var ZRODLO_HTML = '<!doctype html>\n' + document.documentElement.outerHTML;
  var W = window.Warunki;
  var SCHEMAT = JSON.parse(document.getElementById('schemat').textContent);
  // Konfiguracja wdrożeniowca wstawiona do formularza dla klienta; null = tryb wdrożeniowca (pełny formularz).
  var KONF = JSON.parse(document.getElementById('konfiguracja').textContent || 'null');
  var KLIENT = !!KONF;
  var FORMAT = 'enova-definicje-elementow-wynagrodzenia';
  var KLUCZ_SZKICU = 'enova-definicje-elementow:' + SCHEMAT.wersjaSchematu + (KLIENT ? ':klient:' + KONF.id : '');
  var ZAKLADKA_UWAGI = '__uwagi';
  var STANY_KLIENTA = [
    { id: 'ukryte', etykieta: 'Ukryte' },
    { id: 'wglad', etykieta: 'Do wglądu' },
    { id: 'edycja', etykieta: 'Klient może zmienić' },
    { id: 'wymagane', etykieta: 'Klient musi uzupełnić' }
  ];

  var stan = wczytajSzkic() || (KLIENT
    ? { ustawienia: { licencja: KONF.licencja }, definicje: kopia(KONF.definicje), wybrana: null, zakladka: null }
    : { ustawienia: { licencja: 'zlota' }, definicje: [], wybrana: null, zakladka: null });
  if (KLIENT) stan.ustawienia.licencja = KONF.licencja;
  else if (!stan.klient) stan.klient = { mozeDodawac: true, pola: { RodzajZrodla: 'wglad', Nazwa: 'wymagane', Skrot: 'wymagane' } };
  var bledyCache = null;

  function dlaKlienta(sciezka) { return KONF.pola[sciezka] || 'ukryte'; }
  function klientEdytuje(sciezka) { var s = dlaKlienta(sciezka); return s === 'edycja' || s === 'wymagane'; }

  /* ---------------- narzędzia ---------------- */

  function el(tag, atrybuty) {
    var e = document.createElement(tag);
    if (atrybuty) Object.keys(atrybuty).forEach(function (k) {
      var v = atrybuty[k];
      if (v === undefined || v === null || v === false) return;
      if (k === 'tekst') e.textContent = v;
      else if (k === 'klasa') e.className = v;
      else if (k.indexOf('on') === 0) e.addEventListener(k.slice(2), v);
      else if (k in e && k !== 'list') e[k] = v;
      else e.setAttribute(k, v === true ? '' : v);
    });
    for (var i = 2; i < arguments.length; i++) {
      var d = arguments[i];
      if (d === null || d === undefined || d === false) continue;
      if (Array.isArray(d)) d.forEach(function (x) { if (x) e.appendChild(typeof x === 'string' ? document.createTextNode(x) : x); });
      else e.appendChild(typeof d === 'string' ? document.createTextNode(d) : d);
    }
    return e;
  }

  function noweId() {
    if (window.crypto && crypto.randomUUID) return crypto.randomUUID();
    return 'd' + Date.now().toString(36) + Math.random().toString(36).slice(2, 8);
  }

  function kopia(o) { return JSON.parse(JSON.stringify(o)); }

  function wybrana() {
    return stan.definicje.filter(function (d) { return d.id === stan.wybrana; })[0] || null;
  }

  function enumEtykieta(nazwaEnuma, wartosc) {
    var e = SCHEMAT.enumy[nazwaEnuma];
    var w = e && e.wartosci.filter(function (x) { return x.nazwa === wartosc; })[0];
    return w ? w.etykieta : wartosc;
  }

  function nazwaDefinicji(d) {
    return (d.wartosci && d.wartosci.Nazwa) || '(bez nazwy)';
  }

  function dataTekst(iso) {
    return new Date(iso).toLocaleString('pl-PL', { dateStyle: 'short', timeStyle: 'short' });
  }

  /** Akceptacja klienta obowiązuje, dopóki eksportowane wartości i uwagi są takie jak w chwili akceptacji. */
  function odcisk(d) {
    return JSON.stringify([W.doEksportu(SCHEMAT, d, stan.ustawienia), d.uwagi || '']);
  }

  function zaakceptowana(d) {
    return !!(d.akceptacja && d.akceptacja.odcisk === odcisk(d));
  }

  function liczbaNaTekst(v) {
    return (v === undefined || v === null) ? '' : String(v).replace('.', ',');
  }

  function tekstNaLiczbe(t, calkowita) {
    var s = String(t).replace(/\s| /g, '').replace(',', '.');
    if (s === '') return undefined;
    if (calkowita ? /^-?\d+$/.test(s) : /^-?\d+(\.\d+)?$/.test(s)) return Number(s);
    return t; // zostaje tekst – walidacja pokaże błąd typu
  }

  /* ---------------- szkic w przeglądarce ---------------- */

  function wczytajSzkic() {
    try {
      var t = window.localStorage.getItem(KLUCZ_SZKICU);
      if (!t) return null;
      var s = JSON.parse(t);
      return s && Array.isArray(s.definicje) ? s : null;
    } catch (e) {
      return null;
    }
  }

  var timerZapisu = null;
  function zapiszSzkic() {
    clearTimeout(timerZapisu);
    timerZapisu = setTimeout(function () {
      var info = document.getElementById('autozapis');
      try {
        window.localStorage.setItem(KLUCZ_SZKICU, JSON.stringify(stan));
        info.textContent = 'Szkic zapisany w tej przeglądarce o ' + new Date().toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' }) + '. Do przekazania użyj „Zapisz plik dla wdrożeniowca”.';
      } catch (e) {
        info.textContent = 'Autozapis w przeglądarce jest niedostępny – zapisuj plik regularnie.';
      }
    }, 300);
  }

  /* ---------------- walidacja ---------------- */

  function bledy() {
    if (bledyCache) return bledyCache;
    bledyCache = {};
    stan.definicje.forEach(function (d) {
      bledyCache[d.id] = KLIENT ? bledyKlienta(d) : W.waliduj(SCHEMAT, d, stan.definicje, stan.ustawienia);
    });
    return bledyCache;
  }

  /** Klient poprawia tylko swoje pola; błędy pól wdrożeniowca dostają poziom „wdrozeniowiec” (nie blokują). */
  function bledyKlienta(d) {
    var k = new W.Kontekst(SCHEMAT, d, stan.ustawienia);
    var wynik = W.waliduj(SCHEMAT, d, stan.definicje, stan.ustawienia).map(function (b) {
      return klientEdytuje(b.sciezka) || b.poziom !== 'blad' ? b : { sciezka: b.sciezka, poziom: 'wdrozeniowiec', komunikat: b.komunikat };
    }).filter(function (b) { return b.poziom === 'wdrozeniowiec' || klientEdytuje(b.sciezka); });
    Object.keys(KONF.pola).forEach(function (s) {
      if (KONF.pola[s] !== 'wymagane' || !k.stan(s).aktywne || !W.pusty(k.uzytkownika(s))) return;
      if (!wynik.some(function (b) { return b.sciezka === s && b.poziom === 'blad'; }))
        wynik.push({ sciezka: s, poziom: 'blad', komunikat: 'Uzupełnij to pole.' });
    });
    return wynik;
  }

  function liczBledy(lista, poziom) {
    return lista.filter(function (b) { return b.poziom === poziom; }).length;
  }

  function zmieniono(pelnyRender) {
    bledyCache = null;
    zapiszSzkic();
    renderLista();
    if (pelnyRender) renderEdytor();
    else odswiezKomunikaty();
  }

  /* ---------------- lista definicji ---------------- */

  function renderLista() {
    var ul = document.getElementById('definicje');
    ul.textContent = '';
    if (!stan.definicje.length) {
      ul.appendChild(el('li', { klasa: 'pusta-lista' }, 'Brak definicji. Kliknij „+ Nowa definicja”, aby dodać pierwszą (najwygodniej z szablonu).'));
    }
    var b = bledy();
    stan.definicje.forEach(function (d) {
      var lb = b[d.id] || [];
      var nb = liczBledy(lb, 'blad'), no = liczBledy(lb, 'ostrzezenie');
      var rodzaj = d.wartosci && d.wartosci.RodzajZrodla ? enumEtykieta('RodzajŹródłaWypłaty', d.wartosci.RodzajZrodla) : '';
      ul.appendChild(el('li', {
        klasa: d.id === stan.wybrana && !stan.widok ? 'wybrana' : '',
        onclick: function () { stan.wybrana = d.id; stan.widok = null; renderLista(); renderEdytor(); zapiszSzkic(); }
      },
        el('div', { klasa: 'nazwa' }, el('div', { tekst: nazwaDefinicji(d) }), el('div', { klasa: 'rodzaj', tekst: rodzaj })),
        nb ? el('span', { klasa: 'znaczek blad', tekst: nb + ' bł.', title: 'Liczba błędów' })
          : zaakceptowana(d) ? el('span', { klasa: 'znaczek zaakc', tekst: '✓ zaakc.', title: 'Zaakceptowana przez ' + d.akceptacja.osoba + ', ' + dataTekst(d.akceptacja.data) })
          : no ? el('span', { klasa: 'znaczek ostrz', tekst: no + ' ostrz.' })
            : el('span', { klasa: 'znaczek ok', tekst: '✓', title: 'Bez błędów' })
      ));
    });
    var jest = !!wybrana();
    document.getElementById('btnDuplikuj').disabled = !jest;
    document.getElementById('btnUsun').disabled = !jest;
  }

  function dodajDefinicje(szablon) {
    var d = {
      id: noweId(),
      szablon: szablon ? szablon.id : null,
      uwagi: szablon && szablon.uwagi ? szablon.uwagi : '',
      wartosci: szablon ? kopia(szablon.wartosci) : { RodzajZrodla: 'Dodatek' }
    };
    stan.definicje.push(d);
    stan.wybrana = d.id;
    stan.widok = null;
    stan.zakladka = null;
    zmieniono(true);
    var pole = document.querySelector('[data-sciezka="Nazwa"]');
    if (pole) pole.focus();
  }

  function menuNowa() {
    var m = document.getElementById('menuNowa');
    m.textContent = '';
    m.appendChild(el('button', { type: 'button', onclick: function () { zamknijMenu(); dodajDefinicje(null); } },
      el('strong', { tekst: 'Pusta definicja' }), el('small', { tekst: 'Wszystkie pola z wartościami domyślnymi enova.' })));
    (SCHEMAT.szablony || []).forEach(function (s) {
      m.appendChild(el('button', { type: 'button', onclick: function () { zamknijMenu(); dodajDefinicje(s); } },
        el('strong', { tekst: 'Z szablonu: ' + s.etykieta }), el('small', { tekst: s.opis || '' })));
    });
  }

  function zamknijMenu() {
    document.getElementById('menuNowa').hidden = true;
    document.getElementById('btnNowa').setAttribute('aria-expanded', 'false');
  }

  /* ---------------- edytor ---------------- */

  function zakladkiWidoczne(k) {
    return SCHEMAT.zakladki.filter(function (z) { return k.zakladkaWidoczna(z); });
  }

  /** Ścieżki pól widocznych na zakładce (do liczenia błędów). */
  function polaZakladki(z, k) {
    var wynik = {};
    z.sekcje.forEach(function (s) {
      if (!k.sekcjaWidoczna(s)) return;
      s.pola.forEach(function (p) { if (k.stan(p).widoczne) wynik[p] = true; });
    });
    return wynik;
  }

  function zakladkaPola(sciezka, k) {
    var z = zakladkiWidoczne(k).filter(function (z) { return polaZakladki(z, k)[sciezka]; })[0];
    return z ? z.id : null;
  }

  function renderEdytor() {
    var kont = document.getElementById('edytor');
    var fokus = document.activeElement && document.activeElement.getAttribute && document.activeElement.getAttribute('data-klucz');
    var przewiniecie = window.scrollY;
    if (!KLIENT && stan.widok === 'konfiguracja') { renderKonfiguracja(); return; }
    kont.textContent = '';
    var d = wybrana();
    if (!d) {
      kont.appendChild(el('div', { klasa: 'edytor-pusty' },
        el('p', { tekst: 'Wybierz definicję z listy albo dodaj nową.' }),
        el('p', { tekst: 'Każda definicja opisuje jeden składnik wynagrodzenia (dodatek, premię, potrącenie…). Wypełniony plik przekaż wdrożeniowcowi – zaimportuje go do enova.' })));
      return;
    }
    var k = new W.Kontekst(SCHEMAT, d, stan.ustawienia);
    var zakladki = zakladkiWidoczne(k);
    if (stan.zakladka !== ZAKLADKA_UWAGI && !zakladki.some(function (z) { return z.id === stan.zakladka; }))
      stan.zakladka = zakladki.length ? zakladki[0].id : ZAKLADKA_UWAGI;

    var lb = bledy()[d.id] || [];
    var szablon = d.szablon && (SCHEMAT.szablony || []).filter(function (s) { return s.id === d.szablon; })[0];
    kont.appendChild(el('div', { klasa: 'edytor-naglowek' },
      el('h2', { tekst: nazwaDefinicji(d) }),
      szablon ? el('div', { klasa: 'szablon', tekst: 'Utworzono z szablonu: ' + szablon.etykieta }) : null));
    kont.appendChild(el('div', { id: 'podsumowanie' }));
    if (KLIENT) kont.appendChild(trescKlienta(d, k));
    else renderZakladki(kont, d, k, zakladki);
    odswiezKomunikaty();
    window.scrollTo(0, przewiniecie);
    if (fokus) {
      var f = kont.querySelector('[data-klucz="' + fokus.replace(/"/g, '\\"') + '"]');
      if (f) f.focus();
    }
  }

  function trescUwag(d) {
    return [
      el('p', { klasa: 'opis-zakladki', tekst: 'Opisz wszystko, czego nie da się ustawić polami: zasady regulaminu, wyjątki, sposób liczenia (np. „premia 10% od zasadniczego, nie przysługuje przy nieobecności nieusprawiedliwionej”), wątpliwości.' }),
      el('div', { klasa: 'kontrolka' }, el('textarea', {
        'data-klucz': '__uwagi', value: d.uwagi || '', 'aria-label': 'Uwagi dla wdrożeniowca',
        oninput: function (e) { d.uwagi = e.target.value; zapiszSzkic(); },
        onchange: function () { zmieniono(true); } // zmiana uwag cofa akceptację
      }))
    ];
  }

  function opisAkceptacji(d) {
    if (zaakceptowana(d)) return 'Zaakceptowana przez ' + d.akceptacja.osoba + ' – ' + dataTekst(d.akceptacja.data) + '.';
    if (d.akceptacja) return 'Definicję zmieniono po akceptacji (' + d.akceptacja.osoba + ', ' + dataTekst(d.akceptacja.data) + ') – akceptacja nieaktualna.';
    return 'Niezaakceptowana.';
  }

  /** Wdrożeniowiec: informacja dla klienta, uwagi klienta, stan akceptacji. */
  function trescUwagWdrozeniowca(d) {
    return [
      el('h3', { tekst: 'Informacja dla klienta' }),
      el('p', { klasa: 'opis-zakladki', tekst: 'Klient zobaczy ją na górze tej definicji (np. co ustaliliście, co ma sprawdzić).' }),
      el('div', { klasa: 'kontrolka' }, el('textarea', {
        'data-klucz': '__informacja', value: d.informacja || '', 'aria-label': 'Informacja dla klienta',
        oninput: function (e) { d.informacja = e.target.value; zapiszSzkic(); }
      })),
      el('h3', { tekst: 'Uwagi klienta dla wdrożeniowca' })
    ].concat(trescUwag(d), [
      el('h3', { tekst: 'Akceptacja klienta' }),
      el('p', { klasa: zaakceptowana(d) ? 'akceptacja-ok' : 'opis-zakladki', tekst: opisAkceptacji(d) + ' Import w enova ostrzega przy definicjach niezaakceptowanych.' })
    ]);
  }

  function sekcjaAkceptacji(d) {
    var sec = el('section', { klasa: 'grupa-klienta' }, el('h3', { tekst: 'Akceptacja definicji' }));
    if (zaakceptowana(d)) {
      sec.appendChild(el('p', { klasa: 'akceptacja-ok' }, opisAkceptacji(d) + ' Każda zmiana definicji cofa akceptację. ',
        el('button', { type: 'button', klasa: 'link', tekst: 'Cofnij akceptację', onclick: function () { delete d.akceptacja; zmieniono(true); } })));
      return sec;
    }
    if (d.akceptacja) sec.appendChild(el('p', { klasa: 'komunikat ostrzezenie', tekst: opisAkceptacji(d) + ' Zaakceptuj ją ponownie.' }));
    var bl = liczBledy(bledy()[d.id] || [], 'blad');
    sec.appendChild(el('p', { klasa: 'opis-zakladki', tekst: 'Zaakceptuj definicję, gdy jest ostatecznie omówiona z wdrożeniowcem – wdrożeniowiec zobaczy, kto i kiedy ją zaakceptował.' }));
    var btn = el('button', {
      type: 'button', klasa: 'glowny', id: 'btnAkceptuj', tekst: 'Akceptuję definicję', disabled: !!bl || !(stan.akceptujacy || '').trim(),
      onclick: function () { d.akceptacja = { osoba: stan.akceptujacy.trim(), data: new Date().toISOString(), odcisk: odcisk(d) }; zmieniono(true); }
    });
    sec.appendChild(el('div', { klasa: 'akceptacja-wiersz kontrolka' },
      el('input', {
        type: 'text', 'data-klucz': '__akceptujacy', value: stan.akceptujacy || '', placeholder: 'Imię i nazwisko akceptującego', 'aria-label': 'Imię i nazwisko akceptującego',
        oninput: function (e) { stan.akceptujacy = e.target.value; btn.disabled = !!bl || !e.target.value.trim(); zapiszSzkic(); }
      }), btn));
    if (bl) sec.appendChild(el('div', { klasa: 'komunikat blad', tekst: 'Najpierw popraw błędy w definicji.' }));
    return sec;
  }

  /** Klient: jedna strona – tylko pola wskazane przez wdrożeniowca, pogrupowane jak zakładki enova (bez klikania zakładek). */
  function trescKlienta(d, k) {
    var tresc = el('div', { klasa: 'tresc' });
    if (d.informacja) tresc.appendChild(el('div', { klasa: 'wiadomosc' }, el('strong', { tekst: 'Informacja od wdrożeniowca: ' }), d.informacja));
    var byly = {};
    zakladkiWidoczne(k).forEach(function (z) {
      var sekcje = [];
      z.sekcje.forEach(function (s) {
        if (!k.sekcjaWidoczna(s)) return;
        var pola = s.pola.filter(function (p) {
          if (byly[p] || dlaKlienta(p) === 'ukryte' || !k.stan(p).widoczne) return false;
          return (byly[p] = true);
        });
        if (!pola.length) return;
        var fs = el('fieldset', null, el('legend', { tekst: s.etykieta }));
        if (s.opis) fs.appendChild(el('p', { klasa: 'opis-sekcji', tekst: s.opis }));
        pola.forEach(function (p) { fs.appendChild(renderPole(d, k, p)); });
        sekcje.push(fs);
      });
      if (sekcje.length) tresc.appendChild(el('section', { klasa: 'grupa-klienta' }, el('h3', { tekst: z.etykieta }), sekcje));
    });
    if (!tresc.querySelector('fieldset'))
      tresc.appendChild(el('p', { klasa: 'opis-zakladki', tekst: 'W tej definicji nie ma pól do uzupełnienia – ewentualne uwagi wpisz poniżej.' }));
    tresc.appendChild(el('section', { klasa: 'grupa-klienta' }, el('h3', { tekst: 'Uwagi dla wdrożeniowca' }), trescUwag(d)));
    tresc.appendChild(sekcjaAkceptacji(d));
    return tresc;
  }

  function renderZakladki(kont, d, k, zakladki) {
    var nav = el('nav', { klasa: 'zakladki', role: 'tablist' });
    zakladki.concat([{ id: ZAKLADKA_UWAGI, etykieta: 'Uwagi i akceptacja' }]).forEach(function (z) {
      nav.appendChild(el('button', {
        type: 'button', role: 'tab', klasa: z.id === stan.zakladka ? 'aktywna' : '', 'data-zakladka': z.id,
        'aria-selected': z.id === stan.zakladka ? 'true' : 'false',
        onclick: function () { stan.zakladka = z.id; zapiszSzkic(); renderEdytor(); }
      }, z.etykieta, el('span', { klasa: 'znaczek blad', 'data-licznik': z.id, hidden: true })));
    });
    kont.appendChild(nav);

    var tresc = el('div', { klasa: 'tresc', role: 'tabpanel' });
    if (stan.zakladka === ZAKLADKA_UWAGI) {
      trescUwagWdrozeniowca(d).forEach(function (x) { tresc.appendChild(x); });
    } else {
      var z = SCHEMAT.zakladki.filter(function (x) { return x.id === stan.zakladka; })[0];
      if (z.opis) tresc.appendChild(el('p', { klasa: 'opis-zakladki', tekst: z.opis }));
      z.sekcje.forEach(function (s) {
        if (!k.sekcjaWidoczna(s)) return;
        var pola = s.pola.filter(function (p) { return k.stan(p).widoczne; });
        if (!pola.length) return;
        var fs = el('fieldset', null, el('legend', { tekst: s.etykieta }));
        if (s.opis) fs.appendChild(el('p', { klasa: 'opis-sekcji', tekst: s.opis }));
        pola.forEach(function (p) { fs.appendChild(renderPole(d, k, p)); });
        tresc.appendChild(fs);
      });
    }
    kont.appendChild(tresc);
  }

  /** Komunikaty walidacji bez przebudowy kontrolek (po wpisaniu znaku). */
  function odswiezKomunikaty() {
    var d = wybrana();
    if (!d) return;
    var k = new W.Kontekst(SCHEMAT, d, stan.ustawienia);
    var lb = bledy()[d.id] || [];
    document.querySelectorAll('.pole[data-pole]').forEach(function (div) {
      var s = div.getAttribute('data-pole');
      var kom = div.querySelector('.komunikaty');
      kom.textContent = '';
      var wl = lb.filter(function (b) { return b.sciezka === s; });
      div.classList.toggle('ma-blad', wl.some(function (b) { return b.poziom === 'blad'; }));
      wl.forEach(function (b) { kom.appendChild(el('div', { klasa: 'komunikat ' + b.poziom, tekst: b.komunikat })); });
    });
    zakladkiWidoczne(k).forEach(function (z) {
      var pz = polaZakladki(z, k);
      var n = lb.filter(function (b) { return b.poziom === 'blad' && pz[b.sciezka]; }).length;
      var zn = document.querySelector('[data-licznik="' + z.id + '"]');
      if (zn) { zn.hidden = !n; zn.textContent = n; }
    });
    var pods = document.getElementById('podsumowanie');
    if (!pods) return;
    pods.textContent = '';
    var bl = lb.filter(function (b) { return b.poziom === 'blad'; });
    var nw = liczBledy(lb, 'wdrozeniowiec');
    if (!bl.length) {
      pods.className = 'podsumowanie ok';
      pods.textContent = 'Definicja nie ma błędów.' + (liczBledy(lb, 'ostrzezenie') ? ' Ostrzeżenia: ' + liczBledy(lb, 'ostrzezenie') + '.' : '')
        + (nw ? ' Ustawienia wdrożeniowca do sprawdzenia przez niego: ' + nw + ' – nie musisz ich poprawiać.' : '');
      return;
    }
    pods.className = 'podsumowanie blad';
    pods.appendChild(document.createTextNode('Do poprawienia: ' + bl.length + (bl.length === 1 ? ' błąd' : ' błędów')));
    var ul = el('ul');
    bl.forEach(function (b) {
      var pole = SCHEMAT.pola[b.sciezka];
      ul.appendChild(el('li', null, el('button', {
        type: 'button', klasa: 'link', tekst: (pole ? pole.etykieta : b.sciezka) + ': ' + b.komunikat,
        onclick: function () { przejdzDoPola(b.sciezka); }
      })));
    });
    pods.appendChild(ul);
  }

  function przejdzDoPola(sciezka) {
    var d = wybrana();
    var k = new W.Kontekst(SCHEMAT, d, stan.ustawienia);
    var z = KLIENT ? null : zakladkaPola(sciezka, k);
    if (z && z !== stan.zakladka) { stan.zakladka = z; renderEdytor(); }
    var f = document.querySelector('[data-sciezka="' + sciezka + '"]');
    if (f) { f.scrollIntoView({ block: 'center' }); f.focus(); }
  }

  /* ---------------- pole ---------------- */

  function renderPole(d, k, sciezka) {
    var pole = SCHEMAT.pola[sciezka];
    var st = k.stan(sciezka);
    var wglad = KLIENT && !klientEdytuje(sciezka);
    if (wglad) st = Object.assign({}, st, { edytowalne: false, wymagane: false });
    else if (KLIENT && dlaKlienta(sciezka) === 'wymagane' && st.aktywne) st = Object.assign({}, st, { wymagane: true });
    var id = 'p_' + sciezka.replace(/[^\w]/g, '_');
    var wartosc = k.wartosc(sciezka);
    var ustaw = function (v, pelny) {
      if (W.pusty(v)) delete d.wartosci[sciezka];
      else d.wartosci[sciezka] = v;
      zmieniono(pelny);
    };
    var div = el('div', { klasa: 'pole', 'data-pole': sciezka },
      el('div', { klasa: 'etykieta' },
        el('label', { htmlFor: id }, pole.etykieta, st.wymagane ? el('span', { klasa: 'wym', tekst: '*', title: 'Pole wymagane' }) : null),
        el('button', { type: 'button', klasa: 'pomoc', tekst: '?', title: 'Opis pola', onclick: function () { pokazPomoc(sciezka, k); } })));
    var kontr = el('div', { klasa: 'kontrolka' });
    kontr.appendChild(kontrolka(pole, st, wartosc, id, ustaw, d));
    if (pole.enum && wartosc !== undefined) {
      var ew = SCHEMAT.enumy[pole.enum].wartosci.filter(function (x) { return x.nazwa === wartosc; })[0];
      if (ew && ew.opis) kontr.appendChild(el('div', { klasa: 'opis-wartosci', tekst: ew.opis }));
    }
    if (wglad) kontr.appendChild(el('div', { klasa: 'niedostepne', tekst: 'Tylko do wglądu – to ustawienie przygotował wdrożeniowiec. Jeśli u Was jest inaczej, napisz w uwagach.' }));
    if (st.powod) kontr.appendChild(el('div', { klasa: 'niedostepne', tekst: 'Pole nieaktywne – ' + st.powod + '. Obowiązuje wartość domyślna.' }));
    else if (pole.podpowiedz) kontr.appendChild(el('div', { klasa: 'podpowiedz', tekst: pole.podpowiedz }));
    kontr.appendChild(el('div', { klasa: 'komunikaty' }));
    div.appendChild(kontr);
    return div;
  }

  function kontrolka(pole, st, wartosc, id, ustaw, d) {
    var wyl = !st.edytowalne;
    var wsp = { id: id, 'data-sciezka': pole.sciezka, 'data-klucz': pole.sciezka, disabled: wyl };
    switch (pole.typ) {
      case 'bool':
        if (KLIENT && !wyl && dlaKlienta(pole.sciezka) === 'wymagane') { // klient musi świadomie wybrać Tak/Nie
          var b = d.wartosci[pole.sciezka];
          return el('select', Object.assign({ onchange: function (e) { ustaw(e.target.value === '' ? undefined : e.target.value === 'tak', true); } }, wsp),
            el('option', { value: '', tekst: '— wybierz —' }),
            el('option', { value: 'tak', tekst: 'Tak', selected: b === true }),
            el('option', { value: 'nie', tekst: 'Nie', selected: b === false }));
        }
        return el('label', { klasa: 'bool' }, el('input', Object.assign({
          type: 'checkbox', checked: wartosc === true,
          onchange: function (e) { ustaw(e.target.checked, true); }
        }, wsp)), 'Tak');
      case 'enum':
        return selectEnum(pole, st, wartosc, wsp, ustaw);
      case 'int': case 'decimal': case 'percent': case 'currency': {
        var inp = el('input', Object.assign({
          type: 'text', inputMode: pole.typ === 'int' ? 'numeric' : 'decimal', value: typeof wartosc === 'number' ? liczbaNaTekst(wartosc) : (wartosc || ''),
          oninput: function (e) { ustaw(tekstNaLiczbe(e.target.value, pole.typ === 'int'), false); },
          onchange: function () { zmieniono(true); }
        }, wsp));
        var jedn = pole.typ === 'percent' ? '%' : pole.typ === 'currency' ? 'zł' : null;
        return jedn ? el('span', { klasa: 'z-jednostka' }, inp, jedn) : inp;
      }
      case 'lista':
        return siatka(pole, st, wartosc, ustaw);
      default: {
        var lista = null;
        if (st.dozwolone && pole.typ !== 'slownik') lista = st.dozwolone;
        if (lista) return selectTekst(lista, wartosc, wsp, ustaw);
        var dl = pole.slownik ? datalista(pole.slownik, d) : null;
        var t = el('input', Object.assign({
          type: 'text', value: wartosc || '', maxLength: pole.maxDlugosc || undefined,
          placeholder: pole.typ === 'fraction' ? 'np. 1/2' : pole.typ === 'time' ? 'gg:mm' : pole.slownik ? 'wybierz z listy lub wpisz' : '',
          oninput: function (e) { ustaw(e.target.value, false); },
          onchange: function () { zmieniono(true); }
        }, wsp));
        if (dl) t.setAttribute('list', dl);
        return t;
      }
    }
  }

  function selectEnum(pole, st, wartosc, wsp, ustaw) {
    var dozw = st.dozwolone || [];
    var s = el('select', Object.assign({ onchange: function (e) { ustaw(e.target.value || undefined, true); } }, wsp));
    if (wartosc === undefined || wartosc === null) s.appendChild(el('option', { value: '', tekst: '— wybierz —' }));
    var lista = dozw.slice();
    if (wartosc && lista.indexOf(wartosc) < 0) lista.push(wartosc);
    lista.forEach(function (n) {
      var et = enumEtykieta(pole.enum, n) + (dozw.indexOf(n) < 0 ? ' (niedozwolona)' : '');
      s.appendChild(el('option', { value: n, tekst: et, selected: n === wartosc }));
    });
    return s;
  }

  function selectTekst(lista, wartosc, wsp, ustaw) {
    var s = el('select', Object.assign({ onchange: function (e) { ustaw(e.target.value || undefined, true); } }, wsp));
    s.appendChild(el('option', { value: '', tekst: '— wybierz —' }));
    var l = lista.slice();
    if (wartosc && l.indexOf(wartosc) < 0) l.push(wartosc);
    l.forEach(function (v) { s.appendChild(el('option', { value: v, tekst: v, selected: v === wartosc })); });
    return s;
  }

  /** Lista podpowiedzi słownika: wartości standardowe enova (+ nazwy definicji z pliku). */
  function datalista(slownik, d) {
    var sl = SCHEMAT.slowniki[slownik];
    if (!sl) return null;
    var id = 'dl_' + slownik;
    var stara = document.getElementById(id);
    if (stara) stara.remove();
    var dl = el('datalist', { id: id });
    var byly = {};
    (sl.wartosciStandardowe || []).forEach(function (w) {
      byly[w.klucz] = true;
      dl.appendChild(el('option', { value: w.klucz, label: w.opis || '' }));
    });
    if (slownik === 'DefinicjaElementu') stan.definicje.forEach(function (x) {
      var n = x.wartosci && x.wartosci.Nazwa;
      if (n && x !== d && !byly[n]) dl.appendChild(el('option', { value: n, label: 'z tego pliku' }));
    });
    document.body.appendChild(dl);
    return id;
  }

  function siatka(pole, st, wartosc, ustaw) {
    var wiersze = Array.isArray(wartosc) ? kopia(wartosc) : [];
    var wyl = !st.edytowalne;
    var tab = el('table', { klasa: 'siatka', 'data-sciezka': pole.sciezka });
    tab.appendChild(el('thead', null, el('tr', null, pole.kolumny.map(function (k) { return el('th', { tekst: k.etykieta }); }), el('th'))));
    var tb = el('tbody');
    var zapisz = function (pelny) { ustaw(wiersze.length ? wiersze : undefined, pelny); };
    wiersze.forEach(function (w, i) {
      var tr = el('tr');
      pole.kolumny.forEach(function (k) {
        var klucz = pole.sciezka + '#' + i + '#' + k.sciezka;
        var c;
        if (k.typ === 'enum') {
          c = el('select', { disabled: wyl, 'data-klucz': klucz, 'aria-label': k.etykieta, onchange: function (e) { w[k.sciezka] = e.target.value || undefined; zapisz(true); } },
            el('option', { value: '', tekst: '— wybierz —' }),
            SCHEMAT.enumy[k.enum].wartosci.filter(function (x) { return !x.ukryta; }).map(function (x) {
              return el('option', { value: x.nazwa, tekst: x.etykieta, selected: x.nazwa === w[k.sciezka] });
            }));
        } else {
          var liczbowa = ['int', 'decimal', 'percent', 'currency'].indexOf(k.typ) >= 0;
          c = el('input', {
            type: 'text', disabled: wyl, 'data-klucz': klucz, 'aria-label': k.etykieta,
            value: liczbowa ? liczbaNaTekst(w[k.sciezka]) : (w[k.sciezka] || ''),
            oninput: function (e) { w[k.sciezka] = liczbowa ? tekstNaLiczbe(e.target.value, k.typ === 'int') : e.target.value; zapisz(false); },
            onchange: function () { zmieniono(true); }
          });
          if (k.slownik) { var dl = datalista(k.slownik, wybrana()); if (dl) c.setAttribute('list', dl); }
        }
        tr.appendChild(el('td', null, c));
      });
      tr.appendChild(el('td', { klasa: 'usun' }, el('button', {
        type: 'button', tekst: '×', title: 'Usuń wiersz', disabled: wyl,
        onclick: function () { wiersze.splice(i, 1); zapisz(true); }
      })));
      tb.appendChild(tr);
    });
    tab.appendChild(tb);
    return el('div', null, tab, el('button', {
      type: 'button', tekst: '+ Dodaj wiersz', disabled: wyl, style: 'margin-top:6px',
      onclick: function () { wiersze.push({}); zapisz(true); }
    }));
  }

  /* ---------------- okna ---------------- */

  function zapytaj(tytul, tresc, przyciski) {
    return new Promise(function (resolve) {
      var dlg = document.getElementById('dialog');
      document.getElementById('dialogTytul').textContent = tytul;
      var t = document.getElementById('dialogTresc');
      t.textContent = '';
      (Array.isArray(tresc) ? tresc : [tresc]).forEach(function (x) { t.appendChild(typeof x === 'string' ? el('p', { tekst: x }) : x); });
      var m = document.getElementById('dialogPrzyciski');
      m.textContent = '';
      (przyciski || [{ tekst: 'Zamknij', wartosc: 'ok', glowny: true }]).forEach(function (p) {
        m.appendChild(el('button', { value: p.wartosc, klasa: p.glowny ? 'glowny' : '', tekst: p.tekst }));
      });
      dlg.onclose = function () { resolve(dlg.returnValue || null); };
      dlg.returnValue = '';
      dlg.showModal();
    });
  }

  function pokazPomoc(sciezka, k) {
    var pole = SCHEMAT.pola[sciezka];
    var tresc = [];
    if (pole.opis) tresc.push(el('p', { tekst: pole.opis }));
    if (pole.podpowiedz) tresc.push(el('p', null, el('strong', { tekst: 'Wskazówka: ' }), pole.podpowiedz));
    if (pole.enum) {
      var st = k.stan(sciezka);
      var dl = el('dl');
      SCHEMAT.enumy[pole.enum].wartosci.filter(function (w) { return !w.ukryta && (!st.dozwolone || st.dozwolone.indexOf(w.nazwa) >= 0); })
        .forEach(function (w) { dl.appendChild(el('dt', { tekst: w.etykieta })); if (w.opis) dl.appendChild(el('dd', { tekst: w.opis })); });
      tresc.push(el('p', null, el('strong', { tekst: 'Możliwe wartości:' })), dl);
    }
    if (pole.maxDlugosc) tresc.push(el('p', { tekst: 'Maksymalna długość: ' + pole.maxDlugosc + ' znaków.' }));
    var zr = [];
    if (pole.etykietaEnova && pole.etykietaEnova !== pole.etykieta) zr.push('Nazwa pola w enova: „' + pole.etykietaEnova.replace(/^,\s*/, '') + '”.');
    if (pole.zrodloOpisu) zr.push('Źródło opisu: ' + pole.zrodloOpisu + '.');
    if (pole.opisDoWeryfikacji) zr.push('Instrukcja enova nie opisuje tego pola szczegółowo – w razie wątpliwości zapytaj wdrożeniowca.');
    if (zr.length) tresc.push(el('div', { klasa: 'zrodlo', tekst: zr.join(' ') }));
    zapytaj(pole.etykieta, tresc);
  }

  function pokazOgolnaPomoc() {
    if (KLIENT) return zapytaj('Jak wypełnić formularz', [
      'Każda pozycja na liście po lewej to jedna definicja elementu wynagrodzenia (dodatek, premia, potrącenie…). Wdrożeniowiec przygotował je wstępnie – uzupełnij pola na stronie każdej definicji.',
      'Pole z gwiazdką trzeba uzupełnić. Przycisk „?” pokazuje pełny opis pola i znaczenie każdej opcji. Pola szare są tylko do wglądu – jeśli u Was jest inaczej, napisz o tym w „Uwagach dla wdrożeniowca” na dole strony.',
      'Gdy definicja jest ostatecznie omówiona z wdrożeniowcem, wpisz na dole strony imię i nazwisko i kliknij „Akceptuję definicję”. Każda późniejsza zmiana definicji cofa akceptację.',
      'Formularz zapisuje szkic w tej przeglądarce. Gotowy plik zapisz przyciskiem „Zapisz plik dla wdrożeniowca” i prześlij go mailem; zapisany plik można wczytać ponownie przyciskiem „Wczytaj plik…”.'
    ]);
    zapytaj('Jak wypełnić formularz', [
      'Każda pozycja na liście po lewej to jedna definicja elementu wynagrodzenia (dodatek, premia, potrącenie, zasiłek…). Najprościej zacząć od szablonu – ma ustawienia takie jak gotowe składniki enova – i zmienić to, co u Was jest inaczej.',
      'Zakładki odpowiadają oknu definicji w enova. Pola pojawiają się i znikają zależnie od wcześniejszych wyborów (np. procent podatku tylko przy zaliczce procentowej). Pole z gwiazdką jest wymagane, przycisk „?” pokazuje pełny opis i znaczenie każdej opcji.',
      'Ustaw licencję enova u góry – od niej zależy część pól (np. szczegółowe pomniejszenia za zwolnienia ZUS są w wersji złotej i platynowej).',
      'Czego nie da się ustawić polami, opisz na zakładce „Uwagi dla wdrożeniowca”.',
      'Formularz zapisuje szkic w tej przeglądarce. Gotowy plik zapisz przyciskiem „Zapisz plik dla wdrożeniowca” i prześlij go mailem; poprawiony plik można wczytać ponownie przyciskiem „Wczytaj plik…”.'
    ]);
  }

  /* ---------------- plik ---------------- */

  function eksportuj() {
    if (!stan.definicje.length) { zapytaj('Brak definicji', 'Dodaj co najmniej jedną definicję.'); return; }
    var b = bledy();
    var zBledami = stan.definicje.filter(function (d) { return liczBledy(b[d.id] || [], 'blad'); });
    var dalej = zBledami.length
      ? zapytaj('Definicje z błędami', [
        'Błędy ma ' + zBledami.length + ' z ' + stan.definicje.length + ' definicji: ' + zBledami.map(nazwaDefinicji).join(', ') + '.',
        'Plik można zapisać, ale wdrożeniowiec nie zaimportuje błędnych definicji, dopóki nie zostaną poprawione.'
      ], [{ tekst: 'Wróć do poprawiania', wartosc: 'nie', glowny: true }, { tekst: 'Zapisz mimo błędów', wartosc: 'tak' }])
      : Promise.resolve('tak');
    dalej.then(function (odp) {
      if (odp !== 'tak') return;
      var plik = {
        format: FORMAT,
        wersjaSchematu: SCHEMAT.wersjaSchematu,
        wersjaEnova: SCHEMAT.wersjaEnova,
        dataEksportu: new Date().toISOString(),
        ustawienia: kopia(stan.ustawienia),
        definicje: stan.definicje.map(function (d) {
          return {
            id: d.id,
            szablon: d.szablon || null,
            uwagi: d.uwagi || '',
            informacja: d.informacja || '',
            akceptacja: zaakceptowana(d) ? { osoba: d.akceptacja.osoba, data: d.akceptacja.data } : null,
            wartosci: W.doEksportu(SCHEMAT, d, stan.ustawienia),
            // pełna walidacja (także pól wdrożeniowca) – tak samo sprawdzi import
            bledy: W.waliduj(SCHEMAT, d, stan.definicje, stan.ustawienia).filter(function (x) { return x.poziom === 'blad'; })
              .map(function (x) { return (SCHEMAT.pola[x.sciezka] ? SCHEMAT.pola[x.sciezka].etykieta : x.sciezka) + ': ' + x.komunikat; })
          };
        })
      };
      pobierz('elementy-wynagrodzenia.json', JSON.stringify(plik, null, 2), 'application/json');
    });
  }

  function pobierz(nazwa, tresc, typ) {
    var a = el('a', { href: URL.createObjectURL(new Blob([tresc], { type: typ })), download: nazwa });
    document.body.appendChild(a);
    a.click();
    setTimeout(function () { URL.revokeObjectURL(a.href); a.remove(); }, 1000);
  }

  /* ---------------- formularz dla klienta (tryb wdrożeniowca) ---------------- */

  function ustawDlaKlienta(sciezka, v) {
    if (v === 'ukryte') delete stan.klient.pola[sciezka];
    else stan.klient.pola[sciezka] = v;
  }

  function selectStanu(wartosc, onzmiana, etykieta) {
    var s = el('select', { 'aria-label': etykieta, onchange: function (e) { onzmiana(e.target.value); } });
    if (wartosc === null) s.appendChild(el('option', { value: '', tekst: '— ustaw wszystkie —' }));
    STANY_KLIENTA.forEach(function (x) { s.appendChild(el('option', { value: x.id, tekst: x.etykieta, selected: x.id === wartosc })); });
    return s;
  }

  function renderKonfiguracja() {
    var kont = document.getElementById('edytor');
    var fokus = document.activeElement && document.activeElement.id === 'filtrKonf';
    var przewiniecie = window.scrollY;
    var kl = stan.klient;
    var odswiez = function () { zapiszSzkic(); renderKonfiguracja(); };
    var filtr = (stan.filtrKonf || '').trim().toLocaleLowerCase('pl');
    var pusty = new W.Kontekst(SCHEMAT, { wartosci: {} }, stan.ustawienia); // do okna „?”
    var licz = {};
    STANY_KLIENTA.forEach(function (x) { licz[x.id] = 0; });
    Object.keys(SCHEMAT.pola).forEach(function (s) { licz[kl.pola[s] || 'ukryte']++; });
    kont.textContent = '';
    kont.appendChild(el('div', { klasa: 'edytor-naglowek' },
      el('h2', { tekst: 'Formularz dla klienta' }),
      el('p', { klasa: 'opis-zakladki', tekst: 'Wybierz pola, które zobaczy klient, i te, które musi uzupełnić. Pozostałe ustawiasz sam na definicjach: klient ich nie widzi, a w pliku importu będą Twoje wartości (albo domyślne enova). Definicje z listy po lewej trafią do klienta wstępnie wypełnione, licencja – jak ustawiona u góry.' })));
    var tresc = el('div', { klasa: 'tresc' });
    tresc.appendChild(el('div', { klasa: 'konf-pasek' },
      el('span', { klasa: 'konf-liczniki', tekst: STANY_KLIENTA.map(function (x) { return x.etykieta + ': ' + licz[x.id]; }).join(' · ') }),
      el('label', { klasa: 'bool' }, el('input', {
        type: 'checkbox', checked: kl.mozeDodawac,
        onchange: function (e) { kl.mozeDodawac = e.target.checked; odswiez(); }
      }), 'Klient może dodawać i usuwać definicje'),
      el('input', {
        type: 'search', id: 'filtrKonf', placeholder: 'Szukaj pola…', value: stan.filtrKonf || '', 'aria-label': 'Szukaj pola',
        oninput: function (e) { stan.filtrKonf = e.target.value; renderKonfiguracja(); }
      }),
      el('button', { type: 'button', tekst: 'Ukryj wszystkie', onclick: function () { kl.pola = {}; odswiez(); } }),
      el('button', { type: 'button', klasa: 'glowny', id: 'btnPobierzKlienta', tekst: 'Pobierz formularz dla klienta', onclick: pobierzFormularzKlienta })));
    tresc.appendChild(el('div', { klasa: 'kontrolka konf-wiadomosc' },
      el('label', { htmlFor: 'wiadomoscKonf', tekst: 'Informacja dla klienta (na górze formularza, np. termin, kontakt, co uzupełnić):' }),
      el('textarea', {
        id: 'wiadomoscKonf', value: kl.wiadomosc || '',
        oninput: function (e) { kl.wiadomosc = e.target.value; zapiszSzkic(); }
      })));
    var byly = {};
    SCHEMAT.zakladki.forEach(function (z) {
      var sekcje = [];
      z.sekcje.forEach(function (s) {
        // pole występujące w kilku sekcjach (warianty algorytmu) – tylko przy pierwszym wystąpieniu
        var pola = s.pola.filter(function (p) {
          if (byly[p] || (filtr && SCHEMAT.pola[p].etykieta.toLocaleLowerCase('pl').indexOf(filtr) < 0)) return false;
          return (byly[p] = true);
        });
        if (!pola.length) return;
        var fs = el('fieldset', null, el('legend', { tekst: s.etykieta }),
          el('div', { klasa: 'konf-sekcja' }, 'Cała sekcja: ', selectStanu(null, function (v) {
            pola.forEach(function (p) { ustawDlaKlienta(p, v); }); odswiez();
          }, 'Ustaw całą sekcję ' + s.etykieta)));
        pola.forEach(function (p) {
          var pole = SCHEMAT.pola[p];
          fs.appendChild(el('div', { klasa: 'pole konf-' + (kl.pola[p] || 'ukryte'), 'data-konf': p },
            el('div', { klasa: 'etykieta' },
              el('label', null, pole.etykieta, pole.wymagane ? el('span', { klasa: 'wym', tekst: '*', title: 'Wymagane w enova' }) : null),
              el('button', { type: 'button', klasa: 'pomoc', tekst: '?', title: 'Opis pola', onclick: function () { pokazPomoc(p, pusty); } })),
            el('div', { klasa: 'kontrolka' }, selectStanu(kl.pola[p] || 'ukryte', function (v) { ustawDlaKlienta(p, v); odswiez(); }, pole.etykieta))));
        });
        sekcje.push(fs);
      });
      if (sekcje.length) tresc.appendChild(el('section', { klasa: 'grupa-klienta' }, el('h3', { tekst: z.etykieta }), sekcje));
    });
    kont.appendChild(tresc);
    window.scrollTo(0, przewiniecie);
    if (fokus) { var f = document.getElementById('filtrKonf'); f.focus(); f.setSelectionRange(f.value.length, f.value.length); }
  }

  function pobierzFormularzKlienta() {
    var kl = stan.klient;
    var widoczne = Object.keys(kl.pola).length;
    if (!widoczne) { zapytaj('Brak pól dla klienta', 'Wskaż co najmniej jedno pole, które zobaczy klient.'); return; }
    if (!stan.definicje.length && !kl.mozeDodawac) { zapytaj('Brak definicji', 'Dodaj definicje albo pozwól klientowi dodawać je samodzielnie.'); return; }
    var konf = {
      id: noweId(),
      utworzono: new Date().toISOString(),
      licencja: stan.ustawienia.licencja,
      mozeDodawac: !!kl.mozeDodawac,
      wiadomosc: kl.wiadomosc || '',
      pola: kopia(kl.pola),
      definicje: kopia(stan.definicje)
    };
    var BS = String.fromCharCode(92);
    var json = JSON.stringify(konf).replace(/</g, BS + 'u003c').replace(new RegExp(String.fromCharCode(0x2028), 'g'), BS + 'u2028').replace(new RegExp(String.fromCharCode(0x2029), 'g'), BS + 'u2029');
    var html = ZRODLO_HTML.replace(/(<script type="application\/json" id="konfiguracja">)[\s\S]*?(<\/script>)/, function (_, a, b) { return a + json + b; });
    pobierz('formularz-elementow-wynagrodzenia-klient.html', html, 'text/html');
  }

  function wczytaj(plik) {
    var r = new FileReader();
    r.onload = function () {
      var dane;
      try { dane = JSON.parse(r.result); } catch (e) { zapytaj('Nie można wczytać pliku', 'To nie jest poprawny plik JSON.'); return; }
      if (!dane || dane.format !== FORMAT || !Array.isArray(dane.definicje)) {
        zapytaj('Nie można wczytać pliku', 'Plik nie pochodzi z tego formularza.'); return;
      }
      if (String(dane.wersjaSchematu).split('.')[0] !== String(SCHEMAT.wersjaSchematu).split('.')[0]) {
        zapytaj('Inna wersja formularza', 'Plik zapisano formularzem w wersji ' + dane.wersjaSchematu + ', a ten formularz ma wersję ' + SCHEMAT.wersjaSchematu + '. Poproś wdrożeniowca o aktualny formularz.'); return;
      }
      var nowe = dane.definicje.map(function (d) {
        return {
          id: d.id || noweId(), szablon: d.szablon || null, uwagi: d.uwagi || '', informacja: d.informacja || '', wartosci: d.wartosci || {},
          akceptacja: d.akceptacja && d.akceptacja.osoba ? { osoba: d.akceptacja.osoba, data: d.akceptacja.data } : undefined
        };
      });
      var wybor = stan.definicje.length
        ? zapytaj('Wczytywanie pliku', 'Plik zawiera ' + nowe.length + ' definicji. Na liście jest już ' + stan.definicje.length + '.',
          [{ tekst: 'Anuluj', wartosc: 'anuluj' }, { tekst: 'Dołącz do listy', wartosc: 'dolacz' }, { tekst: 'Zastąp listę', wartosc: 'zastap', glowny: true }])
        : Promise.resolve('zastap');
      wybor.then(function (o) {
        if (o === 'zastap') stan.definicje = nowe;
        else if (o === 'dolacz') {
          var ids = {};
          stan.definicje.forEach(function (d) { ids[d.id] = true; });
          nowe.forEach(function (d) { if (ids[d.id]) d.id = noweId(); stan.definicje.push(d); });
        } else return;
        if (dane.ustawienia && dane.ustawienia.licencja && !KLIENT) stan.ustawienia.licencja = dane.ustawienia.licencja;
        // akceptacja z pliku dotyczy wartości z pliku
        nowe.forEach(function (d) { if (d.akceptacja) d.akceptacja.odcisk = odcisk(d); });
        document.getElementById('licencja').value = stan.ustawienia.licencja;
        stan.wybrana = nowe.length ? nowe[0].id : null;
        stan.zakladka = null;
        zmieniono(true);
      });
    };
    r.readAsText(plik, 'utf-8');
  }

  /* ---------------- start ---------------- */

  function start() {
    document.getElementById('podtytul').textContent =
      (KLIENT ? 'Przygotował wdrożeniowiec ' + new Date(KONF.utworzono).toLocaleDateString('pl-PL') + ' · ' : '')
      + 'Wersja formularza ' + SCHEMAT.wersjaSchematu + ' · enova ' + String(SCHEMAT.wersjaEnova).split('-')[0] + ' · dane zostają na tym komputerze';
    var lic = document.getElementById('licencja');
    lic.value = stan.ustawienia.licencja;
    lic.disabled = KLIENT;
    lic.addEventListener('change', function () { stan.ustawienia.licencja = lic.value; zmieniono(true); });
    var btnKlient = document.getElementById('btnKlient');
    btnKlient.hidden = KLIENT;
    btnKlient.addEventListener('click', function () { stan.widok = 'konfiguracja'; zapiszSzkic(); renderLista(); renderEdytor(); });
    var wiad = document.getElementById('wiadomosc');
    wiad.hidden = !(KLIENT && KONF.wiadomosc);
    if (!wiad.hidden) wiad.appendChild(el('div', { klasa: 'wiadomosc' }, el('strong', { tekst: 'Informacja od wdrożeniowca: ' }), KONF.wiadomosc));
    if (KLIENT && !KONF.mozeDodawac) ['btnNowa', 'btnDuplikuj', 'btnUsun'].forEach(function (id) { document.getElementById(id).hidden = true; });
    document.getElementById('btnNowa').addEventListener('click', function (e) {
      e.stopPropagation();
      var m = document.getElementById('menuNowa');
      menuNowa();
      m.hidden = !m.hidden;
      e.currentTarget.setAttribute('aria-expanded', String(!m.hidden));
    });
    document.addEventListener('click', function (e) { if (!e.target.closest('.nowa')) zamknijMenu(); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') zamknijMenu(); });
    document.getElementById('btnDuplikuj').addEventListener('click', function () {
      var d = wybrana();
      if (!d) return;
      var n = kopia(d);
      n.id = noweId();
      if (n.wartosci.Nazwa) n.wartosci.Nazwa = (n.wartosci.Nazwa + ' (kopia)').slice(0, SCHEMAT.pola.Nazwa.maxDlugosc || 40);
      stan.definicje.splice(stan.definicje.indexOf(d) + 1, 0, n);
      stan.wybrana = n.id;
      stan.widok = null;
      zmieniono(true);
    });
    document.getElementById('btnUsun').addEventListener('click', function () {
      var d = wybrana();
      if (!d) return;
      zapytaj('Usunąć definicję?', 'Definicja „' + nazwaDefinicji(d) + '” zostanie usunięta z listy.',
        [{ tekst: 'Anuluj', wartosc: 'nie', glowny: true }, { tekst: 'Usuń', wartosc: 'tak' }]).then(function (o) {
          if (o !== 'tak') return;
          var i = stan.definicje.indexOf(d);
          stan.definicje.splice(i, 1);
          var nast = stan.definicje[Math.min(i, stan.definicje.length - 1)];
          stan.wybrana = nast ? nast.id : null;
          zmieniono(true);
        });
    });
    document.getElementById('btnEksport').addEventListener('click', eksportuj);
    document.getElementById('btnImport').addEventListener('click', function () { document.getElementById('plik').click(); });
    document.getElementById('plik').addEventListener('change', function (e) {
      if (e.target.files[0]) wczytaj(e.target.files[0]);
      e.target.value = '';
    });
    document.getElementById('btnPomoc').addEventListener('click', pokazOgolnaPomoc);
    if (!stan.wybrana && stan.definicje.length) stan.wybrana = stan.definicje[0].id;
    renderLista();
    renderEdytor();
    zapiszSzkic();
  }

  start();
})();
