# ERP

Табель и отчёт по проектам: .NET 8 API, MongoDB, React.
## Запуск

Нужны Docker и свободные порты 3000, 8080, 27017.

```bash
docker compose up --build --force-recreate --remove-orphans -d
```

- UI: http://localhost:3000
- API / Swagger: http://localhost:8080 / http://localhost:8080/swagger

Сидер каждый такой запуск перезаписывает коллекции. Пересид без полного стека: `docker compose up --force-recreate seeder`. Стоп: `docker compose down -v`.

Локально API: `dotnet run --project Api/Api.csproj`. Фронт: см. `Frontend/README.md`.

## Структура

```
Api/           # HTTP: контроллеры, DTO, AutoMapper, middleware
Domain/        # сущности и правила
Aplication/    # MediatR-хендлеры, валидаторы, маппинг ответов (имя папки историческое)
Repository/    # Mongo
Seeder/Data/   # JSON сида
Frontend/      # React + Zustand
Tests/         # тесты доменных правил
```

Запрос: контроллер → `IMediator.Send` → хендлер → `IDomainValidator<T>.ValidateAsync` на CRUD.

## API

| Метод | Путь |
|---|---|
| GET | `/api/employees`, `/api/projects` |
| PUT | `/api/employees/{id}/rates` |
| GET/POST | `/api/time-entries` |
| PUT/DELETE | `/api/time-entries/{id}` |
| GET | `/api/reports/projects?year=&month=` |
| POST | `/api/periods/close`, `/api/periods/open` |

## Repository

### Обоснование индексов

#### ix_time_entries_month_filters на time_entries: 
Date, потом EmployeeId, потом ProjectId.
Список табеля всегда режется по месяцу, иногда ещё по сотруднику и проекту. Поля в индексе в том же порядке, что и в фильтре: сначала то, что есть всегда (месяц), потом необязательные фильтры. Составной индекс работает с левого края: фильтр только по дате этот индекс тоже использует.

#### ix_time_entries_employee_day на time_entries: 
EmployeeId, потом Date.
Тут сначала известен сотрудник, потом точная дата. Предыдущий индекс начинается с Date, для этого запроса он неудобен, поэтому отдельный.

#### ux_projects_code на projects: поле Code, уникальный.
Код проекта не должен повторяться. Unique-индекс — это поиск и запрет дубля на уровне базы.

#### ux_closed_periods_year_month на closed_periods: 
Year + Month, уникальный индекс.
Один документ = один закрытый месяц. Без unique можно было бы закрыть месяц дважды.

На employees своего индекса нет: сотрудника мы берём по Id, а это уже индекс.
