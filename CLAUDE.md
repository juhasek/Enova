# Konwencje pracy w repozytorium

## Branch roboczy

Domyślnym branchem, na który powinny trafiać zmiany w tym repozytorium, jest
`Testy` — nie `main`. Zmiany należy commitować i pushować bezpośrednio na
`Testy`, bez tworzenia dodatkowych branchy `claude/...` na potrzeby
pojedynczych zadań, chyba że użytkownik wyraźnie poprosi inaczej.

Branch `main` traktowany jest jako stabilny/produkcyjny. Przenoszenie zmian
z `Testy` do `main` odbywa się świadomie (np. przez Pull Request) i wymaga
wyraźnej zgody użytkownika za każdym razem.
