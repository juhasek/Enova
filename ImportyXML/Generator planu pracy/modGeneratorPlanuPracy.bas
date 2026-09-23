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
'
' Arkusz "Plan": jeden wiersz = jeden dzien dla jednego pracownika (kolumny A=Kod, B=Data,
' C=Nazwa dnia, D..K = Strefa 1..4 Od/Czas). Zadnego rozwijania zakresow dat/dni tygodnia -
' kazdy dzien, ktory ma powstac w enova, musi miec wlasny wiersz w arkuszu.
' Kolumna C "Nazwa dnia" musi byc nazwa DEFINICJI DNIA juz istniejaca w bazie enova
' (DefinicjeDni) - inaczej import zglasza blad "Definicja dnia o nazwie '' nie zostala
' znaleziona". Jesli C jest puste w danym wierszu, uzywana jest domyslna wartosc z
' Konfiguracja!B3 ("Nazwa definicji dnia") - ale jesli i ta jest pusta, wiersz jest bledny.

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
    If folderXml = "" Then
        ' Puste pole = zapisz obok tego skoroszytu (zawsze istniejacy, zawsze zapisywalny folder).
        folderXml = ThisWorkbook.Path & "\"
    ElseIf Right(folderXml, 1) <> "\" Then
        folderXml = folderXml & "\"
    End If

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

        If Not IsDate(wsPlan.Cells(r, "B").Value) Then
            bledy = bledy & "Wiersz " & r & ": nieprawidlowa Data." & vbCrLf
            GoTo NastepnyWiersz
        End If
        Dim dataDnia As Date
        dataDnia = CDate(wsPlan.Cells(r, "B").Value)

        Dim nazwaDniaWiersz As String
        nazwaDniaWiersz = Trim(CStr(wsPlan.Cells(r, "C").Value))
        If nazwaDniaWiersz = "" Then nazwaDniaWiersz = nazwaDefDnia
        If nazwaDniaWiersz = "" Then
            bledy = bledy & "Wiersz " & r & ": brak Nazwy dnia (kolumna C) i brak domyslnej w Konfiguracja!B3." & vbCrLf
            GoTo NastepnyWiersz
        End If

        ' Strefy: pary kolumn D/E, F/G, H/I, J/K (do 4 stref na dzien)
        Dim strefyOd(1 To 4) As String, strefyCzas(1 To 4) As String
        Dim liczbaStref As Integer
        liczbaStref = 0
        Dim sIdx As Integer
        For sIdx = 0 To 3
            Dim colOd As Integer, colCzas As Integer
            colOd = 4 + sIdx * 2
            colCzas = 5 + sIdx * 2
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
            bledy = bledy & "Wiersz " & r & ": brak zdefiniowanej zadnej strefy pracy (kolumny D..K)." & vbCrLf
            GoTo NastepnyWiersz
        End If

        Dim odGodzDnia As String
        Dim czasDniaMin As Long
        odGodzDnia = NajwczesniejszaGodzina(strefyOd, liczbaStref)
        czasDniaMin = SumaCzasowMin(strefyCzas, liczbaStref)

        Dim dataTxt As String
        dataTxt = Format(dataDnia, "yyyy-mm-dd")

        Dim fragment As String
        fragment = "<DzienPlanu>" & vbCrLf & _
            "<Pracownik>" & EscXml(kod) & "</Pracownik>" & vbCrLf & _
            "<Data>" & dataTxt & "</Data>" & vbCrLf & _
            "<Definicja>" & EscXml(nazwaDniaWiersz) & "</Definicja>" & vbCrLf & _
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

NastepnyWiersz:
    Next r

    If liczbaDni = 0 Then
        MsgBox "Nie wygenerowano zadnego dnia planu." & vbCrLf & vbCrLf & bledy, vbExclamation
        Exit Sub
    End If

    On Error Resume Next
    ZapewnijFolder folderXml
    If Err.Number <> 0 Then
        MsgBox "Nie udalo sie utworzyc/znalezc folderu:" & vbCrLf & folderXml & vbCrLf & vbCrLf & _
               "Blad: " & Err.Description & vbCrLf & vbCrLf & _
               "Popraw Konfiguracja!B2 (np. wpisz folder ktory na pewno istnieje, np. C:\Migracja) " & _
               "albo zostaw to pole puste - wtedy plik zapisze sie obok tego skoroszytu.", vbCritical, "Blad folderu"
        On Error GoTo 0
        Exit Sub
    End If
    On Error GoTo 0

    Dim nazwaPliku As String
    nazwaPliku = folderXml & prefiksPliku & " " & Format(Now, "yyyy-mm-dd_hhnnss") & ".xml"

    Dim xmlTxt As String
    xmlTxt = "<?xml version=""1.0"" encoding=""Unicode"" ?>" & vbCrLf & _
        "<Root xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">" & vbCrLf & _
        "<DniPlanu>" & vbCrLf & _
        dniXml & _
        "</DniPlanu>" & vbCrLf & _
        "</Root>" & vbCrLf

    On Error Resume Next
    ZapiszUnicode nazwaPliku, xmlTxt
    If Err.Number <> 0 Then
        MsgBox "Nie udalo sie zapisac pliku:" & vbCrLf & nazwaPliku & vbCrLf & vbCrLf & _
               "Blad: " & Err.Description & vbCrLf & vbCrLf & _
               "Sprawdz czy masz prawo zapisu do tego folderu.", vbCritical, "Blad zapisu"
        On Error GoTo 0
        Exit Sub
    End If
    On Error GoTo 0

    If Dir(nazwaPliku) = "" Then
        MsgBox "Zapis nie zglosil bledu, ale pliku nie widac pod:" & vbCrLf & nazwaPliku & vbCrLf & vbCrLf & _
               "Sprawdz uprawnienia/antywirusa/synchronizacje chmurowa (OneDrive) dla tego folderu.", vbExclamation, "Nie znaleziono pliku"
        Exit Sub
    End If

    Dim podsumowanie As String
    podsumowanie = "Wygenerowano plik XML (" & liczbaDni & " dni planu):" & vbCrLf & vbCrLf & nazwaPliku & vbCrLf & vbCrLf & _
        "Import w programie enova: Plik | Importuj zapisy | Import czasu pracy i wynagrodzen " & _
        "(wymaga zarejestrowanego rozszerzenia Soneta.CzasPracy.Migrator/Utils w docelowej bazie)."
    If bledy <> "" Then podsumowanie = podsumowanie & vbCrLf & vbCrLf & "BLEDY / pominiete wiersze:" & vbCrLf & bledy
    MsgBox podsumowanie, IIf(bledy <> "", vbExclamation, vbInformation), "Generator planu pracy"
End Sub

' Tworzy wszystkie brakujace poziomy folderu (MkDir tworzy tylko jeden poziom naraz).
Private Sub ZapewnijFolder(ByVal sciezka As String)
    Dim sciezkaBezSlasha As String
    sciezkaBezSlasha = sciezka
    If Right(sciezkaBezSlasha, 1) = "\" Then sciezkaBezSlasha = Left(sciezkaBezSlasha, Len(sciezkaBezSlasha) - 1)
    If sciezkaBezSlasha = "" Then Exit Sub
    If Dir(sciezkaBezSlasha, vbDirectory) <> "" Then Exit Sub

    Dim czesci() As String
    czesci = Split(sciezkaBezSlasha, "\")
    If UBound(czesci) < 1 Then Exit Sub ' sama litera dysku - nic do tworzenia

    Dim biezaca As String
    biezaca = czesci(0) ' np. "C:"
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
