# Карта проекта GlavLib

`GlavLib` — внутренняя библиотека GlavKod для .NET-приложений: типы-значения, сериализация,
многоязычные сообщения, работа с PostgreSQL через NHibernate и Dapper, каркас HTTP-команд
на minimal API и source-генераторы, которые выпускают шаблонный код за разработчика.

Главное отличие от прикладного репозитория: продукт здесь — **публичный API пакетов NuGet**.
Ломающая правка публичного типа расходится по всем приложениям, которые на него ссылаются,
и агент этих приложений не видит. Поэтому изменение сигнатуры, поведения или состава
публичного типа — всегда тема для обсуждения, а не для молчаливой правки.

## Структура репозитория

```
GlavLib.Abstractions/      Контракты без зависимостей: Entity, Error, Result, EnumObject, атрибуты DI
GlavLib.Basics/            Типы-значения, сериализация, логирование, multi-lang, доменные события
GlavLib.Db/                NHibernate + Dapper + Npgsql: сессии, конвенции, user-типы
GlavLib.App/               ASP.NET Core: команды, фильтры, валидация, HTTP-обвязка
GlavLib.SourceGenerators/  Три генератора: EnumObject, ошибки, регистрация сервисов
GlavLib.SourceGenerators.Tests/  Тесты генераторов (базы не требуют)
GlavLib.Tests/             Тесты Basics и Db (часть требует Postgres)
Sandbox/
  GlavLib.Sandbox.API/     Песочница-приложение: как библиотека выглядит со стороны потребителя
  GlavLib.Sandbox.API.Tests/  Интеграционные тесты песочницы (требуют Postgres)
  GlavLib.Sandbox.Console/ Консольная песочница
migrations/                Liquibase: db.changelog.xml + public/
init-database/             init.sql для контейнера Postgres (пользователь sys, база glavdb)
ci/                        Скрипты выпуска: version.sh, pack.sh, push.sh
.github/workflows/         GitHub Actions: publish.yml — сборка, упаковка и публикация пакетов
```

Решение — `GlavLib.sln`. Общие настройки сборки — `Directory.Build.props` (`net10.0`,
`Nullable`, `TreatWarningsAsErrors`, константа компиляции `JETBRAINS_ANNOTATIONS`),
`Directory.Build.targets` (подключение `NullGuard.Fody` и автоматическое включение `_errors/*`
в `AdditionalFiles`), версии пакетов — `Directory.Packages.props` (центральное управление
версиями, в `csproj` версии не пишутся).

Константа `JETBRAINS_ANNOTATIONS` определена на всё решение и снимать её нельзя. Атрибуты
пакета `JetBrains.Annotations` помечены `[Conditional("JETBRAINS_ANNOTATIONS")]`, поэтому без
этой константы компилятор выбрасывает их применение из выходной сборки: в метаданных пакетов
не остаётся ни `[PublicAPI]`, ни `[UsedImplicitly]`, и у потребителей библиотеки подсказки
IDE не работают — например, Rider сообщает «Class is never instantiated» на наследниках
`EnumObject`. Задаётся она с сохранением `$(DefineConstants)`, чтобы не затереть `DEBUG`
и `TRACE`, которые SDK добавляет позже.

## Зависимости сборок

```
Abstractions ← Basics ← Db ← App
                  ↑        ↑
            SourceGenerators (подключается как Analyzer во все сборки)
```

`GlavLib.Abstractions` не зависит ни от чего, кроме `JetBrains.Annotations`
и `Microsoft.Extensions.DependencyInjection.Abstractions`, — это осознанно: на неё ссылаются
доменные сборки потребителей, которым ни ASP.NET, ни NHibernate не нужны. Тип, которому
нужна внешняя библиотека, в `Abstractions` не место.

`GlavLib.SourceGenerators` собирается под `netstandard2.0` (требование Roslyn) и подключается
к остальным проектам как анализатор (`OutputItemType="Analyzer" ReferenceOutputAssembly="false"`).

## GlavLib.Abstractions

| Каталог | Содержимое |
|---|---|
| `DataTypes/` | `EnumObject`, `IEnumObject<T>`, `EnumObjectItemAttribute` |
| `Db/` | `Entity`, `Entity<TId>` — базовые сущности; версионируемой базы в библиотеке нет |
| `Results/` | `Result<TValue, TError>`, `UnitResult<TError>` |
| `Validation/` | `Error` — структура «сообщение + ключ + код + аргументы», с неявным приведением из `string` |
| `ValueObjects/` | `ValueObject`, `ValueObject<T>` |
| `DI/` | Атрибуты `[SingleInstance]`, `[Transient]`, `[AddServicesFrom]`, `[DomainEventHandler<T>]` |

## GlavLib.Basics

| Каталог | Содержимое |
|---|---|
| `DataTypes/` | `UtcDateTime`, `Date`, `YearMonth`, `Optional<T>`, `SingleValueObject`, `Message` |
| `Serialization/` | `GlavJsonSerializer` и конвертеры `System.Text.Json` для всех типов выше |
| `MultiLang/` | `LanguagePack`, `LanguageContext`, `MultiLangMessage`, `AddMultiLang(...)` |
| `DomainEvents/` | `DomainEvent`, `DomainEventsSession` |
| `Logging/` | `LoggerBuilder` поверх Serilog, обогатитель `ClassNameEnricher` |
| `Extensions/` | Расширения `DateTime`, `Stream` и `WithError(...)` для FluentValidation |
| `_errors/` | `basic.errors.yaml` → класс `GlavLib.Errors.BasicErrors` |

Типы дат намеренно свои: `UtcDateTime` хранит момент времени только в UTC, `Date` — календарную
дату без времени, `YearMonth` — месяц. Смысл в том, чтобы часовой пояс и «дата без времени»
не терялись в `DateTime`, у которого `Kind` теряется при сериализации и на границе базы.
У каждого из них есть JSON-конвертер, Dapper-обработчик и NHibernate user-тип — добавляя такой
тип, доводи набор до конца, иначе он поедет в одном слое и сломается в другом.

### Многоязычные сообщения

Пакет языка — YAML: `Language` плюс список `Bundles` с префиксом и сообщениями. Ключ сообщения
собирается как `<Prefix>.<Key>` и совпадает с ключом ошибки (`<Namespace>.<ClassName>.<Error>`),
который проставляет генератор ошибок. За счёт этого `Error` из любой сборки переводится, не зная
про переводы ничего.

`MultiLangMessage.Format` подставляет аргументы по шаблону `{arg:type}` — с типом, а не просто
`{arg}`; на этом расхождении падают два теста в `GlavLib.Tests` (см. `agent-verification.md`).

## GlavLib.Db

| Каталог | Содержимое |
|---|---|
| `DbSession.cs`, `StatefulDbSession.cs`, `StatelessDbSession.cs` | Сессия запроса, доступ через `Current` |
| `DbTransaction.cs` | Транзакция с явным `Commit()` |
| `Providers/` | `DbSessionFactory`, `NpgsqlDataSourceProvider` |
| `NhConventions/` | Конвенции имён: таблица, колонка, ссылка, Id, коллекции, `enum` |
| `NhUserTypes/` | `UtcDateTimeUserType`, `DateUserType`, `YearMonthUserType`, `EnumObjectUserType`, `JsonType<T>`, `SingleValueObjectType` |
| `Dapper/` | Обработчики типов и расширения для Dapper |
| `Extensions/` | `AddNh(...)`, `Add_GlavLib_Db()`, `UsePostgreSQL()`, `UseDefaults()`, `AddFluentMappings(...)`, `EnumObjectType<T>()` |

Подробности маппинга и работы с сессиями — в `persistence.md` и `nhibernate-models.md`.

## GlavLib.App

| Каталог | Содержимое |
|---|---|
| `Commands/` | `CommandResult<T>`, `CommandUnitResult`, `CommandsFilter`, `LocalizedError` |
| `Db/` | `AddDbSession(<строка подключения>)` — открывает сессию на время запроса |
| `DomainEvents/` | `UseDomainEvents()`, `DomainEventsHandler`, `IDomainEventHandler<T>` |
| `Validation/` | `ErrorResponse`, `ErrorResponseFactory`, `AutoValidationEndpointFilter`, `AcceptLanguageHeaderHelper` |
| `Http/` | `FromJsonQuery<T>`, `FromXml`, `FromEnumObject`, `XmlResult`, заголовки `X-Status` / `X-Debug` |
| `Extensions/` | `AddDefaults()`, `AddAppMetrics(...)`, расширения конфигурации |

### Как устроена команда

Команда — статический метод `ExecuteAsync`, возвращающий `CommandResult<T>`; маршрут вешает
на неё фильтры. Образец — `Sandbox/GlavLib.Sandbox.API`:

```csharp
usersGroup.MapPost("/create", CreateUser.ExecuteAsync)
          .AddDbSession(ConnectionStrings.Master)
          .UseCommands();
```

`UseCommands()` подключает сессию доменных событий и фильтр `CommandsFilter`, который
разбирает результат:

- успех — доменные события фиксируются, в ответ уходит значение и заголовок `X-Status: OK`;
- ошибка — события не фиксируются, в ответ уходит `ErrorResponse` (сообщение, код, ошибки
  по параметрам) и заголовок `X-Status: ERR`.

Сообщение ошибки локализуется по `Accept-Language` через `LanguageContext`, а отладочный текст
(`xDebug`) уходит отдельным заголовком `X-Debug` и в тело ответа не попадает.

Отсюда следует, что HTTP-код у прикладной ошибки остаётся успешным, а признаком ошибки служит
`X-Status`. Клиент, который смотрит только на код ответа, разберёт ошибку как успех — это часть
контракта, а не недосмотр.

Аргументы команды валидируются `FluentValidation`: валидатор объявляется вложенным классом
рядом с аргументами, а `WithError(...)` связывает правило с `Error` из сгенерированного класса.

## Source-генераторы

Все три генератора живут в `GlavLib.SourceGenerators/` и покрыты тестами
`GlavLib.SourceGenerators.Tests/`. Их результат — код в `obj/`, который **не коммитится
и не правится руками**.

| Генератор | Исходник | Что выпускает |
|---|---|---|
| `EnumObjectSourceGenerator` | атрибуты `[EnumObjectItem]` на `partial class ... : EnumObject` | константы `<Имя>Key`, статические экземпляры, `Items`, `Create(key)`, `Equals`/`GetHashCode`, операторы сравнения |
| `ErrorsSourceGenerator` | `_errors/*.errors.yaml` | статический класс с полями `Error`; ключ — `<Namespace>.<ClassName>.<Error>` |
| `ServiceRegistrationSourceGenerator` | атрибуты `[SingleInstance]`, `[Transient]`, `[DomainEventHandler<T>]`, `[AddServicesFrom]` | метод `Add_<Assembly_With_Underscores>(IServiceCollection)` |

Файлы `_errors/*` подключаются к компиляции автоматически: `Directory.Build.targets` добавляет
их в `AdditionalFiles`, отдельной записи в `csproj` не нужно.

Формат файла ошибок:

```yaml
Namespace: GlavLib.Sandbox.API
ClassName: ApiErrors

Errors:
  InvalidOperation: Невозможно выполнить операцию
  UserIsNotFound:
    Code: UNF
    Message: Пользователь#{userId:long} не найден
```

Ошибка без аргументов становится полем `Error`, ошибка с аргументами (`{name:type}`) — методом,
принимающим эти аргументы. `Code` необязателен и попадает в `ErrorResponse.Code` — по нему клиент
различает ошибки, не разбирая текст.

Правка генератора — это правка и его тестов: тесты сравнивают выпущенный код с эталонной строкой
посимвольно, поэтому даже изменение отступов в шаблоне ломает их. Это не помеха, а смысл такой
проверки: выпущенный код — часть контракта.

## Sandbox

Песочница не публикуется в NuGet и существует ради двух вещей: показать, как библиотека
выглядит со стороны потребителя, и дать интеграционным тестам настоящее приложение.
`Program.cs` собирает всё вместе — Serilog, метрики, JSON-настройки, автовалидацию, NHibernate,
пакеты языков — и по нему удобно сверять, как задумано подключать новую возможность.

Проверяя правку в библиотеке, посмотри, не устарела ли песочница: она — единственное место
в репозитории, где код библиотеки используется так, как им пользуются снаружи.

## Сборка и публикация

Пакеты выпускает GitHub Actions: единственный workflow `.github/workflows/publish.yml`
собирает решение, упаковывает пакеты и публикует их в nuget.org. Прежнего пайплайна
TeamCity на Kotlin DSL в репозитории больше нет, как раньше не стало сборки на Nuke.

Workflow запускается на каждый push в любую ветку и вручную (`workflow_dispatch`).
Упаковка идёт всегда, публикация — только из основной ветки либо при ручном запуске:
сборка из рабочей ветки даёт предрелизную версию с суффиксом имени ветки, и выложить её
можно, запустив workflow руками. Готовые `.nupkg` каждый прогон сохраняет артефактом
сборки `packages`, поэтому пакет можно забрать из GitHub, ничего не публикуя.

Сама работа вынесена в скрипты каталога `ci/`, а шаги workflow сводятся к их запуску. Это
сделано затем, чтобы выпуск воспроизводился локально той же командой, что и на раннере:

| Скрипт | Что делает |
|---|---|
| `ci/version.sh` | Печатает версию пакетов. Схема — `<год>.<месяц>.<день>.<номер сборки>`, вне основной ветки добавляется предрелизный суффикс `-<ветка>` |
| `ci/pack.sh` | Собирает и упаковывает решение, складывая `.nupkg` в `artifacts/` |
| `ci/push.sh` | Публикует `artifacts/*.nupkg` командой `dotnet nuget push` |

Каждый скрипт настраивается переменными окружения и имеет разумные значения по умолчанию,
так что `ci/pack.sh` с одним заданным `BUILD_COUNTER` собирает пакеты и на машине разработчика.
Номер сборки workflow берёт из `github.run_number`, ветку — из `github.ref_name`, а таймзону
для даты задаёт переменной `TIMEZONE`: раннеры живут в UTC, и без неё вечерняя сборка
получила бы вчерашнюю дату. Версии пакетов в репозитории руками не проставляются: их даёт
`ci/version.sh`.

### Публикация без постоянного ключа

Пакеты уезжают в nuget.org через trusted publishing, поэтому постоянный ключ доступа
в секретах репозитория не хранится. Шаг `NuGet/login` обменивает OIDC-токен задания
на ключ, который живёт один час, и отдаёт его `ci/push.sh` в переменной `NUGET_API_KEY`.
Для обмена заданию нужно разрешение `id-token: write`, а на стороне nuget.org — политика
доверенной публикации с владельцем репозитория, именем репозитория и именем файла workflow
(`publish.yml`). Отсюда два следствия: переименование файла workflow ломает публикацию, пока
политику не поправят, а имя учётной записи nuget.org берётся из секрета репозитория
`NUGET_USER`. При локальном запуске `ci/push.sh` в `NUGET_API_KEY` подставляют обычный ключ,
выпущенный в личном кабинете.

### Версия .NET SDK

Версия SDK, которым идёт упаковка, задана явно: `GlavLib.SourceGenerators` компилируется
против Roslyn той версии, что указана для `Microsoft.CodeAnalysis.CSharp`
в `Directory.Packages.props`, а компилятор более старой версии отказывается загружать такой
анализатор с ошибкой CS9057. `Microsoft.CodeAnalysis.CSharp` 5.9.0 требует SDK не старее
10.0.400, поэтому эта версия и стоит в переменной `DOTNET_VERSION` workflow, откуда её берёт
`actions/setup-dotnet`. Поднимая версию `Microsoft.CodeAnalysis.CSharp`, поднимай и её.

Проверок на pull request не запускается: workflow занят только выпуском, прогона тестов
в нём нет. Единственная проверка изменения — та, которую агент или разработчик прогнал сам.

## Локальное окружение

`docker-compose.yml` поднимает пару PostgreSQL 16: мастер на `127.100.0.1:5432` и потоковую
реплику на `127.100.0.2:5432`, база `glavdb`, пользователь `sys`. На macOS адреса нужно
предварительно навесить на loopback — `iface-setup.sh`.

| Скрипт | Что делает |
|---|---|
| `up.ps1` / `stop.ps1` / `down.ps1` | Поднять, остановить, удалить окружение вместе с томами |
| `migrate.ps1` | Накатить миграции Liquibase на `glavdb` |
| `rollback.ps1 <n>` | Откатить `n` последних changeset'ов |
| `ci/pack.sh` | Собрать пакеты, результат — в `artifacts/` |

В облачной среде Claude Code Docker и Postgres недоступны, поэтому всё, что требует базы,
там не проверяется — подробности в `agent-verification.md`.
