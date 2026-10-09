# Dodatek A1.RezerwyUrlopowe (enova365 2512.5.6)

Implementacja planu `Addon/Plan RezerwyUrlopowe/` (etapy 1–3, todo.md). Moduł `Rezerwy`, przestrzeń nazw `A1.Rezerwy`.
Działa **obok** rozwiązania skryptowego `Rezerwy urlopowe/` (własne GUID-y, nic nie nadpisuje).

## Budowanie
- Soneta.Sdk 1.2.0 + biblioteki Soneta 2512.5.6 z **nuget.org** (`global.json`, `Directory.Build.props`, `NuGet.Config`), target `net8.0`.
- `dotnet restore --disable-parallel`, potem `dotnet build`. Generator tworzy `Rezerwy.business.cs` z `Rezerwy.business.xml` (plik w .gitignore).
- W Claude Code restore wymaga uruchomienia poza piaskownicą poleceń (w piaskownicy NuGet zgłasza „Nieznany host”).

## Stan
| Krok todo | Stan |
|---|---|
| 1. Rusztowanie | solucja `A1.RezerwyUrlopowe.sln`: logika `A1.RezerwyUrlopowe` + `A1.RezerwyUrlopowe.UI` (Tests — krok 13) |
| 2. business.xml | `RezerwaUrlopowa` (RezerwyUrlopowe), `PozycjaRezerwyUrlopowej` (PozRezerwUrlop), subrow `ParametryNaliczenia`, enumy — build 0 błędów / 0 ostrzeżeń |
| 3. rightstree | `RezerwyUrlopowe.rightstree.xml` — gałąź Kadry i płace/Rezerwy urlopowe |
| 4. Konfiguracja | `UstawieniaRezerwUrlopowych` (CfgNodes A1/Rezerwy urlopowe), strona Opcji `Config.RezerwyUrlopowe.pageform.xml` (XSD OK) |
| 5. Kalkulator | `KalkulatorRezerwyUrlopowej` + `SymulatorLimituUrlopu` — port 1:1 ze skryptu, parametry z konfiguracji, Log("Rezerwa urlopowa") |
| 6. Algorytm elementu | `AlgorytmRezerwy : AlgorytmBase` (Klasa algorytmu `A1.Rezerwy.AlgorytmRezerwy`, limit pola 30 znaków), wyniki dla czynności przez `RejestrWynikowRezerwy` |
| 7. Dane inicjujące | `Dane/RezerwyUrlopowe.dbinit.xml` (wersja A1Rezerwy 1): 2 elementy „(dodatek)”, planowane listy AREZURL / ABUDREZURL — własne GUID-y; struktura OK, import próbny do wykonania przy teście instalacji |
| 8. Weryfikatory | `BudzetPozaSierpniemVerifier` (Warning), `UjemnaRezerwaVerifier` (Error), pozycje zamkniętej rezerwy tylko do odczytu, `KontrolaKonfiguracji` (lista błędów przed naliczeniem); unikalność — klucze w bazie |
| 9. Naliczenie | `NaliczRezerweWorker` (lista Pracownicy, zaznaczeni) i `NaliczRezerweWszystkimWorker` (lista Rezerwy urlopowe, zatrudnieni); logika `NaliczanieRezerwyUrlopowej`: kontrole → nagłówek → standardowe naliczenie planowanej listy → pozycje → sumy |
| 10. Zamknięcie | `ZamknijRezerweWorker`, `OtworzRezerweWorker` (prawo specjalne — kwestia 11 otwarta); pozycja: `KwotaPoprzednia`, `Zmiana` |
| 11. UI | projekt `A1.RezerwyUrlopowe.UI`: foldery „Kadry i płace/Płace/Rezerwy urlopowe” (+ „- pozycje”), formularze rezerwy (Ogólne, Pozycje, Parametry) i pozycji (Ogólne, Zapis obliczeń), zakładka w kartotece pracownika, strona Opcji — 9 formularzy zgodnych z XSD |
