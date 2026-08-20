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

# Версия берётся из TargetFramework в Directory.Build.props: канал должен совпадать
# с тем, на что нацелены проекты, иначе восстановление пакетов упадёт.
DOTNET_CHANNEL="10.0"

if [ ! -x "$DOTNET_INSTALL_DIR/dotnet" ]; then
  echo "Устанавливаю .NET SDK $DOTNET_CHANNEL в $DOTNET_INSTALL_DIR"

  install_script="$(mktemp)"

  curl -sSL --max-time 120 https://dot.net/v1/dotnet-install.sh -o "$install_script"

  bash "$install_script" --channel "$DOTNET_CHANNEL" --install-dir "$DOTNET_INSTALL_DIR"

  rm -f "$install_script"
else
  echo ".NET SDK уже установлен в $DOTNET_INSTALL_DIR"
fi

export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
export PATH="$DOTNET_INSTALL_DIR:$PATH"

# Переменные нужны каждой команде сессии, а не только этому скрипту: без записи в CLAUDE_ENV_FILE
# агент не найдёт `dotnet` в PATH.
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
if [ -f "${CLAUDE_PROJECT_DIR:-.}/GlavLib.sln" ]; then
  echo "Восстанавливаю пакеты решения GlavLib.sln"

  "$DOTNET_INSTALL_DIR/dotnet" restore "${CLAUDE_PROJECT_DIR:-.}/GlavLib.sln"
fi

echo "Окружение .NET готово: $("$DOTNET_INSTALL_DIR/dotnet" --version)"
