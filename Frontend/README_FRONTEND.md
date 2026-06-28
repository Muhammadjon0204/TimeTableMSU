# TimeTableMSU Admin Frontend

## Created Pages

- `/login`
- `/admin`
- `/admin/faculties`
- `/admin/specialities`
- `/admin/groups`
- `/admin/students`
- `/admin/teachers`
- `/admin/subjects`
- `/admin/disciplines`
- `/admin/audiences`
- `/admin/weeks`
- `/admin/schedules`
- `/admin/attendances`
- `/admin/academic-performances`
- `/admin/executions`
- `/admin/users`
- `/admin/settings`

Only the Admin frontend is implemented. Student and Teacher portal screens are intentionally not included yet.

## Connected Endpoints

Base URL:

```text
http://localhost:5010/api
```

Auth:

- `POST /auth/login`

Admin CRUD:

- `GET/POST/PUT/DELETE /faculties`
- `GET/POST/PUT/DELETE /specialities`
- `GET/POST/PUT/DELETE /groups`
- `GET/POST/PUT/DELETE /students`
- `GET/POST/PUT/DELETE /teachers`
- `GET/POST/PUT/DELETE /subjects`
- `GET/POST/PUT/DELETE /disciplines`
- `GET/POST/PUT/DELETE /audiences`
- `GET/POST/PUT/DELETE /weeks`
- `GET/POST/PUT/DELETE /schedules`
- `GET/POST/PUT/DELETE /attendances`
- `GET/POST/PUT/DELETE /academic-performances`
- `GET/POST/PUT/DELETE /executions`

Dashboard counts are temporarily loaded from:

- `/students`
- `/teachers`
- `/groups`
- `/subjects`
- `/schedules`
- `/attendances`

## Missing Backend Endpoints

- No Admin Users CRUD endpoint was found, so `/admin/users` shows a backend limitation message.
- No dashboard summary endpoint exists yet, so the dashboard uses individual list endpoints for counts.
- No lookup endpoints optimized for select controls exist yet. Forms currently accept related entity IDs such as `facultyId`, `groupId`, `teacherId`, `subjectId`, `disciplineId`, `weekId`, and `audienceId`.

## TODO

- Replace ID inputs with selects once lookup endpoints or compact list endpoints are finalized.
- Add a backend dashboard summary endpoint and switch `/admin` to it.
- Add refresh-token retry handling in the axios interceptor.
- Add frontend validation schemas per DTO.
- Add Student and Teacher portal screens in a later stage.
- Add automated frontend tests for auth guard and CRUD flows.

## How To Run

Install dependencies:

```powershell
npm install
```

Build:

```powershell
npm run build
```

Run development server:

```powershell
npm run dev
```

The app expects the backend at:

```text
http://localhost:5010/api
```

To override it, create a Vite environment variable:

```text
VITE_API_BASE_URL=http://localhost:5010/api
```

---

# UI Polish Update

## Что переведено на русский

- Login page: заголовки, подписи полей, кнопка входа и сообщения ошибок.
- Sidebar: все пункты меню Admin Dashboard.
- Topbar: заголовки страниц, поиск, подписи профиля и logout tooltip.
- Dashboard: статистические карточки, блок активности, операционные заметки.
- CRUD pages: названия разделов, описания, колонки таблиц, labels и placeholders форм.
- Shared states: загрузка, пустая таблица, действия, success messages и delete confirmation.

## Какие файлы стилей изменены

- `src/styles/theme.css`
- `src/styles/layout.css`
- `src/styles/cards.css`
- `src/styles/tables.css`
- `src/styles/forms.css`

## Какие страницы визуально обновлены

- `/login`
- `/admin`
- Все Admin CRUD pages:
  - `/admin/faculties`
  - `/admin/specialities`
  - `/admin/groups`
  - `/admin/students`
  - `/admin/teachers`
  - `/admin/subjects`
  - `/admin/disciplines`
  - `/admin/audiences`
  - `/admin/weeks`
  - `/admin/schedules`
  - `/admin/attendances`
  - `/admin/academic-performances`
  - `/admin/executions`
  - `/admin/users`
  - `/admin/settings`

## Что изменилось в дизайне

- Бежево-желтая основа заменена на чистую светлую палитру с почти белым фоном.
- Sidebar стал белым, active state теперь синий `#EFF6FF / #2563EB`.
- Карточки стали белыми с border `#E6EAF0`, radius `20-24px` и мягкими тенями.
- Таблицы получили белые card containers, header `#F8FAFC` и аккуратные action buttons на русском.
- Формы получили белые поля, border `#DDE3EC`, синий focus state и русские labels/placeholders.

## Проверка

```powershell
npm run build
```

Результат:

```text
✓ built
```
# Admin Attendance Analytics Added

- Backend files added/changed: `AdminDashboardController.cs`, dashboard DTOs, `IAdminDashboardService`, `IAdminDashboardRepository`, `AdminDashboardService`, `AdminDashboardRepository`, and DI registrations in Application/Infrastructure.
- Frontend files changed: `AdminDashboard.tsx`, `cards.css`, `package.json`, `package-lock.json`.
- Added endpoints: `GET /api/admin-dashboard/attendance-weekly?weekId={weekId}&groupId={groupId}` and `GET /api/admin-dashboard/group-lookups`.
- The dashboard chart now requests `/admin-dashboard/attendance-weekly`; it no longer calculates attendance analytics from raw `/attendances` on the frontend.
- Expected is calculated on the backend as the sum of students in each scheduled group for every scheduled lesson in the selected week/day.
- Present is calculated as `Math.Max(expected - absent - late, 0)`.
- Late is calculated from `AttendanceMark.Late`. The current Attendance entity has no `LateMinutes` field.
- Absent is calculated from `AttendanceMark.Absent`, `AttendanceMark.ValidReason`, and `AttendanceMark.None`; `ValidReason` is temporarily included in "Не пришли" with a backend TODO for a future separate line.
- Checks passed: `dotnet build Backend/src/Api/Api.csproj -o Backend/artifacts/api-build-check` and `npm run build`.
- `dotnet build Backend/Backend.slnx` was blocked by an already-running `Api` process locking `Backend/src/Api/bin/Debug/net10.0/Application.dll`; the alternate output build passed.
- Month/Semester analytics remain TODO; only the weekly period is active.

# CRUD Actions Polish & Backend Binding

- Changed files: `src/components/DataTable.tsx`, `src/pages/admin/AdminCrudPage.tsx`, `src/styles/tables.css`, `src/styles/forms.css`, and `README_FRONTEND.md`.
- Action buttons are now compact icon-only pills by default; on hover they expand and reveal "Изменить" or "Удалить".
- Edit keeps using the shared `FormModal`; it opens with current row data, sends `PUT {endpoint}/{id}` with the matching `id` in the payload, closes on success, refreshes the table, and shows a success message.
- Delete now uses a reusable confirm modal instead of `window.confirm`; confirming sends `DELETE {endpoint}/{id}`, blocks duplicate clicks while loading, closes on success, refreshes the table, and shows a success message.
- The shared `AdminCrudPage` and `DataTable` apply this behavior to all active CRUD pages: faculties, specialities, groups, students, teachers, subjects, disciplines, audiences, weeks, schedules, attendances, academic-performances, and executions.
- `/admin/users` remains a limitation page because there is no backend CRUD endpoint for admin user management.
- Real endpoints called: `GET {endpoint}`, `GET {endpoint}/{id}` for edit hydration, `POST {endpoint}`, `PUT {endpoint}/{id}`, and `DELETE {endpoint}/{id}` through `axiosClient` with the existing Authorization header.
- `npm run build` passed. The remaining Vite chunk-size warning is not a build error.

# Academic Gradebook Module Added

- Added backend-backed premium gradebook flow for `/admin/academic-performances`; the page now focuses on the hierarchy without a duplicate `Зачетка / Таблица оценок` switch.
- Frontend files changed: `Frontend/src/pages/admin/AcademicPerformancesPage.tsx` and `Frontend/src/styles/cards.css`.
- The hierarchy is `Faculty -> Group -> Student -> Gradebook`: faculty cards load groups, group cards load students, student selection opens a preview panel, and a dedicated button opens the gradebook.
- The page calls real API endpoints under `/admin-academic-journal`; no mock or random data is used.
- Added compact progress breadcrumb, faculty/group search and sorting/filter controls, student master-detail layout, student preview KPIs, semester summaries, print/export actions, and premium gradebook hero.
- Semesters are displayed as tabs; each tab contains subjects with control form, teacher, mark, status, retake placeholder, and summary KPIs.
- `Credit` is displayed as `Зачет`; `Exam` is displayed as `Экзамен`.
- Retake history is not supported by the current backend model. The UI shows `нет`, and backend code has TODO: `Retake history requires separate backend model later.`
- Loading and empty states were added for faculties, groups, students, search results, and empty gradebooks.
- Latest check attempted: `npm run build`; command execution was rejected in the current environment, so this UI pass still needs a local build rerun.

# Academic Journal Minimal Redesign

- `/admin/academic-performances` was simplified into a strict academic journal style: no stepper, no student preview panel, no gradient hero, no KPI-heavy blocks.
- Components changed in `AcademicPerformancesPage.tsx`: faculty grid, group grid, `StudentsTable`, `StudentJournalHeader`, `SemesterTabs`, and `GradebookTable`.
- Student selection is now a clean table with `ФИО`, `Телефон`, `Email`, `Адрес`, optional average mark, and `Открыть журнал`.
- Student journal is now a quiet header plus semester tabs and one gradebook table.
- Backend student DTO was extended with `Address`; no mock data was added.
- Latest `npm run build` attempt was rejected by the environment, so build verification is pending.

# Academic Statement Registry Layout

- `/admin/academic-performances` now uses the target registry structure: after faculty and group selection, the screen becomes two columns with a left student list and a right academic statement.
- Removed rendered page title, flow stepper, hero blocks, preview profile, KPI cards, distribution bars, and unused premium CSS from this module.
- Left panel: `Студенты группы {GroupName}`, search, total count, compact vertical selectable student list.
- Right panel: neutral empty state until a student is selected; then student info header, print/export buttons, semester tabs, academic year selector, and statement table.
- Statement table columns: `№`, `Дисциплина`, `Форма контроля`, `Преподаватель`, `1 атт.`, `2 атт.`, `Итог`, `Статус`, `Пересдача`, `Примечание`.
- Frontend now calls `GET /api/admin-academic-journal/students/{studentId}/journal` for the strict journal DTO.
- Latest `npm run build` attempt was rejected by the environment, so build verification is still pending.

# Academic Statement Sheet Refinement

- The journal view was adjusted closer to the reference: a flat 300px student registry on the left and a wide sheet-like statement on the right.
- Desktop table layout now uses `table-layout: fixed`, fixed column proportions, ellipsis/two-line cells, and no desktop `min-width` forcing horizontal scroll. Overflow is only enabled below the responsive breakpoint.
- The visible table labels changed from `1 атт.` / `2 атт.` to `1 кр` / `2 кр`.
- Added the `Тур` column beside `Пересдача`.
- The table now consumes backend display fields for retake status and round: `retakeStatusDisplayValue`, `retakeRoundDisplayValue`, `resultType`, and `retakeType`.
- The student list was made more compact and registry-like, with a small filter icon and subdued `Показать всех` control.
- Latest `npm run build` attempt was rejected by the environment, so this pass still needs local build verification.

# Academic Performances Flow Update

- `/admin/academic-performances` no longer renders the split layout with students on the left and a journal on the right.
- The active flow is now `faculties -> groups -> students -> student-journal`.
- After selecting a group, the module opens a full student table screen for that group.
- The student table columns are `№`, `ФИО`, `Телефон`, `Email`, `Адрес`, `Оценок`, `Средний балл`, and `Действие`.
- Opening a student now shows the academic statement as a full-width screen.
- The first two screens keep the restored faculty/group card styling.
- Latest `npm run build` attempt was rejected by the environment, so build verification is pending.
