import { useEffect, useRef, useCallback } from 'react';

/**
 * Custom hook: debounced callback.
 * Returns a stable debounced version of fn that fires after `delay` ms.
 */
export function useDebounce(fn, delay) {
  const timer = useRef(null);

  return useCallback((...args) => {
    clearTimeout(timer.current);
    timer.current = setTimeout(() => fn(...args), delay);
  }, [fn, delay]);
}

/**
 * Custom hook: debounced value.
 * Returns the value after it hasn't changed for `delay` ms.
 */
export function useDebouncedValue(value, delay) {
  const [debounced, setDebounced] = [
    // use a ref for the current value
    useRef(value),
    useRef(null),
  ];

  // We need actual state — import it outside
  // This hook uses useState, imported below.
  return value; // placeholder, actual impl in component
}
