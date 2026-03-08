#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Installe MecanoERP sur ce PC.
    A executer depuis le dossier extrait du ZIP, en tant qu'Administrateur.
#>

$AppName    = "MecanoERP"
$Publisher  = "GISEBS"
$Version    = "1.0.0"
$AppExe     = "MecanoERP.WPF.exe"
$InstallDir = Join-Path $env:ProgramFiles $AppName
$ScriptDir  = $PSScriptRoot

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Installation de MecanoERP v$Version          " -ForegroundColor Cyan
Write-Host "  Copyright 2026 $Publisher                    " -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Dossier d'installation : $InstallDir" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Confirmer l'installation ? (O/N)"
if ($confirm -notmatch "^[Oo]$") { Write-Host "Installation annulee."; exit 0 }

# -- Copier les fichiers ------------------------------------------------------
Write-Host "> Copie des fichiers vers $InstallDir..." -ForegroundColor Cyan
if (Test-Path $InstallDir) { Remove-Item $InstallDir -Recurse -Force }
New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null

Get-ChildItem $ScriptDir -Exclude "Install-MecanoERP.ps1" |
    Copy-Item -Destination $InstallDir -Recurse -Force

Write-Host "[OK] Fichiers copies." -ForegroundColor Green

# -- Raccourci Bureau ---------------------------------------------------------
$desktop = [Environment]::GetFolderPath("CommonDesktopDirectory")
$wsh     = New-Object -ComObject WScript.Shell
$lnk     = $wsh.CreateShortcut((Join-Path $desktop "$AppName.lnk"))
$lnk.TargetPath       = Join-Path $InstallDir $AppExe
$lnk.WorkingDirectory = $InstallDir
$lnk.Description      = "$AppName - Systeme de gestion de garage"
$lnk.Save()
Write-Host "[OK] Raccourci bureau cree." -ForegroundColor Green

# -- Raccourci Menu Demarrer --------------------------------------------------
$startMenu = Join-Path ([Environment]::GetFolderPath("CommonStartMenu")) "Programs\$AppName"
New-Item -ItemType Directory -Path $startMenu -Force | Out-Null

$lnk2 = $wsh.CreateShortcut((Join-Path $startMenu "$AppName.lnk"))
$lnk2.TargetPath       = Join-Path $InstallDir $AppExe
$lnk2.WorkingDirectory = $InstallDir
$lnk2.Description      = "$AppName - Systeme de gestion de garage"
$lnk2.Save()
Write-Host "[OK] Raccourci Menu Demarrer cree." -ForegroundColor Green

# -- Desinstalleur (raccourci vers un script) ---------------------------------
$uninstallScript = Join-Path $InstallDir "Uninstall-MecanoERP.ps1"
@"
#Requires -RunAsAdministrator
`$dir = "$InstallDir"
`$desk = [Environment]::GetFolderPath('CommonDesktopDirectory')
`$start = Join-Path ([Environment]::GetFolderPath('CommonStartMenu')) 'Programs\$AppName'
Remove-Item (Join-Path `$desk '$AppName.lnk') -Force -ErrorAction SilentlyContinue
Remove-Item `$start -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item 'HKLM:\SOFTWARE\$Publisher\$AppName' -Recurse -Force -ErrorAction SilentlyContinue
Start-Sleep 1
Remove-Item `$dir -Recurse -Force
Write-Host '$AppName desinstalle avec succes.'
"@ | Set-Content $uninstallScript -Encoding UTF8

# -- Registre Windows (Ajout/Suppression de programmes) -----------------------
$regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$AppName"
New-Item -Path $regPath -Force | Out-Null
Set-ItemProperty $regPath -Name "DisplayName"          -Value $AppName
Set-ItemProperty $regPath -Name "DisplayVersion"       -Value $Version
Set-ItemProperty $regPath -Name "Publisher"            -Value $Publisher
Set-ItemProperty $regPath -Name "InstallLocation"      -Value $InstallDir
Set-ItemProperty $regPath -Name "DisplayIcon"          -Value (Join-Path $InstallDir $AppExe)
Set-ItemProperty $regPath -Name "UninstallString"      -Value "powershell -ExecutionPolicy Bypass -File `"$uninstallScript`""
Set-ItemProperty $regPath -Name "NoModify"             -Value 1 -Type DWord
Set-ItemProperty $regPath -Name "NoRepair"             -Value 1 -Type DWord
Write-Host "[OK] Entree registre creee (Ajout/Suppression de programmes)." -ForegroundColor Green

# -- Fin ----------------------------------------------------------------------
Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  MecanoERP installe avec succes!               " -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

$launch = Read-Host "Lancer MecanoERP maintenant ? (O/N)"
if ($launch -match "^[Oo]$") {
    Start-Process (Join-Path $InstallDir $AppExe)
}
