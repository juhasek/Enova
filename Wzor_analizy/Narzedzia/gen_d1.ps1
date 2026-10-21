param([string]$Src, [string]$Content, [string]$Out)
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Xml.Linq
$W = [System.Xml.Linq.XNamespace]::Get('http://schemas.openxmlformats.org/wordprocessingml/2006/main')
$XML = [System.Xml.Linq.XNamespace]::Xml

Copy-Item -LiteralPath $Src -Destination $Out -Force
$zip = [System.IO.Compression.ZipFile]::Open($Out, 'Update')
$entry = $zip.GetEntry('word/document.xml')
$s = $entry.Open(); $doc = [System.Xml.Linq.XDocument]::Load($s); $s.Close()
$body = $doc.Root.Element($W + 'body')
$els = @($body.Elements())
$sect = $els[$els.Count - 1]
$pHead = [System.Xml.Linq.XElement]::new($els[6])
$pSub = [System.Xml.Linq.XElement]::new($els[31])
$pNorm = [System.Xml.Linq.XElement]::new($els[32])
$pBul = [System.Xml.Linq.XElement]::new($els[34])
$tProto = [System.Xml.Linq.XElement]::new($els[139])
$pEmpty = [System.Xml.Linq.XElement]::new($els[4])
for ($i = 6; $i -lt $els.Count - 1; $i++) { $els[$i].Remove() }

function New-Run([string]$text, [bool]$bold) {
  $r = [System.Xml.Linq.XElement]::new($W + 'r')
  if ($bold) { $r.Add(([System.Xml.Linq.XElement]::new($W + 'rPr', ([System.Xml.Linq.XElement]::new($W + 'b')), ([System.Xml.Linq.XElement]::new($W + 'bCs'))))) }
  $t = [System.Xml.Linq.XElement]::new($W + 't', $text)
  $t.SetAttributeValue($XML + 'space', 'preserve')
  $r.Add($t); return $r
}
function Set-Text($p, [string]$text, [bool]$bold) {
  $p.Elements($W + 'r') | ForEach-Object { $_ } | ForEach-Object { $_.Remove() }
  $p.Elements($W + 'hyperlink') | ForEach-Object { $_ } | ForEach-Object { $_.Remove() }
  $parts = [regex]::Split($text, '\*\*')
  for ($k = 0; $k -lt $parts.Count; $k++) {
    if ($parts[$k] -ne '') { $p.Add((New-Run $parts[$k] ($bold -or ($k % 2 -eq 1)))) }
  }
}
function New-Para($proto, [string]$text, [bool]$bold) {
  $p = [System.Xml.Linq.XElement]::new($proto); Set-Text $p $text $bold; return $p
}

$rows = @(); $hdr = $null; $lines = [System.IO.File]::ReadAllLines($Content, [System.Text.Encoding]::UTF8)
$new = New-Object System.Collections.Generic.List[object]
foreach ($line in $lines) {
  if ($line.Trim() -eq '') { continue }
  $f = $line.Split('|')
  switch ($f[0]) {
    'H' { $new.Add((New-Para $pEmpty '' $false)); $new.Add((New-Para $pHead $f[1] $true)) }
    'S' { $new.Add((New-Para $pSub $f[1] $true)) }
    'P' { $new.Add((New-Para $pNorm ($f[1..($f.Count-1)] -join '|') $false)) }
    'B' { $new.Add((New-Para $pBul $f[1] $false)) }
    'T' { $hdr = $f[1..($f.Count-1)]; $rows = @() }
    'R' { $rows += ,($f[1..($f.Count-1)]) }
    'E' {
      $tbl = [System.Xml.Linq.XElement]::new($tProto)
      $protoRows = @($tbl.Elements($W + 'tr'))
      $hRow = [System.Xml.Linq.XElement]::new($protoRows[0]); $dRow = [System.Xml.Linq.XElement]::new($protoRows[1])
      $protoRows | ForEach-Object { $_.Remove() }
      $n = $hdr.Count
      $total = 9000
      $widths = @()
      if ($hdr[0] -match '^ID') { $first = 1300 } elseif ($n -eq 3) { $first = 3000 } else { $first = [int]($total / $n) }
      $widths += $first
      $rest = [int](($total - $first) / [Math]::Max(1, $n - 1))
      for ($c = 1; $c -lt $n; $c++) { $widths += $rest }
      if ($n -eq 5) { $widths = @(1100, 3600, 1300, 1200, 1800) }
      $grid = $tbl.Element($W + 'tblGrid'); $grid.RemoveNodes()
      foreach ($wd in $widths) { $gc = [System.Xml.Linq.XElement]::new($W + 'gridCol'); $gc.SetAttributeValue($W + 'w', $wd); $grid.Add($gc) }
      $mk = {
        param($rowProto, $cells, $bold)
        $r = [System.Xml.Linq.XElement]::new($rowProto)
        $tcs = @($r.Elements($W + 'tc')); $tcProto = [System.Xml.Linq.XElement]::new($tcs[0]); $tcs | ForEach-Object { $_.Remove() }
        for ($c = 0; $c -lt $n; $c++) {
          $tc = [System.Xml.Linq.XElement]::new($tcProto)
          $tcw = $tc.Element($W + 'tcPr').Element($W + 'tcW'); $tcw.SetAttributeValue($W + 'w', $widths[$c]); $tcw.SetAttributeValue($W + 'type', 'dxa')
          $p = $tc.Element($W + 'p'); $txt = ''; if ($c -lt $cells.Count) { $txt = $cells[$c].Trim() }
          Set-Text $p $txt $bold
          $r.Add($tc)
        }
        return $r
      }
      $tbl.Add((& $mk $hRow $hdr $true))
      foreach ($rw in $rows) { $tbl.Add((& $mk $dRow $rw $false)) }
      $new.Add($tbl); $new.Add((New-Para $pEmpty '' $false))
    }
  }
}
foreach ($e in $new) { $sect.AddBeforeSelf($e) }
$entry.Delete()
$ne = $zip.CreateEntry('word/document.xml')
$ws = $ne.Open()
$settings = New-Object System.Xml.XmlWriterSettings; $settings.Encoding = New-Object System.Text.UTF8Encoding($false)
$xw = [System.Xml.XmlWriter]::Create($ws, $settings); $doc.Save($xw); $xw.Close(); $ws.Close()
$zip.Dispose()
"OK $Out"
