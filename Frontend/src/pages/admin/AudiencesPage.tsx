import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function AudiencesPage() {
  return <AdminCrudPage config={crudConfigs.audiences} />;
}
