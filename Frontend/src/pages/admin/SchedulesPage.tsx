import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function SchedulesPage() {
  return <AdminCrudPage config={crudConfigs.schedules} />;
}
