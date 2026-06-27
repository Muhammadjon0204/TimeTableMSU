import { AlertTriangle, Plus, Search } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { axiosClient, getApiError } from '../../api/axiosClient';
import { CustomSearchableSelect, type SelectOption } from '../../components/CustomSearchableSelect';
import { DataTable, type TableColumn } from '../../components/DataTable';
import { ErrorAlert } from '../../components/ErrorAlert';
import { FormModal, type FieldConfig } from '../../components/FormModal';
import { Loading } from '../../components/Loading';

type EntityRecord = Record<string, unknown> & { id?: number };

type CrudFilterConfig = {
  key: string;
  label: string;
  allLabel: string;
};

export type CrudPageConfig = {
  title: string;
  endpoint: string;
  description: string;
  columns: Array<TableColumn<EntityRecord>>;
  fields: FieldConfig[];
  createLabel?: string;
  unavailableReason?: string;
  searchPlaceholder?: string;
  filters?: CrudFilterConfig[];
};

type ListResponse = {
  items?: EntityRecord[];
  totalCount?: number;
};

export function AdminCrudPage({ config }: { config: CrudPageConfig }) {
  const [rows, setRows] = useState<EntityRecord[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [editingRow, setEditingRow] = useState<EntityRecord | null>(null);
  const [rowToDelete, setRowToDelete] = useState<EntityRecord | null>(null);
  const [deleteError, setDeleteError] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [filterValues, setFilterValues] = useState<Record<string, string>>({});

  const isUnavailable = Boolean(config.unavailableReason);
  const pageMeta = getCrudMeta(config);
  const filters = config.filters ?? pageMeta.filters;

  const fetchRows = useCallback(async () => {
    if (isUnavailable) {
      return;
    }

    setIsLoading(true);
    setError('');

    try {
      const response = await axiosClient.get<ListResponse | EntityRecord[]>(config.endpoint, {
        params: { pageNumber: 1, pageSize: 100 },
      });
      const data = response.data;
      const nextRows = unwrapList(data);
      setRows(nextRows);
      setTotalCount(Array.isArray(data) ? data.length : data.totalCount ?? data.items?.length ?? nextRows.length);
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setIsLoading(false);
    }
  }, [config.endpoint, isUnavailable]);

  useEffect(() => {
    void fetchRows();
  }, [fetchRows]);

  useEffect(() => {
    setSearchQuery('');
    setFilterValues({});
  }, [config.endpoint]);

  const modalTitle = useMemo(() => {
    return editingRow ? `Изменить: ${config.title}` : config.createLabel ?? `Создать: ${config.title}`;
  }, [config.createLabel, config.title, editingRow]);

  const filteredRows = useMemo(() => {
    const normalizedSearch = searchQuery.trim().toLowerCase();

    return rows.filter((row) => {
      const matchesSearch =
        normalizedSearch.length === 0 ||
        Object.values(row).some((value) => String(value ?? '').toLowerCase().includes(normalizedSearch));

      if (!matchesSearch) {
        return false;
      }

      return filters.every((filter) => {
        const selectedValue = filterValues[filter.key] ?? 'all';

        if (selectedValue === 'all') {
          return true;
        }

        return String(row[filter.key] ?? '') === selectedValue;
      });
    });
  }, [filterValues, filters, rows, searchQuery]);

  const filterOptions = useMemo(() => {
    return Object.fromEntries(
      filters.map((filter) => [
        filter.key,
        Array.from(new Set(rows.map((row) => row[filter.key]).filter((value) => value !== null && value !== undefined && value !== '')))
          .map((value) => String(value))
          .sort((first, second) => first.localeCompare(second, 'ru')),
      ]),
    );
  }, [filters, rows]);

  function openCreateModal() {
    setEditingRow(null);
    setSuccess('');
    setError('');
    setIsModalOpen(true);
  }

  async function openEditModal(row: EntityRecord) {
    setSuccess('');
    setError('');

    if (!row.id) {
      setEditingRow(row);
      setIsModalOpen(true);
      return;
    }

    try {
      const response = await axiosClient.get<EntityRecord>(`${config.endpoint}/${row.id}`);
      setEditingRow(response.data);
    } catch {
      setEditingRow(row);
    }

    setIsModalOpen(true);
  }

  async function submitForm(values: Record<string, unknown>) {
    setIsSubmitting(true);
    setError('');
    setSuccess('');

    const payload = normalizePayload(values);

    try {
      if (editingRow?.id) {
        await axiosClient.put(`${config.endpoint}/${editingRow.id}`, { ...payload, id: editingRow.id });
        setSuccess('Запись успешно обновлена.');
      } else {
        await axiosClient.post(config.endpoint, payload);
        setSuccess('Запись успешно создана.');
      }

      setIsModalOpen(false);
      await fetchRows();
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setIsSubmitting(false);
    }
  }

  function openDeleteModal(row: EntityRecord) {
    if (!row.id) {
      setError('ID записи не найден');
      return;
    }

    setError('');
    setSuccess('');
    setDeleteError('');
    setRowToDelete(row);
  }

  async function confirmDelete() {
    if (!rowToDelete?.id) {
      setError('ID записи не найден');
      return;
    }

    setError('');
    setSuccess('');
    setDeleteError('');
    setIsDeleting(true);

    try {
      await axiosClient.delete(`${config.endpoint}/${rowToDelete.id}`);
      setSuccess('Запись успешно удалена');
      setRowToDelete(null);
      await fetchRows();
    } catch (requestError) {
      setDeleteError(getApiError(requestError));
    } finally {
      setIsDeleting(false);
    }
  }

  if (isUnavailable) {
    return (
      <section className="page-stack">
        <div className="page-card">
          <p className="section-copy">{config.unavailableReason}</p>
        </div>
      </section>
    );
  }

  return (
    <section className="page-stack">
      <div className="crud-toolbar-card">
        <div className="crud-toolbar-left">
          <label className="crud-search">
            <Search size={18} />
            <input
              value={searchQuery}
              placeholder={config.searchPlaceholder ?? pageMeta.searchPlaceholder}
              onChange={(event) => setSearchQuery(event.target.value)}
            />
          </label>

          {filters.map((filter) => {
            const dynamicOptions = filterOptions[filter.key] ?? [];

            if (dynamicOptions.length === 0) {
              return null;
            }

            const options: SelectOption[] = [
              { value: 'all', label: filter.allLabel },
              ...dynamicOptions.map((option) => ({ value: option, label: formatFilterLabel(option) })),
            ];

            return (
              <CustomSearchableSelect
                key={filter.key}
                value={filterValues[filter.key] ?? 'all'}
                options={options}
                placeholder={filter.allLabel}
                onChange={(value) => setFilterValues((current) => ({ ...current, [filter.key]: value }))}
              />
            );
          })}
        </div>

        <div className="crud-toolbar-right">
          <div className="crud-count">
            <strong>{filteredRows.length}</strong>
            <span>{filteredRows.length === totalCount ? 'записей' : `из ${totalCount}`}</span>
          </div>
          <button className="primary-button create-button" type="button" onClick={openCreateModal}>
            <Plus size={18} />
            Создать
          </button>
        </div>
      </div>

      {error ? <ErrorAlert message={error} /> : null}
      {success ? <div className="success-alert">{success}</div> : null}

      <div className="page-card table-card">
        {isLoading ? (
          <Loading label="Загрузка записей..." />
        ) : (
          <DataTable columns={config.columns} rows={filteredRows} onEdit={(row) => void openEditModal(row)} onDelete={openDeleteModal} />
        )}
      </div>

      <FormModal
        title={modalTitle}
        fields={config.fields}
        initialValues={editingRow ?? undefined}
        isOpen={isModalOpen}
        isSubmitting={isSubmitting}
        onClose={() => setIsModalOpen(false)}
        onSubmit={submitForm}
      />

      {rowToDelete ? (
        <div className="modal-backdrop" role="presentation">
          <div className="modal-card confirm-modal" role="dialog" aria-modal="true" aria-label="Удалить запись?">
            <div className="confirm-icon">
              <AlertTriangle size={24} />
            </div>
            <h2>Удалить запись?</h2>
            <p className="confirm-copy">Это действие нельзя отменить. Запись будет удалена из системы.</p>
            {deleteError ? <div className="modal-error">{deleteError}</div> : null}
            <div className="modal-actions confirm-actions">
              <button
                className="ghost-button"
                type="button"
                onClick={() => {
                  setRowToDelete(null);
                  setDeleteError('');
                }}
                disabled={isDeleting}
              >
                Отмена
              </button>
              <button className="danger-button" type="button" onClick={() => void confirmDelete()} disabled={isDeleting}>
                {isDeleting ? 'Удаление...' : 'Удалить'}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </section>
  );
}

function getCrudMeta(config: CrudPageConfig) {
  const metaByEndpoint: Record<string, { searchPlaceholder: string; filters: CrudFilterConfig[] }> = {
    '/faculties': {
      searchPlaceholder: 'Поиск по факультетам...',
      filters: [],
    },
    '/specialities': {
      searchPlaceholder: 'Поиск по специальностям...',
      filters: [{ key: 'facultyName', label: 'Факультет', allLabel: 'Все факультеты' }],
    },
    '/groups': {
      searchPlaceholder: 'Поиск по группам...',
      filters: [{ key: 'course', label: 'Курс', allLabel: 'Все курсы' }],
    },
    '/students': {
      searchPlaceholder: 'Поиск по студентам...',
      filters: [{ key: 'groupName', label: 'Группа', allLabel: 'Все группы' }],
    },
    '/teachers': {
      searchPlaceholder: 'Поиск по преподавателям...',
      filters: [{ key: 'teacherPost', label: 'Должность', allLabel: 'Все должности' }],
    },
    '/subjects': {
      searchPlaceholder: 'Поиск по предметам...',
      filters: [
        { key: 'controlForm', label: 'Форма контроля', allLabel: 'Все формы' },
        { key: 'semester', label: 'Семестр', allLabel: 'Все семестры' },
      ],
    },
    '/disciplines': {
      searchPlaceholder: 'Поиск по дисциплинам...',
      filters: [
        { key: 'groupName', label: 'Группа', allLabel: 'Все группы' },
        { key: 'teacherFullName', label: 'Преподаватель', allLabel: 'Все преподаватели' },
      ],
    },
    '/audiences': {
      searchPlaceholder: 'Поиск по аудиториям...',
      filters: [{ key: 'type', label: 'Тип аудитории', allLabel: 'Все типы' }],
    },
    '/weeks': {
      searchPlaceholder: 'Поиск по учебным неделям...',
      filters: [],
    },
    '/schedules': {
      searchPlaceholder: 'Поиск по расписанию...',
      filters: [
        { key: 'groupName', label: 'Группа', allLabel: 'Все группы' },
        { key: 'den', label: 'День', allLabel: 'Все дни' },
      ],
    },
    '/attendances': {
      searchPlaceholder: 'Поиск по посещаемости...',
      filters: [
        { key: 'mark', label: 'Статус', allLabel: 'Все статусы' },
        { key: 'weekName', label: 'Неделя', allLabel: 'Все недели' },
      ],
    },
    '/academic-performances': {
      searchPlaceholder: 'Поиск по успеваемости...',
      filters: [{ key: 'mark', label: 'Оценка', allLabel: 'Все оценки' }],
    },
    '/executions': {
      searchPlaceholder: 'Поиск по выполнению занятий...',
      filters: [{ key: 'status', label: 'Статус', allLabel: 'Все статусы' }],
    },
    '/users': {
      searchPlaceholder: 'Поиск по пользователям...',
      filters: [],
    },
  };

  return (
    metaByEndpoint[config.endpoint] ?? {
      searchPlaceholder: 'Поиск по записям...',
      filters: [],
    }
  );
}

function formatFilterLabel(value: string) {
  return value || 'Не указано';
}

function unwrapList(data: ListResponse | EntityRecord[] | { value?: ListResponse | EntityRecord[] }) {
  if (Array.isArray(data)) {
    return data;
  }

  if ('value' in data && data.value) {
    if (Array.isArray(data.value)) {
      return data.value;
    }

    return data.value.items ?? [];
  }

  if ('items' in data) {
    return data.items ?? [];
  }

  return [];
}

function normalizePayload(values: Record<string, unknown>) {
  return Object.fromEntries(
    Object.entries(values).map(([key, value]) => {
      if (value === '') {
        return [key, null];
      }

      return [key, value];
    }),
  );
}
