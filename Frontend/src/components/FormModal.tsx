import { X } from 'lucide-react';
import { useEffect, useState } from 'react';

export type FieldOption = {
  label: string;
  value: string | number;
};

export type FieldConfig = {
  name: string;
  label: string;
  type?: 'text' | 'number' | 'email' | 'date' | 'select' | 'textarea';
  required?: boolean;
  options?: FieldOption[];
  placeholder?: string;
};

type FormModalProps = {
  title: string;
  fields: FieldConfig[];
  initialValues?: Record<string, unknown>;
  isOpen: boolean;
  isSubmitting?: boolean;
  onClose: () => void;
  onSubmit: (values: Record<string, unknown>) => Promise<void>;
};

export function FormModal({ title, fields, initialValues, isOpen, isSubmitting, onClose, onSubmit }: FormModalProps) {
  const [values, setValues] = useState<Record<string, unknown>>({});

  useEffect(() => {
    const nextValues = fields.reduce<Record<string, unknown>>((acc, field) => {
      acc[field.name] = initialValues?.[field.name] ?? '';
      return acc;
    }, {});
    setValues(nextValues);
  }, [fields, initialValues, isOpen]);

  if (!isOpen) {
    return null;
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onSubmit(values);
  }

  function updateValue(field: FieldConfig, value: string) {
    if (field.type === 'number') {
      setValues((current) => ({ ...current, [field.name]: value === '' ? null : Number(value) }));
      return;
    }

    setValues((current) => ({ ...current, [field.name]: value }));
  }

  return (
    <div className="modal-backdrop" role="presentation">
      <div className="modal-card" role="dialog" aria-modal="true" aria-label={title}>
        <div className="modal-header">
          <h2>{title}</h2>
          <button className="icon-button small" type="button" onClick={onClose} aria-label="Закрыть форму">
            <X size={18} />
          </button>
        </div>

        <form className="form-grid" onSubmit={handleSubmit}>
          {fields.map((field) => (
            <label key={field.name} className={field.type === 'textarea' ? 'form-field wide' : 'form-field'}>
              <span>{field.label}</span>
              {renderField(field, values[field.name], (value) => updateValue(field, value))}
            </label>
          ))}

          <div className="modal-actions">
            <button className="ghost-button" type="button" onClick={onClose}>
              Отмена
            </button>
            <button className="primary-button" type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Сохранение...' : 'Сохранить'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

function renderField(field: FieldConfig, value: unknown, onChange: (value: string) => void) {
  const fieldValue = value === null || value === undefined ? '' : String(value);

  if (field.type === 'select') {
    return (
      <select value={fieldValue} required={field.required} onChange={(event) => onChange(event.target.value)}>
        <option value="">Выберите значение</option>
        {field.options?.map((option) => (
          <option key={String(option.value)} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    );
  }

  if (field.type === 'textarea') {
    return (
      <textarea
        value={fieldValue}
        required={field.required}
        placeholder={field.placeholder}
        rows={4}
        onChange={(event) => onChange(event.target.value)}
      />
    );
  }

  return (
    <input
      type={field.type ?? 'text'}
      value={fieldValue}
      required={field.required}
      placeholder={field.placeholder}
      onChange={(event) => onChange(event.target.value)}
    />
  );
}
