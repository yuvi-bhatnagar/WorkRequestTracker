-- ═══════════════════════════════════════════════════════════════
-- Work Request Tracker — Seed Data
-- Run this AFTER schema.sql on a fresh database.
-- Intended for: manual developer setup / demo environments.
-- The application seeds automatically on first startup via DbInitializer.cs
-- ═══════════════════════════════════════════════════════════════

PRAGMA foreign_keys = ON;

INSERT INTO "WorkRequests" ("Title", "ClientName", "Description", "Priority", "Status", "DueDate", "CreatedAtUtc", "UpdatedAtUtc") VALUES
    ('Redesign Customer Portal',      'Acme Corp',    'Redesign the customer-facing portal UI with new branding guidelines and improved UX.',   'High',   'InProgress', '2027-11-04', '2026-10-16T12:00:00Z', '2026-10-19T10:00:00Z'),
    ('API Performance Audit',         'TechStart Inc','Audit all public APIs for performance bottlenecks and propose optimizations.',              'Medium', 'New',        '2027-11-11', '2026-10-18T09:00:00Z', '2026-10-18T09:00:00Z'),
    ('Fix Login Bug on Mobile',       'RetailPlus',   'Investigate and fix the login failure reported on iOS Safari 17.',                          'High',   'Blocked',    '2027-10-24', '2026-10-20T15:00:00Z', '2026-10-21T08:00:00Z'),
    ('Database Backup Automation',    'FinServ Ltd',  'Automate nightly database backup with 30-day retention policy.',                           'Low',    'Completed',  '2026-10-14', '2026-10-01T07:00:00Z', '2026-10-14T18:00:00Z'),
    ('Integrate Payment Gateway',     'Acme Corp',    'Integrate Stripe payment gateway into the checkout flow.',                                   'High',   'New',        '2027-11-20', '2026-10-20T11:00:00Z', '2026-10-20T11:00:00Z');

INSERT INTO "WorkRequestNotes" ("WorkRequestId", "Text", "CreatedAtUtc") VALUES
    (1, 'Initial design mockups reviewed and approved by the client.',  '2026-10-17T10:00:00Z'),
    (1, 'Development started. Frontend components being built.',        '2026-10-19T10:00:00Z'),
    (3, 'Waiting for iOS test device to reproduce the issue consistently.', '2026-10-21T08:00:00Z');
