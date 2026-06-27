import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function AttendancesPage() {
  return <AdminCrudPage config={crudConfigs.attendances} />;
}
