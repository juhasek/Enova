# Odświeża Excel ze słownikami rozwiązania umowy na podstawie DLL serwera enova i bazy.
# Przykład: .\Odswiez.ps1 -Wersja 2610 -Baza Al
param(
    [string]$Wersja = "2610",
    [string]$Baza = "Al",
    [string]$Serwer = "localhost\SQLEXPRESS",
    [string]$Wyjscie = (Join-Path $PSScriptRoot "..\Rozwiązanie umowy - kody zwolnienia i podstawy prawne.xlsx")
)
$ErrorActionPreference = "Stop"

$enovaDir = "C:\enovaServer\$Wersja\Soneta.Products.Server.Standard"
$robocze = Join-Path $env:TEMP "GenSlownikRozwiazaniaUmowy"
if (Test-Path $robocze) { Remove-Item -Recurse -Force $robocze -Confirm:$false }
New-Item -ItemType Directory -Force (Join-Path $robocze "enumy") | Out-Null

# 1. Dekompilacja typów wyliczeniowych do plików (na konsoli ilspycmd psuje polskie znaki).
$ilspy = Get-ChildItem "$env:USERPROFILE\.dotnet\tools\.store\ilspycmd" -Recurse -Filter ilspycmd.dll | Select-Object -First 1
if ($null -eq $ilspy) { throw "Brak ilspycmd (dotnet tool install -g ilspycmd --version 8.2.0.7535)" }
$env:DOTNET_ROLL_FORWARD = "Major"
foreach ($typ in "KodZwolnienia", "KodPodstawyPrawnejZwolnienia") {
    & dotnet $ilspy.FullName -t "Soneta.Kadry.$typ" "$enovaDir\Soneta.KadryPlace.dll" -r $enovaDir -o (Join-Path $robocze "enumy\$typ") | Out-Null
}

# 2. Słownik przyczyn z bazy do TSV w UTF-8 (sqlcmd psuje rzadsze polskie znaki).
$c = New-Object System.Data.SqlClient.SqlConnection "Server=$Serwer;Database=$Baza;Integrated Security=true"
$c.Open()
$cmd = $c.CreateCommand()
$cmd.CommandText = "SELECT ID, Nazwa, Blokada, ZaOdszkodowaniem, Typ, PodstawaPrawna, KodZwolnienia, Inicjatywa FROM PrzyczRozwUmow ORDER BY ID"
$r = $cmd.ExecuteReader()
$wiersze = New-Object System.Collections.Generic.List[string]
while ($r.Read()) { $wiersze.Add(((0..7 | ForEach-Object { $r.GetValue($_).ToString() }) -join "`t")) }
$c.Close()
[System.IO.File]::WriteAllLines((Join-Path $robocze "przyczyny.tsv"), $wiersze, (New-Object System.Text.UTF8Encoding $false))

# 3. Budowa i uruchomienie generatora (uruchamiać .dll, nie .exe).
& dotnet build $PSScriptRoot -c Release -o (Join-Path $PSScriptRoot "bin\out") | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Błąd kompilacji generatora" }
& dotnet (Join-Path $PSScriptRoot "bin\out\GenSlownik.dll") $robocze ([System.IO.Path]::GetFullPath($Wyjscie)) $Wersja
