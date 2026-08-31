#!/usr/bin/env bash
#
# Публикует пакеты из каталога `artifacts` в NuGet-хранилище.
#
# Скрипт ничего не собирает: он рассчитан на то, что `.nupkg` уже лежат в `artifacts` —
# либо после `pack.sh`, либо как артефакты предыдущей сборки в цепочке TeamCity.
#
# Настраивается переменными окружения. Имена унаследованы от параметров, которые задавались
# в проекте TeamCity ещё для сборки на Nuke, поэтому менять настройки сервера не пришлось:
#
#   NugetApiKey  ключ доступа, обязателен;
#   NugetSource  адрес хранилища, по умолчанию `https://api.nuget.org/v3/index.json`.
#
# На агенте нужен `dotnet` — любой версии: публикация готового пакета от версии SDK
# не зависит, в отличие от компиляции.

set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

if [[ -z "${NugetApiKey:-}" ]]; then
  echo "Не задан NugetApiKey — публиковать нечем" >&2
  exit 1
fi

nuget_source="${NugetSource:-https://api.nuget.org/v3/index.json}"

shopt -s nullglob
packages=("$root"/artifacts/*.nupkg)

if [[ ${#packages[@]} -eq 0 ]]; then
  echo "В каталоге artifacts нет ни одного .nupkg — публиковать нечего" >&2
  exit 1
fi

for package in "${packages[@]}"; do
  echo "Публикую $(basename "$package") в $nuget_source"
  dotnet nuget push "$package" --api-key "$NugetApiKey" --source "$nuget_source"
done
