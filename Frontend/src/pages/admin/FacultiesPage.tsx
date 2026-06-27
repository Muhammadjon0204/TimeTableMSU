import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function FacultiesPage() {
  return <AdminCrudPage config={crudConfigs.faculties} />;
}
