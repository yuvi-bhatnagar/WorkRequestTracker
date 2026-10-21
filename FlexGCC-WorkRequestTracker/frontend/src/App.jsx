import { useState, useEffect, useCallback, useRef } from 'react';
import {
  getWorkRequests, getWorkRequestById, createWorkRequest,
  updateWorkRequestStatus, addWorkRequestNote,
} from './api/workRequestsApi';
import { formatDate, formatDateTime, statusClass, priorityClass, statusLabel, ALL_STATUSES, ALL_PRIORITIES } from './utils';
import { StatusDropdown, CustomSelect } from './components/Dropdowns';
import './index.css';

// ─── Toast system ──────────────────────────────────────────────────────────
let toastId = 0;
function useToasts() {
  const [toasts, setToasts] = useState([]);
  const add = useCallback((msg, type = 'success') => {
    const id = ++toastId;
    setToasts(t => [...t, { id, msg, type }]);
    setTimeout(() => setToasts(t => t.filter(x => x.id !== id)), 3500);
  }, []);
  return { toasts, addToast: add };
}

// ─── Status Badge ──────────────────────────────────────────────────────────
function StatusBadge({ status }) {
  return (
    <span className={`badge ${statusClass(status)}`}>
      <span className="badge-dot" />
      {statusLabel(status)}
    </span>
  );
}

function PriorityBadge({ priority }) {
  return (
    <span className={`badge ${priorityClass(priority)}`}>
      {priority}
    </span>
  );
}

// ─── Create Form Modal ─────────────────────────────────────────────────────
function CreateModal({ onClose, onCreated, addToast }) {
  const [form, setForm] = useState({
    title: '', clientName: '', description: '',
    priority: 'Medium', status: 'New', dueDate: '',
  });
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [serverError, setServerError] = useState('');

  function set(field) {
    return e => setForm(f => ({ ...f, [field]: e.target.value }));
  }

  // Current local date in YYYY-MM-DD format to prevent past due dates
  const todayStr = (() => {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  })();

  function validate() {
    const e = {};
    if (!form.title.trim()) e.title = 'Title is required.';
    else if (form.title.length > 150) e.title = 'Max 150 characters.';
    if (!form.clientName.trim()) e.clientName = 'Client name is required.';
    else if (form.clientName.length > 150) e.clientName = 'Max 150 characters.';
    if (!form.description.trim()) e.description = 'Description is required.';
    else if (form.description.length > 2000) e.description = 'Max 2000 characters.';
    if (!form.dueDate) {
      e.dueDate = 'Due date is required.';
    } else if (form.dueDate < todayStr) {
      e.dueDate = 'Due date cannot be in the past.';
    }
    return e;
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const errs = validate();
    if (Object.keys(errs).length) { setErrors(errs); return; }
    setSubmitting(true);
    setServerError('');
    try {
      const created = await createWorkRequest({
        title: form.title,
        clientName: form.clientName,
        description: form.description,
        priority: form.priority,
        status: form.status,
        dueDate: form.dueDate,
      });
      addToast(`"${created.title}" created successfully.`);
      onCreated();
    } catch (err) {
      setServerError(err.message || 'Failed to create request.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal" role="dialog" aria-modal="true" aria-label="Create Work Request">
        <div className="modal-header">
          <h2 className="modal-title">➕ New Work Request</h2>
          <button className="modal-close" onClick={onClose} aria-label="Close">✕</button>
        </div>
        <form className="modal-body" onSubmit={handleSubmit} noValidate>
          {serverError && (
            <div className="alert alert-error" role="alert">
              <span className="alert-icon">⚠</span>
              <span>{serverError}</span>
            </div>
          )}

          <div className="form-grid">
            <div className={`form-group form-grid-full`}>
              <label className="form-label">Title <span className="required">*</span></label>
              <input
                id="create-title"
                className={`form-input${errors.title ? ' input-error' : ''}`}
                value={form.title}
                onChange={set('title')}
                placeholder="Brief title of the work request"
                maxLength={150}
              />
              {errors.title && <span className="form-error">{errors.title}</span>}
            </div>

            <div className="form-group">
              <label className="form-label">Client Name <span className="required">*</span></label>
              <input
                id="create-clientName"
                className={`form-input${errors.clientName ? ' input-error' : ''}`}
                value={form.clientName}
                onChange={set('clientName')}
                placeholder="Client or company name"
                maxLength={150}
              />
              {errors.clientName && <span className="form-error">{errors.clientName}</span>}
            </div>

            <div className="form-group">
              <label className="form-label">Due Date <span className="required">*</span></label>
              <input
                id="create-dueDate"
                type="date"
                min={todayStr}
                className={`form-input${errors.dueDate ? ' input-error' : ''}`}
                value={form.dueDate}
                onChange={set('dueDate')}
              />
              {errors.dueDate && <span className="form-error">{errors.dueDate}</span>}
            </div>

            <div className="form-group">
              <label className="form-label">Priority <span className="required">*</span></label>
              <CustomSelect
                id="create-priority"
                value={form.priority}
                onChange={v => setForm(f => ({ ...f, priority: v }))}
                options={ALL_PRIORITIES.map(p => ({
                  value: p,
                  label: p,
                  dotColor: p === 'High' ? 'var(--priority-high)' : p === 'Medium' ? 'var(--priority-medium)' : 'var(--priority-low)'
                }))}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Status <span className="required">*</span></label>
              <CustomSelect
                id="create-status"
                value={form.status}
                onChange={v => setForm(f => ({ ...f, status: v }))}
                options={ALL_STATUSES.map(s => ({
                  value: s,
                  label: statusLabel(s),
                  dotColor: s === 'New' ? 'var(--status-new)' : s === 'InProgress' ? 'var(--status-inprogress)' : s === 'Blocked' ? 'var(--status-blocked)' : 'var(--status-completed)'
                }))}
              />
            </div>

            <div className="form-group form-grid-full">
              <label className="form-label">Description <span className="required">*</span></label>
              <textarea
                id="create-description"
                className={`form-textarea${errors.description ? ' input-error' : ''}`}
                value={form.description}
                onChange={set('description')}
                placeholder="Detailed description of the work required"
                maxLength={2000}
                rows={4}
              />
              {errors.description && <span className="form-error">{errors.description}</span>}
            </div>
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
            <button id="create-submit" type="submit" className="btn btn-primary" disabled={submitting}>
              {submitting ? <><span className="spinner spinner-sm" /> Creating…</> : 'Create Request'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ─── Detail / Notes Modal ──────────────────────────────────────────────────
function DetailModal({ requestId, onClose, onUpdated, addToast }) {
  const [item, setItem] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [noteText, setNoteText] = useState('');
  const [addingNote, setAddingNote] = useState(false);
  const [noteError, setNoteError] = useState('');
  const [statusUpdating, setStatusUpdating] = useState(false);

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try {
      const data = await getWorkRequestById(requestId);
      setItem(data);
    } catch (err) {
      setError(err.message || 'Failed to load request.');
    } finally {
      setLoading(false);
    }
  }, [requestId]);

  useEffect(() => { load(); }, [load]);

  async function handleStatusChange(newStatus) {
    setStatusUpdating(true);
    try {
      const updated = await updateWorkRequestStatus(requestId, newStatus);
      setItem(updated);
      addToast(`Status updated to "${statusLabel(newStatus)}".`);
      onUpdated();
    } catch (err) {
      addToast(err.message || 'Failed to update status.', 'error');
    } finally {
      setStatusUpdating(false);
    }
  }

  async function handleAddNote(e) {
    e.preventDefault();
    if (!noteText.trim()) { setNoteError('Note text cannot be empty.'); return; }
    setAddingNote(true); setNoteError('');
    try {
      await addWorkRequestNote(requestId, noteText);
      setNoteText('');
      addToast('Note added successfully.');
      await load();
      onUpdated();
    } catch (err) {
      setNoteError(err.message || 'Failed to add note.');
    } finally {
      setAddingNote(false);
    }
  }

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal modal-lg" role="dialog" aria-modal="true" aria-label="Request Details">
        <div className="modal-header">
          <h2 className="modal-title">📋 Request Details</h2>
          <button className="modal-close" onClick={onClose} aria-label="Close">✕</button>
        </div>
        <div className="modal-body">
          {loading && <div className="loading-overlay"><div className="spinner" /><span>Loading…</span></div>}
          {error && <div className="alert alert-error"><span className="alert-icon">⚠</span>{error}</div>}
          {item && !loading && (
            <>
              <div className="detail-section">
                <div className="detail-title">Title</div>
                <div className="detail-value" style={{ fontWeight: 700, fontSize: '1.05rem' }}>{item.title}</div>
              </div>

              <div className="detail-grid detail-section">
                <div>
                  <div className="detail-title">Client</div>
                  <div className="detail-value">{item.clientName}</div>
                </div>
                <div>
                  <div className="detail-title">Due Date</div>
                  <div className="detail-value">{formatDate(item.dueDate)}</div>
                </div>
                <div>
                  <div className="detail-title">Priority</div>
                  <div className="detail-value"><PriorityBadge priority={item.priority} /></div>
                </div>
                <div>
                  <div className="detail-title">Status</div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.65rem' }}>
                    <StatusDropdown
                      value={item.status}
                      onChange={handleStatusChange}
                      disabled={statusUpdating}
                    />
                    {statusUpdating && <span className="spinner spinner-sm" />}
                  </div>
                </div>
                <div>
                  <div className="detail-title">Created</div>
                  <div className="detail-value" style={{ fontSize: '0.8rem' }}>{formatDateTime(item.createdAtUtc)}</div>
                </div>
                <div>
                  <div className="detail-title">Updated</div>
                  <div className="detail-value" style={{ fontSize: '0.8rem' }}>{formatDateTime(item.updatedAtUtc)}</div>
                </div>
              </div>

              <div className="detail-section">
                <div className="detail-title">Description</div>
                <div className="detail-value" style={{ whiteSpace: 'pre-wrap' }}>{item.description}</div>
              </div>

              <div className="detail-section">
                <div className="detail-title">Notes ({item.notes?.length || 0})</div>
                <div className="notes-list">
                  {(!item.notes || item.notes.length === 0) && (
                    <div style={{ color: 'var(--text-muted)', fontSize: '0.85rem', padding: '0.5rem 0' }}>
                      No notes yet.
                    </div>
                  )}
                  {item.notes?.map(note => (
                    <div key={note.id} className="note-item">
                      <div className="note-text">{note.text}</div>
                      <div className="note-time">{formatDateTime(note.createdAtUtc)}</div>
                    </div>
                  ))}
                </div>
                <form onSubmit={handleAddNote}>
                  {noteError && <div className="form-error" style={{ marginBottom: '0.5rem' }}>{noteError}</div>}
                  <div className="note-form">
                    <textarea
                      id="note-text-input"
                      className="form-textarea"
                      value={noteText}
                      onChange={e => setNoteText(e.target.value)}
                      placeholder="Add a note…"
                      maxLength={2000}
                    />
                    <button
                      id="note-submit-btn"
                      type="submit"
                      className="btn btn-primary"
                      disabled={addingNote}
                    >
                      {addingNote ? <span className="spinner spinner-sm" /> : 'Add'}
                    </button>
                  </div>
                </form>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

// ─── Main App ──────────────────────────────────────────────────────────────
export default function App() {
  const { toasts, addToast } = useToasts();

  // Filter / search state
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const PAGE_SIZE = 10;

  // Data state
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Summary counts
  const [summary, setSummary] = useState({ total: 0, new: 0, inProgress: 0, blocked: 0 });

  // UI state
  const [showCreate, setShowCreate] = useState(false);
  const [detailId, setDetailId] = useState(null);

  // Status update per-row
  const [updatingStatus, setUpdatingStatus] = useState({});

  // Debounce search
  const debounceTimer = useRef(null);
  useEffect(() => {
    clearTimeout(debounceTimer.current);
    debounceTimer.current = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 300);
    return () => clearTimeout(debounceTimer.current);
  }, [search]);

  // Reset page when filter changes
  useEffect(() => { setPage(1); }, [statusFilter]);

  // Fetch list (with stale request protection)
  const fetchRef = useRef(0);
  const fetchList = useCallback(async () => {
    const reqId = ++fetchRef.current;
    setLoading(true); setError('');
    try {
      const result = await getWorkRequests({
        status: statusFilter || undefined,
        search: debouncedSearch || undefined,
        page,
        pageSize: PAGE_SIZE,
      });
      if (reqId !== fetchRef.current) return; // stale
      setData(result);
    } catch (err) {
      if (reqId !== fetchRef.current) return;
      setError(err.message || 'Failed to load work requests.');
    } finally {
      if (reqId === fetchRef.current) setLoading(false);
    }
  }, [statusFilter, debouncedSearch, page]);

  useEffect(() => { fetchList(); }, [fetchList]);

  // Fetch summary counts (all statuses, no filter)
  const fetchSummary = useCallback(async () => {
    try {
      const [all, newR, inP, blk] = await Promise.all([
        getWorkRequests({ pageSize: 1 }),
        getWorkRequests({ status: 'New', pageSize: 1 }),
        getWorkRequests({ status: 'InProgress', pageSize: 1 }),
        getWorkRequests({ status: 'Blocked', pageSize: 1 }),
      ]);
      setSummary({
        total: all.totalCount,
        new: newR.totalCount,
        inProgress: inP.totalCount,
        blocked: blk.totalCount,
      });
    } catch { /* ignore summary errors */ }
  }, []);

  useEffect(() => { fetchSummary(); }, [fetchSummary]);

  function refresh() {
    fetchList();
    fetchSummary();
  }

  async function handleRowStatusChange(id, newStatus) {
    setUpdatingStatus(s => ({ ...s, [id]: true }));
    try {
      await updateWorkRequestStatus(id, newStatus);
      addToast(`Status updated to "${statusLabel(newStatus)}".`);
      refresh();
    } catch (err) {
      addToast(err.message || 'Failed to update status.', 'error');
    } finally {
      setUpdatingStatus(s => ({ ...s, [id]: false }));
    }
  }

  // Pagination helpers
  const totalPages = data?.totalPages || 0;
  const pageNumbers = () => {
    const pages = [];
    const start = Math.max(1, page - 2);
    const end = Math.min(totalPages, page + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  };

  return (
    <div className="app-wrapper">
      {/* Header */}
      <header className="app-header">
        <div className="header-brand">
          <div className="header-logo">WR</div>
          <div>
            <div className="header-title">Work Request Tracker</div>
            <div className="header-subtitle">Enterprise Operations & Workflow Management</div>
          </div>
        </div>
        <button
          id="btn-create-request"
          className="btn btn-primary"
          onClick={() => setShowCreate(true)}
        >
          ✚ New Request
        </button>
      </header>

      {/* Main */}
      <main className="main-content">
        {/* Summary Cards */}
        <div className="summary-grid" role="region" aria-label="Summary statistics">
          <div className="summary-card total">
            <span className="summary-label">Total Requests</span>
            <span className="summary-value">{summary.total}</span>
            <span className="summary-icon">📊</span>
          </div>
          <div className="summary-card new">
            <span className="summary-label">New</span>
            <span className="summary-value">{summary.new}</span>
            <span className="summary-icon">🆕</span>
          </div>
          <div className="summary-card inprogress">
            <span className="summary-label">In Progress</span>
            <span className="summary-value">{summary.inProgress}</span>
            <span className="summary-icon">⚙</span>
          </div>
          <div className="summary-card blocked">
            <span className="summary-label">Blocked</span>
            <span className="summary-value">{summary.blocked}</span>
            <span className="summary-icon">🚫</span>
          </div>
        </div>

        {/* Toolbar */}
        <div className="toolbar">
          <div className="toolbar-search">
            <span className="search-icon">🔍</span>
            <input
              id="search-input"
              type="search"
              placeholder="Search by title or client name…"
              value={search}
              onChange={e => setSearch(e.target.value)}
              aria-label="Search work requests"
            />
          </div>
          <div className="toolbar-filter">
            <CustomSelect
              id="status-filter"
              value={statusFilter}
              onChange={setStatusFilter}
              placeholder="All Statuses"
              options={[
                { value: '', label: 'All Statuses' },
                ...ALL_STATUSES.map(s => ({
                  value: s,
                  label: statusLabel(s),
                  dotColor: s === 'New' ? 'var(--status-new)' : s === 'InProgress' ? 'var(--status-inprogress)' : s === 'Blocked' ? 'var(--status-blocked)' : 'var(--status-completed)'
                }))
              ]}
            />
          </div>
        </div>

        {/* Table */}
        <div className="table-container">
          <div className="table-header-row">
            <span className="table-title">
              {data ? `${data.totalCount} request${data.totalCount !== 1 ? 's' : ''}` : 'Work Requests'}
            </span>
            {loading && <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--text-muted)', fontSize: '0.8rem' }}>
              <div className="spinner spinner-sm" /> Loading…
            </div>}
          </div>

          {error && (
            <div className="alert alert-error" style={{ margin: '1rem' }}>
              <span className="alert-icon">⚠</span>
              <span>{error}</span>
              <button className="btn btn-sm btn-secondary" onClick={fetchList} style={{ marginLeft: 'auto' }}>Retry</button>
            </div>
          )}

          {!loading && !error && data?.items?.length === 0 && (
            <div className="empty-state">
              <div className="empty-icon">📭</div>
              <div className="empty-title">No requests found</div>
              <div className="empty-subtitle">
                {debouncedSearch || statusFilter
                  ? 'Try adjusting your search or filter.'
                  : 'Create your first work request to get started.'}
              </div>
            </div>
          )}

          {data?.items?.length > 0 && (
            <table aria-label="Work requests table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Title</th>
                  <th>Client</th>
                  <th>Priority</th>
                  <th>Status</th>
                  <th>Due Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map(item => (
                  <tr
                    key={item.id}
                    onClick={() => setDetailId(item.id)}
                    role="button"
                    tabIndex={0}
                    onKeyDown={e => e.key === 'Enter' && setDetailId(item.id)}
                    aria-label={`View details for ${item.title}`}
                  >
                    <td style={{ color: 'var(--text-muted)', fontSize: '0.8rem', whiteSpace: 'nowrap' }}>#{item.id}</td>
                    <td>
                      <div className="td-title" title={item.title}>{item.title}</div>
                    </td>
                    <td>
                      <div className="td-client" title={item.clientName}>{item.clientName}</div>
                    </td>
                    <td><PriorityBadge priority={item.priority} /></td>
                    <td><StatusBadge status={item.status} /></td>
                    <td style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>{formatDate(item.dueDate)}</td>
                    <td onClick={e => e.stopPropagation()}>
                      <div className="row-actions">
                        <StatusDropdown
                          value={item.status}
                          onChange={newStatus => handleRowStatusChange(item.id, newStatus)}
                          disabled={updatingStatus[item.id]}
                        />
                        {updatingStatus[item.id] && <span className="spinner spinner-sm" />}
                        <button
                          className="btn btn-sm btn-secondary"
                          onClick={() => setDetailId(item.id)}
                          aria-label={`View notes for ${item.title}`}
                          title="View details & notes"
                        >
                          🗒
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {/* Pagination */}
          {data && totalPages > 1 && (
            <div className="pagination">
              <div className="pagination-info">
                Showing {((page - 1) * PAGE_SIZE) + 1}–{Math.min(page * PAGE_SIZE, data.totalCount)} of {data.totalCount}
              </div>
              <div className="pagination-controls">
                <button
                  className="pagination-btn"
                  onClick={() => setPage(1)}
                  disabled={page === 1}
                  aria-label="First page"
                >«</button>
                <button
                  className="pagination-btn"
                  onClick={() => setPage(p => Math.max(1, p - 1))}
                  disabled={page === 1}
                  aria-label="Previous page"
                >‹</button>
                {pageNumbers().map(n => (
                  <button
                    key={n}
                    className={`pagination-btn${n === page ? ' active' : ''}`}
                    onClick={() => setPage(n)}
                    aria-label={`Page ${n}`}
                    aria-current={n === page ? 'page' : undefined}
                  >{n}</button>
                ))}
                <button
                  className="pagination-btn"
                  onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  aria-label="Next page"
                >›</button>
                <button
                  className="pagination-btn"
                  onClick={() => setPage(totalPages)}
                  disabled={page === totalPages}
                  aria-label="Last page"
                >»</button>
              </div>
            </div>
          )}
        </div>
      </main>

      {/* Modals */}
      {showCreate && (
        <CreateModal
          onClose={() => setShowCreate(false)}
          onCreated={() => { setShowCreate(false); setPage(1); refresh(); }}
          addToast={addToast}
        />
      )}
      {detailId && (
        <DetailModal
          requestId={detailId}
          onClose={() => setDetailId(null)}
          onUpdated={() => { refresh(); }}
          addToast={addToast}
        />
      )}

      {/* Toast notifications */}
      <div className="toast-container" role="status" aria-live="polite">
        {toasts.map(t => (
          <div key={t.id} className={`toast toast-${t.type}`}>
            <span>{t.type === 'success' ? '✅' : '❌'}</span>
            <span>{t.msg}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
