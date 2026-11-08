#!/usr/bin/env python3
"""Генерирует матрицу проектов и проверяет сборку/тесты."""

from __future__ import annotations

import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "artifacts" / "verify"
PACKAGE = ROOT / "artifacts" / "Pnv.Templates.1.0.0.nupkg"

AVALONIA_VERSIONS = [
    "12.0.4",
    "12.0.5",
    "12.1.0",
    "12.1.1",
    "12.1.2",
    "12.1.3",
    "12.1.4",
    "12.1.5",
]


def run(args: list[str], cwd: Path, check: bool = True) -> subprocess.CompletedProcess[str]:
    result = subprocess.run(args, cwd=cwd, text=True, capture_output=True)
    if check and result.returncode != 0:
        print("КОМАНДА:", " ".join(args))
        print(result.stdout)
        print(result.stderr)
        raise SystemExit(1)
    return result


def install() -> None:
    run(["dotnet", "new", "uninstall", "Pnv.Templates"], ROOT, check=False)
    run(["dotnet", "new", "install", str(PACKAGE)], ROOT)


def generate(name: str, options: list[str]) -> Path:
    target = OUT / name
    if target.exists():
        shutil.rmtree(target)
    target.mkdir(parents=True)
    run(
        ["dotnet", "new", "pnv-desktop", "-n", name, "-o", ".", "--no-restore", *options],
        target,
    )
    return target


def build(target: Path) -> None:
    run(["dotnet", "build", "--nologo", "-v", "q"], target)
    print(f"  build ok: {target.name}")


def expected_skills(di: str, mvvm: str, tests: bool, use_ef: bool, use_skills: bool) -> set[str]:
    if not use_skills:
        return set()

    always = {
        "architecture", "async", "avalonia-colors", "avalonia-converters",
        "avalonia-theming", "clarifying-questions", "code-sections", "code-style",
        "collections", "error-handling", "json-sourcegen", "models-organization",
        "paths", "services-organization", "trimming", "view-organization", "xaml",
        "app-settings",
    }
    if di != "None":
        always |= {"options", "logging"}
        always |= {"dependency-injection-scrutor"} if di == "HostingScrutor" else {"dependency-injection"}
    if mvvm == "ReactiveUI":
        always |= {"mvvm-reactiveui", "reactiveui-interactions", "reactiveui-sourcegenerators"}
    else:
        always |= {"mvvm-communitytoolkit"}
    if use_ef:
        always |= {"ef-core-sqlite", "ef-core-interceptors", "infrastructure-organization"}
    if tests:
        always |= {"testing"}
    return always


def check_generated(
    target: Path,
    expect_ef: bool,
    expect_split: bool,
    di: str,
    mvvm: str,
    tests: bool,
    use_skills: bool = True,
) -> None:
    leftovers = run(
        ["grep", "-rn", "#if", "--include=*.cs", "--include=*.csproj", "--include=*.slnx", "."],
        target,
        check=False,
    )
    if leftovers.returncode == 0:
        print(leftovers.stdout)
        raise SystemExit(f"В {target.name} остались шаблонные директивы")

    infrastructure = target / "Source" / f"{target.name}.Infrastructure"
    if infrastructure.exists() != (expect_ef and expect_split):
        raise SystemExit(f"В {target.name} неверное наличие Infrastructure")
    if not expect_ef and list(target.rglob("GreetingRecord.cs")):
        raise SystemExit(f"В {target.name} создана EF-модель без EF")

    skills = target / "Skills"
    mirrored = target / ".claude" / "skills"
    if skills.exists() != mirrored.exists():
        raise SystemExit(f"В {target.name} скиллы и зеркало расходятся")
    actual = {p.parent.name for p in skills.glob("*/SKILL.md")}
    mirror = {p.parent.name for p in mirrored.glob("*/SKILL.md")}
    if actual != mirror:
        raise SystemExit(f"В {target.name} скиллы и зеркало расходятся")
    expected = expected_skills(di, mvvm, tests, expect_ef, use_skills)
    if actual != expected:
        missing = expected - actual
        extra = actual - expected
        raise SystemExit(f"В {target.name} неверный набор скиллов: нет {missing}, лишние {extra}")

    stale = [
        path
        for path in skills.rglob("SKILL.md")
        if "PnvApp" in path.read_text(encoding="utf-8")
    ]
    if stale:
        raise SystemExit(f"В {target.name} в скиллах осталось старое имя: {stale[:3]}")


def main() -> None:
    if not PACKAGE.exists():
        raise SystemExit(f"Нет пакета: {PACKAGE}. Запустите build/pack.py")
    install()
    OUT.mkdir(parents=True, exist_ok=True)

    print("== Комбинации DI x MVVM x Split (EF включён везде, где допустим) ==")
    for di in ["None", "Hosting", "HostingScrutor"]:
        for mvvm in ["CommunityToolkit", "ReactiveUI"]:
            for split in [False, True]:
                use_ef = di != "None"
                name = f"{di}{mvvm}{'Split' if split else 'Single'}"
                options = [
                    "--di", di,
                    "--mvvm", mvvm,
                    "--tests", "true",
                    "--skills", "true",
                    "--split", str(split).lower(),
                    "--ef", str(use_ef).lower(),
                ]
                print(f"== {name} (ef={use_ef}) ==")
                target = generate(name, options)
                check_generated(target, use_ef, split, di, mvvm, True)
                build(target)

    print("== EF при Di=None игнорируется ==")
    target = generate(
        "NoneEfIgnored",
        ["--di", "None", "--mvvm", "CommunityToolkit", "--ef", "true", "--split", "true"],
    )
    check_generated(target, expect_ef=False, expect_split=True, di="None", mvvm="CommunityToolkit", tests=True)
    build(target)

    print("== Версии Avalonia ==")
    for version in AVALONIA_VERSIONS:
        name = f"Avalonia{version.replace('.', '')}"
        print(f"== {name} ==")
        target = generate(
            name,
            [
                "--di", "Hosting",
                "--mvvm", "CommunityToolkit",
                "--tests", "false",
                "--skills", "false",
                "--split", "false",
                "--avalonia-version", version,
            ],
        )
        check_generated(target, expect_ef=False, expect_split=False, di="Hosting", mvvm="CommunityToolkit", tests=False, use_skills=False)
        build(target)

    print("== Тесты: default и split+EF+Scrutor+ReactiveUI ==")
    default_target = generate(
        "TestDefault",
        ["--di", "Hosting", "--mvvm", "CommunityToolkit", "--tests", "true", "--skills", "false"],
    )
    run(["dotnet", "test", "--nologo", "-v", "q"], default_target)
    ef_target = generate(
        "TestEfSplit",
        [
            "--di", "HostingScrutor",
            "--mvvm", "ReactiveUI",
            "--tests", "true",
            "--skills", "true",
            "--ef", "true",
            "--split", "true",
        ],
    )
    run(["dotnet", "test", "--nologo", "-v", "q"], ef_target)

    print("Все проверки прошли.")


if __name__ == "__main__":
    main()
