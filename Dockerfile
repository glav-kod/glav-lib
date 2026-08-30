# syntax=docker/dockerfile:1

# Сборка и упаковка NuGet-пакетов GlavLib.
#
# Файл существует ради того, чтобы версия .NET SDK задавалась явно, а не выбиралась по тому,
# какой SDK оказался установлен на агенте сборки. Source-генератор `GlavLib.SourceGenerators`
# компилируется против той версии Roslyn, которая указана для `Microsoft.CodeAnalysis.CSharp`
# в `Directory.Packages.props`, и компилятор более старой версии отказывается загружать такой
# анализатор с ошибкой CS9057. Значит, образ SDK обязан нести Roslyn не ниже этой версии:
# `Microsoft.CodeAnalysis.CSharp` 5.9.0 требует SDK 10.0.400, он и выбран по умолчанию.
# Обновляя `Microsoft.CodeAnalysis.CSharp`, обновляй и образ здесь.
#
# Нужен BuildKit (Docker 23 и новее): используются кеш-mount и экспорт артефактов через
# `--output`. Ручной запуск:
#
#   docker build --build-arg PACKAGE_VERSION=1.2.3 --target artifacts --output artifacts .

ARG DOTNET_SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0.400

FROM ${DOTNET_SDK_IMAGE} AS build

ARG CONFIGURATION=Release
ARG PACKAGE_VERSION=0.0.0
ARG PACKAGE_AUTHORS=GlavKod

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

WORKDIR /src

COPY . .

# Восстановление, компиляция и упаковка идут одной командой намеренно: кеш пакетов подключён
# через `--mount`, он доступен только внутри своего шага, поэтому разнести шаги нельзя.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet pack GlavLib.sln \
        --configuration "$CONFIGURATION" \
        -p:Version="$PACKAGE_VERSION" \
        -p:Authors="$PACKAGE_AUTHORS" \
        --output /artifacts

# Стадия нужна только для выгрузки результата на хост: `--target artifacts --output <каталог>`
# кладёт туда сами `.nupkg`, а не слои образа.
FROM scratch AS artifacts

COPY --from=build /artifacts/ /
