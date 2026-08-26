# Code review

Файл: `TimesheetReportHandler.cs`. По важности.

---

### 1. CancellationToken никуда не передаётся

`Handle` принимает `CancellationToken token`, но ни в один вызов драйвера он не попадает: ни в `ToListAsync()`, ни в `FirstOrDefaultAsync()`.

Клиент отменил запрос — чтение Mongo всё равно идёт до конца. Токен нужно передавать в каждый async-метод.

```csharp
.ToListAsync(token)
.FirstOrDefaultAsync(token)
```

### 2. `FirstOrDefaultAsync().Result` вместо `await`

```csharp
var employee = _db.GetCollection<Employee>("employees")
    .Find(e => e.Id == entry.EmployeeId)
    .FirstOrDefaultAsync().Result;
```

`FirstOrDefaultAsync()` возвращает `Task`. `.Result` блокирует поток. Асинхронность ломается, в ASP.NET — голод пула и deadlock.

Нужно: `await ...FirstOrDefaultAsync(token)`.

### 3. В память грузится вся коллекция

`year` и `month` есть в запросе, в Mongo не используются:

```csharp
.Find(FilterDefinition<TimeEntry>.Empty).ToListAsync()
// потом
.Where(e => e.Date.Year == request.Year && e.Date.Month == request.Month)
```

Фильтр месяца должен быть в `Find`, не после выгрузки всех `time_entries`.

### 4. `FirstOrDefault().Value` без проверки на null

```csharp
var rate = employee.Rates.FirstOrDefault().Value;
```

`FirstOrDefault()` на списке `Rate` даёт `null`, если ставок нет. Сразу `.Value` — `NullReferenceException`, 500. Нужна проверка:

```csharp
var rate = employee.Rates?.FirstOrDefault();
if (rate is null)
{
    continue;
}
var amount = Math.Round(entry.Hours * rate.Value, 2);
```

То же для `employee` после `FirstOrDefaultAsync`: сотрудника может не быть.

---

Исправление: `TimesheetReportHandler.fixed.cs`.
