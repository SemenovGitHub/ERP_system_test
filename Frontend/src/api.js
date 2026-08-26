export async function apiRequest(path, options = {}) {
  const response = await fetch(path, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers || {})
    },
    ...options
  });

  if (response.status === 204) {
    return null;
  }

  const text = await response.text();
  let body = null;
  if (text) {
    try {
      body = JSON.parse(text);
    } catch {
      body = { message: text.slice(0, 400) };
    }
  }

  if (!response.ok) {
    throw toApiError(response.status, body);
  }

  return body;
}

export const NO_RATE_MESSAGE =
  'На дату записи у сотрудника нет ни одной ставки. Запись создать нельзя.';

export function hasRateOnDate(employee, date) {
  const iso = String(date || '').slice(0, 10);
  if (!employee || !iso) {
    return false;
  }
  return (employee.rates || []).some((rate) => String(rate.from).slice(0, 10) <= iso);
}

function collectFieldMessages(fields) {
  if (!fields || typeof fields !== 'object' || Array.isArray(fields)) {
    return [];
  }
  return Object.entries(fields).flatMap(([, messages]) => {
    if (Array.isArray(messages)) {
      return messages.filter(Boolean);
    }
    return messages ? [String(messages)] : [];
  });
}

export function toApiError(status, body) {
  const fields = body?.validationErrors || body?.errors || {};
  const message = [body?.message, body?.title, body?.detail, ...collectFieldMessages(fields)]
    .filter(Boolean)
    .filter((item, index, list) => list.indexOf(item) === index)
    .join('\n');

  const error = new Error(message || `Не удалось сохранить запись. Код ${status}.`);
  error.status = status;
  error.code = body?.code;
  error.fields = typeof fields === 'object' && !Array.isArray(fields) ? fields : {};
  return error;
}

export function formatMoney(value) {
  return Number(value || 0).toLocaleString('ru-RU', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });
}

export function formatHours(value) {
  return Number(value || 0).toLocaleString('ru-RU', {
    minimumFractionDigits: 1,
    maximumFractionDigits: 2
  });
}

export function formatDate(value) {
  if (!value) {
    return '';
  }
  const iso = String(value).slice(0, 10);
  const [year, month, day] = iso.split('-');
  return `${day}.${month}.${year}`;
}

export function toDateInput(value) {
  return String(value || '').slice(0, 10);
}

export function monthToParts(monthValue) {
  const [year, month] = monthValue.split('-').map(Number);
  return { year, month };
}

export function partsToMonth(year, month) {
  return `${year}-${String(month).padStart(2, '0')}`;
}

export function isValidEntryHours(hours) {
  return hours > 0 && hours <= 24 && Number.isInteger(hours * 2);
}

export function hoursValidationMessage() {
  return 'Часы должны быть положительными, кратными 0,5 и не больше 24.';
}

export function fieldMessages(fieldErrors, name) {
  return fieldErrors?.[name] || fieldErrors?.[name.charAt(0).toLowerCase() + name.slice(1)] || [];
}
