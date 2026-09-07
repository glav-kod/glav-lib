# Трекер задач: GitHub

Задачи и спецификации этого репозитория живут как issues на GitHub, в репозитории
`glav-kod/glav-lib`. Все операции выполняются через CLI `gh`. Запущенный внутри клона
`gh` сам определяет репозиторий по `git remote`, указывать его отдельно не нужно.

## Конвенции

- **Создать задачу:** `gh issue create --title "..." --body "..."`. Для многострочного
  описания используй heredoc.
- **Прочитать задачу:** `gh issue view <номер> --comments`; метки и отдельные комментарии
  при необходимости отбирай через `jq`.
- **Список задач:**
  `gh issue list --state open --json number,title,body,labels,comments --jq '[.[] | {number, title, body, labels: [.labels[].name], comments: [.comments[].body]}]'`
  с нужными фильтрами `--label` и `--state`.
- **Комментарий:** `gh issue comment <номер> --body "..."`
- **Метки:** `gh issue edit <номер> --add-label "..."` и `--remove-label "..."`
- **Закрыть:** `gh issue close <номер> --comment "..."`

Заголовки, описания и комментарии задач пиши **на русском языке** — то же требование,
что и для коммитов и pull request'ов (`.claude/rules/language-russian.md`).

## Pull request'ы

Pull request'ы этого репозитория тоже ведутся через `gh`: `gh pr create`, `gh pr view`,
`gh pr edit`, `gh pr comment`, `gh pr merge --squash`. Требования к заголовку, описанию,
выбору базовой ветки и стратегии merge остаются прежними и описаны
в `.claude/rules/git-workflow.md`.

**Считать ли pull request'ы источником заявок: нет.** _(Поставь «да», если внешние pull
request'ы нужно разбирать наравне с задачами; этот признак читает скилл `/triage`.)_

Когда стоит «да», pull request'ы проходят через те же метки и состояния, что и задачи:

- **Прочитать:** `gh pr view <номер> --comments`, диф — `gh pr diff <номер>`.
- **Отобрать внешние для триажа:**
  `gh pr list --state open --json number,title,body,labels,author,authorAssociation,comments`,
  оставив `authorAssociation` со значением `CONTRIBUTOR`, `FIRST_TIME_CONTRIBUTOR` или
  `NONE` (отбросив `OWNER`, `MEMBER`, `COLLABORATOR`).
- **Комментарий, метки, закрытие:** `gh pr comment`, `gh pr edit --add-label` /
  `--remove-label`, `gh pr close`.

Нумерация задач и pull request'ов на GitHub общая, поэтому голое `#42` может означать
и то, и другое: сначала пробуй `gh pr view 42`, при неудаче — `gh issue view 42`.

## Что имеется в виду под «опубликовать в трекере»

Создать issue на GitHub.

## Что имеется в виду под «взять соответствующий тикет»

Выполнить `gh issue view <номер> --comments`.

## Операции навигации (`/wayfinder`)

Карта — отдельная задача, тикеты — её дочерние задачи.

- **Карта:** задача с меткой `wayfinder:map`, в теле которой лежат разделы «Заметки»,
  «Принятые решения» и «Туман». Создаётся как `gh issue create --label wayfinder:map`.
- **Дочерний тикет:** задача, связанная с картой как sub-issue GitHub (через `gh api`
  на endpoint sub-issues). Если sub-issues в репозитории не включены, добавь ссылку
  на дочернюю задачу в список задач в теле карты, а в начало тела дочерней —
  строку `Part of #<номер карты>`. Метки — `wayfinder:<тип>`: `research`, `prototype`,
  `grilling` или `task`. Взятый в работу тикет назначается на ведущего разработчика.
- **Блокировки:** родные зависимости задач GitHub — единственное представление, видимое
  в интерфейсе. Ребро добавляется командой
  `gh api --method POST repos/<owner>/<repo>/issues/<дочерняя>/dependencies/blocked_by -F issue_id=<database id блокирующей>`,
  где `<database id>` — числовой идентификатор блокирующей задачи
  (`gh api repos/<owner>/<repo>/issues/<n> --jq .id`, а **не** её `#номер` и не `node_id`).
  GitHub отдаёт `issue_dependencies_summary.blocked_by` — счётчик только открытых
  блокирующих задач, он и служит признаком блокировки. Если зависимости недоступны,
  используй запасной вариант: строку `Blocked by: #<n>, #<n>` в начале тела задачи.
  Тикет разблокирован, когда все блокирующие задачи закрыты.
- **Поиск фронта работ:** возьми открытые дочерние задачи карты (`gh issue list --state open`,
  ограниченный её sub-issues или списком задач в теле), отбрось те, у которых есть открытая
  блокирующая задача (`issue_dependencies_summary.blocked_by > 0` либо открытая задача
  в строке `Blocked by`) или назначенный исполнитель; побеждает первая в порядке карты.
- **Взять в работу:** `gh issue edit <n> --add-assignee @me` — первая запись в сессии.
- **Закрыть вопрос:** `gh issue comment <n> --body "<ответ>"`, затем `gh issue close <n>`,
  затем допиши ссылку на контекст в раздел «Принятые решения» карты.
