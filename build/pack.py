#!/usr/bin/env python3
"""Собирает NuGet-пакет шаблонов Pnv.Templates."""

from __future__ import annotations

import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
TEMPLATE_SRC = ROOT / "template-src"
STAGE = ROOT / "build" / "stage"
CONTENT = STAGE / "content"
SKILLS = TEMPLATE_SRC / "skills"
ARTIFACTS = ROOT / "artifacts"

PACKAGE_ID = "Pnv.Templates"
PACKAGE_VERSION = "1.0.1"

NUSPEC = f"""<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd">
  <metadata>
    <id>{PACKAGE_ID}</id>
    <version>{PACKAGE_VERSION}</version>
    <authors>PNV</authors>
    <description>Шаблоны .NET для проектов ПНВ: десктопное приложение на AvaloniaUI и веб-приложение на ASP.NET Core Minimal API.</description>
    <packageTypes>
      <packageType name="Template" />
    </packageTypes>
  </metadata>
</package>
"""


def reset_stage() -> None:
    if STAGE.exists():
        shutil.rmtree(STAGE)
    CONTENT.mkdir(parents=True)


def copy_tree(source: Path, target: Path) -> None:
    if not source.exists():
        raise SystemExit(f"Нет каталога: {source}")
    shutil.copytree(source, target, dirs_exist_ok=True)


def stage_content() -> None:
    copy_tree(TEMPLATE_SRC / "desktop", CONTENT / "desktop")
    copy_tree(TEMPLATE_SRC / "desktop-content", CONTENT / "desktop-content")
    copy_tree(TEMPLATE_SRC / "web", CONTENT / "web")
    copy_tree(TEMPLATE_SRC / "web-content", CONTENT / "web-content")

    skill_files = sorted(SKILLS.glob("*/SKILL.md"))
    if not skill_files:
        raise SystemExit(f"Не найдены скиллы в {SKILLS}")

    skills_target = CONTENT / "desktop-content" / "skills"
    claude_target = CONTENT / "desktop-content" / "claude-skills"

    for target in (skills_target, claude_target):
        if target.exists():
            shutil.rmtree(target)
        target.mkdir(parents=True)

    for skill_file in skill_files:
        skill_id = skill_file.parent.name
        for target in (skills_target, claude_target):
            destination = target / skill_id
            destination.mkdir(parents=True, exist_ok=True)
            shutil.copy2(skill_file, destination / "SKILL.md")

    web_skills = TEMPLATE_SRC / "web-skills"
    web_skill_files = sorted(web_skills.glob("*/SKILL.md"))
    if not web_skill_files:
        raise SystemExit(f"Не найдены скиллы в {web_skills}")

    for target_name in ("skills", "claude-skills"):
        web_target = CONTENT / "web-content" / target_name
        if web_target.exists():
            shutil.rmtree(web_target)
        web_target.mkdir(parents=True)

        for skill_file in web_skill_files:
            destination = web_target / skill_file.parent.name
            destination.mkdir(parents=True, exist_ok=True)
            shutil.copy2(skill_file, destination / "SKILL.md")

    print(f"Скиллов: {len(skill_files)} + {len(web_skill_files)} веб-скиллов")


def build_package() -> Path:
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    package_path = ARTIFACTS / f"{PACKAGE_ID}.{PACKAGE_VERSION}.nupkg"
    if package_path.exists():
        package_path.unlink()

    nuspec_path = STAGE / f"{PACKAGE_ID}.nuspec"
    nuspec_path.write_text(NUSPEC, encoding="utf-8")

    with zipfile.ZipFile(package_path, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.write(nuspec_path, nuspec_path.name)
        for path in sorted(CONTENT.rglob("*")):
            if path.is_file():
                archive.write(path, path.relative_to(STAGE).as_posix())

    print(f"Пакет: {package_path} ({package_path.stat().st_size} байт)")
    return package_path


def install(package_path: Path) -> None:
    subprocess.run(
        ["dotnet", "new", "uninstall", PACKAGE_ID],
        cwd=ROOT,
        check=False,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )
    subprocess.run(
        ["dotnet", "new", "install", str(package_path)],
        cwd=ROOT,
        check=True,
    )


def main() -> None:
    reset_stage()
    stage_content()
    package_path = build_package()
    if "--install" in sys.argv:
        install(package_path)


if __name__ == "__main__":
    main()
