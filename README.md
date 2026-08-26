# ERP System

Система управления рабочим временем и проектами на .NET 8, MongoDB и Docker.

## 🚀 Быстрый запуск

### Требования
- Docker и Docker Compose
- Порты 8080 и 27017 должны быть свободны

### Запуск системы

1. **Первый запуск:**
   ```bash
   docker compose up --build --force-recreate --remove-orphans
   ```

2. **Доступные сервисы:**
   - **Фронтенд**: http://localhost:3000
   - **API**: http://localhost:8080
   - **Swagger UI**: http://localhost:8080/swagger

3. **Останавливаем:** 
   ```bash
   Ctrl+C
   ```
   или
   ```bash
   docker compose down -v
   ```

4. **Следующий запуск:** снова команда из п.1
   ```bash
   docker compose up --build --force-recreate --remove-orphans
   ```

### Что происходит при запуске
1. 🐳 Запускается MongoDB контейнер
2. 🌱 Сидер заполняет базу тестовыми данными
3. 🚀 API поднимается и готов к работе
4. 🌐 Фронтенд запускается и подключается к API

### Тестовые данные
После запуска в базе будут:
- 15 сотрудников 
- 6 проектов
- 30 записей времени за 2026 год

### Основные функции
#### Через веб-интерфейс (http://localhost:3000):
- 👥 Просмотр сотрудников с пагинацией
- 📁 Просмотр проектов с пагинацией  
- ⏱️ Просмотр записей времени по периодам
- 📊 Отчеты по проектам за месяц/год
- ➕ Создание новых записей времени

#### API Endpoints (http://localhost:8080/api):
- `GET /api/employees` - список сотрудников (с пагинацией)
- `GET /api/projects` - список проектов (с пагинацией)
- `GET /api/time-entries?year=2026&month=3` - записи времени за март 2026
- `POST /api/time-entries` - создание записи времени
- `GET /api/reports/projects?year=2026&month=3` - отчет по проектам за март 2026

## 🛠 Разработка

### Структура проекта
```
ERP_system_test/
├── ERP/                 # Основное API приложение
├── Domain/              # Бизнес-логика и правила
├── Aplication/          # Обработчики команд и запросов
├── Repository/          # Доступ к данным MongoDB
├── Seeder/              # Заполнение тестовыми данными
├── Frontend/            # Веб-интерфейс (HTML/CSS/JS)
└── compose.yaml         # Docker Compose конфигурация
```

### Docker команды
```bash
# Полный перезапуск с пересборкой
docker compose up --build --force-recreate --remove-orphans

# Остановка с очисткой данных
docker compose down -v

# Просмотр логов
docker compose logs

# Только сборка без запуска
docker compose build
```