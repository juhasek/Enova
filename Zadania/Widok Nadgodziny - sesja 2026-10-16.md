# Widok „Nadgodziny” — sesja 2026-10-16

## Stan

Widok globalny **Kadry → Ewidencje → Dokumenty dodatkowe → Nadgodziny** w bazie Al — założony SQL-em
(CfgNodes 4246 folder + 4247 viewform, 9 kolumn e-wniosku), widoczny w programie po restarcie `server.dll`.
Kopia bazy sprzed zmiany: `Al_przed_widok_nadgodziny_20261016.bak`.

Repo: commit d319203 — skrypt i opis kolumn w `Widoki/Nadgodziny (Dokumenty dodatkowe)/`.

## Decyzja operatora

Widok zaakceptowany w obecnym kształcie (bez zmian kolumn, bez dalszych prac nad zawężeniem do jednej
definicji). Sprawdzenie na danych operator robi u klienta — w Al nie ma żadnych dokumentów dodatkowych.

## Do sprawdzenia u klienta

1. Czy kolumny wypełniają się na rzeczywistych wnioskach o nadgodziny (po wybraniu w filtrze właściwej
   definicji — filtr nie jest zapamiętywany).
2. Kolumna **Do przeniesienia** — jeśli pusta, w definicji klienta to cecha, a nie pole wniosku:
   zmienić EditValue na `{Features.DoPrzeniesienia}` w skrypcie i w bazie.
