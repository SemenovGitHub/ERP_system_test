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
