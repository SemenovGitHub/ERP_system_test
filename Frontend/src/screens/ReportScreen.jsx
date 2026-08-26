import { formatHours, formatMoney } from '../api';
import { useStore } from '../store';

export function ReportScreen() {
  const month = useStore((state) => state.month);
  const setMonth = useStore((state) => state.setMonth);
  const items = useStore((state) => state.reportItems);
  const total = useStore((state) => state.reportTotal);

  return (
    <section>
      <h2>Отчёт по проектам</h2>
      <div className="filters">
        <label>
          Месяц
          <input type="month" value={month} onChange={(event) => setMonth(event.target.value)} />
        </label>
      </div>

      <table>
        <thead>
          <tr>
            <th>Проект</th>
            <th>Часы</th>
            <th>Стоимость</th>
            <th>Бюджет</th>
            <th>% освоения</th>
          </tr>
        </thead>
        <tbody>
          {items.length === 0 ? (
            <tr><td colSpan="5">Нет данных</td></tr>
          ) : items.map((item) => (
            <tr key={item.projectId} className={item.isOverBudget ? 'over' : item.isRisk ? 'risk' : ''}>
              <td>{item.projectCode} {item.projectName}</td>
              <td>{formatHours(item.hours)}</td>
              <td>{formatMoney(item.cost)}</td>
              <td>{formatMoney(item.budget)}</td>
              <td>{formatHours(item.budgetUsagePercent)}%</td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr>
            <th>Итого</th>
            <th>{formatHours(total.hours)}</th>
            <th>{formatMoney(total.cost)}</th>
            <th colSpan="2"></th>
          </tr>
        </tfoot>
      </table>
    </section>
  );
}
