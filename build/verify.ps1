# Матрица сборок/тестов десктопного шаблона; версия пакета — из version.txt.
[CmdletBinding()]
param(
    [string]$Version
)

$ErrorActionPreference = 'Stop'

$Root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$Out = Join-Path $Root 'artifacts' 'verify'

if (-not $Version) {
    $Version = (Get-Content (Join-Path $Root 'version.txt') -Raw).Trim()
}
$Package = Join-Path $Root 'artifacts' "Pnv.Templates.$Version.nupkg"

$AvaloniaVersions = '12.0.4', '12.0.5', '12.1.0', '12.1.1', '12.1.2', '12.1.3', '12.1.4', '12.1.5'

function Invoke-Tool {
    param([string[]]$ToolArgs, [string]$Cwd)
    Push-Location $Cwd
    try {
        $exe = $ToolArgs[0]
        $rest = @()
        if ($ToolArgs.Count -gt 1) { $rest = @($ToolArgs | Select-Object -Skip 1) }
        $output = (& $exe @rest 2>&1) | Out-String
    }
    finally {
        Pop-Location
    }
    if ($LASTEXITCODE -ne 0) {
        Write-Host "КОМАНДА: $($ToolArgs -join ' ') (cwd: $Cwd)"
        Write-Host $output
        exit 1
    }
    return $output
}

function Install-Package {
    # Пакет мог быть не установлен — код возврата удаления не важен.
    dotnet new uninstall Pnv.Templates 2>$null | Out-Null
    Invoke-Tool @('dotnet', 'new', 'install', $Package) $Root | Out-Null
}

function New-Project {
    param([string]$Name, [string[]]$Options)
    $target = Join-Path $Out $Name
    if (Test-Path $target) { Remove-Item -Recurse -Force $target }
    New-Item -ItemType Directory -Force -Path $target | Out-Null
    Invoke-Tool (@('dotnet', 'new', 'pnv-desktop', '-n', $Name, '-o', '.', '--no-restore') + @($Options)) $target | Out-Null
    return $target
}

function Build-Project([string]$Target) {
    Invoke-Tool @('dotnet', 'build', '--nologo', '-v', 'q') $Target | Out-Null
    Write-Host "  build ok: $(Split-Path $Target -Leaf)"
}

function Get-ExpectedSkills {
    param([string]$Di, [string]$Mvvm, [bool]$Tests, [bool]$UseEf, [bool]$UseSkills, [bool]$UseLogging)
    $set = [System.Collections.Generic.HashSet[string]]::new()
    if (-not $UseSkills) { return $set }

    'architecture', 'async', 'avalonia-colors', 'avalonia-converters',
    'avalonia-theming', 'clarifying-questions', 'code-sections', 'code-style',
    'collections', 'error-handling', 'json-sourcegen', 'models-organization',
    'paths', 'services-organization', 'trimming', 'view-organization', 'xaml',
    'app-settings' | ForEach-Object { [void]$set.Add($_) }

    if ($Di -ne 'None') {
        [void]$set.Add('options')
        if ($Di -eq 'HostingScrutor') { [void]$set.Add('dependency-injection-scrutor') }
        else { [void]$set.Add('dependency-injection') }
        if ($UseLogging) { [void]$set.Add('logging') }
    }
    if ($Mvvm -eq 'ReactiveUI') {
        'mvvm-reactiveui', 'reactiveui-interactions', 'reactiveui-sourcegenerators' |
            ForEach-Object { [void]$set.Add($_) }
    }
    else {
        [void]$set.Add('mvvm-communitytoolkit')
    }
    if ($UseEf) {
        'ef-core-sqlite', 'ef-core-interceptors', 'infrastructure-organization' |
            ForEach-Object { [void]$set.Add($_) }
    }
    if ($Tests) { [void]$set.Add('testing') }
    return $set
}

function Test-Generated {
    param(
        [string]$Target, [bool]$ExpectEf, [bool]$ExpectSplit,
        [string]$Di, [string]$Mvvm, [bool]$Tests,
        [bool]$UseSkills = $true, [bool]$UseLogging = $true
    )
    $name = Split-Path $Target -Leaf

    $leftovers = @(Get-ChildItem -Path $Target -Recurse -File -Include *.cs, *.csproj, *.slnx |
        Select-String -SimpleMatch '#if')
    if ($leftovers.Count) {
        $leftovers | ForEach-Object { Write-Host $_.Path }
        throw "В $name остались шаблонные директивы"
    }

    $infrastructure = Join-Path $Target 'Source' "$name.Infrastructure"
    if ((Test-Path $infrastructure) -ne ($ExpectEf -and $ExpectSplit)) {
        throw "В $name неверное наличие Infrastructure"
    }
    if (-not $ExpectEf -and @(Get-ChildItem -Path $Target -Recurse -Filter GreetingRecord.cs -File).Count) {
        throw "В $name создана EF-модель без EF"
    }

    if ($Di -ne 'None') {
        $projectText = Get-Content (Join-Path $Target 'Source' $name "$name.csproj") -Raw
        $settingsText = Get-Content (Join-Path $Target 'Source' $name 'appsettings.json') -Raw
        if ($UseLogging -and ($projectText -notmatch 'Serilog\.Sinks\.Console' -or $settingsText -notmatch '"Serilog"')) {
            throw "В $name не включено логирование"
        }
        if (-not $UseLogging -and ($projectText -match 'Serilog' -or $settingsText -match '"Serilog"')) {
            throw "В $name логирование осталось при выключенной опции"
        }
    }

    $skills = Join-Path $Target 'Skills'
    if (Test-Path $skills) {
        $mirrored = Join-Path $Target '.claude' 'skills'
        if ((Test-Path $skills) -ne (Test-Path $mirrored)) { throw "В $name скиллы и зеркало расходятся" }
        $actual = @(Get-ChildItem (Join-Path $skills '*/SKILL.md') -File | ForEach-Object { $_.Directory.Name })
        $mirror = @(Get-ChildItem (Join-Path $mirrored '*/SKILL.md') -File | ForEach-Object { $_.Directory.Name })
        if (Compare-Object $actual $mirror) { throw "В $name скиллы и зеркало расходятся" }

        $expected = Get-ExpectedSkills -Di $Di -Mvvm $Mvvm -Tests $Tests -UseEf $ExpectEf -UseSkills $UseSkills -UseLogging $UseLogging
        $missing = @($expected | Where-Object { $actual -notcontains $_ })
        $extra = @($actual | Where-Object { -not $expected.Contains($_) })
        if ($missing.Count -or $extra.Count) {
            throw "В $name неверный набор скиллов: нет [$($missing -join ', ')], лишние [$($extra -join ', ')]"
        }

        $stale = @(Get-ChildItem -Path $skills -Recurse -Filter SKILL.md -File |
            Where-Object { (Get-Content $_.FullName -Raw) -match 'PnvApp' })
        if ($stale.Count) { throw "В $name в скиллах осталось старое имя: $($stale[0].FullName)" }
    }
}


if (-not (Test-Path $Package)) { throw "Нет пакета: $Package. Запустите build/pack.ps1" }
Install-Package
New-Item -ItemType Directory -Force -Path $Out | Out-Null

Write-Host '== Комбинации DI x MVVM x Split (EF включён везде, где допустим) =='
foreach ($di in 'None', 'Hosting', 'HostingScrutor') {
    foreach ($mvvm in 'CommunityToolkit', 'ReactiveUI') {
        foreach ($split in $false, $true) {
            $useEf = $di -ne 'None'
            $splitValue = "$split".ToLower()
            $efValue = "$useEf".ToLower()
            $suffix = if ($split) { 'Split' } else { 'Single' }
            $name = "$di$mvvm$suffix"
            $options = @(
                '--di', $di,
                '--mvvm', $mvvm,
                '--tests', 'true',
                '--skills', 'true',
                '--split', $splitValue,
                '--ef', $efValue
            )
            Write-Host "== $name (ef=$useEf) =="
            $target = New-Project $name $options
            Test-Generated $target $useEf $split $di $mvvm $true
            Build-Project $target
        }
    }
}

Write-Host '== EF при Di=None игнорируется =='
$target = New-Project 'NoneEfIgnored' @(
    '--di', 'None', '--mvvm', 'CommunityToolkit', '--ef', 'true', '--split', 'true'
)
Test-Generated $target $false $true 'None' 'CommunityToolkit' $true
Build-Project $target

Write-Host '== Логирование выключено =='
foreach ($case in @(
        @{ Name = 'LoggingOffSingle';  Di = 'Hosting'; Mvvm = 'CommunityToolkit'; Ef = $false; Split = $false },
        @{ Name = 'LoggingOffEfSplit'; Di = 'Hosting'; Mvvm = 'CommunityToolkit'; Ef = $true;  Split = $true }
    )) {
    $target = New-Project $case.Name @(
        '--di', $case.Di,
        '--mvvm', $case.Mvvm,
        '--tests', 'true',
        '--skills', 'true',
        '--logging', 'false',
        '--ef', "$($case.Ef)".ToLower(),
        '--split', "$($case.Split)".ToLower()
    )
    Test-Generated $target $case.Ef $case.Split $case.Di $case.Mvvm $true -UseLogging $false
    Build-Project $target
}

Write-Host '== Версии Avalonia =='
foreach ($version in $AvaloniaVersions) {
    $name = "Avalonia$($version.Replace('.', ''))"
    Write-Host "== $name =="
    $target = New-Project $name @(
        '--di', 'Hosting',
        '--mvvm', 'CommunityToolkit',
        '--tests', 'false',
        '--skills', 'false',
        '--split', 'false',
        '--avalonia-version', $version
    )
    Test-Generated $target $false $false 'Hosting' 'CommunityToolkit' $false -UseSkills $false
    Build-Project $target
}

Write-Host '== Тесты: default и split+EF+Scrutor+ReactiveUI =='
$defaultTarget = New-Project 'TestDefault' @(
    '--di', 'Hosting', '--mvvm', 'CommunityToolkit', '--tests', 'true', '--skills', 'false'
)
Invoke-Tool @('dotnet', 'test', '--nologo', '-v', 'q') $defaultTarget | Out-Null

$efTarget = New-Project 'TestEfSplit' @(
    '--di', 'HostingScrutor',
    '--mvvm', 'ReactiveUI',
    '--tests', 'true',
    '--skills', 'true',
    '--ef', 'true',
    '--split', 'true'
)
Invoke-Tool @('dotnet', 'test', '--nologo', '-v', 'q') $efTarget | Out-Null

Write-Host 'Все проверки прошли.'
