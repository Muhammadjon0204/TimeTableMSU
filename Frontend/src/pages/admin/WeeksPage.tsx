import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function WeeksPage() {
  return <AdminCrudPage config={crudConfigs.weeks} />;
}
