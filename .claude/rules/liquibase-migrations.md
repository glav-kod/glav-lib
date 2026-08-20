---
paths:
  - "migrations/**/*.sql"
---

# Liquibase SQL Migrations

Миграции этого репозитория обслуживают базу `glavdb`, на которой работают интеграционные тесты
и песочница. Прикладной схемы у библиотеки нет — база нужна только для того, чтобы проверять
`GlavLib.Db` и `Sandbox` на настоящем Postgres.

## Регистрация миграций (`db.changelog.xml`)

Новые SQL-файлы **не нужно** вручную прописывать в `migrations/db.changelog.xml` — changelog
подключает каталог целиком через `<includeAll path="public" relativeToChangelogFile="true"/>`.

Достаточно положить новый `.sql` в `migrations/public/` с корректным именем и changeset'ами.
`db.changelog.xml` редактировать **не нужно**, если не меняется сама схема подключения каталогов.

`includeAll` подхватывает файлы в **алфавитном порядке** имён — поэтому файлы именуются
с числовым префиксом: `00000_users.sql`. Новый файл получает следующий свободный префикс,
чтобы он накатывался после уже существующих.

Схема наката задаётся в `migrations/liquibase.properties` (`defaultSchemaName: public`),
а сам накат выполняется скриптом `migrate.ps1` из корня репозитория; откат — `rollback.ps1`.
Обе команды требуют поднятого Docker-окружения (`up.ps1`) и в облачной среде недоступны.

## Заголовок файла

Каждый файл начинается с:

```sql
--liquibase formatted sql
```

## Changesets

```sql
--changeset claude:10
create table public.orders ( ... );
--rollback drop table public.orders;
```

Номера changeset'ов кратны 10 для удобства вставки.

### Автор changeset'а

В директиве `--changeset <author>:<id>` автором указывается тот, кто пишет миграцию:
разработчик указывает свой логин, а **AI-агент всегда указывает `claude`**.

Правило распространяется на все миграции, созданные агентом, и на changeset'ы, которые агент
дописывает в уже существующий файл: новый changeset получает автора `claude`, даже если
остальные changeset'ы файла написаны человеком. Автора у чужих changeset'ов менять нельзя —
Liquibase считает пару «автор + id» частью идентификатора changeset'а, и его правка приводит
к повторному применению миграции на существующих базах.

```sql
-- ❌ BAD — агент подставил логин разработчика или обезличенный placeholder
--changeset author:10
--changeset omeshechkov:10

-- ✅ GOOD — миграцию написал агент
--changeset claude:10
```

## Правка существующего файла

Уже применённый changeset переписывать нельзя: Liquibase сверяет контрольную сумму и падает
на накате, а откатить применённое изменение он не может. Правка существующего файла допустима
только добавлением нового changeset'а в конец — так и сделано в `00000_users.sql`, где колонка
`birth_date` добавлена отдельным changeset'ом `:20`, а не дописана в `create table`.

## Без комментариев в миграциях

В файлах миграций **запрещены любые комментарии**, кроме системных директив liquibase.
Лишние комментарии ломают миграцию, поэтому в теле `.sql`-файла допустимы только:

- `--liquibase formatted sql` — заголовок файла;
- `--changeset <author>:<id>` — начало changeset'а;
- `--rollback <...>` — откат changeset'а.

Никаких пояснительных `--`-комментариев, блочных `/* ... */` и IDE-хинтов
(`-- noinspection ...`) в миграциях быть не должно. Пояснения к решению —
в описании pull request'а, сообщении коммита или доменной документации,
но не в теле миграции.

```sql
-- ❌ BAD — пояснительный комментарий внутри миграции
--changeset claude:30
-- Колонка нужна для теста ленивой загрузки
alter table public.users
    add column note varchar(255) null;
--rollback ;

-- ✅ GOOD — только системные директивы
--changeset claude:30
alter table public.users
    add column note varchar(255) null;
--rollback ;
```

## Откат

У каждого changeset'а должна быть директива `--rollback`. Если откатывать нечего (вставка
данных, идемпотентная правка), пиши пустой откат `--rollback ;`, а не пропускай директиву:
без неё `rollback.ps1` останавливается на этом changeset'е.

## Общие правила SQL

Форматирование SQL (регистр, схемы, именование таблиц, constraints, FK) — см. правило
**`sql-queries.md`**.

## Таблицы для EnumObject

Отдельной схемы `enums` в этом репозитории нет: справочных таблиц библиотека не заводит,
а `EnumObject` в ней — тип данных, значения которого объявляются атрибутами `[EnumObjectItem]`
в C#-коде (см. `project-map.md`, раздел «Source-генераторы»). Если справочная таблица
понадобится песочнице, она кладётся в тот же каталог `migrations/public/` и оформляется так:

```sql
--liquibase formatted sql

--changeset claude:10
create table public.user_statuses
(
    key          varchar(255) not null,
    display_name varchar(255) not null,

    constraint pk_user_statuses
        primary key (key)
);
--rollback drop table public.user_statuses;

--changeset claude:20
insert into public.user_statuses(key, display_name)
values ('act', 'Активен' ),
       ('bl',  'Заблокирован');
--rollback ;
```

Значения `key` обязаны совпадать со вторым аргументом `[EnumObjectItem]` в C#-классе
**посимвольно, включая регистр**: по этому ключу `EnumObjectUserType` находит значение
при чтении из базы, а `Create(key)` на неизвестном ключе бросает исключение.
