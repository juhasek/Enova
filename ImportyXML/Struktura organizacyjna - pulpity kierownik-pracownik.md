# Struktura organizacyjna — pulpity: kierownik i pracownicy

Struktura podległości dla Pulpitów enova365 (Pulpit pracownika / Pulpit kierownika, e-wnioski):
węzeł **Kierownik**, pod nim węzeł **Pracownik**. Pracownik podpięty do węzła Pracownik podlega
pracownikowi podpiętemu do węzła nadrzędnego (Kierownik).

| Plik | Zawartość | Reimport |
|---|---|---|
| `Struktura organizacyjna - pulpity kierownik-pracownik.xml` | struktura, 2 definicje elementów, 2 elementy, powiązania pracowników | idempotentny: stałe guidy `a1500000-...`, `DefinicjeElementów`/`Elementy` z `addnew="true"` (bez tego reimport pada na „Nie można kasować elementu posiadającego powiązania”), kolekcja `Powiązania` zastępowana — lista pracowników w węźle = zawartość pliku; węzła usuniętego z pliku reimport nie skasuje |
| `Struktura organizacyjna - pulpity kierownik-pracownik - prawa.xml` | prawa do struktury: Administrator, pulpitKadryPlace, pulpitKierownik, pulpitDyrektor | **jednorazowy** — tabela `Rights` nie ma unikalnego indeksu, ponowny import dubluje wpisy |

## Baza Claude (zaimportowane 2026-09-10)

- `StrukturyOrg` ID 6 „Pulpity - kierownik i pracownicy”, guid `a1500000-0000-0000-0000-000000000001`.
- Definicje: Kierownik (`...0011`), Pracownik (`...0012`), typ źródła powiązania = Pracownicy.
- Elementy: `KIEROWNIK` (korzeń, `...0021`) → `PRACOWNIK` (`...0022`).
- Powiązania (okres „wszystko”): Kierownik = **TS-05 Lewandowski Marek**; Pracownik = TS-01 Kowalska
  Anna, TS-02 Wiśniewski Piotr, TS-03 Wójcik Katarzyna, TS-09 Zielińska Agnieszka.

W innej bazie trzeba podmienić guidy pracowników w `<Zrodlo>Pracownik:...</Zrodlo>` (tu `b1000000-...`
z importu testowego).

## Jak pulpity z tego korzystają (ustalone z kodu enova 2512.5.6)

- **Narzędzia → Opcje → Kadry NET → Ogólne → Struktura podległości** — musi wskazywać tę strukturę.
  enova zapisuje tu **nazwę** struktury (`CfgAttributes.StrValue`), więc zmiana nazwy struktury
  zrywa powiązanie z konfiguracją. Gdy pole jest puste, pulpity liczą podwładnych z cech pracownika
  (`InnerKalkulatorPodwladniCechy`), a nie ze struktury.
- Kierownik pracownika (e-wnioski: `EWniosekBase.GetKierownik` → `KalkulatorPodwladnych`) =
  `Pracownik.StrukturaOraganizacyjna.Przełożony(struktura, data)` — pracownicy podpięci do węzłów
  nadrzędnych. Pole `Warunek1` definicji nie ma tu znaczenia (używane tylko przez teczki).
- Lista podwładnych na pulpicie kierownika filtruje powiązania po `Okres` — dlatego powiązania mają
  jawnie ustawiony okres „wszystko” (pusty okres = brak podwładnych).
- Uprawnienia kierownika na pulpicie w trybie struktury wymagają licencji **PKN** (Pulpit Kierownika);
  bez niej `JestKierownikiem` = false.
- Rekordy historii podległości (`ElementyStOrgRef`) tworzy sam enova przy imporcie
  (`ElementStrukturyOrganizacyjnej.OnImported`).
- Pracownik po zmianie przełożonego: w pulpicie „Podwładni niższego poziomu” (`Net.PodwladniNizszegoPoziomu`)
  decyduje, czy kierownik widzi tylko bezpośrednich podwładnych.

## Wycofanie

Import z `deleted="True"` na strukturze (`<StrukturaOrganizacyjna guid="a1500000-0000-0000-0000-000000000001" deleted="True" />`)
albo usunięcie struktury w GUI; wcześniej wyczyścić „Struktura podległości” w konfiguracji.
