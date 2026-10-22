# Dokumenty aktualizacji kalendarza (DAK) — sesja 2026-10-22

Ciąg dalszy sesji z [2026-10-14](Dokumenty%20aktualizacji%20kalendarza%20-%20sesja%202026-10-14.md): cecha
elementu struktury, raport dla PIP bez DLL, test na danych w bazie **Claude**, instrukcja korekty dla klienta,
uwzględnienie procesu klienta A1_DokAkt. Wszystko przeniesione na `main` (PR #25, #26, #27).

## 1. Wyniki

| Co | Gdzie | Commit / PR |
|---|---|---|
| Cecha `ElementStruktury` (element struktury org. z dokumentu, `Row.Powiazanie.ElementStrukturyOrganizacyjnej`) + kolumna w „Aktualne dokumenty”; skrypt widoku umie aktualizować istniejący widok | [Cechy/Dokument aktualizacji kalendarza.md](../Cechy/Dokument%20aktualizacji%20kalendarza.md) | f6f3291, PR #25 |
| Raport PIP: cechy `ZlozylKto/Kiedy/Rola`, `ZatwierdzilKto/Kiedy/Rola` (z zadań procesu: `Task.Executor`, `ExecutionTime`) + widok „Ewidencja zatwierdzeń (PIP)” | [Widoki/…Ewidencja zatwierdzen PIP](../Widoki/Dokumenty%20aktualizacji%20kalendarzy%20-%20Ewidencja%20zatwierdzen%20PIP/Ewidencja%20zatwierdzen%20PIP.md) | c251c38, d464646, PR #26 |
| Poprawka `Aktualny`: zatwierdzony dokument bez własnych dni = Tak (wcześniej Nie) | jw. | c251c38 |
| Kroki procesu po **nazwie** zadania (standard + A1_DokAkt „… 2”) zamiast Guid | jw. | dc7bb0d, PR #26 |
| Instrukcja klienta „Korekta czasu pracy przed wypłatą” (9 zrzutów) | `Weryfikatory czasu pracy/Instrukcja_Korekta_czasu_pracy_DAK.docx` | 23d22e4, PR #27 |

Generator instrukcji (`gen_instrukcja.ps1`, `obrobka.ps1` – przycięcie zrzutów i czerwone ramki) jednorazowy,
w scratchpadzie sesji – poza repo. Styl z `Instrukcja_Weryfikatory.docx`.

## 2. Ustalenia

- **Kto i kiedy**: dokument pamięta tylko stan, `DataZatwierdzenia` i `Odpowiedzialny`. Wykonawcę i czas każdego
  kroku zapisuje proces (zadania powiązane `TaskLinkedObjs.WgLinkedObject[dokument]`). Pracownik z pulpitu
  jest wykonawcą jako Pracownik (`Task.Executor` zamienia użytkownika WWW na Host) – na żywo niesprawdzone.
- **Pracownicy dokumentu „Dowolnego źródła”** pochodzą wyłącznie z **elementu struktury organizacyjnej**
  (i podelementów) wskazanego przy zakładaniu – nie z wydziału. „Jednostki organizacyjne” daje pustą listę.
- Definicja elementu z typem źródła powiązania „Pracownicy” → błąd „Powiązania można dodawać tylko do wierszy
  typu Soneta.Kadry.Pracownik” (to był błąd z 14.10). W Al struktura „Pulpity HR” ma typ (Wszystkie).
- Zatwierdzanie: zadanie „Do akceptacji” przypisane do roli **„Kadry - zatwierdzanie czasu”**; definicja dokumentu
  wymaga nadania prawa (wiersz `Rights`), inaczej brak pod „Dodaj”.
- **A1_DokAkt** (proces klienta, eksport XML od użytkownika): kopia standardu z zadaniami „Wypełniany 2”,
  „Do akceptacji 2”, „Poprawiany 2”, „Zatwierdzony 2”, „Anulowany 2”; z Wypełniany 2 przycisk **„Zatwierdź”**;
  Do akceptacji 2 → kierownik (`KalkulatorPodwladnych.GetKierownik`) dla def. „Składającego”, inaczej rola kadr.
  U klienta standard ma tylko „Czas pracy” (CZ/PRAC), pozostałe definicje (CZ/PRAC/ADM, CZ/PRAC/POD, CZ/ZL,
  CZ/ZL/ADMIN, CZ/ZL/POD, PL/PRAC/ADM, PL/PRAC/POD) – A1_DokAkt.
- Import A1_DokAkt do Claude niemożliwy: brak licencji modułu Workflow (eksport ma też pole `SonetaExplorer`
  nieznane w 2610).
- Lokalna kopia bazy **Al**: żaden kalendarz nie ma `WersjonowanieCzas`; definicje DAK bez powiązania ze strukturą
  i tylko proces standardowy – starsza niż produkcja klienta.

## 3. Test na danych (baza Claude)

- PL/DO/2026/10/1 zatwierdzony przez MCP (zakładka „Obsługa procesu” → „Zatwierdź”).
- CZ/DO/2026/10/1 założony dla elementu „Pracownik” (4 osoby), poprawione TS-01 02.10 → 7:00–15:00,
  TS-02 05.10 → 8:00–14:00, przekazany i zatwierdzony; w `DniPracy` dokładnie te dwa dni.
- Widok PIP: oba dokumenty, Złożył/Zatwierdził = Administrator, rola „Inny operator”.

## 4. Zmiany w bazie Claude

- Administrator dopisany do roli „Kadry - zatwierdzanie czasu”.
- `Rights` 10572 – prawo Administratora do definicji CZ/DO (zgoda użytkownika).
- Definicja CZ/DO: Powiązanie = Elementy struktur organizacyjnych, struktura „Pulpity - kierownik i pracownicy”.
- Widok PIP (tableviews 4251), kopie: `Claude_przed_widokiem_DAK_20261022_1005.bak`, `…_PIP_20261022_1046.bak`.
- Restart procesu `server.dll` (cache praw i widoków).
- Wyczyszczony `%USERPROFILE%\.nuget\packages` (brak miejsca na C:).

## 5. Decyzje użytkownika

- Raport PIP jako **nowy widok** (bez wydruku DevExpress), obejmuje plan i czas pracy.
- Instrukcji korekty **nie zmieniać** pod A1_DokAkt (opisuje proces standardowy).

## 6. Otwarte

1. Wdrożenie u klienta: cechy + kod wspólny + widok PIP; kalendarze z `WersjonowanieCzas`; powiązanie definicji
   kadr ze strukturą; rola i prawa.
2. Test A1_DokAkt na bazie z licencją Workflow (kolumny PIP, przyciski „Zatwierdź” ×2).
3. Test obiegu z pulpitu pracownik → kierownik (wykonawca jako Pracownik / Przełożony).
4. `RolaWykonawcy` porównuje z domyślnym przełożonym (struktura podległości); A1_DokAkt wybiera kierownika
   przez `KalkulatorPodwladnych` – przy różnych strukturach rola może wyjść „Inny pracownik”.
