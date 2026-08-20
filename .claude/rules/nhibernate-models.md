---
paths:
  - "**/*.cs"
---

# NHibernate Models

Это правило описывает общий подход к NHibernate/FluentNHibernate и должно переноситься между проектами без правок.
Project-specific значения (root namespace, schema constant, migration path, registration extension, базовые entity-классы)
бери из persistence-правил текущего проекта и соседних entity-файлов.

## Структура файла

Entity и FluentNHibernate map живут в **одном файле**. Отдельных папок `Mappings/` нет.
Используй имя map-класса из project-specific правила; если оно не задано, предпочитай `NhClassMap`.

```csharp
using FluentNHibernate.Mapping;
using Project.Core.Basics;
using Project.Core.Db;

namespace Project.Core.Orders;

public class Order : Entity                            // или project-specific versioned base class
{
    public virtual string Number { get; protected set; } = null!;

    public virtual long ClientId { get; protected set; }

    protected Order() { }                              // required for NHibernate

    public static Order Create(string number, long clientId)
    {
        return new Order
        {
            Number   = number,
            ClientId = clientId,
        };
    }

    [UsedImplicitly]
    public sealed class NhClassMap : ClassMap<Order>
    {
        public NhClassMap()
        {
            Id(x => x.Id);                             // identity по умолчанию

            Map(x => x.Number);
            Map(x => x.ClientId);
        }
    }
}
```

## Базовые классы

| Класс | Id | Когда |
|---|---|---|
| `Entity` | `long Id` | сущность с суррогатным ключом-счётчиком |
| `Entity<TId>` | типизированный Id | assigned/string Id, справочники и технические сущности |
| `VersionedEntity` / `VersionedEntity<TId>` | Id + `long Version` | только в проектах, где такой базовый класс заведён |

`Entity` и `Entity<TId>` живут в самой библиотеке (`GlavLib.Abstractions.Db`), а версионируемые
базовые классы — нет: оптимистичная блокировка описывается в приложении, поэтому наличие
`VersionedEntity` уточняй в persistence-правиле проекта. Если такой класс есть, добавь в маппинг:

```csharp
Version(x => x.Version);
```

## Свойства и методы

Все `public` и `protected` свойства и методы Entity **обязаны** быть `virtual` — NHibernate требует это для lazy loading и proxy-генерации.

- Свойства: `public virtual` / `protected virtual`
- Сеттеры: `protected set` (rich domain model)
- Методы: `public virtual void ...` / `public virtual T ...`
- Инициализация `= null!` для not-null ссылочных типов
- Коллекции: `IList<T>` или `ISet<T>`, инициализировать в `Create`: `Labels = new List<AgentLabel>()`

Исключения: `static` методы (`Create`, `GetActiveAsync` и т.п.) и вложенные классы (`NhClassMap`) — `virtual` не нужен.

## Id и генерация

```csharp
Id(x => x.Id);                                      // identity (по умолчанию)
Id(x => x.Id).GeneratedBy.Assigned();               // внешний ключ / guid
Id(x => x.Id).EnumObjectType<MyEnumId>()
              .GeneratedBy.Assigned();               // EnumObject-Id
Id(x => x.Id).CustomType<MyId.UserType>()
              .GeneratedBy.Assigned();               // single value object Id
```

## Custom types

```csharp
Map(x => x.Currency);                               // Currency — по convention
Map(x => x.CreatedAt);                              // UtcDateTime — по convention
Map(x => x.ReportType).EnumObjectType<ReportType>();  // EnumObject
Map(x => x.Payload).CustomType<MyDto.UserType>();   // JSON / custom
```

Типы вроде `Currency`, `UtcDateTime`, `Date` и JSON/custom DTO маппятся project-specific conventions/user types.

## Связи

### References (many-to-one)
```csharp
public virtual Client Client { get; protected set; } = null!;

// маппинг:
References(x => x.Client);
// или только FK без навигации:
Map(x => x.ClientId);
```

### HasMany (one-to-many)
```csharp
public virtual IList<OrderLine> Lines { get; protected init; } = null!;

// owned children:
HasMany(x => x.Lines).Cascade.AllDeleteOrphan();

// inverse, если дочерний хранит FK:
HasMany(x => x.Documents)
    .KeyColumn("order_id")
    .Inverse()
    .Cascade.AllDeleteOrphan();

// явное указание таблицы/схемы:
HasMany(x => x.HistoryItems)
    .Schema(DatabaseSchemas.App)
    .KeyColumn("order_id")
    .Cascade.AllDeleteOrphan();

// primitive collection:
HasMany(x => x.Tags)
    .Table("order_tags")
    .KeyColumn("order_id")
    .Element("tag", x => x.Type<string>())
    .Not.Inverse();
```

### HasManyToMany
```csharp
HasManyToMany(x => x.Roles)
    .Schema(DatabaseSchemas.App)
    .Table("user_roles")
    .ParentKeyColumn("user_id")
    .ChildKeyColumn("role_id")
    .Not.Inverse();
```

## Наследование (table-per-hierarchy)

```csharp
// базовый класс:
DiscriminateSubClassesOnColumn("type");

// подкласс — отдельный NhSubclassMap:
public class WorkerService : SystemService
{
    public virtual string? Configuration { get; protected set; }

    [UsedImplicitly]
    public class NhSubclassMap : SubclassMap<WorkerService>
    {
        public NhSubclassMap()
        {
            DiscriminatorValue("worker-service");
            Map(x => x.Configuration, "worker_service_configuration");
        }
    }
}
```

## Component (value object)

```csharp
Component(x => x.Schedule);   // свойства Schedule маппятся в ту же таблицу
```

## Join (secondary table, 1:1)

```csharp
Join("wallet_notifications", t =>
{
    t.Schema(DatabaseSchemas.App);
    t.KeyColumn("wallet_id");
    t.Map(x => x.SentControlVersion).Not.Update();
});
```

## Сохранение агрегатов

Всегда явно вызывать `SaveAsync` или `SaveAndFlushAsync` для агрегатов — и при создании, и после изменения. Не полагаться на автоматический dirty-checking NHibernate или flush по окончании UoW.

```csharp
// ❌ плохо — агрегат создан, но не сохранён явно
var order = Order.Create(number, clientId);

// ❌ плохо — агрегат изменён, но сохранение не вызвано явно
order.UpdateStatus(newStatus, userId);

// ✅ хорошо — создание
var order = Order.Create(number, clientId);
await dbSession.SaveAsync(order);

// ✅ хорошо — изменение
order.UpdateStatus(newStatus, userId);
await dbSession.SaveAsync(order);

// ✅ хорошо — когда нужен немедленный flush (например, для получения Id до конца транзакции)
await dbSession.SaveAndFlushAsync(order);
```

## Read-model (DTO без Entity)

Для NHibernate-запросов без доменных сущностей используй `record` с `ClassMap`:

```csharp
public record OrderRow
{
    public virtual required long Id       { get; init; }

    public virtual required long ClientId { get; init; }

    [UsedImplicitly]
    public sealed class NhClassMap : ClassMap<OrderRow>
    {
        public NhClassMap()
        {
            Table("orders");
            Id(x => x.Id);
            Map(x => x.ClientId);
        }
    }
}
```

## Project-specific значения

Не хардкодь в переносимом правиле значения конкретного проекта. Перед созданием entity уточни в project-specific persistence rule:
- root namespace;
- базовые entity-классы;
- имя вложенного map-класса;
- schema constant;
- migration path;
- registration extension / assembly mapping.

## Миграция БД

После создания сущности нужно добавить миграцию. Правила оформления — в правиле `liquibase-migrations`.

### Обычная сущность

Файл: project-specific path из persistence-правила, например `migrations/<schema>/<NNNNN_table_name>.sql`.

```sql
--liquibase formatted sql

--changeset claude:10
create table public.orders
(
    id         bigserial    not null,
    number     varchar(255) not null,
    client_id  bigint       not null,

    constraint pk_orders primary key (id),
    constraint "fk_orders#client" foreign key (client_id)
        references public.clients (id)
);
--rollback drop table public.orders;
```

### EnumObject

Файл: project-specific path для enum-таблиц из persistence-правила.

```sql
--liquibase formatted sql

--changeset claude:10
create table enums.marital_statuses
(
    key          char(3)      not null,   -- varchar(255) если ключи переменной длины
    display_name varchar(255) not null,

    constraint pk_marital_statuses primary key (key)
);
--rollback drop table enums.marital_statuses;

--changeset claude:20
insert into enums.marital_statuses(key, display_name)
values ('nmr', 'Не женат'),
       ('mar', 'Женат'   ),
       ('div', 'Разведен' ),
       ('wid', 'Вдовец'  );
--rollback ;
```

Значения `key` — **в нижнем регистре**. Они должны совпадать со вторым аргументом `[EnumObjectItem]` в C#-классе:

```csharp
[EnumObjectItem("NotMarried", "nmr", "Не женат")]
public sealed partial class MaritalStatus : EnumObject;
```

## Регистрация сборки

Сборка подключается один раз в composition root (`_Program.cs`, DI extension, bootstrapper). Используй project-specific
registration extension / assembly mapping из persistence-правила.
Новые `NhClassMap`/`NhSubclassMap` подхватываются автоматически — ничего регистрировать вручную не нужно.

## Чеклист новой сущности

- [ ] Наследует `Entity` / `Entity<TId>` (или versioned-базу проекта, если оптимистичная блокировка нужна)
- [ ] Все свойства `public virtual`, сеттеры `protected set`
- [ ] `protected T() { }` конструктор для NHibernate
- [ ] Статический `Create(...)` с блочным телом вместо `public` конструктора
- [ ] Вложенный `[UsedImplicitly] public sealed class NhClassMap : ClassMap<T>`
- [ ] Схема и имена колонок заданы так, как требует persistence-правило проекта (явным `Schema(...)` или соглашениями)
- [ ] Коллекции инициализированы в `Create` через `new List<>()`
- [ ] Написана миграция в project-specific migration path
- [ ] Агрегат сохранён явно через `SaveAsync` / `SaveAndFlushAsync` — и при создании, и после изменения
