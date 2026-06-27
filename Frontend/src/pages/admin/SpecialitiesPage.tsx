import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function SpecialitiesPage() {
  return <AdminCrudPage config={crudConfigs.specialities} />;
}
