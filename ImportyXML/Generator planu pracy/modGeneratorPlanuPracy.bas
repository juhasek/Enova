Attribute VB_Name = "modGeneratorPlanuPracy"
Option Explicit

' Generator cyklicznego importu "dnia planu" (DzienPlanu) do enova365 z danych w arkuszu "Plan".
' Wymaga arkuszy "Konfiguracja" i "Plan" w tym samym skoroszycie (patrz arkusz "Instrukcja").
' Uzywa polaczenia ADODB (late binding, bez dodatkowych referencji) do bazy SQL enova,
' zeby dla kazdego Kodu pracownika odnalezc GUID jego indywidualnego kalendarza (Kalendarze.Typ=2).

Sub GenerujPlanyPracy()
    Dim wsCfg As Worksheet, wsPlan As Worksheet
    Set wsCfg = ThisWorkbook.Worksheets("Konfiguracja")
    Set wsPlan = ThisWorkbook.Worksheets("Plan")

    Dim sqlServer As String, baza As String, folderXml As String
    Dim guidDzienPracy As String, guidStrefaNorma As String, prefiksPliku As String
    sqlServer = Trim(wsCfg.Range("B2").Value)
    baza = Trim(wsCfg.Range("B3").Value)
    folderXml = Trim(wsCfg.Range("B4").Value)
    guidDzienPracy = Trim(wsCfg.Range("B5").Value)
    guidStrefaNorma = Trim(wsCfg.Range("B6").Value)
    prefiksPliku = Trim(wsCfg.Range("B7").Value)
    If Len(folderXml) > 0 And Right(folderXml, 1) <> "\" Then folderXml = folderXml & "\"

    Dim conn As Object
    Set conn = CreateObject("ADODB.Connection")

    On Error Resume Next
    conn.Open "Provider=MSOLEDBSQL;Data Source=" & sqlServer & ";Initial Catalog=" & baza & ";Integrated Security=SSPI;"
    If conn.State = 0 Then
        Err.Clear
        conn.Open "Provider=SQLOLEDB;Data Source=" & sqlServer & ";Initial Catalog=" & baza & ";Integrated Security=SSPI;"
    End If
    On Error GoTo 0

    If conn.State = 0 Then
        MsgBox "Nie udalo sie polaczyc z baza """ & baza & """ na serwerze """ & sqlServer & """." & vbCrLf & _
               "Sprawdz Konfiguracja!B2 (SQL Server) i B3 (Baza danych) oraz czy masz zainstalowany sterownik " & _
               "SQL (MSOLEDBSQL lub SQLOLEDB) i dostep Windows Auth do tego serwera.", vbCritical, "Blad polaczenia"
        Exit Sub
    End If

    Dim lastRow As Long
    lastRow = wsPlan.Cells(wsPlan.Rows.Count, "A").End(xlUp).Row
    If lastRow < 2 Then
        MsgBox "Arkusz ""Plan"" nie ma zadnych wierszy danych (wiersz 1 to naglowki).", vbExclamation
        conn.Close
        Exit Sub
    End If

    Dim kalendarze As Object: Set kalendarze = CreateObject("Scripting.Dictionary")
    Dim dniPerPracownik As Object: Set dniPerPracownik = CreateObject("Scripting.Dictionary")
    Dim minData As Object: Set minData = CreateObject("Scripting.Dictionary")
    Dim maxData As Object: Set maxData = CreateObject("Scripting.Dictionary")

    Dim r As Long
    Dim bledy As String
    Dim liczbaDni As Long
    liczbaDni = 0

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

        Dim guidKalendarza As String
        If kalendarze.Exists(kod) Then
            guidKalendarza = kalendarze(kod)
        Else
            guidKalendarza = PobierzGuidKalendarza(conn, kod)
            kalendarze.Add kod, guidKalendarza
        End If
        If guidKalendarza = "" Then
            bledy = bledy & "Wiersz " & r & ": w bazie """ & baza & """ nie ma pracownika o kodzie """ & kod & _
                   """ z zalozonym kalendarzem indywidualnym (najpierw zaimportuj/zapisz jego Etat w enova)." & vbCrLf
            GoTo NastepnyWiersz
        End If

        If Not dniPerPracownik.Exists(kod) Then dniPerPracownik.Add kod, ""

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
                fragment = "      <DzienKalendarzaBase class=""Soneta.Kalend.DzienPlanu,Soneta.KadryPlace"">" & vbCrLf & _
                    "        <Typ>KalendarzPracownika</Typ>" & vbCrLf & _
                    "        <Data>" & dataTxt & "</Data>" & vbCrLf & _
                    "        <Definicja>" & guidDzienPracy & "</Definicja>" & vbCrLf & _
                    "        <Praca>" & vbCrLf & _
                    "          <OdGodziny>" & odGodzDnia & "</OdGodziny>" & vbCrLf & _
                    "          <Czas>" & FormatMinuty(czasDniaMin) & "</Czas>" & vbCrLf & _
                    "        </Praca>" & vbCrLf & _
                    "        <Strefy>" & vbCrLf

                Dim sIdx2 As Integer
                For sIdx2 = 1 To liczbaStref
                    fragment = fragment & _
                        "          <StrefaKalendarza>" & vbCrLf & _
                        "            <Definicja>" & guidStrefaNorma & "</Definicja>" & vbCrLf & _
                        "            <Praca>" & vbCrLf & _
                        "              <OdGodziny>" & strefyOd(sIdx2) & "</OdGodziny>" & vbCrLf & _
                        "              <Czas>" & strefyCzas(sIdx2) & "</Czas>" & vbCrLf & _
                        "            </Praca>" & vbCrLf & _
                        "          </StrefaKalendarza>" & vbCrLf
                Next sIdx2

                fragment = fragment & "        </Strefy>" & vbCrLf & "      </DzienKalendarzaBase>" & vbCrLf

                dniPerPracownik(kod) = dniPerPracownik(kod) & fragment
                liczbaDni = liczbaDni + 1

                If Not minData.Exists(kod) Then
                    minData.Add kod, dataTxt
                ElseIf dataTxt < minData(kod) Then
                    minData(kod) = dataTxt
                End If
                If Not maxData.Exists(kod) Then
                    maxData.Add kod, dataTxt
                ElseIf dataTxt > maxData(kod) Then
                    maxData(kod) = dataTxt
                End If
            End If
        Next d

NastepnyWiersz:
    Next r

    conn.Close

    If dniPerPracownik.Count = 0 Then
        MsgBox "Nie wygenerowano zadnego dnia planu." & vbCrLf & vbCrLf & bledy, vbExclamation
        Exit Sub
    End If

    If Len(folderXml) > 0 Then
        If Dir(folderXml, vbDirectory) = "" Then MkDir folderXml
    End If

    Dim klucz As Variant
    Dim raport As String
    For Each klucz In dniPerPracownik.Keys
        Dim nazwaPliku As String
        nazwaPliku = folderXml & prefiksPliku & " - " & klucz & " - " & minData(klucz) & "_do_" & maxData(klucz) & ".xml"

        Dim xmlTxt As String
        xmlTxt = "<?xml version=""1.0"" encoding=""utf-8""?>" & vbCrLf & _
            "<!-- Wygenerowano makrem GenerujPlanyPracy dnia " & Format(Now, "yyyy-mm-dd hh:nn") & _
            ". Import przez dbmgr importxml " & baza & " w trybie standard. -->" & vbCrLf & _
            "<session xmlns=""http://www.soneta.pl/schema/business"" fromto=""" & minData(klucz) & "..." & maxData(klucz) & """>" & vbCrLf & _
            "  <KalendarzBase guid=""" & kalendarze(klucz) & """>" & vbCrLf & _
            "    <Dni>" & vbCrLf & _
            dniPerPracownik(klucz) & _
            "    </Dni>" & vbCrLf & _
            "  </KalendarzBase>" & vbCrLf & _
            "</session>" & vbCrLf

        ZapiszUtf8 nazwaPliku, xmlTxt
        raport = raport & klucz & ": " & nazwaPliku & vbCrLf
    Next klucz

    Dim podsumowanie As String
    podsumowanie = "Wygenerowano " & dniPerPracownik.Count & " plik(i) XML, lacznie " & liczbaDni & " dni planu:" & vbCrLf & vbCrLf & raport
    If bledy <> "" Then podsumowanie = podsumowanie & vbCrLf & "BLEDY / pominiete wiersze:" & vbCrLf & bledy
    MsgBox podsumowanie, IIf(bledy <> "", vbExclamation, vbInformation), "Generator planu pracy"
End Sub

' Pomocnicze makro do szybkiego sprawdzenia polaczenia z baza bez generowania plikow.
Sub TestujPolaczenie()
    Dim wsCfg As Worksheet
    Set wsCfg = ThisWorkbook.Worksheets("Konfiguracja")
    Dim sqlServer As String, baza As String
    sqlServer = Trim(wsCfg.Range("B2").Value)
    baza = Trim(wsCfg.Range("B3").Value)

    Dim conn As Object
    Set conn = CreateObject("ADODB.Connection")
    On Error Resume Next
    conn.Open "Provider=MSOLEDBSQL;Data Source=" & sqlServer & ";Initial Catalog=" & baza & ";Integrated Security=SSPI;"
    If conn.State = 0 Then
        Err.Clear
        conn.Open "Provider=SQLOLEDB;Data Source=" & sqlServer & ";Initial Catalog=" & baza & ";Integrated Security=SSPI;"
    End If
    On Error GoTo 0

    If conn.State = 0 Then
        MsgBox "Polaczenie NIEUDANE z """ & baza & """ na """ & sqlServer & """.", vbCritical
    Else
        MsgBox "Polaczenie OK z """ & baza & """ na """ & sqlServer & """.", vbInformation
        conn.Close
    End If
End Sub

Private Function PobierzGuidKalendarza(conn As Object, kod As String) As String
    Dim rs As Object
    Set rs = CreateObject("ADODB.Recordset")
    Dim sql As String
    sql = "SELECT k.Guid FROM Kalendarze k INNER JOIN Pracownicy p ON k.Pracownik = p.ID " & _
          "WHERE p.Kod = " & SqlQuote(kod) & " AND k.Typ = 2"
    On Error Resume Next
    rs.Open sql, conn
    On Error GoTo 0
    If Not rs Is Nothing Then
        If rs.State = 1 Then
            If Not rs.EOF Then
                PobierzGuidKalendarza = Trim(CStr(rs.Fields(0).Value))
            Else
                PobierzGuidKalendarza = ""
            End If
            rs.Close
        Else
            PobierzGuidKalendarza = ""
        End If
    End If
End Function

Private Function SqlQuote(s As String) As String
    SqlQuote = "'" & Replace(s, "'", "''") & "'"
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

' Zapisuje tekst jako plik UTF-8 BEZ BOM (dbmgr importxml oczekuje czystego UTF-8).
Private Sub ZapiszUtf8(sciezka As String, tresc As String)
    Dim strm As Object
    Set strm = CreateObject("ADODB.Stream")
    strm.Type = 2 ' adTypeText
    strm.Charset = "utf-8"
    strm.Open
    strm.WriteText tresc
    strm.Position = 0
    strm.Type = 1 ' adTypeBinary - zeby wyciac BOM
    strm.Position = 0

    Dim bajty() As Byte
    bajty = strm.Read
    strm.Close

    Dim dlugosc As Long
    dlugosc = UBound(bajty) - LBound(bajty) + 1

    Dim bezBom() As Byte
    If dlugosc >= 3 Then
        If bajty(0) = 239 And bajty(1) = 187 And bajty(2) = 191 Then
            ReDim bezBom(0 To dlugosc - 4)
            Dim i As Long
            For i = 3 To dlugosc - 1
                bezBom(i - 3) = bajty(i)
            Next i
        Else
            bezBom = bajty
        End If
    Else
        bezBom = bajty
    End If

    Dim strm2 As Object
    Set strm2 = CreateObject("ADODB.Stream")
    strm2.Type = 1 ' adTypeBinary
    strm2.Open
    strm2.Write bezBom
    strm2.SaveToFile sciezka, 2 ' adSaveCreateOverWrite
    strm2.Close
End Sub
