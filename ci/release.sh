#!/usr/bin/env bash
#
# Создаёт релиз GitHub для собранной версии и прикладывает к нему пакеты из каталога
# `artifacts`.
#
# Скрипт ничего не собирает: он рассчитан на то, что `.nupkg` уже лежат в `artifacts`
# после `pack.sh`. Тег релиза создаётся самим GitHub на указанном коммите, поэтому
# отдельно помечать коммит тегом не нужно.
#
# Повторный запуск на той же версии не падает: если релиз с таким тегом уже есть
# (например, при повторном прогоне того же задания workflow — номер сборки у него
# прежний), скрипт обновляет его заметки и перезаписывает вложения.
#
# Настраивается переменными окружения:
#
#   GH_TOKEN         токен для `gh`, обязателен; в GitHub Actions это `github.token`,
#                    и заданию нужно разрешение `contents: write`;
#   PACKAGE_VERSION  версия пакетов; если пусто, считается скриптом `version.sh`,
#                    которому нужен BUILD_COUNTER;
#   RELEASE_TAG      тег релиза, по умолчанию `v<версия>`;
#   RELEASE_TARGET   коммит, на котором создаётся тег, по умолчанию `GITHUB_SHA`,
#                    а вне GitHub Actions — текущий HEAD.
#
# Локально скрипту нужен установленный `gh`, авторизованный в репозитории.

set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

if [[ -z "${GH_TOKEN:-}" ]]; then
  echo "Не задан GH_TOKEN — создавать релиз нечем" >&2
  exit 1
fi

if ! command -v gh >/dev/null 2>&1; then
  echo "Не найден gh — без него релиз не создать" >&2
  exit 1
fi

version="${PACKAGE_VERSION:-$("$root/ci/version.sh")}"
tag="${RELEASE_TAG:-v$version}"
target="${RELEASE_TARGET:-${GITHUB_SHA:-$(git rev-parse HEAD)}}"

shopt -s nullglob
packages=("$root"/artifacts/*.nupkg)

if [[ ${#packages[@]} -eq 0 ]]; then
  echo "В каталоге artifacts нет ни одного .nupkg — прикладывать к релизу нечего" >&2
  exit 1
fi

# Версия с суффиксом ветки — предрелизная, и релиз помечается соответственно, чтобы
# не подменять собой последний выпуск из основной ветки.
prerelease_flag=()
if [[ "$version" == *-* ]]; then
  prerelease_flag=(--prerelease)
fi

notes="Пакеты версии \`$version\`, собранные из коммита \`$target\`."
notes+=$'\n\n'
notes+="Те же пакеты опубликованы в nuget.org и в GitHub Packages; к релизу они приложены файлами \`.nupkg\`."
notes+=$'\n\n'
notes+=$'### Пакеты\n\n'
for package in "${packages[@]}"; do
  notes+="- \`$(basename "$package")\`"$'\n'
done

if gh release view "$tag" >/dev/null 2>&1; then
  echo "Релиз $tag уже есть — обновляю заметки и вложения"
  gh release edit "$tag" --notes "$notes" "${prerelease_flag[@]}"
  gh release upload "$tag" "${packages[@]}" --clobber
else
  echo "Создаю релиз $tag на коммите $target"
  gh release create "$tag" \
    --target "$target" \
    --title "$version" \
    --notes "$notes" \
    "${prerelease_flag[@]}" \
    "${packages[@]}"
fi

# Адрес релиза нужен только для сообщений, поэтому его неудача сам выпуск не отменяет:
# пакеты к этому моменту уже приложены.
release_url="$(gh release view "$tag" --json url --jq .url 2>/dev/null || true)"
echo "Релиз готов: ${release_url:-$tag}"

# Сводка задания в GitHub Actions: ссылку на релиз видно, не открывая лог.
if [[ -n "${GITHUB_STEP_SUMMARY:-}" && -n "$release_url" ]]; then
  {
    echo "### Релиз GitHub"
    echo
    echo "- [$tag]($release_url)"
  } >> "$GITHUB_STEP_SUMMARY"
fi
