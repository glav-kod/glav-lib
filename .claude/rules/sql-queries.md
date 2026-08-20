---
paths:
  - "**/*.sql"
  - "**/*.cs"
---

# SQL Style

## Общие правила

- Все ключевые слова и идентификаторы — **строчными буквами**
- Разделитель слов в именах — `_`
- Схема **всегда** указывается явно: `public.table_name`
- Названия таблиц — **во множественном числе**
- «Главная» таблица в DML-запросе всегда получает алиас `t`

```sql
-- ❌ BAD
SELECT Id, Name FROM Clients WHERE IsDeleted = false
CREATE TABLE clients ( ... );

-- ✅ GOOD
select t.id,
       t.name
  from public.clients t
 where t.is_deleted = false

create table public.clients ( ... );
```

## CREATE TABLE — форматирование

Столбцы выровнены по имени и типу. Constraints — **отдельными клаузами после всех столбцов**, отделены пустой строкой:

```sql
create table public.orders
(
    id          bigserial    not null,
    user_id     bigint       not null,
    merchant_id bigint       not null,

    constraint pk_orders
        primary key (id),

    constraint uq_orders_number
        unique (number),

    constraint "fk_orders#user" foreign key (user_id)
        references public.users (id),

    constraint "fk_orders#merchant" foreign key (merchant_id)
        references public.merchants (id)
);
```

## NULL / NOT NULL — всегда явно

Для каждого столбца всегда явно указывайте `null` или `not null`:

```sql
-- ❌ BAD
create table public.orders
(
    id          bigserial,
    user_id     bigint       not null,
    note        text,
    approved_at timestamp
);

-- ✅ GOOD
create table public.orders
(
    id          bigserial    not null,
    user_id     bigint       not null,
    note        text         null,
    approved_at timestamp    null
);
```

То же правило применяется при добавлении столбца через `alter table ... add column`.

## DEFAULT — не указывать

Не пишите `default` для столбцов — ни в `create table`, ни в `alter table ... add column`.
Значения по умолчанию должны быть явными в коде приложения, а не скрытыми в схеме БД.

```sql
-- ❌ BAD
create table public.orders
(
    id         bigserial    not null,
    version    bigint       not null default 0,
    is_deleted boolean      not null default false
);

-- ✅ GOOD
create table public.orders
(
    id         bigserial    not null,
    version    bigint       not null,
    is_deleted boolean      not null
);
```

При добавлении через `alter table` — отдельным changeset'ом:

```sql
alter table public.orders
    add constraint "fk_orders#user" foreign key (user_id)
        references public.users (id);
```

## Именование constraints

| Тип | Шаблон | Пример |
|-----|--------|--------|
| Primary key | `pk_<table>` | `pk_orders` |
| Foreign key | `"fk_<table>#<field_without_id>"` | `"fk_orders#user"` |
| Unique | `uq_<table>_<fields>` | `uq_orders_number` |

FK: имя поля берётся **без суффикса `_id`**:
- `user_id` → `#user`
- `merchant_id` → `#merchant`
- `created_by_user_id` → `#created_by_user`

FK-имена берутся в **двойные кавычки**, т.к. содержат `#`.

## SELECT — форматирование и отступы

Каждый столбец на своей строке, выровнен под первый столбец (7 пробелов после `select`).
Клаузы `from`, `join`, `where`, `order by` — выровнены правым краем по `select` (отступ 2 пробела для `from`/`where`/`order by`, 1 пробел для `left`/`inner join`).

```sql
select t.id,
       t.name,
       t.create_date_time
  from public.some_table t
  left join public.other_table ot on ot.id = t.other_table_id
                                      and ot.value > 10
 where t.create_date_time > :dateTime
 order by t.create_date_time desc
```

Принцип выравнивания:
- `select` — 6 символов → список столбцов начинается с позиции 8 (пробел + столбец)
- `from  ` — 4 символа + 2 пробела → ровно под `select`
- `left join` — с 2-пробельным отступом, условие `on` — с позиции после `on `
- продолжение `on` (`and`) — выровнено под первое условие `on`
- `where ` — 5 символов + 1 пробел → ровно под `select`
- `order by` — аналогично `where`

## UPDATE — форматирование и отступы

```sql
update public.table1 t
   set create_date_time    = now(),
       schedule_date_time  = now() + '1 day'::interval
 where t.create_date_time > now() - '1 day'::interval
```

- `set` с отступом в 3 пробела, присваиваемые столбцы выровнены
- `where` — 1 пробел отступа (симметрично с `select`)

## INSERT

```sql
insert into public.clients (name, inn, create_date_time)
values (:name, :inn, now())
```

## Параметры запросов

В Dapper/NHibernate использовать именованные параметры через `:paramName`:

```sql
 where t.id = :id
   and t.status = :status
```

## PL/pgSQL функции и процедуры

В PostgreSQL функциях и процедурах используйте префиксы для различения параметров и локальных переменных:

- параметры функции / процедуры — `p_`
- локальные переменные в `declare` — `v_`

```sql
create or replace function public.calculate_balance(
        p_client_id bigint,
        p_date      date
    )
returns numeric
language plpgsql
as $$
declare
    v_balance numeric;
begin
    select coalesce(sum(t.amount), 0)
      into v_balance
      from public.transactions t
     where t.client_id = p_client_id
       and t.date <= p_date;

    return v_balance;
end;
$$;
```

## JOIN условия

Дополнительные условия `and` в `join` выравниваются под начало первого условия после `on`:

```sql
  left join public.documents d on d.client_id = t.id
                              and d.is_deleted = false
```
