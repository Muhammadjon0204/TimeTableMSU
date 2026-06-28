import {
  BarChart3,
  BookOpen,
  Building2,
  CalendarDays,
  ChevronUp,
  ClipboardCheck,
  GraduationCap,
  Landmark,
  Layers3,
  LayoutDashboard,
  LibraryBig,
  LogOut,
  MapPinned,
  NotebookTabs,
  School,
  Settings,
  ShieldCheck,
  UserRoundCog,
  UsersRound,
} from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

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
];

export function Sidebar() {
  const navigate = useNavigate();
  const { fullName, email, logout } = useAuth();
  const [isAccountMenuOpen, setIsAccountMenuOpen] = useState(false);
  const [isLogoutModalOpen, setIsLogoutModalOpen] = useState(false);
  const accountDropdownRef = useRef<HTMLDivElement | null>(null);
  const displayName = fullName || 'Администратор';
  const displayEmail = email || 'Панель управления';
  const avatarLetter = (fullName ?? email ?? 'A').slice(0, 1).toUpperCase();

  useEffect(() => {
    if (!isAccountMenuOpen) {
      return;
    }

    function handlePointerDown(event: MouseEvent) {
      if (!accountDropdownRef.current?.contains(event.target as Node)) {
        setIsAccountMenuOpen(false);
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setIsAccountMenuOpen(false);
      }
    }

    document.addEventListener('mousedown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isAccountMenuOpen]);

  function openSettings() {
    setIsAccountMenuOpen(false);
    navigate('/admin/settings');
  }

  function requestLogout() {
    setIsAccountMenuOpen(false);
    setIsLogoutModalOpen(true);
  }

  function confirmLogout() {
    setIsLogoutModalOpen(false);
    logout();
    navigate('/login', { replace: true });
  }

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

      <div className="sidebar-account" ref={accountDropdownRef}>
        <button
          className="account-dropdown-trigger sidebar-account-trigger"
          type="button"
          aria-haspopup="menu"
          aria-expanded={isAccountMenuOpen}
          onClick={() => setIsAccountMenuOpen((current) => !current)}
        >
          <span className="account-avatar">{avatarLetter}</span>
          <span className="account-info">
            <strong>{displayName}</strong>
            <span>{displayEmail}</span>
          </span>
          <ChevronUp className="account-chevron" size={17} />
        </button>

        {isAccountMenuOpen ? (
          <div className="account-menu account-menu--up" role="menu">
            <div className="account-menu__header">
              <span className="account-menu__avatar">{avatarLetter}</span>
              <div>
                <strong>{displayName}</strong>
                <span>{displayEmail}</span>
              </div>
            </div>
            <button className="account-menu__item" type="button" role="menuitem" onClick={openSettings}>
              <Settings size={18} />
              <span>Настройки</span>
            </button>
            <button className="account-menu__item account-menu__item--danger" type="button" role="menuitem" onClick={requestLogout}>
              <LogOut size={18} />
              <span>Выйти</span>
            </button>
          </div>
        ) : null}
      </div>

      {isLogoutModalOpen ? (
        <div className="logout-modal-overlay" role="presentation" onMouseDown={() => setIsLogoutModalOpen(false)}>
          <div className="logout-modal" role="dialog" aria-modal="true" aria-labelledby="logout-modal-title" onMouseDown={(event) => event.stopPropagation()}>
            <h3 id="logout-modal-title">Выйти из аккаунта?</h3>
            <p>Вы завершите текущую сессию и вернетесь на страницу входа.</p>
            <div className="logout-modal__actions">
              <button className="logout-modal__cancel" type="button" onClick={() => setIsLogoutModalOpen(false)}>
                Отмена
              </button>
              <button className="logout-modal__confirm" type="button" onClick={confirmLogout}>
                Выйти
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </aside>
  );
}
