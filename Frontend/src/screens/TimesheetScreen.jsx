import { formatDate, formatHours, formatMoney } from '../api';
import { useStore } from '../store';
import { Modal } from '../components/Modal';
import { TimeEntryForm } from '../components/TimeEntryForm';
import { RatesForm } from '../components/RatesForm';

export function TimesheetScreen() {
  const month = useStore((state) => state.month);
  const employeeId = useStore((state) => state.employeeId);
  const projectId = useStore((state) => state.projectId);
  const employees = useStore((state) => state.employees);
  const projects = useStore((state) => state.projects);
  const entries = useStore((state) => state.entries);
  const totals = useStore((state) => state.totals);
  const modal = useStore((state) => state.modal);
  const formError = useStore((state) => state.formError);
  const saving = useStore((state) => state.saving);
  const setMonth = useStore((state) => state.setMonth);
  const setEmployeeId = useStore((state) => state.setEmployeeId);
  const setProjectId = useStore((state) => state.setProjectId);
  const openCreate = useStore((state) => state.openCreate);
  const openEdit = useStore((state) => state.openEdit);
  const openDelete = useStore((state) => state.openDelete);
  const openRates = useStore((state) => state.openRates);
  const closePeriod = useStore((state) => state.closePeriod);
  const openPeriod = useStore((state) => state.openPeriod);
  const closeModal = useStore((state) => state.closeModal);
  const deleteEntry = useStore((state) => state.deleteEntry);

  return (
    <section>
      <h2>Табель</h2>
      <div className="filters">
        <label>
          Месяц
          <input type="month" value={month} onChange={(event) => setMonth(event.target.value)} />
        </label>
        <label>
          Сотрудник
          <select value={employeeId} onChange={(event) => setEmployeeId(event.target.value)}>
            <option value="">Все</option>
            {employees.map((employee) => (
              <option key={employee.id} value={employee.id}>{employee.fullName}</option>
            ))}
          </select>
        </label>
        <label>
          Проект
          <select value={projectId} onChange={(event) => setProjectId(event.target.value)}>
            <option value="">Все</option>
            {projects.map((project) => (
              <option key={project.id} value={project.id}>{project.code} {project.name}</option>
            ))}
          </select>
        </label>
        <button type="button" onClick={openCreate}>Добавить запись</button>
        <button type="button" onClick={openRates}>Ставки</button>
        <button type="button" onClick={closePeriod}>Закрыть месяц</button>
        <button type="button" onClick={openPeriod}>Открыть месяц</button>
      </div>

      <table>
        <thead>
          <tr>
            <th>Дата</th>
            <th>Сотрудник</th>
            <th>Проект</th>
            <th>Часы</th>
            <th>Ставка</th>
            <th>Стоимость</th>
            <th>Комментарий</th>
            <th>Переработка</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {entries.length === 0 ? (
            <tr><td colSpan="9">Нет записей</td></tr>
          ) : entries.map((entry) => (
            <tr key={entry.id}>
              <td>{formatDate(entry.date)}</td>
              <td>{entry.employeeFullName}</td>
              <td>{entry.projectCode} {entry.projectName}</td>
              <td>{formatHours(entry.hours)}</td>
              <td>{formatMoney(entry.rate)}</td>
              <td>{formatMoney(entry.cost)}</td>
              <td>{entry.comment || ''}</td>
              <td>{entry.isOvertime ? 'Да' : ''}</td>
              <td>
                <button type="button" onClick={() => openEdit(entry)}>Изменить</button>
                <button type="button" onClick={() => openDelete(entry)}>Удалить</button>
              </td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr>
            <th colSpan="3">Итого по фильтру</th>
            <th>{formatHours(totals.hours)}</th>
            <th></th>
            <th>{formatMoney(totals.cost)}</th>
            <th colSpan="3"></th>
          </tr>
        </tfoot>
      </table>

      {modal?.type === 'form' ? (
        <Modal
          title={modal.mode === 'create' ? 'Новая запись' : 'Редактирование записи'}
          onClose={closeModal}
        >
          <TimeEntryForm />
        </Modal>
      ) : null}

      {modal?.type === 'delete' ? (
        <Modal title="Удаление записи" onClose={closeModal}>
          {formError ? <div className="banner error">{formError}</div> : null}
          <p>Удалить запись {formatDate(modal.entry.date)} / {modal.entry.employeeFullName}?</p>
          <div className="actions">
            <button type="button" disabled={saving} onClick={deleteEntry}>
              {saving ? 'Удаление...' : 'Удалить'}
            </button>
            <button type="button" onClick={closeModal}>Отмена</button>
          </div>
        </Modal>
      ) : null}

      {modal?.type === 'rates' ? (
        <Modal title="Ставки сотрудника" onClose={closeModal}>
          <RatesForm />
        </Modal>
      ) : null}
    </section>
  );
}
