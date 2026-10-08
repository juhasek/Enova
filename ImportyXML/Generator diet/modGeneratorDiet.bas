Attribute VB_Name = "modGeneratorDiet"
Option Explicit

' Generator importu zestawien diet pracownika do enova365 z danych w arkuszach
' "Diety zagraniczne" i "Pakiet mobilnosci" (patrz arkusz "Instrukcja").
'
' Generuje pliki XML w formacie importu wg rekordow enova (<session>), wczytywane przez
' dbmgr importxml <baza> <plik> (lub import zapisow z pliku XML w programie enova).
'
' TO MAKRO NIE LACZY SIE Z BAZA SQL W ZADEN SPOSOB. Pracownik jest wskazywany WYLACZNIE
' po kodzie (<Pracownik where="Kod=...">), kraj oddelegowania po kodzie z arkusza "Kraje"
' (standardowa lista krajow delegacji enova - identyfikatory sa takie same w kazdej bazie).
'
' Dwa pliki, bo oba zestawienia importuja sie inaczej (sprawdzone probnym importem):
'
' 1) Diety zagraniczne (tabela ZestawDietZagr, zakladka pracownika "Zestawienie diet"):
'    plik ZASTEPUJE zestawienia pracownikow z pliku w okresie od 1. dnia pierwszego do
'    ostatniego dnia ostatniego miesiaca z pliku (atrybut fromto sesji). Zestawienia spoza
'    tego okresu oraz pracownicy spoza pliku zostaja nietkniete. Ponowny import tego samego
'    pliku nie dubluje wpisow.
'
' 2) Pakiet mobilnosci (tabela ZestDietPaMob): plik DOPISUJE pozycje (addnew). enova ma
'    unikalny klucz pracownik + kraj + miesiac - jesli taka pozycja juz jest w bazie, CALY
'    plik zostaje odrzucony i nic sie nie zapisuje. Poprawke istniejacej pozycji robi sie
'    w enova recznie albo usuwa ja i importuje ponownie.
'
' Import wg rekordow NIE uruchamia walidacji enova (okres w jednym miesiacu, okres
' zatrudnienia "Pracownik za granica", blokada okresu) - dlatego makro samo sprawdza,
' co da sie sprawdzic bez bazy, i przy JAKIMKOLWIEK bledzie nie tworzy zadnego pliku.

Private Const ARK_ZAGR As String = "Diety zagraniczne"
Private Const ARK_PM As String = "Pakiet mobilnosci"
Private Const ARK_KRAJE As String = "Kraje"
Private Const ARK_CFG As String = "Konfiguracja"

Sub GenerujDiety()
    ' Brak ktoregos arkusza (np. zmieniona nazwa) - czytelny komunikat zamiast bledu 9 VBA.
    Dim nazwa As Variant, brak As String
    For Each nazwa In Array(ARK_CFG, ARK_ZAGR, ARK_PM, ARK_KRAJE)
        If Not ArkuszIstnieje(CStr(nazwa)) Then brak = brak & "  - " & nazwa & vbCrLf
    Next nazwa
    If brak <> "" Then
        MsgBox "W skoroszycie brakuje arkuszy o nazwach:" & vbCrLf & brak & vbCrLf & _
               "Przywroc te nazwy (bez zmian, bez polskich znakow) i uruchom makro ponownie.", _
               vbCritical, "Generator diet"
        Exit Sub
    End If

    Dim wsCfg As Worksheet
    Set wsCfg = ThisWorkbook.Worksheets(ARK_CFG)

    Dim folderXml As String, prefiksPliku As String
    folderXml = Trim(CStr(wsCfg.Range("B2").Value))
    prefiksPliku = Trim(CStr(wsCfg.Range("B3").Value))
    If prefiksPliku = "" Then prefiksPliku = "Diety"
    If folderXml = "" Then
        ' Puste pole = zapisz obok tego skoroszytu.
        folderXml = ThisWorkbook.Path & "\"
    ElseIf Right(folderXml, 1) <> "\" Then
        folderXml = folderXml & "\"
    End If

    Dim kraje As Object, waluty As Object
    Set kraje = CreateObject("Scripting.Dictionary")
    Set waluty = CreateObject("Scripting.Dictionary")
    WczytajKraje kraje, waluty

    Dim bledy As String
    Dim xmlZagr As String, xmlPm As String
    Dim nZagr As Long, nPm As Long
    Dim zagrOd As Date, zagrDo As Date

    xmlZagr = BudujZagr(bledy, nZagr, zagrOd, zagrDo)
    xmlPm = BudujPakiet(kraje, waluty, bledy, nPm)

    If bledy <> "" Then
        MsgBox "Nie wygenerowano zadnego pliku - popraw bledy w arkuszach:" & vbCrLf & vbCrLf & bledy, _
               vbCritical, "Generator diet"
        Exit Sub
    End If
    If nZagr = 0 And nPm = 0 Then
        MsgBox "Arkusze """ & ARK_ZAGR & """ i """ & ARK_PM & """ nie maja zadnych wierszy danych " & _
               "(wiersz 1 to naglowki).", vbExclamation, "Generator diet"
        Exit Sub
    End If

    On Error Resume Next
    ZapewnijFolder folderXml
    If Err.Number <> 0 Then
        MsgBox "Nie udalo sie utworzyc/znalezc folderu:" & vbCrLf & folderXml & vbCrLf & vbCrLf & _
               "Blad: " & Err.Description & vbCrLf & vbCrLf & _
               "Popraw Konfiguracja!B2 albo zostaw to pole puste - wtedy pliki zapisza sie obok tego skoroszytu.", _
               vbCritical, "Blad folderu"
        On Error GoTo 0
        Exit Sub
    End If
    On Error GoTo 0

    Dim znacznik As String, podsumowanie As String, plik As String
    znacznik = Format(Now, "yyyy-mm-dd_hhnnss")

    If nZagr > 0 Then
        plik = folderXml & prefiksPliku & " zagraniczne " & znacznik & ".xml"
        If Not ZapiszPlik(plik, _
            "<session fromto=""" & DataXml(zagrOd) & "..." & DataXml(zagrDo) & """>" & vbCrLf & xmlZagr & "</session>" & vbCrLf) Then Exit Sub
        podsumowanie = podsumowanie & "Diety zagraniczne (" & nZagr & " poz.):" & vbCrLf & plik & vbCrLf & _
            "  ZASTEPUJE zestawienia pracownikow z pliku w okresie " & Format(zagrOd, "dd.mm.yyyy") & _
            " - " & Format(zagrDo, "dd.mm.yyyy") & "." & vbCrLf & vbCrLf
    End If

    If nPm > 0 Then
        plik = folderXml & prefiksPliku & " pakiet mobilnosci " & znacznik & ".xml"
        If Not ZapiszPlik(plik, _
            "<session fromto=""(wszystko)"">" & vbCrLf & xmlPm & "</session>" & vbCrLf) Then Exit Sub
        podsumowanie = podsumowanie & "Pakiet mobilnosci (" & nPm & " poz.):" & vbCrLf & plik & vbCrLf & _
            "  DOPISUJE pozycje - jesli ktoras (pracownik + kraj + miesiac) juz jest w enova, " & _
            "caly plik zostanie odrzucony." & vbCrLf & vbCrLf
    End If

    MsgBox podsumowanie & "Import: dbmgr importxml <baza> ""<plik>""", vbInformation, "Generator diet"
End Sub

' ---------------------------------------------------------------------------------------
' Arkusz "Diety zagraniczne": A Kod pracownika | B Okres od | C Okres do | D Liczba diet
' ---------------------------------------------------------------------------------------
Private Function BudujZagr(ByRef bledy As String, ByRef liczba As Long, ByRef minOd As Date, ByRef maxDo As Date) As String
    Dim ws As Worksheet
    Set ws = ThisWorkbook.Worksheets(ARK_ZAGR)

    Dim prac As Object, okresy As Object
    Set prac = CreateObject("Scripting.Dictionary")   ' kod -> fragment XML wierszy
    Set okresy = CreateObject("Scripting.Dictionary") ' kod -> lista "od|do" do kontroli nakladania

    Dim lastRow As Long, r As Long
    lastRow = ws.Cells(ws.Rows.Count, "A").End(xlUp).Row

    For r = 2 To lastRow
        Dim kod As String
        kod = Trim(CStr(ws.Cells(r, "A").Value))
        If kod = "" Then GoTo Nastepny

        Dim pref As String
        pref = ARK_ZAGR & ", wiersz " & r & " (" & kod & "): "

        If Not IsDate(ws.Cells(r, "B").Value) Or Not IsDate(ws.Cells(r, "C").Value) Then
            bledy = bledy & pref & "nieprawidlowa data Okres od / Okres do." & vbCrLf
            GoTo Nastepny
        End If
        Dim dOd As Date, dDo As Date
        dOd = Int(CDate(ws.Cells(r, "B").Value))
        dDo = Int(CDate(ws.Cells(r, "C").Value))
        If dDo < dOd Then
            bledy = bledy & pref & "Okres do jest wczesniejszy niz Okres od." & vbCrLf
            GoTo Nastepny
        End If
        If Year(dOd) <> Year(dDo) Or Month(dOd) <> Month(dDo) Then
            bledy = bledy & pref & "okres musi miescic sie w jednym miesiacu kalendarzowym (wymog enova)." & vbCrLf
            GoTo Nastepny
        End If

        Dim vDiety As Variant
        vDiety = ws.Cells(r, "D").Value
        If IsEmpty(vDiety) Or Not IsNumeric(vDiety) Then
            bledy = bledy & pref & "brak liczby diet (kolumna D)." & vbCrLf
            GoTo Nastepny
        End If
        If CDbl(vDiety) < 0 Or CDbl(vDiety) <> Int(CDbl(vDiety)) Then
            bledy = bledy & pref & "liczba diet zagranicznych musi byc liczba calkowita >= 0." & vbCrLf
            GoTo Nastepny
        End If

        Dim klucz As String
        klucz = UCase(kod)
        If okresy.Exists(klucz) Then
            Dim p As Variant, czesci() As String
            For Each p In Split(okresy(klucz), ";")
                czesci = Split(p, "|")
                If dOd <= CDate(CLng(czesci(1))) And dDo >= CDate(CLng(czesci(0))) Then
                    bledy = bledy & pref & "okres naklada sie na inny wiersz tego pracownika." & vbCrLf
                    GoTo Nastepny
                End If
            Next p
            okresy(klucz) = okresy(klucz) & ";" & CLng(dOd) & "|" & CLng(dDo)
        Else
            okresy.Add klucz, CLng(dOd) & "|" & CLng(dDo)
            prac.Add klucz, ""
        End If

        prac(klucz) = prac(klucz) & _
            "      <ZestawienieDietZagr><Okres>" & DataXml(dOd) & "..." & DataXml(dDo) & "</Okres>" & _
            "<Diety>" & CLng(vDiety) & "</Diety></ZestawienieDietZagr>" & vbCrLf

        ' Zakres sesji (fromto) = pelne miesiace od pierwszego do ostatniego wiersza.
        Dim mOd As Date, mDo As Date
        mOd = DateSerial(Year(dOd), Month(dOd), 1)
        mDo = DateSerial(Year(dDo), Month(dDo) + 1, 0)
        If liczba = 0 Then
            minOd = mOd
            maxDo = mDo
        Else
            If mOd < minOd Then minOd = mOd
            If mDo > maxDo Then maxDo = mDo
        End If
        liczba = liczba + 1
Nastepny:
    Next r

    ' Jeden element <Pracownik> na kod - drugi element tego samego pracownika
    ' zastapilby zestawienia wczytane z pierwszego.
    Dim k As Variant, xml As String
    For Each k In prac.Keys
        xml = xml & "  <Pracownik where=""Kod=" & EscXml(KodOryginalny(ws, lastRow, CStr(k))) & """>" & vbCrLf & _
            "    <ZestawieniaDiet>" & vbCrLf & prac(k) & "    </ZestawieniaDiet>" & vbCrLf & _
            "  </Pracownik>" & vbCrLf
    Next k
    BudujZagr = xml
End Function

' ---------------------------------------------------------------------------------------
' Arkusz "Pakiet mobilnosci":
'   A Kod pracownika | B Miesiac | C Kod kraju | D Czas | E Liczba diet | F Korekta diet |
'   G Korekta reczna (T/N) | H Wartosc diet | I Waluta | J Wartosc diet PIT (PLN)
' ---------------------------------------------------------------------------------------
Private Function BudujPakiet(kraje As Object, waluty As Object, ByRef bledy As String, ByRef liczba As Long) As String
    Dim ws As Worksheet
    Set ws = ThisWorkbook.Worksheets(ARK_PM)

    Dim prac As Object, klucze As Object
    Set prac = CreateObject("Scripting.Dictionary")
    Set klucze = CreateObject("Scripting.Dictionary") ' kod|kraj|rrrr-mm -> numer wiersza

    Dim lastRow As Long, r As Long
    lastRow = ws.Cells(ws.Rows.Count, "A").End(xlUp).Row

    For r = 2 To lastRow
        Dim kod As String
        kod = Trim(CStr(ws.Cells(r, "A").Value))
        If kod = "" Then GoTo Nastepny

        Dim pref As String
        pref = ARK_PM & ", wiersz " & r & " (" & kod & "): "

        If Not IsDate(ws.Cells(r, "B").Value) Then
            bledy = bledy & pref & "nieprawidlowy Miesiac (wpisz dowolna date z tego miesiaca)." & vbCrLf
            GoTo Nastepny
        End If
        Dim dMies As Date
        dMies = CDate(ws.Cells(r, "B").Value)
        ' enova trzyma miesiac jako ostatni dzien miesiaca (ZestDietPakietMobil.Miesiac).
        dMies = DateSerial(Year(dMies), Month(dMies) + 1, 0)

        Dim kraj As String
        kraj = UCase(Trim(CStr(ws.Cells(r, "C").Value)))
        If Not kraje.Exists(kraj) Then
            bledy = bledy & pref & "nieznany kod kraju '" & kraj & "' (lista w arkuszu Kraje)." & vbCrLf
            GoTo Nastepny
        End If

        Dim czas As String
        czas = FormatCzas(ws.Cells(r, "D").Value)
        If czas = "" And Not IsEmpty(ws.Cells(r, "D").Value) Then
            bledy = bledy & pref & "nieprawidlowy Czas (format G:MM, np. 120:30)." & vbCrLf
            GoTo Nastepny
        End If
        If czas = "" Then czas = "0:00"

        Dim diety As String, korekta As String
        If Not LiczbaXml(ws.Cells(r, "E").Value, False, diety) Then
            bledy = bledy & pref & "brak/nieprawidlowa liczba diet (kolumna E)." & vbCrLf
            GoTo Nastepny
        End If
        If Not LiczbaXml(ws.Cells(r, "F").Value, True, korekta) Then
            bledy = bledy & pref & "nieprawidlowa korekta diet (kolumna F)." & vbCrLf
            GoTo Nastepny
        End If

        Dim reczna As Boolean
        reczna = TakNie(ws.Cells(r, "G").Value)

        ' Wartosci diet maja znaczenie TYLKO przy korekcie recznej - bez niej enova przy
        ' wyplacie sama liczy: stawka diety kraju x (liczba diet - korekta).
        Dim wart As String, walutaWart As String, wartPit As String
        wart = "0.00"
        wartPit = "0.00"
        walutaWart = waluty(kraj)
        If reczna Then
            If Not LiczbaXml(ws.Cells(r, "H").Value, False, wart) Then
                bledy = bledy & pref & "przy korekcie recznej podaj Wartosc diet (kolumna H)." & vbCrLf
                GoTo Nastepny
            End If
            If Trim(CStr(ws.Cells(r, "I").Value)) <> "" Then walutaWart = UCase(Trim(CStr(ws.Cells(r, "I").Value)))
            If Not LiczbaXml(ws.Cells(r, "J").Value, True, wartPit) Then
                bledy = bledy & pref & "nieprawidlowa Wartosc diet PIT (kolumna J)." & vbCrLf
                GoTo Nastepny
            End If
            wart = Kwota2(wart)
            wartPit = Kwota2(wartPit)
        End If
        If walutaWart = "" Then walutaWart = "PLN"

        Dim klucz As String, kluczPoz As String
        klucz = UCase(kod)
        kluczPoz = klucz & "|" & kraj & "|" & Format(dMies, "yyyy-mm")
        If klucze.Exists(kluczPoz) Then
            bledy = bledy & pref & "ten sam pracownik, kraj i miesiac co w wierszu " & klucze(kluczPoz) & _
                " (enova pozwala na jedna pozycje)." & vbCrLf
            GoTo Nastepny
        End If
        klucze.Add kluczPoz, r
        If Not prac.Exists(klucz) Then prac.Add klucz, ""

        prac(klucz) = prac(klucz) & _
            "      <ZestDietPakietMobil>" & vbCrLf & _
            "        <KrajOddelegowania>" & kraje(kraj) & "</KrajOddelegowania>" & vbCrLf & _
            "        <Czas>" & czas & "</Czas>" & vbCrLf & _
            "        <DzienMiesiaca>" & DataXml(dMies) & "</DzienMiesiaca>" & vbCrLf & _
            "        <Diety>" & diety & "</Diety>" & vbCrLf & _
            "        <DietyKorekta>" & korekta & "</DietyKorekta>" & vbCrLf & _
            "        <WartoscDiet>" & wart & " " & EscXml(walutaWart) & "</WartoscDiet>" & vbCrLf & _
            "        <KorektaReczna>" & IIf(reczna, "True", "False") & "</KorektaReczna>" & vbCrLf & _
            "        <WartoscDietPIT>" & wartPit & " PLN</WartoscDietPIT>" & vbCrLf & _
            "      </ZestDietPakietMobil>" & vbCrLf
        liczba = liczba + 1
Nastepny:
    Next r

    Dim k As Variant, xml As String
    For Each k In prac.Keys
        xml = xml & "  <Pracownik where=""Kod=" & EscXml(KodOryginalny(ws, lastRow, CStr(k))) & """>" & vbCrLf & _
            "    <ZestDietPakietMobil addnew=""true"">" & vbCrLf & prac(k) & "    </ZestDietPakietMobil>" & vbCrLf & _
            "  </Pracownik>" & vbCrLf
    Next k
    BudujPakiet = xml
End Function

' ---------------------------------------------------------------------------------------
' Pomocnicze
' ---------------------------------------------------------------------------------------

Private Function ArkuszIstnieje(nazwa As String) As Boolean
    Dim ws As Worksheet
    For Each ws In ThisWorkbook.Worksheets
        If ws.Name = nazwa Then
            ArkuszIstnieje = True
            Exit Function
        End If
    Next ws
End Function

' Arkusz "Kraje": A Kod | B Nazwa | C Waluta | D Identyfikator (GUID z enova).
Private Sub WczytajKraje(kraje As Object, waluty As Object)
    Dim ws As Worksheet
    Set ws = ThisWorkbook.Worksheets(ARK_KRAJE)
    Dim r As Long, kod As String
    For r = 2 To ws.Cells(ws.Rows.Count, "A").End(xlUp).Row
        kod = UCase(Trim(CStr(ws.Cells(r, "A").Value)))
        If kod <> "" And Not kraje.Exists(kod) Then
            kraje.Add kod, Trim(CStr(ws.Cells(r, "D").Value))
            waluty.Add kod, UCase(Trim(CStr(ws.Cells(r, "C").Value)))
        End If
    Next r
End Sub

' Kod pracownika w pisowni z arkusza (slownik trzyma klucz wielkimi literami).
Private Function KodOryginalny(ws As Worksheet, lastRow As Long, klucz As String) As String
    Dim r As Long
    For r = 2 To lastRow
        If UCase(Trim(CStr(ws.Cells(r, "A").Value))) = klucz Then
            KodOryginalny = Trim(CStr(ws.Cells(r, "A").Value))
            Exit Function
        End If
    Next r
    KodOryginalny = klucz
End Function

' Liczba z kropka dziesietna (format XML enova), niezaleznie od ustawien regionalnych.
Private Function LiczbaXml(v As Variant, pustaToZero As Boolean, ByRef wynik As String) As Boolean
    If IsEmpty(v) Or Trim(CStr(v)) = "" Then
        wynik = "0"
        LiczbaXml = pustaToZero
        Exit Function
    End If
    Dim d As Double
    If IsNumeric(v) Then
        d = CDbl(v)
    Else
        Dim s As String
        s = Replace(Trim(CStr(v)), " ", "")
        s = Replace(s, ",", Mid(CStr(1.5), 2, 1))
        If Not IsNumeric(s) Then
            LiczbaXml = False
            Exit Function
        End If
        d = CDbl(s)
    End If
    If d < 0 Then
        LiczbaXml = False
        Exit Function
    End If
    wynik = Trim(Str(d))
    If Left(wynik, 1) = "." Then wynik = "0" & wynik
    LiczbaXml = True
End Function

' "767.2" -> "767.25"-styl: zawsze dwa miejsca po kropce.
Private Function Kwota2(s As String) As String
    Dim p As Long
    p = InStr(s, ".")
    If p = 0 Then
        Kwota2 = s & ".00"
    ElseIf Len(s) - p = 1 Then
        Kwota2 = s & "0"
    ElseIf Len(s) - p > 2 Then
        Kwota2 = Trim(Str(Round(Val(s), 2)))
        If InStr(Kwota2, ".") = 0 Then Kwota2 = Kwota2 & ".00"
        If Len(Kwota2) - InStr(Kwota2, ".") = 1 Then Kwota2 = Kwota2 & "0"
    Else
        Kwota2 = s
    End If
End Function

Private Function TakNie(v As Variant) As Boolean
    Dim s As String
    s = UCase(Trim(CStr(v)))
    TakNie = (s = "T" Or s = "TAK" Or s = "1" Or s = "PRAWDA" Or s = "TRUE" Or s = "X")
End Function

Private Function DataXml(d As Date) As String
    DataXml = Format(d, "yyyy-mm-dd")
End Function

' Akceptuje: puste, tekst "G:MM" (takze powyzej 24h, np. 120:30), liczbe/godzine Excela
' (ulamek doby, format [g]:mm) - zwraca "G:MM" albo "".
Private Function FormatCzas(v As Variant) As String
    If IsEmpty(v) Then Exit Function
    If VarType(v) = vbString Then
        Dim s As String, cz() As String
        s = Trim(CStr(v))
        cz = Split(s, ":")
        If UBound(cz) <> 1 Then Exit Function
        If Not IsNumeric(cz(0)) Or Not IsNumeric(cz(1)) Then Exit Function
        If CLng(cz(1)) > 59 Or CLng(cz(0)) < 0 Or CLng(cz(1)) < 0 Then Exit Function
        FormatCzas = CLng(cz(0)) & ":" & Format(CLng(cz(1)), "00")
        Exit Function
    End If
    If IsNumeric(v) Then
        Dim totalMin As Long
        totalMin = CLng(Round(CDbl(v) * 24 * 60))
        If totalMin < 0 Then Exit Function
        FormatCzas = (totalMin \ 60) & ":" & Format(totalMin Mod 60, "00")
    End If
End Function

Private Function EscXml(s As String) As String
    Dim t As String
    t = Replace(s, "&", "&amp;")
    t = Replace(t, "<", "&lt;")
    t = Replace(t, ">", "&gt;")
    t = Replace(t, """", "&quot;")
    EscXml = t
End Function

' Zapis UTF-8 (z BOM) - zgodny z naglowkiem encoding="utf-8". To tylko zapis pliku
' lokalnego (ADODB.Stream), NIE polaczenie z baza danych.
Private Function ZapiszPlik(sciezka As String, tresc As String) As Boolean
    On Error GoTo Blad
    Dim strm As Object
    Set strm = CreateObject("ADODB.Stream")
    strm.Type = 2 ' adTypeText
    strm.Charset = "utf-8"
    strm.Open
    strm.WriteText "<?xml version=""1.0"" encoding=""utf-8"" ?>" & vbCrLf & tresc
    strm.SaveToFile sciezka, 2 ' adSaveCreateOverWrite
    strm.Close
    ZapiszPlik = True
    Exit Function
Blad:
    MsgBox "Nie udalo sie zapisac pliku:" & vbCrLf & sciezka & vbCrLf & vbCrLf & _
           "Blad: " & Err.Description, vbCritical, "Blad zapisu"
    ZapiszPlik = False
End Function

' Tworzy wszystkie brakujace poziomy folderu (MkDir tworzy tylko jeden poziom naraz).
Private Sub ZapewnijFolder(ByVal sciezka As String)
    Dim sciezkaBezSlasha As String
    sciezkaBezSlasha = sciezka
    If Right(sciezkaBezSlasha, 1) = "\" Then sciezkaBezSlasha = Left(sciezkaBezSlasha, Len(sciezkaBezSlasha) - 1)
    If sciezkaBezSlasha = "" Then Exit Sub
    If Dir(sciezkaBezSlasha, vbDirectory) <> "" Then Exit Sub

    Dim czesci() As String
    czesci = Split(sciezkaBezSlasha, "\")
    If UBound(czesci) < 1 Then Exit Sub

    Dim biezaca As String, i As Integer
    biezaca = czesci(0)
    For i = 1 To UBound(czesci)
        biezaca = biezaca & "\" & czesci(i)
        If Dir(biezaca, vbDirectory) = "" Then MkDir biezaca
    Next i
End Sub
