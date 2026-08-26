# ERP

Табель и отчёт по проектам: .NET 8 API, MongoDB, React.
## Запуск

Нужны Docker и свободные порты 3000, 8080, 27017.

```bash
docker compose up --build --force-recreate --remove-orphans
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

Запрос: контроллер → `IMediator.Send` → `ValidationBehavior` → хендлер. `MongoCollections` — ручки к коллекциям, не кэш базы.

## Правила

- Ставка на дату записи: последняя с `From <= date` (`RateResolver`). Стоимость не хранится, считается при чтении (`TimeEntryMapper` / отчёт — Mongo `$lookup` + `CostStage`).
- Часы записи: > 0, шаг 0.5, ≤ 24. Сумма за день ≤ 24. Переработка: сумма за день > 12.
- Дата должна попадать в период проекта. Закрытый месяц — запись нельзя менять.
- Отчёт: `% = cost / budget * 100`. Перерасход: `cost > budget`. Риск: перерасход или освоение > 80%.

## API

| Метод | Путь |
|---|---|
| GET | `/api/employees`, `/api/projects` |
| PUT | `/api/employees/{id}/rates` |
| GET/POST | `/api/time-entries` |
| PUT/DELETE | `/api/time-entries/{id}` |
| GET | `/api/reports/projects?year=&month=` |
| POST | `/api/periods/close`, `/api/periods/open` |

Ошибки: `{ code, message, validationErrors }`. Бизнес — `BusinessException` (в т.ч. 409 `CONCURRENCY_CONFLICT`).

## Сид

Иванов: 500 ₽ с 01.01.2026, 600 ₽ с 01.03.2026. Петрова: 700 ₽ с 01.02.2026.  
П-001 бюджет 20 000, до 31.03.2026. П-002 бюджет 5 000, без даты окончания.  
Записи: 20.02 Иванов 8 ч; 05.03 Иванов 8 ч; 05.03 Петрова 4 ч; 06.03 Петрова 10 ч.
