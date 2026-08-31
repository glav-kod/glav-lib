#!/usr/bin/env bash
#
# Собирает решение и упаковывает пакеты NuGet, складывая `.nupkg` в каталог `artifacts`.
#
# Упаковываются только проекты библиотеки: у тестов и песочницы в `csproj` проставлено
# `IsPackable=false`, поэтому отдельный список проектов скрипту не нужен.
#
# Версия SDK, которым идёт сборка, важна: `GlavLib.SourceGenerators` компилируется против
# той версии Roslyn, которая указана для `Microsoft.CodeAnalysis.CSharp`
# в `Directory.Packages.props`, и компилятор более старой версии отказывается загружать
# такой анализатор с ошибкой CS9057. `Microsoft.CodeAnalysis.CSharp` 5.9.0 требует
# SDK не старее 10.0.400. В GitHub Actions версия задаётся шагом `actions/setup-dotnet`
# в `.github/workflows/publish.yml`, при локальном запуске — тем SDK, который стоит
# на машине.
#
# Настраивается переменными окружения:
#
#   PACKAGE_VERSION    версия пакетов; если пусто, считается скриптом `version.sh`,
#                      которому нужен BUILD_COUNTER;
#   CONFIGURATION      конфигурация сборки, по умолчанию `Release`;
#   PACKAGE_AUTHORS    значение поля `Authors` в пакетах, по умолчанию `GlavKod`.

set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

version="${PACKAGE_VERSION:-$("$root/ci/version.sh")}"
configuration="${CONFIGURATION:-Release}"
authors="${PACKAGE_AUTHORS:-GlavKod}"
artifacts="$root/artifacts"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

echo "Версия пакетов: $version"
echo "Версия .NET SDK: $(dotnet --version)"

rm -rf "$artifacts"
mkdir -p "$artifacts"

dotnet pack GlavLib.sln \
  --configuration "$configuration" \
  -p:Version="$version" \
  -p:Authors="$authors" \
  --output "$artifacts"

echo "Готовые пакеты:"
ls -1 "$artifacts"
