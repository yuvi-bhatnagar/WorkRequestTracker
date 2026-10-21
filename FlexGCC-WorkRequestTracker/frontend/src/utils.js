// Utility functions for formatting and classification in user's local timezone

/**
 * Parses date/time strings so they correctly reflect local timezone.
 * - For date-only strings ("YYYY-MM-DD"), constructs a local date at midnight to prevent off-by-one day shifts.
 * - For timestamps (UTC ISO strings from SQLite/backend), ensures UTC indicator is present so the browser accurately converts to the user's local timezone.
 */
export function parseDate(dateStr) {
  if (!dateStr) return null;

  // Date-only format e.g. "2026-10-23"
  if (/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) {
    const [y, m, d] = dateStr.split('-').map(Number);
    return new Date(y, m - 1, d);
  }

  // Datetime string: ensure UTC indicator
  let s = String(dateStr).trim().replace(' ', 'T');
  if (!s.endsWith('Z') && !/[+-]\d{2}:?\d{2}$/.test(s)) {
    s += 'Z';
  }
  const d = new Date(s);
  return isNaN(d.getTime()) ? null : d;
}

/**
 * Format a date in the user's local timezone.
 * E.g. "23 Oct 2026"
 */
export function formatDate(dateStr) {
  const d = parseDate(dateStr);
  if (!d) return '—';
  return d.toLocaleDateString(undefined, {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });
}

/**
 * Format date and time in the user's local timezone.
 * E.g. "22 Oct 2026, 00:06" / "22 Oct 2026, 12:06 AM"
 */
export function formatDateTime(dateStr) {
  const d = parseDate(dateStr);
  if (!d) return '—';
  return d.toLocaleString(undefined, {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function statusClass(status) {
  return {
    New: 'badge-new',
    InProgress: 'badge-inprogress',
    Blocked: 'badge-blocked',
    Completed: 'badge-completed',
  }[status] || 'badge-new';
}

export function priorityClass(priority) {
  return {
    Low: 'badge-low',
    Medium: 'badge-medium',
    High: 'badge-high',
  }[priority] || 'badge-low';
}

export function statusLabel(status) {
  return { InProgress: 'In Progress' }[status] || status;
}

export const ALL_STATUSES = ['New', 'InProgress', 'Blocked', 'Completed'];
export const ALL_PRIORITIES = ['Low', 'Medium', 'High'];
