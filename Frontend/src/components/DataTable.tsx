import { Pencil, Trash2 } from 'lucide-react';

export type TableColumn<T> = {
  key: keyof T | string;
  label: string;
  render?: (row: T) => React.ReactNode;
};

type DataTableProps<T extends { id?: number }> = {
  columns: Array<TableColumn<T>>;
  rows: T[];
  onEdit?: (row: T) => void;
  onDelete?: (row: T) => void;
};

export function DataTable<T extends { id?: number }>({ columns, rows, onEdit, onDelete }: DataTableProps<T>) {
  return (
    <div className="table-shell">
      <table className="data-table">
        <thead>
          <tr>
            {columns.map((column) => (
              <th key={String(column.key)}>{column.label}</th>
            ))}
            <th className="table-actions-col">Действия</th>
          </tr>
        </thead>
        <tbody>
          {rows.length === 0 ? (
            <tr>
              <td colSpan={columns.length + 1} className="empty-cell">
                Данные отсутствуют
              </td>
            </tr>
          ) : (
            rows.map((row, rowIndex) => (
              <tr key={row.id ?? rowIndex}>
                {columns.map((column) => (
                  <td key={String(column.key)}>{column.render ? column.render(row) : formatValue(row[column.key as keyof T])}</td>
                ))}
                <td className="actions-cell">
                  <div className="table-actions">
                    {onEdit ? (
                      <button className="action-pill action-pill--edit" type="button" onClick={() => onEdit(row)} aria-label="Изменить" title="Изменить">
                        <Pencil className="action-pill__icon" size={18} />
                      </button>
                    ) : null}
                    {onDelete ? (
                      <button className="action-pill action-pill--delete" type="button" onClick={() => onDelete(row)} aria-label="Удалить" title="Удалить">
                        <Trash2 className="action-pill__icon" size={18} />
                      </button>
                    ) : null}
                  </div>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

function formatValue(value: unknown) {
  if (value === null || value === undefined || value === '') {
    return <span className="muted">—</span>;
  }

  return String(value);
}
