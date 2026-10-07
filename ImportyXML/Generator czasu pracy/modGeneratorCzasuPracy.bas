Attribute VB_Name = "modGeneratorCzasuPracy"
Option Explicit

' Generator importu rzeczywistego czasu pracy (dzien pracy + strefy) do enova365 z arkusza "Dane".
' Wymaga arkuszy "Konfiguracja", "Dane" i "Bledy" w tym samym skoroszycie (patrz arkusz "Instrukcja").
'
' Generuje plik XML w formacie importu enova <Root><CzasPracy><DzienPracy>...</DzienPracy></CzasPracy></Root>,
' importowany w programie enova przez:
'   Plik | Importuj zapisy | Import czasu pracy i wynagrodzen
' (wymaga zarejestrowanego w bazie rozszerzenia Soneta.CzasPracy.Migrator/Utils).
'
' TO MAKRO NIE LACZY SIE Z BAZA SQL. Pracownik jest identyfikowany wylacznie po kodzie (<Pracownik>),
' strefa po nazwie definicji strefy - importer enova sam je wyszukuje.
'
' Arkusz "Dane": jeden wiersz = jeden dzien jednego pracownika.
'   A = Kod pracownika, B = Data, potem 20 stref po 3 kolumny (C..BJ):
'   Strefa N nazwa | Strefa N godzina od | Strefa N czas
' Nazwa = nazwa definicji strefy w enova. Godzina od jest opcjonalna (zalezy od konfiguracji strefy),
' czas jest wymagany. Godziny jako tekst G:MM albo godzina Excela (np. wpisane 16:00).
' Strefa z pustymi wszystkimi trzema kolumnami jest pomijana.
'
' Import enova dla istniejacego dnia KASUJE wszystkie jego strefy i wpisuje te z pliku. Dlatego wiersz
' z jakimkolwiek bledem jest pomijany w calosci (czesciowy import skasowalby pozostale strefy dnia).
' Strefa "Praca w normie" NIE jest w pliku - dopisuje ja Task w enova na podstawie planu pracy.

Private Const KOL_PIERWSZEJ_STREFY As Long = 3   ' C
Private Const MAKS_STREF As Long = 20             ' C..BJ, po 3 kolumny na strefe
Private Const KOLUMN_NA_STREFE As Long = 3

Sub GenerujCzasPracy()
    Dim wsCfg As Worksheet, wsDane As Worksheet, wsBledy As Worksheet
    Set wsCfg = ThisWorkbook.Worksheets("Konfiguracja")
    Set wsDane = ThisWorkbook.Worksheets("Dane")
    Set wsBledy = ThisWorkbook.Worksheets("Bledy")

    Dim folderXml As String, prefiksPliku As String
    folderXml = Trim(CStr(wsCfg.Range("B2").Value))
    prefiksPliku = Trim(CStr(wsCfg.Range("B3").Value))
    If prefiksPliku = "" Then prefiksPliku = "Czas pracy"
    If folderXml = "" Then
        folderXml = ThisWorkbook.Path & "\"
    ElseIf Right(folderXml, 1) <> "\" Then
        folderXml = folderXml & "\"
    End If

    ' Czyszczenie arkusza bledow (naglowek zostaje)
    wsBledy.Range("A2:D" & wsBledy.Rows.Count).ClearContents

    Dim lastRow As Long
    lastRow = wsDane.Cells(wsDane.Rows.Count, "A").End(xlUp).Row
    If lastRow < 2 Then
        MsgBox "Arkusz ""Dane"" nie ma zadnych wierszy danych (wiersz 1 to naglowki).", vbExclamation
        Exit Sub
    End If

    Dim dniXml As String, liczbaDni As Long, liczbaStrefRazem As Long
    Dim liczbaBledow As Long, wierszeBledne As Long
    Dim widziane As Object
    Set widziane = CreateObject("Scripting.Dictionary")

    Dim r As Long
    For r = 2 To lastRow
        Dim kod As String
        kod = Trim(CStr(wsDane.Cells(r, 1).Value))
        If kod = "" Then GoTo NastepnyWiersz

        Dim bladWiersza As Boolean
        bladWiersza = False

        Dim dataTxt As String
        dataTxt = ""
        If IsDate(wsDane.Cells(r, 2).Value) Then
            dataTxt = Format(CDate(wsDane.Cells(r, 2).Value), "yyyy-mm-dd")
        Else
            DopiszBlad wsBledy, liczbaBledow, r, "B", wsDane.Cells(r, 2).Value, "Nieprawidlowa data."
            bladWiersza = True
        End If

        If dataTxt <> "" Then
            Dim klucz As String
            klucz = UCase(kod) & "|" & dataTxt
            If widziane.Exists(klucz) Then
                DopiszBlad wsBledy, liczbaBledow, r, "A:B", kod & " " & dataTxt, _
                    "Ten sam pracownik i dzien juz byl w wierszu " & widziane(klucz) & _
                    ". Import zapisalby tylko jeden z nich - polacz strefy w jednym wierszu."
                bladWiersza = True
            Else
                widziane.Add klucz, r
            End If
        End If

        Dim strefyXml As String, liczbaStref As Long
        strefyXml = ""
        liczbaStref = 0

        Dim k As Long
        For k = 0 To MAKS_STREF - 1
            Dim kol As Long
            kol = KOL_PIERWSZEJ_STREFY + k * KOLUMN_NA_STREFE
            Dim nazwa As String, vOd As Variant, vCzas As Variant
            nazwa = NormalizujNazwe(CStr(wsDane.Cells(r, kol).Value))
            vOd = wsDane.Cells(r, kol + 1).Value
            vCzas = wsDane.Cells(r, kol + 2).Value

            If nazwa <> "" Or Not PustaKomorka(vOd) Or Not PustaKomorka(vCzas) Then
                Dim minOd As Long, minCzas As Long, opisBledu As String, kolBledu As Long
                opisBledu = ""
                minOd = -1
                minCzas = MinutyZWartosci(vCzas)
                If nazwa = "" Then
                    opisBledu = "Strefa " & (k + 1) & ": brak nazwy strefy."
                    kolBledu = kol
                ElseIf PustaKomorka(vCzas) Then
                    opisBledu = "Strefa " & (k + 1) & ": brak czasu."
                    kolBledu = kol + 2
                ElseIf minCzas < 0 Then
                    opisBledu = "Strefa " & (k + 1) & ": czas nie jest w formacie G:MM."
                    kolBledu = kol + 2
                ElseIf minCzas = 0 Or minCzas > 24 * 60 Then
                    opisBledu = "Strefa " & (k + 1) & ": czas musi byc wiekszy od 0:00 i nie wiekszy niz 24:00."
                    kolBledu = kol + 2
                ElseIf Not PustaKomorka(vOd) Then
                    minOd = MinutyZWartosci(vOd)
                    If minOd < 0 Or minOd >= 48 * 60 Then
                        opisBledu = "Strefa " & (k + 1) & ": godzina od nie jest w formacie G:MM (0:00-47:59)."
                        kolBledu = kol + 1
                    End If
                End If

                If opisBledu = "" Then
                    strefyXml = strefyXml & "<StrefaPracy Definicja=""" & EscXml(nazwa) & """"
                    If minOd >= 0 Then strefyXml = strefyXml & " OdGodziny=""" & FormatMinuty(minOd) & """"
                    strefyXml = strefyXml & " Czas=""" & FormatMinuty(minCzas) & """ />" & vbCrLf
                    liczbaStref = liczbaStref + 1
                Else
                    DopiszBlad wsBledy, liczbaBledow, r, KolumnaLitera(kolBledu), _
                        CStr(wsDane.Cells(r, kolBledu).Text), opisBledu
                    bladWiersza = True
                End If
            End If
        Next k

        If liczbaStref = 0 And Not bladWiersza Then
            DopiszBlad wsBledy, liczbaBledow, r, "C:BJ", "", _
                "Brak stref. Wiersz pominiety - import skasowalby istniejace strefy tego dnia."
            bladWiersza = True
        End If

        If bladWiersza Then
            wierszeBledne = wierszeBledne + 1
            GoTo NastepnyWiersz
        End If

        ' Celowo bez <OdGodziny>/<Czas> na poziomie dnia - dzien opisuja wylacznie strefy
        ' (pole dnia liczy sie razem ze strefami przy kontroli nachodzenia stref).
        dniXml = dniXml & "<DzienPracy>" & vbCrLf & _
            "<Pracownik>" & EscXml(kod) & "</Pracownik>" & vbCrLf & _
            "<Data>" & dataTxt & "</Data>" & vbCrLf & _
            "<Strefy>" & vbCrLf & strefyXml & "</Strefy>" & vbCrLf & _
            "</DzienPracy>" & vbCrLf
        liczbaDni = liczbaDni + 1
        liczbaStrefRazem = liczbaStrefRazem + liczbaStref

NastepnyWiersz:
    Next r

    If liczbaDni = 0 Then
        wsBledy.Activate
        MsgBox "Nie wygenerowano zadnego dnia pracy. Bledne wiersze: " & wierszeBledne & _
               " - szczegoly w arkuszu ""Bledy"".", vbExclamation, "Generator czasu pracy"
        Exit Sub
    End If

    On Error Resume Next
    ZapewnijFolder folderXml
    If Err.Number <> 0 Then
        MsgBox "Nie udalo sie utworzyc/znalezc folderu:" & vbCrLf & folderXml & vbCrLf & vbCrLf & _
               "Blad: " & Err.Description & vbCrLf & vbCrLf & _
               "Popraw Konfiguracja!B2 albo zostaw to pole puste - wtedy plik zapisze sie obok tego skoroszytu.", _
               vbCritical, "Blad folderu"
        On Error GoTo 0
        Exit Sub
    End If
    On Error GoTo 0

    Dim nazwaPliku As String
    nazwaPliku = folderXml & prefiksPliku & " " & Format(Now, "yyyy-mm-dd_hhnnss") & ".xml"

    Dim xmlTxt As String
    xmlTxt = "<?xml version=""1.0"" encoding=""Unicode"" ?>" & vbCrLf & _
        "<Root xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">" & vbCrLf & _
        "<CzasPracy>" & vbCrLf & _
        dniXml & _
        "</CzasPracy>" & vbCrLf & _
        "</Root>" & vbCrLf

    On Error Resume Next
    ZapiszUnicode nazwaPliku, xmlTxt
    If Err.Number <> 0 Then
        MsgBox "Nie udalo sie zapisac pliku:" & vbCrLf & nazwaPliku & vbCrLf & vbCrLf & _
               "Blad: " & Err.Description, vbCritical, "Blad zapisu"
        On Error GoTo 0
        Exit Sub
    End If
    On Error GoTo 0

    Dim podsumowanie As String
    podsumowanie = "Wygenerowano plik XML: " & liczbaDni & " dni pracy, " & liczbaStrefRazem & " stref." & _
        vbCrLf & vbCrLf & nazwaPliku & vbCrLf & vbCrLf & _
        "Import w programie enova: Plik | Importuj zapisy | Import czasu pracy i wynagrodzen."
    If wierszeBledne > 0 Then
        podsumowanie = podsumowanie & vbCrLf & vbCrLf & "POMINIETE wiersze z bledami: " & wierszeBledne & _
            " - szczegoly w arkuszu ""Bledy""."
        wsBledy.Activate
    End If
    MsgBox podsumowanie, IIf(wierszeBledne > 0, vbExclamation, vbInformation), "Generator czasu pracy"
End Sub

' Nazwa strefy: twarde spacje/tabulatory -> spacja, zwiniecie wielokrotnych spacji, Trim.
Private Function NormalizujNazwe(ByVal s As String) As String
    Dim t As String
    t = Replace(s, Chr(160), " ")
    t = Replace(t, vbTab, " ")
    t = Replace(t, vbCr, " ")
    t = Replace(t, vbLf, " ")
    t = Trim(t)
    Do While InStr(t, "  ") > 0
        t = Replace(t, "  ", " ")
    Loop
    NormalizujNazwe = t
End Function

Private Function PustaKomorka(v As Variant) As Boolean
    If IsEmpty(v) Then
        PustaKomorka = True
    ElseIf VarType(v) = vbString Then
        PustaKomorka = (Trim(Replace(CStr(v), Chr(160), " ")) = "")
    Else
        PustaKomorka = False
    End If
End Function

' Wartosc komorki godziny -> minuty; -1 gdy to nie jest godzina.
' Akceptuje tekst "G:MM"/"GG:MM" oraz godzine/liczbe Excela (ulamek doby, np. 16:00 = 0,6667;
' wartosci powyzej doby, np. 30:00 = 1,25, tez sa poprawne).
Private Function MinutyZWartosci(v As Variant) As Long
    MinutyZWartosci = -1
    If IsEmpty(v) Or IsError(v) Then Exit Function
    If VarType(v) = vbString Then
        MinutyZWartosci = MinutyZTekstu(Trim(Replace(CStr(v), Chr(160), " ")))
    ElseIf VarType(v) = vbDate Or IsNumeric(v) Then
        If CDbl(v) < 0 Then Exit Function
        MinutyZWartosci = CLng(Round(CDbl(v) * 24 * 60))
    End If
End Function

' "G:MM" / "GG:MM" -> minuty; -1 gdy to nie jest godzina.
Private Function MinutyZTekstu(ByVal s As String) As Long
    MinutyZTekstu = -1
    Dim p As Long
    p = InStr(s, ":")
    If p < 2 Or p > 3 Then Exit Function
    If Len(s) = p + 5 And Mid(s, p + 3, 3) = ":00" Then s = Left(s, p + 2)   ' "16:00:00"
    If Len(s) <> p + 2 Then Exit Function
    Dim h As String, m As String
    h = Left(s, p - 1)
    m = Mid(s, p + 1, 2)
    If Not (h Like "#" Or h Like "##") Then Exit Function
    If Not (m Like "##") Then Exit Function
    If CLng(m) > 59 Then Exit Function
    MinutyZTekstu = CLng(h) * 60 + CLng(m)
End Function

Private Function FormatMinuty(totalMin As Long) As String
    FormatMinuty = CStr(totalMin \ 60) & ":" & Format(totalMin Mod 60, "00")
End Function

Private Sub DopiszBlad(ws As Worksheet, ByRef licznik As Long, wiersz As Long, kolumna As String, _
                       wartosc As Variant, opis As String)
    licznik = licznik + 1
    ws.Cells(licznik + 1, 1).Value = wiersz
    ws.Cells(licznik + 1, 2).Value = kolumna
    ws.Cells(licznik + 1, 3).NumberFormat = "@"
    ws.Cells(licznik + 1, 3).Value = CStr(wartosc)
    ws.Cells(licznik + 1, 4).Value = opis
End Sub

Private Function KolumnaLitera(kol As Long) As String
    KolumnaLitera = Split(ThisWorkbook.Worksheets("Dane").Cells(1, kol).Address(True, False), "$")(0)
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

    Dim biezaca As String
    biezaca = czesci(0)
    Dim i As Integer
    For i = 1 To UBound(czesci)
        biezaca = biezaca & "\" & czesci(i)
        If Dir(biezaca, vbDirectory) = "" Then MkDir biezaca
    Next i
End Sub

Private Function EscXml(s As String) As String
    Dim t As String
    t = Replace(s, "&", "&amp;")
    t = Replace(t, "<", "&lt;")
    t = Replace(t, ">", "&gt;")
    t = Replace(t, """", "&quot;")
    EscXml = t
End Function

' Zapis pliku jako Unicode (UTF-16LE z BOM) - format wymagany przez importer enova.
' To tylko zapis pliku lokalnego (ADODB.Stream), nie polaczenie z baza.
Private Sub ZapiszUnicode(sciezka As String, tresc As String)
    Dim strm As Object
    Set strm = CreateObject("ADODB.Stream")
    strm.Type = 2
    strm.Charset = "Unicode"
    strm.Open
    strm.WriteText tresc
    strm.SaveToFile sciezka, 2
    strm.Close
End Sub
