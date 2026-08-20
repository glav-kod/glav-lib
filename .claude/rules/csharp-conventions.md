---
paths:
  - "**/*.cs"
---

# C# Code Conventions

## Object Initializers

Каждое свойство в object initializer — на отдельной строке.

```csharp
// ❌ BAD
var order = new Order { Id = 1, Amount = 100, Status = OrderStatus.New };

// ✅ GOOD
var order = new Order
{
    Id = 1,
    Amount = 100,
    Status = OrderStatus.New
};
```

## Фигурные скобки в условных операторах

Если тело `if`, `else`, `for`, `foreach`, `while` и т.д. занимает более одной строки — фигурные скобки обязательны.

```csharp
// ❌ BAD
if (condition)
    DoFirstThing();
    DoSecondThing(); // выполняется всегда, несмотря на отступ

// ✅ GOOD
if (condition)
{
    DoFirstThing();
    DoSecondThing();
}
```

Однострочные тела допустимы без фигурных скобок **только если тело занимает ровно одну строку**:

```csharp
// ✅ OK — одна строка
if (user == null)
    return NotFound();

// ❌ BAD — тело визуально многострочное (object initializer), фигурные скобки обязательны
if (ctx is not null)
    return new Config
    {
        AgentId    = ctx.AgentId,
        MerchantId = ctx.MerchantId
    };

// ✅ GOOD
if (ctx is not null)
{
    return new Config
    {
        AgentId    = ctx.AgentId,
        MerchantId = ctx.MerchantId
    };
}
```

## Структура класса

### Порядок элементов

1. `public`/`protected` константы
2. `private` константы
3. `private static` свойства
4. `public`/`protected static` свойства
5. Статический конструктор
6. `public`/`protected` свойства
7. `private` поля
8. Конструктор(ы)
9. Статический фабричный метод `Create`
10. `public`/`protected static` методы
11. `public`/`protected` методы экземпляра
12. `private` методы экземпляра
13. `private static` методы

### Пустые строки между блоками

Между каждым элементом (полем, свойством, методом, конструктором) обязательно должна быть пустая строка.

```csharp
// ❌ BAD
public class Order
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    private readonly ILogger _logger;
    public Order(ILogger logger)
    {
        _logger = logger;
    }
    public void Apply(decimal amount)
    {
        Amount = amount;
    }
}

// ✅ GOOD
public class Order
{
    public Guid Id { get; private set; }

    public decimal Amount { get; private set; }

    private readonly ILogger _logger;

    public Order(ILogger logger)
    {
        _logger = logger;
    }

    public void Apply(decimal amount)
    {
        Amount = amount;
    }
}
```

### Поле рядом со свойством

Если у свойства есть backing field, оно объявляется непосредственно перед этим свойством (не в общем блоке приватных полей).

```csharp
// ❌ BAD
public class Order
{
    private decimal _amount;
    private string _note;

    public decimal Amount => _amount;
    public string Note => _note;
}

// ✅ GOOD
public class Order
{
    public const string DefaultCurrency = "KZT";

    private const int MaxItems = 100;

    public static OrderStatus DefaultStatus => OrderStatus.New;

    public Guid Id { get; private set; }

    private decimal _amount;
    public decimal Amount
    {
        get { return _amount; }
        private set { _amount = value; }
    }

    private string _note;
    public string Note
    {
        get { return _note; }
        private set { _note = value; }
    }

    // приватные поля без свойств — здесь
    private readonly ILogger _logger;

    public Order(ILogger logger)
    {
        _logger = logger;
    }

    public static Order Create(decimal amount, string note)
    {
        return new Order { _amount = amount, _note = note };
    }

    public void Apply(decimal newAmount)
    {
        _amount = newAmount;
    }

    private void Validate()
    {
        // ...
    }

    private static bool IsValidAmount(decimal amount)
    {
        return amount > 0;
    }
}
```

## Методы в классах

Методы всегда объявляются в блочном виде с фигурными скобками. Лямбда-методы (`=>`) запрещены.

```csharp
// ❌ BAD
public int GetTotal() => _items.Sum(x => x.Price);

public void Reset() => _items.Clear();

// ✅ GOOD
public int GetTotal()
{
    return _items.Sum(x => x.Price);
}

public void Reset()
{
    _items.Clear();
}
```

## Порядок объявления переменных в методе

Переменные объявляются в следующем порядке:

1. **Текущие контексты** (`Current`, `DbSession.Current` и т.п.) — в самом начале, сигнализируют о зависимостях метода.
2. **«Глобальные» значения** — дата/время и прочее, что может понадобиться в любой точке метода.
3. **Остальные переменные** — ближе к месту использования.

```csharp
public async Task SomeMethod(CancellationToken cancellationToken)
{
    var dbSession  = StatefulDbSession.Current;   // метод работает с БД
    var nhSession  = dbSession.NhSession;         // метод использует NHibernate

    var now = _dateTimeProvider.Now();    // может понадобиться в любой момент

    // ... остальные переменные объявляются ближе к использованию
    var user = await nhSession.GetAsync<User>(userId, cancellationToken);
    var name = user.Name;
}
```

## Dapper-запросы (Query...Async)

Приватные методы чтения данных через Dapper именуются `Query...Async` и имеют фиксированную структуру тела: сначала SQL и параметры, затем инфраструктура подключения, затем выполнение.

```csharp
private static async Task<List<UserRow>> QuerySomeUsersAsync(
    DbSession dbSession,
    UtcDateTime dateTime,
    CancellationToken cancellationToken)
{
    // 1. SQL-запрос
    const string sql = @"
select t.id,
       t.name
  from public.users t
 where t.create_date_time > :dateTime
";

    // 2. Параметры — рядом с запросом
    var parameters = new
    {
        dateTime
    };

    // 3. Подключение и транзакция
    var dbConnection  = dbSession.Connection;
    var dbTransaction = dbSession.Transaction; // только для запросов, изменяющих данные

    // 4. CommandDefinition — чтобы передать cancellationToken
    var commandDefinition = new CommandDefinition(
        commandText: sql,
        parameters: parameters,
        transaction: dbTransaction,
        cancellationToken: cancellationToken
    );

    // 5. Выполнение
    var rows = await dbConnection.QueryAsync<UserRow>(commandDefinition);

    return rows.ToList();
}
```

## Цепочки вызовов

Избегай обращения к свойству или методу напрямую через результат другого вызова. Сохраняй результат в переменную.

```csharp
// ❌ BAD
var name = GetUser().Name;
var count = GetItems().Count;
ProcessOrder(GetCart().TotalAmount);

// ✅ GOOD
var user = GetUser();
var name = user.Name;

var items = GetItems();
var count = items.Count;

var cart = GetCart();
ProcessOrder(cart.TotalAmount);
```

## Именование свойств с датой и временем

Свойства типа `UtcDateTime` и `Date` именуются с суффиксом **`DateTime`** / **`Date`** соответственно.
В качестве префикса используется **глагол в форме императива** (инфинитив без «to»).

```csharp
// ❌ BAD
public virtual UtcDateTime CreatedAt    { get; protected set; }
public virtual UtcDateTime BlockedOn    { get; protected set; }
public virtual Date        BirthDay     { get; protected set; }

// ✅ GOOD
public virtual UtcDateTime CreateDateTime { get; protected set; }
public virtual UtcDateTime BlockDateTime  { get; protected set; }
public virtual Date        BirthDate      { get; protected set; }
```

## Именование параметров с датой и временем

Параметры методов **нельзя** называть `now` или `today`. Используй `dateTime` или `date`.

Имена `now` и `today` допустимы **только** как локальные переменные, получаемые внутри метода из `dateTimeProvider`.

```csharp
// ❌ BAD
public void Process(UtcDateTime now) { ... }
public void Expire(Date today) { ... }

// ✅ GOOD
public void Process(UtcDateTime dateTime) { ... }
public void Expire(Date date) { ... }

// ✅ GOOD — локальные переменные из dateTimeProvider
public async Task RunAsync()
{
    var now   = _dateTimeProvider.Now();
    var today = _dateTimeProvider.Today();
    ...
}
```

## Ключи EnumObject в SQL-строках

В SQL-строках используй ключи из `EnumObject` через строковую интерполяцию, а не строковые литералы.

```csharp
// ❌ BAD
const string sql = @"
select t.id
  from public.users t
 where t.status = 'active'
";

// ✅ GOOD
var sql = @$"
select t.id
  from public.users t
 where t.status = {UserStatus.ActiveKey}
";
```

## Именованные аргументы в вызовах методов

### Константы в аргументах

Если передаётся литерал (число, булево значение, `null`, строка) — аргумент должен быть именованным.

```csharp
// ❌ BAD
CreateUser("admin", true, 30);
SetTimeout(5000);

// ✅ GOOD
CreateUser("admin", isActive: true, age: 30);
SetTimeout(milliseconds: 5000);
```

### Более двух аргументов

Если метод вызывается с тремя и более аргументами — каждый аргумент именуется и пишется на отдельной строке.

```csharp
// ❌ BAD
CreateOrder(userId, amount, currency, OrderStatus.New);

// ✅ GOOD
CreateOrder(
    userId: userId,
    amount: amount,
    currency: currency,
    status: OrderStatus.New
  );
```
