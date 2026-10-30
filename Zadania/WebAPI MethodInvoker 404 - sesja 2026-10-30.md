# WebAPI: 404 na InvokeServiceMethod - sesja 2026-10-30

## Cel

Analiza logu startu serwera 2606.2.3 (działa na innym komputerze) po zmianie `appsettings.json`
oraz błędu 404 na `http://localhost:9010/Login/webApi/api/MethodInvoker/InvokeServiceMethod`.

## Ustalenia

- Log startu czysty: same wpisy `Information`, wszystkie komponenty dostały właściwy `-c`.
  We fragmencie nie widać startu serwera aplikacji.
- Port 9010 to domyślny port WebApi.
- Przyczyna 1 - zła ścieżka. `webapi.dll` nie ustawia PathBase, a kontroler ma trasę
  `api/[controller]`, więc prefiks `/Login/webApi` powoduje 404.
- Przyczyna 2 - `WebApi:LegacyControllers:MethodInvoker` ma domyślnie `false`.
  `ConditionalControllerConvention` usuwa wtedy trasy kontrolera przy starcie.
- Po poprawieniu samej ścieżki przy wyłączonym kontrolerze żądanie trafia do
  `DynamicApiMiddleware`, który bez uwierzytelnienia zwraca 401.
- Metoda ma `[Obsolete]`: enova zaleca dedykowane kontrolery.

## Rozwiązanie

W nakładce `-c` dopisać do sekcji `WebApi` w korzeniu pliku (nie do
`Orchestrator:Process:Composition:WebApi`):

```json
"LegacyControllers": { "MethodInvoker": true }
```

`Token` nie jest potrzebny: kontroler nie ma `[Authorize]`, a dane logowania bierze z ciała żądania
(`DatabaseHandle`, `Operator`, `Password`) albo, gdy `Operator` i `Password` są puste, z sekcji
`WebApi:Invokers` po `InvokerName` = `ServiceName`. Po zmianie restart, potem
`POST http://localhost:9010/api/MethodInvoker/InvokeServiceMethod`.

## Wynik

Operator potwierdził, że wywołanie działa, po włączeniu kontrolera i usunięciu prefiksu z adresu
(wcześniej logi WebApi pokazywały 404 na `/Login/webApi/api/...`, `/Login/api/...` i `/Login//api/...`).
Pełna konfiguracja: `Zadania/Konfiguracja serwera 2606.2.3/appsettings.json`.

## Otwarte punkty

- CommHub (Garnet) nasłuchuje na `tcp://+:4000` bez uwierzytelniania: przy serwerze dostępnym z sieci
  zablokować port albo ustawić uwierzytelnianie.
- Web ma `h2` na `http://+:80`: bez proxy przeglądarka może nie otworzyć strony (niesprawdzone).
