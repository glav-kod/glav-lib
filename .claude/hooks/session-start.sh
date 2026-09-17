#!/bin/bash
#
# SessionStart hook: ставит .NET SDK в облачной среде Claude Code.
#
# В контейнерах Claude Code на вебе .NET SDK не предустановлен, поэтому без этого хука
# в сессии недоступны ни `dotnet build`, ни `dotnet test`. Локально хук ничего не делает:
# на машине разработчика SDK ставится вместе с остальным окружением.
#
# Скрипт идемпотентен: если SDK нужной версии уже установлен, повторная установка
# не выполняется. Состояние контейнера кешируется после первого прогона, поэтому
# последующие сессии стартуют без загрузки.
#
# Хук асинхронный: сессия начинается сразу, не дожидаясь установки. Плата за это —
# первые несколько минут в новом контейнере `dotnet` может быть ещё недоступен. Если команда
# не находит его, подожди или выполни этот скрипт вручную:
#   CLAUDE_CODE_REMOTE=true ./.claude/hooks/session-start.sh
# В контейнере, где SDK уже стоит, хук завершается почти мгновенно и гонки не создаёт.

set -euo pipefail

# Локальные запуски пропускаем: хук существует только ради облачной среды. Проверка идёт
# до объявления асинхронного режима — переводить в фон нечего, если работы нет.
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

# Объявление асинхронного режима обязано быть первым, что скрипт пишет в stdout:
# всё, что выведено раньше, ломает разбор этого JSON.
echo '{"async": true, "asyncTimeout": 600000}'

DOTNET_INSTALL_DIR="/root/.dotnet"

project_dir="${CLAUDE_PROJECT_DIR:-.}"
props_file="$project_dir/Directory.Build.props"

# Канал SDK вычисляется из TargetFramework проектов (`net10.0` → `10.0`), а не задаётся
# константой: иначе при следующем обновлении фреймворка хук молча разъедется с решением
# и восстановление пакетов упадёт.
dotnet_channel="$(sed -n 's:.*<TargetFramework>net\([0-9.]*\)</TargetFramework>.*:\1:p' "$props_file" | head -1)"

if [ -z "$dotnet_channel" ]; then
  echo "Не удалось прочитать TargetFramework из $props_file" >&2
  exit 1
fi

if [ ! -x "$DOTNET_INSTALL_DIR/dotnet" ]; then
  echo "Устанавливаю .NET SDK $dotnet_channel в $DOTNET_INSTALL_DIR"

  install_script="$(mktemp)"
  trap 'rm -f "$install_script"' EXIT

  # `--fail` обязателен: без него curl складывает в файл тело ошибки HTTP и выходит с нулевым
  # кодом, после чего bash исполняет страницу ошибки вместо установщика.
  curl -fsSL --max-time 120 https://dot.net/v1/dotnet-install.sh -o "$install_script"

  bash "$install_script" --channel "$dotnet_channel" --install-dir "$DOTNET_INSTALL_DIR"
else
  echo ".NET SDK уже установлен в $DOTNET_INSTALL_DIR"
fi

export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
export PATH="$DOTNET_INSTALL_DIR:$PATH"

# Переменные нужны каждой команде сессии, а не только этому скрипту: без записи в CLAUDE_ENV_FILE
# агент не найдёт `dotnet` в PATH. При ручном запуске эта переменная не задана — тогда
# записывать некуда, и `dotnet` вызывается по полному пути.
if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  {
    echo "export DOTNET_ROOT=\"$DOTNET_INSTALL_DIR\""
    echo "export PATH=\"$DOTNET_INSTALL_DIR:\$PATH\""
    echo "export DOTNET_CLI_TELEMETRY_OPTOUT=1"
    echo "export DOTNET_NOLOGO=1"
  } >> "$CLAUDE_ENV_FILE"
fi

# Пакеты восстанавливаем заранее: они складываются в кеш контейнера, и первая же сборка
# в сессии обходится без обращения к сети.
if [ -f "$project_dir/GlavLib.sln" ]; then
  echo "Восстанавливаю пакеты решения GlavLib.sln"

  "$DOTNET_INSTALL_DIR/dotnet" restore "$project_dir/GlavLib.sln"
fi

echo "Окружение .NET готово: $("$DOTNET_INSTALL_DIR/dotnet" --version)"
