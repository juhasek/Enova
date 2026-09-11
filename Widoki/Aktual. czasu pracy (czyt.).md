# Aktualizacja czasu pracy — czytelna wersja (dokumentacja biznesowa)

Nazwa w enova (`DefZestawCzasu.Nazwa`, limit 30 znaków): **„Aktual. czasu
pracy (czyt.)"**.

## 1. Punkt wyjścia

Klon systemowego zestawienia enova **„Aktualizacja czasu pracy"** (`DefZestawCzasu`,
GUID systemowy `00000000-0040-0001-0005-000000000000`, ID=4 w bazie Claude) —
siatka do ręcznego wstawiania/poprawiania godzin pracy pracowników w dokumencie
aktualizacji kalendarza. Zgłoszenie: użytkownicy oceniają widok jako wizualnie
nieprzyjazny.

Ponieważ oryginał jest obiektem **systemowym** Soneta, nie modyfikujemy go
bezpośrednio (nadpisanie zniknęłoby przy aktualizacji enova i wpłynęłoby na
wszystkie firmy w bazie) — powstał klon z nowym GUID, pod osobną nazwą, w
tabeli `DefZestawCzasu`.

## 2. Zmiany wizualne (bez zmiany logiki biznesowej)

Struktura kolumn i cała logika wyliczania danych pozostały identyczne jak w
oryginale — zmieniono wyłącznie to, co wpływa na czytelność:

- **Domyślny kolor tekstu komórki** (`GetForeColor`, fallback gdy dzień nie ma
  żadnej definicji) — był `Color.DeepPink` (jaskrawy róż, wyglądający jak
  niedopracowany kolor błędu/debugowy), zmieniony na neutralną czerń.
- **Kolor tła nieobecności bez własnego koloru** (`GetBackColor`, fallback
  gdy `Definicja.BackColor == 0`) — był pełny, jaskrawy `Color.Orange`,
  zmieniony na pastelowy bursztyn (`RGB 255,214,153`) — ten sam sygnał
  („nieobecność"), mniej męczący przy dłuższej pracy z siatką.
- **Kolumna „Info"** — pierwotne kryptonimy `Del`/`Nb`/`OWn`/`PZ` (nie do
  odgadnięcia bez znajomości kodu) zamienione na czytelne skróty: `Deleg.`
  (delegacja), `Nieob.` (nieobecność bez własnego kodu), `Wniosek`
  (oczekujący wniosek urlopowy), `Zdalna` (praca zdalna). Szerokość kolumny
  zwiększona z 3 do 7 znaków, żeby się nie ucinały.
- **Kolumny „Praca / Plan (okres)" i „(rozl)"** — szerokość zwiększona z
  14/13 do 16 znaków, żeby sklejony format `"8:00 / 8:00"` nie był
  obcinany przy dłuższych wartościach (np. nadgodziny dwucyfrowe).

Kolory oznaczające dni wolne/świąteczne (`KoloryKalendarza.DzieńWolnyTło`,
`DzieńŚwiątecznyTło`) oraz kolory z definicji stref/nieobecności (gdy
ustawione w konfiguracji) pozostały **bez zmian** — są systemowe i spójne z
resztą aplikacji, nie było powodu ich ruszać.

## 3. Czego NIE zmieniono (świadomie, poza zakresem zgłoszenia)

- Format godzin „Od"/„Czas" jako wolny tekst (parsowany przez `Time.Parse`)
  — zgłoszenie dotyczyło wyglądu, nie sposobu edycji.
- Brak osobnej kolumny „Do" (godzina końcowa) — wymagałoby zmiany logiki,
  nie tylko wyglądu.
- Zakomentowana kolumna „Typ dnia" — pozostawiona wyłączona jak w oryginale.
- Legenda kolorów jako osobny element UI — enova nie daje prostego
  mechanizmu na statyczną legendę w tego typu siatce; do rozważenia jako
  osobne zgłoszenie, jeśli nadal potrzebne po tej poprawce.

## 4. Status

Wstawiony jako nowy rekord w `DefZestawCzasu` w bazie **Claude** (sandbox
localhost\SQLEXPRESS), obok oryginału — nie podmienia systemowego widoku.

**Zgłoszony błąd (11.09.2026)** — po otwarciu dokumentu aktualizacji
kalendarza i przejściu na zakładkę tego zestawienia: `[MultiSources[0].
Zestawienie.GetSource() GET]: Object reference not set to an instance of
an object.` Przyczyna: pierwszy insert do bazy (`sqlcmd -i plik.sql -f
65001`) uszkodził rzadsze polskie znaki w polu `Algorytm` (np. `Źródło` →
zniekształcone bajty), mimo że plik źródłowy na dysku miał poprawny UTF-8
— zniekształcony tekst C# najwyraźniej nie kompilował się poprawnie w
silniku skryptowym enova, co ujawniło się dopiero jako myląca
`NullReferenceException` przy renderowaniu zakładki, nie jako błąd
składni. Szczegóły mechanizmu: [[reference_sqlcmd_f65001_mangles_niektore_znaki]]
w pamięci.

**Poprawka:** tekst algorytmu wstawiony ponownie przez PowerShell +
`System.Data.SqlClient` (`SqlParameter` typu `NText`, wartość z
`Get-Content -Raw -Encoding UTF8`) — bez pośredniej konwersji przez
tekstowy SQL. Zweryfikowano odczytem z powrotem przez ADO.NET: treść w
bazie jest teraz bajt-w-bajt identyczna z plikiem w repo. Rekord
zaktualizowany (`UPDATE ... WHERE ID = 9`), nie usunięty i wstawiony na
nowo — usunięcie blokował klucz obcy z tabeli `ZestAktKalend` (enova
najwyraźniej sama utworzyła tam wiersz łączący zakładkę dokumentu z tym
`DefZestawCzasu` przy pierwszym otwarciu).

**Do potwierdzenia przez użytkownika:** czy po poprawce zakładka otwiera
się bez błędu w GUI — środowisko robocze tego repo nie ma dostępu do
buscall/testu na żywej aplikacji.
