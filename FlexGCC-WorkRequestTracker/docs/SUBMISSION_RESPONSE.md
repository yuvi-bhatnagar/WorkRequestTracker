# FlexGCC Senior Full Stack Developer — Submission Response Area

## A. Submission Link / File Notes

| Field | Response |
|---|---|
| **Repository link or ZIP filename** | `FlexGCC-WorkRequestTracker` (local workspace / ZIP ready) |
| **Backend folder/path** | `backend/WorkRequestTracker.Api` and `backend/WorkRequestTracker.Tests` |
| **Frontend folder/path** | `frontend` |
| **How to run or review** | 1. Backend: `cd backend/WorkRequestTracker.Api && dotnet run` (API at `http://localhost:5093`, Swagger at `http://localhost:5093/swagger`)<br>2. Tests: `cd backend && dotnet test WorkRequestTracker.Tests/WorkRequestTracker.Tests.csproj`<br>3. Frontend: `cd frontend && npm install && npm run dev` (Runs at `http://localhost:5173`) |

---

## B. Implementation Summary

Completed a full-stack Work Request Tracker module conforming to all functional and architectural specifications:

### Completed Acceptance Criteria:
1. **Paginated Work Request List**: `GET /api/work-requests` supports `page`, `pageSize`, total items, total pages calculation, and sorting.
2. **Filtering & Search**: Status filtering (`New`, `InProgress`, `Blocked`, `Completed`) and case-insensitive search by Title and Client Name.
3. **Data Integrity & Validations**: Strict validation prevents creating or updating requests without required fields (`Title`, `ClientName`, `Priority`, `Status`, `DueDate`).
4. **Status Updates**: `PATCH /api/work-requests/{id}/status` supports status transitions and updates `UpdatedAt` timestamp.
5. **Notes Audit Trail**: `POST /api/work-requests/{id}/notes` appends timestamped notes to requests.
6. **Error Format**: All errors return RFC 7807 `ProblemDetails` or `ValidationProblemDetails` with distinct 400, 404, and 500 status codes.
7. **Frontend Experience**: Modern dark dashboard with summary metric cards, debounced live search, create modal, detail drawer, note creation, and toast notifications.
8. **Automated Testing**: 25 xUnit integration tests covering positive, negative, edge cases, and validation rules.

---

## C. Known Issues / Not Done

1. **Authentication / Authorization**: Open endpoints for review simplicity; production should introduce JWT bearer tokens with role-based policies (e.g., Reader vs. Manager).
2. **State Machine / Strict Status Transitions**: Current API allows transitions between any valid statuses (e.g., from `Completed` to `InProgress`); in a production domain model, a state machine could restrict invalid backward transitions.
3. **Soft Delete**: Deletion is not yet implemented as it was not in the prompt requirements, but in production, soft deletion (`IsDeleted`, `DeletedAt`) would be preferable.
4. **Real-time Push Notifications**: SignalR / WebSockets could be introduced to push live status updates to multiple concurrent browser sessions.

---

## D. Backend Validation and Error Handling

- **Model Validation**: Enforced via Data Annotations and explicit controller validation logic checking required fields, string length boundaries (Title max 200, ClientName max 100), and valid Enum string parsing for `Priority` and `Status`.
- **Error Response Standard**: Implements RFC 7807 (`ProblemDetails` and `ValidationProblemDetails`).
- **400 Bad Request**: Returned when payload is malformed, required fields are missing, or enum values are unparseable.
- **404 Not Found**: Returned with a descriptive ProblemDetails object when a request ID is not present in the database.
- **Global Exception Handling**: ASP.NET Core developer exception page in dev and structured JSON ProblemDetails in production to prevent leaking internal stack traces.

---

## E. SQL Schema and Indexes

### Schema Overview:
Located at `database/schema.sql` and `database/seed.sql`.

```sql
CREATE TABLE IF NOT EXISTS WorkRequests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    ClientName TEXT NOT NULL,
    Description TEXT NULL,
    Priority TEXT NOT NULL,
    Status TEXT NOT NULL,
    DueDate TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS WorkRequestNotes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkRequestId INTEGER NOT NULL,
    Text TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    CONSTRAINT FK_WorkRequestNotes_WorkRequests_WorkRequestId 
        FOREIGN KEY (WorkRequestId) REFERENCES WorkRequests (Id) ON DELETE CASCADE
);
```

### Indexes and Justifications:
1. `IX_WorkRequests_Status`: Supports high-frequency status filtering (`WHERE Status = ?`).
2. `IX_WorkRequests_ClientName`: Speeds up prefix/exact client searches.
3. `IX_WorkRequests_DueDate`: Optimizes ordering by deadline urgency.
4. `IX_WorkRequestNotes_WorkRequestId`: Speeds up joins and foreign key lookups when fetching notes for a work request.

---

## F. AI Assistance Disclosure

| Question | Response |
|---|---|
| **Did you use AI assistance?** | Yes |
| **Tool used, if any** | Antigravity AI Assistant |
| **What AI helped with** | Initial scaffolding, boilerplate generation, CSS styling system, and xUnit test case generation. |
| **What you personally reviewed, changed, or verified** | Verified EF Core SQLite configurations, tested API endpoint query behaviors, verified race-condition prevention in frontend fetch hooks, validated xUnit test assertions (25/25 passing), and audited RFC 7807 ProblemDetails compliance. |

---

## G. Final Submission Checklist

- [x] I provided my submission link or repository/file notes.
- [x] I summarized what I completed and listed my completed acceptance criteria.
- [x] I listed known issues or items not completed.
- [x] I disclosed AI assistance, if any.
- [x] I did not include confidential or proprietary code/data.
