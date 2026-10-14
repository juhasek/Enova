# Widok „Aktualne dokumenty” – dokumentacja biznesowa

Podwidok listy **Kadry i płace → Kadry → Czas pracy → Dokumenty aktualizacji kalendarzy**.
Te same filtry co lista standardowa (Definicja, Zakres dat, Stan), kolumny:

Aktualny · Data · Numer · Okres · Stan · Kod pracownika · Pracownik · Kod działu · Dział ·
Przełożony · Zatwierdzono (data i godzina)

Kolumny z cech opisanych w [Cechy/Dokument aktualizacji kalendarza.md](../../Cechy/Dokument%20aktualizacji%20kalendarza.md).
Widok listy (viewform) nie ma warunku na wiersze – żeby zostawić tylko obowiązujące dokumenty,
filtruje się kolumnę „Aktualny” = Tak (filtr kolumny w programie).

## Instalacja

`dodaj-widok.ps1 -Baza <nazwa bazy SQL>` (PowerShell 5.1, plik zapisany z BOM):

1. kopia bazy `BACKUP ... COPY_ONLY` (plik `<baza>_przed_widokiem_DAK_<data>.bak` w katalogu kopii SQL Servera),
2. w jednej transakcji: węzeł `Storages/foldery` (jeśli brak), plik folderów listy
   (`kadry i płace.kadry.czas pracy.dokumenty aktualizacji kalendarzy.xml` przycięty od lewej do 40 znaków),
   wpis `Info` widoku i `tableviews/<guid>.viewform.xml` z układem kolumn (HEX UTF-8 z BOM, Type 15).
3. Program widzi widok dopiero po restarcie procesu serwera (`server.dll`) – sam restart
   orchestratora go nie zamyka.

Wymaga wcześniej zainstalowanych cech (inaczej kolumny z cechami będą puste/błędne).

## Status

Baza Claude, 2026-10-14: zainstalowany, program pokazuje podwidok z 11 kolumnami. Na danych
niesprawdzony (brak dokumentów aktualizacji w bazie).
