import {
  Activity,
  ArrowRight,
  BookOpen,
  CalendarClock,
  ClipboardCheck,
  GraduationCap,
  ShieldCheck,
  UserRoundCheck,
  UsersRound,
} from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { axiosClient, getApiError } from '../../api/axiosClient';
import { ErrorAlert } from '../../components/ErrorAlert';
import { Loading } from '../../components/Loading';
import { StatCard } from '../../components/StatCard';

type DashboardStats = {
  students: number;
  teachers: number;
  groups: number;
  subjects: number;
  schedules: number;
  attendance: number;
};

type ListResponse<T> = {
  items?: T[];
  totalCount?: number;
};

type GroupLookup = {
  id: number;
  name: string;
};

type AdminAttendanceDay = {
  day: number;
  dayName: string;
  expected: number;
  present: number;
  late: number;
  absent: number;
  attendancePercent: number;
};

type AdminAttendanceWeekly = {
  weekId?: number | null;
  weekName?: string | null;
  groupId?: number | null;
  groupName: string;
  days: AdminAttendanceDay[];
};

type AttendanceChartPoint = {
  day: string;
  present: number;
  late: number;
  absent: number;
};

const initialStats: DashboardStats = {
  students: 0,
  teachers: 0,
  groups: 0,
  subjects: 0,
  schedules: 0,
  attendance: 0,
};

export function AdminDashboard() {
  const [stats, setStats] = useState<DashboardStats>(initialStats);
  const [groups, setGroups] = useState<GroupLookup[]>([]);
  const [weeklyAnalytics, setWeeklyAnalytics] = useState<AdminAttendanceWeekly | null>(null);
  const [period, setPeriod] = useState('week');
  const [selectedGroupId, setSelectedGroupId] = useState('all');
  const [isLoading, setIsLoading] = useState(true);
  const [isAnalyticsLoading, setIsAnalyticsLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadDashboard() {
      setIsLoading(true);
      setError('');

      try {
        const [students, teachers, groupsCount, subjects, schedules, attendancesCount, groupItems] = await Promise.all([
          getCount('/students'),
          getCount('/teachers'),
          getCount('/groups'),
          getCount('/subjects'),
          getCount('/schedules'),
          getCount('/attendances'),
          getGroupLookups(),
        ]);

        setStats({
          students,
          teachers,
          groups: groupsCount,
          subjects,
          schedules,
          attendance: attendancesCount,
        });
        setGroups(groupItems);
      } catch (requestError) {
        setError(getApiError(requestError));
      } finally {
        setIsLoading(false);
      }
    }

    void loadDashboard();
  }, []);

  useEffect(() => {
    async function loadAttendanceAnalytics() {
      setIsAnalyticsLoading(true);
      setError('');

      try {
        setWeeklyAnalytics(await getAttendanceWeekly(selectedGroupId));
      } catch (requestError) {
        setError(getApiError(requestError));
      } finally {
        setIsAnalyticsLoading(false);
      }
    }

    void loadAttendanceAnalytics();
  }, [selectedGroupId]);

  const chartData = toChartData(weeklyAnalytics);
  const hasExpectedAttendance = weeklyAnalytics?.days.some((day) => day.expected > 0) ?? false;
  const attendancePercent = getWeeklyAttendancePercent(weeklyAnalytics);
  const attendanceStatus = attendancePercent === null ? 'В пределах нормы' : `${attendancePercent}% присутствия`;

  if (isLoading) {
    return <Loading label="Загрузка панели администратора..." />;
  }

  return (
    <section className="page-stack">
      {error ? <ErrorAlert message={error} /> : null}

      <div className="stats-grid">
        <StatCard title="Всего студентов" value={stats.students} detail="зарегистрированных профилей" icon={GraduationCap} variant="students" />
        <StatCard title="Всего преподавателей" value={stats.teachers} detail="преподавателей" icon={UserRoundCheck} variant="teachers" />
        <StatCard title="Всего групп" value={stats.groups} detail="активных групп" icon={UsersRound} variant="groups" />
        <StatCard title="Всего предметов" value={stats.subjects} detail="учебных предметов" icon={BookOpen} variant="subjects" />
        <StatCard title="Активных расписаний" value={stats.schedules} detail="запланированных занятий" icon={CalendarClock} variant="schedules" />
        <StatCard title="Посещаемость за неделю" value={stats.attendance} detail="записей посещаемости" icon={ClipboardCheck} variant="attendance" />
      </div>

      <div className="dashboard-grid">
        <article className="attendance-analytics-card wide-card">
          <div className="attendance-analytics-header">
            <div>
              <p className="eyebrow">Учебная активность</p>
              <h2 className="attendance-analytics-title">Анализ посещаемости</h2>
              <p className="attendance-analytics-subtitle">Динамика посещаемости студентов за неделю</p>
            </div>
            <div className="attendance-analytics-filters">
              <select
                className="attendance-analytics-select"
                value={period}
                onChange={(event) => setPeriod(event.target.value)}
                aria-label="Период аналитики"
              >
                <option value="week">Неделя</option>
                <option value="month" disabled>
                  Месяц
                </option>
                <option value="semester" disabled>
                  Семестр
                </option>
              </select>
              <select
                className="attendance-analytics-select"
                value={selectedGroupId}
                onChange={(event) => setSelectedGroupId(event.target.value)}
                aria-label="Группа"
              >
                <option value="all">Все группы</option>
                {groups.map((group) => (
                  <option key={group.id} value={group.id}>
                    {group.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="attendance-analytics-legend" aria-label="Легенда графика">
            <span>
              <i className="legend-dot present" /> Пришли
            </span>
            <span>
              <i className="legend-dot late" /> Опоздали
            </span>
            <span>
              <i className="legend-dot absent" /> Не пришли
            </span>
          </div>

          <div className="attendance-chart-shell">
            {isAnalyticsLoading ? (
              <div className="attendance-chart-loading">Загрузка аналитики...</div>
            ) : (
              <ResponsiveContainer width="100%" height={280}>
                <LineChart data={chartData} margin={{ top: 12, right: 18, left: -18, bottom: 0 }}>
                  <CartesianGrid stroke="#E6EAF0" strokeDasharray="4 4" vertical={false} />
                  <XAxis dataKey="day" axisLine={false} tickLine={false} tick={{ fill: '#6B7280', fontSize: 13 }} />
                  <YAxis allowDecimals={false} axisLine={false} tickLine={false} tick={{ fill: '#6B7280', fontSize: 13 }} />
                  <Tooltip
                    contentStyle={{
                      border: '1px solid #E6EAF0',
                      borderRadius: 12,
                      boxShadow: '0 12px 30px rgba(15, 23, 42, 0.08)',
                    }}
                  />
                  <Line
                    type="monotone"
                    dataKey="present"
                    name="Пришли"
                    stroke="#2563EB"
                    strokeWidth={3}
                    dot={{ r: 4, fill: '#2563EB', strokeWidth: 0 }}
                    activeDot={{ r: 6 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="late"
                    name="Опоздали"
                    stroke="#F59E0B"
                    strokeWidth={3}
                    dot={{ r: 4, fill: '#F59E0B', strokeWidth: 0 }}
                    activeDot={{ r: 6 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="absent"
                    name="Не пришли"
                    stroke="#EF4444"
                    strokeWidth={3}
                    dot={{ r: 4, fill: '#EF4444', strokeWidth: 0 }}
                    activeDot={{ r: 6 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            )}
          </div>

          <p className="attendance-analytics-note">
            По умолчанию показана статистика по всем студентам. Выберите группу для детализации.
          </p>
          {period !== 'week' ? (
            <p className="attendance-analytics-note">Периоды "Месяц" и "Семестр" будут подключены позже.</p>
          ) : null}
          {!hasExpectedAttendance ? (
            <p className="attendance-analytics-note">Нет ожидаемых посещений по выбранной неделе и группе.</p>
          ) : null}
        </article>

        <article className="admin-focus-card">
          <div className="admin-focus-header">
            <p className="eyebrow">Операции</p>
            <h2>Оперативный контроль</h2>
            <p>Ключевые события и задачи на сегодня</p>
          </div>

          <div className="admin-focus-list">
            <div className="admin-focus-item success">
              <div className="admin-focus-icon">
                <ShieldCheck size={21} />
              </div>
              <div>
                <strong>Конфликты расписания</strong>
                <span>0 найдено</span>
              </div>
            </div>

            <div className="admin-focus-item primary">
              <div className="admin-focus-icon">
                <CalendarClock size={21} />
              </div>
              <div>
                <strong>Занятия сегодня</strong>
                <span>{stats.schedules > 0 ? `${stats.schedules} запланировано` : 'Нет запланированных занятий'}</span>
              </div>
            </div>

            <div className="admin-focus-item warning">
              <div className="admin-focus-icon">
                <ClipboardCheck size={21} />
              </div>
              <div>
                <strong>Посещаемость</strong>
                <span>{attendanceStatus}</span>
              </div>
            </div>

            <div className="admin-focus-item muted">
              <div className="admin-focus-icon">
                <Activity size={21} />
              </div>
              <div>
                <strong>Новые записи</strong>
                <span>{stats.attendance > 0 ? 'Данные обновлены' : 'Ожидают первых отметок'}</span>
              </div>
            </div>
          </div>

          <Link className="admin-focus-action" to="/admin/schedules">
            <span>Открыть расписание</span>
            <ArrowRight size={18} />
          </Link>
        </article>
      </div>
    </section>
  );
}

async function getCount(endpoint: string) {
  const response = await axiosClient.get<ListResponse<unknown> | unknown[]>(endpoint, {
    params: { pageNumber: 1, pageSize: 1 },
  });
  const data = response.data;

  if (Array.isArray(data)) {
    return data.length;
  }

  return data.totalCount ?? data.items?.length ?? 0;
}

async function getGroupLookups() {
  const response = await axiosClient.get<GroupLookup[]>('/admin-dashboard/group-lookups');
  return response.data;
}

async function getAttendanceWeekly(selectedGroupId: string) {
  const params: { groupId?: string } = {};

  if (selectedGroupId !== 'all') {
    params.groupId = selectedGroupId;
  }

  const response = await axiosClient.get<AdminAttendanceWeekly>('/admin-dashboard/attendance-weekly', { params });
  return response.data;
}

function getWeeklyAttendancePercent(analytics: AdminAttendanceWeekly | null) {
  if (!analytics) {
    return null;
  }

  const expected = analytics.days.reduce((sum, day) => sum + day.expected, 0);
  const attended = analytics.days.reduce((sum, day) => sum + day.present + day.late, 0);

  if (expected <= 0) {
    return null;
  }

  return Math.round((attended / expected) * 100);
}

function toChartData(analytics: AdminAttendanceWeekly | null): AttendanceChartPoint[] {
  if (!analytics) {
    return [1, 2, 3, 4, 5, 6].map((day) => ({
      day: getDayName(day),
      present: 0,
      late: 0,
      absent: 0,
    }));
  }

  return analytics.days.map((day) => ({
    day: getDayName(day.day, day.dayName),
    present: day.present,
    late: day.late,
    absent: day.absent,
  }));
}

function getDayName(day: number, fallback?: string) {
  const names: Record<number, string> = {
    1: 'Пн',
    2: 'Вт',
    3: 'Ср',
    4: 'Чт',
    5: 'Пт',
    6: 'Сб',
  };

  return names[day] ?? fallback ?? String(day);
}
