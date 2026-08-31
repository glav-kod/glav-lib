#!/usr/bin/env bash
#
# Собирает решение и упаковывает пакеты NuGet, складывая `.nupkg` в каталог `artifacts`.
#
# Компиляция идёт не на агенте, а в контейнере по корневому `Dockerfile`: версия .NET SDK
# там задана явно, потому что компилятор старее Roslyn из `Microsoft.CodeAnalysis.CSharp`
# не загружает наш source-генератор (ошибка CS9057).
#
# Настраивается переменными окружения:
#
#   PACKAGE_VERSION    версия пакетов; если пусто, считается скриптом `version.sh`,
#                      которому нужен BUILD_COUNTER;
#   CONFIGURATION      конфигурация сборки, по умолчанию `Release`;
#   DOTNET_SDK_IMAGE   образ .NET SDK, по умолчанию `mcr.microsoft.com/dotnet/sdk:10.0.400`;
#   PACKAGE_AUTHORS    значение поля `Authors` в пакетах, по умолчанию `GlavKod`.
#
# Требуется Docker с BuildKit (версия 23 и новее): результат забирается из контейнера
# через `--output`, а сам `Dockerfile` использует кеш-mount для пакетов NuGet.

set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

version="${PACKAGE_VERSION:-$("$root/ci/version.sh")}"
configuration="${CONFIGURATION:-Release}"
sdk_image="${DOTNET_SDK_IMAGE:-mcr.microsoft.com/dotnet/sdk:10.0.400}"
authors="${PACKAGE_AUTHORS:-GlavKod}"
artifacts="$root/artifacts"

echo "Версия пакетов: $version"
echo "Образ .NET SDK: $sdk_image"

# Номер сборки в TeamCity заменяется на версию пакетов, чтобы по списку сборок было видно,
# какая версия из какой собрана. На счётчик сборок это не влияет: версия считается из
# `%build.counter%`, а он живёт отдельно от номера.
if [[ -n "${TEAMCITY_VERSION:-}" ]]; then
  echo "##teamcity[buildNumber '$version']"
fi

rm -rf "$artifacts"
mkdir -p "$artifacts"

export DOCKER_BUILDKIT=1

docker build \
  --file "$root/Dockerfile" \
  --target artifacts \
  --output "type=local,dest=$artifacts" \
  --progress plain \
  --build-arg "DOTNET_SDK_IMAGE=$sdk_image" \
  --build-arg "CONFIGURATION=$configuration" \
  --build-arg "PACKAGE_VERSION=$version" \
  --build-arg "PACKAGE_AUTHORS=$authors" \
  "$root"

echo "Готовые пакеты:"
ls -1 "$artifacts"
