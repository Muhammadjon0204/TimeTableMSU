import {
  BarChart3,
  BookMarked,
  BookOpen,
  Building2,
  CalendarClock,
  CalendarDays,
  CirclePlay,
  ClipboardCheck,
  GraduationCap,
  Landmark,
  LayoutDashboard,
  LogOut,
  NotebookTabs,
  Search,
  Settings,
  UserCog,
  UserRoundCheck,
  UsersRound,
  type LucideIcon,
} from 'lucide-react';
import { useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

type PageMeta = {
  title: string;
  description: string;
  icon: LucideIcon;
};

const defaultMeta: PageMeta = {
  title: 'Панель администратора',
  description: 'Общий обзор расписания, посещаемости и учебных данных.',
  icon: LayoutDashboard,
};

const pageMetaByPath: Record<string, PageMeta> = {
  '/admin': defaultMeta,
  '/admin/faculties': {
    title: 'Факультеты',
    description: 'Структурные подразделения университета и основные направления обучения.',
    icon: Landmark,
  },
  '/admin/specialities': {
    title: 'Специальности',
    description: 'Образовательные программы, привязанные к факультетам.',
    icon: BookMarked,
  },
  '/admin/groups': {
    title: 'Группы',
    description: 'Академические группы, курсы и учебные потоки.',
    icon: UsersRound,
  },
  '/admin/students': {
    title: 'Студенты',
    description: 'Личные данные студентов, группы и контактная информация.',
    icon: GraduationCap,
  },
  '/admin/teachers': {
    title: 'Преподаватели',
    description: 'Профили преподавателей, должности и кафедры.',
    icon: UserRoundCheck,
  },
  '/admin/subjects': {
    title: 'Предметы',
    description: 'Учебные предметы, семестры, часы и формы контроля.',
    icon: BookOpen,
  },
  '/admin/disciplines': {
    title: 'Дисциплины',
    description: 'Учебная нагрузка: преподаватель, предмет и группа.',
    icon: NotebookTabs,
  },
  '/admin/audiences': {
    title: 'Аудитории',
    description: 'Кабинеты, лаборатории и учебные помещения университета.',
    icon: Building2,
  },
  '/admin/weeks': {
    title: 'Учебные недели',
    description: 'Календарные недели семестра и учебного периода.',
    icon: CalendarDays,
  },
  '/admin/schedules': {
    title: 'Расписание',
    description: 'Пары, аудитории, преподаватели, группы и недели.',
    icon: CalendarClock,
  },
  '/admin/attendances': {
    title: 'Посещаемость',
    description: 'Журнал пропусков, опозданий и уважительных причин.',
    icon: ClipboardCheck,
  },
  '/admin/academic-performances': {
    title: 'Успеваемость',
    description: 'Оценки студентов, зачеты и экзамены.',
    icon: BarChart3,
  },
  '/admin/executions': {
    title: 'Выполнение занятий',
    description: 'Проведенные, отмененные и перенесенные занятия.',
    icon: CirclePlay,
  },
  '/admin/users': {
    title: 'Пользователи',
    description: 'Учетные записи, роли и доступ к системе.',
    icon: UserCog,
  },
  '/admin/settings': {
    title: 'Настройки',
    description: 'Параметры интерфейса и администрирования.',
    icon: Settings,
  },
};

export function Topbar() {
  const location = useLocation();
  const { fullName, email, logout } = useAuth();
  const meta = pageMetaByPath[location.pathname] ?? defaultMeta;
  const PageIcon = meta.icon;

  return (
    <header className="topbar">
      <div className="topbar-page">
        <p className="topbar-kicker">
          <PageIcon size={16} />
          <span>Управление университетом</span>
        </p>
        <h1>{meta.title}</h1>
        <p>{meta.description}</p>
      </div>

      <div className="topbar-actions">
        <div className="search-shell">
          <Search size={17} />
          <input aria-label="Поиск по данным администратора" placeholder="Поиск по записям" />
        </div>
        <div className="profile-chip">
          <span className="profile-avatar">{(fullName ?? email ?? 'A').slice(0, 1).toUpperCase()}</span>
          <span>
            <strong>{fullName || 'Администратор'}</strong>
            <small>{email || 'Панель управления'}</small>
          </span>
        </div>
        <button className="icon-button" type="button" onClick={logout} aria-label="Выйти" title="Выйти">
          <LogOut size={18} />
        </button>
      </div>
    </header>
  );
}
