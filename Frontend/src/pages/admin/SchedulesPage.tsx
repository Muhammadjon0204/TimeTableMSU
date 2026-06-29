import { AlertTriangle, CalendarDays, Edit2, Plus, RefreshCcw, Trash2, X } from 'lucide-react';
import type { FormEvent, ReactNode } from 'react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { axiosClient, getApiError } from '../../api/axiosClient';
import { CustomSearchableSelect, type SelectOption } from '../../components/CustomSearchableSelect';
import { ErrorAlert } from '../../components/ErrorAlert';
import { Loading } from '../../components/Loading';
import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

type ViewMode = 'week' | 'table';

type LookupDto = {
  id: number;
  name: string;
};

type WeekLookupDto = LookupDto & {
  startDate: string;
  endDate: string;
  displayName?: string;
};

type SubjectLookupDto = {
  subjectId: number;
  subjectName: string;
  semester: number;
  controlForm: string;
  displayName: string;
};

type DisciplineScheduleOption = {
  disciplineId: number;
  subjectId: number;
  subjectName: string;
  teacherId: number;
  teacherFullName: string;
  groupId: number;
  groupName: string;
  specialityId: number;
  specialityName: string;
  facultyId: number;
  facultyName: string;
  semester: number;
};

type AudienceLookupDto = {
  id: number;
  number: string;
  type: string;
  displayName: string;
};

type WeeklyScheduleBoardDto = {
  weekId: number | null;
  weekName: string;
  startDate: string | null;
  endDate: string | null;
  days: ScheduleDayDto[];
};

type ScheduleDayDto = {
  day: number;
  dayName: string;
  date: string | null;
  paras: ScheduleParaDto[];
};

type ScheduleParaDto = {
  para: number;
  timeRange: string;
  lessons: ScheduleLessonDto[];
};

type ScheduleLessonDto = {
  scheduleId: number;
  subjectName: string;
  teacherFullName: string;
  groupName: string;
  audienceNumber: string;
  lectureType: string;
  audienceType?: string | null;
  day: number;
  para: number;
};

type ScheduleFormValues = Record<string, unknown> & { id?: number };

const allValue = 'all';

export function SchedulesPage() {
  const [viewMode, setViewMode] = useState<ViewMode>('week');
  const [board, setBoard] = useState<WeeklyScheduleBoardDto | null>(null);
  const [weeks, setWeeks] = useState<WeekLookupDto[]>([]);
  const [groups, setGroups] = useState<LookupDto[]>([]);
  const [teachers, setTeachers] = useState<LookupDto[]>([]);
  const [audiences, setAudiences] = useState<LookupDto[]>([]);
  const [selectedWeekId, setSelectedWeekId] = useState(allValue);
  const [selectedGroupId, setSelectedGroupId] = useState(allValue);
  const [selectedTeacherId, setSelectedTeacherId] = useState(allValue);
  const [selectedAudienceId, setSelectedAudienceId] = useState(allValue);
  const [isLoadingBoard, setIsLoadingBoard] = useState(false);
  const [isLoadingLookups, setIsLoadingLookups] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingSchedule, setEditingSchedule] = useState<ScheduleFormValues | null>(null);
  const [lessonToDelete, setLessonToDelete] = useState<ScheduleLessonDto | null>(null);
  const [deleteError, setDeleteError] = useState('');

  const scheduleConfig = crudConfigs.schedules;

  const fetchLookups = useCallback(async () => {
    setIsLoadingLookups(true);
    setError('');

    try {
      const [weekResponse, groupResponse, teacherResponse, audienceResponse] = await Promise.all([
        axiosClient.get<WeekLookupDto[]>('/admin-schedule-board/week-lookups'),
        axiosClient.get<LookupDto[]>('/admin-schedule-board/group-lookups'),
        axiosClient.get<LookupDto[]>('/admin-schedule-board/teacher-lookups'),
        axiosClient.get<LookupDto[]>('/admin-schedule-board/audience-lookups'),
      ]);

      setWeeks(weekResponse.data);
      setGroups(groupResponse.data);
      setTeachers(teacherResponse.data);
      setAudiences(audienceResponse.data);
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setIsLoadingLookups(false);
    }
  }, []);

  const fetchBoard = useCallback(async () => {
    setIsLoadingBoard(true);
    setError('');

    try {
      const response = await axiosClient.get<WeeklyScheduleBoardDto>('/admin-schedule-board/weekly', {
        params: {
          weekId: selectedWeekId === allValue ? undefined : Number(selectedWeekId),
          groupId: selectedGroupId === allValue ? undefined : Number(selectedGroupId),
          teacherId: selectedTeacherId === allValue ? undefined : Number(selectedTeacherId),
          audienceId: selectedAudienceId === allValue ? undefined : Number(selectedAudienceId),
        },
      });

      setBoard(response.data);

      if (selectedWeekId === allValue && response.data.weekId) {
        setSelectedWeekId(String(response.data.weekId));
      }
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setIsLoadingBoard(false);
    }
  }, [selectedAudienceId, selectedGroupId, selectedTeacherId, selectedWeekId]);

  useEffect(() => {
    void fetchLookups();
  }, [fetchLookups]);

  useEffect(() => {
    void fetchBoard();
  }, [fetchBoard]);

  const weekOptions = useMemo<SelectOption[]>(() => {
    return [
      { value: allValue, label: 'Текущая неделя' },
      ...weeks.map((week) => ({
        value: String(week.id),
        label: `${week.name} · ${formatDate(week.startDate)} - ${formatDate(week.endDate)}`,
      })),
    ];
  }, [weeks]);

  const groupOptions = useMemo(() => createLookupOptions('Все группы', groups), [groups]);
  const teacherOptions = useMemo(() => createLookupOptions('Все преподаватели', teachers), [teachers]);
  const audienceOptions = useMemo(() => createLookupOptions('Все аудитории', audiences), [audiences]);

  const lessonCount = useMemo(() => {
    return board?.days.reduce((total, day) => total + countDayLessons(day), 0) ?? 0;
  }, [board]);

  function openCreateModal() {
    setEditingSchedule(null);
    setError('');
    setSuccess('');
    setIsModalOpen(true);
  }

  async function openEditModal(scheduleId: number) {
    setError('');
    setSuccess('');

    try {
      const response = await axiosClient.get<ScheduleFormValues>(`${scheduleConfig.endpoint}/${scheduleId}`);
      setEditingSchedule(response.data);
      setIsModalOpen(true);
    } catch (requestError) {
      setError(getApiError(requestError));
    }
  }

  async function submitForm(values: Record<string, unknown>) {
    setIsSubmitting(true);
    setError('');
    setSuccess('');

    try {
      if (editingSchedule?.id) {
        await axiosClient.put(`${scheduleConfig.endpoint}/${editingSchedule.id}`, { ...normalizePayload(values), id: editingSchedule.id });
        setSuccess('Занятие обновлено.');
      } else {
        await axiosClient.post(scheduleConfig.endpoint, normalizePayload(values));
        setSuccess('Занятие создано.');
      }

      setIsModalOpen(false);
      await fetchBoard();
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function confirmDelete() {
    if (!lessonToDelete) {
      return;
    }

    setDeleteError('');
    setIsDeleting(true);

    try {
      await axiosClient.delete(`${scheduleConfig.endpoint}/${lessonToDelete.scheduleId}`);
      setSuccess('Занятие удалено.');
      setLessonToDelete(null);
      await fetchBoard();
    } catch (requestError) {
      setDeleteError(getApiError(requestError));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <section className="page-stack schedule-page">
      <div className="schedule-mode-bar">
        <div className="schedule-mode-switcher" aria-label="Режим отображения расписания">
          <button className={viewMode === 'week' ? 'active' : ''} type="button" onClick={() => setViewMode('week')}>
            Недельный вид
          </button>
          <button className={viewMode === 'table' ? 'active' : ''} type="button" onClick={() => setViewMode('table')}>
            Таблица
          </button>
        </div>
      </div>

      {viewMode === 'table' ? (
        <AdminCrudPage config={scheduleConfig} />
      ) : (
        <>
          <ScheduleFiltersBar
            weekOptions={weekOptions}
            groupOptions={groupOptions}
            teacherOptions={teacherOptions}
            audienceOptions={audienceOptions}
            selectedWeekId={selectedWeekId}
            selectedGroupId={selectedGroupId}
            selectedTeacherId={selectedTeacherId}
            selectedAudienceId={selectedAudienceId}
            lessonCount={lessonCount}
            isLoading={isLoadingLookups || isLoadingBoard}
            onWeekChange={setSelectedWeekId}
            onGroupChange={setSelectedGroupId}
            onTeacherChange={setSelectedTeacherId}
            onAudienceChange={setSelectedAudienceId}
            onCurrentWeek={() => setSelectedWeekId(allValue)}
            onRefresh={() => void fetchBoard()}
            onCreate={openCreateModal}
          />

          {error ? <ErrorAlert message={error} /> : null}
          {success ? <div className="success-alert">{success}</div> : null}

          {isLoadingBoard ? (
            <div className="page-card">
              <Loading label="Загрузка расписания..." />
            </div>
          ) : (
            <WeeklyScheduleBoard
              board={board}
              lessonCount={lessonCount}
              hasActiveFilters={selectedGroupId !== allValue || selectedTeacherId !== allValue || selectedAudienceId !== allValue}
              onCreate={openCreateModal}
              onEdit={(lesson) => void openEditModal(lesson.scheduleId)}
              onDelete={setLessonToDelete}
            />
          )}
        </>
      )}

      <ScheduleFormModal
        title={editingSchedule ? 'Изменить занятие' : 'Создать занятие'}
        initialValues={editingSchedule ?? undefined}
        isOpen={isModalOpen}
        isSubmitting={isSubmitting}
        onClose={() => setIsModalOpen(false)}
        onSubmit={submitForm}
      />

      {lessonToDelete ? (
        <div className="modal-backdrop" role="presentation">
          <div className="modal-card confirm-modal" role="dialog" aria-modal="true" aria-label="Удалить занятие?">
            <div className="confirm-icon">
              <AlertTriangle size={24} />
            </div>
            <h2>Удалить занятие?</h2>
            <p className="confirm-copy">
              {lessonToDelete.subjectName} · {lessonToDelete.groupName} · {lessonToDelete.audienceNumber}
            </p>
            {deleteError ? <div className="modal-error">{deleteError}</div> : null}
            <div className="modal-actions confirm-actions">
              <button className="ghost-button" type="button" onClick={() => setLessonToDelete(null)} disabled={isDeleting}>
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

type FiltersBarProps = {
  weekOptions: SelectOption[];
  groupOptions: SelectOption[];
  teacherOptions: SelectOption[];
  audienceOptions: SelectOption[];
  selectedWeekId: string;
  selectedGroupId: string;
  selectedTeacherId: string;
  selectedAudienceId: string;
  lessonCount: number;
  isLoading: boolean;
  onWeekChange: (value: string) => void;
  onGroupChange: (value: string) => void;
  onTeacherChange: (value: string) => void;
  onAudienceChange: (value: string) => void;
  onCurrentWeek: () => void;
  onRefresh: () => void;
  onCreate: () => void;
};

type ScheduleFormModalProps = {
  title: string;
  initialValues?: ScheduleFormValues;
  isOpen: boolean;
  isSubmitting?: boolean;
  onClose: () => void;
  onSubmit: (values: Record<string, unknown>) => Promise<void>;
};

function ScheduleFormModal({ title, initialValues, isOpen, isSubmitting, onClose, onSubmit }: ScheduleFormModalProps) {
  const [weeks, setWeeks] = useState<WeekLookupDto[]>([]);
  const [subjects, setSubjects] = useState<SubjectLookupDto[]>([]);
  const [disciplineOptions, setDisciplineOptions] = useState<DisciplineScheduleOption[]>([]);
  const [audiences, setAudiences] = useState<AudienceLookupDto[]>([]);
  const [selectedWeekId, setSelectedWeekId] = useState<number | null>(null);
  const [selectedDay, setSelectedDay] = useState<number | null>(null);
  const [selectedPara, setSelectedPara] = useState<number | null>(null);
  const [selectedSubjectId, setSelectedSubjectId] = useState<number | null>(null);
  const [selectedTeacherId, setSelectedTeacherId] = useState<number | null>(null);
  const [selectedGroupId, setSelectedGroupId] = useState<number | null>(null);
  const [selectedDisciplineId, setSelectedDisciplineId] = useState<number | null>(null);
  const [selectedAudienceId, setSelectedAudienceId] = useState<number | null>(null);
  const [selectedLectureType, setSelectedLectureType] = useState<string | null>(null);
  const [isLoadingBase, setIsLoadingBase] = useState(false);
  const [isLoadingDisciplines, setIsLoadingDisciplines] = useState(false);
  const [isLoadingAudiences, setIsLoadingAudiences] = useState(false);
  const [formError, setFormError] = useState('');

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    setSelectedWeekId(toNumber(initialValues?.weekId));
    setSelectedDay(toNumber(initialValues?.den));
    setSelectedPara(toNumber(initialValues?.para));
    setSelectedSubjectId(toNumber(initialValues?.subjectId));
    setSelectedTeacherId(toNumber(initialValues?.teacherId));
    setSelectedGroupId(toNumber(initialValues?.groupId));
    setSelectedDisciplineId(toNumber(initialValues?.disciplineId));
    setSelectedAudienceId(toNumber(initialValues?.audienceId));
    setSelectedLectureType(toStringValue(initialValues?.lectureType));
    setFormError('');
  }, [initialValues, isOpen]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    async function loadBaseLookups() {
      setIsLoadingBase(true);
      setFormError('');

      try {
        const [weeksResponse, subjectsResponse] = await Promise.all([
          axiosClient.get<WeekLookupDto[]>('/admin-schedule-lookups/weeks'),
          axiosClient.get<SubjectLookupDto[]>('/admin-schedule-lookups/disciplines'),
        ]);

        setWeeks(weeksResponse.data);
        setSubjects(subjectsResponse.data);
      } catch (requestError) {
        setFormError(getApiError(requestError));
      } finally {
        setIsLoadingBase(false);
      }
    }

    void loadBaseLookups();
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen || !selectedSubjectId) {
      setDisciplineOptions([]);
      return;
    }

    async function loadDisciplineOptions() {
      setIsLoadingDisciplines(true);
      setFormError('');

      try {
        const response = await axiosClient.get<DisciplineScheduleOption[]>('/admin-schedule-lookups/discipline-options', {
          params: { subjectId: selectedSubjectId },
        });

        setDisciplineOptions(response.data);
      } catch (requestError) {
        setFormError(getApiError(requestError));
      } finally {
        setIsLoadingDisciplines(false);
      }
    }

    void loadDisciplineOptions();
  }, [isOpen, selectedSubjectId]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    async function loadAudiences() {
      setIsLoadingAudiences(true);

      try {
        const response = await axiosClient.get<AudienceLookupDto[]>('/admin-schedule-lookups/audiences', {
          params: { lectureType: selectedLectureType ?? undefined },
        });

        setAudiences(response.data);
      } catch (requestError) {
        setFormError(getApiError(requestError));
      } finally {
        setIsLoadingAudiences(false);
      }
    }

    void loadAudiences();
  }, [isOpen, selectedLectureType]);

  useEffect(() => {
    if (!selectedTeacherId || !selectedGroupId) {
      setSelectedDisciplineId(null);
      return;
    }

    const matchedDiscipline = disciplineOptions.find((option) => option.teacherId === selectedTeacherId && option.groupId === selectedGroupId);
    setSelectedDisciplineId(matchedDiscipline?.disciplineId ?? null);
  }, [disciplineOptions, selectedGroupId, selectedTeacherId]);

  useEffect(() => {
    if (!selectedAudienceId || audiences.length === 0) {
      return;
    }

    if (!audiences.some((audience) => audience.id === selectedAudienceId)) {
      setSelectedAudienceId(null);
    }
  }, [audiences, selectedAudienceId]);

  if (!isOpen) {
    return null;
  }

  const weekOptions = weeks.map((week) => ({
    value: String(week.id),
    label: week.displayName ?? `${week.name} · ${formatDate(week.startDate)} - ${formatDate(week.endDate)}`,
  }));

  const subjectOptions = subjects.map((subject) => ({
    value: String(subject.subjectId),
    label: subject.subjectName,
    subtitle: `${subject.semester} семестр · ${subject.controlForm}`,
  }));

  const dayOptions = [
    { value: '1', label: 'Понедельник' },
    { value: '2', label: 'Вторник' },
    { value: '3', label: 'Среда' },
    { value: '4', label: 'Четверг' },
    { value: '5', label: 'Пятница' },
    { value: '6', label: 'Суббота' },
  ];

  const paraOptions = Array.from({ length: 7 }, (_, index) => {
    const para = index + 1;
    return { value: String(para), label: `${para} пара`, subtitle: getParaTimeRange(para) };
  });

  const teacherSource = selectedGroupId ? disciplineOptions.filter((option) => option.groupId === selectedGroupId) : disciplineOptions;
  const groupSource = selectedTeacherId ? disciplineOptions.filter((option) => option.teacherId === selectedTeacherId) : disciplineOptions;
  const teacherOptions = uniqueBy(teacherSource, (option) => option.teacherId).map((option) => ({
    value: String(option.teacherId),
    label: option.teacherFullName,
    subtitle: option.facultyName,
  }));
  const groupOptions = uniqueBy(groupSource, (option) => option.groupId).map((option) => ({
    value: String(option.groupId),
    label: option.groupName,
    subtitle: option.specialityName,
  }));
  const audienceOptions = audiences.map((audience) => ({
    value: String(audience.id),
    label: audience.number,
    subtitle: audience.type,
  }));
  const lectureTypeOptions = [
    { value: 'Lecture', label: 'Лекция' },
    { value: 'Practice', label: 'Практика' },
    { value: 'Laboratory', label: 'Лабораторная' },
  ];
  const hasDisciplineMismatch = Boolean(selectedSubjectId && selectedTeacherId && selectedGroupId && !selectedDisciplineId && !isLoadingDisciplines);
  const canSubmit = Boolean(
    selectedWeekId &&
      selectedDay &&
      selectedPara &&
      selectedSubjectId &&
      selectedTeacherId &&
      selectedGroupId &&
      selectedDisciplineId &&
      selectedAudienceId &&
      selectedLectureType &&
      !isSubmitting &&
      !hasDisciplineMismatch,
  );

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!canSubmit) {
      return;
    }

    await onSubmit({
      den: selectedDay,
      para: selectedPara,
      disciplineId: selectedDisciplineId,
      teacherId: selectedTeacherId,
      audienceId: selectedAudienceId,
      groupId: selectedGroupId,
      weekId: selectedWeekId,
      lectureType: selectedLectureType,
    });
  }

  function updateSubject(value: string) {
    setSelectedSubjectId(toNumber(value));
    setSelectedTeacherId(null);
    setSelectedGroupId(null);
    setSelectedDisciplineId(null);
  }

  function updateLectureType(value: string) {
    setSelectedLectureType(value || null);
    setSelectedAudienceId(null);
  }

  return (
    <div className="modal-backdrop" role="presentation">
      <div className="modal-card schedule-form-modal" role="dialog" aria-modal="true" aria-label={title}>
        <div className="modal-header">
          <h2>{title}</h2>
          <button className="icon-button small" type="button" onClick={onClose} aria-label="Закрыть форму">
            <X size={18} />
          </button>
        </div>

        <form className="form-grid schedule-form-grid" onSubmit={handleSubmit}>
          <ScheduleSelectField label="Учебная неделя">
            <CustomSearchableSelect value={toSelectValue(selectedWeekId)} options={weekOptions} placeholder="Выберите учебную неделю" clearValue="" disabled={isLoadingBase} onChange={(value) => setSelectedWeekId(toNumber(value))} />
          </ScheduleSelectField>
          <ScheduleSelectField label="День">
            <CustomSearchableSelect value={toSelectValue(selectedDay)} options={dayOptions} placeholder="Выберите день" clearValue="" onChange={(value) => setSelectedDay(toNumber(value))} />
          </ScheduleSelectField>
          <ScheduleSelectField label="Пара">
            <CustomSearchableSelect value={toSelectValue(selectedPara)} options={paraOptions} placeholder="Выберите пару" clearValue="" onChange={(value) => setSelectedPara(toNumber(value))} />
          </ScheduleSelectField>
          <ScheduleSelectField label="Дисциплина">
            <CustomSearchableSelect value={toSelectValue(selectedSubjectId)} options={subjectOptions} placeholder="Выберите дисциплину" clearValue="" disabled={isLoadingBase} onChange={updateSubject} />
          </ScheduleSelectField>
          <ScheduleSelectField label="Преподаватель">
            <CustomSearchableSelect value={toSelectValue(selectedTeacherId)} options={teacherOptions} placeholder={selectedSubjectId ? 'Выберите преподавателя' : 'Сначала выберите дисциплину'} clearValue="" disabled={!selectedSubjectId || isLoadingDisciplines} onChange={(value) => setSelectedTeacherId(toNumber(value))} />
          </ScheduleSelectField>
          <ScheduleSelectField label="Группа">
            <CustomSearchableSelect value={toSelectValue(selectedGroupId)} options={groupOptions} placeholder={selectedSubjectId ? 'Выберите группу' : 'Сначала выберите дисциплину'} clearValue="" disabled={!selectedSubjectId || isLoadingDisciplines} onChange={(value) => setSelectedGroupId(toNumber(value))} />
          </ScheduleSelectField>
          <ScheduleSelectField label="Аудитория">
            <CustomSearchableSelect value={toSelectValue(selectedAudienceId)} options={audienceOptions} placeholder="Выберите аудиторию" clearValue="" disabled={isLoadingAudiences} onChange={(value) => setSelectedAudienceId(toNumber(value))} />
          </ScheduleSelectField>
          <ScheduleSelectField label="Тип занятия">
            <CustomSearchableSelect value={selectedLectureType ?? ''} options={lectureTypeOptions} placeholder="Выберите тип занятия" clearValue="" onChange={updateLectureType} />
          </ScheduleSelectField>

          {hasDisciplineMismatch ? (
            <div className="modal-error schedule-form-message">
              Для выбранной дисциплины не найдена учебная нагрузка с этим преподавателем и группой.
            </div>
          ) : null}
          {formError ? <div className="modal-error schedule-form-message">{formError}</div> : null}

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

function ScheduleSelectField({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="form-field schedule-select-field">
      <span>{label}</span>
      {children}
    </label>
  );
}

function ScheduleFiltersBar({
  weekOptions,
  groupOptions,
  teacherOptions,
  audienceOptions,
  selectedWeekId,
  selectedGroupId,
  selectedTeacherId,
  selectedAudienceId,
  lessonCount,
  isLoading,
  onWeekChange,
  onGroupChange,
  onTeacherChange,
  onAudienceChange,
  onCurrentWeek,
  onRefresh,
  onCreate,
}: FiltersBarProps) {
  return (
    <div className="schedule-filters-bar">
      <div className="schedule-filters">
        <CustomSearchableSelect value={selectedWeekId} options={weekOptions} placeholder="Учебная неделя" onChange={onWeekChange} />
        <CustomSearchableSelect value={selectedGroupId} options={groupOptions} placeholder="Все группы" onChange={onGroupChange} />
        <CustomSearchableSelect value={selectedTeacherId} options={teacherOptions} placeholder="Все преподаватели" onChange={onTeacherChange} />
        <CustomSearchableSelect value={selectedAudienceId} options={audienceOptions} placeholder="Все аудитории" onChange={onAudienceChange} />
      </div>

      <div className="schedule-filter-actions">
        <div className="schedule-count">
          <strong>{lessonCount}</strong>
          <span>занятий</span>
        </div>
        <button className="ghost-button schedule-action-button" type="button" onClick={onCurrentWeek}>
          <CalendarDays size={17} />
          Текущая
        </button>
        <button className="ghost-button schedule-icon-action" type="button" onClick={onRefresh} disabled={isLoading} aria-label="Обновить расписание">
          <RefreshCcw size={17} />
        </button>
        <button className="primary-button create-button" type="button" onClick={onCreate}>
          <Plus size={18} />
          Создать
        </button>
      </div>
    </div>
  );
}

type WeeklyScheduleBoardProps = {
  board: WeeklyScheduleBoardDto | null;
  lessonCount: number;
  hasActiveFilters: boolean;
  onCreate: () => void;
  onEdit: (lesson: ScheduleLessonDto) => void;
  onDelete: (lesson: ScheduleLessonDto) => void;
};

function WeeklyScheduleBoard({ board, lessonCount, hasActiveFilters, onCreate, onEdit, onDelete }: WeeklyScheduleBoardProps) {
  if (!board?.weekId) {
    return (
      <div className="schedule-empty-state page-card">
        <h3>Учебные недели не найдены</h3>
        <p>Добавьте недели в разделе "Учебные недели", чтобы сформировать расписание.</p>
      </div>
    );
  }

  if (lessonCount === 0) {
    return (
      <div className="schedule-empty-state page-card">
        <h3>{hasActiveFilters ? 'По выбранным фильтрам занятий не найдено' : 'На выбранную неделю расписание не заполнено'}</h3>
        <p>{hasActiveFilters ? 'Измените фильтры или выберите другую неделю.' : 'Создайте первое занятие или выберите другую неделю.'}</p>
        <button className="primary-button create-button" type="button" onClick={onCreate}>
          <Plus size={18} />
          Создать
        </button>
      </div>
    );
  }

  return (
    <>
      <div className="schedule-week-heading">
        <div>
          <h2>{board.weekName || 'Учебная неделя'}</h2>
          <p>
            {formatDate(board.startDate)} - {formatDate(board.endDate)}
          </p>
        </div>
      </div>

      <div className="schedule-week-grid">
        {board.days.map((day) => (
          <ScheduleDayCard key={day.day} day={day} onEdit={onEdit} onDelete={onDelete} />
        ))}
      </div>
    </>
  );
}

type ScheduleDayCardProps = {
  day: ScheduleDayDto;
  onEdit: (lesson: ScheduleLessonDto) => void;
  onDelete: (lesson: ScheduleLessonDto) => void;
};

function ScheduleDayCard({ day, onEdit, onDelete }: ScheduleDayCardProps) {
  const dayLessonCount = countDayLessons(day);

  return (
    <article className="schedule-day-card">
      <header className="schedule-day-header">
        <div>
          <h3 className="schedule-day-title">{day.dayName}</h3>
          <p className="schedule-day-date">{formatDate(day.date)}</p>
        </div>
        <span className="schedule-day-count">{dayLessonCount} занятий</span>
      </header>

      <div className="schedule-day-body">
        {day.paras.length === 0 ? (
          <div className="schedule-day-empty">Занятий нет</div>
        ) : (
          day.paras.map((para) => <ScheduleParaBlock key={para.para} para={para} onEdit={onEdit} onDelete={onDelete} />)
        )}
      </div>
    </article>
  );
}

type ScheduleParaBlockProps = {
  para: ScheduleParaDto;
  onEdit: (lesson: ScheduleLessonDto) => void;
  onDelete: (lesson: ScheduleLessonDto) => void;
};

function ScheduleParaBlock({ para, onEdit, onDelete }: ScheduleParaBlockProps) {
  return (
    <section className="schedule-para-block">
      <div className="schedule-para-meta">
        <span className="schedule-para-number">{para.para} пара</span>
        <span className="schedule-para-time">{para.timeRange}</span>
      </div>

      <div className="schedule-lessons-stack">
        {para.lessons.map((lesson) => (
          <ScheduleLessonCard key={lesson.scheduleId} lesson={lesson} onEdit={onEdit} onDelete={onDelete} />
        ))}
      </div>
    </section>
  );
}

type ScheduleLessonCardProps = {
  lesson: ScheduleLessonDto;
  onEdit: (lesson: ScheduleLessonDto) => void;
  onDelete: (lesson: ScheduleLessonDto) => void;
};

function ScheduleLessonCard({ lesson, onEdit, onDelete }: ScheduleLessonCardProps) {
  return (
    <article className="schedule-lesson-card">
      <div className="schedule-lesson-actions">
        <button className="schedule-lesson-action schedule-lesson-action--edit" type="button" onClick={() => onEdit(lesson)} aria-label="Редактировать занятие">
          <Edit2 size={14} />
        </button>
        <button className="schedule-lesson-action schedule-lesson-action--delete" type="button" onClick={() => onDelete(lesson)} aria-label="Удалить занятие">
          <Trash2 size={14} />
        </button>
      </div>

      <h4 className="schedule-lesson-title">{lesson.subjectName || 'Предмет не указан'}</h4>
      <div className="schedule-lesson-details">
        <span>{lesson.groupName || 'Группа не указана'} · {lesson.teacherFullName || 'Преподаватель не указан'}</span>
        <span>Аудитория: {lesson.audienceNumber || 'не указана'}</span>
      </div>
      <div className="schedule-lesson-footer">
        <span className="schedule-lesson-type">{formatLectureType(lesson.lectureType)}</span>
        {lesson.audienceType ? <span className="schedule-audience-type">{lesson.audienceType}</span> : null}
      </div>
    </article>
  );
}

function createLookupOptions(allLabel: string, items: LookupDto[]): SelectOption[] {
  return [{ value: allValue, label: allLabel }, ...items.map((item) => ({ value: String(item.id), label: item.name }))];
}

function uniqueBy<T>(items: T[], getKey: (item: T) => number) {
  const seen = new Set<number>();

  return items.filter((item) => {
    const key = getKey(item);

    if (seen.has(key)) {
      return false;
    }

    seen.add(key);
    return true;
  });
}

function toNumber(value: unknown) {
  if (value === null || value === undefined || value === '') {
    return null;
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function toStringValue(value: unknown) {
  if (typeof value !== 'string' || value.trim() === '') {
    return null;
  }

  return value;
}

function toSelectValue(value: number | null) {
  return value === null ? '' : String(value);
}

function countDayLessons(day: ScheduleDayDto) {
  return day.paras.reduce((total, para) => total + para.lessons.length, 0);
}

function formatDate(value?: string | null) {
  if (!value) {
    return '';
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat('ru-RU').format(date);
}

function formatLectureType(value: string) {
  const labels: Record<string, string> = {
    Lecture: 'Лекция',
    Practice: 'Практика',
    Laboratory: 'Лабораторная',
  };

  return labels[value] ?? value;
}

function getParaTimeRange(para: number) {
  const ranges: Record<number, string> = {
    1: '08:00 - 09:20',
    2: '09:30 - 10:50',
    3: '11:00 - 12:20',
    4: '12:40 - 14:00',
    5: '14:10 - 15:30',
    6: '15:40 - 17:00',
    7: '17:10 - 18:30',
  };

  return ranges[para] ?? '';
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
