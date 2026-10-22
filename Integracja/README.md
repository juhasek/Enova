# Integracja

Analizy, specyfikacje i materiały testowe integracji enova365 z systemami zewnętrznymi
(WebAPI, wymiana plików, synchronizacje). Każda integracja ma własny podkatalog.

| Katalog | Zawartość |
|---|---|
| [WebAPI-Kadry/](WebAPI-Kadry/) | Dodatek WebAPI zasilający kadry (pracownicy, rozwiązania umów, wydziały) + biblioteka WCF.Core: [analiza](WebAPI-Kadry/Analiza.md), [mapowanie pól źródłowych](WebAPI-Kadry/Mapowanie%20pól.md), scenariusze testowe (xlsx), kolekcja Postman, generator. |

Zasady:
- surowe pliki od klienta (DLL, eksporty, logi) zostają w `Pobrane/` — tu trafiają tylko
  wnioski i artefakty wynikowe;
- uzupełnionych środowisk Postman (hasła, adresy produkcyjne) nie commitować;
- scenariusze testowe w `.xlsx`, generowane z kodu, jeśli są powiązane z ramkami Postman.
