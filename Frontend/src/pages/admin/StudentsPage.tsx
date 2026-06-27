import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function StudentsPage() {
  return <AdminCrudPage config={crudConfigs.students} />;
}
