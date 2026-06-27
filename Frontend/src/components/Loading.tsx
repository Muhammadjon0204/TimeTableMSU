export function Loading({ label = 'Загрузка данных...' }: { label?: string }) {
  return (
    <div className="loading-state">
      <span className="loading-spinner" />
      <p>{label}</p>
    </div>
  );
}
