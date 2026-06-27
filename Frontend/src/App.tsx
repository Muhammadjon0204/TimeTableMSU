import { Navigate, Route, Routes } from 'react-router-dom';
import { AdminLayout } from './layouts/AdminLayout';
import { LoginPage } from './pages/LoginPage';
import { AcademicPerformancesPage } from './pages/admin/AcademicPerformancesPage';
import { AdminDashboard } from './pages/admin/AdminDashboard';
import { AttendancesPage } from './pages/admin/AttendancesPage';
import { AudiencesPage } from './pages/admin/AudiencesPage';
import { DisciplinesPage } from './pages/admin/DisciplinesPage';
import { ExecutionsPage } from './pages/admin/ExecutionsPage';
import { FacultiesPage } from './pages/admin/FacultiesPage';
import { GroupsPage } from './pages/admin/GroupsPage';
import { SchedulesPage } from './pages/admin/SchedulesPage';
import { SettingsPage } from './pages/admin/SettingsPage';
import { SpecialitiesPage } from './pages/admin/SpecialitiesPage';
import { StudentsPage } from './pages/admin/StudentsPage';
import { SubjectsPage } from './pages/admin/SubjectsPage';
import { TeachersPage } from './pages/admin/TeachersPage';
import { UsersPage } from './pages/admin/UsersPage';
import { WeeksPage } from './pages/admin/WeeksPage';
import { ProtectedRoute } from './routes/ProtectedRoute';

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<AdminDashboard />} />
          <Route path="faculties" element={<FacultiesPage />} />
          <Route path="specialities" element={<SpecialitiesPage />} />
          <Route path="groups" element={<GroupsPage />} />
          <Route path="students" element={<StudentsPage />} />
          <Route path="teachers" element={<TeachersPage />} />
          <Route path="subjects" element={<SubjectsPage />} />
          <Route path="disciplines" element={<DisciplinesPage />} />
          <Route path="audiences" element={<AudiencesPage />} />
          <Route path="weeks" element={<WeeksPage />} />
          <Route path="schedules" element={<SchedulesPage />} />
          <Route path="attendances" element={<AttendancesPage />} />
          <Route path="academic-performances" element={<AcademicPerformancesPage />} />
          <Route path="executions" element={<ExecutionsPage />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="settings" element={<SettingsPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/admin" replace />} />
    </Routes>
  );
}

export default App;
