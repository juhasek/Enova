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

```json
{ "WebApi": { "LegacyControllers": { "MethodInvoker": true, "Token": true } } }
```

`Token` tylko, gdy klient pobiera token z `/api/Token`. Po zmianie restart WebApi, potem
`POST http://localhost:9010/api/MethodInvoker/InvokeServiceMethod`.

## Otwarte punkty

- Kod sprawdzony na DLL 2610, serwer ma 2606.2.3: nie zweryfikowano, czy zachowanie jest identyczne.
- Nie testowano na żywym serwerze.
- Nie wiadomo, skąd klient bierze prefiks `/Login/webApi`.
