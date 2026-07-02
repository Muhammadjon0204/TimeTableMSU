import { X } from 'lucide-react';
import { useEffect, useState } from 'react';
import { axiosClient, getApiError } from '../api/axiosClient';
import { CustomSearchableSelect, type SelectOption } from './CustomSearchableSelect';

export type FieldOption = {
  label: string;
  value: string | number;
};

export type FieldConfig = {
  name: string;
  label: string;
  type?: 'text' | 'number' | 'email' | 'date' | 'select' | 'textarea' | 'searchable-select';
  required?: boolean;
  options?: FieldOption[];
  placeholder?: string;
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  validation?: 'entityName' | 'specialityName' | 'groupName' | 'personName' | 'email' | 'phone' | 'birthDate';
  lookup?: {
    endpoint: string;
    labelKey: string;
    subtitleKeys?: string[];
  };
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
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [lookupOptions, setLookupOptions] = useState<Record<string, SelectOption[]>>({});
  const [lookupErrors, setLookupErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    const nextValues = fields.reduce<Record<string, unknown>>((acc, field) => {
      acc[field.name] = initialValues?.[field.name] ?? '';
      return acc;
    }, {});
    setValues(nextValues);
    setErrors({});
  }, [fields, initialValues, isOpen]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    fields
      .filter((field) => field.type === 'searchable-select' && field.lookup)
      .forEach((field) => {
        void fetchLookupOptions(field);
      });
  }, [fields, isOpen]);

  if (!isOpen) {
    return null;
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const sanitizedValues = sanitizeValues(values, fields);
    const nextErrors = validateValues(sanitizedValues, fields);

    setValues(sanitizedValues);
    setErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) {
      return;
    }

    await onSubmit(sanitizedValues);
  }

  function updateValue(field: FieldConfig, value: string) {
    if (field.type === 'number' || field.type === 'searchable-select') {
      setValues((current) => ({ ...current, [field.name]: value === '' ? null : Number(value) }));
      setErrors((current) => ({ ...current, [field.name]: '' }));
      return;
    }

    setValues((current) => ({ ...current, [field.name]: value }));
    setErrors((current) => ({ ...current, [field.name]: '' }));
  }

  async function fetchLookupOptions(field: FieldConfig) {
    if (!field.lookup) {
      return;
    }

    try {
      const response = await axiosClient.get(field.lookup.endpoint, {
        params: { pageNumber: 1, pageSize: 300 },
      });
      const rows = unwrapRows(response.data);
      const options = rows
        .map((row) => toSelectOption(row, field.lookup?.labelKey ?? 'name', field.lookup?.subtitleKeys ?? []))
        .filter((option): option is SelectOption => Boolean(option));

      setLookupOptions((current) => ({ ...current, [field.name]: options }));
      setLookupErrors((current) => ({ ...current, [field.name]: '' }));
    } catch (error) {
      setLookupOptions((current) => ({ ...current, [field.name]: [] }));
      setLookupErrors((current) => ({ ...current, [field.name]: getApiError(error) }));
    }
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
            <label key={field.name} className={field.type === 'textarea' || field.type === 'searchable-select' ? 'form-field wide' : 'form-field'}>
              <span>{field.label}</span>
              {renderField(field, values[field.name], errors[field.name], lookupOptions[field.name] ?? field.options?.map(toStaticOption) ?? [], (value) => updateValue(field, value))}
              {errors[field.name] ? <small className="form-field-error">{errors[field.name]}</small> : null}
              {lookupErrors[field.name] ? <small className="form-field-error">{lookupErrors[field.name]}</small> : null}
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

function renderField(field: FieldConfig, value: unknown, error: string | undefined, options: SelectOption[], onChange: (value: string) => void) {
  const fieldValue = value === null || value === undefined ? '' : String(value);

  if (field.type === 'searchable-select') {
    return (
      <CustomSearchableSelect
        value={fieldValue}
        options={options}
        placeholder={field.placeholder ?? 'Выберите значение'}
        clearValue=""
        error={Boolean(error)}
        onChange={onChange}
      />
    );
  }

  if (field.type === 'select') {
    return (
      <select className={error ? 'form-input--error' : ''} value={fieldValue} required={field.required} onChange={(event) => onChange(event.target.value)}>
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
        className={error ? 'form-input--error' : ''}
        value={fieldValue}
        required={field.required}
        placeholder={field.placeholder}
        rows={4}
        maxLength={field.maxLength}
        onChange={(event) => onChange(event.target.value)}
      />
    );
  }

  return (
    <input
      className={error ? 'form-input--error' : ''}
      type={field.type ?? 'text'}
      value={fieldValue}
      required={field.required}
      placeholder={field.placeholder}
      min={field.min}
      max={field.max}
      minLength={field.minLength}
      maxLength={field.maxLength}
      onChange={(event) => onChange(event.target.value)}
    />
  );
}

function sanitizeValues(values: Record<string, unknown>, fields: FieldConfig[]) {
  return Object.fromEntries(
    fields.map((field) => {
      const value = values[field.name];

      if (typeof value === 'string') {
        return [field.name, value.trim()];
      }

      return [field.name, value];
    }),
  );
}

function validateValues(values: Record<string, unknown>, fields: FieldConfig[]) {
  return fields.reduce<Record<string, string>>((acc, field) => {
    const value = values[field.name];
    const stringValue = value === null || value === undefined ? '' : String(value).trim();

    if (field.required && !stringValue) {
      acc[field.name] = 'Заполните поле.';
      return acc;
    }

    if (!stringValue) {
      return acc;
    }

    if (field.minLength && stringValue.length < field.minLength) {
      acc[field.name] = `Минимальная длина: ${field.minLength}.`;
      return acc;
    }

    if (field.maxLength && stringValue.length > field.maxLength) {
      acc[field.name] = `Максимальная длина: ${field.maxLength}.`;
      return acc;
    }

    if (field.type === 'number') {
      const numberValue = Number(value);

      if (Number.isNaN(numberValue)) {
        acc[field.name] = 'Введите число.';
        return acc;
      }

      if (field.min !== undefined && numberValue < field.min) {
        acc[field.name] = `Значение должно быть не меньше ${field.min}.`;
        return acc;
      }

      if (field.max !== undefined && numberValue > field.max) {
        acc[field.name] = `Значение должно быть не больше ${field.max}.`;
        return acc;
      }
    }

    const validationError = validateByRule(field, stringValue);

    if (validationError) {
      acc[field.name] = validationError;
    }

    return acc;
  }, {});
}

function validateByRule(field: FieldConfig, value: string) {
  const nameRegex = /^[A-Za-zА-Яа-яЁё\s\-—]+$/;
  const specialityNameRegex = /^[A-Za-zА-Яа-яЁё\s\-—()]+$/;
  const groupNameRegex = /^[A-Za-zА-Яа-яЁё0-9\s\-—]+$/;
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const phoneRegex = /^[0-9+\s\-()]+$/;

  if (field.validation === 'entityName' && !nameRegex.test(value)) {
    return 'Название может содержать только буквы, пробелы и дефис.';
  }

  if (field.validation === 'specialityName' && !specialityNameRegex.test(value)) {
    return 'Название специальности может содержать только буквы, пробелы, дефис и скобки.';
  }

  if (field.validation === 'groupName' && !groupNameRegex.test(value)) {
    return 'Название группы может содержать только буквы, цифры, пробелы и дефис.';
  }

  if (field.validation === 'personName' && !nameRegex.test(value)) {
    return `${field.label} может содержать только буквы, пробелы и дефис.`;
  }

  if (field.validation === 'email' && !emailRegex.test(value)) {
    return 'Введите корректный email.';
  }

  if (field.validation === 'phone' && !phoneRegex.test(value)) {
    return 'Телефон может содержать только цифры, +, пробелы, дефис и скобки.';
  }

  if (field.validation === 'birthDate' && !isValidBirthDate(value)) {
    return 'Дата рождения указана некорректно.';
  }

  if (field.name === 'course' && field.max === 6) {
    const course = Number(value);

    if (Number.isNaN(course) || course < 1 || course > 6) {
      return 'Курс должен быть от 1 до 6.';
    }
  }

  return '';
}

function isValidBirthDate(value: string) {
  const birthDate = new Date(`${value}T00:00:00`);

  if (Number.isNaN(birthDate.getTime())) {
    return false;
  }

  const today = new Date();
  const todayDate = new Date(today.getFullYear(), today.getMonth(), today.getDate());

  if (birthDate > todayDate) {
    return false;
  }

  let age = todayDate.getFullYear() - birthDate.getFullYear();
  const monthDelta = todayDate.getMonth() - birthDate.getMonth();

  if (monthDelta < 0 || (monthDelta === 0 && todayDate.getDate() < birthDate.getDate())) {
    age -= 1;
  }

  return age >= 15 && age <= 80;
}

function unwrapRows(data: unknown): Array<Record<string, unknown>> {
  if (Array.isArray(data)) {
    return data as Array<Record<string, unknown>>;
  }

  if (data && typeof data === 'object') {
    const record = data as { items?: unknown; value?: unknown };

    if (Array.isArray(record.items)) {
      return record.items as Array<Record<string, unknown>>;
    }

    if (Array.isArray(record.value)) {
      return record.value as Array<Record<string, unknown>>;
    }

    if (record.value && typeof record.value === 'object') {
      return unwrapRows(record.value);
    }
  }

  return [];
}

function toSelectOption(row: Record<string, unknown>, labelKey: string, subtitleKeys: string[]): SelectOption | null {
  const id = row.id ?? row.Id;
  const label = row[labelKey] ?? row[capitalize(labelKey)];

  if (id === null || id === undefined || label === null || label === undefined || String(label).trim() === '') {
    return null;
  }

  const subtitle = subtitleKeys
    .map((key) => formatSubtitleValue(key, row[key] ?? row[capitalize(key)]))
    .filter((value) => value !== null && value !== undefined && String(value).trim() !== '')
    .map((value) => String(value))
    .join(' · ');

  return {
    value: String(id),
    label: String(label),
    subtitle: subtitle || undefined,
  };
}

function toStaticOption(option: FieldOption): SelectOption {
  return {
    value: String(option.value),
    label: option.label,
  };
}

function capitalize(value: string) {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

function formatSubtitleValue(key: string, value: unknown) {
  if (value === null || value === undefined || String(value).trim() === '') {
    return value;
  }

  if (key === 'course') {
    return `${value} курс`;
  }

  return value;
}
