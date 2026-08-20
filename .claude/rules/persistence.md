---
paths:
  - "GlavLib.Db/**"
  - "Sandbox/**"
  - "migrations/**"
---

# Персистентность (GlavLib)

Entity Framework Core в проекте **не** используется. Не добавляйте `DbContext`, миграции EF
и атрибуты EF.

Общие соглашения по NHibernate entity/маппингам — в `nhibernate-models.md`.
Оформление Liquibase-миграций — в `liquibase-migrations.md` и `sql-queries.md`.
Устройство сборок — в `project-map.md`.

Особенность репозитория: работа с базой здесь — сама библиотека, а не прикладной домен.
Доменных сущностей у `GlavLib` нет; единственная сущность в репозитории — `User` в песочнице
(`Sandbox/GlavLib.Sandbox.API/Model/User.cs`), и живёт она затем, чтобы на ней проверялись
сессии, транзакции и маппинги.

## Стек

| Слой | Технология |
|------|------------|
| ORM | **FluentNHibernate** (маппинги co-located в entity-классах) |
| БД | **PostgreSQL** (`Npgsql`) |
| Миграции | **Liquibase** SQL (`migrations/`) |
| Ad-hoc чтения | **Dapper** через `dbSession.Connection` |
| Доступ к БД | `DbSessionFactory`, `StatefulDbSession`, `StatelessDbSession`, `DbTransaction` |
| Ошибки | `_errors/*.errors.yaml` → классы `Error` при сборке |

## Project-specific значения (для nhibernate-models)

| Параметр | Значение |
|----------|----------|
| Сборка доступа к БД | `GlavLib.Db` |
| Где лежат сущности | у потребителя библиотеки; в этом репозитории — `Sandbox/GlavLib.Sandbox.API/Model/` |
| Базовый класс | `Entity` / `Entity<TId>` из `GlavLib.Abstractions.Db` |
| Versioned-база | **отсутствует** — `Version(x => x.Version)` в маппинги не добавляется |
| Map class | `NhClassMap` |
| Схема БД | `public` (единственная; `Schema(...)` в маппингах не указывается) |
| Миграции | `migrations/public/` |
| Регистрация NHibernate | `.AddNh(config => config.UsePostgreSQL().UseDefaults().AddFluentMappings("<Assembly>"))` |
| Регистрация сервисов | `services.Add_GlavLib_Db()` |

## Соглашения по именованию — делает конвенция, а не маппинг

`UseDefaults()` подключает набор конвенций из `GlavLib.Db/NhConventions/`, поэтому имена
таблиц и колонок в маппинге обычно писать не нужно:

- `ClassConvention` — имя таблицы: имя класса в `snake_case` и во множественном числе
  (`User` → `users`);
- `PropertyConvention` — имя колонки: имя свойства в `snake_case` (`BirthDate` → `birth_date`);
- `ReferenceConvention` — колонка ссылки: имя свойства в `snake_case` плюс `_id`, ленивая
  загрузка через proxy и `Cascade.SaveUpdate()`;
- `IdConvention` — при незаданном генераторе ставит `Native` с последовательностью
  `<table>_<column>_seq` и `unsaved-value = 0`;
- `EnumConvention` — свойства обычных C#-перечислений (`enum`), через `EnumType<T>`;
- `HasManyConvention`, `HasOneConvention` — коллекции и связи «один к одному»;
- `UserTypesConventions` — пользовательские типы, но не все: см. раздел ниже.

Пиши `Table(...)`, `Column(...)` или `Schema(...)` только тогда, когда имя действительно
расходится с конвенцией. Маппинг, дословно повторяющий её вывод, — лишний код, который
разъедется с конвенцией при следующей правке.

## Пользовательские типы

`UserTypesConventions` подставляет user-тип сам только для `UtcDateTime`, `Date`, `YearMonth`
и `TimeSpan` — эти свойства маппятся обычным `Map(x => x.SomeDateTime)`.

Остальным типам из `GlavLib.Db/NhUserTypes/` конвенции не помогают, и тип указывается явно:

```csharp
Map(x => x.Currency).EnumObjectType<Currency>();     // EnumObject, хранится ключом (строкой)
Map(x => x.Payload).CustomType<JsonType<Payload>>(); // JSON-колонка
Id(x => x.Id).EnumObjectType<ReportTypeId>()
             .GeneratedBy.Assigned();                // EnumObject в качестве Id
```

Расширения `EnumObjectType<T>()` для `Id`, свойства и индекса лежат в
`GlavLib.Db/Extensions/NhExtensions.cs`. `EnumObjectUserType` читает значение по ключу через
`TValue.Create(key)`, поэтому неизвестный ключ в базе даёт исключение, а не `null`.

Для Dapper те же типы подключаются вручную, обработчиками из `GlavLib.Db/Dapper/`. Образец —
`Sandbox/GlavLib.Sandbox.API/Db/DapperConventions.cs`: он же включает
`DefaultTypeMap.MatchNamesWithUnderscores`, без которого `snake_case`-колонки не лягут
на свойства DTO.

## Сессии и транзакции

- Сессия открывается на запрос фильтром `.AddDbSession(<имя строки подключения>)`
  (`GlavLib.App/Db/DbSessionEndpointFilter.cs`) и внутри команды достаётся статически:
  `StatefulDbSession.Current`, `StatefulDbSession.CurrentNhSession`.
- Транзакция — `using var dbTransaction = new DbTransaction();` с явным `Commit()`.
  Не вызванный `Commit()` означает откат, поэтому забытый вызов не «сохранит на всякий случай»,
  а молча потеряет изменения.
- Строка подключения выбирается по имени из `ConnectionStrings` приложения. Реплика — только
  для чтения: маршрут, который пишет, обязан открывать сессию на мастере.
- `StatelessDbSession` берётся для массовых операций без кэша первого уровня.

## Ошибки

Доменные ошибки описываются в `_errors/*.errors.yaml` соответствующего проекта; классы
с полями `Error` выпускает source-генератор, и править их руками нельзя (см. `agent-scope.md`).

## Чеклист новой сущности

1. Entity + вложенный `NhClassMap` (`nhibernate-models.md`)
2. Миграция в `migrations/public/` с числовым префиксом (`liquibase-migrations.md`)
3. Регистрация сборки маппингов через `AddFluentMappings("<Assembly>")` — сам класс маппинга
   подхватывается автоматически
4. При необходимости — ошибки в `_errors/*.errors.yaml`
5. `dotnet build GlavLib.sln -c Debug`

## Типичные ошибки

- Предполагать EF Core, Code First или `[Table]`
- Дублировать в маппинге то, что уже делает конвенция
- Добавлять `Version(x => x.Version)`: версионируемого базового класса в библиотеке нет
- Редактировать сгенерированные классы ошибок вместо YAML
- Писать в базу через сессию, открытую на реплике
- Забыть `dbTransaction.Commit()` и потерять изменения
- Фильтровать LINQ-запрос NHibernate по массиву: `.Where(x => ids.Contains(x.Id))`, где `ids` —
  массив, падает в рантайме с `HibernateException: Evaluation failure on op_Implicit`. У массива
  `Contains` связывается с `MemoryExtensions.Contains(ReadOnlySpan<T>, T)`, и в дерево выражения
  попадает преобразование массива в `ReadOnlySpan`, которое NHibernate не может вычислить
  рефлексией. Держи такие наборы идентификаторов в `List<T>` / `IList<T>` / `HashSet<T>`
