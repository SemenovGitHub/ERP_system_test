# ERP System - Clean Architecture

## 🏗️ Архитектурный обзор

Проект следует принципам **Clean Architecture** с четким разделением ответственности между слоями.

## 📁 Структура проекта

```
src/
├── Core/                           # Ядро приложения
│   ├── ERP.Domain/                # Бизнес-логика и правила
│   └── ERP.Application.Abstractions/ # Интерфейсы и контракты
├── Infrastructure/                 # Внешние зависимости
│   ├── ERP.Infrastructure/        # Общая инфраструктура
│   └── ERP.Persistence/          # Доступ к данным
└── Presentation/                   # Представление
    └── ERP.Api/                   # REST API
```

## 🔄 Поток данных и ответственности

### 1. **ERP.Domain** (Ядро)
- **Entities**: Основные бизнес-сущности (`Employee`, `Project`, `TimeEntry`)
- **Value Objects**: Неизменяемые объекты (`Rate`, `Money`)
- **Business Rules**: Бизнес-правила (`BudgetRules`)
- **Exceptions**: Доменные исключения (`BusinessException`)

**Принципы:**
- ❌ Никаких зависимостей от внешних слоев
- ✅ Содержит только чистую бизнес-логику
- ✅ Все правила валидации и расчетов

### 2. **ERP.Application.Abstractions** (Интерфейсы)
- **Repositories**: Интерфейсы доступа к данным
- **Commands/Queries**: CQRS паттерн с MediatR
- **Behaviors**: Pipeline behaviors (логирование, валидация)
- **Models**: DTO и модели запросов/ответов

**Принципы:**
- ✅ Зависит только от Domain
- ✅ Определяет контракты для Infrastructure
- ✅ Содержит абстракции без реализации

### 3. **ERP.Infrastructure** (Реализация)
- **Middleware**: Обработка ошибок (`GlobalExceptionMiddleware`)
- **Behaviors**: Реализация pipeline behaviors
- **Services**: Внешние сервисы и интеграции

**Принципы:**
- ✅ Реализует интерфейсы из Application.Abstractions
- ✅ Содержит кросс-доменную логику

### 4. **ERP.Persistence** (Данные)
- **Repositories**: MongoDB реализации
- **Configuration**: Настройки подключений (`MongoDbSettings`)
- **Documents**: MongoDB схемы данных

**Принципы:**
- ✅ Изолирует детали хранения данных
- ✅ Конфигурируется через appsettings.json
- ✅ Валидация настроек при старте

### 5. **ERP.Api** (Представление)
- **Controllers**: REST API endpoints
- **DTOs**: Модели для API
- **Configuration**: Настройка приложения

## 🔧 Основные паттерны

### CQRS + MediatR
```csharp
// Команда (изменение данных)
public record CreateTimeEntryCommand(Guid EmployeeId, Guid ProjectId, DateOnly Date, decimal Hours) 
    : ICommand<TimeEntryResponse>;

// Запрос (чтение данных)
public record GetTimeEntriesQuery(int Year, int Month, Guid? EmployeeId = null) 
    : IQuery<PagedResponse<TimeEntryDto>>;
```

### Pipeline Behaviors
1. **LoggingBehavior** - логирование всех запросов
2. **ValidationBehavior** - автоматическая валидация с FluentValidation

### Обработка ошибок
```csharp
// В Domain
throw new BusinessException("BUDGET_EXCEEDED", "Project budget exceeded");

// В API
// Автоматически конвертируется в HTTP 400 с JSON ответом
{
  "code": "BUDGET_EXCEEDED",
  "message": "Project budget exceeded"
}
```

### Конфигурация
```json
// appsettings.json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "erp",
    "ConnectionTimeoutSeconds": 30,
    "MaxConnectionPoolSize": 100
  }
}
```

## 🎯 Преимущества архитектуры

### ✅ Separation of Concerns
- **Domain**: Чистая бизнес-логика
- **Application**: Оркестрация и интерфейсы
- **Infrastructure**: Техническая реализация
- **Presentation**: UI/API слой

### ✅ Dependency Inversion
```
API → Infrastructure → Application.Abstractions → Domain
  ↑                                                  ↑
  └─────── Dependencies flow inward ←────────────────┘
```

### ✅ Testability
- Domain легко тестируется (unit tests)
- Infrastructure можно мокать через интерфейсы
- Behaviors тестируются независимо

### ✅ Maintainability
- Изменения в базе данных не влияют на бизнес-логику
- Новые features добавляются через новые handlers
- Конфигурация централизована

## 🚀 Запуск приложения

```bash
# Сборка нового проекта
dotnet build ERP.Clean.sln

# Запуск API
cd src/Presentation/ERP.Api
dotnet run

# Swagger UI будет доступен на
http://localhost:5000/swagger
```

## 📋 Checklist миграции

- [x] ✅ Создана новая структура проектов
- [x] ✅ Перенесен Domain слой с обновленными namespace'ами
- [x] ✅ Созданы абстракции и интерфейсы
- [x] ✅ Настроена конфигурация MongoDB через appsettings
- [x] ✅ Добавлена обработка ошибок через middleware
- [x] ✅ Pipeline behaviors для валидации и логирования
- [ ] 🔄 Перенос существующих handlers
- [ ] 🔄 Реализация MongoDB repositories
- [ ] 🔄 Миграция контроллеров
- [ ] 🔄 Перенос тестов

## 🎓 Следующие шаги

1. **Перенести handlers** из старого Application проекта
2. **Реализовать repositories** с MongoDB
3. **Обновить контроллеры** для использования новых интерфейсов
4. **Настроить Docker** для нового проекта
5. **Написать тесты** для каждого слоя