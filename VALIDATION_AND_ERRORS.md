# 🛡️ Валидация и обработка ошибок в ERP системе

## 📋 Обзор системы валидации

Система использует **многоуровневую валидацию** для обеспечения корректности данных и соблюдения бизнес-правил:

```
HTTP Request → Model Binding → FluentValidation → Business Rules → Response
      ↓              ↓              ↓               ↓           ↓
   JSON Schema   Required Fields  Format Rules   Domain Logic  Error JSON
```

## 🔄 Поток валидации и обработки ошибок

### 1. **Уровень ASP.NET Core (Model Binding)**
```csharp
// Автоматическая валидация JSON структуры
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateTimeEntryCommand request)
```

**Что проверяется:**
- Корректность JSON формата
- Соответствие типов данных (Guid, decimal, DateOnly)
- Обязательные поля с `[Required]`

**Ошибки:** HTTP 400 с описанием проблем парсинга

---

### 2. **Уровень FluentValidation (ValidationBehavior)**
```csharp
// Автоматически выполняется для всех команд через Pipeline Behavior
public class CreateTimeEntryCommandValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Hours).Must(HoursRules.IsValidEntryHours);
        // ...
    }
}
```

**Что проверяется:**
- Формат и ограничения полей
- Базовые бизнес-правила (часы кратные 0.5)
- Длина текстовых полей

**Ошибки:** ValidationException → HTTP 400 с детализацией по полям

**Пример ответа:**
```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "validationErrors": {
    "Hours": ["Часы должны быть положительными, кратными 0,5 и не больше 24"],
    "EmployeeId": ["ID сотрудника обязателен"]
  }
}
```

---

### 3. **Уровень бизнес-логики (Domain Rules в Handlers)**
```csharp
public async Task<CreateTimeEntryResponse> Handle(CreateTimeEntryCommand request, CancellationToken cancellationToken)
{
    // 1. Проверка существования сущностей
    var employee = await GetRequiredEmployeeAsync(request.EmployeeId, cancellationToken);
    var project = await GetRequiredProjectAsync(request.ProjectId, cancellationToken);
    
    // 2. Бизнес-правила
    ProjectPeriodRules.EnsureDateFits(project, request.Date);
    RateResolver.Require(employee.Rates, request.Date);
    HoursRules.EnsureDailyLimit(currentHours, request.Hours);
}
```

**Что проверяется:**
- Существование связанных сущностей
- Соответствие бизнес-правилам домена
- Ограничения и инварианты

**Ошибки:** BusinessException → HTTP 400/404 с кодом ошибки

---

### 4. **Global Exception Middleware**
```csharp
public async Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    var (statusCode, response) = exception switch
    {
        ValidationException validationEx => (BadRequest, ErrorResponse with ValidationErrors),
        BusinessException businessEx => (BadRequest, ErrorResponse with Code),
        ArgumentException => (BadRequest, "INVALID_ARGUMENT"),
        _ => (InternalServerError, "INTERNAL_ERROR")
    };
}
```

## 🎯 Типы валидации и их назначение

### ✅ **Input Validation (FluentValidation)**
**Назначение:** Проверка формата и базовых ограничений входных данных

**Примеры:**
```csharp
RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("ID сотрудника обязателен");
RuleFor(x => x.Hours).GreaterThan(0).WithMessage("Часы должны быть положительными");
RuleFor(x => x.Comment).MaximumLength(500);
```

**Где выполняется:** `ValidationBehavior` в MediatR pipeline  
**Результат ошибки:** `ValidationException` → HTTP 400 с `validationErrors`

---

### 🏢 **Business Rules Validation (Domain Rules)**
**Назначение:** Проверка соблюдения бизнес-правил и инвариантов

**Примеры:**
```csharp
// В Domain/Rules/HoursRules.cs
public static void EnsureDailyLimit(decimal hoursAlreadyLogged, decimal hoursToAdd)
{
    var total = hoursAlreadyLogged + hoursToAdd;
    if (total > MaxHoursPerDay)
    {
        throw new BusinessException(
            ErrorCodes.DailyHoursLimit,
            $"Превышен лимит {MaxHoursPerDay} часов в день. Уже: {hoursAlreadyLogged}, добавляется: {hoursToAdd}");
    }
}

// В Domain/Rules/ProjectPeriodRules.cs  
public static void EnsureDateFits(Project project, DateOnly date)
{
    if (date < project.StartDate || date > project.EndDate)
    {
        throw new BusinessException(
            ErrorCodes.ProjectDateOutOfRange,
            $"Дата {date:dd.MM.yyyy} вне периода проекта {project.Code} ({project.StartDate:dd.MM.yyyy} - {project.EndDate:dd.MM.yyyy})");
    }
}
```

**Где выполняется:** В handlers, вызывается из Domain Rules  
**Результат ошибки:** `BusinessException` → HTTP 400 с `code` и `message`

---

### 🔍 **Entity Validation (Repository Level)**
**Назначение:** Проверка существования и доступности сущностей

**Примеры:**
```csharp
private async Task<Employee> GetRequiredEmployeeAsync(Guid employeeId, CancellationToken cancellationToken)
{
    var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
    if (employee == null)
    {
        throw new BusinessException(ErrorCodes.NotFound, $"Сотрудник с ID {employeeId} не найден");
    }
    return employee;
}
```

**Где выполняется:** В handlers при обращении к репозиториям  
**Результат ошибки:** `BusinessException` с `ErrorCodes.NotFound`

## 📝 Коды ошибок (ErrorCodes)

```csharp
// В Domain/Exceptions/ErrorCodes.cs
public static class ErrorCodes
{
    // Общие ошибки
    public const string NotFound = "NOT_FOUND";
    public const string Validation = "VALIDATION_ERROR";
    public const string InvalidArgument = "INVALID_ARGUMENT";
    
    // Бизнес-правила для часов
    public const string InvalidHours = "INVALID_HOURS";
    public const string DailyHoursLimit = "DAILY_HOURS_LIMIT";
    
    // Проекты
    public const string ProjectDateOutOfRange = "PROJECT_DATE_OUT_OF_RANGE";
    public const string ProjectClosed = "PROJECT_CLOSED";
    
    // Сотрудники
    public const string RateNotFound = "RATE_NOT_FOUND";
    public const string EmployeeInactive = "EMPLOYEE_INACTIVE";
    
    // Периоды
    public const string PeriodClosed = "PERIOD_CLOSED";
    public const string PeriodNotFound = "PERIOD_NOT_FOUND";
    
    // Бюджеты  
    public const string BudgetExceeded = "BUDGET_EXCEEDED";
    public const string BudgetNotSet = "BUDGET_NOT_SET";
}
```

## 🚨 Примеры обработки различных ошибок

### 1. **Ошибка валидации формата (FluentValidation)**
```http
POST /api/timeentries
{
  "employeeId": "",
  "projectId": "invalid-guid",
  "date": "2026-08-26", 
  "hours": 25.3
}
```

**Ответ:**
```json
HTTP 400 Bad Request
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "validationErrors": {
    "EmployeeId": ["ID сотрудника обязателен"],
    "ProjectId": ["Некорректный формат GUID"],
    "Hours": ["Часы должны быть положительными, кратными 0,5 и не больше 24"]
  }
}
```

---

### 2. **Ошибка бизнес-правил (BusinessException)**
```http
POST /api/timeentries
{
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "projectId": "33333333-3333-3333-3333-333333333333",
  "date": "2026-12-25",  // Вне периода проекта
  "hours": 8
}
```

**Ответ:**
```json
HTTP 400 Bad Request  
{
  "code": "PROJECT_DATE_OUT_OF_RANGE",
  "message": "Дата 25.12.2026 вне периода проекта П-001 (01.01.2026 - 31.03.2026)"
}
```

---

### 3. **Превышение дневного лимита часов**
```http
POST /api/timeentries
{
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "projectId": "33333333-3333-3333-3333-333333333333", 
  "date": "2026-02-20",
  "hours": 10  // Уже есть 16 часов за день
}
```

**Ответ:**
```json
HTTP 400 Bad Request
{
  "code": "DAILY_HOURS_LIMIT", 
  "message": "Суммарно у сотрудника за день не может быть больше 24 часов. Уже учтено 16, попытка добавить 10 (итого 26)."
}
```

---

### 4. **Сущность не найдена**
```http
POST /api/timeentries
{
  "employeeId": "99999999-9999-9999-9999-999999999999", // Не существует
  "projectId": "33333333-3333-3333-3333-333333333333",
  "date": "2026-02-20", 
  "hours": 8
}
```

**Ответ:**
```json
HTTP 400 Bad Request
{
  "code": "NOT_FOUND",
  "message": "Сотрудник с ID 99999999-9999-9999-9999-999999999999 не найден"
}
```

## 🔧 Как добавить новую валидацию

### 1. **Добавить правило в FluentValidation**
```csharp
// В Infrastructure/Validators/CreateTimeEntryCommandValidator.cs
RuleFor(x => x.NewField)
    .NotEmpty()
    .WithMessage("Новое поле обязательно");
```

### 2. **Добавить бизнес-правило в Domain**
```csharp  
// В Domain/Rules/NewBusinessRules.cs
public static class NewBusinessRules
{
    public static void EnsureNewRule(SomeEntity entity)
    {
        if (!entity.MeetsCondition())
        {
            throw new BusinessException(
                ErrorCodes.NewRuleViolation,
                "Описание нарушения нового правила");
        }
    }
}
```

### 3. **Добавить код ошибки**
```csharp
// В Domain/Exceptions/ErrorCodes.cs
public const string NewRuleViolation = "NEW_RULE_VIOLATION";
```

### 4. **Использовать в Handler**
```csharp
// В Infrastructure/Handlers/SomeHandler.cs
public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
{
    // Validate business rules
    NewBusinessRules.EnsureNewRule(entity);
    
    // Continue processing...
}
```

## 📊 Мониторинг и логирование ошибок

**LoggingBehavior** автоматически логирует:
- Время выполнения каждого запроса
- Успешные операции (INFO level)
- Ошибки с stack trace (ERROR level)

**GlobalExceptionMiddleware** логирует:
- Все необработанные исключения
- Контекст запроса при ошибке
- Детали для отладки

**Пример логов:**
```
2026-08-26 15:09:23 [INFO] Processing request CreateTimeEntryCommand
2026-08-26 15:09:23 [DEBUG] Creating time entry for Employee 11111111-1111-1111-1111-111111111111
2026-08-26 15:09:23 [ERROR] Business rule violation: DAILY_HOURS_LIMIT - Превышен дневной лимит часов
2026-08-26 15:09:23 [INFO] Completed request CreateTimeEntryCommand in 245ms
```

---

**Система валидации обеспечивает:**
- ✅ **Многоуровневую защиту** от некорректных данных
- ✅ **Четкие, понятные ошибки** для клиентов API
- ✅ **Автоматическое применение** через MediatR behaviors
- ✅ **Централизованную обработку** через middleware
- ✅ **Логирование и мониторинг** всех ошибок