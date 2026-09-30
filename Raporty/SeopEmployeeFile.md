# SEOP Employee File – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy raport

Raport generuje plik „Employee file" dla programu akcji pracowniczych SEOP — plik
z danymi osobowymi i adresowymi uczestników, wysyłany do operatora planu.

Zgodnie z wymaganiem klienta plik ma zawierać osoby, które w raportowanym okresie:

- po raz pierwszy przystąpiły do programu SEOP,
- po raz pierwszy przystąpiły do nowej edycji SEOP,
- **zostały zwolnione, a kiedykolwiek w trakcie zatrudnienia były uczestnikami SEOP.**

Zmiany danych (np. adresu zamieszkania, nazwiska) nie są obsługiwane przez raport —
klient zgłasza je i wprowadza ręcznie, po zgłoszeniu przez uczestnika.

## 2. Parametry raportu

| Parametr | Opis |
|---|---|
| **Okres** | Miesiąc (domyślnie bieżący), za który generowany jest plik. |

Dane kadrowe (historia, etat, adres, oddział) czytane są na **koniec** okresu
(`pars.Okres.To`).

## 3. Kto trafia do pliku – reguły filtrowania

Do pliku trafia pracownik, dla którego spełniony jest **przynajmniej jeden** z warunków:

| Warunek | Skąd liczony |
|---|---|
| `przystapilWOkresie` | `OkresOd` aktualnego uczestnictwa mieści się w raportowanym okresie |
| `zrezygnowalWOkresie` | `OkresDo` aktualnego uczestnictwa mieści się w raportowanym okresie |
| `zakonczylPraceWOkresie` | data końca zatrudnienia w okresie **i** pracownik kiedykolwiek był uczestnikiem |

Aktualne uczestnictwo wyznacza `SeopPracownikWorker` + `AktualnyParametrUczestnictwaWorker`
na dzień `pars.Okres.To`.

## 4. Poprawka: osoby zwolnione wypadały z pliku

**Objaw:** pracownicy zwolnieni w raportowanym miesiącu nie pojawiali się w pliku,
mimo że byli uczestnikami programu.

**Przyczyna – dwa niezależne miejsca:**

1. Warunek „zakończył pracę" wymagał, żeby pracownik miał **aktualne** uczestnictwo
   na koniec okresu (`dataPrzystapienia != null`, liczone przez
   `AktualnyParametrUczestnictwaWorker` na dzień `Okres.To`). Uczestnictwo osoby
   zwolnionej jest zamykane z datą końca umowy (patrz
   `Zadania/Aktualizacja OkresuDo w UczestnictwieWAkcji wg daty zakończenia umowy.md`),
   więc na koniec miesiąca nie było już aktualne — worker zwracał `null` i pracownik
   wypadał z filtra. Tym bardziej wypadała osoba, która zrezygnowała z programu
   wcześniej, w czasie trwania zatrudnienia.
2. Wcześniejszy warunek `if (edycja == null) continue;` odsiewał każdego pracownika,
   dla którego oddział nie miał edycji akcji obejmującej raportowany okres — a osoba
   zwolniona mogła być uczestnikiem edycji już zakończonej.

**Poprawka:**

- „kiedykolwiek był uczestnikiem" liczone jest z **historii** uczestnictw, a nie
  z uczestnictwa aktualnego: wystarczy dowolny wiersz `UczestnictwoWAkcji` pracownika
  (`AkcjePracA1Module...UczesWAkcji.WgPracownik[p]`), niezależnie od tego, do której
  edycji należy i kiedy został zamknięty;
- filtr edycji oddziału odsiewa już tylko kandydatów na przystąpienie/rezygnację —
  osoba zwolniona będąca uczestnikiem trafia do pliku także bez bieżącej edycji;
- sprawdzenie historii uczestnictw uruchamiane jest wyłącznie dla osób zwolnionych
  w okresie, więc nie obciąża przebiegu dla pozostałych pracowników.

```csharp
bool kiedykolwiekUczestnik = false;

if (zwolnionyWOkresie)
{
    foreach (UczestnictwoWAkcji u in akcjeModule.UczesWAkcji.WgPracownik[p])
    {
        kiedykolwiekUczestnik = true;
        break;
    }
}

bool zakonczylPraceWOkresie = zwolnionyWOkresie && kiedykolwiekUczestnik;
```

## 5. Poprawka: kolumna Termination_Date przy umowie bezterminowej

Koniec okresu zatrudnienia przy umowie bez daty końcowej ma w enova wartość skrajną
(`Date.MaxValue`), która w kolumnie `TerminationDate` wychodziła jako `31-Dec-9999`.
Dla osób przystępujących do programu (nadal zatrudnionych) kolumna jest teraz pusta —
data pojawia się tylko wtedy, gdy zatrudnienie faktycznie ma koniec. Ta sama
znormalizowana wartość steruje filtrem „zwolniony w okresie".

## 6. Poprawka: brak adresu zamieszkania

Adres zamieszkania (`historia.AdresZamieszkania`) nie zawsze jest uzupełniony
w kartotece — wtedy kolumny adresowe wychodziły puste. Jeśli adres zamieszkania nie
ma miejscowości, dane adresowe brane są z adresu zameldowania (`AdresZameldowania`) —
ten sam subrow `Soneta.Core.Adres`, więc te same pola.

## 7. Znane ograniczenia / do potwierdzenia

- **Poprawki z punktów 4–5 nie zostały sprawdzone na żywej aplikacji** — środowisko
  robocze repo nie ma biblioteki dodatku akcji pracowniczych, więc kod nie był
  kompilowany ani uruchomiony. Przed wysyłką pliku do operatora warto wygenerować
  raport za miesiąc, w którym wiadomo, że ktoś został zwolniony, i sprawdzić, czy
  jest na liście.
- Warunek „kiedykolwiek był uczestnikiem" jest celowo szeroki: wystarczy **dowolny**
  wiersz `UczestnictwoWAkcji`, również z okresu sprzed wielu lat. Jeśli klient będzie
  chciał ograniczyć go np. do uczestnictw mieszczących się w bieżącym okresie
  zatrudnienia (istotne przy ponownym zatrudnieniu tej samej osoby), trzeba dołożyć
  porównanie `uczestnictwo.OkresOd` z `Etat.OkresZatrudnienia.From`.
- `Nationality` czytane jest niespójnie: przy kodzie `PL` wpisywane jest `POL`
  z obywatelstwa, ale w przeciwnym razie brany jest kod kraju z **adresu
  zamieszkania**, nie z obywatelstwa. Do potwierdzenia z klientem, które źródło jest
  właściwe dla cudzoziemców.
- Dane kadrowe czytane są na koniec okresu — pracownik, którego historia na ten dzień
  nie ma jeszcze etatu (np. skrajne przypadki wielu zapisów historycznych), będzie
  miał puste kolumny oddziałowe.
