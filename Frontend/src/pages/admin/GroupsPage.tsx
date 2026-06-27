import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function GroupsPage() {
  return <AdminCrudPage config={crudConfigs.groups} />;
}
