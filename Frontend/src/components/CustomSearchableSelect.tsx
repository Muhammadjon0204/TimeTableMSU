import { ChevronDown, X } from 'lucide-react';
import { useEffect, useMemo, useRef, useState } from 'react';

export type SelectOption = {
  value: string;
  label: string;
};

type CustomSearchableSelectProps = {
  value: string;
  options: SelectOption[];
  placeholder: string;
  onChange: (value: string) => void;
};

export function CustomSearchableSelect({ value, options, placeholder, onChange }: CustomSearchableSelectProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [alignEnd, setAlignEnd] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find((option) => option.value === value);
  const isWide = options.some((option) => option.label.length > 24);
  const filteredOptions = useMemo(() => {
    const normalizedSearch = searchText.trim().toLowerCase();

    if (!normalizedSearch) {
      return options;
    }

    return options.filter((option) => option.label.toLowerCase().includes(normalizedSearch));
  }, [options, searchText]);

  useEffect(() => {
    function handlePointerDown(event: MouseEvent) {
      if (!rootRef.current?.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener('mousedown', handlePointerDown);
    return () => document.removeEventListener('mousedown', handlePointerDown);
  }, []);

  function chooseOption(nextValue: string) {
    onChange(nextValue);
    setSearchText('');
    setIsOpen(false);
  }

  function toggleMenu() {
    setIsOpen((current) => {
      const nextOpen = !current;

      if (nextOpen && rootRef.current) {
        const rect = rootRef.current.getBoundingClientRect();
        const menuWidth = Math.min(420, window.innerWidth - 48);
        setAlignEnd(rect.left + menuWidth > window.innerWidth - 24);
      }

      return nextOpen;
    });
  }

  const rootClassName = [
    'custom-select',
    isOpen ? 'custom-select--open' : '',
    isWide ? 'custom-select--wide' : '',
    alignEnd ? 'custom-select--align-end' : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <div ref={rootRef} className={rootClassName}>
      <button className="custom-select__control" type="button" onClick={toggleMenu}>
        <span className={selectedOption ? 'custom-select__value' : 'custom-select__placeholder'} title={selectedOption?.label ?? placeholder}>
          {selectedOption?.label ?? placeholder}
        </span>
        {value !== 'all' ? (
          <span
            className="custom-select__clear"
            role="button"
            tabIndex={0}
            onClick={(event) => {
              event.stopPropagation();
              chooseOption('all');
            }}
            onKeyDown={(event) => {
              if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                chooseOption('all');
              }
            }}
            aria-label="Очистить фильтр"
          >
            <X size={14} />
          </span>
        ) : null}
        <ChevronDown className="custom-select__chevron" size={16} />
      </button>

      {isOpen ? (
        <div className="custom-select__menu">
          <input
            className="custom-select__search"
            value={searchText}
            placeholder="Поиск..."
            autoFocus
            onChange={(event) => setSearchText(event.target.value)}
          />
          <div className="custom-select__options">
            {filteredOptions.length > 0 ? (
              filteredOptions.map((option) => (
                <button
                  key={option.value}
                  className={
                    option.value === value ? 'custom-select__option custom-select__option--active' : 'custom-select__option'
                  }
                  type="button"
                  title={option.label}
                  onClick={() => chooseOption(option.value)}
                >
                  <span>{option.label}</span>
                </button>
              ))
            ) : (
              <div className="custom-select__empty">Ничего не найдено</div>
            )}
          </div>
        </div>
      ) : null}
    </div>
  );
}
