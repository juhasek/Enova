/*
 * Silnik reguł schematu definicji elementu wynagrodzenia.
 * Ta sama semantyka obowiązuje w imporcie (dodatek enova) – zmiany w obu miejscach naraz.
 *
 * Wartość pola w definicji:
 *   - użytkownika: def.wartosci[sciezka] (tylko pola, które klient zmienił lub wczytał z szablonu),
 *   - efektywna:   wartość użytkownika, jeżeli pole jest aktywne i niepuste; w przeciwnym razie domyślna.
 * Pole aktywne = widoczne i edytowalne (warunki pola, sekcji, zakładki, licencji).
 * Eksportowane są wartości efektywne wszystkich pól aktywnych – import ustawia je w kolejności schematu.
 */
(function (root, factory) {
  if (typeof module === 'object' && module.exports) module.exports = factory();
  else root.Warunki = factory();
})(typeof self !== 'undefined' ? self : this, function () {
  'use strict';

  var LICENCJE = { srebrna: 1, zlota: 2, platynowa: 3 };

  function pusty(v) {
    return v === undefined || v === null || v === '' || (Array.isArray(v) && v.length === 0);
  }

  function rowne(a, b) {
    if (typeof a === 'number' || typeof b === 'number') return Number(a) === Number(b);
    return a === b;
  }

  /** Ocena warunku DSL. ctx: { wartosc(sciezka), licencja }. */
  function ocen(w, ctx) {
    if (!w) return true;
    if (w.wszystkie) return w.wszystkie.every(function (x) { return ocen(x, ctx); });
    if (w.dowolny) return w.dowolny.some(function (x) { return ocen(x, ctx); });
    if (w.nie) return !ocen(w.nie, ctx);
    if (w.licencja) return (LICENCJE[ctx.licencja] || 0) >= (LICENCJE[w.licencja] || 99);
    var v = ctx.wartosc(w.pole);
    if ('rowne' in w) return rowne(v, w.rowne);
    if ('rozne' in w) return !rowne(v, w.rozne);
    if (w.wsrod) return w.wsrod.some(function (x) { return rowne(v, x); });
    if (w.poza) return !w.poza.some(function (x) { return rowne(v, x); });
    if ('pusty' in w) return pusty(v) === w.pusty;
    if ('wiekszeOd' in w) return Number(v) > w.wiekszeOd;
    throw new Error('Nieznany warunek: ' + JSON.stringify(w));
  }

  /** Mapa ścieżka pola → lista {zakladka, sekcja}, w których pole występuje. */
  function indeksWystapien(schemat) {
    if (schemat._wystapienia) return schemat._wystapienia;
    var m = {};
    schemat.zakladki.forEach(function (z) {
      z.sekcje.forEach(function (s) {
        s.pola.forEach(function (p) { (m[p] = m[p] || []).push({ zakladka: z, sekcja: s }); });
      });
    });
    Object.defineProperty(schemat, '_wystapienia', { value: m, enumerable: false });
    return m;
  }

  /**
   * Kontekst oceny jednej definicji. Wartości i stany są liczone leniwie z pamięcią;
   * zależności cykliczne (pole A zależy od B, B od A) przerywa się wartością bez warunków.
   */
  function Kontekst(schemat, def, ustawienia) {
    this.schemat = schemat;
    this.def = def;
    this.licencja = (ustawienia && ustawienia.licencja) || 'zlota';
    this._wartosci = {};
    this._stany = {};
    this._wToku = {};
  }

  Kontekst.prototype.ctx = function () {
    var self = this;
    return { licencja: this.licencja, wartosc: function (s) { return self.wartosc(s); } };
  };

  Kontekst.prototype.domyslna = function (sciezka) {
    var pole = this.schemat.pola[sciezka];
    if (!pole) return undefined;
    var d = pole.domyslna;
    if (d && typeof d === 'object' && !Array.isArray(d) && d.wgWarunku) {
      for (var i = 0; i < d.wgWarunku.length; i++)
        if (ocen(d.wgWarunku[i].gdy, this.ctx())) return d.wgWarunku[i].wartosc;
      return d.inaczej;
    }
    if (d === undefined && pole.typ === 'bool') return false;
    return d;
  };

  Kontekst.prototype.uzytkownika = function (sciezka) {
    var w = this.def.wartosci || {};
    return w[sciezka];
  };

  Kontekst.prototype.wartosc = function (sciezka) {
    if (sciezka in this._wartosci) return this._wartosci[sciezka];
    if (this._wToku[sciezka]) {
      var u = this.uzytkownika(sciezka);
      return pusty(u) ? this.domyslna(sciezka) : u;
    }
    this._wToku[sciezka] = true;
    var v = this.uzytkownika(sciezka);
    if (pusty(v) || !this.stan(sciezka).aktywne) v = this.domyslna(sciezka);
    delete this._wToku[sciezka];
    this._wartosci[sciezka] = v;
    return v;
  };

  /** Stan pola: widoczne, edytowalne, aktywne, wymagane, dozwolone (lista wartości), powod (gdy nieaktywne). */
  Kontekst.prototype.stan = function (sciezka) {
    if (this._stany[sciezka]) return this._stany[sciezka];
    var pole = this.schemat.pola[sciezka];
    var ctx = this.ctx();
    var st = { widoczne: false, edytowalne: false, aktywne: false, wymagane: false, dozwolone: null, powod: null };
    this._stany[sciezka] = st; // ochrona przed cyklem – wewnątrz liczenia pole uchodzi za nieaktywne
    if (!pole) return st;
    var wyst = indeksWystapien(this.schemat)[sciezka] || [];
    var kontener = wyst.some(function (w) {
      return ocen(w.zakladka.widocznaGdy, ctx) && ocen(w.sekcja.widocznaGdy, ctx);
    });
    var lic = !pole.licencja || ocen({ licencja: pole.licencja }, ctx);
    var widoczne = kontener && lic && ocen(pole.widocznyGdy, ctx);
    var edytowalne = widoczne && ocen(pole.edytowalnyGdy, ctx);
    var wynik = {
      widoczne: widoczne,
      edytowalne: edytowalne,
      aktywne: widoczne && edytowalne,
      wymagane: edytowalne && (pole.wymagane === true || (pole.wymaganeGdy ? ocen(pole.wymaganeGdy, ctx) : false)),
      dozwolone: this.dozwolone(pole, ctx),
      powod: !lic ? 'wymaga licencji ' + nazwaLicencji(pole.licencja)
        : (widoczne && !edytowalne && pole.edytowalnyGdy) ? (pole.powodNieaktywnosci || 'dostępne, gdy: ' + opisz(pole.edytowalnyGdy, this.schemat)) : null
    };
    this._stany[sciezka] = wynik;
    return wynik;
  };

  /** Dozwolone wartości pola (enum lub lista tekstów) albo null, gdy dowolne. */
  Kontekst.prototype.dozwolone = function (pole, ctx) {
    var lista = null;
    if (pole.enum && this.schemat.enumy[pole.enum])
      lista = this.schemat.enumy[pole.enum].wartosci.filter(function (w) { return !w.ukryta; }).map(function (w) { return w.nazwa; });
    if (pole.dozwoloneWartosci) {
      for (var i = 0; i < pole.dozwoloneWartosci.length; i++) {
        var r = pole.dozwoloneWartosci[i];
        if (!r.gdy || ocen(r.gdy, ctx)) {
          var dozw = r.wartosci;
          lista = lista ? lista.filter(function (x) { return dozw.indexOf(x) >= 0; }) : dozw.slice();
          break;
        }
      }
    }
    return lista;
  };

  Kontekst.prototype.zakladkaWidoczna = function (z) { return ocen(z.widocznaGdy, this.ctx()); };
  Kontekst.prototype.sekcjaWidoczna = function (s) { return ocen(s.widocznaGdy, this.ctx()); };

  function nazwaLicencji(l) {
    return l === 'zlota' ? 'złotej lub platynowej' : l === 'platynowa' ? 'platynowej' : l;
  }

  /** Czytelny opis warunku (komunikaty „dostępne, gdy…”). */
  function opisz(w, schemat) {
    if (!w) return '';
    if (w.wszystkie) return w.wszystkie.map(function (x) { return nawias(x, schemat); }).join(' i ');
    if (w.dowolny) return w.dowolny.map(function (x) { return nawias(x, schemat); }).join(' lub ');
    if (w.nie) return 'nie (' + opisz(w.nie, schemat) + ')';
    if (w.licencja) return 'licencja ' + nazwaLicencji(w.licencja);
    var et = schemat.pola[w.pole] ? '„' + schemat.pola[w.pole].etykieta + '”' : w.pole;
    var wart = function (x) { return etykietaWartosci(schemat, w.pole, x); };
    if ('rowne' in w) return et + ' = ' + wart(w.rowne);
    if ('rozne' in w) return et + ' ≠ ' + wart(w.rozne);
    if (w.wsrod) return et + ': ' + w.wsrod.map(wart).join(' / ');
    if (w.poza) return et + ' inne niż ' + w.poza.map(wart).join(' / ');
    if ('pusty' in w) return et + (w.pusty ? ' puste' : ' wypełnione');
    if ('wiekszeOd' in w) return et + ' > ' + w.wiekszeOd;
    return JSON.stringify(w);
  }

  function nawias(x, schemat) {
    var t = opisz(x, schemat);
    return (x.wszystkie || x.dowolny) ? '(' + t + ')' : t;
  }

  function etykietaWartosci(schemat, sciezka, v) {
    if (v === true) return 'Tak';
    if (v === false) return 'Nie';
    var pole = schemat.pola[sciezka];
    if (pole && pole.enum && schemat.enumy[pole.enum]) {
      var e = schemat.enumy[pole.enum].wartosci.filter(function (x) { return x.nazwa === v; })[0];
      if (e) return e.etykieta;
    }
    return String(v);
  }

  /* ---------------- walidacja ---------------- */

  var WZORZEC_ULAMEK = /^\s*\d+([.,]\d+)?\s*(\/\s*\d+([.,]\d+)?)?\s*$/;
  var WZORZEC_CZAS = /^\s*\d{1,4}:[0-5]\d\s*$/;

  function liczba(v) { return typeof v === 'number' && isFinite(v); }

  function sprawdzTyp(pole, v, schemat) {
    switch (pole.typ) {
      case 'int': return Number.isInteger(v) ? null : 'Wpisz liczbę całkowitą.';
      case 'decimal': case 'percent': case 'currency': return liczba(v) ? null : 'Wpisz liczbę.';
      case 'bool': return typeof v === 'boolean' ? null : 'Wybierz Tak albo Nie.';
      case 'fraction': return WZORZEC_ULAMEK.test(String(v)) ? null : 'Wpisz ułamek, np. 1/2.';
      case 'time': return WZORZEC_CZAS.test(String(v)) ? null : 'Wpisz czas w formacie gg:mm.';
      case 'string': case 'text': case 'slownik':
        if (typeof v !== 'string') return 'Wpisz tekst.';
        if (pole.maxDlugosc && v.length > pole.maxDlugosc) return 'Maksymalnie ' + pole.maxDlugosc + ' znaków (jest ' + v.length + ').';
        return null;
      case 'lista': return Array.isArray(v) ? null : 'Nieprawidłowa lista.';
      default: return null;
    }
  }

  function norm(s) { return String(s == null ? '' : s).trim().toLocaleLowerCase('pl'); }

  /**
   * Walidacja definicji. Zwraca listę { sciezka, poziom: 'blad'|'ostrzezenie'|'informacja', komunikat }.
   * wszystkie – lista definicji z formularza (do unikalności); ustawienia – { licencja }.
   */
  function waliduj(schemat, def, wszystkie, ustawienia) {
    var k = new Kontekst(schemat, def, ustawienia);
    var wynik = [];
    function dodaj(s, poziom, kom) { wynik.push({ sciezka: s, poziom: poziom, komunikat: kom }); }
    var inne = (wszystkie || []).filter(function (d) { return d !== def && d.id !== def.id; })
      .map(function (d) { return new Kontekst(schemat, d, ustawienia); });

    Object.keys(schemat.pola).forEach(function (s) {
      var pole = schemat.pola[s];
      var st = k.stan(s);
      if (!st.aktywne) return;
      var v = k.wartosc(s);
      if (pusty(v)) {
        if (st.wymagane) dodaj(s, 'blad', 'Pole jest wymagane.');
        return;
      }
      var bladTypu = sprawdzTyp(pole, v, schemat);
      if (bladTypu) { dodaj(s, 'blad', bladTypu); return; }
      if (st.dozwolone && pole.typ !== 'lista' && st.dozwolone.indexOf(v) < 0)
        dodaj(s, 'blad', 'Wartość „' + etykietaWartosci(schemat, s, v) + '” jest niedozwolona przy obecnych ustawieniach.');
      var wal = pole.walidacje || {};
      if (wal.min !== undefined && liczba(v) && v < wal.min) dodaj(s, 'blad', 'Wartość nie może być mniejsza niż ' + wal.min + '.');
      if (wal.max !== undefined && liczba(v) && v > wal.max) dodaj(s, 'blad', 'Wartość nie może być większa niż ' + wal.max + '.');
      if (wal.wzorzec && !(new RegExp(wal.wzorzec)).test(String(v))) dodaj(s, 'blad', wal.komunikat || 'Nieprawidłowy format.');
      if (wal.unikalne) {
        var poziom = wal.unikalne === 'blad' ? 'blad' : 'ostrzezenie';
        var dubel = inne.some(function (ik) { return norm(ik.wartosc(s)) === norm(v); });
        if (dubel) dodaj(s, poziom, 'Ta sama wartość jest w innej definicji w tym pliku.');
        if (s === 'Nazwa' && (schemat.definicjeStandardowe || []).some(function (d) { return norm(d.nazwa) === norm(v); }))
          dodaj(s, 'blad', 'Taką nazwę ma definicja standardowa enova – wybierz inną.');
      }
      if (pole.typ === 'slownik' && pole.slownik && schemat.slowniki[pole.slownik]) {
        var std = schemat.slowniki[pole.slownik].wartosciStandardowe;
        if (std && !std.some(function (x) { return norm(x.klucz) === norm(v); }))
          dodaj(s, 'informacja', 'Wartości nie ma na liście standardowej – import sprawdzi, czy istnieje w bazie enova.');
      }
      if (pole.typ === 'lista' && pole.kolumny) walidujListe(schemat, s, pole, v, dodaj);
    });
    return wynik;
  }

  function walidujListe(schemat, s, pole, wiersze, dodaj) {
    wiersze.forEach(function (w, i) {
      pole.kolumny.forEach(function (kol) {
        var v = w[kol.sciezka];
        if (pusty(v)) { dodaj(s, 'blad', 'Wiersz ' + (i + 1) + ': uzupełnij „' + kol.etykieta + '”.'); return; }
        var b = sprawdzTyp(kol, v, schemat);
        if (b) dodaj(s, 'blad', 'Wiersz ' + (i + 1) + ', „' + kol.etykieta + '”: ' + b);
      });
    });
  }

  /** Wartości do eksportu: efektywne wartości aktywnych pól, w kolejności schematu. */
  function doEksportu(schemat, def, ustawienia) {
    var k = new Kontekst(schemat, def, ustawienia);
    var w = {};
    Object.keys(schemat.pola).forEach(function (s) {
      if (!k.stan(s).aktywne) return;
      var v = k.wartosc(s);
      if (!pusty(v)) w[s] = v;
    });
    return w;
  }

  return {
    LICENCJE: LICENCJE,
    pusty: pusty,
    ocen: ocen,
    opisz: opisz,
    etykietaWartosci: etykietaWartosci,
    Kontekst: Kontekst,
    waliduj: waliduj,
    doEksportu: doEksportu
  };
});
