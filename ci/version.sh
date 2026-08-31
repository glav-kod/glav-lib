#!/usr/bin/env bash
#
# Печатает версию пакетов NuGet в stdout.
#
# Схема версии перенесена из прежней сборки на Nuke и не изменилась:
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
#   BUILD_COUNTER  номер сборки, обязателен (в TeamCity — `%build.counter%`);
#   BUILD_BRANCH   имя ветки; если пусто, берётся из git. TeamCity подставляет
#                  `<default>` для основной ветки — это учтено;
#   TimeZone       идентификатор таймзоны для вычисления даты (например `Asia/Bishkek`);
#                  если пусто, берётся таймзона машины. Имя унаследовано от параметра,
#                  который проект TeamCity задавал ещё для сборки на Nuke.

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
  # `<default>` — так TeamCity называет основную ветку, когда в VCS-корне не настроены
  # спецификации веток. Для версии это то же самое, что `main`.
  "<default>" | main | master)
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
if [[ -n "${TimeZone:-}" ]]; then
  today="$(TZ="$TimeZone" date '+%Y %m %d')"
else
  today="$(date '+%Y %m %d')"
fi

read -r year month day <<<"$today"

printf '%s.%s.%s.%s%s\n' "$year" "$((10#$month))" "$((10#$day))" "$build_counter" "$branch_suffix"
