# Dodatek za pracę – konfiguracja elementu wynagrodzenia

Dokumentacja konfiguracyjna dla operatora enova365.

## 1. Charakterystyka

„Dodatek za pracę" to element wynagrodzenia, którego kwota **nie jest wyliczana wzorem**, tylko
**podawana ręcznie przez operatora** przy naliczaniu listy płac (np. wprowadzana na czasówce/liście
płac dla konkretnego pracownika i okresu). Element ma być:

- **opodatkowany** (wliczany do podstawy podatku dochodowego),
- **ozusowany** (wliczany do podstawy składek ZUS — społecznych i zdrowotnej).

Ponieważ wartość elementu to wprost wartość parametru (bez żadnej dodatkowej kalkulacji), **nie
jest potrzebny żaden kod C#** (nie ma pliku bez rozszerzenia obok tego `.md`) — całość konfiguruje
się z poziomu GUI, typem elementu „Kwota z parametru", a nie „Algorytm".

## 2. Ścieżka konfiguracji

`Narzędzia → Opcje → Kadry i płace → Płace → Elementy wynagrodzenia → Dodaj`

## 3. Ustawienia elementu

| Pole | Wartość | Komentarz |
|---|---|---|
| Nazwa | `Dodatek za pracę` | |
| Symbol | np. `DODPRACA` | Symbol wewnętrzny, dowolny, bez polskich znaków/spacji |
| Rodzaj elementu | Dodatek | Element zwiększający wynagrodzenie |
| Sposób ustalania kwoty | Wartość z parametru | Kwota = wartość podana przy naliczaniu, bez wzoru |
| Podatek dochodowy | Tak | Element wchodzi do podstawy opodatkowania |
| Ubezpieczenia społeczne (emerytalne, rentowe, chorobowe, wypadkowe) | Tak | Element wchodzi do podstawy składek społecznych |
| Ubezpieczenie zdrowotne | Tak | Element wchodzi do podstawy składki zdrowotnej |
| Wliczany do podstawy urlopu/ekwiwalentu | Do ustalenia z operatorem | Zależnie od charakteru dodatku (stały/zmienny) — patrz pkt 5 |

## 4. Parametr „Kwota"

Element musi mieć zdefiniowany parametr, z którego pobierana jest wartość:

| Pole parametru | Wartość |
|---|---|
| Nazwa | `Kwota` |
| Typ | Wartość (kwota/waluta) |
| Źródło wartości | Wprowadzana ręcznie przez operatora (na poziomie listy płac / karty pracownika, per okres) |
| Wartość domyślna | 0,00 (brak dodatku, dopóki operator nie wpisze kwoty) |

Przy naliczaniu listy płac operator wpisuje kwotę dodatku bezpośrednio na tym parametrze dla
danego pracownika i okresu — element przyjmuje tę wartość bez przeliczeń.

## 5. Do potwierdzenia przed wdrożeniem

Poniższe punkty zależą od wersji/konfiguracji enova365 w Twoim środowisku i wymagają weryfikacji
na żywym systemie przed użyciem produkcyjnym — nazwy pól i dostępne opcje mogą się nieznacznie
różnić między wersjami:

- Dokładna nazwa pola „Sposób ustalania kwoty" (w niektórych wersjach: „Rodzaj wyliczenia" albo
  „Typ elementu" z opcją „Kwotowy"/„Parametryzowany") — cel jest ten sam: kwota = wartość parametru,
  bez wzoru.
- Czy dodatek ma wchodzić do podstawy wynagrodzenia urlopowego i ekwiwalentu za urlop — to osobne
  ustawienie (checkbox „Podstawa urlopu"), niezależne od opodatkowania/ozusowania, i zależy od tego,
  czy dodatek ma charakter stały (co miesiąc) czy okazjonalny.
- Czy potrzebny jest dodatkowy checkbox „Element do wypłaty" / „Widoczny na pasku wypłaty" —
  standardowo tak dla dodatku pieniężnego, ale warto potwierdzić w konfiguracji listy płac.

Jeśli po sprawdzeniu w systemie okaże się, że wymagany jest niestandardowy algorytm (np. dodatkowa
walidacja kwoty, limit miesięczny, zależność od innego elementu) — wtedy dopiero potrzebny będzie
plik z kodem C# w tym folderze (typ elementu „Algorytm"). Obecna wersja tego celowo nie zakłada,
żeby nie komplikować prostego, ręcznie wprowadzanego dodatku.
