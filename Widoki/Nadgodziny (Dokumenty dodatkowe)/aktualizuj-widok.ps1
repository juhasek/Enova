$ErrorActionPreference = 'Stop'
# Nadpisuje kolumny istniejącego widoku "Nadgodziny" definicją z widok-definicja.ps1 (baza Al).
# Po zapisie zrestartować proces server.dll.
$baza = 'Al'
$cs = "Server=localhost\SQLEXPRESS;Database=$baza;Integrated Security=true;TrustServerCertificate=true"
$cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()

. (Join-Path $PSScriptRoot 'widok-definicja.ps1')

# --- kopia bazy ---
$dzis = Get-Date -Format 'yyyyMMdd'
$bk = $cn.CreateCommand(); $bk.CommandTimeout = 600
$bk.CommandText = "BACKUP DATABASE [$baza] TO DISK = N'${baza}_przed_aktualizacja_widoku_nadgodziny_$dzis.bak' WITH INIT, COPY_ONLY"
$bk.ExecuteNonQuery() | Out-Null
"Kopia bazy: ${baza}_przed_aktualizacja_widoku_nadgodziny_$dzis.bak"

try {
    $c = $cn.CreateCommand()
    $c.CommandText = "select cast(a.MemoValue as nvarchar(max)) from CfgNodes n join CfgAttributes a on a.Node = n.ID and isnull(a.Name,'') = ''
        where n.Name = @n and n.Parent = (select ID from CfgNodes where Name = 'foldery' and Parent = (select ID from CfgNodes where Name = 'Storages'))"
    $c.Parameters.AddWithValue('@n', (Ostatnie40 'kadry i płace.kadry.ewidencje.dokumenty dodatkowe.xml')) | Out-Null
    $hex = $c.ExecuteScalar()
    if (-not $hex) { throw 'Brak węzła folderu Dokumenty dodatkowe' }
    [xml]$doc = TextFromHex $hex
    $info = $doc.SelectNodes('/FoldersInfo/Infos/Info') | Where-Object { $_.Name -eq $nazwaWidoku }
    if (-not $info) { throw "Brak widoku '$nazwaWidoku' — użyj dodaj-widok.ps1" }
    $g = $info.ID.Substring($nazwaWidoku.Length + 1)

    $c.Parameters.Clear()
    $c.CommandText = "update a set MemoValue = @m from CfgAttributes a join CfgNodes n on n.ID = a.Node and isnull(a.Name,'') = ''
        where n.Name = @n and n.Parent = (select ID from CfgNodes where Name = 'tableviews' and Parent = (select ID from CfgNodes where Name = 'Storages'))"
    $c.Parameters.Add('@m', [Data.SqlDbType]::NText).Value = (HexUtf8Bom $viewform)
    $c.Parameters.AddWithValue('@n', (Ostatnie40 "$g.viewform.xml")) | Out-Null
    $ile = $c.ExecuteNonQuery()
    if ($ile -ne 1) { throw "Zaktualizowano $ile wierszy zamiast 1" }
    "Widok '$nazwaWidoku' zaktualizowany: $($kolumny.Count) kolumn."
}
finally { $cn.Close() }
