# FlexGCC — Work Request Tracker

A full-stack Work Request Tracker module built for FlexGCC LLC Senior Full Stack Developer evaluation.

- **Backend**: ASP.NET Core Web API (.NET 8/10), Entity Framework Core, SQLite, xUnit
- **Frontend**: React 19, Vite, Vanilla CSS design system (Modern Dark SaaS UI)
- **Database**: SQLite with relational schema, foreign keys, and optimized indexes

---

## Architecture Overview

```
FlexGCC-WorkRequestTracker/
├── backend/
│   ├── WorkRequestTracker.Api/       # ASP.NET Core Web API project
│   │   ├── Controllers/             # REST endpoints (WorkRequestsController)
│   │   ├── Data/                    # AppDbContext, DbInitializer, Seeder
│   │   ├── DTOs/                    # Request/Response contracts & validators
│   │   ├── Models/                  # Domain entities (WorkRequest, WorkRequestNote)
│   │   └── Program.cs               # Service registration, CORS, middleware
│   └── WorkRequestTracker.Tests/     # xUnit integration & unit test suite (25 tests)
│       ├── CustomWebApplicationFactory.cs
│       └── WorkRequestApiTests.cs
├── frontend/
│   ├── src/
│   │   ├── api/workRequestsApi.js   # Centralized API client & fetch wrappers
│   │   ├── hooks/useDebounce.js     # Debounce utility hook
│   │   ├── utils.js                 # Formatting and UI helpers
│   │   ├── App.jsx                  # Main dashboard component
│   │   ├── index.css                # Dark-mode design system & animations
│   │   └── main.jsx                 # React root
│   ├── .env.example
│   └── package.json
├── database/
│   ├── schema.sql                   # SQLite DDL schema with 4 explained indexes
│   └── seed.sql                     # Realistic mock data
└── docs/
    └── SUBMISSION_RESPONSE.md       # Completed candidate screening submission form
```

---

## Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/) or higher (.NET 10 supported)
- [Node.js](https://nodejs.org/) (v18+) and npm

---

### 1. Running the Backend API

```bash
cd backend/WorkRequestTracker.Api
dotnet run
```

- API Base URL: `http://localhost:5093` (or `https://localhost:7193`)
- Swagger UI Documentation: `http://localhost:5093/swagger`
- The SQLite database file (`workrequests.db`) and seed data are automatically initialized on startup.

---

### 2. Running the Backend Tests

The test suite runs against an isolated SQLite test database with 25 comprehensive unit and integration tests.

```bash
cd backend
dotnet test WorkRequestTracker.Tests/WorkRequestTracker.Tests.csproj
```

**Results:**
- 25 tests passing
- 0 failed, 0 skipped

---

### 3. Running the Frontend

```bash
cd frontend
npm install
npm run dev
```

- Local Dev URL: `http://localhost:5173`
- Configured via `.env` (`VITE_API_BASE_URL=http://localhost:5093`).

To build for production:
```bash
npm run build
```

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/work-requests` | List with query params: `status`, `search`, `page`, `pageSize`, `sortBy`, `sortDescending` |
| `GET` | `/api/work-requests/{id}` | Get single work request by ID with its notes collection |
| `POST` | `/api/work-requests` | Create a new work request (validates required fields, priority, status) |
| `PATCH` | `/api/work-requests/{id}/status` | Update status (`New`, `InProgress`, `Blocked`, `Completed`) |
| `POST` | `/api/work-requests/{id}/notes` | Append an audit/activity note to a work request |

---

## Key Design & Engineering Decisions

1. **RFC 7807 ProblemDetails Standard**: All API validation failures and HTTP errors return standard `ValidationProblemDetails` or `ProblemDetails` payloads.
2. **Resilient Data Layer**: Uses EF Core with SQLite for zero-configuration, cross-platform local development.
3. **Optimistic UI & Debounced Search**: Frontend search inputs debounce at 300ms to avoid hammering the backend, with race condition protection via request identifiers.
4. **Clean Code & Test Coverage**: Integration tests exercise full HTTP request pipelines, verifying validation rules, pagination math, search filtering, and error states.
