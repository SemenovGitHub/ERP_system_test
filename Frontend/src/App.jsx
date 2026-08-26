import { useEffect } from 'react';
import { useStore } from './store';
import { TimesheetScreen } from './screens/TimesheetScreen';
import { ReportScreen } from './screens/ReportScreen';

export default function App() {
  const screen = useStore((state) => state.screen);
  const setScreen = useStore((state) => state.setScreen);
  const bootstrap = useStore((state) => state.bootstrap);
  const pageError = useStore((state) => state.pageError);
  const loading = useStore((state) => state.loading);

  useEffect(() => {
    bootstrap();
  }, [bootstrap]);

  return (
    <div className="page">
      <header className="top">
        <h1>ERP</h1>
        <nav>
          <button
            className={screen === 'timesheet' ? 'active' : ''}
            onClick={() => setScreen('timesheet')}
          >
            Табель
          </button>
          <button
            className={screen === 'report' ? 'active' : ''}
            onClick={() => setScreen('report')}
          >
            Отчёт по проектам
          </button>
        </nav>
      </header>
      {pageError ? <div className="banner error">{pageError}</div> : null}
      {loading ? <div className="banner">Загрузка...</div> : null}
      {screen === 'timesheet' ? <TimesheetScreen /> : <ReportScreen />}
    </div>
  );
}
