# Команды для Docker Compose

## 🚀 Основные команды

### Первый запуск:
```bash
docker compose up --build --force-recreate --remove-orphans
```

### API доступен на:
- http://localhost:8080
- Swagger: http://localhost:8080/swagger

### Остановка:
```bash
Ctrl+C
```
или
```bash
docker compose down -v
```

### Следующий запуск:
```bash
docker compose up --build --force-recreate --remove-orphans
```

## 📋 Дополнительные команды

### Остановка с полной очисткой:
```bash
docker compose down --remove-orphans --volumes --rmi local
```

### Просмотр логов:
```bash
docker compose logs
```

### Только сборка без запуска:
```bash
docker compose build
```
