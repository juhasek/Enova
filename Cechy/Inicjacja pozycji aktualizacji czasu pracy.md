# Inicjacja pozycji aktualizacji czasu pracy – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy cecha

Cecha wyliczana (`GetInitValueInicjacja`), przypisana do tabeli **PozycjaAktualizacjiCzasu**
(`Row`), w module **KalendModule**. Uruchamia się przy zakładaniu dokumentu aktualizacji kalendarza
(np. gdy przełożony składa dokument z aktualizacją kalendarza pracownika) i **inicjuje dni pracy**
całego okresu dokumentu (`Row.Dokument.Okres`) w postaci wierszy `DzienPracyAktualizacja` — po
jednym na każdy dzień okresu, tworzonych w tabeli `PozAktCzasu`.

Dla każdego dnia okresu:

- jeśli dla danego dnia istnieje już wcześniejsza aktualizacja (`DzienPracyAktualizacja`) albo dzień
  z kalendarza pracownika z niezerowym czasem pracy — dzień jest **kopiowany** (`Copy`) razem ze
  swoimi strefami pracy,
- w przeciwnym razie dzień jest **przeliczany od zera** przez
  `Row.ZrodloPlanu.PrzeliczDzieńPracyAktualizacja`.

Cecha działa tylko przy pierwszej inicjacji dokumentu — jeśli pozycja ma już jakiekolwiek
aktualizacje czasu (`Row.AktualizacjeCzasu.Count > 0`), inicjacja jest pomijana, żeby nie nadpisywać
ręcznych zmian wprowadzonych przez użytkownika.

## 2. Zgłoszony błąd i poprawka (31.08.2026) — dublująca się strefa „Nieobecność”

**Zgłoszenie klienta:** cecha poprawnie inicjuje dni pracy, ale gdy w dniu jest nieobecność,
strefa „Nieobecność” dubluje się. Scenariusz: nieobecność zostaje dodana w kalendarzu pracownika
(co samo w sobie poprawnie dodaje strefę „Nieobecność” do dnia), a następnie przełożony składa
dokument z aktualizacją kalendarza — po inicjacji strefa „Nieobecność” występuje w dniu **dwa
razy**.

**Przyczyna:** po skopiowaniu dnia (`dzienakt.Copy(da)` albo `dzienakt.Copy(d)`) do nowo utworzonego
wiersza `dzienakt` trafiają wszystkie strefy dnia źródłowego — w tym już istniejąca strefa
„Nieobecność” (dodana wcześniej w kalendarzu). Mimo to kod bezwarunkowo wykonywał dalej blok:

```csharp
INieobecnosc nb = kp.Nieobecnosc(data);
if (nb != null && dzienakt != null)
{
    for (int i = 0; i < defStrefy.Distinct().Count(); i++)
    {
        // ręczne dodanie kolejnej strefy "NB"
    }
}
```

Ponieważ `kp.Nieobecnosc(data)` nadal zwracał tę samą nieobecność, pętla dokładała **kolejne**
strefy „NB” do dnia, który już miał tę strefę skopiowaną ze źródła — stąd duplikat. Ręczne
dokładanie stref „NB” ma sens tylko w gałęzi, w której dzień nie ma żadnego źródła do skopiowania
(czyli jest przeliczany od zera przez `PrzeliczDzieńPracyAktualizacja`, które w tym momencie nie
uwzględnia jeszcze nieobecności) — a mimo to wykonywało się też po `Copy`.

**Poprawka:** dodano flagę `skopiowanoZeZrodla`, ustawianą na `true` w obu gałęziach `Copy` i na
`false` w gałęzi `PrzeliczDzieńPracyAktualizacja`. Ręczne dokładanie stref „NB” wykonuje się teraz
tylko wtedy, gdy `!skopiowanoZeZrodla` — czyli wyłącznie dla dni bez istniejącego źródła, dla
których dzień trzeba było zbudować od zera i nieobecność nie jest jeszcze na nim odzwierciedlona.
Dla dni skopiowanych ze źródła strefa „Nieobecność” pochodzi wyłącznie z `Copy` i nie jest już
dokładana drugi raz.

## 3. Do potwierdzenia / obserwacji na przyszłość

- `defStrefy` jest budowane **raz, przed pętlą po dniach**, jako suma stref występujących w
  *całym okresie dokumentu* (`Row.Dokument.Okres`), a nie osobno dla każdego dnia. W efekcie w
  gałęzi „przeliczenie od zera” liczba dokładanych stref „NB” dla konkretnego dnia zależy od tego,
  jakie strefy pracy występują gdziekolwiek w całym okresie, a nie od tego, co faktycznie powinno
  wystąpić danego dnia. Nie było to przyczyną zgłoszonego dublowania i nie zostało zmienione w tej
  poprawce, ale warto to zweryfikować pod kątem zamierzonego zachowania, jeśli okres obejmuje dni o
  różnym układzie stref.
- Kod zawiera zakomentowany fragment diagnostyczny (log dla konkretnego pracownika/daty) — pozostał
  bez zmian, nieaktywny.
