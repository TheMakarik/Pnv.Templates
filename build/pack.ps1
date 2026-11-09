#!/usr/bin/env pwsh
<#
.SYNOPSIS
Собирает NuGet-пакет шаблонов Pnv.Templates.

.EXAMPLE
pwsh build/pack.ps1                 # собрать пакет (версия из version.txt)
pwsh build/pack.ps1 -Install        # собрать и установить
pwsh build/pack.ps1 -Version 2026.0.0
#>
[CmdletBinding()]
param(
    # Версия пакета. По умолчанию берётся из version.txt в корне репозитория.
    [string]$Version,

    # Установить пакет после сборки (dotnet new install).
    [switch]$Install
)

$ErrorActionPreference = 'Stop'

$Root        = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$TemplateSrc = Join-Path $Root 'template-src'
$Stage       = Join-Path $Root 'build' 'stage'
$Content     = Join-Path $Stage 'content'
$Artifacts   = Join-Path $Root 'artifacts'

$PackageId = 'Pnv.Templates'

if (-not $Version) {
    $versionFile = Join-Path $Root 'version.txt'
    if (-not (Test-Path $versionFile)) { throw "Нет файла $versionFile — укажите -Version явно" }
    $Version = (Get-Content $versionFile -Raw).Trim()
}

$Nuspec = @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd">
  <metadata>
    <id>$PackageId</id>
    <version>$Version</version>
    <authors>PNV</authors>
    <description>Шаблоны .NET для проектов ПНВ: десктопное приложение на AvaloniaUI и веб-приложение на ASP.NET Core Minimal API.</description>
    <packageTypes>
      <packageType name="Template" />
    </packageTypes>
  </metadata>
</package>
"@

function Copy-Tree {
    # Рекурсивное слияние каталога-источника в цель; -Force захватывает скрытые файлы (.gitignore и т.п.)
    param([string]$Source, [string]$Target)
    if (-not (Test-Path $Source)) { throw "Нет каталога: $Source" }
    New-Item -ItemType Directory -Force -Path $Target | Out-Null
    Get-ChildItem -LiteralPath $Source -Force | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $Target -Recurse -Force
    }
}

function Reset-Dir([string]$Path) {
    if (Test-Path $Path) { Remove-Item -Recurse -Force $Path }
    New-Item -ItemType Directory -Force -Path $Path | Out-Null
}

function Get-SkillFiles([string]$RootDir) {
    if (-not (Test-Path $RootDir)) { throw "Не найдены скиллы в $RootDir" }
    $files = @(Get-ChildItem -Path $RootDir -Directory | Sort-Object Name |
        ForEach-Object { Join-Path $_.FullName 'SKILL.md' } | Where-Object { Test-Path $_ })
    if (-not $files.Count) { throw "Не найдены скиллы в $RootDir" }
    return , $files
}

function Copy-SkillSet([string]$TargetRoot, [object[]]$Files) {
    Reset-Dir $TargetRoot
    foreach ($file in $Files) {
        $skillId = Split-Path -Path (Split-Path -Path $file -Parent) -Leaf
        $dest = Join-Path $TargetRoot $skillId
        New-Item -ItemType Directory -Force -Path $dest | Out-Null
        Copy-Item -LiteralPath $file -Destination (Join-Path $dest 'SKILL.md') -Force
    }
}

# --- staging ---

Reset-Dir $Stage

Copy-Tree (Join-Path $TemplateSrc 'desktop')         (Join-Path $Content 'desktop')
Copy-Tree (Join-Path $TemplateSrc 'desktop-content') (Join-Path $Content 'desktop-content')
Copy-Tree (Join-Path $TemplateSrc 'web')             (Join-Path $Content 'web')
Copy-Tree (Join-Path $TemplateSrc 'web-content')     (Join-Path $Content 'web-content')

$skillFiles    = Get-SkillFiles (Join-Path $TemplateSrc 'skills')
$webSkillFiles = Get-SkillFiles (Join-Path $TemplateSrc 'web-skills')

# Десктопные скиллы: Skills/ и зеркало .claude/skills/
foreach ($name in 'skills', 'claude-skills') {
    Copy-SkillSet (Join-Path $Content 'desktop-content' $name) $skillFiles
}

# Веб-скиллы
foreach ($name in 'skills', 'claude-skills') {
    Copy-SkillSet (Join-Path $Content 'web-content' $name) $webSkillFiles
}

# Комбо: скиллы = десктопные + веб-специфичные (общие id берутся из десктопа) + комбо-переопределения.
$comboTemplate = Join-Path $TemplateSrc 'combo'
if (Test-Path $comboTemplate) {
    Copy-Tree $comboTemplate (Join-Path $Content 'combo')
    Copy-Tree (Join-Path $TemplateSrc 'combo-content') (Join-Path $Content 'combo-content')

    $webShared = @('architecture', 'logging', 'options', 'testing')
    $webOnly = @($webSkillFiles | Where-Object {
        $webShared -notcontains (Split-Path -Path (Split-Path -Path $_ -Parent) -Leaf)
    })
    $comboSkillsDir = Join-Path $TemplateSrc 'combo-skills'
    $comboOverrides = @(if (Test-Path $comboSkillsDir) { Get-SkillFiles $comboSkillsDir } else { @() })

    foreach ($name in 'skills', 'claude-skills') {
        Copy-SkillSet (Join-Path $Content 'combo-content' $name) ($skillFiles + $webOnly + $comboOverrides)
    }
    Write-Host 'Комбо: скиллы собраны (десктоп + веб-специфичные + комбо-переопределения)'
}

Write-Host "Скиллов: $($skillFiles.Count) + $($webSkillFiles.Count) веб-скиллов"

# --- сборка пакета ---

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.ZipFile

New-Item -ItemType Directory -Force -Path $Artifacts | Out-Null
$packagePath = Join-Path $Artifacts "$PackageId.$Version.nupkg"
if (Test-Path $packagePath) { Remove-Item $packagePath -Force }

$nuspecPath = Join-Path $Stage "$PackageId.nuspec"
Set-Content -LiteralPath $nuspecPath -Value $Nuspec -Encoding utf8

$zip = [System.IO.Compression.ZipFile]::Open($packagePath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $nuspecPath, "$PackageId.nuspec", 'Optimal')
    Get-ChildItem -Path $Content -Recurse -File -Force | Sort-Object FullName | ForEach-Object {
        $entry = [System.IO.Path]::GetRelativePath($Stage, $_.FullName).Replace('\', '/')
        [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $entry, 'Optimal')
    }
}
finally {
    $zip.Dispose()
}

Write-Host "Пакет: $packagePath ($((Get-Item $packagePath).Length) байт)"

# --- установка ---

if ($Install) {
    # Пакет мог быть не установлен — код возврата удаления не важен.
    dotnet new uninstall $PackageId 2>$null | Out-Null
    dotnet new install $packagePath
    if ($LASTEXITCODE -ne 0) { exit 1 }
}
