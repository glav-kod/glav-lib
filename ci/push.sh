#!/usr/bin/env bash
#
# Публикует пакеты из каталога `artifacts` в NuGet-хранилище.
#
# Скрипт ничего не собирает: он рассчитан на то, что `.nupkg` уже лежат в `artifacts` —
# либо после `pack.sh`, либо как артефакты предыдущего задания workflow.
#
# Настраивается переменными окружения:
#
#   NUGET_API_KEY  ключ доступа, обязателен;
#   NUGET_SOURCE   адрес хранилища, по умолчанию `https://api.nuget.org/v3/index.json`.
#
# В GitHub Actions ключ не хранится в секретах репозитория: его выдаёт на один час
# шаг `NuGet/login` по OIDC-токену задания (trusted publishing на nuget.org). Локально
# в `NUGET_API_KEY` подставляют обычный ключ, выпущенный в личном кабинете nuget.org.

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

echo "Опубликовано пакетов: ${#packages[@]}"

# Сводка задания в GitHub Actions: по ней видно, что уехало в хранилище, не открывая лог.
if [[ -n "${GITHUB_STEP_SUMMARY:-}" ]]; then
  {
    echo "### Опубликованные пакеты"
    echo
    for package in "${packages[@]}"; do
      echo "- \`$(basename "$package")\`"
    done
  } >> "$GITHUB_STEP_SUMMARY"
fi
