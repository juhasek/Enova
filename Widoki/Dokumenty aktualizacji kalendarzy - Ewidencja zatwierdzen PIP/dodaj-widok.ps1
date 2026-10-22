# Podwidok "Ewidencja zatwierdzeń (PIP)" pod listą Kadry i płace / Kadry / Czas pracy / Dokumenty aktualizacji kalendarzy:
# kto i kiedy złożył (przekazał do akceptacji) i zatwierdził plan / czas pracy pracownika.
# Kolumny standardowe + cechy DokAktKalendarzy (Cechy/Dokument aktualizacji kalendarza - *).
# Wzór: Rezerwy urlopowe/Rezerwa Al/Widoki/dodaj-widoki.ps1 (CfgNodes/CfgAttributes, treść jako HEX UTF-8 z BOM).
param([string]$Baza = 'Claude')
$ErrorActionPreference = 'Stop'

$cs = "Server=localhost\SQLEXPRESS;Database=$Baza;Integrated Security=true;TrustServerCertificate=true"
$cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()

# --- kopia bazy przed zmianą ---
$bk = $cn.CreateCommand(); $bk.CommandTimeout = 600
$plikKopii = "${Baza}_przed_widokiem_DAK_PIP_$(Get-Date -Format yyyyMMdd_HHmm).bak"
$bk.CommandText = "BACKUP DATABASE [$Baza] TO DISK = N'$plikKopii' WITH INIT, COPY_ONLY"
$bk.ExecuteNonQuery() | Out-Null
"Kopia bazy: $plikKopii"

function HexUtf8Bom([string]$t) {
    $b = [byte[]](0xEF,0xBB,0xBF) + [Text.Encoding]::UTF8.GetBytes($t)
    return ($b | ForEach-Object { $_.ToString('X2') }) -join ''
}
function TextFromHex([string]$h) {
    $b = New-Object byte[] ($h.Length/2); for ($i=0; $i -lt $b.Length; $i++) { $b[$i] = [Convert]::ToByte($h.Substring($i*2,2),16) }
    $s = [Text.Encoding]::UTF8.GetString($b); return $s.TrimStart([char]0xFEFF)
}
# Nazwy węzłów CfgNodes mają najwyżej 40 znaków - dłuższe są ucinane od lewej.
function NazwaWezla([string]$n) { if ($n.Length -gt 40) { return $n.Substring($n.Length - 40) } return $n }

$nazwaWidoku = 'Ewidencja zatwierdzeń (PIP)'
$plikFolderow = NazwaWezla 'kadry i płace.kadry.czas pracy.dokumenty aktualizacji kalendarzy.xml'

$kolumny = @(
    @('Pracownik', 'Features.PracownikNazwisko', '26'),
    @('Kod pracownika', 'Features.PracownikKod', '10'),
    @('Dział', 'Features.DzialNazwa', '18'),
    @('Okres', 'Okres', '17'),
    @('Rodzaj (definicja)', 'Definicja', '22'),
    @('Numer', 'Numer', '16'),
    @('Stan', 'Stan', '11'),
    @('Aktualny', 'Features.Aktualny', '8'),
    @('Złożył', 'Features.ZlozylKto', '26'),
    @('Złożono', 'Features.ZlozylKiedy', '16'),
    @('Złożył jako', 'Features.ZlozylRola', '14'),
    @('Zatwierdził', 'Features.ZatwierdzilKto', '26'),
    @('Zatwierdzono', 'Features.ZatwierdzilKiedy', '16'),
    @('Zatwierdził jako', 'Features.ZatwierdzilRola', '14'),
    @('Przełożony', 'Features.Przelozony', '24')
)
$pola = ($kolumny | ForEach-Object { "    <Field CaptionHtml=""$($_[0])"" Width=""$($_[2])"" EditValue=""{$($_[1])}"" Footer=""None"" />" }) -join "`r`n"
$viewform = @"
<?xml version="1.0" encoding="utf-8"?>
<DataForm xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:schemaLocation="http://www.enova.pl/schema/ http://www.enova.pl/schema/form.xsd" xmlns="http://www.enova.pl/schema/form.xsd">
  <Flow Name="FilterPanel">
    <Field CaptionHtml="Definicja" Width="30" EditValue="{KadryDokAktKalendarzyViewInfo+Params.Definicja}" />
    <Field CaptionHtml="Zakres dat" Width="25" EditValue="{KadryDokAktKalendarzyViewInfo+Params.Okres}" />
    <Field CaptionHtml="Stan" Width="18" EditValue="{KadryDokAktKalendarzyViewInfo+Params.Stan}" />
  </Flow>
  <Grid Name="List">
$pola
  </Grid>
</DataForm>
"@

$tr = $cn.BeginTransaction()
try {
    $c = $cn.CreateCommand(); $c.Transaction = $tr
    function Skalar([string]$sql, [hashtable]$p) {
        $c.Parameters.Clear(); $c.CommandText = $sql
        foreach ($k in $p.Keys) { $c.Parameters.AddWithValue($k, $p[$k]) | Out-Null }
        return $c.ExecuteScalar()
    }
    $storages = Skalar "select ID from CfgNodes where Name = 'Storages' and Parent = 2" @{}
    $tableviews = Skalar "select ID from CfgNodes where Name = 'tableviews' and Parent = @p" @{ '@p' = $storages }
    if (-not $tableviews) { throw "Brak węzła tableviews" }

    # Katalog 'foldery' - zakładany, jeśli w bazie nie ma jeszcze żadnego własnego widoku.
    $foldery = Skalar "select ID from CfgNodes where Name = 'foldery' and Parent = @p" @{ '@p' = $storages }
    if (-not $foldery) {
        $foldery = Skalar "insert into CfgNodes (Guid, Parent, Name, Type) values (newid(), @p, 'foldery', 0); select cast(scope_identity() as int)" @{ '@p' = $storages }
        "Założono węzeł foldery: $foldery"
    }

    $wezelFolderow = Skalar "select ID from CfgNodes where Name = @n and Parent = @p" @{ '@n' = $plikFolderow; '@p' = $foldery }
    $atrybutFolderow = $null
    if ($wezelFolderow) {
        $atrybutFolderow = Skalar "select ID from CfgAttributes where Node = @n and isnull(Name,'') = ''" @{ '@n' = $wezelFolderow }
        [xml]$doc = TextFromHex (Skalar "select cast(MemoValue as nvarchar(max)) from CfgAttributes where ID = @id" @{ '@id' = $atrybutFolderow })
    } else {
        $wezelFolderow = Skalar "insert into CfgNodes (Guid, Parent, Name, Type) values (newid(), @p, @n, 1); select cast(scope_identity() as int)" @{ '@p' = $foldery; '@n' = $plikFolderow }
        [xml]$doc = '<?xml version="1.0" encoding="utf-8"?><FoldersInfo xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"><Infos /></FoldersInfo>'
    }
    # Widok już jest - podmiana samej treści viewform.xml (kolumny), foldery bez zmian.
    $istniejacy = $doc.FoldersInfo.Infos.Info | Where-Object { $_.Name -eq $nazwaWidoku }
    if ($istniejacy) {
        $g = $istniejacy.ID.Substring($nazwaWidoku.Length + 1)
        $wezelWidoku = Skalar "select ID from CfgNodes where Name = @n and Parent = @p" @{ '@n' = (NazwaWezla "$g.viewform.xml"); '@p' = $tableviews }
        if (-not $wezelWidoku) { throw "Brak węzła viewform widoku '$nazwaWidoku'" }
        $c.Parameters.Clear()
        $c.CommandText = "update CfgAttributes set MemoValue = @m where Node = @node and isnull(Name,'') = ''"
        $c.Parameters.AddWithValue('@node', $wezelWidoku) | Out-Null
        $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom $viewform)
        if ($c.ExecuteNonQuery() -ne 1) { throw "Nie zaktualizowano treści widoku (węzeł $wezelWidoku)" }
        $tr.Commit()
        "Widok '$nazwaWidoku' zaktualizowany: tableviews $wezelWidoku"
        return
    }

    $g = [Guid]::NewGuid().ToString('N')
    $info = $doc.CreateElement('Info')
    foreach ($p in @(@('Name',$nazwaWidoku), @('ID',"$nazwaWidoku.$g"), @('ViewType','Soneta.Business.Forms.Tables.TableView,Soneta.Business.Forms'), @('Icon','List'), @('SnapshotGuid','00000000-0000-0000-0000-000000000000'), @('Licenses','(pusty)'))) {
        $e = $doc.CreateElement($p[0]); $e.InnerText = $p[1]; $info.AppendChild($e) | Out-Null
    }
    $doc.SelectSingleNode('/FoldersInfo/Infos').AppendChild($info) | Out-Null

    $wezelWidoku = Skalar "insert into CfgNodes (Guid, Parent, Name, Type) values (newid(), @p, @n, 1); select cast(scope_identity() as int)" @{ '@p' = $tableviews; '@n' = (NazwaWezla "$g.viewform.xml") }
    $c.Parameters.Clear()
    $c.CommandText = "insert into CfgAttributes (Node, Name, Type, MemoValue) values (@node, '', 15, @m)"
    $c.Parameters.AddWithValue('@node', $wezelWidoku) | Out-Null
    $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom $viewform)
    $c.ExecuteNonQuery() | Out-Null

    $sw = New-Object IO.StringWriter; $xs = New-Object Xml.XmlWriterSettings; $xs.Indent = $true
    $xw = [Xml.XmlWriter]::Create($sw, $xs); $doc.Save($xw); $xw.Close()
    $folderyXml = $sw.ToString() -replace 'encoding="utf-16"', 'encoding="utf-8"'
    $c.Parameters.Clear()
    if ($atrybutFolderow) {
        $c.CommandText = "update CfgAttributes set MemoValue = @m where ID = @id"
        $c.Parameters.AddWithValue('@id', $atrybutFolderow) | Out-Null
    } else {
        $c.CommandText = "insert into CfgAttributes (Node, Name, Type, MemoValue) values (@id, '', 15, @m)"
        $c.Parameters.AddWithValue('@id', $wezelFolderow) | Out-Null
    }
    $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom $folderyXml)
    $c.ExecuteNonQuery() | Out-Null
    $tr.Commit()
    "Widok '$nazwaWidoku' zapisany: foldery $wezelFolderow ($plikFolderow), tableviews $wezelWidoku"
} catch { $tr.Rollback(); throw }
finally { $cn.Close() }
