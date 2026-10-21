import React, { useState, useRef, useEffect } from 'react';
import { statusClass, statusLabel, ALL_STATUSES } from '../utils';

/**
 * Interactive Status Dropdown Pill:
 * Displays current status with colored dot and chevron.
 * Clicking opens a sleek floating dark menu with status dots and checkmarks.
 */
export function StatusDropdown({ value, onChange, disabled = false, size = 'md' }) {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    function handleClickOutside(e) {
      if (ref.current && !ref.current.contains(e.target)) {
        setOpen(false);
      }
    }
    function handleKeyDown(e) {
      if (e.key === 'Escape') setOpen(false);
    }
    if (open) {
      document.addEventListener('mousedown', handleClickOutside);
      document.addEventListener('keydown', handleKeyDown);
      return () => {
        document.removeEventListener('mousedown', handleClickOutside);
        document.removeEventListener('keydown', handleKeyDown);
      };
    }
  }, [open]);

  return (
    <div className={`status-dropdown-container ${size}`} ref={ref}>
      <button
        type="button"
        className={`status-dropdown-trigger badge ${statusClass(value)}`}
        onClick={(e) => {
          e.stopPropagation();
          if (!disabled) setOpen(!open);
        }}
        disabled={disabled}
        aria-haspopup="listbox"
        aria-expanded={open}
        title="Click to change status"
      >
        <span className="badge-dot" />
        <span className="status-text">{statusLabel(value)}</span>
        <svg
          className={`dropdown-chevron ${open ? 'open' : ''}`}
          width="10" height="10" viewBox="0 0 24 24" fill="none"
          stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"
        >
          <polyline points="6 9 12 15 18 9" />
        </svg>
      </button>

      {open && (
        <div className="status-dropdown-menu" role="listbox">
          {ALL_STATUSES.map((status) => {
            const isSelected = status === value;
            return (
              <button
                key={status}
                type="button"
                className={`status-dropdown-item ${statusClass(status)} ${isSelected ? 'selected' : ''}`}
                onClick={(e) => {
                  e.stopPropagation();
                  setOpen(false);
                  if (status !== value) onChange(status);
                }}
                role="option"
                aria-selected={isSelected}
              >
                <span className="badge-dot" />
                <span className="item-label">{statusLabel(status)}</span>
                {isSelected && <span className="checkmark">✓</span>}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}

/**
 * Custom Dropdown Select for generic options (Toolbar filters, Create forms).
 */
export function CustomSelect({
  value,
  onChange,
  options,
  placeholder = 'Select…',
  id,
  className = '',
  disabled = false,
}) {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    function handleClickOutside(e) {
      if (ref.current && !ref.current.contains(e.target)) {
        setOpen(false);
      }
    }
    function handleKeyDown(e) {
      if (e.key === 'Escape') setOpen(false);
    }
    if (open) {
      document.addEventListener('mousedown', handleClickOutside);
      document.addEventListener('keydown', handleKeyDown);
      return () => {
        document.removeEventListener('mousedown', handleClickOutside);
        document.removeEventListener('keydown', handleKeyDown);
      };
    }
  }, [open]);

  const selectedOption = options.find((o) => o.value === value);

  return (
    <div className={`custom-select-container ${className}`} ref={ref} id={id}>
      <button
        type="button"
        className={`custom-select-trigger ${open ? 'active' : ''}`}
        onClick={() => !disabled && setOpen(!open)}
        disabled={disabled}
        aria-haspopup="listbox"
        aria-expanded={open}
      >
        <span className="selected-label">
          {selectedOption ? selectedOption.label : placeholder}
        </span>
        <svg
          className={`dropdown-chevron ${open ? 'open' : ''}`}
          width="12" height="12" viewBox="0 0 24 24" fill="none"
          stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"
        >
          <polyline points="6 9 12 15 18 9" />
        </svg>
      </button>

      {open && (
        <div className="custom-select-menu" role="listbox">
          {options.map((opt) => {
            const isSelected = opt.value === value;
            return (
              <button
                key={opt.value}
                type="button"
                className={`custom-select-item ${isSelected ? 'selected' : ''}`}
                onClick={() => {
                  setOpen(false);
                  onChange(opt.value);
                }}
                role="option"
                aria-selected={isSelected}
              >
                {opt.dotColor && (
                  <span
                    className="badge-dot"
                    style={{ background: opt.dotColor }}
                  />
                )}
                <span className="item-label">{opt.label}</span>
                {isSelected && <span className="checkmark">✓</span>}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
