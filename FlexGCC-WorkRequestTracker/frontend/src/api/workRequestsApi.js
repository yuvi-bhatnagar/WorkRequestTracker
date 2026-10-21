const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5196';

async function request(path, options = {}) {
  const url = `${BASE_URL}${path}`;
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...options.headers },
    ...options,
  });

  if (!res.ok) {
    let errorData;
    try {
      errorData = await res.json();
    } catch {
      errorData = { title: `HTTP ${res.status}`, detail: res.statusText };
    }
    const err = new Error(errorData.detail || errorData.title || 'Request failed');
    err.status = res.status;
    err.data = errorData;
    throw err;
  }

  // 204 No Content
  if (res.status === 204) return null;

  return res.json();
}

export async function getWorkRequests({ status, search, page = 1, pageSize = 10 } = {}) {
  const params = new URLSearchParams();
  if (status) params.set('status', status);
  if (search) params.set('search', search);
  params.set('page', String(page));
  params.set('pageSize', String(pageSize));
  return request(`/api/work-requests?${params}`);
}

export async function getWorkRequestById(id) {
  return request(`/api/work-requests/${id}`);
}

export async function createWorkRequest(data) {
  return request('/api/work-requests', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export async function updateWorkRequestStatus(id, status) {
  return request(`/api/work-requests/${id}/status`, {
    method: 'PATCH',
    body: JSON.stringify({ status }),
  });
}

export async function addWorkRequestNote(id, text) {
  return request(`/api/work-requests/${id}/notes`, {
    method: 'POST',
    body: JSON.stringify({ text }),
  });
}
