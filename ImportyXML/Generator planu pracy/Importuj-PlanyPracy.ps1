<#
.SYNOPSIS
    Dokonczenie importu planu pracy wygenerowanego makrem GenerujPlanyPracy (Excel/VBA).

.OPIS
    Excel/VBA (modGeneratorPlanuPracy.bas) NIE laczy sie z SQL - generuje pliki XML z dniami
    planu, w ktorych KalendarzBase ma placeholder guid="KOD:<Kod pracownika>" zamiast
    prawdziwego GUID-u kalendarza indywidualnego pracownika.

    Ten skrypt (uruchamiany POZA Excelem, np. przez osobe wykonujaca import) dla kazdego
    znalezionego placeholdera "KOD:xxx":
      1. laczy sie z baza SQL enova (Windows Auth) i odczytuje GUID kalendarza indywidualnego
         (Kalendarze.Typ=2) pracownika o danym Kodzie - bez znaczenia, jaki konkretnie kalendarz
         ma ten pracownik (Standard, wlasny wzorcowy, ruchomy itd.) - liczy sie tylko to, ze ma
         juz zalozony Etat (indywidualny kalendarz powstaje wtedy automatycznie);
      2. podmienia placeholder na prawdziwy GUID w kopii pliku (<nazwa>.gotowy.xml);
      3. opcjonalnie (-Importuj) od razu wywoluje dbmgr importxml.

    Pracownik bez zalozonego kalendarza w bazie (Kod nieznaleziony) powoduje pominiecie
    CALEGO pliku dla tego pracownika, z ostrzezeniem - reszta plikow importuje sie normalnie.

.PARAMETR Folder
    Folder z plikami *.xml wygenerowanymi przez makro (domyslnie folder tego skryptu).

.PARAMETR SqlServer
    Adres serwera SQL bazy enova (Windows Auth), np. localhost\SQLEXPRESS.

.PARAMETR Baza
    Nazwa bazy enova.

.PARAMETR DbmgrPath
    Sciezka do dbmgr.exe - potrzebna tylko z przelacznikiem -Importuj.

.PARAMETR Importuj
    Gdy podane, po podmianie placeholderow od razu wywoluje "dbmgr importxml <Baza> <plik> --standard"
    dla kazdego gotowego pliku. Bez tego przelacznika skrypt tylko przygotowuje pliki *.gotowy.xml
    do recznego zaimportowania.

.PRZYKLAD
    .\Importuj-PlanyPracy.ps1 -SqlServer "localhost\SQLEXPRESS" -Baza "Claude" -Importuj
#>

param(
    [string]$Folder = $PSScriptRoot,
    [string]$SqlServer = "localhost\SQLEXPRESS",
    [string]$Baza = "Claude",
    [string]$DbmgrPath = "C:\enovaServer\2512.5.6\Soneta.Products.Server.Standard\dbmgr.exe",
    [switch]$Importuj
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName "System.Data" | Out-Null

$connString = "Server=$SqlServer;Database=$Baza;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $conn.Open()
} catch {
    Write-Error "Nie udalo sie polaczyc z baza '$Baza' na serwerze '$SqlServer': $($_.Exception.Message)"
    exit 1
}

function Resolve-KalendarzGuid {
    param([string]$Kod)
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT k.Guid FROM Kalendarze k INNER JOIN Pracownicy p ON k.Pracownik = p.ID WHERE p.Kod = @kod AND k.Typ = 2"
    $null = $cmd.Parameters.AddWithValue("@kod", $Kod)
    $result = $cmd.ExecuteScalar()
    if ($null -eq $result -or $result -eq [System.DBNull]::Value) {
        return $null
    }
    return [string]$result
}

$pliki = Get-ChildItem -Path $Folder -Filter "*.xml" -File |
    Where-Object { $_.Name -notmatch '\.gotowy\.xml$' } |
    Where-Object { (Get-Content $_.FullName -Raw -Encoding UTF8) -match 'guid="KOD:' }

if (-not $pliki -or $pliki.Count -eq 0) {
    Write-Host "Brak plikow z placeholderem guid=`"KOD:...`" w folderze '$Folder'."
    $conn.Close()
    exit 0
}

$guidCache = @{}
$jakiekolwiekBledy = $false

foreach ($plik in $pliki) {
    $tresc = Get-Content $plik.FullName -Raw -Encoding UTF8
    $brakujaceKody = New-Object System.Collections.Generic.List[string]

    $tresc2 = [regex]::Replace($tresc, 'guid="KOD:([^"]+)"', {
        param($m)
        $kod = $m.Groups[1].Value
        if (-not $guidCache.ContainsKey($kod)) {
            $guidCache[$kod] = Resolve-KalendarzGuid -Kod $kod
        }
        $guid = $guidCache[$kod]
        if (-not $guid) {
            $brakujaceKody.Add($kod)
            return $m.Value
        }
        return "guid=`"$guid`""
    })

    if ($brakujaceKody.Count -gt 0) {
        Write-Warning "Plik '$($plik.Name)': w bazie '$Baza' brak pracownika/kalendarza dla kodu: $($brakujaceKody -join ', ') (pracownik musi juz istniec z zalozonym Etatem). Plik POMINIETY."
        $jakiekolwiekBledy = $true
        continue
    }

    $docelowy = Join-Path $plik.DirectoryName ($plik.BaseName + ".gotowy.xml")
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($docelowy, $tresc2, $utf8NoBom)
    Write-Host "OK: $($plik.Name) -> $(Split-Path $docelowy -Leaf)"

    if ($Importuj) {
        if (-not (Test-Path $DbmgrPath)) {
            Write-Warning "Nie znaleziono dbmgr.exe pod '$DbmgrPath' - pomijam import pliku '$($docelowy)'. Popraw -DbmgrPath."
            $jakiekolwiekBledy = $true
            continue
        }
        Write-Host "Import: $Baza <- $(Split-Path $docelowy -Leaf)"
        & $DbmgrPath importxml $Baza $docelowy --standard
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "dbmgr importxml zakonczyl sie kodem $LASTEXITCODE dla pliku '$($docelowy)'."
            $jakiekolwiekBledy = $true
        }
    }
}

$conn.Close()

if ($jakiekolwiekBledy) {
    Write-Warning "Zakonczono z ostrzezeniami/bledami - patrz komunikaty wyzej."
    exit 1
} else {
    Write-Host "Zakonczono bez bledow."
    exit 0
}
