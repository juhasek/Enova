# Dokumenty aktualizacji kalendarza (DAK) — sesja 2026-10-14

Mail klienta o wersjonowaniu kalendarzy (dokumenty aktualizacji planu/czasu pracy): przygotowana
odpowiedź, a następnie wdrożenie części proponowanych rozwiązań na bazie testowej **Claude**.

## 1. Pytania klienta (skrót)

1. Po nowym planie od kierownika poprzedni plan i czas nie zmieniają stanu na „Anulowany” —
   są dwa „Zatwierdzone”.
2. Na liście dokumentów nie wiadomo, kogo dotyczą i kiedy zatwierdzone (ID / nazwisko pracownika,
   dział, przełożony, data i godzina zatwierdzenia).
3. Skąd raport „kto i kiedy zatwierdził” dla PIP.
4. Pulpit kierownika: plan/czas wybierany z rozwijanej strzałki — nieintuicyjne.
5. Jak administrator koryguje plan/czas przed wypłatą.

## 2. Ustalenia z dekompilacji `Soneta.KadryPlace.dll` (2610)

- Stany: Wypełniany → Do akceptacji → Zatwierdzony / Anulowany; **Zatwierdzony i Anulowany są
  końcowe** — zatwierdzonego dokumentu nie da się anulować (to założenie producenta).
- Zatwierdzenie kopiuje dni dokumentu do kalendarza pracownika i zapisuje poprzednią wersję dnia
  w historii kalendarza; obowiązuje ostatni zatwierdzony.
- Przy zatwierdzeniu enova zapisuje `DataZatwierdzenia = DateTime.Now` (data **i godzina**);
  sam dokument nie przechowuje, kto zatwierdził.
- Przy wersjonowaniu włączonym na kalendarzu dni planu/czasu są tylko do odczytu dla wszystkich
  (także administratora) — korekty wyłącznie dokumentem aktualizacji, który kadry mogą same
  zatwierdzić.
- Definicja dokumentu ma algorytm tylko z hakiem `OnAdded` i listami podpowiedzi — brak haka
  na zatwierdzenie.

## 3. Odpowiedź dla klienta

Plik Word w `Pobrane/Odpowiedz - plan i czas pracy, zatwierdzanie.docx` (poza gitem) —
prostym językiem: zasada „obowiązuje ostatni zatwierdzony, starsze to historia”, propozycje:
kolumna „Aktualny”, dodatkowe kolumny na liście, wydruk dla PIP, uprawnienia/pulpit (do
sprawdzenia), procedura korekty przed wypłatą.

## 4. Wdrożone w bazie Claude (zakres wybrany przez użytkownika: pkt 1 i 2)

- 7 cech tabeli `DokAktKalendarzy` + kod wspólny — [Cechy/Dokument aktualizacji kalendarza.md](../Cechy/Dokument%20aktualizacji%20kalendarza.md).
- Podwidok listy „Aktualne dokumenty” — [Widoki/Dokumenty aktualizacji kalendarzy - Aktualne](../Widoki/Dokumenty%20aktualizacji%20kalendarzy%20-%20Aktualne/Aktualne%20dokumenty.md).
- Cechy zapisane przez GUI (MCP) — kompilator enova je przyjął; widok widoczny w programie.
- Wydruk dla PIP i pulpit kierownika — odłożone.

## 5. Zmiany konfiguracji bazy Claude przy próbie testu

- Prawo Administratora do definicji „Plan - dowolni pracownicy” (wiersz `Rights` ID 10571;
  w rozszerzonym systemie praw wiersz = nadanie).
- Definicja „Plan - dowolni pracownicy”: powiązanie z elementami struktury „Pulpity - kierownik
  i pracownicy”.
- Kopia bazy przed dodaniem widoku: `Claude_przed_widokiem_DAK_20261014_1604.bak`.

## 6. Do zrobienia

1. **Test na danych w GUI** (MCP nie założył dokumentu: pole „Pracownik” to interfejs, a wybór
   elementu struktury daje błąd „Powiązania można dodawać tylko do wierszy typu
   Soneta.Kadry.Pracownik”): dwa zatwierdzone plany tego samego pracownika na ten sam okres →
   starszy `Aktualny = Nie`; sprawdzić pracownika, dział, przełożonego, datę i godzinę.
2. Decyzja, czy cofnąć zmiany z pkt 5.
3. Później: wydruk dla PIP (kto/kiedy złożył i zatwierdził — „kto” z historii procesu),
   uproszczenie wyboru plan/czas na pulpicie, przeniesienie na bazę klienta.
