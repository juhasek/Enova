$ErrorActionPreference = 'Stop'
# Widok "Nadgodziny" pod Kadry i płace/Kadry/Ewidencje/Dokumenty dodatkowe (baza Al).
# Lista = dokumenty dodatkowe wszystkich definicji; w filtrze "Definicja" wybrać e-wniosek o nadgodziny.
$baza = 'Al'
$cs = "Server=localhost\SQLEXPRESS;Database=$baza;Integrated Security=true;TrustServerCertificate=true"
$cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()

function HexUtf8Bom([string]$t) {
    $b = [byte[]](0xEF,0xBB,0xBF) + [Text.Encoding]::UTF8.GetBytes($t)
    return ($b | ForEach-Object { $_.ToString('X2') }) -join ''
}
function TextFromHex([string]$h) {
    $b = New-Object byte[] ($h.Length/2); for ($i=0; $i -lt $b.Length; $i++) { $b[$i] = [Convert]::ToByte($h.Substring($i*2,2),16) }
    $s = [Text.Encoding]::UTF8.GetString($b); return $s.TrimStart([char]0xFEFF)
}
function Ostatnie40([string]$n) { if ($n.Length -gt 40) { return $n.Substring($n.Length - 40) } else { return $n } }

# --- kopia bazy ---
$dzis = Get-Date -Format 'yyyyMMdd'
$bk = $cn.CreateCommand(); $bk.CommandTimeout = 600
$bk.CommandText = "BACKUP DATABASE [$baza] TO DISK = N'${baza}_przed_widok_nadgodziny_$dzis.bak' WITH INIT, COPY_ONLY"
$bk.ExecuteNonQuery() | Out-Null
"Kopia bazy: ${baza}_przed_widok_nadgodziny_$dzis.bak"

$nazwaWidoku = 'Nadgodziny'
$kolumny = @(
    @('Kod', 'Pracownik.Kod', '9'),
    @('Imię i nazwisko', 'Pracownik.ImięNazwisko', '25'),
    @('Data pracy', 'WniosekData', '10'),
    @('Od godziny', 'WniosekElement.OdGodziny', '8'),
    @('Do godziny', 'DoGodziny', '8'),
    @('Uwagi', 'WniosekOpis', '40'),
    @('Przełożony', 'Kierownik', '25'),
    @('Data decyzji', 'DataDecyzji', '10'),
    @('Do przeniesienia', 'DoPrzeniesienia', '10')
)
$pola = ($kolumny | ForEach-Object { "    <Field CaptionHtml=""$($_[0])"" Width=""$($_[2])"" EditValue=""{$($_[1])}"" Footer=""None"" />" }) -join "`r`n"
$viewform = @"
<?xml version="1.0" encoding="utf-8"?>
<DataForm xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:schemaLocation="http://www.enova.pl/schema/ http://www.enova.pl/schema/form.xsd" xmlns="http://www.enova.pl/schema/form.xsd">
  <Flow Name="FilterPanel">
    <Field CaptionHtml="Kategoria" Width="25" EditValue="{HostTuplesPageParams.Kategoria}" />
    <Field CaptionHtml="Definicja" Width="25" EditValue="{HostTuplesPageParams.Definicja}" />
    <Field CaptionHtml="Dane" Width="25" EditValue="{HostTuplesPageParams.Filtr}" />
    <Field CaptionHtml="Okres" Width="25" EditValue="{HostTuplesPageParams.Okres}" />
  </Flow>
  <Grid Name="List" OrderBy="WniosekData" AutoColumnWidthState="Off" PreviewControlLocation="0">
$pola
  </Grid>
</DataForm>
"@

$pustyFolder = @"
<?xml version="1.0" encoding="utf-8"?>
<FoldersInfo xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Infos>
  </Infos>
</FoldersInfo>
"@

$tr = $cn.BeginTransaction()
try {
    $c = $cn.CreateCommand(); $c.Transaction = $tr
    $c.CommandText = "select ID from CfgNodes where Name = 'foldery' and Parent = (select ID from CfgNodes where Name = 'Storages')"
    $wezelFoldery = [int]$c.ExecuteScalar()
    $c.CommandText = "select ID from CfgNodes where Name = 'tableviews' and Parent = (select ID from CfgNodes where Name = 'Storages')"
    $wezelWidoki = [int]$c.ExecuteScalar()

    # węzeł folderu (nazwa ucinana od lewej do 40 znaków)
    $nazwaFolderu = Ostatnie40 'kadry i płace.kadry.ewidencje.dokumenty dodatkowe.xml'
    $c.Parameters.Clear()
    $c.CommandText = "select a.ID, cast(a.MemoValue as nvarchar(max)) from CfgNodes n join CfgAttributes a on a.Node = n.ID and isnull(a.Name,'') = '' where n.Parent = @p and n.Name = @n"
    $c.Parameters.AddWithValue('@p', $wezelFoldery) | Out-Null
    $c.Parameters.AddWithValue('@n', $nazwaFolderu) | Out-Null
    $r = $c.ExecuteReader()
    if ($r.Read()) { $attrFolderow = $r.GetInt32(0); $folderyXml = TextFromHex $r.GetString(1) } else { $attrFolderow = 0; $folderyXml = $pustyFolder }
    $r.Close()
    if ($attrFolderow -eq 0) {
        $c.Parameters.Clear()
        $c.CommandText = "insert into CfgNodes (Guid, Parent, Name, Type) values (newid(), @p, @n, 1); select cast(scope_identity() as int)"
        $c.Parameters.AddWithValue('@p', $wezelFoldery) | Out-Null
        $c.Parameters.AddWithValue('@n', $nazwaFolderu) | Out-Null
        $wezelFolderu = [int]$c.ExecuteScalar()
        $c.Parameters.Clear()
        $c.CommandText = "insert into CfgAttributes (Node, Name, Type, MemoValue) values (@node, '', 15, ''); select cast(scope_identity() as int)"
        $c.Parameters.AddWithValue('@node', $wezelFolderu) | Out-Null
        $attrFolderow = [int]$c.ExecuteScalar()
        "Nowy węzeł folderu: CfgNodes $wezelFolderu ($nazwaFolderu)"
    }

    [xml]$doc = $folderyXml
    if ($doc.SelectNodes('/FoldersInfo/Infos/Info') | Where-Object { $_.Name -eq $nazwaWidoku }) { throw "Widok '$nazwaWidoku' już istnieje" }
    $g = [Guid]::NewGuid().ToString('N')
    $info = $doc.CreateElement('Info')
    foreach ($p in @(@('Name',$nazwaWidoku), @('ID',"$nazwaWidoku.$g"), @('ViewType','Soneta.Business.Forms.Tables.TableView,Soneta.Business.Forms'), @('Icon','List'), @('SnapshotGuid','00000000-0000-0000-0000-000000000000'), @('Licenses','(pusty)'))) {
        $e = $doc.CreateElement($p[0]); $e.InnerText = $p[1]; $info.AppendChild($e) | Out-Null
    }
    $doc.SelectSingleNode('/FoldersInfo/Infos').AppendChild($info) | Out-Null

    $nazwaWezla = Ostatnie40 "$g.viewform.xml"
    $c.Parameters.Clear()
    $c.CommandText = "insert into CfgNodes (Guid, Parent, Name, Type) values (newid(), @p, @n, 1); select cast(scope_identity() as int)"
    $c.Parameters.AddWithValue('@p', $wezelWidoki) | Out-Null
    $c.Parameters.AddWithValue('@n', $nazwaWezla) | Out-Null
    $wezel = [int]$c.ExecuteScalar()
    $c.Parameters.Clear()
    $c.CommandText = "insert into CfgAttributes (Node, Name, Type, MemoValue) values (@node, '', 15, @m)"
    $c.Parameters.AddWithValue('@node', $wezel) | Out-Null
    $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom $viewform)
    $c.ExecuteNonQuery() | Out-Null
    "Widok '$nazwaWidoku': ID folderu $nazwaWidoku.$g, CfgNodes $wezel ($nazwaWezla)"

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
