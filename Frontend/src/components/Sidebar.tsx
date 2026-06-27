import {
  BarChart3,
  BookOpen,
  Building2,
  CalendarDays,
  ClipboardCheck,
  GraduationCap,
  Landmark,
  Layers3,
  LayoutDashboard,
  LibraryBig,
  MapPinned,
  NotebookTabs,
  School,
  Settings,
  ShieldCheck,
  UserRoundCog,
  UsersRound,
} from 'lucide-react';
import { NavLink } from 'react-router-dom';
import msuImage from '../icons/MSU.jpg';

const menuItems = [
  { to: '/admin', label: 'Главная', icon: LayoutDashboard, end: true },
  { to: '/admin/faculties', label: 'Факультеты', icon: Landmark },
  { to: '/admin/specialities', label: 'Специальности', icon: LibraryBig },
  { to: '/admin/groups', label: 'Группы', icon: UsersRound },
  { to: '/admin/students', label: 'Студенты', icon: GraduationCap },
  { to: '/admin/teachers', label: 'Преподаватели', icon: School },
  { to: '/admin/subjects', label: 'Предметы', icon: BookOpen },
  { to: '/admin/disciplines', label: 'Дисциплины', icon: NotebookTabs },
  { to: '/admin/audiences', label: 'Аудитории', icon: MapPinned },
  { to: '/admin/weeks', label: 'Учебные недели', icon: CalendarDays },
  { to: '/admin/schedules', label: 'Расписание', icon: Layers3 },
  { to: '/admin/attendances', label: 'Посещаемость', icon: ClipboardCheck },
  { to: '/admin/academic-performances', label: 'Успеваемость', icon: BarChart3 },
  { to: '/admin/executions', label: 'Выполнение занятий', icon: ShieldCheck },
  { to: '/admin/users', label: 'Пользователи', icon: UserRoundCog },
  { to: '/admin/settings', label: 'Настройки', icon: Settings },
];

export function Sidebar() {
  return (
    <aside className="sidebar">
      <div className="brand">
        <div className="brand-mark">
          <Building2 size={24} />
        </div>
        <div>
          <strong>TimeTableMSU</strong>
          <span>Панель управления</span>
        </div>
      </div>

      <nav className="sidebar-nav" aria-label="Навигация администратора">
        {menuItems.map((item) => {
          const Icon = item.icon;

          return (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
            >
              <Icon size={18} />
              <span>{item.label}</span>
            </NavLink>
          );
        })}
      </nav>

      <div className="sidebar-msu-mini">
        <div className="sidebar-msu-mini__imageWrap">
          <img src={msuImage} alt="МГУ" className="sidebar-msu-mini__image" />
        </div>
        <div className="sidebar-msu-mini__content">
          <div className="sidebar-msu-mini__title">МГУ</div>
          <div className="sidebar-msu-mini__subtitle">Университетская система</div>
        </div>
      </div>
    </aside>
  );
}
