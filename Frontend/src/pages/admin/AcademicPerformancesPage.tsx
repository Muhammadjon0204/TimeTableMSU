import { ArrowLeft, ChevronRight, Download, GraduationCap, Info, Printer, RefreshCw, Search, UsersRound } from 'lucide-react';
import type { ReactNode } from 'react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { axiosClient, getApiError } from '../../api/axiosClient';
import { ErrorAlert } from '../../components/ErrorAlert';
import { Loading } from '../../components/Loading';

type FacultySort = 'name' | 'students';
type AcademicView = 'faculties' | 'groups' | 'students' | 'student-journal';
type StudentSort = 'name' | 'email' | 'average';

type FacultyCard = {
  facultyId: number;
  facultyName: string;
  specialitiesCount: number;
  groupsCount: number;
  studentsCount: number;
};

type GroupCard = {
  groupId: number;
  groupName: string;
  course: number;
  specialityName: string;
  studentsCount: number;
  marksCount: number;
};

type StudentListItem = {
  studentId: number;
  fullName: string;
  email?: string | null;
  telephone?: string | null;
  address?: string | null;
  marksCount?: number;
  averageMark?: number | null;
};

type StudentJournal = {
  header: StudentJournalHeader;
  semesters: StudentSemesterJournal[];
};

type StudentJournalHeader = {
  studentId: number;
  fullName: string;
  groupName: string;
  specialityName: string;
  facultyName: string;
  academicYear?: string | null;
};

type StudentSemesterJournal = {
  semester: number;
  academicYear?: string | null;
  rows: StudentJournalRow[];
};

type StudentJournalRow = {
  rowNumber: number;
  subjectName: string;
  controlForm: 'Credit' | 'Exam' | string;
  teacherFullName: string;
  firstControlWork?: number | null;
  secondControlWork?: number | null;
  creditResult?: string | null;
  examResult?: string | null;
  finalDisplayValue: string;
  statusDisplayValue: string;
  hasRetake?: boolean;
  retakeDisplayValue?: string;
  retakeStatusDisplayValue?: string | null;
  retakeRoundDisplayValue?: string | null;
  note?: string | null;
  resultType?: string | null;
  retakeType?: string | null;
};

export function AcademicPerformancesPage() {
  return (
    <section className="page-stack academic-journal-page">
      <AcademicJournalModule />
    </section>
  );
}

function AcademicJournalModule() {
  const [faculties, setFaculties] = useState<FacultyCard[]>([]);
  const [groups, setGroups] = useState<GroupCard[]>([]);
  const [students, setStudents] = useState<StudentListItem[]>([]);
  const [journal, setJournal] = useState<StudentJournal | null>(null);
  const [view, setView] = useState<AcademicView>('faculties');
  const [selectedFaculty, setSelectedFaculty] = useState<FacultyCard | null>(null);
  const [selectedGroup, setSelectedGroup] = useState<GroupCard | null>(null);
  const [facultySearch, setFacultySearch] = useState('');
  const [facultySort, setFacultySort] = useState<FacultySort>('name');
  const [groupSearch, setGroupSearch] = useState('');
  const [studentSearch, setStudentSearch] = useState('');
  const [studentSort, setStudentSort] = useState<StudentSort>('name');
  const [loading, setLoading] = useState({ faculties: false, groups: false, students: false, journal: false });
  const [error, setError] = useState('');

  const fetchFaculties = useCallback(async () => {
    setLoading((current) => ({ ...current, faculties: true }));
    setError('');

    try {
      const response = await axiosClient.get<FacultyCard[]>('/admin-academic-journal/faculties');
      setFaculties(response.data);
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setLoading((current) => ({ ...current, faculties: false }));
    }
  }, []);

  useEffect(() => {
    void fetchFaculties();
  }, [fetchFaculties]);

  async function openFaculty(faculty: FacultyCard) {
    setSelectedFaculty(faculty);
    setView('groups');
    setSelectedGroup(null);
    setJournal(null);
    setGroups([]);
    setStudents([]);
    setGroupSearch('');
    setStudentSearch('');
    setError('');
    setLoading((current) => ({ ...current, groups: true }));

    try {
      const response = await axiosClient.get<GroupCard[]>(`/admin-academic-journal/faculties/${faculty.facultyId}/groups`);
      setGroups(response.data);
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setLoading((current) => ({ ...current, groups: false }));
    }
  }

  async function openGroup(group: GroupCard) {
    setSelectedGroup(group);
    setView('students');
    setJournal(null);
    setStudents([]);
    setStudentSearch('');
    setError('');
    setLoading((current) => ({ ...current, students: true }));

    try {
      const response = await axiosClient.get<StudentListItem[]>(`/admin-academic-journal/groups/${group.groupId}/students`);
      setStudents(response.data);
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setLoading((current) => ({ ...current, students: false }));
    }
  }

  async function openStudentJournal(student: StudentListItem) {
    setView('student-journal');
    setJournal(null);
    setError('');
    setLoading((current) => ({ ...current, journal: true }));

    try {
      const response = await axiosClient.get<StudentJournal>(`/admin-academic-journal/students/${student.studentId}/journal`);
      setJournal(response.data);
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setLoading((current) => ({ ...current, journal: false }));
    }
  }

  const visibleFaculties = useMemo(() => {
    const search = facultySearch.trim().toLowerCase();
    const filtered = faculties.filter((faculty) => faculty.facultyName.toLowerCase().includes(search));

    return filtered.sort((first, second) => {
      if (facultySort === 'students') {
        return second.studentsCount - first.studentsCount || first.facultyName.localeCompare(second.facultyName, 'ru');
      }

      return first.facultyName.localeCompare(second.facultyName, 'ru');
    });
  }, [faculties, facultySearch, facultySort]);

  const visibleGroups = useMemo(() => {
    const search = groupSearch.trim().toLowerCase();

    return groups.filter((group) => group.groupName.toLowerCase().includes(search) || group.specialityName.toLowerCase().includes(search));
  }, [groupSearch, groups]);

  const visibleStudents = useMemo(() => {
    const search = studentSearch.trim().toLowerCase();

    const filtered = students
      .filter((student) => {
        return [student.fullName, student.telephone, student.email, student.address]
          .filter(Boolean)
          .some((value) => String(value).toLowerCase().includes(search));
      })
      .sort((first, second) => {
        if (studentSort === 'email') {
          return (first.email || '').localeCompare(second.email || '', 'ru') || first.fullName.localeCompare(second.fullName, 'ru');
        }

        if (studentSort === 'average') {
          return (second.averageMark ?? -1) - (first.averageMark ?? -1) || first.fullName.localeCompare(second.fullName, 'ru');
        }

        return first.fullName.localeCompare(second.fullName, 'ru');
      });

    return filtered;
  }, [studentSearch, studentSort, students]);

  function backToFaculties() {
    setSelectedFaculty(null);
    setSelectedGroup(null);
    setJournal(null);
    setView('faculties');
  }

  function backToGroups() {
    setSelectedGroup(null);
    setJournal(null);
    setView(selectedFaculty ? 'groups' : 'faculties');
  }

  function backToStudents() {
    setJournal(null);
    setView(selectedGroup ? 'students' : 'groups');
  }

  return (
    <div className="journal-shell journal-registry-shell">
      {error ? <ErrorAlert message={error} /> : null}

      {view === 'faculties' ? (
        <JournalSection
          title="Факультеты"
          toolbar={
            <JournalToolbar
              searchValue={facultySearch}
              searchPlaceholder="Поиск по факультетам..."
              onSearch={setFacultySearch}
              controls={
                <select value={facultySort} onChange={(event) => setFacultySort(event.target.value as FacultySort)}>
                  <option value="name">По названию</option>
                  <option value="students">По студентам</option>
                </select>
              }
            />
          }
        >
          {loading.faculties ? (
            <Loading label="Загрузка факультетов..." />
          ) : visibleFaculties.length === 0 ? (
            <EmptyState title="Факультеты не найдены" />
          ) : (
            <AcademicFacultyGrid faculties={visibleFaculties} onOpen={(faculty) => void openFaculty(faculty)} />
          )}
        </JournalSection>
      ) : null}

      {view === 'groups' && selectedFaculty ? (
        <JournalSection
          title={`Группы: ${selectedFaculty.facultyName}`}
          action={<BackButton label="Назад к факультетам" onClick={backToFaculties} />}
          toolbar={<JournalToolbar searchValue={groupSearch} searchPlaceholder="Поиск по группам..." onSearch={setGroupSearch} />}
        >
          {loading.groups ? (
            <Loading label="Загрузка групп..." />
          ) : visibleGroups.length === 0 ? (
            <EmptyState title="Группы не найдены" />
          ) : (
            <AcademicGroupGrid groups={visibleGroups} onOpen={(group) => void openGroup(group)} />
          )}
        </JournalSection>
      ) : null}

      {view === 'students' && selectedGroup ? (
        <AcademicStudentsScreen
          group={selectedGroup}
          isStudentsLoading={loading.students}
          onBackToGroups={backToGroups}
          onSearch={setStudentSearch}
          onSort={setStudentSort}
          onSelectStudent={(student) => void openStudentJournal(student)}
          searchValue={studentSearch}
          sortValue={studentSort}
          students={visibleStudents}
          totalStudents={students.length}
        />
      ) : null}

      {view === 'student-journal' && selectedGroup ? (
        <StudentJournalFullPage
          isLoading={loading.journal}
          journal={journal}
          onBackToStudents={backToStudents}
        />
      ) : null}
    </div>
  );
}

function AcademicFacultyGrid({ faculties, onOpen }: { faculties: FacultyCard[]; onOpen: (faculty: FacultyCard) => void }) {
  return (
    <div className="academic-card-grid">
      {faculties.map((faculty) => (
        <button className="academic-choice-card" type="button" key={faculty.facultyId} onClick={() => onOpen(faculty)}>
          <div className="academic-choice-card__top">
            <span className="academic-choice-card__icon">
              <GraduationCap size={20} />
            </span>
            <strong>{faculty.facultyName}</strong>
          </div>
          <dl>
            <Stat label="Специальности" value={faculty.specialitiesCount} />
            <Stat label="Группы" value={faculty.groupsCount} />
            <Stat label="Студенты" value={faculty.studentsCount} />
          </dl>
          <span className="academic-open-link">
            Открыть <ChevronRight size={16} />
          </span>
        </button>
      ))}
    </div>
  );
}

function AcademicGroupGrid({ groups, onOpen }: { groups: GroupCard[]; onOpen: (group: GroupCard) => void }) {
  return (
    <div className="academic-card-grid">
      {groups.map((group) => (
        <button className="academic-choice-card academic-choice-card--compact" type="button" key={group.groupId} onClick={() => onOpen(group)}>
          <div className="academic-choice-card__top">
            <span className="academic-choice-card__icon">
              <UsersRound size={20} />
            </span>
            <div>
              <strong>{group.groupName}</strong>
              <p>{group.specialityName}</p>
            </div>
          </div>
          <dl>
            <Stat label="Курс" value={group.course} />
            <Stat label="Студенты" value={group.studentsCount} />
            <Stat label="Оценки" value={group.marksCount} />
          </dl>
          <span className="academic-open-link">
            Открыть <ChevronRight size={16} />
          </span>
        </button>
      ))}
    </div>
  );
}

function AcademicStudentsScreen({
  group,
  isStudentsLoading,
  onBackToGroups,
  onSearch,
  onSort,
  onSelectStudent,
  searchValue,
  sortValue,
  students,
  totalStudents,
}: {
  group: GroupCard;
  isStudentsLoading: boolean;
  onBackToGroups: () => void;
  onSearch: (value: string) => void;
  onSort: (value: StudentSort) => void;
  onSelectStudent: (student: StudentListItem) => void;
  searchValue: string;
  sortValue: StudentSort;
  students: StudentListItem[];
  totalStudents: number;
}) {
  return (
    <JournalSection
      title={`Студенты группы ${group.groupName}`}
      subtitle={group.specialityName}
      action={<BackButton label="Назад к группам" onClick={onBackToGroups} />}
      toolbar={
        <JournalToolbar
          searchValue={searchValue}
          searchPlaceholder="Поиск по студентам..."
          onSearch={onSearch}
          controls={
            <>
              <select value={sortValue} onChange={(event) => onSort(event.target.value as StudentSort)}>
                <option value="name">По ФИО</option>
                <option value="email">По email</option>
                <option value="average">По среднему баллу</option>
              </select>
              <span className="academic-count-pill">{totalStudents} студентов</span>
            </>
          }
        />
      }
    >
      {isStudentsLoading ? (
        <Loading label="Загрузка студентов..." />
      ) : students.length === 0 ? (
        <EmptyState title="В этой группе пока нет студентов" />
      ) : (
        <AcademicStudentsTable students={students} onOpen={onSelectStudent} />
      )}
    </JournalSection>
  );
}

function AcademicStudentsTable({ students, onOpen }: { students: StudentListItem[]; onOpen: (student: StudentListItem) => void }) {
  return (
    <div className="academic-students-table-card">
      <table className="academic-students-table">
        <thead>
          <tr>
            <th>№</th>
            <th>ФИО</th>
            <th>Телефон</th>
            <th>Email</th>
            <th>Адрес</th>
            <th>Оценок</th>
            <th>Средний балл</th>
          </tr>
        </thead>
        <tbody>
          {students.map((student, index) => (
            <tr key={student.studentId} onClick={() => onOpen(student)}>
              <td>{index + 1}</td>
              <td>
                <strong className="academic-cell-ellipsis" title={student.fullName}>
                  {student.fullName}
                </strong>
              </td>
              <td>{student.telephone || '—'}</td>
              <td>
                <span className="academic-cell-ellipsis" title={student.email || '—'}>
                  {student.email || '—'}
                </span>
              </td>
              <td>
                <span className="academic-cell-ellipsis" title={student.address || '—'}>
                  {student.address || '—'}
                </span>
              </td>
              <td>{student.marksCount ?? '—'}</td>
              <td>{formatAverageMark(student.averageMark)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function StudentJournalFullPage({
  isLoading,
  journal,
  onBackToStudents,
}: {
  isLoading: boolean;
  journal: StudentJournal | null;
  onBackToStudents: () => void;
}) {
  return (
    <div className="student-journal-fullscreen">
      {isLoading ? <Loading label="Загрузка ведомости..." /> : null}
      {!isLoading && !journal ? <EmptyState title="Для этого студента пока нет записей успеваемости" /> : null}
      {!isLoading && journal ? <StudentJournalView journal={journal} onBackToStudents={onBackToStudents} /> : null}
    </div>
  );
}

function StudentJournalView({ journal, onBackToStudents }: { journal: StudentJournal; onBackToStudents: () => void }) {
  const [activeSemester, setActiveSemester] = useState<number | null>(journal.semesters[0]?.semester ?? null);
  const currentSemester = journal.semesters.find((semester) => semester.semester === activeSemester);

  useEffect(() => {
    setActiveSemester(journal.semesters[0]?.semester ?? null);
  }, [journal]);

  return (
    <div className="academic-statement">
      <AcademicStudentHeader header={journal.header} onBackToStudents={onBackToStudents} />

      {journal.semesters.length === 0 ? (
        <EmptyState title="У студента пока нет оценок" />
      ) : (
        <>
          <AcademicSemesterTabs
            academicYear={currentSemester?.academicYear ?? journal.header.academicYear ?? '2024/2025'}
            activeSemester={activeSemester}
            semesters={journal.semesters}
            onChange={setActiveSemester}
          />
          <AcademicJournalTable rows={currentSemester?.rows ?? []} />
          <div className="academic-table-footer">
            <span>
              <Info size={15} />
              Контрольные работы являются необязательными. Итог определяется формой контроля: зачет или экзамен.
            </span>
            <span>
              Дата обновления: {new Date().toLocaleDateString('ru-RU')} <RefreshCw size={15} />
            </span>
          </div>
        </>
      )}
    </div>
  );
}

function AcademicStudentHeader({ header, onBackToStudents }: { header: StudentJournalHeader; onBackToStudents: () => void }) {
  return (
    <header className="academic-statement-header">
      <div className="academic-statement-student">
        <span>{getInitials(header.fullName)}</span>
        <div>
          <h2>{header.fullName}</h2>
          <p>
            Группа: {header.groupName || '—'} / Специальность: {header.specialityName || '—'} / Факультет: {header.facultyName || '—'}
          </p>
        </div>
      </div>

      <div className="academic-statement-actions">
        <button type="button" onClick={onBackToStudents}>
          <ArrowLeft size={16} />
          Назад к студентам
        </button>
        <button type="button">
          <Printer size={16} />
          Печать
        </button>
        <button type="button">
          <Download size={16} />
          Экспорт
        </button>
      </div>
    </header>
  );
}

function AcademicSemesterTabs({
  academicYear,
  activeSemester,
  semesters,
  onChange,
}: {
  academicYear: string;
  activeSemester: number | null;
  semesters: StudentSemesterJournal[];
  onChange: (semester: number) => void;
}) {
  return (
    <div className="academic-semester-bar">
      <div className="academic-semester-tabs" role="tablist" aria-label="Семестры">
        {semesters.map((semester) => (
          <button
            className={semester.semester === activeSemester ? 'active' : ''}
            type="button"
            key={semester.semester}
            onClick={() => onChange(semester.semester)}
          >
            {semester.semester} семестр
          </button>
        ))}
      </div>
      <label>
        Учебный год:
        <select value={academicYear} onChange={() => undefined}>
          <option value={academicYear}>{academicYear}</option>
        </select>
      </label>
    </div>
  );
}

function AcademicJournalTable({ rows }: { rows: StudentJournalRow[] }) {
  return (
    <div className="academic-table-shell">
      <table className="academic-table academic-statement-table">
        <colgroup>
          <col className="academic-col-number" />
          <col className="academic-col-subject" />
          <col className="academic-col-control" />
          <col className="academic-col-teacher" />
          <col className="academic-col-small" />
          <col className="academic-col-small" />
          <col className="academic-col-result" />
          <col className="academic-col-status" />
          <col className="academic-col-retake" />
          <col className="academic-col-round" />
          <col className="academic-col-note" />
        </colgroup>
        <thead>
          <tr>
            <th>№</th>
            <th>Дисциплина</th>
            <th>Форма контроля</th>
            <th>Преподаватель</th>
            <th title="Первая контрольная работа. Необязательное поле.">1 кр</th>
            <th title="Вторая контрольная работа. Необязательное поле.">2 кр</th>
            <th title="Итоговый результат по форме контроля.">Итог</th>
            <th>Статус</th>
            <th title="Показывает, требуется ли пересдача.">Пересдача</th>
            <th title="Текущий этап пересдачи.">Тур</th>
            <th>Примечание</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={`${row.rowNumber}-${row.subjectName}`}>
              <td>{row.rowNumber}</td>
              <td>
                <strong className="academic-subject-cell" title={row.subjectName}>
                  {row.subjectName}
                </strong>
              </td>
              <td className="academic-table__cell--fill">
                <ControlFormBadge controlForm={row.controlForm} />
              </td>
              <td>
                <span className="academic-cell-ellipsis" title={row.teacherFullName || '—'}>
                  {row.teacherFullName || '—'}
                </span>
              </td>
              <td>{formatNullableNumber(row.firstControlWork)}</td>
              <td>{formatNullableNumber(row.secondControlWork)}</td>
              <td className="academic-table__cell--fill">
                <ResultBadge row={row} />
              </td>
              <td className="academic-table__cell--fill">
                <StatusBadge value={row.statusDisplayValue} />
              </td>
              <td className="academic-table__cell--fill">
                <RetakeBadge value={row.retakeStatusDisplayValue || row.retakeDisplayValue || 'Нет'} type={row.retakeType} />
              </td>
              <td className="academic-table__cell--fill">
                <RetakeRoundBadge value={row.retakeRoundDisplayValue || '—'} type={row.retakeType} />
              </td>
              <td className="academic-note-cell">{row.note || '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function JournalSection({
  title,
  subtitle,
  action,
  toolbar,
  children,
}: {
  title?: string;
  subtitle?: string;
  action?: ReactNode;
  toolbar?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section className="journal-section academic-minimal-section">
      {(title || action) && (
        <div className="journal-section-head">
          <div>
            {title ? <h2>{title}</h2> : null}
            {subtitle ? <p>{subtitle}</p> : null}
          </div>
          {action}
        </div>
      )}
      {toolbar}
      {children}
    </section>
  );
}

function JournalToolbar({
  searchValue,
  searchPlaceholder,
  controls,
  onSearch,
}: {
  searchValue: string;
  searchPlaceholder: string;
  controls?: ReactNode;
  onSearch: (value: string) => void;
}) {
  return (
    <div className="academic-toolbar">
      <label className="academic-search">
        <Search size={17} />
        <input value={searchValue} placeholder={searchPlaceholder} onChange={(event) => onSearch(event.target.value)} />
      </label>
      {controls ? <div className="academic-toolbar__controls">{controls}</div> : null}
    </div>
  );
}

function Stat({ label, value }: { label: string; value: number }) {
  return (
    <div>
      <dt>{label}</dt>
      <dd>{value}</dd>
    </div>
  );
}

function BackButton({ label, onClick }: { label: string; onClick: () => void }) {
  return (
    <button className="academic-back-button" type="button" onClick={onClick}>
      <ArrowLeft size={16} />
      {label}
    </button>
  );
}

function EmptyState({ title }: { title: string }) {
  return <div className="journal-empty academic-empty">{title}</div>;
}

function ControlFormBadge({ controlForm }: { controlForm: string }) {
  const isExam = controlForm === 'Exam';
  return <span className={isExam ? 'academic-cell-fill academic-cell-fill--primary' : 'academic-cell-fill academic-cell-fill--success'}>{isExam ? 'Экзамен' : 'Зачет'}</span>;
}

function ResultBadge({ row }: { row: StudentJournalRow }) {
  return (
    <span className={`academic-cell-fill ${getFillToneClass(row.resultType) || getResultTone(row)}`} title={row.finalDisplayValue || '—'}>
      {shortenFinalResult(row.finalDisplayValue) || '—'}
    </span>
  );
}

function StatusBadge({ value }: { value: string }) {
  return <span className={`academic-cell-fill ${getStatusTone(value)}`}>{value || 'Не выставлена'}</span>;
}

function RetakeBadge({ value, type }: { value: string; type?: string | null }) {
  return <span className={`academic-cell-fill ${getFillToneClass(type) || getRetakeTone(value)}`}>{value}</span>;
}

function RetakeRoundBadge({ value, type }: { value: string; type?: string | null }) {
  return <span className={`academic-cell-fill ${value === '—' ? 'academic-cell-fill--muted' : getFillToneClass(type) || getRoundTone(value)}`}>{value}</span>;
}

function getFillToneClass(type?: string | null) {
  switch (type) {
    case 'success':
      return 'academic-cell-fill--success';
    case 'primary':
      return 'academic-cell-fill--primary';
    case 'warning':
      return 'academic-cell-fill--warning';
    case 'danger':
      return 'academic-cell-fill--danger';
    case 'muted':
      return 'academic-cell-fill--muted';
    default:
      return '';
  }
}

function getResultTone(row: StudentJournalRow) {
  const result = row.examResult ?? row.creditResult;

  if (result === 'Excellent' || result === 'Good' || result === 'Passed') {
    return result === 'Good' ? 'academic-cell-fill--primary' : 'academic-cell-fill--success';
  }

  if (result === 'Satisfactory') {
    return 'academic-cell-fill--warning';
  }

  if (result === 'Unsatisfactory' || result === 'NotPassed') {
    return 'academic-cell-fill--danger';
  }

  if (result === 'NotAllowed') {
    return 'academic-cell-fill--warning';
  }

  return 'academic-cell-fill--muted';
}

function getStatusTone(value: string) {
  if (value === 'Сдано' || value === 'Зачтено') {
    return 'academic-cell-fill--success';
  }

  if (value === 'Не сдано' || value === 'Не зачтено') {
    return 'academic-cell-fill--danger';
  }

  if (value === 'Недопуск') {
    return 'academic-cell-fill--warning';
  }

  return 'academic-cell-fill--muted';
}

function getRetakeTone(value: string) {
  if (value === 'Нет') {
    return 'academic-cell-fill--muted';
  }

  if (value === 'Закрыта') {
    return 'academic-cell-fill--success';
  }

  if (value === 'В процессе') {
    return 'academic-cell-fill--primary';
  }

  if (value === 'Недопуск') {
    return 'academic-cell-fill--warning';
  }

  return 'academic-cell-fill--danger';
}

function getRoundTone(value: string) {
  if (value === '3 тур') {
    return 'academic-cell-fill--warning';
  }

  if (value === 'Комиссия') {
    return 'academic-cell-fill--danger';
  }

  return 'academic-cell-fill--primary';
}

function formatNullableNumber(value?: number | null) {
  return value == null ? '—' : String(value);
}

function formatAverageMark(value?: number | null) {
  return value == null ? '—' : value.toFixed(2).replace(/\.?0+$/, '');
}

function shortenFinalResult(value?: string | null) {
  if (!value) {
    return value;
  }

  if (value === 'Неудовлетворительно') {
    return 'Неуд.';
  }

  return value === 'Удовлетворительно' ? 'Удовл.' : value;
}

function getInitials(fullName: string) {
  return fullName
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase();
}
