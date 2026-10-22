# Rezerwy urlopowe

Rezerwy urlopowe i budżety rezerw liczone w enova365 na **planowanych listach płac** (moduł rezerw,
licencja Płace Platynowe). Każde wdrożenie w osobnym podkatalogu:

| Katalog | Zawartość |
|---|---|
| `Rezerwa Al/` | Rezerwa urlopowa miesięczna i budżet rezerwy (sierpień → stan na 01.01 roku następnego): algorytm elementu, Dodatkowy kod kompilacji (cechy widoku), import XML, instrukcja docx, scenariusze testowe xlsx. |

W podkatalogu wdrożenia:
- plik bez rozszerzenia = kod wklejany do edytora enova (algorytm elementu, Dodatkowy kod kompilacji),
- `.md` = dokumentacja techniczna i biznesowa,
- `.dbinit.xml` = import definicji do bazy (`dbmgr importxml`), generowany skryptem `generuj-xml.sh`,
- `Instrukcja_*.docx` = instrukcja dla klienta, `* - scenariusze testowe.xlsx` = scenariusze testowe.
