# Zakłada widok "Przypomnienia" pod Kadry i płace -> Kadry -> Pracownicy (konfiguracja bazy, bez DLL).
# Wzór: Rezerwy urlopowe/Rezerwa Al/Widoki/dodaj-widoki.ps1. Uruchomienie: powershell -File dodaj-widok.ps1 -Baza Claude
# Polskie znaki w kodzie przez [char], plik czytany bez BOM przez PowerShell 5.1.
param([string]$Baza = 'Claude', [string]$Nazwa = 'Przypomnienia')
$ErrorActionPreference = 'Stop'
$viewform = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Przypomnienia.viewform.xml'), [Text.Encoding]::UTF8)
$plikFolderow = "kadry i p$([char]0x0142)ace.kadry.pracownicy.xml"

$cn = New-Object System.Data.SqlClient.SqlConnection "Server=localhost\SQLEXPRESS;Database=$Baza;Integrated Security=true;TrustServerCertificate=true"
$cn.Open()

function HexUtf8Bom([string]$t) {
    $b = [byte[]](0xEF,0xBB,0xBF) + [Text.Encoding]::UTF8.GetBytes($t)
    return ($b | ForEach-Object { $_.ToString('X2') }) -join ''
}
function TextFromHex([string]$h) {
    $b = New-Object byte[] ($h.Length/2); for ($i=0; $i -lt $b.Length; $i++) { $b[$i] = [Convert]::ToByte($h.Substring($i*2,2),16) }
    return [Text.Encoding]::UTF8.GetString($b).TrimStart([char]0xFEFF)
}
function Skalar($c, [string]$sql) { $c.CommandText = $sql; return $c.ExecuteScalar() }

$bk = $cn.CreateCommand(); $bk.CommandTimeout = 600
$kopia = "$($Baza)_przed_widok_przypomnienia_$(Get-Date -Format yyyyMMdd_HHmm).bak"
$bk.CommandText = "BACKUP DATABASE [$Baza] TO DISK = N'$kopia' WITH INIT, COPY_ONLY"; $bk.ExecuteNonQuery() | Out-Null
"Kopia bazy: $kopia"

$tr = $cn.BeginTransaction()
try {
    $c = $cn.CreateCommand(); $c.Transaction = $tr
    $foldery = [int](Skalar $c "select ID from CfgNodes where Name = 'foldery' and Parent = (select ID from CfgNodes where Name = 'Storages')")
    $tableviews = [int](Skalar $c "select ID from CfgNodes where Name = 'tableviews' and Parent = (select ID from CfgNodes where Name = 'Storages')")

    $c.Parameters.Clear(); $c.Parameters.AddWithValue('@n', $plikFolderow) | Out-Null; $c.Parameters.AddWithValue('@p', $foldery) | Out-Null
    $wezelFolderow = Skalar $c "select ID from CfgNodes where Parent = @p and Name = @n"
    if ($wezelFolderow -eq $null) {
        $wezelFolderow = [int](Skalar $c "insert into CfgNodes (Guid, Parent, Name, Type) values (newid(), @p, @n, 1); select cast(scope_identity() as int)")
        $c.Parameters.Clear()
        $pusty = '<?xml version="1.0" encoding="utf-8"?><FoldersInfo xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"><Infos /></FoldersInfo>'
        $c.CommandText = "insert into CfgAttributes (Node, Name, Type, MemoValue) values (@node, '', 15, @m)"
        $c.Parameters.AddWithValue('@node', $wezelFolderow) | Out-Null
        $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom $pusty)
        $c.ExecuteNonQuery() | Out-Null
        "Utworzono plik folderow Pracownicy: CfgNodes $wezelFolderow"
    }
    $c.Parameters.Clear()
    $c.CommandText = "select ID, cast(MemoValue as nvarchar(max)) from CfgAttributes where Node = $wezelFolderow and isnull(Name,'') = ''"
    $r = $c.ExecuteReader(); $r.Read() | Out-Null; $attrFolderow = $r.GetInt32(0); [xml]$doc = TextFromHex $r.GetString(1); $r.Close()
    if ($doc.FoldersInfo.Infos.Info | Where-Object { $_.Name -eq $Nazwa }) { throw "Widok '$Nazwa' juz istnieje" }

    $g = [Guid]::NewGuid().ToString('N')
    $info = $doc.CreateElement('Info')
    foreach ($p in @(@('Name',$Nazwa), @('ID',"$Nazwa.$g"), @('ViewType','Soneta.Business.Forms.Tables.TableView,Soneta.Business.Forms'), @('Icon','List'), @('SnapshotGuid','00000000-0000-0000-0000-000000000000'), @('Licenses','(pusty)'))) {
        $e = $doc.CreateElement($p[0]); $e.InnerText = $p[1]; $info.AppendChild($e) | Out-Null
    }
    $doc.FoldersInfo.SelectSingleNode('Infos').AppendChild($info) | Out-Null

    $nazwaWezla = "$g.viewform.xml"; $nazwaWezla = $nazwaWezla.Substring($nazwaWezla.Length - 40)
    $c.Parameters.Clear(); $c.Parameters.AddWithValue('@n', $nazwaWezla) | Out-Null
    $wezel = [int](Skalar $c "insert into CfgNodes (Guid, Parent, Name, Type) values (newid(), $tableviews, @n, 1); select cast(scope_identity() as int)")
    $c.Parameters.Clear()
    $c.CommandText = "insert into CfgAttributes (Node, Name, Type, MemoValue) values (@node, '', 15, @m)"
    $c.Parameters.AddWithValue('@node', $wezel) | Out-Null
    $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom $viewform)
    $c.ExecuteNonQuery() | Out-Null

    $sw = New-Object IO.StringWriter; $xs = New-Object Xml.XmlWriterSettings; $xs.Indent = $true
    $xw = [Xml.XmlWriter]::Create($sw, $xs); $doc.Save($xw); $xw.Close()
    $c.Parameters.Clear()
    $c.CommandText = "update CfgAttributes set MemoValue = @m where ID = @id"
    $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom ($sw.ToString() -replace 'encoding="utf-16"', 'encoding="utf-8"'))
    $c.Parameters.AddWithValue('@id', $attrFolderow) | Out-Null
    $c.ExecuteNonQuery() | Out-Null
    $tr.Commit()
    "Widok '$Nazwa': ID $Nazwa.$g, CfgNodes $wezel ($nazwaWezla)"
} catch { $tr.Rollback(); throw }
finally { $cn.Close() }
