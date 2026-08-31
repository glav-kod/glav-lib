#!/usr/bin/env bash
#
# Публикует пакеты из каталога `artifacts` в NuGet-хранилище.
#
# Скрипт ничего не собирает: он рассчитан на то, что `.nupkg` уже лежат в `artifacts` —
# либо после `pack.sh`, либо как артефакты предыдущей сборки в цепочке TeamCity.
#
# Настраивается переменными окружения:
#
#   NUGET_API_KEY  ключ доступа, обязателен;
#   NUGET_SOURCE   адрес хранилища, по умолчанию `https://api.nuget.org/v3/index.json`.
#
# На агенте нужен `dotnet` — любой версии: публикация готового пакета от версии SDK
# не зависит, в отличие от компиляции.

set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

if [[ -z "${NUGET_API_KEY:-}" ]]; then
  echo "Не задан NUGET_API_KEY — публиковать нечем" >&2
  exit 1
fi

nuget_source="${NUGET_SOURCE:-https://api.nuget.org/v3/index.json}"

shopt -s nullglob
packages=("$root"/artifacts/*.nupkg)

if [[ ${#packages[@]} -eq 0 ]]; then
  echo "В каталоге artifacts нет ни одного .nupkg — публиковать нечего" >&2
  exit 1
fi

for package in "${packages[@]}"; do
  echo "Публикую $(basename "$package") в $nuget_source"
  dotnet nuget push "$package" --api-key "$NUGET_API_KEY" --source "$nuget_source"
done
