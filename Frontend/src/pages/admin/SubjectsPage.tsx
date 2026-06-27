import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function SubjectsPage() {
  return <AdminCrudPage config={crudConfigs.subjects} />;
}
