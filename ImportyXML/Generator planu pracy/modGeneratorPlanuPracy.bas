Attribute VB_Name = "modGeneratorPlanuPracy"
Option Explicit

' Generator cyklicznego importu "dnia planu" (Norma czasu pracy) do enova365 z danych w arkuszu "Plan".
' Wymaga arkuszy "Konfiguracja" i "Plan" w tym samym skoroszycie (patrz arkusz "Instrukcja").
'
' Generuje plik(i) XML w OFICJALNYM formacie enova <Root><DniPlanu><DzienPlanu>...</DzienPlanu></DniPlanu></Root>
' (ten sam format co wzorcowy arkusz Soneta "xml- Norma pracy.xlsm"), importowany w programie enova przez:
'   Plik | Importuj zapisy | Import czasu pracy i wynagrodzen
' (wymaga zarejestrowanego w bazie rozszerzenia Soneta.CzasPracy.Migrator/Utils).
'
' TO MAKRO NIE LACZY SIE Z BAZA SQL W ZADEN SPOSOB. Pracownik jest identyfikowany WYLACZNIE
' po polu <Pracownik> = Kod pracownika - importer enova sam go wyszukuje (kadry.Pracownicy.WgKodu),
' bez znaczenia jaki kalendarz/GUID ma jego indywidualny kalendarz.
' Dzien przerywany (np. 8-12 i 13-17) to kolekcja <Strefy> z wieloma <StrefaPracy Definicja=... OdGodziny=... Czas=.../>.

Sub GenerujPlanyPracy()
    Dim wsCfg As Worksheet, wsPlan As Worksheet
    Set wsCfg = ThisWorkbook.Worksheets("Konfiguracja")
    Set wsPlan = ThisWorkbook.Worksheets("Plan")

    Dim folderXml As String
    Dim nazwaDefDnia As String, nazwaDefStrefy As String, prefiksPliku As String
    folderXml = Trim(wsCfg.Range("B2").Value)
    nazwaDefDnia = Trim(wsCfg.Range("B3").Value)
    nazwaDefStrefy = Trim(wsCfg.Range("B4").Value)
    prefiksPliku = Trim(wsCfg.Range("B5").Value)
    If Len(folderXml) > 0 And Right(folderXml, 1) <> "\" Then folderXml = folderXml & "\"

    Dim lastRow As Long
    lastRow = wsPlan.Cells(wsPlan.Rows.Count, "A").End(xlUp).Row
    If lastRow < 2 Then
        MsgBox "Arkusz ""Plan"" nie ma zadnych wierszy danych (wiersz 1 to naglowki).", vbExclamation
        Exit Sub
    End If

    Dim dniXml As String
    Dim liczbaDni As Long
    dniXml = ""
    liczbaDni = 0

    Dim r As Long
    Dim bledy As String

    For r = 2 To lastRow
        Dim kod As String
        kod = Trim(CStr(wsPlan.Cells(r, "A").Value))
        If kod = "" Then GoTo NastepnyWiersz

        If Not IsDate(wsPlan.Cells(r, "B").Value) Or Not IsDate(wsPlan.Cells(r, "C").Value) Then
            bledy = bledy & "Wiersz " & r & ": nieprawidlowa Data od / Data do." & vbCrLf
            GoTo NastepnyWiersz
        End If
        Dim dataOd As Date, dataDo As Date
        dataOd = CDate(wsPlan.Cells(r, "B").Value)
        dataDo = CDate(wsPlan.Cells(r, "C").Value)
        If dataDo < dataOd Then
            bledy = bledy & "Wiersz " & r & ": Data do jest wczesniejsza niz Data od." & vbCrLf
            GoTo NastepnyWiersz
        End If

        ' Kolumny D..J = Pn..Nd (1=Pn .. 7=Nd wg Weekday(d, vbMonday)); dowolna niepusta wartosc = dzien wlaczony
        Dim dniTyg(1 To 7) As Boolean
        Dim kIdx As Integer
        For kIdx = 1 To 7
            dniTyg(kIdx) = (Trim(CStr(wsPlan.Cells(r, 3 + kIdx).Value)) <> "")
        Next kIdx

        ' Strefy: pary kolumn K/L, M/N, O/P, Q/R (do 4 stref na dzien)
        Dim strefyOd(1 To 4) As String, strefyCzas(1 To 4) As String
        Dim liczbaStref As Integer
        liczbaStref = 0
        Dim sIdx As Integer
        For sIdx = 0 To 3
            Dim colOd As Integer, colCzas As Integer
            colOd = 11 + sIdx * 2
            colCzas = 12 + sIdx * 2
            Dim vOd As String, vCzas As String
            vOd = FormatCzas(wsPlan.Cells(r, colOd).Value)
            vCzas = FormatCzas(wsPlan.Cells(r, colCzas).Value)
            If vOd <> "" And vCzas <> "" Then
                liczbaStref = liczbaStref + 1
                strefyOd(liczbaStref) = vOd
                strefyCzas(liczbaStref) = vCzas
            End If
        Next sIdx

        If liczbaStref = 0 Then
            bledy = bledy & "Wiersz " & r & ": brak zdefiniowanej zadnej strefy pracy (kolumny K..R)." & vbCrLf
            GoTo NastepnyWiersz
        End If

        Dim odGodzDnia As String
        Dim czasDniaMin As Long
        odGodzDnia = NajwczesniejszaGodzina(strefyOd, liczbaStref)
        czasDniaMin = SumaCzasowMin(strefyCzas, liczbaStref)

        Dim d As Date
        For d = dataOd To dataDo
            Dim nrDnia As Integer
            nrDnia = Weekday(d, vbMonday)
            If dniTyg(nrDnia) Then
                Dim dataTxt As String
                dataTxt = Format(d, "yyyy-mm-dd")

                Dim fragment As String
                fragment = "<DzienPlanu>" & vbCrLf & _
                    "<Pracownik>" & EscXml(kod) & "</Pracownik>" & vbCrLf & _
                    "<Data>" & dataTxt & "</Data>" & vbCrLf & _
                    "<Definicja>" & EscXml(nazwaDefDnia) & "</Definicja>" & vbCrLf & _
                    "<OdGodziny>" & odGodzDnia & "</OdGodziny>" & vbCrLf & _
                    "<Czas>" & FormatMinuty(czasDniaMin) & "</Czas>" & vbCrLf & _
                    "<Strefy>" & vbCrLf

                Dim sIdx2 As Integer
                For sIdx2 = 1 To liczbaStref
                    fragment = fragment & _
                        "<StrefaPracy Definicja=""" & EscXml(nazwaDefStrefy) & """ OdGodziny=""" & _
                        strefyOd(sIdx2) & """ Czas=""" & strefyCzas(sIdx2) & """ />" & vbCrLf
                Next sIdx2

                fragment = fragment & "</Strefy>" & vbCrLf & "</DzienPlanu>" & vbCrLf

                dniXml = dniXml & fragment
                liczbaDni = liczbaDni + 1
            End If
        Next d

NastepnyWiersz:
    Next r

    If liczbaDni = 0 Then
        MsgBox "Nie wygenerowano zadnego dnia planu." & vbCrLf & vbCrLf & bledy, vbExclamation
        Exit Sub
    End If

    If Len(folderXml) > 0 Then
        If Dir(folderXml, vbDirectory) = "" Then MkDir folderXml
    End If

    Dim nazwaPliku As String
    nazwaPliku = folderXml & prefiksPliku & " " & Format(Now, "yyyy-mm-dd_hhnnss") & ".xml"

    Dim xmlTxt As String
    xmlTxt = "<?xml version=""1.0"" encoding=""Unicode"" ?>" & vbCrLf & _
        "<Root xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">" & vbCrLf & _
        "<DniPlanu>" & vbCrLf & _
        dniXml & _
        "</DniPlanu>" & vbCrLf & _
        "</Root>" & vbCrLf

    ZapiszUnicode nazwaPliku, xmlTxt

    Dim podsumowanie As String
    podsumowanie = "Wygenerowano plik XML (" & liczbaDni & " dni planu):" & vbCrLf & vbCrLf & nazwaPliku & vbCrLf & vbCrLf & _
        "Import w programie enova: Plik | Importuj zapisy | Import czasu pracy i wynagrodzen " & _
        "(wymaga zarejestrowanego rozszerzenia Soneta.CzasPracy.Migrator/Utils w docelowej bazie)."
    If bledy <> "" Then podsumowanie = podsumowanie & vbCrLf & vbCrLf & "BLEDY / pominiete wiersze:" & vbCrLf & bledy
    MsgBox podsumowanie, IIf(bledy <> "", vbExclamation, vbInformation), "Generator planu pracy"
End Sub

Private Function EscXml(s As String) As String
    Dim t As String
    t = Replace(s, "&", "&amp;")
    t = Replace(t, "<", "&lt;")
    t = Replace(t, ">", "&gt;")
    t = Replace(t, """", "&quot;")
    EscXml = t
End Function

' Akceptuje: puste, tekst "H:MM", liczbe/godzine Excela (ulamek doby) - zwraca "H:MM" albo "".
Private Function FormatCzas(v As Variant) As String
    If IsEmpty(v) Then
        FormatCzas = ""
        Exit Function
    End If
    If VarType(v) = vbString Then
        Dim s As String
        s = Trim(CStr(v))
        If InStr(s, ":") = 0 Then
            FormatCzas = ""
        Else
            FormatCzas = s
        End If
        Exit Function
    End If
    If IsNumeric(v) Then
        Dim totalMin As Long
        totalMin = CLng(Round(CDbl(v) * 24 * 60))
        FormatCzas = FormatMinuty(totalMin)
    Else
        FormatCzas = ""
    End If
End Function

Private Function FormatMinuty(totalMin As Long) As String
    Dim h As Long, m As Long
    h = totalMin \ 60
    m = totalMin Mod 60
    FormatMinuty = CStr(h) & ":" & Format(m, "00")
End Function

Private Function NajwczesniejszaGodzina(strefyOd() As String, n As Integer) As String
    Dim i As Integer
    Dim najmniejszyMin As Long, biezacyMin As Long
    najmniejszyMin = -1
    For i = 1 To n
        biezacyMin = MinutyZGodziny(strefyOd(i))
        If najmniejszyMin = -1 Or biezacyMin < najmniejszyMin Then najmniejszyMin = biezacyMin
    Next i
    NajwczesniejszaGodzina = FormatMinuty(najmniejszyMin)
End Function

Private Function SumaCzasowMin(strefyCzas() As String, n As Integer) As Long
    Dim i As Integer, suma As Long
    suma = 0
    For i = 1 To n
        suma = suma + MinutyZGodziny(strefyCzas(i))
    Next i
    SumaCzasowMin = suma
End Function

Private Function MinutyZGodziny(s As String) As Long
    Dim czesci() As String
    czesci = Split(s, ":")
    MinutyZGodziny = CLng(czesci(0)) * 60 + CLng(czesci(1))
End Function

' Zapisuje tekst jako plik Unicode (UTF-16LE z BOM) - dokladnie taki format, jaki wymaga
' importer enova "Import czasu pracy i wynagrodzen" (encoding="Unicode" w naglowku XML).
' To tylko zapis pliku lokalnego (ADODB.Stream) - NIE jest to polaczenie z baza danych.
Private Sub ZapiszUnicode(sciezka As String, tresc As String)
    Dim strm As Object
    Set strm = CreateObject("ADODB.Stream")
    strm.Type = 2 ' adTypeText
    strm.Charset = "Unicode" ' UTF-16LE z BOM - format wymagany przez importer enova
    strm.Open
    strm.WriteText tresc
    strm.SaveToFile sciezka, 2 ' adSaveCreateOverWrite
    strm.Close
End Sub
