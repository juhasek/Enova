# Uruchomienie generatora wzorów analizy KPCP na dowolnym środowisku.
#   .\uruchom.ps1            – buduje wszystkie wzory .docx z ..\Tresc\*.txt
#   .\uruchom.ps1 sprawdz    – tylko kontrola gotowych .docx (style, nagłówki, zakazane słowa)
# Wymaga .NET 8 lub nowszego: z SDK program jest kompilowany ze źródeł, z samym runtime
# uruchamiany jest gotowy bin\GenWzor.dll z paczki. Uruchamiamy dll, nie exe (Defender blokuje nowe .exe).
param([string]$Tryb = "buduj")

$ErrorActionPreference = "Stop"
$gen = $PSScriptRoot
$root = Split-Path $gen -Parent
$dll = Join-Path $gen "bin\GenWzor.dll"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Brak polecenia dotnet – zainstaluj .NET 8 (lub nowszy) Runtime albo SDK."
}

if (dotnet --list-sdks) {
    dotnet build (Join-Path $gen "GenWzor.csproj") -c Release -o (Join-Path $gen "bin") --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "Kompilacja generatora nie powiodła się." }
}
elseif (-not (Test-Path $dll)) {
    throw "Brak SDK .NET i brak gotowego $dll – skopiuj katalog bin z paczki albo zainstaluj SDK."
}

if (-not (Test-Path (Join-Path $root "_robocze\zakazane_slowa.txt"))) {
    Write-Warning "Brak _robocze\zakazane_slowa.txt – kontrola nie wychwyci nazw klientów we wzorach."
}

dotnet $dll $root $Tryb
exit $LASTEXITCODE
