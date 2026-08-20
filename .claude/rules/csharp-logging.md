---
paths:
  - "**/*.cs"
---

# C# Logging Conventions

## Уровни логов

| Метод | Когда использовать |
|---|---|
| `LogError` | Только для пойманных `Exception` |
| `LogWarning` | Ожидаемые / бизнес-ошибки (не исключения) |
| `LogInformation` | Всё остальное: события, состояния, результаты |

```csharp
// ❌ BAD — LogError без exception
_logger.LogError("Payment not found");

// ❌ BAD — LogError для бизнес-ошибки
_logger.LogError("Payment {PaymentId} already processed", paymentId);

// ✅ GOOD
try
{
    await ProcessPaymentAsync(paymentId);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Обработка платежа завершилась ошибкой, Платёж#{PaymentId}", paymentId);
}

// ✅ GOOD — ожидаемая ошибка
if (payment.IsAlreadyProcessed)
{
    _logger.LogWarning("Обработка платежа пропущена, платёж уже обработан, Платёж#{PaymentId}", paymentId);
    return;
}

// ✅ GOOD — информационное событие
_logger.LogInformation("Платёж обработан, Платёж#{PaymentId}", paymentId);
```

## Структура сообщения

Формат для Warning/Error: **`<Что пытались сделать>, <почему не получилось>`**.  
Для Information достаточно описания события.

```csharp
// ✅ GOOD
_logger.LogWarning("Создание заказа отклонено, мерчант не найден, Мерчант#{MerchantId}", merchantId);
_logger.LogError(ex, "Отправка уведомления не удалась, ошибка HTTP-запроса, СистемныйПользователь#{UserId}", userId);
_logger.LogInformation("Заказ создан, Заказ#{OrderId}", orderId);
```

## Язык сообщений

Сообщения пишутся **на русском языке**. Имена placeholder-ов — на английском.

## Обозначение сущностей

Имя сущности — перевод имени класса на русский, слитно, ПаскальКейсом:

| Класс | В логе |
|---|---|
| `SystemUser` | `СистемныйПользователь` |
| `PaymentOrder` | `ПлатёжноеПоручение` |
| `MerchantAccount` | `МерчантАккаунт` |

Идентификатор сущности указывается через `#`, опциональное название — в квадратных скобках:

```
СистемныйПользователь#{SystemUserId}
СистемныйПользователь#{SystemUserId}[{SystemUserName}]
```

```csharp
// ✅ GOOD
_logger.LogWarning(
    "Создание заказа отклонено, Мерчант#{MerchantId} не найден",
    merchantId);

_logger.LogInformation(
    "Платёж обработан, Платёж#{PaymentId}[{PaymentNumber}], СистемныйПользователь#{UserId}[{UserName}]",
    paymentId, paymentNumber, userId, userName);

_logger.LogError(
    ex,
    "Отправка уведомления не удалась, ошибка HTTP-запроса, СистемныйПользователь#{UserId}",
    userId);
```

## Структурированное логирование

Всегда использовать placeholders `{Name}` вместо конкатенации или интерполяции строк.

```csharp
// ❌ BAD
_logger.LogInformation($"Заказ {orderId} создан для пользователя {userId}");
_logger.LogInformation("Заказ " + orderId + " создан");

// ✅ GOOD
_logger.LogInformation("Заказ создан, Заказ#{OrderId}, СистемныйПользователь#{UserId}", orderId, userId);
```
