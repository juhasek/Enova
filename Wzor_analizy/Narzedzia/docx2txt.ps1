param([string]$Path, [string]$Out)
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Xml.Linq
$zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
try {
  $e = $zip.GetEntry('word/document.xml')
  $s = $e.Open()
  $doc = [System.Xml.Linq.XDocument]::Load($s)
  $s.Close()
} finally { $zip.Dispose() }
$w = [System.Xml.Linq.XNamespace]::Get('http://schemas.openxmlformats.org/wordprocessingml/2006/main')
$sb = New-Object System.Text.StringBuilder
function ParaText($p) {
  $t = ''
  foreach ($n in $p.Descendants()) {
    if ($n.Name -eq $w + 't') { $t += $n.Value }
    elseif ($n.Name -eq $w + 'tab') { $t += "`t" }
    elseif ($n.Name -eq $w + 'br') { $t += ' / ' }
  }
  $st = $p.Element($w + 'pPr')
  if ($st) { $ps = $st.Element($w + 'pStyle'); if ($ps) { $sv = $ps.Attribute($w + 'val').Value; if ($sv -match 'Nag|Heading') { $t = "## [$sv] " + $t } } }
  return $t
}
$body = $doc.Root.Element($w + 'body')
foreach ($el in $body.Elements()) {
  if ($el.Name -eq $w + 'p') { $t = ParaText $el; if ($t.Trim()) { [void]$sb.AppendLine($t) } }
  elseif ($el.Name -eq $w + 'tbl') {
    $ti = 0
    [void]$sb.AppendLine('<<TABELA>>')
    foreach ($tr in $el.Elements($w + 'tr')) {
      $cells = @()
      foreach ($tc in $tr.Elements($w + 'tc')) {
        $ct = (($tc.Elements($w + 'p') | ForEach-Object { ParaText $_ }) -join ' / ')
        $cells += $ct
      }
      [void]$sb.AppendLine('| ' + ($cells -join ' | ') + ' |')
    }
    [void]$sb.AppendLine('<</TABELA>>')
  }
}
[System.IO.File]::WriteAllText($Out, $sb.ToString(), [System.Text.Encoding]::UTF8)
