/*
 * Silnik ankiety elementu wynagrodzenia (schema/ankieta.json).
 * Odpowiedzi ustawiają pola definicji; reguły widoczności pytań w DSL warunki.js („pole” = id pytania).
 *
 * Kolejność nakładania wartości: szablon (odpowiedź z „szablon”) → „ustaw”/„ustawGdy” odpowiedzi w kolejności pytań
 * → pola zmienione przez użytkownika (def.ankieta.reczne – także odpowiedzi na pytania „pole”/„pola”).
 * Wartość null w „ustaw” = usuń (domyślna enova).
 * Odpowiedź NIE_WIEM nie zmienia wartości (zostaje szablon) i trafia na listę do wyjaśnienia z wdrożeniowcem.
 */
(function (root, factory) {
  if (typeof module === 'object' && module.exports) module.exports = factory(require('./warunki.js'));
  else root.Ankieta = factory(root.Warunki);
})(typeof self !== 'undefined' ? self : this, function (W) {
  'use strict';

  var NIE_WIEM = '?';

  function kopia(o) { return o === undefined ? undefined : JSON.parse(JSON.stringify(o)); }

  function typ(p) { return p.typ || 'wybor'; }

  /** Pola, o które pyta pytanie typu „pole”/„pola” (pusta lista dla pozostałych). */
  function polaPytania(p) { return p.pola || (p.pole ? [p.pole] : []); }

  function ctxOdpowiedzi(odp) {
    return { licencja: null, wartosc: function (id) { return odp[id]; } };
  }

  /** Pytania widoczne przy danych odpowiedziach, w kolejności ankiety. */
  function widocznePytania(ankieta, odp) {
    var ctx = ctxOdpowiedzi(odp || {});
    return ankieta.pytania.filter(function (p) { return W.ocen(p.widoczneGdy, ctx); });
  }

  function odpowiedz(p, id) {
    return (p.odpowiedzi || []).filter(function (o) { return o.id === id; })[0] || null;
  }

  /** Czy pole pasuje do wzorca „szczegoly” (ścieżka albo prefiks zakończony „*”). */
  function pasuje(wzorzec, sciezka) {
    return wzorzec.slice(-1) === '*' ? sciezka.indexOf(wzorzec.slice(0, -1)) === 0 : wzorzec === sciezka;
  }

  function rozwinSzczegoly(schemat, wzorce) {
    return Object.keys(schemat.pola).filter(function (s) { return wzorce.some(function (w) { return pasuje(w, s); }); });
  }

  /**
   * Wynik ankiety dla definicji.
   * def: { wartosci, ankieta: { odpowiedzi: {id: odp}, reczne: {sciezka: true} } }
   * ukryte(p): pytanie ukryte przed klientem – bez odpowiedzi nie trafia na listę „brak” (obowiązuje ustawienie wdrożeniowca).
   * Zwraca { szablon, wartosci, pochodzenie: {sciezka: 'szablon'|'reczne'|idPytania}, szczegoly: [ścieżki pól do pokazania],
   *          pola: [pola z widocznych pytań o pola],
   *          wdrozeniowiec: [{pytanie, powod}], doPotwierdzenia: [idPytania], nieWiem: [idPytania], brak: [idPytania] }.
   */
  function zastosuj(ankieta, schemat, def, ukryte) {
    var a = def.ankieta || {};
    var odp = a.odpowiedzi || {};
    var reczne = a.reczne || {};
    var biezace = def.wartosci || {};
    var wynik = { szablon: null, wartosci: {}, pochodzenie: {}, szczegoly: [], pola: [], wdrozeniowiec: [], doPotwierdzenia: [], nieWiem: [], brak: [] };
    var widoczne = widocznePytania(ankieta, odp);
    var ctx = ctxOdpowiedzi(odp);
    var wzorce = [];

    // szablon – z ostatniej odpowiedzi, która go wskazuje
    widoczne.forEach(function (p) {
      var o = odpowiedz(p, odp[p.id]);
      if (o && o.szablon) wynik.szablon = o.szablon;
    });
    var szablon = wynik.szablon && (schemat.szablony || []).filter(function (s) { return s.id === wynik.szablon; })[0];
    if (szablon) Object.keys(szablon.wartosci).forEach(function (s) {
      wynik.wartosci[s] = kopia(szablon.wartosci[s]);
      wynik.pochodzenie[s] = 'szablon';
    });

    function ustaw(mapa, idPytania) {
      Object.keys(mapa || {}).forEach(function (s) {
        if (mapa[s] === null) delete wynik.wartosci[s];
        else wynik.wartosci[s] = kopia(mapa[s]);
        wynik.pochodzenie[s] = idPytania;
      });
    }

    var pytaniePola = {};
    widoczne.forEach(function (p) {
      var t = typ(p);
      if (t === 'pola' || t === 'pole') {
        if (ukryte && ukryte(p)) return; // zmiana takiego pola u klienta to zmiana ręczna (np. szczegóły po „Inaczej”)
        polaPytania(p).forEach(function (s) { wynik.pola.push(s); pytaniePola[s] = p.id; });
        if (odp[p.id] === NIE_WIEM) wynik.nieWiem.push(p.id);
        return;
      }
      if (t !== 'wybor') return;
      var v = odp[p.id];
      if (v === undefined) { if (!(ukryte && ukryte(p))) wynik.brak.push(p.id); return; }
      if (v === NIE_WIEM) { wynik.nieWiem.push(p.id); return; }
      var o = odpowiedz(p, v);
      if (!o) { wynik.brak.push(p.id); return; }
      ustaw(o.ustaw, p.id);
      (o.ustawGdy || []).forEach(function (r) { if (W.ocen(r.gdy, ctx)) ustaw(r.ustaw, p.id); });
      if (o.szczegoly) wzorce = wzorce.concat(o.szczegoly);
      if (o.wdrozeniowiec) wynik.wdrozeniowiec.push({ pytanie: p.id, powod: o.wdrozeniowiec });
      if (p.prawne && p.odpowiedzi[0].id !== o.id) wynik.doPotwierdzenia.push(p.id);
    });

    Object.keys(reczne).forEach(function (s) {
      if (W.pusty(biezace[s])) delete wynik.wartosci[s];
      else wynik.wartosci[s] = kopia(biezace[s]);
      wynik.pochodzenie[s] = pytaniePola[s] || 'reczne';
    });
    wynik.szczegoly = rozwinSzczegoly(schemat, wzorce);
    return wynik;
  }

  /** Krótki opis odpowiedzi na pytanie (do podsumowania). */
  function opisOdpowiedzi(p, def) {
    var odp = (def.ankieta && def.ankieta.odpowiedzi) || {};
    if (odp[p.id] === NIE_WIEM) return 'Nie wiem';
    if (typ(p) === 'wybor') {
      var o = odpowiedz(p, odp[p.id]);
      return o ? o.etykieta : '';
    }
    if (typ(p) === 'uwagi') return def.uwagi || '';
    return null; // pola – opis buduje interfejs z wartości pól
  }

  /** Kontrola spójności ankiety ze schematem – lista błędów (pusta = OK). */
  function sprawdz(ankieta, schemat) {
    var bledy = [];
    var ids = {};
    var szablony = {};
    (schemat.szablony || []).forEach(function (s) { szablony[s.id] = true; });
    function wartoscPola(s, v, gdzie) {
      var pole = schemat.pola[s];
      if (!pole) { bledy.push(gdzie + ': brak pola ' + s); return; }
      if (v === null) return;
      if (pole.enum) {
        var e = schemat.enumy[pole.enum];
        if (!e || !e.wartosci.some(function (x) { return x.nazwa === v; })) bledy.push(gdzie + ': ' + s + ' = ' + JSON.stringify(v) + ' spoza enuma ' + pole.enum);
      } else if (pole.typ === 'bool' && typeof v !== 'boolean') bledy.push(gdzie + ': ' + s + ' – oczekiwano Tak/Nie');
      else if (['int', 'decimal', 'percent', 'currency'].indexOf(pole.typ) >= 0 && typeof v !== 'number') bledy.push(gdzie + ': ' + s + ' – oczekiwano liczby');
    }
    function warunek(w, gdzie) {
      if (!w) return;
      ['wszystkie', 'dowolny'].forEach(function (k) { if (w[k]) w[k].forEach(function (x) { warunek(x, gdzie); }); });
      if (w.nie) warunek(w.nie, gdzie);
      if (!w.pole) return;
      if (!ids[w.pole]) { bledy.push(gdzie + ': warunek odwołuje się do pytania ' + w.pole + ' (brak albo dalej w ankiecie)'); return; }
      var p = ids[w.pole];
      [].concat(w.rowne !== undefined ? [w.rowne] : [], w.rozne !== undefined ? [w.rozne] : [], w.wsrod || [], w.poza || []).forEach(function (v) {
        if (!odpowiedz(p, v)) bledy.push(gdzie + ': pytanie ' + w.pole + ' nie ma odpowiedzi ' + v);
      });
    }
    var rozdzialy = {};
    (ankieta.rozdzialy || []).forEach(function (r) { rozdzialy[r.id] = true; });
    ankieta.pytania.forEach(function (p) {
      var g = 'pytanie ' + p.id;
      if (ids[p.id]) bledy.push(g + ': powtórzone id');
      if (ankieta.rozdzialy && !rozdzialy[p.rozdzial]) bledy.push(g + ': brak rozdziału ' + p.rozdzial);
      warunek(p.widoczneGdy, g);
      polaPytania(p).forEach(function (s) { if (!schemat.pola[s]) bledy.push(g + ': brak pola ' + s); });
      if (typ(p) === 'pole' && p.etykiety) Object.keys(p.etykiety).forEach(function (v) {
        var pole = schemat.pola[p.pole];
        var ok = pole && (pole.typ === 'bool' ? v === 'true' || v === 'false'
          : pole.enum && schemat.enumy[pole.enum].wartosci.some(function (x) { return x.nazwa === v; }));
        if (!ok) bledy.push(g + ': odpowiedź ' + v + ' spoza wartości pola ' + p.pole);
      });
      if (typ(p) === 'pole') return;
      (p.odpowiedzi || []).forEach(function (o) {
        var go = g + '/' + o.id;
        if (o.id === NIE_WIEM) bledy.push(go + ': id zarezerwowane dla „Nie wiem”');
        if (o.szablon && !szablony[o.szablon]) bledy.push(go + ': brak szablonu ' + o.szablon);
        Object.keys(o.ustaw || {}).forEach(function (s) { wartoscPola(s, o.ustaw[s], go); });
        (o.ustawGdy || []).forEach(function (r) {
          warunek(r.gdy, go);
          Object.keys(r.ustaw || {}).forEach(function (s) { wartoscPola(s, r.ustaw[s], go); });
        });
        (o.szczegoly || []).forEach(function (w) { if (!rozwinSzczegoly(schemat, [w]).length) bledy.push(go + ': szczegóły „' + w + '” nie pasują do żadnego pola'); });
      });
      ids[p.id] = p;
    });
    return bledy;
  }

  /** Pola schematu, o które nie pyta żadne pytanie typu „pole”/„pola”. */
  function niepokryte(ankieta, schemat) {
    var jest = {};
    ankieta.pytania.forEach(function (p) { polaPytania(p).forEach(function (s) { jest[s] = true; }); });
    return Object.keys(schemat.pola).filter(function (s) { return !jest[s]; });
  }

  return {
    NIE_WIEM: NIE_WIEM,
    typ: typ,
    polaPytania: polaPytania,
    niepokryte: niepokryte,
    widocznePytania: widocznePytania,
    odpowiedz: odpowiedz,
    zastosuj: zastosuj,
    opisOdpowiedzi: opisOdpowiedzi,
    sprawdz: sprawdz
  };
});
