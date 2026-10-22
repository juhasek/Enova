# Wspólna definicja widoku "Nadgodziny" — dot-source z dodaj-widok.ps1 i aktualizuj-widok.ps1.
$nazwaWidoku = 'Nadgodziny'
$kolumny = @(
    @('Kod', 'Pracownik.Kod', '9'),
    @('Imię i nazwisko', 'Pracownik.ImięNazwisko', '25'),
    @('Data pracy', 'WniosekData', '10'),
    @('Od godziny', 'WniosekElement.OdGodziny', '8'),
    @('Do godziny', 'DoGodziny', '8'),
    @('Uwagi', 'WniosekOpis', '40'),
    @('Przełożony 1', 'Przelozony1', '25'),
    @('Przełożony 2', 'Przelozony2', '25'),
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

function HexUtf8Bom([string]$t) {
    $b = [byte[]](0xEF,0xBB,0xBF) + [Text.Encoding]::UTF8.GetBytes($t)
    return ($b | ForEach-Object { $_.ToString('X2') }) -join ''
}
function TextFromHex([string]$h) {
    $b = New-Object byte[] ($h.Length/2); for ($i=0; $i -lt $b.Length; $i++) { $b[$i] = [Convert]::ToByte($h.Substring($i*2,2),16) }
    $s = [Text.Encoding]::UTF8.GetString($b); return $s.TrimStart([char]0xFEFF)
}
function Ostatnie40([string]$n) { if ($n.Length -gt 40) { return $n.Substring($n.Length - 40) } else { return $n } }
