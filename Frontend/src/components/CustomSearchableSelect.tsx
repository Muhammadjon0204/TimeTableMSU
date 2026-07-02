import { ChevronDown, X } from 'lucide-react';
import { type CSSProperties, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { createPortal } from 'react-dom';

export type SelectOption = {
  value: string;
  label: string;
  subtitle?: string;
};

type CustomSearchableSelectProps = {
  value: string;
  options: SelectOption[];
  placeholder: string;
  disabled?: boolean;
  clearValue?: string;
  error?: boolean;
  onChange: (value: string) => void;
};

type DropdownPosition = {
  top?: number;
  bottom?: number;
  left: number;
  width: number;
  maxHeight: number;
};

const DROPDOWN_GAP = 8;
const VIEWPORT_MARGIN = 12;
const MAX_DROPDOWN_HEIGHT = 320;
const MIN_DROPDOWN_HEIGHT = 180;

export function CustomSearchableSelect({
  value,
  options,
  placeholder,
  disabled = false,
  clearValue = 'all',
  error = false,
  onChange,
}: CustomSearchableSelectProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [dropdownPosition, setDropdownPosition] = useState<DropdownPosition | null>(null);
  const rootRef = useRef<HTMLDivElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find((option) => option.value === value);
  const isWide = options.some((option) => option.label.length > 24);
  const filteredOptions = useMemo(() => {
    const normalizedSearch = searchText.trim().toLowerCase();

    if (!normalizedSearch) {
      return options;
    }

    return options.filter((option) => `${option.label} ${option.subtitle ?? ''}`.toLowerCase().includes(normalizedSearch));
  }, [options, searchText]);

  const updateDropdownPosition = useCallback(() => {
    if (!rootRef.current) {
      return;
    }

    const rect = rootRef.current.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const maxAvailableWidth = viewportWidth - VIEWPORT_MARGIN * 2;
    const width = Math.min(maxAvailableWidth, rect.width);
    const left = Math.min(Math.max(VIEWPORT_MARGIN, rect.left), viewportWidth - width - VIEWPORT_MARGIN);
    const spaceBelow = viewportHeight - rect.bottom - DROPDOWN_GAP - VIEWPORT_MARGIN;
    const spaceAbove = rect.top - DROPDOWN_GAP - VIEWPORT_MARGIN;
    const shouldOpenUp = spaceBelow < MAX_DROPDOWN_HEIGHT && spaceAbove > spaceBelow;
    const availableHeight = shouldOpenUp ? spaceAbove : spaceBelow;
    const maxHeight = Math.max(MIN_DROPDOWN_HEIGHT, Math.min(MAX_DROPDOWN_HEIGHT, availableHeight));

    setDropdownPosition({
      top: shouldOpenUp ? undefined : rect.bottom + DROPDOWN_GAP,
      bottom: shouldOpenUp ? viewportHeight - rect.top + DROPDOWN_GAP : undefined,
      left,
      width,
      maxHeight,
    });
  }, []);

  useEffect(() => {
    if (!isOpen) {
      setDropdownPosition(null);
      return;
    }

    updateDropdownPosition();
  }, [isOpen, updateDropdownPosition]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    function handlePointerDown(event: MouseEvent) {
      const target = event.target as Node;

      if (rootRef.current?.contains(target) || dropdownRef.current?.contains(target)) {
        return;
      }

      setIsOpen(false);
    }

    function closeDropdown() {
      setIsOpen(false);
    }

    function handleScroll(event: Event) {
      const target = event.target as Node;

      if (dropdownRef.current?.contains(target)) {
        return;
      }

      closeDropdown();
    }

    document.addEventListener('mousedown', handlePointerDown);
    window.addEventListener('resize', closeDropdown);
    window.addEventListener('scroll', handleScroll, true);

    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      window.removeEventListener('resize', closeDropdown);
      window.removeEventListener('scroll', handleScroll, true);
    };
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setIsOpen(false);
      }
    }

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isOpen]);

  function chooseOption(nextValue: string) {
    onChange(nextValue);
    setSearchText('');
    setIsOpen(false);
  }

  function toggleMenu() {
    if (disabled) {
      return;
    }

    setIsOpen((current) => !current);
  }

  const dropdownStyle: CSSProperties | undefined = dropdownPosition
    ? {
        position: 'fixed',
        top: dropdownPosition.top ?? 'auto',
        bottom: dropdownPosition.bottom ?? 'auto',
        left: dropdownPosition.left,
        width: dropdownPosition.width,
        maxHeight: dropdownPosition.maxHeight,
        zIndex: 1100,
      }
    : undefined;

  const dropdownMenu =
    isOpen && dropdownPosition
      ? createPortal(
          <div ref={dropdownRef} className="custom-select__menu custom-select__menu--portal" style={dropdownStyle}>
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
                    className={option.value === value ? 'custom-select__option custom-select__option--active' : 'custom-select__option'}
                    type="button"
                    title={option.label}
                    onClick={() => chooseOption(option.value)}
                  >
                    <span>{option.label}</span>
                    {option.subtitle ? <small>{option.subtitle}</small> : null}
                  </button>
                ))
              ) : (
                <div className="custom-select__empty">Ничего не найдено</div>
              )}
            </div>
          </div>,
          document.body,
        )
      : null;

  const rootClassName = [
    'custom-select',
    isOpen ? 'custom-select--open' : '',
    isWide ? 'custom-select--wide' : '',
    disabled ? 'custom-select--disabled' : '',
    error ? 'custom-select--error' : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <div ref={rootRef} className={rootClassName}>
      <button className="custom-select__control" type="button" onClick={toggleMenu}>
        <span className={selectedOption ? 'custom-select__value' : 'custom-select__placeholder'} title={selectedOption?.label ?? placeholder}>
          {selectedOption?.label ?? placeholder}
        </span>
        {value !== clearValue && !disabled ? (
          <span
            className="custom-select__clear"
            role="button"
            tabIndex={0}
            onClick={(event) => {
              event.stopPropagation();
              chooseOption(clearValue);
            }}
            onKeyDown={(event) => {
              if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                chooseOption(clearValue);
              }
            }}
            aria-label="Очистить фильтр"
          >
            <X size={14} />
          </span>
        ) : null}
        <ChevronDown className="custom-select__chevron" size={16} />
      </button>

      {dropdownMenu}
    </div>
  );
}
