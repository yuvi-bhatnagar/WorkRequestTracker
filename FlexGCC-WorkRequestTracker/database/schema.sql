-- ═══════════════════════════════════════════════════════════════
-- Work Request Tracker — SQLite Schema
-- Compatible with EF Core entity configuration in AppDbContext.cs
-- ═══════════════════════════════════════════════════════════════

PRAGMA foreign_keys = ON;

-- ───────────────────────────────────────────────────────────────
-- Table: WorkRequests
-- ───────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "WorkRequests" (
    "Id"           INTEGER NOT NULL CONSTRAINT "PK_WorkRequests" PRIMARY KEY AUTOINCREMENT,
    "Title"        TEXT    NOT NULL CHECK(length("Title") <= 150),
    "ClientName"   TEXT    NOT NULL CHECK(length("ClientName") <= 150),
    "Description"  TEXT    NOT NULL CHECK(length("Description") <= 2000),
    "Priority"     TEXT    NOT NULL CHECK("Priority" IN ('Low', 'Medium', 'High')),
    "Status"       TEXT    NOT NULL CHECK("Status"   IN ('New', 'InProgress', 'Blocked', 'Completed')),
    "DueDate"      TEXT    NOT NULL,           -- stored as 'yyyy-MM-dd'
    "CreatedAtUtc" TEXT    NOT NULL,           -- stored as ISO 8601 UTC
    "UpdatedAtUtc" TEXT    NOT NULL            -- stored as ISO 8601 UTC
);

-- ───────────────────────────────────────────────────────────────
-- Table: WorkRequestNotes
-- ───────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "WorkRequestNotes" (
    "Id"              INTEGER NOT NULL CONSTRAINT "PK_WorkRequestNotes" PRIMARY KEY AUTOINCREMENT,
    "WorkRequestId"   INTEGER NOT NULL,
    "Text"            TEXT    NOT NULL CHECK(length("Text") <= 2000),
    "CreatedAtUtc"    TEXT    NOT NULL,

    CONSTRAINT "FK_WorkRequestNotes_WorkRequests_WorkRequestId"
        FOREIGN KEY ("WorkRequestId")
        REFERENCES "WorkRequests" ("Id")
        ON DELETE CASCADE
);

-- ═══════════════════════════════════════════════════════════════
-- INDEXES
--
-- 1. IX_WorkRequests_Status
--    Purpose : The GET /api/work-requests endpoint supports filtering by
--              status (e.g., ?status=Blocked). A B-tree index on Status
--              allows SQLite to scan only the matching rows instead of
--              performing a full table scan. With many requests this
--              reduces I/O proportional to the selectivity of the value.
--
-- 2. IX_WorkRequests_DueDate
--    Purpose : Supports sorting or filtering by due date. Useful for
--              future "overdue" or "upcoming" dashboard features.
--              Also enables range queries (WHERE DueDate < '2027-01-01')
--              to run efficiently without a full scan.
--
-- 3. IX_WorkRequests_CreatedAtUtc
--    Purpose : The list endpoint orders results by CreatedAtUtc DESC.
--              An index on this column allows the ORDER BY to be
--              satisfied by the index rather than a sort step,
--              improving performance as the dataset grows.
--
-- 4. IX_WorkRequestNotes_WorkRequestId
--    Purpose : Foreign key index. When EF Core loads notes for a given
--              WorkRequest (via JOIN or Include), this index lets
--              SQLite retrieve matching notes in O(log n) rather than
--              scanning the entire notes table. SQLite does not create
--              foreign key indexes automatically.
--
-- NOTE: These are standard B-tree indexes. They do NOT accelerate
-- arbitrary substring searches (LIKE '%keyword%'). Full-text search
-- (FTS5) is a future enhancement for the title/client search feature.
-- ═══════════════════════════════════════════════════════════════

CREATE INDEX IF NOT EXISTS "IX_WorkRequests_Status"
    ON "WorkRequests" ("Status");

CREATE INDEX IF NOT EXISTS "IX_WorkRequests_DueDate"
    ON "WorkRequests" ("DueDate");

CREATE INDEX IF NOT EXISTS "IX_WorkRequests_CreatedAtUtc"
    ON "WorkRequests" ("CreatedAtUtc");

CREATE INDEX IF NOT EXISTS "IX_WorkRequestNotes_WorkRequestId"
    ON "WorkRequestNotes" ("WorkRequestId");
