param([string]$Src, [string]$Content, [string]$Out, [string]$ClientName)
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Xml.Linq
$W = [System.Xml.Linq.XNamespace]::Get('http://schemas.openxmlformats.org/wordprocessingml/2006/main')
$XML = [System.Xml.Linq.XNamespace]::Xml
$PILCROW = [string][char]0x00B6
$PH = '[DO UZUPE' + [char]0x0141 + 'NIENIA: nazwa klienta]'

function TextOf($el) { (($el.Descendants($W + 't') | ForEach-Object { $_.Value }) -join '') }
function New-Run([string]$text) {
  $r = [System.Xml.Linq.XElement]::new($W + 'r')
  $t = [System.Xml.Linq.XElement]::new($W + 't', $text); $t.SetAttributeValue($XML + 'space', 'preserve')
  $r.Add($t); return $r
}
function Set-CellText($tc, [string]$text) {
  $ps = @($tc.Elements($W + 'p'))
  $proto = [System.Xml.Linq.XElement]::new($ps[0])
  $ps | ForEach-Object { $_.Remove() }
  foreach ($para in $text.Split($PILCROW)) {
    $p = [System.Xml.Linq.XElement]::new($W + 'p')
    $ppr = $proto.Element($W + 'pPr'); if ($ppr) { $p.Add([System.Xml.Linq.XElement]::new($ppr)) }
    if ($para -ne '') { $p.Add((New-Run $para)) }
    $tc.Add($p)
  }
}
function Process-Xml($doc) {
  # placeholder nazwy klienta w akapitach
  foreach ($p in @($doc.Descendants($W + 'p'))) {
    $t = TextOf $p
    if ($t.Contains($PH)) {
      $runs = @($p.Elements($W + 'r') | Where-Object { $_.Element($W + 't') })
      $first = $runs[0]
      $newText = $t.Replace($PH, $ClientName)
      foreach ($r in $runs) { if ($r -ne $first) { $r.Remove() } }
      $p.Elements($W + 'proofErr') | ForEach-Object { $_ } | ForEach-Object { $_.Remove() }
      $rpr = $first.Element($W + 'rPr')
      if ($rpr) { $rpr.Elements() | Where-Object { $_.Name.LocalName -eq 'highlight' -or ($_.Name.LocalName -eq 'rStyle' -and $_.Attribute($W + 'val').Value -eq 'Poledouzupenienia') } | ForEach-Object { $_ } | ForEach-Object { $_.Remove() } }
      $tt = $first.Element($W + 't'); $tt.Value = $newText; $tt.SetAttributeValue($XML + 'space', 'preserve')
      $script:log += "placeholder: $newText"
    }
  }
}

Copy-Item -LiteralPath $Src -Destination $Out -Force
$zip = [System.IO.Compression.ZipFile]::Open($Out, 'Update')
$script:log = @()
$lines = [System.IO.File]::ReadAllLines($Content, [System.Text.Encoding]::UTF8) | Where-Object { $_.Trim() -ne '' }

foreach ($name in @('word/document.xml', 'word/header1.xml', 'word/header2.xml', 'word/header3.xml', 'word/footer1.xml')) {
  $entry = $zip.GetEntry($name); if (-not $entry) { continue }
  $s = $entry.Open(); $doc = [System.Xml.Linq.XDocument]::Load($s); $s.Close()
  Process-Xml $doc
  if ($name -eq 'word/document.xml') {
    $rows = @($doc.Descendants($W + 'tr'))
    $used = @{}
    $lastInserted = @{}
    foreach ($line in $lines) {
      $f = $line.Split('|')
      switch ($f[0]) {
        'M' {
          $row = $rows | Where-Object { (TextOf (@($_.Elements($W + 'tc'))[0])).Trim() -eq $f[1] } | Select-Object -First 1
          if ($row) { Set-CellText (@($row.Elements($W + 'tc'))[1]) $f[2] } else { $script:log += "BRAK M: $($f[1])" }
        }
        'H' {
          $row = $rows | Where-Object { (TextOf (@($_.Elements($W + 'tc'))[0])).Trim() -eq $f[1] } | Select-Object -First 1
          if ($row) { $tcs = @($row.Elements($W + 'tc')); for ($c = 1; $c -lt 4; $c++) { Set-CellText $tcs[$c] $f[$c + 1] } ; $script:histRow = $row } else { $script:log += "BRAK H" }
        }
        'H+' {
          $nr = [System.Xml.Linq.XElement]::new($script:histRow); $tcs = @($nr.Elements($W + 'tc'))
          for ($c = 0; $c -lt 4; $c++) { Set-CellText $tcs[$c] $f[$c + 1] }
          $script:histRow.AddAfterSelf($nr); $script:histRow = $nr
        }
        'A' {
          $row = $rows | Where-Object { (TextOf (@($_.Elements($W + 'tc'))[0])).Trim().StartsWith($f[1]) } | Select-Object -First 1
          if ($row) {
            $tcs = @($row.Elements($W + 'tc')); Set-CellText $tcs[1] $f[2]; Set-CellText $tcs[3] $f[3]
            $used[$f[1]] = 1
          } else { $script:log += "BRAK A: $($f[1])" }
        }
        'N' {
          $anchor = $rows | Where-Object { (TextOf (@($_.Elements($W + 'tc'))[0])).Trim().StartsWith($f[1]) } | Select-Object -First 1
          if ($anchor) {
            $after = $anchor; if ($lastInserted.ContainsKey($f[1])) { $after = $lastInserted[$f[1]] }
            $nr = [System.Xml.Linq.XElement]::new($anchor); $tcs = @($nr.Elements($W + 'tc'))
            Set-CellText $tcs[0] $f[2]; Set-CellText $tcs[1] $f[3]; Set-CellText $tcs[2] $f[4]; Set-CellText $tcs[3] $f[5]
            $after.AddAfterSelf($nr); $lastInserted[$f[1]] = $nr
            $script:log += "dodano: $($f[2])"
          } else { $script:log += "BRAK N: $($f[1])" }
        }
      }
    }
  }
  $entry.Delete()
  $ne = $zip.CreateEntry($name); $ws = $ne.Open()
  $settings = [System.Xml.XmlWriterSettings]::new(); $settings.Encoding = [System.Text.UTF8Encoding]::new($false)
  $xw = [System.Xml.XmlWriter]::Create($ws, $settings); $doc.Save($xw); $xw.Close(); $ws.Close()
}
$zip.Dispose()
$script:log
"OK $Out"
