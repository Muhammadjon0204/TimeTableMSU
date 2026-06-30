import { AlertTriangle, Plus, Search, X } from 'lucide-react';
import { type FormEvent, useCallback, useEffect, useMemo, useState } from 'react';
import { axiosClient, getApiError } from '../../api/axiosClient';
import { CustomSearchableSelect, type SelectOption } from '../../components/CustomSearchableSelect';
import { DataTable, type TableColumn } from '../../components/DataTable';
import { ErrorAlert } from '../../components/ErrorAlert';
import { Loading } from '../../components/Loading';

type DisciplineRow = {
  id: number;
  subjectId?: number;
  subjectName: string;
  teacherId?: number;
  teacherFullName: string;
  groupId?: number;
  groupName: string;
  lectureHourCount?: number | null;
  practiceHourCount?: number | null;
  laboratoryHourCount?: number | null;
};

type SubjectLookup = {
  id: number;
  name: string;
  semester: number;
  hourCount: number;
  controlForm: string;
};

type TeacherLookup = {
  id: number;
  fullName: string;
  teacherPost?: string | null;
};

type GroupLookup = {
  id: number;
  name: string;
  course?: number | null;
  specialityName?: string | null;
};

type HourValue = number | '';

const columns: Array<TableColumn<DisciplineRow>> = [
  { key: 'id', label: 'ID' },
  { key: 'subjectName', label: 'Предмет' },
  { key: 'teacherFullName', label: 'Преподаватель' },
  { key: 'groupName', label: 'Группа' },
];

export function DisciplinesPage() {
  const [rows, setRows] = useState<DisciplineRow[]>([]);
  const [subjects, setSubjects] = useState<SubjectLookup[]>([]);
  const [teachers, setTeachers] = useState<TeacherLookup[]>([]);
  const [groups, setGroups] = useState<GroupLookup[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isLookupsLoading, setIsLookupsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [formError, setFormError] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRow, setEditingRow] = useState<DisciplineRow | null>(null);
  const [rowToDelete, setRowToDelete] = useState<DisciplineRow | null>(null);

  const fetchRows = useCallback(async () => {
    setIsLoading(true);
    setError('');

    try {
      const response = await axiosClient.get<DisciplineRow[] | { items?: DisciplineRow[]; value?: DisciplineRow[] }>('/disciplines', {
        params: { pageNumber: 1, pageSize: 100 },
      });
      setRows(unwrapRows(response.data));
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setIsLoading(false);
    }
  }, []);

  const fetchLookups = useCallback(async () => {
    setIsLookupsLoading(true);
    setError('');

    try {
      const [subjectsResponse, teachersResponse, groupsResponse] = await Promise.all([
        axiosClient.get<SubjectLookup[]>('/subjects'),
        axiosClient.get<TeacherLookup[]>('/teachers'),
        axiosClient.get<GroupLookup[]>('/groups'),
      ]);

      setSubjects(unwrapRows(subjectsResponse.data));
      setTeachers(unwrapRows(teachersResponse.data));
      setGroups(unwrapRows(groupsResponse.data));
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setIsLookupsLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchRows();
    void fetchLookups();
  }, [fetchLookups, fetchRows]);

  const filteredRows = useMemo(() => {
    const normalizedSearch = searchQuery.trim().toLowerCase();

    if (!normalizedSearch) {
      return rows;
    }

    return rows.filter((row) =>
      [row.subjectName, row.teacherFullName, row.groupName]
        .some((value) => value.toLowerCase().includes(normalizedSearch)),
    );
  }, [rows, searchQuery]);

  function openCreateModal() {
    setEditingRow(null);
    setFormError('');
    setError('');
    setSuccess('');
    setIsModalOpen(true);
  }

  async function openEditModal(row: DisciplineRow) {
    setFormError('');
    setError('');
    setSuccess('');

    try {
      const response = await axiosClient.get<DisciplineRow>(`/disciplines/${row.id}`);
      setEditingRow(response.data);
    } catch {
      setEditingRow(row);
    }

    setIsModalOpen(true);
  }

  async function submitForm(payload: DisciplinePayload) {
    setIsSubmitting(true);
    setFormError('');
    setError('');
    setSuccess('');

    try {
      if (editingRow?.id) {
        await axiosClient.put(`/disciplines/${editingRow.id}`, { ...payload, id: editingRow.id });
        setSuccess('Дисциплина обновлена.');
      } else {
        await axiosClient.post('/disciplines', payload);
        setSuccess('Дисциплина создана.');
      }

      setIsModalOpen(false);
      await fetchRows();
    } catch (requestError) {
      setFormError(getApiError(requestError));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function confirmDelete() {
    if (!rowToDelete) {
      return;
    }

    setIsDeleting(true);
    setError('');
    setSuccess('');

    try {
      await axiosClient.delete(`/disciplines/${rowToDelete.id}`);
      setSuccess('Дисциплина удалена.');
      setRowToDelete(null);
      await fetchRows();
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <section className="page-stack">
      <div className="crud-toolbar-card">
        <div className="crud-toolbar-left">
          <label className="crud-search">
            <Search size={18} />
            <input value={searchQuery} placeholder="Поиск по дисциплинам..." onChange={(event) => setSearchQuery(event.target.value)} />
          </label>
        </div>
        <div className="crud-toolbar-right">
          <div className="crud-count">
            <strong>{filteredRows.length}</strong>
            <span>{filteredRows.length === rows.length ? 'записей' : `из ${rows.length}`}</span>
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
          <Loading label="Загрузка дисциплин..." />
        ) : (
          <DataTable columns={columns} rows={filteredRows} onEdit={(row) => void openEditModal(row)} onDelete={setRowToDelete} />
        )}
      </div>

      <DisciplineFormModal
        isOpen={isModalOpen}
        isSubmitting={isSubmitting}
        isLookupsLoading={isLookupsLoading}
        initialValues={editingRow}
        subjects={subjects}
        teachers={teachers}
        groups={groups}
        formError={formError}
        onClose={() => setIsModalOpen(false)}
        onSubmit={submitForm}
      />

      {rowToDelete ? (
        <div className="modal-backdrop" role="presentation">
          <div className="modal-card confirm-modal" role="dialog" aria-modal="true" aria-label="Удалить дисциплину?">
            <div className="confirm-icon">
              <AlertTriangle size={24} />
            </div>
            <h2>Удалить дисциплину?</h2>
            <p className="confirm-copy">
              {rowToDelete.subjectName} · {rowToDelete.groupName} · {rowToDelete.teacherFullName}
            </p>
            <div className="modal-actions confirm-actions">
              <button className="ghost-button" type="button" onClick={() => setRowToDelete(null)} disabled={isDeleting}>
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

type DisciplinePayload = {
  subjectId: number;
  teacherId: number;
  groupId: number;
  lectureHourCount: number;
  practiceHourCount: number;
  laboratoryHourCount: number;
};

type DisciplineFormModalProps = {
  isOpen: boolean;
  isSubmitting: boolean;
  isLookupsLoading: boolean;
  initialValues: DisciplineRow | null;
  subjects: SubjectLookup[];
  teachers: TeacherLookup[];
  groups: GroupLookup[];
  formError: string;
  onClose: () => void;
  onSubmit: (payload: DisciplinePayload) => Promise<void>;
};

function DisciplineFormModal({
  isOpen,
  isSubmitting,
  isLookupsLoading,
  initialValues,
  subjects,
  teachers,
  groups,
  formError,
  onClose,
  onSubmit,
}: DisciplineFormModalProps) {
  const [selectedSubjectId, setSelectedSubjectId] = useState<number | null>(null);
  const [selectedTeacherId, setSelectedTeacherId] = useState<number | null>(null);
  const [selectedGroupId, setSelectedGroupId] = useState<number | null>(null);
  const [lectureHours, setLectureHours] = useState<HourValue>('');
  const [practiceHours, setPracticeHours] = useState<HourValue>('');
  const [laboratoryHours, setLaboratoryHours] = useState<HourValue>('');
  const [localError, setLocalError] = useState('');

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    setSelectedSubjectId(toNumber(initialValues?.subjectId));
    setSelectedTeacherId(toNumber(initialValues?.teacherId));
    setSelectedGroupId(toNumber(initialValues?.groupId));
    setLectureHours(toHourValue(initialValues?.lectureHourCount));
    setPracticeHours(toHourValue(initialValues?.practiceHourCount));
    setLaboratoryHours(toHourValue(initialValues?.laboratoryHourCount));
    setLocalError('');
  }, [initialValues, isOpen]);

  if (!isOpen) {
    return null;
  }

  const selectedSubject = subjects.find((subject) => subject.id === selectedSubjectId);
  const recommendedCourse = selectedSubject ? Math.ceil(selectedSubject.semester / 2) : null;
  const subjectOptions = subjects.map(toSubjectOption);
  const teacherOptions = teachers.map(toTeacherOption);
  const groupOptions = orderGroups(groups, recommendedCourse).map((group) => toGroupOption(group, group.course === recommendedCourse));

  const hasValidHours = [lectureHours, practiceHours, laboratoryHours].every((value) => value === '' || value >= 0);
  const totalHours = toNumberOrZero(lectureHours) + toNumberOrZero(practiceHours) + toNumberOrZero(laboratoryHours);
  const canSubmit = Boolean(selectedSubjectId && selectedTeacherId && selectedGroupId && hasValidHours && totalHours > 0 && !isSubmitting);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedSubjectId || !selectedTeacherId || !selectedGroupId) {
      setLocalError('Выберите предмет, преподавателя и группу.');
      return;
    }

    if (!hasValidHours) {
      setLocalError('Количество часов не может быть меньше 0.');
      return;
    }

    if (totalHours <= 0) {
      setLocalError('Укажите хотя бы один тип учебных часов.');
      return;
    }

    await onSubmit({
      subjectId: selectedSubjectId,
      teacherId: selectedTeacherId,
      groupId: selectedGroupId,
      lectureHourCount: toNumberOrZero(lectureHours),
      practiceHourCount: toNumberOrZero(practiceHours),
      laboratoryHourCount: toNumberOrZero(laboratoryHours),
    });
  }

  return (
    <div className="modal-backdrop" role="presentation">
      <div className="modal-card discipline-form-modal" role="dialog" aria-modal="true" aria-label={initialValues ? 'Изменить дисциплину' : 'Создать дисциплину'}>
        <div className="modal-header">
          <h2>{initialValues ? 'Изменить дисциплину' : 'Создать дисциплину'}</h2>
          <button className="icon-button small" type="button" onClick={onClose} aria-label="Закрыть форму">
            <X size={18} />
          </button>
        </div>

        <form className="form-grid discipline-form-grid" onSubmit={handleSubmit}>
          <label className="form-field discipline-select-field">
            <span>Предмет</span>
            <CustomSearchableSelect
              value={toSelectValue(selectedSubjectId)}
              options={subjectOptions}
              placeholder="Выберите предмет"
              clearValue=""
              disabled={isLookupsLoading}
              onChange={(value) => setSelectedSubjectId(toNumber(value))}
            />
          </label>

          <label className="form-field discipline-select-field">
            <span>Преподаватель</span>
            <CustomSearchableSelect
              value={toSelectValue(selectedTeacherId)}
              options={teacherOptions}
              placeholder="Выберите преподавателя"
              clearValue=""
              disabled={isLookupsLoading}
              onChange={(value) => setSelectedTeacherId(toNumber(value))}
            />
          </label>

          <label className="form-field discipline-select-field">
            <span>Группа</span>
            <CustomSearchableSelect
              value={toSelectValue(selectedGroupId)}
              options={groupOptions}
              placeholder="Выберите группу"
              clearValue=""
              disabled={isLookupsLoading}
              onChange={(value) => setSelectedGroupId(toNumber(value))}
            />
          </label>

          <label className="form-field">
            <span>Лекционные часы</span>
            <input type="number" min="0" value={lectureHours} placeholder="24" onChange={(event) => setLectureHours(toHourValue(event.target.value))} />
          </label>

          <label className="form-field">
            <span>Практические часы</span>
            <input type="number" min="0" value={practiceHours} placeholder="24" onChange={(event) => setPracticeHours(toHourValue(event.target.value))} />
          </label>

          <label className="form-field">
            <span>Лабораторные часы</span>
            <input type="number" min="0" value={laboratoryHours} placeholder="12" onChange={(event) => setLaboratoryHours(toHourValue(event.target.value))} />
          </label>

          {localError || formError ? <div className="modal-error discipline-form-message">{localError || formError}</div> : null}

          <div className="modal-actions">
            <button className="ghost-button" type="button" onClick={onClose}>
              Отмена
            </button>
            <button className="primary-button" type="submit" disabled={!canSubmit}>
              {isSubmitting ? 'Сохранение...' : 'Сохранить'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

function toSubjectOption(subject: SubjectLookup): SelectOption {
  return {
    value: String(subject.id),
    label: subject.name,
    subtitle: `${subject.semester} семестр · ${formatControlForm(subject.controlForm)} · ${subject.hourCount} часов`,
  };
}

function toTeacherOption(teacher: TeacherLookup): SelectOption {
  return {
    value: String(teacher.id),
    label: teacher.fullName,
    subtitle: teacher.teacherPost ? formatTeacherPost(teacher.teacherPost) : 'Должность не указана',
  };
}

function toGroupOption(group: GroupLookup, isRecommended: boolean): SelectOption {
  const subtitleParts = [
    group.course ? `${group.course} курс` : null,
    group.specialityName,
    isRecommended ? 'рекомендуемая' : null,
  ].filter(Boolean);

  return {
    value: String(group.id),
    label: group.name,
    subtitle: subtitleParts.join(' · '),
  };
}

function orderGroups(groups: GroupLookup[], recommendedCourse: number | null) {
  return [...groups].sort((first, second) => {
    const firstRecommended = recommendedCourse !== null && first.course === recommendedCourse;
    const secondRecommended = recommendedCourse !== null && second.course === recommendedCourse;

    if (firstRecommended !== secondRecommended) {
      return firstRecommended ? -1 : 1;
    }

    return first.name.localeCompare(second.name, 'ru');
  });
}

function unwrapRows<T>(data: T[] | { items?: T[]; value?: T[] | { items?: T[] } }) {
  if (Array.isArray(data)) {
    return data;
  }

  if (Array.isArray(data.value)) {
    return data.value;
  }

  if (data.value && 'items' in data.value) {
    return data.value.items ?? [];
  }

  return data.items ?? [];
}

function toNumber(value: unknown) {
  if (value === null || value === undefined || value === '') {
    return null;
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function toSelectValue(value: number | null) {
  return value === null ? '' : String(value);
}

function toHourValue(value: unknown): HourValue {
  if (value === null || value === undefined || value === '') {
    return '';
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : '';
}

function toNumberOrZero(value: HourValue) {
  return value === '' ? 0 : value;
}

function formatControlForm(value: string) {
  const labels: Record<string, string> = {
    Credit: 'Зачёт',
    Exam: 'Экзамен',
  };

  return labels[value] ?? value;
}

function formatTeacherPost(value: string) {
  const labels: Record<string, string> = {
    Assistant: 'Ассистент',
    SeniorTeacher: 'Старший преподаватель',
    SeniorLecturer: 'Старший преподаватель',
    Docent: 'Доцент',
    Professor: 'Профессор',
    HeadOfDepartment: 'Заведующий кафедрой',
  };

  return labels[value] ?? value;
}
