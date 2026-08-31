#!/usr/bin/env bash
#
# Печатает версию пакетов NuGet в stdout.
#
# Схема версии не менялась с тех пор, как выпуск делался на Nuke и на TeamCity:
#
#   <год>.<месяц>.<день>.<номер сборки>[-<ветка>]
#
# Месяц и день идут без ведущих нулей, суффикс ветки добавляется всюду, кроме основной
# ветки, а `/` и `\` в имени ветки заменяются на `-`, потому что в версии NuGet они
# недопустимы. Сборка из ветки `feature/foo` даёт предрелизную версию вида
# `2026.8.31.42-feature-foo`, сборка из `main` — релизную `2026.8.31.42`.
#
# Входные данные передаются переменными окружения:
#
#   BUILD_COUNTER  номер сборки, обязателен (в GitHub Actions — `github.run_number`);
#   BUILD_BRANCH   имя ветки; если пусто, берётся из git (в GitHub Actions —
#                  `github.ref_name`);
#   TIMEZONE       идентификатор таймзоны для вычисления даты (например `Asia/Bishkek`);
#                  если пусто, берётся таймзона машины. Раннеры GitHub Actions живут
#                  в UTC, поэтому таймзону там задают явно — иначе дата в версии
#                  разойдётся с датой сборки, сделанной разработчиком у себя.

set -euo pipefail

build_counter="${BUILD_COUNTER:-}"

if [[ -z "$build_counter" ]]; then
  echo "Не задан BUILD_COUNTER — без номера сборки версию не собрать" >&2
  exit 1
fi

branch="${BUILD_BRANCH:-}"

if [[ -z "$branch" ]]; then
  branch="$(git rev-parse --abbrev-ref HEAD 2>/dev/null || true)"
fi

branch="${branch#refs/heads/}"

case "$branch" in
  main | master)
    branch_suffix=""
    ;;
  "" | HEAD)
    echo "Не удалось определить ветку: задай BUILD_BRANCH явно" >&2
    exit 1
    ;;
  *)
    branch_suffix="-$(printf '%s' "$branch" | tr '/\\' '--')"
    ;;
esac

# Ведущие нули убираются арифметикой с основанием 10: без явного основания `08` считается
# восьмеричным числом, и август с сентябрём ломают скрипт.
if [[ -n "${TIMEZONE:-}" ]]; then
  today="$(TZ="$TIMEZONE" date '+%Y %m %d')"
else
  today="$(date '+%Y %m %d')"
fi

read -r year month day <<<"$today"

printf '%s.%s.%s.%s%s\n' "$year" "$((10#$month))" "$((10#$day))" "$build_counter" "$branch_suffix"
