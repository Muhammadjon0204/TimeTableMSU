import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function DisciplinesPage() {
  return <AdminCrudPage config={crudConfigs.disciplines} />;
}
