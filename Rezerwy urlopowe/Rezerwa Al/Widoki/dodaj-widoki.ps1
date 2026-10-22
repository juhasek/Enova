$ErrorActionPreference = 'Stop'
$cs = "Server=localhost\SQLEXPRESS;Database=Al;Integrated Security=true;TrustServerCertificate=true"
$cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()

function HexUtf8Bom([string]$t) {
    $b = [byte[]](0xEF,0xBB,0xBF) + [Text.Encoding]::UTF8.GetBytes($t)
    return ($b | ForEach-Object { $_.ToString('X2') }) -join ''
}
function TextFromHex([string]$h) {
    $b = New-Object byte[] ($h.Length/2); for ($i=0; $i -lt $b.Length; $i++) { $b[$i] = [Convert]::ToByte($h.Substring($i*2,2),16) }
    $s = [Text.Encoding]::UTF8.GetString($b); return $s.TrimStart([char]0xFEFF)
}

# --- kopia bazy ---
$bk = $cn.CreateCommand(); $bk.CommandTimeout = 600
$bk.CommandText = "BACKUP DATABASE [Al] TO DISK = N'Al_przed_widoki_rezerwy_20261013.bak' WITH INIT, COPY_ONLY"
$bk.ExecuteNonQuery() | Out-Null
"Kopia bazy: Al_przed_widoki_rezerwy_20261013.bak"

$filtry = @'
  <Flow Name="FilterPanel">
    <Field CaptionHtml="Jednostka organizacyjna" Width="30" EditValue="{KadryPracownicyViewInfo+PrParams.Wydział}" Important="true" />
    <Field CaptionHtml="Z zależnymi" Width="7" EditValue="{KadryPracownicyViewInfo+PrParams.Pośrednio}" Important="true" />
    <Field CaptionHtml="Okres" Width="22" EditValue="{KadryPracownicyViewInfo+PrParams.Okres}" Important="true" />
    <Field CaptionHtml="Data" Width="15" EditValue="{KadryPracownicyViewInfo+PrParams.Aktualny}" Important="true" />
  </Flow>
'@

function Widok([string[][]]$kolumny) {
    $pola = ($kolumny | ForEach-Object { "    <Field CaptionHtml=""$($_[0])"" Width=""$($_[2])"" EditValue=""{$($_[1])}"" Footer=""$($_[3])"" />" }) -join "`r`n"
    return @"
<?xml version="1.0" encoding="utf-8"?>
<DataForm xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:schemaLocation="http://www.enova.pl/schema/ http://www.enova.pl/schema/form.xsd" xmlns="http://www.enova.pl/schema/form.xsd">
$filtry
  <Grid Name="List" OrderBy="Kod" AutoColumnWidthState="Off" PreviewControlLocation="0">
$pola
  </Grid>
</DataForm>
"@
}

$wspolne = @(
    @('Kod', 'Kod', '9', 'None'),
    @('Nazwisko i imię', 'NazwiskoImię', '30', 'None'),
    @('Wydział', 'Workers.Info.Historia.Etat.Wydzial', '25', 'None'),
    @('MPK', 'Features.Rezerwa MPK', '25', 'None')
)
$rezerwa = $wspolne + @(
    @('Urlop zaległy (dni)', 'Features.Rezerwa urlop zaległy', '12', 'Sum'),
    @('Urlop bieżący (dni)', 'Features.Rezerwa urlop bieżący', '12', 'Sum'),
    @('Urlop wykorzystany (dni)', 'Features.Rezerwa urlop wykorzystany', '12', 'Sum'),
    @('Godziny rezerwy', 'Features.Rezerwa godziny', '12', 'Sum'),
    @('Podstawa 1', 'Features.Rezerwa podstawa 1', '14', 'None'),
    @('Podstawa 2', 'Features.Rezerwa podstawa 2', '14', 'None'),
    @('Kwota rezerwy', 'Features.Rezerwa kwota', '14', 'Sum'),
    @('Narzuty', 'Features.Rezerwa narzuty', '14', 'Sum')
)
$budzet = $wspolne + @(
    @('Urlop zaległy na 01.01 (dni)', 'Features.Budżet urlop zaległy', '12', 'Sum'),
    @('Urlop należny (dni)', 'Features.Budżet urlop należny', '12', 'Sum'),
    @('Godziny budżetu', 'Features.Budżet godziny', '12', 'Sum'),
    @('Podstawa 1', 'Features.Budżet podstawa 1', '14', 'None'),
    @('Podstawa 2', 'Features.Budżet podstawa 2', '14', 'None'),
    @('Kwota budżetu', 'Features.Budżet kwota', '14', 'Sum'),
    @('Narzuty', 'Features.Budżet narzuty', '14', 'Sum')
)

$nowe = @(
    @{ Nazwa = 'Rezerwa urlopowa'; Xml = (Widok $rezerwa) },
    @{ Nazwa = 'Budżet rezerwy urlopowej'; Xml = (Widok $budzet) }
)

$tr = $cn.BeginTransaction()
try {
    $c = $cn.CreateCommand(); $c.Transaction = $tr
    $c.CommandText = "select a.ID, cast(a.MemoValue as nvarchar(max)) from CfgAttributes a where a.Node = 2096 and isnull(a.Name,'') = ''"
    $r = $c.ExecuteReader(); $r.Read() | Out-Null; $attrFolderow = $r.GetInt32(0); $folderyXml = TextFromHex $r.GetString(1); $r.Close()

    [xml]$doc = $folderyXml
    foreach ($w in $nowe) {
        if ($doc.FoldersInfo.Infos.Info | Where-Object { $_.Name -eq $w.Nazwa }) { throw "Widok '$($w.Nazwa)' już istnieje" }
        $g = [Guid]::NewGuid().ToString('N')
        $info = $doc.CreateElement('Info')
        foreach ($p in @(@('Name',$w.Nazwa), @('ID',"$($w.Nazwa).$g"), @('ViewType','Soneta.Business.Forms.Tables.TableView,Soneta.Business.Forms'), @('Icon','List'), @('SnapshotGuid','00000000-0000-0000-0000-000000000000'), @('Licenses','(pusty)'))) {
            $e = $doc.CreateElement($p[0]); $e.InnerText = $p[1]; $info.AppendChild($e) | Out-Null
        }
        $doc.FoldersInfo.Infos.AppendChild($info) | Out-Null

        $nazwaWezla = "$g.viewform.xml"; $nazwaWezla = $nazwaWezla.Substring($nazwaWezla.Length - 40)
        $c.Parameters.Clear()
        $c.CommandText = "insert into CfgNodes (Guid, Parent, Name, Type) values (newid(), 2097, @n, 1); select cast(scope_identity() as int)"
        $c.Parameters.AddWithValue('@n', $nazwaWezla) | Out-Null
        $wezel = [int]$c.ExecuteScalar()
        $c.Parameters.Clear()
        $c.CommandText = "insert into CfgAttributes (Node, Name, Type, MemoValue) values (@node, '', 15, @m)"
        $c.Parameters.AddWithValue('@node', $wezel) | Out-Null
        $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom $w.Xml)
        $c.ExecuteNonQuery() | Out-Null
        "Widok '$($w.Nazwa)': ID folderu $($w.Nazwa).$g, CfgNodes $wezel ($nazwaWezla)"
    }

    $sw = New-Object IO.StringWriter; $xs = New-Object Xml.XmlWriterSettings; $xs.Indent = $true; $xs.Encoding = [Text.Encoding]::UTF8
    $xw = [Xml.XmlWriter]::Create($sw, $xs); $doc.Save($xw); $xw.Close()
    $nowyFolderyXml = $sw.ToString() -replace 'encoding="utf-16"', 'encoding="utf-8"'
    $c.Parameters.Clear()
    $c.CommandText = "update CfgAttributes set MemoValue = @m where ID = @id"
    $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom $nowyFolderyXml)
    $c.Parameters.AddWithValue('@id', $attrFolderow) | Out-Null
    $c.ExecuteNonQuery() | Out-Null
    $tr.Commit(); "Zapisano."
} catch { $tr.Rollback(); throw }
finally { $cn.Close() }
