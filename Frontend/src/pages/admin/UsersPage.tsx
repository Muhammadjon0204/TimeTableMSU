import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function UsersPage() {
  return <AdminCrudPage config={crudConfigs.users} />;
}
