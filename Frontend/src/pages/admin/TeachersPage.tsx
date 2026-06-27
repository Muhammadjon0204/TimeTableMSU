import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function TeachersPage() {
  return <AdminCrudPage config={crudConfigs.teachers} />;
}
