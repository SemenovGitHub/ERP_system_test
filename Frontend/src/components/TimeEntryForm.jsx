import { useStore } from '../store';

export function TimeEntryForm() {
  const employees = useStore((state) => state.employees);
  const projects = useStore((state) => state.projects);
  const form = useStore((state) => state.form);
  const fieldErrors = useStore((state) => state.fieldErrors);
  const formError = useStore((state) => state.formError);
  const saving = useStore((state) => state.saving);
  const setFormField = useStore((state) => state.setFormField);
  const saveEntry = useStore((state) => state.saveEntry);
  const closeModal = useStore((state) => state.closeModal);

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        saveEntry();
      }}
    >
      {formError ? <div className="banner error">{formError}</div> : null}

      <label>
        Сотрудник
        <select
          value={form.employeeId}
          onChange={(event) => setFormField('employeeId', event.target.value)}
          required
        >
          <option value="">Выберите</option>
          {employees.map((employee) => (
            <option key={employee.id} value={employee.id}>
              {employee.fullName}
            </option>
          ))}
        </select>
        {fieldErrors.EmployeeId ? <span className="field-error">{fieldErrors.EmployeeId.join(' ')}</span> : null}
      </label>

      <label>
        Проект
        <select
          value={form.projectId}
          onChange={(event) => setFormField('projectId', event.target.value)}
          required
        >
          <option value="">Выберите</option>
          {projects.map((project) => (
            <option key={project.id} value={project.id}>
              {project.code} {project.name}
            </option>
          ))}
        </select>
        {fieldErrors.ProjectId ? <span className="field-error">{fieldErrors.ProjectId.join(' ')}</span> : null}
      </label>

      <label>
        Дата
        <input
          type="date"
          value={form.date}
          onChange={(event) => setFormField('date', event.target.value)}
          required
        />
        {fieldErrors.Date ? <span className="field-error">{fieldErrors.Date.join(' ')}</span> : null}
      </label>

      <label>
        Часы
        <input
          type="number"
          min="0.5"
          max="24"
          step="0.5"
          value={form.hours}
          onChange={(event) => setFormField('hours', event.target.value)}
          required
        />
        {fieldErrors.Hours ? <span className="field-error">{fieldErrors.Hours.join(' ')}</span> : null}
      </label>

      <label>
        Комментарий
        <input
          type="text"
          maxLength="500"
          value={form.comment}
          onChange={(event) => setFormField('comment', event.target.value)}
        />
        {fieldErrors.Comment ? <span className="field-error">{fieldErrors.Comment.join(' ')}</span> : null}
      </label>

      <div className="actions">
        <button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Сохранить'}</button>
        <button type="button" onClick={closeModal}>Отмена</button>
      </div>
    </form>
  );
}
