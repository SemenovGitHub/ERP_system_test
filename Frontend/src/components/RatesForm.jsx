import { useStore } from '../store';

export function RatesForm() {
  const employees = useStore((state) => state.employees);
  const ratesForm = useStore((state) => state.ratesForm);
  const formError = useStore((state) => state.formError);
  const saving = useStore((state) => state.saving);
  const setRatesEmployee = useStore((state) => state.setRatesEmployee);
  const setRateRow = useStore((state) => state.setRateRow);
  const addRateRow = useStore((state) => state.addRateRow);
  const removeRateRow = useStore((state) => state.removeRateRow);
  const saveRates = useStore((state) => state.saveRates);
  const closeModal = useStore((state) => state.closeModal);

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        saveRates();
      }}
    >
      {formError ? <div className="banner error">{formError}</div> : null}

      <label>
        Сотрудник
        <select
          value={ratesForm.employeeId}
          onChange={(event) => setRatesEmployee(event.target.value)}
          required
        >
          {employees.map((employee) => (
            <option key={employee.id} value={employee.id}>{employee.fullName}</option>
          ))}
        </select>
      </label>

      <p className="hint">
        На дату записи действует последняя ставка, у которой «действует с» не позже этой даты.
      </p>

      {ratesForm.rates.map((rate, index) => (
        <div className="rate-row" key={`${rate.from}-${index}`}>
          <label>
            Действует с
            <input
              type="date"
              value={rate.from}
              onChange={(event) => setRateRow(index, 'from', event.target.value)}
              required
            />
          </label>
          <label>
            Ставка, ₽/ч
            <input
              type="number"
              min="1"
              step="any"
              value={rate.value}
              onChange={(event) => setRateRow(index, 'value', event.target.value)}
              required
            />
          </label>
          {ratesForm.rates.length > 1 ? (
            <button type="button" onClick={() => removeRateRow(index)}>Удалить</button>
          ) : null}
        </div>
      ))}

      <div className="actions">
        <button type="button" onClick={addRateRow}>Добавить ставку</button>
        <button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Сохранить'}</button>
        <button type="button" onClick={closeModal}>Отмена</button>
      </div>
    </form>
  );
}
