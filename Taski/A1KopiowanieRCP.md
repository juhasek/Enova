# Dodatkowy kod „A1KopiowanieRCP”

Wspólna metoda `A1.Runtime.Business.TblCodeFiles.A1KopiowanieRCP.KopiujZapis(kalend, oryginal)`
kopiująca jeden zapis Oryginalnych danych RCP do Danych RCP. Używają jej oba zadania:

- „Kopiuj oryginalne dane RCP” (`KopiujOryginalneDaneRCP`, wyzwalacz przy zapisie),
- „Kopiuj oryginalne dane RCP harmonogram” (`KopiujOryginalneDaneRCPHarmonogram`, harmonogram).

Reguły kopiowania i zabezpieczenia przed duplikatami: `KopiujOryginalneDaneRCP.md`.

## Konfiguracja w enova

Ustawienia/Systemowe/Dodatkowy kod do kompilacji → nowy kod:

| Pole | Wartość |
|---|---|
| Nazwa | A1KopiowanieRCP |
| Projekt | Business (`Soneta.Runtime.Database.Business` — ten sam co definicje zadań; inny projekt nie byłby widoczny z kodu zadań) |
| Kod | zawartość pliku `A1KopiowanieRCP` |

W bazie Al: CodeFiles 1016 (2026-10-20). Po zmianie kodu: „Wymuś rekompilację” na definicji
zadania (serwer), usługa harmonogramu przekompilowuje się sama.
