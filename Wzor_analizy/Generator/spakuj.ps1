# Paczka generatora wzorów analizy KPCP do przeniesienia na inne środowisko.
#   .\spakuj.ps1                          – zip w Pobranych (bez listy zakazanych słów)
#   .\spakuj.ps1 -Cel D:\paczka.zip       – własna ścieżka
#   .\spakuj.ps1 -ZZakazanymiSlowami      – dołącza _robocze\zakazane_slowa.txt (nazwy klientów – tylko na zaufane środowisko)
# Zawartość: Generator (źródła, baza, obrazy, skompilowany bin), Tresc, STYLE_GUIDE.md.
# Na docelowym środowisku: rozpakuj, potem Wzor_analizy\Generator\uruchom.ps1.
param(
    [string]$Cel = (Join-Path $HOME "Downloads\Generator_wzorow_KPCP.zip"),
    [switch]$ZZakazanymiSlowami
)

$ErrorActionPreference = "Stop"
$gen = $PSScriptRoot
$root = Split-Path $gen -Parent

dotnet build (Join-Path $gen "GenWzor.csproj") -c Release -o (Join-Path $gen "bin") --nologo -v q
if ($LASTEXITCODE -ne 0) { throw "Kompilacja generatora nie powiodła się." }

$tmp = Join-Path ([IO.Path]::GetTempPath()) ("GenWzor_" + [guid]::NewGuid().ToString("N"))
$dst = Join-Path $tmp "Wzor_analizy"
try {
    New-Item -ItemType Directory -Force (Join-Path $dst "Generator\bin"), (Join-Path $dst "_robocze") | Out-Null
    Copy-Item (Join-Path $gen "GenWzor.csproj"), (Join-Path $gen "Program.cs"), (Join-Path $gen "uruchom.ps1"), (Join-Path $gen "spakuj.ps1") (Join-Path $dst "Generator")
    Copy-Item (Join-Path $gen "baza"), (Join-Path $gen "obrazy") (Join-Path $dst "Generator") -Recurse
    Copy-Item (Join-Path $gen "bin\GenWzor.*") (Join-Path $dst "Generator\bin")
    Copy-Item (Join-Path $root "Tresc") $dst -Recurse
    Copy-Item (Join-Path $root "STYLE_GUIDE.md") $dst
    if ($ZZakazanymiSlowami) { Copy-Item (Join-Path $root "_robocze\zakazane_slowa.txt") (Join-Path $dst "_robocze") }

    if (Test-Path $Cel) { Remove-Item $Cel -Force }
    Compress-Archive -Path $dst -DestinationPath $Cel
    Write-Host "Paczka: $Cel"
}
finally { Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue }
