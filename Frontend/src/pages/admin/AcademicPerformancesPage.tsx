import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function AcademicPerformancesPage() {
  return <AdminCrudPage config={crudConfigs.academicPerformances} />;
}
