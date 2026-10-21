# Import definicji elementów wynagrodzenia – plan prac

Narzędzie: formularz HTML dla działu kadr klienta → plik `elementy-wynagrodzenia.json` →
dodatek enova „Importuj definicje z pliku”. Jedno źródło prawdy: `Addon/ImportDefinicjiElementow/schema/definicja-elementu.schema.json`.

Założenia (do potwierdzenia przy STOP po etapie 1):
- wersja enova schematu: **2610** (serwer lokalny `C:\enovaServer\2610`); paczki NuGet 2610 nie mieszczą się na dysku
  (2 GB wolnego) – generator czyta DLL serwera w runtime, dodatek zbudujemy na wersji wskazanej przez użytkownika;
- instrukcja: `Pobrane/enova365_Kadry-Płace-i-HR_instrukcja.pdf`, rozdział „Elementy wynagrodzenia” s. 1668–1764
  (tekst w `Wzor_analizy/_robocze/instrukcja.txt`);
- licencja klienta wybierana w formularzu (srebrna / złota / platynowa) – warunki `{"licencja": …}` w schemacie.

## Etap 1 – inwentaryzacja (GOTOWE, czeka na akceptację)
- [x] Generator schematu (`Generator/`, .NET 10, refleksja na DLL serwera + zasoby pageform/form/dbinit)
- [x] Zakładki i sekcje jak w oknie enova (warianty Pro = nadzbiór), aliasy nakładek, pola pominięte z powodem
- [x] Typy, wymagalność (`[Required]`), długości (`[MaxLength]`), wartości początkowe rekordu, reguły IsVisible/IsReadOnly/GetList
- [x] Enumy z etykietami enova, słowniki z wartościami standardowymi (PIT, RSA, tytuły ubezpieczeń, limity, strefy, listy płac, 255 definicji standardowych)
- [x] `opisy.json`: etykiety, opisy dla kadrowej (instrukcja), warunki widoczności/edytowalności/wymagalności, dozwolone wartości
- [x] `PRZEGLAD.md` + lista pól do weryfikacji + domyślne do weryfikacji
- [x] Wartości domyślne sprawdzone na żywo (baza Claude, 2026-10-21) – zgodne lub poprawione w opisy.json
- [ ] Decyzje: pominięte zakładki (Nazwy parametrów, Pomniejszenia nieobecności), ukryte rodzaje elementów, wersja dodatku

## Etap 2 – formularz HTML dla klienta (GOTOWE)
- [x] Szablon strony (vanilla JS, offline) + `build.mjs` wstawiający schemat (okrojony do potrzeb klienta) do jednego HTML
- [x] Lista definicji (nowa/z szablonu, duplikuj, usuń), zakładki jak w enova, „?” z opisem i opisami wartości, podpowiedzi
- [x] Silnik warunków DSL (`src/warunki.js`) – widoczność, edytowalność, wymagalność, dozwolone wartości, licencja, domyślne
- [x] Walidacja na żywo (wymagane, typy, zakresy, wzorce, unikalność nazwy/skrótu, nazwy standardowe), liczniki błędów, potwierdzenie eksportu z błędami
- [x] 6 szablonów z definicji standardowych enova (zasadnicze, premia uznaniowa, procentowa, stażowy z progami, funkcyjny, potrącenie)
- [x] Uwagi dla wdrożeniowca, eksport/import JSON (wersja schematu, data, zastąp/dołącz), autozapis localStorage (try/catch)
- [x] Testy: 9 testów silnika (node:test) + E2E w Edge headless (zrzuty, eksport, import, autozapis)

## Etap 3 – dodatek enova: import
- [ ] Projekt dodatku (Soneta.MsBuild.SDK / Soneta.Sdk, bez HintPath), schemat jako zasób
- [ ] Worker na liście definicji elementów: „Importuj definicje z pliku”
- [ ] Wczytanie + wersja schematu, walidacja (wymagane, enumy, słowniki w bazie), podgląd nowe/aktualizacje/błędy z różnicami
- [ ] Zapis w jednej transakcji (tylko nowe / nowe i aktualizacja), ochrona definicji standardowych, raport eksportowalny
- [ ] Testy jednostkowe mapowania JSON → DefinicjaElementu i walidacji
- [ ] README: build, przekazanie formularza, import, aktualizacja schematu po upgradzie enova
