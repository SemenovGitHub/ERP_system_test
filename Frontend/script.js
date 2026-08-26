const API_BASE_URL = '/api';

// Вспомогательные функции
function showLoading(elementId) {
    document.getElementById(elementId).innerHTML = '<div class="loading">Загрузка...</div>';
}

function showError(elementId, message) {
    document.getElementById(elementId).innerHTML = `<div class="error">Ошибка: ${message}</div>`;
}

function showSuccess(elementId, message) {
    document.getElementById(elementId).innerHTML = `<div class="success">${message}</div>`;
}

async function apiRequest(url, options = {}) {
    try {
        const response = await fetch(url, {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        });
        
        if (!response.ok) {
            const errorData = await response.text();
            throw new Error(`HTTP ${response.status}: ${errorData}`);
        }
        
        return await response.json();
    } catch (error) {
        throw new Error(`Ошибка сети: ${error.message}`);
    }
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('ru-RU');
}

function createTable(headers, rows) {
    let html = '<table><thead><tr>';
    headers.forEach(header => {
        html += `<th>${header}</th>`;
    });
    html += '</tr></thead><tbody>';
    
    rows.forEach(row => {
        html += '<tr>';
        row.forEach(cell => {
            html += `<td>${cell || '-'}</td>`;
        });
        html += '</tr>';
    });
    
    html += '</tbody></table>';
    return html;
}

function createPaginationInfo(currentPage, pageSize, totalCount) {
    const totalPages = Math.ceil(totalCount / pageSize);
    return `<div class="pagination-info">
        Страница ${currentPage} из ${totalPages} | 
        Показано ${Math.min(pageSize, totalCount - (currentPage - 1) * pageSize)} из ${totalCount} записей
    </div>`;
}

// Функции для работы с сотрудниками
async function loadEmployees() {
    const page = parseInt(document.getElementById('employeePage').value) || 1;
    const pageSize = parseInt(document.getElementById('employeePageSize').value) || 10;
    
    showLoading('employeesResult');
    
    try {
        const url = `${API_BASE_URL}/employees?page=${page}&pageSize=${pageSize}`;
        const data = await apiRequest(url);
        
        const headers = ['ID', 'ФИО', 'Отдел'];
        const rows = data.items.map(emp => [
            emp.id,
            emp.fullName,
            emp.department
        ]);
        
        const table = createTable(headers, rows);
        const pagination = createPaginationInfo(page, pageSize, data.totalCount);
        
        document.getElementById('employeesResult').innerHTML = pagination + table;
    } catch (error) {
        showError('employeesResult', error.message);
    }
}

// Функции для работы с проектами
async function loadProjects() {
    const page = parseInt(document.getElementById('projectPage').value) || 1;
    const pageSize = parseInt(document.getElementById('projectPageSize').value) || 10;
    
    showLoading('projectsResult');
    
    try {
        const url = `${API_BASE_URL}/projects?page=${page}&pageSize=${pageSize}`;
        const data = await apiRequest(url);
        
        const headers = ['ID', 'Код', 'Название', 'Бюджет', 'Дата начала', 'Дата окончания'];
        const rows = data.items.map(proj => [
            proj.id,
            proj.code,
            proj.name,
            proj.budget ? `$${proj.budget.toLocaleString()}` : 'Не указан',
            proj.startDate ? formatDate(proj.startDate) : 'Не указана',
            proj.endDate ? formatDate(proj.endDate) : 'Не указана'
        ]);
        
        const table = createTable(headers, rows);
        const pagination = createPaginationInfo(page, pageSize, data.totalCount);
        
        document.getElementById('projectsResult').innerHTML = pagination + table;
    } catch (error) {
        showError('projectsResult', error.message);
    }
}

// Функции для работы с записями времени
async function loadTimeEntries() {
    const year = parseInt(document.getElementById('timeEntryYear').value);
    const month = parseInt(document.getElementById('timeEntryMonth').value);
    const page = parseInt(document.getElementById('timeEntryPage').value) || 1;
    const pageSize = parseInt(document.getElementById('timeEntryPageSize').value) || 10;
    
    showLoading('timeEntriesResult');
    
    try {
        const url = `${API_BASE_URL}/time-entries?year=${year}&month=${month}&page=${page}&pageSize=${pageSize}`;
        const data = await apiRequest(url);
        
        const headers = ['ID', 'Сотрудник', 'Проект', 'Дата', 'Часы', 'Комментарий'];
        const rows = data.items.map(entry => [
            entry.id,
            entry.employeeFullName,
            entry.projectName,
            formatDate(entry.date),
            entry.hours,
            entry.comment
        ]);
        
        const table = createTable(headers, rows);
        const pagination = createPaginationInfo(page, pageSize, data.totalCount);
        
        document.getElementById('timeEntriesResult').innerHTML = pagination + table;
    } catch (error) {
        showError('timeEntriesResult', error.message);
    }
}

// Функции для отчетов
async function loadProjectReport() {
    const year = parseInt(document.getElementById('reportYear').value);
    const month = parseInt(document.getElementById('reportMonth').value);
    
    showLoading('reportResult');
    
    try {
        const url = `${API_BASE_URL}/reports/projects?year=${year}&month=${month}`;
        const data = await apiRequest(url);
        
        const headers = ['Проект', 'Общие часы', 'Стоимость', 'Бюджет', 'Статус бюджета'];
        const rows = data.map(report => [
            report.projectName,
            report.totalHours,
            `$${report.totalCost.toLocaleString()}`,
            report.budget ? `$${report.budget.toLocaleString()}` : 'Не указан',
            report.isOverBudget ? 'Превышен' : 'В рамках бюджета'
        ]);
        
        const table = createTable(headers, rows);
        
        document.getElementById('reportResult').innerHTML = table;
    } catch (error) {
        showError('reportResult', error.message);
    }
}

// Функции для создания записи времени
async function createTimeEntry() {
    const employeeId = document.getElementById('createEmployeeId').value.trim();
    const projectId = document.getElementById('createProjectId').value.trim();
    const date = document.getElementById('createDate').value;
    const hours = parseFloat(document.getElementById('createHours').value);
    const comment = document.getElementById('createComment').value.trim();
    
    if (!employeeId || !projectId || !date || !hours) {
        showError('createResult', 'Все поля обязательны для заполнения');
        return;
    }
    
    showLoading('createResult');
    
    try {
        const requestBody = {
            employeeId,
            projectId,
            date,
            hours,
            comment: comment || 'Без комментария'
        };
        
        await apiRequest(`${API_BASE_URL}/time-entries`, {
            method: 'POST',
            body: JSON.stringify(requestBody)
        });
        
        showSuccess('createResult', 'Запись времени успешно создана!');
        
        // Очистка формы
        document.getElementById('createEmployeeId').value = '';
        document.getElementById('createProjectId').value = '';
        document.getElementById('createDate').value = '';
        document.getElementById('createHours').value = '';
        document.getElementById('createComment').value = '';
        
    } catch (error) {
        showError('createResult', error.message);
    }
}

// Автоматическая загрузка данных при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    // Устанавливаем сегодняшнюю дату по умолчанию для создания записи
    const today = new Date().toISOString().split('T')[0];
    document.getElementById('createDate').value = today;
    
    // Загружаем начальные данные
    loadEmployees();
    loadProjects();
});