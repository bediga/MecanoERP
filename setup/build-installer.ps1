#Requires -Version 5.1
<#
.SYNOPSIS
    Build et package MecanoERP sans outil tiers.
    Genere : publish/ + Install-MecanoERP.ps1 + MecanoERP_Package.zip
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root       = Split-Path -Parent $PSScriptRoot
$Project    = Join-Path $Root "src\MecanoERP.WPF\MecanoERP.WPF.csproj"
$PublishDir = Join-Path $Root "publish"
$SetupDir   = Join-Path $Root "setup"
$OutDir     = Join-Path $SetupDir "output"

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  MecanoERP - Build Package v1.0.0             " -ForegroundColor Cyan
Write-Host "  Copyright 2026 GISEBS                        " -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# -- Nettoyage ----------------------------------------------------------------
Write-Host "> Nettoyage..." -ForegroundColor Cyan
if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }
if (Test-Path $OutDir)     { Remove-Item $OutDir     -Recurse -Force }
New-Item -ItemType Directory -Path $OutDir -Force | Out-Null

# -- Publication .NET ---------------------------------------------------------
Write-Host "> Publication self-contained win-x64..." -ForegroundColor Cyan

dotnet publish $Project `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $PublishDir `
    /nologo

if ($LASTEXITCODE -ne 0) { Write-Host "[ERREUR] Publication echouee." -ForegroundColor Red; exit 1 }
Write-Host "[OK] Publication reussie." -ForegroundColor Green

# -- Copier le script installateur dans le package ----------------------------
Copy-Item (Join-Path $SetupDir "Install-MecanoERP.ps1") -Destination $PublishDir

# -- Creer le ZIP -------------------------------------------------------------
Write-Host "> Creation du ZIP..." -ForegroundColor Cyan
$ZipPath = Join-Path $OutDir "MecanoERP_v1.0.0_win-x64.zip"
Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipPath -Force

$size = [math]::Round((Get-Item $ZipPath).Length / 1MB, 1)
Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  PACKAGE CREE AVEC SUCCES!                     " -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Fichier : $ZipPath" -ForegroundColor White
Write-Host "  Taille  : $size MB"  -ForegroundColor White
Write-Host ""
Write-Host "  Pour installer : extraire le ZIP puis executer" -ForegroundColor Yellow
Write-Host "  Install-MecanoERP.ps1 en tant qu'administrateur." -ForegroundColor Yellow
Write-Host ""

$open = Read-Host "Ouvrir le dossier de sortie ? (O/N)"
if ($open -match "^[Oo]$") { Start-Process explorer.exe $OutDir }
