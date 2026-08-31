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
# Версия публикуемых пакетов скрипту нужна только для отчёта в TeamCity. Она берётся
# из `PACKAGE_VERSION`, а если переменная пуста — из номера сборки `BUILD_NUMBER`: номер
# конфигурации `Publish` повторяет номер `Pack`, а тот равен вычисленной версии пакетов.
# При локальном запуске обеих переменных нет, и скрипт просто не сообщает версию.
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
version="${PACKAGE_VERSION:-${BUILD_NUMBER:-}}"

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

# В TeamCity опубликованная версия проставляется тегом и выносится в текст статуса сборки:
# по списку сборок сразу видно, что и куда уехало, а по тегу сборку можно найти поиском.
if [[ -n "${TEAMCITY_VERSION:-}" ]]; then
  if [[ -n "$version" ]]; then
    echo "##teamcity[addBuildTag '$version']"
    echo "##teamcity[buildStatus text='Опубликовано пакетов: ${#packages[@]}, версия $version, хранилище $nuget_source']"
  else
    echo "##teamcity[buildStatus text='Опубликовано пакетов: ${#packages[@]}, хранилище $nuget_source']"
  fi
fi
