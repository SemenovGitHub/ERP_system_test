import { create } from 'zustand';
import { apiRequest, monthToParts } from './api';

const DEFAULT_MONTH = '2026-03';

function emptyForm() {
  return {
    employeeId: '',
    projectId: '',
    date: `${DEFAULT_MONTH}-01`,
    hours: '8',
    comment: ''
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
      fieldErrors: { ...state.fieldErrors, [name]: undefined }
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

  closeModal: () => set({ modal: null, formError: '', fieldErrors: {}, saving: false }),

  saveEntry: async () => {
    const { form, modal } = get();
    const payload = {
      employeeId: form.employeeId,
      projectId: form.projectId,
      date: form.date,
      hours: Number(form.hours),
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
  }
}));
