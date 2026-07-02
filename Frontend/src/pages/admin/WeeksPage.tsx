import { CalendarDays, RefreshCw } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { axiosClient, getApiError } from '../../api/axiosClient';
import { CustomSearchableSelect, type SelectOption } from '../../components/CustomSearchableSelect';
import { DataTable, type TableColumn } from '../../components/DataTable';
import { ErrorAlert } from '../../components/ErrorAlert';
import { Loading } from '../../components/Loading';

type AcademicYear = {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  weeksCount: number;
};

type Week = {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  academicYearId?: number | null;
  academicYearName?: string;
  weekType?: string;
  isCurrent?: boolean;
};

type GenerateWeeksResponse = {
  createdCount: number;
  skippedCount: number;
  weeks: Week[];
};

const weekColumns: Array<TableColumn<Week>> = [
  { key: 'name', label: 'Неделя' },
  { key: 'academicYearName', label: 'Учебный год' },
  { key: 'startDate', label: 'Начало', render: (row) => formatDate(row.startDate) },
  { key: 'endDate', label: 'Окончание', render: (row) => formatDate(row.endDate) },
  { key: 'weekType', label: 'Тип', render: (row) => formatWeekType(row.weekType) },
];

export function WeeksPage() {
  const [academicYears, setAcademicYears] = useState<AcademicYear[]>([]);
  const [weeks, setWeeks] = useState<Week[]>([]);
  const [selectedAcademicYearId, setSelectedAcademicYearId] = useState('');
  const [startDate, setStartDate] = useState('');
  const [generateUntil, setGenerateUntil] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [recovered, setRecovered] = useState(false);

  const selectedAcademicYear = academicYears.find((year) => String(year.id) === selectedAcademicYearId);
  const academicYearOptions: SelectOption[] = academicYears.map((year) => ({
    value: String(year.id),
    label: year.name,
    subtitle: `${formatDate(year.startDate)} - ${formatDate(year.endDate)}`,
  }));

  const filteredWeeks = useMemo(() => {
    return weeks
      .filter((week) => !selectedAcademicYearId || String(week.academicYearId ?? '') === selectedAcademicYearId)
      .sort((first, second) => first.startDate.localeCompare(second.startDate));
  }, [selectedAcademicYearId, weeks]);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    setError('');

    try {
      const [yearsResponse, weeksResponse] = await Promise.all([
        axiosClient.get<AcademicYear[]>('/academic-years'),
        axiosClient.get<Week[]>('/weeks'),
      ]);

      const years = yearsResponse.data;
      const allWeeks = weeksResponse.data;

      // If no academic years but weeks exist, attempt backend recovery once
      if (years.length === 0 && allWeeks.length > 0 && !recovered) {
        try {
          await axiosClient.post('/academic-years/recover-from-weeks');
          setRecovered(true);
          // reload data after recovery
          const [newYearsResponse, newWeeksResponse] = await Promise.all([
            axiosClient.get<AcademicYear[]>('/academic-years'),
            axiosClient.get<Week[]>('/weeks'),
          ]);

          const newYears = newYearsResponse.data;
          const newWeeks = newWeeksResponse.data;
          setAcademicYears(newYears);
          setWeeks(newWeeks);

          const defaultYear = newYears.find((year) => year.isCurrent) ?? newYears[0];
          if (defaultYear) {
            setSelectedAcademicYearId((current) => current || String(defaultYear.id));
            setStartDate((current) => current || toDateInputValue(defaultYear.startDate));
            setGenerateUntil((current) => current || defaultGenerateUntil(defaultYear));
          }

          return;
        } catch (recErr) {
          // ignore and continue with empty years
        }
      }

      const defaultYear = years.find((year) => year.isCurrent) ?? years[0];

      setAcademicYears(years);
      setWeeks(allWeeks);

      if (defaultYear) {
        setSelectedAcademicYearId((current) => current || String(defaultYear.id));
        setStartDate((current) => current || toDateInputValue(defaultYear.startDate));
        setGenerateUntil((current) => current || defaultGenerateUntil(defaultYear));
      }
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchData();
  }, [fetchData]);

  function selectAcademicYear(value: string) {
    const year = academicYears.find((item) => String(item.id) === value);
    setSelectedAcademicYearId(value);
    setSuccess('');
    setError('');

    if (year) {
      setStartDate(toDateInputValue(year.startDate));
      setGenerateUntil(defaultGenerateUntil(year));
    }
  }

  async function generateWeeks() {
    setError('');
    setSuccess('');

    if (!selectedAcademicYearId) {
      setError('Выберите учебный год.');
      return;
    }

    if (!startDate || !generateUntil) {
      setError('Укажите даты начала и окончания генерации.');
      return;
    }

    if (startDate > generateUntil) {
      setError('Дата начала не может быть позже даты окончания генерации.');
      return;
    }

    setIsGenerating(true);

    try {
      const response = await axiosClient.post<GenerateWeeksResponse>(`/academic-years/${selectedAcademicYearId}/generate-weeks`, {
        startDate,
        generateUntil,
        skipExistingWeeks: true,
        overwriteExistingWeeks: false,
      });

      setSuccess(`Создано ${response.data.createdCount} недель, пропущено ${response.data.skippedCount} уже существующих.`);
      await fetchData();
    } catch (requestError) {
      setError(`Не удалось сгенерировать недели. ${getApiError(requestError)}`);
    } finally {
      setIsGenerating(false);
    }
  }

  return (
    <section className="page-stack">
      <div className="page-card weeks-generator-card">
        <div className="weeks-generator-header">
          <div>
            <h2>Учебные недели</h2>
            <p className="section-copy">Сгенерируйте календарь недель по выбранному учебному году без ручного создания дублей.</p>
          </div>
          <div className="resource-count">
            <span>{filteredWeeks.length}</span>
            <small>недель</small>
          </div>
        </div>

        <div className="weeks-generator-grid">
          <label className="weeks-generator-field">
            <span>Учебный год</span>
            <CustomSearchableSelect
              value={selectedAcademicYearId}
              options={academicYearOptions}
              placeholder="Выберите учебный год"
              clearValue=""
              onChange={selectAcademicYear}
            />
          </label>

          <label className="weeks-generator-field">
            <span>Начало</span>
            <input type="date" value={startDate} onChange={(event) => setStartDate(event.target.value)} />
          </label>

          <label className="weeks-generator-field">
            <span>До</span>
            <input type="date" value={generateUntil} onChange={(event) => setGenerateUntil(event.target.value)} />
          </label>

          <button className="primary-button weeks-generate-button" type="button" onClick={() => void generateWeeks()} disabled={isGenerating}>
            {isGenerating ? <RefreshCw size={18} /> : <CalendarDays size={18} />}
            {isGenerating ? 'Генерация...' : 'Сгенерировать недели'}
          </button>
        </div>

        {selectedAcademicYear ? (
          <p className="weeks-generator-note">
            {selectedAcademicYear.name}: {formatDate(selectedAcademicYear.startDate)} - {formatDate(selectedAcademicYear.endDate)}
          </p>
        ) : null}
      </div>

      {error ? <ErrorAlert message={error} /> : null}
      {success ? <div className="success-alert">{success}</div> : null}

      <div className="page-card table-card">
        {isLoading ? <Loading label="Загрузка учебных недель..." /> : <DataTable columns={weekColumns} rows={filteredWeeks} />}
      </div>
    </section>
  );
}

function toDateInputValue(value: string) {
  return value.slice(0, 10);
}

function defaultGenerateUntil(year: AcademicYear) {
  const start = new Date(`${toDateInputValue(year.startDate)}T00:00:00`);
  const end = toDateInputValue(year.endDate);
  const generated = `${start.getFullYear() + 1}-07-20`;

  return generated > end ? end : generated;
}

function formatDate(value?: string) {
  if (!value) {
    return '-';
  }

  const [year, month, day] = toDateInputValue(value).split('-');
  return `${day}.${month}.${year}`;
}

function formatWeekType(value?: string) {
  const labels: Record<string, string> = {
    Study: 'Учебная',
    Exam: 'Сессия',
    Practice: 'Практика',
    Vacation: 'Каникулы',
    Holiday: 'Праздник',
  };

  return labels[value ?? ''] ?? value ?? '-';
}
