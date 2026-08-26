import { create } from 'zustand';
import { apiRequest, hasRateOnDate, hoursValidationMessage, isValidEntryHours, monthToParts, NO_RATE_MESSAGE } from './api';

const DEFAULT_MONTH = '2026-03';
const IVANOV_ID = '11111111-1111-1111-1111-111111111111';

function emptyForm() {
  return {
    employeeId: '',
    projectId: '',
    date: `${DEFAULT_MONTH}-01`,
    hours: '8',
    comment: ''
  };
}

function ratesFormFromEmployee(employee) {
  const rates = (employee?.rates || [])
    .map((rate) => ({
      from: String(rate.from).slice(0, 10),
      value: String(rate.value)
    }))
    .sort((a, b) => b.from.localeCompare(a.from));
  return {
    employeeId: employee?.id || '',
    rates: rates.length > 0 ? rates : [{ from: `${DEFAULT_MONTH}-01`, value: '' }]
  };
}

export const useStore = create((set, get) => ({
  screen: 'timesheet',
  month: DEFAULT_MONTH,
  employeeId: '',
  projectId: '',
  employees: [],
  projects: [],
  entries: [],
  totals: { hours: 0, cost: 0 },
  reportItems: [],
  reportTotal: { hours: 0, cost: 0 },
  loading: false,
  pageError: '',
  modal: null,
  form: emptyForm(),
  ratesForm: ratesFormFromEmployee(null),
  formError: '',
  fieldErrors: {},
  saving: false,

  setScreen: (screen) => {
    set({ screen, pageError: '' });
    if (screen === 'timesheet') {
      get().loadTimesheet();
    } else {
      get().loadReport();
    }
  },

  setMonth: (month) => {
    set({ month });
    const { screen } = get();
    if (screen === 'timesheet') {
      get().loadTimesheet();
    } else {
      get().loadReport();
    }
  },

  setEmployeeId: (employeeId) => {
    set({ employeeId });
    get().loadTimesheet();
  },

  setProjectId: (projectId) => {
    set({ projectId });
    get().loadTimesheet();
  },

  setFormField: (name, value) => {
    set((state) => ({
      form: { ...state.form, [name]: value },
      fieldErrors: { ...state.fieldErrors, [name]: undefined, [name.charAt(0).toUpperCase() + name.slice(1)]: undefined }
    }));
  },

  setRatesEmployee: (employeeId) => {
    const employee = get().employees.find((item) => item.id === employeeId);
    set({ ratesForm: ratesFormFromEmployee(employee), formError: '' });
  },

  setRateRow: (index, name, value) => {
    set((state) => ({
      ratesForm: {
        ...state.ratesForm,
        rates: state.ratesForm.rates.map((rate, rowIndex) =>
          rowIndex === index ? { ...rate, [name]: value } : rate)
      }
    }));
  },

  addRateRow: () => {
    set((state) => ({
      ratesForm: {
        ...state.ratesForm,
        rates: [...state.ratesForm.rates, { from: `${get().month}-01`, value: '' }]
      }
    }));
  },

  removeRateRow: (index) => {
    set((state) => ({
      ratesForm: {
        ...state.ratesForm,
        rates: state.ratesForm.rates.filter((_, rowIndex) => rowIndex !== index)
      }
    }));
  },

  bootstrap: async () => {
    try {
      const [employeesPage, projectsPage] = await Promise.all([
        apiRequest('/api/employees?page=1&pageSize=100'),
        apiRequest('/api/projects?page=1&pageSize=100')
      ]);
      set({
        employees: employeesPage.items || [],
        projects: projectsPage.items || []
      });
      await get().loadTimesheet();
    } catch (error) {
      set({ pageError: error.message });
    }
  },

  loadTimesheet: async () => {
    const { month, employeeId, projectId } = get();
    const { year, month: monthNumber } = monthToParts(month);
    const params = new URLSearchParams({
      year: String(year),
      month: String(monthNumber),
      page: '1',
      pageSize: '100'
    });
    if (employeeId) {
      params.set('employeeId', employeeId);
    }
    if (projectId) {
      params.set('projectId', projectId);
    }

    set({ loading: true, pageError: '' });
    try {
      const data = await apiRequest(`/api/time-entries?${params}`);
      set({
        entries: data.items || [],
        totals: { hours: data.totalHours || 0, cost: data.totalCost || 0 },
        loading: false
      });
    } catch (error) {
      set({ loading: false, pageError: error.message, entries: [], totals: { hours: 0, cost: 0 } });
    }
  },

  loadReport: async () => {
    const { year, month } = monthToParts(get().month);
    set({ loading: true, pageError: '' });
    try {
      const data = await apiRequest(`/api/reports/projects?year=${year}&month=${month}`);
      set({
        reportItems: data.items || [],
        reportTotal: data.total || { hours: 0, cost: 0 },
        loading: false
      });
    } catch (error) {
      set({
        loading: false,
        pageError: error.message,
        reportItems: [],
        reportTotal: { hours: 0, cost: 0 }
      });
    }
  },

  openCreate: () => {
    const { month } = get();
    set({
      modal: { type: 'form', mode: 'create' },
      form: { ...emptyForm(), date: `${month}-01` },
      formError: '',
      fieldErrors: {}
    });
  },

  openEdit: (entry) => {
    set({
      modal: { type: 'form', mode: 'edit', entry },
      form: {
        employeeId: entry.employeeId,
        projectId: entry.projectId,
        date: String(entry.date).slice(0, 10),
        hours: String(entry.hours),
        comment: entry.comment || ''
      },
      formError: '',
      fieldErrors: {}
    });
  },

  openDelete: (entry) => {
    set({
      modal: { type: 'delete', entry },
      formError: ''
    });
  },

  openRates: () => {
    const employees = get().employees;
    const employee = employees.find((item) => item.id === IVANOV_ID) || employees[0];
    set({
      modal: { type: 'rates' },
      ratesForm: ratesFormFromEmployee(employee),
      formError: '',
      fieldErrors: {}
    });
  },

  closeModal: () => set({ modal: null, formError: '', fieldErrors: {}, saving: false }),

  saveEntry: async () => {
    const { form, modal } = get();
    const hours = Number(form.hours);

    if (!isValidEntryHours(hours)) {
      set({
        saving: false,
        formError: hoursValidationMessage(),
        fieldErrors: { Hours: [hoursValidationMessage()] }
      });
      return;
    }

    const employee = get().employees.find((item) => item.id === form.employeeId);
    if (form.employeeId && form.date && !hasRateOnDate(employee, form.date)) {
      set({
        saving: false,
        formError: NO_RATE_MESSAGE,
        fieldErrors: { Date: [NO_RATE_MESSAGE] }
      });
      return;
    }

    const payload = {
      employeeId: form.employeeId,
      projectId: form.projectId,
      date: form.date,
      hours,
      comment: form.comment || null
    };

    set({ saving: true, formError: '', fieldErrors: {} });
    try {
      if (modal.mode === 'create') {
        await apiRequest('/api/time-entries', {
          method: 'POST',
          body: JSON.stringify(payload)
        });
      } else {
        await apiRequest(`/api/time-entries/${modal.entry.id}`, {
          method: 'PUT',
          body: JSON.stringify({ ...payload, version: modal.entry.version })
        });
      }
      set({ saving: false, modal: null });
      await get().loadTimesheet();
    } catch (error) {
      set({
        saving: false,
        formError: error.message,
        fieldErrors: error.fields || {}
      });
    }
  },

  deleteEntry: async () => {
    const { modal } = get();
    set({ saving: true, formError: '' });
    try {
      await apiRequest(`/api/time-entries/${modal.entry.id}`, { method: 'DELETE' });
      set({ saving: false, modal: null });
      await get().loadTimesheet();
    } catch (error) {
      set({ saving: false, formError: error.message });
    }
  },

  closePeriod: async () => {
    const { year, month } = monthToParts(get().month);
    set({ saving: true, pageError: '' });
    try {
      await apiRequest('/api/periods/close', {
        method: 'POST',
        body: JSON.stringify({ year, month })
      });
      set({ saving: false, pageError: `Период ${String(month).padStart(2, '0')}.${year} закрыт.` });
    } catch (error) {
      set({ saving: false, pageError: error.message });
    }
  },

  openPeriod: async () => {
    const { year, month } = monthToParts(get().month);
    set({ saving: true, pageError: '' });
    try {
      await apiRequest('/api/periods/open', {
        method: 'POST',
        body: JSON.stringify({ year, month })
      });
      set({ saving: false, pageError: `Период ${String(month).padStart(2, '0')}.${year} открыт.` });
    } catch (error) {
      set({ saving: false, pageError: error.message });
    }
  },

  saveRates: async () => {
    const { ratesForm } = get();
    const payload = {
      rates: ratesForm.rates.map((rate) => ({
        from: rate.from,
        value: Number(rate.value)
      }))
    };

    set({ saving: true, formError: '', fieldErrors: {} });
    try {
      await apiRequest(`/api/employees/${ratesForm.employeeId}/rates`, {
        method: 'PUT',
        body: JSON.stringify(payload)
      });
      set({ saving: false, modal: null });
      await get().bootstrap();
      if (get().screen === 'report') {
        await get().loadReport();
      }
    } catch (error) {
      set({
        saving: false,
        formError: error.message,
        fieldErrors: error.fields || {}
      });
    }
  }
}));
