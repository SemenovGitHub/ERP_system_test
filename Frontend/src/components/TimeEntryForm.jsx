import { fieldMessages } from '../api';
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
  const hoursError = fieldMessages(fieldErrors, 'Hours').join(' ');

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        saveEntry();
      }}
      noValidate
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
        {fieldMessages(fieldErrors, 'EmployeeId').length ? (
          <span className="field-error">{fieldMessages(fieldErrors, 'EmployeeId').join(' ')}</span>
        ) : null}
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
        {fieldMessages(fieldErrors, 'ProjectId').length ? (
          <span className="field-error">{fieldMessages(fieldErrors, 'ProjectId').join(' ')}</span>
        ) : null}
      </label>

      <label>
        Дата
        <input
          type="date"
          value={form.date}
          onChange={(event) => setFormField('date', event.target.value)}
          required
        />
        {fieldMessages(fieldErrors, 'Date').length ? (
          <span className="field-error">{fieldMessages(fieldErrors, 'Date').join(' ')}</span>
        ) : null}
      </label>

      <label>
        Часы
        <input
          type="number"
          step="any"
          value={form.hours}
          onChange={(event) => setFormField('hours', event.target.value)}
          required
        />
        {hoursError ? <span className="field-error">{hoursError}</span> : null}
      </label>

      <label>
        Комментарий
        <input
          type="text"
          maxLength="500"
          value={form.comment}
          onChange={(event) => setFormField('comment', event.target.value)}
        />
        {fieldMessages(fieldErrors, 'Comment').length ? (
          <span className="field-error">{fieldMessages(fieldErrors, 'Comment').join(' ')}</span>
        ) : null}
      </label>

      <div className="actions">
        <button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Сохранить'}</button>
        <button type="button" onClick={closeModal}>Отмена</button>
      </div>
    </form>
  );
}
