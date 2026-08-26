# SEOP Enrollment File – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy raport

Raport generuje plik „Enrollment file" dla programu SEOP (Skanska Employee
Ownership Plan / akcje pracownicze). Zgodnie z wymaganiem klienta plik ten
ma zawierać **wyłącznie** osoby, które w raportowanym okresie:

- po raz pierwszy przystąpiły do programu SEOP, **lub**
- po raz pierwszy przystąpiły do nowej edycji SEOP.

Nie jest to lista wszystkich aktualnych uczestników programu — to plik
zgłoszeniowy nowych przystąpień, wysyłany do operatora planu.

## 2. Parametry raportu

| Parametr | Opis |
|---|---|
| **Okres** | Miesiąc (domyślnie bieżący), za który generowany jest plik. |

## 3. Poprawka: raport pokazywał wszystkich uczestników zamiast tylko nowo przystępujących

**Objaw:** w sierpniu do aktualnej edycji SEOP przystąpił jeden pracownik, a
raport wygenerował ponad 200 osób zamiast tylko tego jednego pracownika.

**Przyczyna:** pętla po pracownikach znajdowała edycję SEOP obowiązującą dla
oddziału pracownika w raportowanym okresie, a następnie sprawdzała jedynie,
czy pracownik **w ogóle** jest uczestnikiem tej edycji
(`edycja.Uczestnictwa.FirstOrDefault(u => u.Pracownik == p)`). Warunek
odsiewający po „aktualności" uczestnictwa był w kodzie zakomentowany. W
efekcie do raportu trafiał każdy aktywny uczestnik danej edycji, niezależnie
od tego, kiedy faktycznie do niej przystąpił — stąd 200+ osób zamiast
jednej.

**Poprawka:** każde przystąpienie do programu (czy to pierwsze w ogóle, czy
do kolejnej edycji) tworzy nowy wiersz `UczestnictwoWAkcji`, którego
`OkresOd` to data przystąpienia. Dodano warunek, że do raportu trafiają
tylko uczestnictwa, których `OkresOd` mieści się w raportowanym okresie
(`pars.Okres.Contains(uczestnictwo.OkresOd)`). Uczestnicy kontynuujący
wcześniej rozpoczęte uczestnictwo (z `OkresOd` sprzed raportowanego okresu)
są pomijani.

```csharp
if (!pars.Okres.Contains(uczestnictwo.OkresOd))
{
    log.WriteLine($"Uczestnictwo pracownika rozpoczęło się poza raportowanym okresem (OkresOd={uczestnictwo.OkresOd}) - pomijam");
    continue;
}
```

## 4. Znane ograniczenia / do potwierdzenia

- Filtr opiera się na założeniu, że `UczestnictwoWAkcji.OkresOd` to
  rzeczywista data przystąpienia pracownika do danej edycji (a nie np. data
  początku samej edycji, jednakowa dla wszystkich jej uczestników). Warto
  zweryfikować to założenie na danych produkcyjnych — jeśli `OkresOd`
  wszystkich uczestników edycji jest identyczny (np. równy początkowi
  edycji), filtr nie zadziała poprawnie i trzeba będzie znaleźć inne pole
  (np. datę utworzenia wiersza `UczestnictwoWAkcji`) jako źródło daty
  przystąpienia.
- Pozostaje otwarty, niezwiązany z tą poprawką problem opisany w kodzie:
  pętla pobiera historię pracownika na dzień `pars.Okres.From` (początek
  miesiąca), co może pomijać pracownika, który zaczął pracę w trakcie
  raportowanego miesiąca.
