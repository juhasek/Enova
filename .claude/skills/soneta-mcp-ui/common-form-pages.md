# Zakładki formularzy najważniejszych obiektów Soneta

Lista `pageID` zakładek na formularzach kluczowych obiektów. Używaj ich z `switch_form_page(pageID)` po wcześniejszym otwarciu formularza przez `open_form` / `open_subform` / `add_object`.

Identyfikatory zakładek (`pageID`) są stałe i można je używać bez odczytu z poprzedniej odpowiedzi.

**UWAGA:** Pełna lista zakładek dostępna na danym formularzu zależy od wariantu i kontekstu (np. Pracownik vs Zleceniobiorca, dostępne moduły). Zawsze weryfikuj listę przez odpowiedź `open_form`.

## Spis treści

- [Pracownik](#pracownik-tablename-pracownicy) — `Pracownicy`
- [Kontrahent](#kontrahent-tablename-kontrahenci) — `Kontrahenci`
- [Bank](#bank-tablename-banki) — `Banki`
- [Urząd skarbowy](#urząd-skarbowy-tablename-urzedyskarbowe) — `UrzedySkarbowe`
- [Towar / Usługa](#towar--usługa-tablename-towary) — `Towary`
- [Definicja elementu wynagrodzenia](#definicja-elementu-wynagrodzenia-tablename-defelementow) — `DefElementow`
- [Operator](#operator-tablename-operators) — `Operators`
- [Zakładki współdzielone](#zakładki-współdzielone)
- [Uwagi praktyczne](#uwagi-praktyczne)

> Czytaj tylko sekcję dotyczącą interesującego Cię obiektu — plik jest długi.

---

## Pracownik (`tableName: Pracownicy`)

Folder: `Kadry i płace/Kadry/Pracownicy`

### Kadry
| pageID | Tytuł |
|---|---|
| `PracownikOgolnePage` | Kadry/Ogólne (domyślna) |
| `PracownikAdresyPage` | Kadry/Adresy |
| `PracownikDaneKontaktowePage` | Kadry/Dane kontaktowe |
| `PracownikPitPage` | Kadry/Dane podatkowe |
| `PracownikPodatkiBOPage` | Kadry/Dane podatkowe - bilans otwarcia |
| `PracownikUbezpieczeniePage` | Kadry/Dane ubezpieczeniowe |
| `PracownikUbezpieczeniePozostalePage` | Kadry/Dane ubezpieczeniowe pozostałe |
| `PracownikWebPage` | Kadry/Dostęp WWW |
| `PracownikHistZatrPage` | Kadry/Historia zatrudnienia |
| `PracownikPFRONPage` | Kadry/Informacje PFRON |
| `PracownikZUSPage` | Kadry/Informacje ZUS |
| `PracownikInnePage` | Kadry/Inne dane |
| `PracownikLokalizacjePracyZdalnejPage` | Kadry/Lokalizacje pracy zdalnej |
| `PracownikSkladkiFPPage` | Kadry/Podst. składek FP i FGŚP |
| `PracownikSkladkiZUSPage` | Kadry/Podst. składek ZUS |
| `PracownikPrawaPage` | Kadry/Prawa dostępu |
| `PracownikRachunkiPage` | Kadry/Rachunki bankowe |
| `PracownikRodzinaPage` | Kadry/Rodzina |
| `PracownikWojskoPage` | Kadry/Służba wojskowa |
| `PracownikUwagiPage` | Kadry/Uwagi |
| `PracownikWyksztalceniePage` | Kadry/Wykształcenie |

### Kadry — pozostałe
| pageID | Tytuł |
|---|---|
| `PracownikBadaniaLekarskiePage` | Badania lekarskie |
| `PracownikGUSPage` | Dane statystyczne |
| `DefinicjeCzynnikowSzkodliwychPage` | Czynniki szkodliwe i uciążliwe |
| `PracownikKartyRCPPage` | Karty RCP |
| `PracownikNagrodyKaryPage` | Nagrody i kary |
| `PracownikPodzielnikiPage` | Podzielniki kosztów |
| `PracownikSzkoleniaBHPPage` | Szkolenia BHP |
| `PracownikSwiadczeniaPage` | Świadczenia socjalne |
| `PracownikUkonczoneSzkoleniaPage` | Ukończone szkolenia |
| `PracownikUprawnieniaPage` | Uprawnienia |
| `PracownikWypadkiPage` | Wypadki |
| `PracownikZnajomoscJezykowObcychPage` | Znajomość języków obcych |

### Etat
| pageID | Tytuł |
|---|---|
| `EtatOgolnePage` | Etat/Ogólne |
| `EtatUmowaOPracePage` | Etat/Umowa o pracę |
| `EtatDodatkiPage` | Etat/Dodatki |
| `EtatInnePage` | Etat/Inne dane |
| `EtatUbezpieczeniaPage` | Etat/Ubezpieczenia |
| `ElementyWynagrodzeniaEtatPage` | Etat/Rozliczenia czasu pracy |
| `EtatRozwUmowyPage` | Etat/Rozwiązanie umowy |
| `EtatWyrejestrowaniePage` | Etat/Wyrejestrowanie |
| `EtatRGAPage` | Etat/Dane historyczne |
| `PracownikAkordyPage` | Etat/Akordy |
| `PracownikWymaganiaPage` | Etat/Wymagania |

### Kalendarz
| pageID | Tytuł |
|---|---|
| `PracownikCzasPracy2Page` | Czas pracy |
| `PracownikNieobecBOPage` | Bilans otwarcia |
| `PracownikNieobecUrlopBOPage` | Bilans otwarcia (urlopy) |
| `PracownikLimityPage` | Limity nieobecności |
| `PracownikNieobecnosciPage` | Nieobecności |
| `PracownikPlanPracy2Page` | Norma czasu pracy |
| `PracownikNieobecRozliczPage` | Podstawy |
| `PracownikPodstawyWypoczynkowegoPage` | Podstawy naliczania urlopu wypoczynkowego |
| `PracownikPodstawyZwolnienZUSPage` | Podstawy naliczania zwolnień ZUS |
| `PracHistoriaPracaZdalnaPage` | Praca zdalna |
| `PracownikRozliczeniaCzasuPracyPage` | Rozliczenia czasu pracy |
| `PracownikStatystykiPage` | Statystyka |
| `PracownikZbiegPracyIRodzicielstwaPage` | Zbiegi pracy i rodzicielstwa |
| `PracownikZestawieniaPage` | Zestawienia czasu pracy |

### Płace, Umowy, PPK
| pageID | Tytuł |
|---|---|
| `PracownikWyplatyPage` | Płace/Wypłaty (wszystkie) |
| `PracownikKosztyAutorskiePage` | Płace/Koszty autorskie |
| `OdpisNaOPPPage` | Płace/Odpis na OPP |
| `PracownikZajeciaKomorniczePage` | Płace/Zajęcia wynagrodzenia |
| `PracownikZaliczkiPage` | Płace/Zaliczki |
| `PracownikKosztyAutorskiePage1` | Płace/Zasiłki (inny płatnik) |
| `PracPlaceStornaPage` | Płace/Elementy stornowane |
| `PracownikUmowyPage` | Umowy/Umowy |
| `PracownikFunduszZPPage` | KZP, ZFM, .../KZP, ZFM, ... |
| `PracownikPozyczkiPage` | KZP, ZFM, .../Pożyczki |
| `SkladkaNaPPKPage` | PPK/Ogólne |
| `PracownikDeklaracjePPKPage` | PPK/Dokumenty PPK |

### Deklaracje
| pageID | Tytuł |
|---|---|
| `PracownikDeklaracjePITPage` | Podatkowe (PIT) |
| `PracownikDeklaracjePFRONPage` | PFRON |
| `PracownikDeklaracjeZUSRozlPage` | Rozliczeniowe ZUS |
| `PracownikDeklaracjeZUSZgPage` | Zgłoszeniowe ZUS |
| `PracownikDeklaracjeRMUAPage` | Informacje IMIR |
| `PracownikUlgaInnowacyjnaPage` | Ulga innowacyjna (PIT) |
| `PracownikDeklaracjePITBOPage` | Bilans otwarcia deklaracji PIT |
| `PracownikDeklaracjePIT40Page` | Historyczne |
| `PracownikNieobecnosciERP7Page` | ERP-7 BO (nieobecności) |
| `PracownikWynagrodzeniaERP7Page` | ERP-7 BO (wynagrodzenia) |
| `PracownikDeklaracjeRIABOWarSzczegPage` | RIA BO - warunki szczególne |
| `PracownikDeklaracjeRIABOOswiataPage` | RIA BO - praca nauczycielska |
| `PracownikDeklaracjeRIABOWynagrPage` | RIA BO - wynagrodzenia |

### Pozostałe (ZZL, Oświadczenia, GIODO, CRM)
| pageID | Tytuł |
|---|---|
| `KartyOpisuStanowiskaPage` | ZZL/Karty opisu stanowiska |
| `KartyKompetencjiPracownikaPage` | ZZL/Karty kompetencji |
| `CeleOkresowePracownikaPage` | ZZL/Cele okresowe pracownika |
| `KartyRealizacjiCeluPage` | ZZL/Karty realizacji celu |
| `PracownikRekrutacjaPage` | ZZL/Rekrutacja |
| `PracownikOswiadczeniaPage` | Oświadczenia i wnioski/Oświadczenia |
| `PracownikZUSOpiekaPage` | Oświadczenia do zasiłku opiekuńczego |
| `PracownikWnioskiOSzkoleniaPage` | Wnioski o szkolenia |
| `PracownikWnioskiUrlopowePage` | Wnioski o urlopy, delegacje |
| `KadryDokumentyDodatkowePage` | Inne kadrowe |
| `PracownikArkuszeOcenOceniajacyPage` | Oceny/Oceniający |
| `PracownikArkuszeOcenOcenianyPage` | Oceny/Oceniany |
| `PracownikGIODOOswiadczeniaPage` | GIODO/Oświadczenia |
| `PracownikGIODOUprawnieniaPage` | GIODO/Uprawnienia |
| `PracownikGIODOWymianaDanychPage` | GIODO/Wymiana danych |
| `PracownikRozrachunkiPage` | Rozrachunki/Dokumenty rozliczeniowe |
| `PracownikPrzelewyPage` | Rozrachunki/Przelewy |
| `PracownikRozrachunkiPage1` | Rozrachunki/Rozrachunki |
| `PracownikWarunkiPlatnosciPage` | Rozrachunki/Warunki płatności |
| `PracownikRozrachunkiPage2` | Dokumenty ewidencji/Dokumenty |
| `PracHistoriaMatrycePage` | Dokumenty ewidencji/Powiązane matryce |
| `SimpleIHistoryHostTuples1` | Dokumenty dodatkowe |
| `PracownikHistorycznePage` | Dane historyczne |
| `PracownikNiepelnosprawnosciPage` | Schorzenia |
| `PracownikZaniechPodatPage` | Zaniechania podatkowe |
| `PracownikArchiwumPage` | Pracownik w archiwum |
| `RowHistoryPage` | Historia zapisów |
| `PracHistoriaWiadomosciPowiazanePage` | CRM/Wiadomości email |
| `PracownikZadaniaPage` | Dokumenty i wnioski/Zadania |

---

## Kontrahent (`tableName: Kontrahenci`)

Folder: `Kontrahenci i urzędy/Kontrahenci`

| pageID | Tytuł |
|---|---|
| `KontrahentPage` | Ogólne (domyślna) |
| `PodmiotyPowiazaniaPage` | Powiązania |
| `KontrahentDodatkowePage` | Warunki płatności |
| `KontrahentAdresyPage` | Adres do korespondencji |
| `KontrahentKontaktPage` | Osoby kontaktowe |
| `IKontrahentDaneKontaktowePage` | Dane kontaktowe |
| `KontrahentUwagiPage` | Uwagi |
| `KontrahentGusPage` | GUS |
| `KontrahentKategoriePage` | Kategorie i branże |
| `KontrahentKodyKreskowePage` | Kody kreskowe |
| `IKontrahentLokalizacjePage` | CRM/Lokalizacje |
| `IKontrahentProjekty` | CRM/Projekty |
| `IKontrahentZadania` | CRM/Zadania |
| `IKontrahentUrzadzenia` | CRM/Urządzenia |
| `KontrahentOpiekunowiePage` | CRM/Opiekunowie |
| `LeadyPage` | CRM/Leady |
| `TransakcjePage` | CRM/Transakcje |
| `WiadomosciPowiazaneKontrahentaPage` | CRM/Wiadomości email |
| `BasicDocsPage` | DMS/Dokumenty podstawowe |
| `KontrahentCennikPage` | Handel/Cennik indywidualny |
| `KontrahentPrzecenyPage` | Handel/Przeceny indywidualne |
| `KontrahentRabatCenaPage` | Handel/Rabat i cena |
| `KontrahentRabatyPage` | Handel/Rabaty indywidualne |
| `KontrahentDokumentyPage` | Handel/Dokumenty |
| `KontrahentMarzaPage` | Handel/Marża |
| `KontrahentZasobyPage` | Handel/Zasoby |
| `KontrahentKSeFHandelPage` | KSeF/Handel |
| `RozrachunkiPodmiotuPage` | Rozrachunki/Rozrachunki |
| `PrzelewyPodmiotuPage` | Rozrachunki/Przelewy |
| `RozliczeniowePodmiotuPage` | Rozrachunki/Dokumenty rozliczeniowe |
| `DokEwidencjiPodmiotuPage` | Dokumenty ewidencji/Dokumenty |
| `PodmiotKasowyMatrycePage` | Dokumenty ewidencji/Powiązane matryce |
| `StatusyVatPage` | Historia statusu VAT |
| `AdresyWWWPage` | Adresy WWW |
| `KontrahentGIODOOswiadczeniaPage` | GIODO/Oświadczenia |
| `KontrahentGIODOWymianaDanychPage` | GIODO/Wymiana danych |

---

## Bank (`tableName: Banki`)

Folder: `Kontrahenci i urzędy/Banki`

| pageID | Tytuł |
|---|---|
| `BankOgolnePage` | Ogólne (domyślna) |
| `PodmiotyPowiazaniaPage` | Powiązania |
| `KontrahentDodatkowePage` | Warunki płatności |
| `KontrahentKontaktPage` | Osoby kontaktowe |
| `IKontrahentDaneKontaktowePage` | Dane kontaktowe |
| `KontrahentUwagiPage` | Uwagi |
| `IKontrahentLokalizacjePage` | CRM/Lokalizacje |
| `IKontrahentProjekty` | CRM/Projekty |
| `IKontrahentZadania` | CRM/Zadania |
| `IKontrahentUrzadzenia` | CRM/Urządzenia |
| `BasicDocsPage` | DMS/Dokumenty podstawowe |
| `RozrachunkiPodmiotuPage` | Rozrachunki/Rozrachunki |
| `PrzelewyPodmiotuPage` | Rozrachunki/Przelewy |
| `RozliczeniowePodmiotuPage` | Rozrachunki/Dokumenty rozliczeniowe |
| `DokEwidencjiPodmiotuPage` | Dokumenty ewidencji/Dokumenty |
| `PodmiotKasowyMatrycePage` | Dokumenty ewidencji/Powiązane matryce |
| `IEmailElementWiadomosciPowiazanePage` | CRM/Wiadomości email |
| `AdresyWWWPage` | Adresy WWW |

---

## Urząd skarbowy (`tableName: UrzedySkarbowe`)

Folder: `Kontrahenci i urzędy/Urzędy skarbowe`

| pageID | Tytuł |
|---|---|
| `UrzadSkarbowyPage` | Ogólne (domyślna) |
| `UrzadSkarbowyKontaPage` | Konta podatkowe |
| `PodmiotyPowiazaniaPage` | Powiązania |
| `KontrahentDodatkowePage` | Warunki płatności |
| `KontrahentKontaktPage` | Osoby kontaktowe |
| `IKontrahentDaneKontaktowePage` | Dane kontaktowe |
| `KontrahentUwagiPage` | Uwagi |
| `IKontrahentLokalizacjePage` | CRM/Lokalizacje |
| `IKontrahentProjekty` | CRM/Projekty |
| `IKontrahentZadania` | CRM/Zadania |
| `IKontrahentUrzadzenia` | CRM/Urządzenia |
| `BasicDocsPage` | DMS/Dokumenty podstawowe |
| `RozrachunkiPodmiotuPage` | Rozrachunki/Rozrachunki |
| `PrzelewyPodmiotuPage` | Rozrachunki/Przelewy |
| `RozliczeniowePodmiotuPage` | Rozrachunki/Dokumenty rozliczeniowe |
| `DokEwidencjiPodmiotuPage` | Dokumenty ewidencji/Dokumenty |
| `PodmiotKasowyMatrycePage` | Dokumenty ewidencji/Powiązane matryce |
| `IEmailElementWiadomosciPowiazanePage` | CRM/Wiadomości email |
| `AdresyWWWPage` | Adresy WWW |

---

## Towar / Usługa (`tableName: Towary`)

Folder: `Handel/Kartoteki/Towary i usługi`

| pageID | Tytuł |
|---|---|
| `TowarPage` | Ogólne (domyślna) |
| `TowarDodatkowePage` | Dodatkowe |
| `TowarOpisyPage` | PIM/Opisy |
| `TowarJednostkiPage` | Jednostki i opakowania |
| `TowarKodyPage` | Kody |
| `TowarCennikKontrahentowPage` | Cennik/Kontrahentów |
| `TowarRabatyKontrahentowPage` | Cennik/Rabaty kontrahentów |
| `TowarZamowieniaPage` | Zamówienia |
| `TowarZamiennikiPage` | Zamienniki |
| `TowarProdukcjaZaawansowanaPage` | Produkcja zaawansowana |
| `TowarZasobyPage` | Magazyn/Zasoby |
| `TowarMarzaPage` | Magazyn/Marża |
| `TowarObrotyPage` | Magazyn/Obroty |
| `TowarGrupyDostawPage` | Magazyn/Partie |
| `TowarPozycjeMagazynowePage` | Pozycje/Magazynowe |
| `TowarPozycjeHandlowePage` | Pozycje/Handlowe |
| `TowarPozycjeZamowienPage` | Pozycje/Zamówień |
| `TowarPozycjeOfertPage` | Pozycje/Ofert |
| `TowarPozycjePozostalePage` | Pozycje/Pozostałe |
| `DefinePage` | Dodatkowe (cechy) |
| `AdresyWWWPage` | Adresy WWW |

---

## Definicja elementu wynagrodzenia (`tableName: DefElementow`)

Folder: `Ustawienia/Kadry i płace/Płace/Elementy wynagrodzenia` (otwierany jako subform z gridem).

| pageID | Tytuł |
|---|---|
| `DefinicjaElementuProPage` | Ogólne (domyślna) |
| `DefinicjaElementuDeklaracjePage` | Deklaracje |
| `DefinicjaElementuDeklaracjeCdPage` | Deklaracje (cd) |
| `DefinicjaElementuNieobecnosciProPage` | Nieobecności |
| `DefinicjaElementuRozliczeniePage` | Rozliczenie |
| `DefinicjaElementuGUSPage` | Dane statystyczne |
| `DefinicjaElementuDodatkowePage` | Dodatkowe |
| `DefinicjaElementuAlgorytmPage` | Algorytm/Ogólne |
| `DefinicjaElementuEdytorPage` | Algorytm/Edytor |
| `DefinicjaElementuParametryPage` | Algorytm/Nazwy parametrów |
| `DefinicjaElementuPodgladPage` | Algorytm/Podgląd |
| `FieldsPagePage` | Zakładki definiowane |

---

## Operator (`tableName: Operators`)

Folder: `Ustawienia/Systemowe/Uprawnienia/Operatorzy` (otwierany jako subform z gridem `_New_Operators_Operatorzy`).

| pageID | Tytuł |
|---|---|
| `OperatorGeneralPage` | Ogólne (domyślna) |
| `OperatorSystemowePage` | Systemowe |
| `OperatorConfigPage` | Wsparcie |
| `OperUstawieniaCRM` | CRM |
| `OperUstawieniaESPPage1` | Ewidencje ŚP |
| `OperKontrahentFiltrPage` | Filtrowanie |
| `HandelUstawieniaOperatoraPage` | Handel |
| `OperUstawieniaKadrowePage` | Kadry i płace |
| `OperatorProdukcjaZaawansowanaPage` | Produkcja zaawansowana |
| `OperatorUserGroupsPage` | Grupy użytkowników |

---

## Zakładki współdzielone

Niektóre `pageID` powtarzają się na różnych formularzach podmiotów (Kontrahent, Bank, US, itp.) — to wspólny zestaw zakładek "podmiotowych":

| pageID | Tytuł | Występuje na |
|---|---|---|
| `PodmiotyPowiazaniaPage` | Powiązania | Kontrahent, Bank, US |
| `KontrahentDodatkowePage` | Warunki płatności | Kontrahent, Bank, US |
| `KontrahentKontaktPage` | Osoby kontaktowe | Kontrahent, Bank, US |
| `IKontrahentDaneKontaktowePage` | Dane kontaktowe | Kontrahent, Bank, US |
| `KontrahentUwagiPage` | Uwagi | Kontrahent, Bank, US |
| `IKontrahentLokalizacjePage` | CRM/Lokalizacje | Kontrahent, Bank, US |
| `IKontrahentProjekty` | CRM/Projekty | Kontrahent, Bank, US |
| `IKontrahentZadania` | CRM/Zadania | Kontrahent, Bank, US |
| `IKontrahentUrzadzenia` | CRM/Urządzenia | Kontrahent, Bank, US |
| `BasicDocsPage` | DMS/Dokumenty podstawowe | Kontrahent, Bank, US |
| `RozrachunkiPodmiotuPage` | Rozrachunki/Rozrachunki | Kontrahent, Bank, US |
| `PrzelewyPodmiotuPage` | Rozrachunki/Przelewy | Kontrahent, Bank, US |
| `RozliczeniowePodmiotuPage` | Rozrachunki/Dokumenty rozliczeniowe | Kontrahent, Bank, US |
| `DokEwidencjiPodmiotuPage` | Dokumenty ewidencji/Dokumenty | Kontrahent, Bank, US |
| `PodmiotKasowyMatrycePage` | Dokumenty ewidencji/Powiązane matryce | Kontrahent, Bank, US |
| `AdresyWWWPage` | Adresy WWW | Kontrahent, Bank, US, Towar |
| `DefinePage` | Cechy definiowane | większość obiektów |
| `RowHistoryPage` | Historia zapisów | większość obiektów |

---

## Uwagi praktyczne

- Po `open_form` odpowiedź zawiera tablicę `pages` z listą dostępnych zakładek dla *tego konkretnego* obiektu — różne instancje mogą mieć różne zestawy zakładek (np. zatrudniony pracownik vs zwolniony).
- `currentPageID` w odpowiedzi `open_form` wskazuje, która zakładka jest aktualnie aktywna (zwykle "Ogólne").
- `switch_form_page` zmienia zakładkę — `fieldPath` z poprzedniej zakładki przestają być ważne, używaj `fieldPath` z odpowiedzi `switch_form_page`.
- Przy formularzach z wieloma zakładkami możesz zawęzić listę: `get_form_pages(regexFilter="...")` zwraca tylko zakładki pasujące do wyrażenia (dopasowanie po tytule lub `pageID`, nieczułe na wielkość liter).
- Niektóre formularze konfiguracyjne (Definicja elementu, Operator) otwierane są jako **subform** z gridu — używaj `open_subform(gridPath, id)` zamiast `open_form`.
- Pliki w skill `soneta-place-def-elementow` zawierają szczegółowe receptury algorytmów dla elementów wynagrodzenia.
