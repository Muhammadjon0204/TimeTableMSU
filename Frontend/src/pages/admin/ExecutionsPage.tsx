import { AdminCrudPage } from './AdminCrudPage';
import { crudConfigs } from './crudConfigs';

export function ExecutionsPage() {
  return <AdminCrudPage config={crudConfigs.executions} />;
}
