# Expense Tracker — Project Status

> Status verified against the codebase on 2026-07-16. Items marked **[unverified]** are leads that still need confirming.

## Project Goal

This is a paycheck-based budgeting application, **not** a generic expense tracker.

Workflow:

1. User creates a future paycheck.
2. User plans expenses/bills against that paycheck.
3. Expenses are settled over time.
4. Paycheck is closed once budgeting for that paycheck is complete.

Future enhancement:
- Distinguish Planned vs Settled expenses.

---

# Current Architecture

- ASP.NET Core 8 Web API
- EF Core Code First
- SQL Server
- Service Layer
- DTOs
- Dependency Injection
- CurrentUserService
- FluentValidation
- Global Exception Handler
- Request Logging Middleware
- Serilog (console + rolling daily file sink, configured from `appsettings.json`)

---

# Completed Features

## Authentication

- User Registration
- Password Hashing
- Login
- JWT Generation
- JWT Bearer configured in the pipeline (`AddAuthentication` / `UseAuthentication` / `UseAuthorization`)
- CurrentUserService
- User-scoped queries — every service filters by `_currentUserService.UserId`

> **Not complete: endpoint authorization.** Only `PaychecksController.CreatePaycheck` carries `[Authorize]`. It is commented out on `GetPaycheck`, and absent from `ExpensesController`, `CategoriesController`, and every other paycheck action. This fails closed rather than leaking — `CurrentUserService.UserId` throws `UnauthorizedAccessException` when there are no claims — but the rejection comes from a service, not the auth pipeline, so callers get a 500 where a 401 belongs. See Remaining Backend Tasks #1.

## Paychecks

- Create
- Get
- Get All — paginated, filtered, sortable
- Close Paycheck
- Get Summary
- Get Dashboard — **implemented in the service but not exposed by any controller action**

> Update and Delete do not exist.

## Expenses

- Create
- Update
- Delete
- Get
- Get All — not yet paginated (see Remaining Backend Tasks #2)

## Categories

- Create
- Get All

> Update and Delete do not exist. This is Create + Read, not CRUD.

## Pagination / Filtering / Sorting — Paychecks only

Done and verified end-to-end against the running API:

- `QueryRequest` base (`Page`, `PageSize`, `SortBy`, `Descending`)
- `PaycheckQueryRequest` (`Search`, `IsClosed`)
- `PagedResponse<T>` returning `Items`, `Page`, `PageSize`, `TotalRecords`, `TotalPages`
- `PaycheckQueryRequestValidator` — bounds `Page`/`PageSize`, whitelists `SortBy`
- Whitelist `ApplySort` switch (no reflection, no dynamic LINQ)
- `Skip`/`Take`, count-after-filter, `IQueryable` held until final projection

Verified: default sort (ReceivedDate desc), `IsClosed`, `Search`, `SortBy=Amount&Descending=true`, `Page=2&PageSize=4`, and 400s on invalid `SortBy` / `PageSize=101` / `Page=0`.

## Validation

- FluentValidation
- Generic Validation Filter (`ValidationFilter<T>`, registered as an open generic)

## Exception Handling

- Global Exception Handler
- NotFoundException
- ConflictException
- BusinessRuleException

## Logging

### Request Logging Middleware

Logs: TraceId, UserId, Request Method, Request Path, Status Code, Duration.

### Business Logging

Important business events only — Paycheck Closed, Expense Created, Expense Deleted, Paycheck list retrieved.

### Global Exception Handler

Unexpected exceptions only.

---

# Remaining Backend Tasks

Ordered by what actually blocks or exposes the project.

## 1. Endpoint Authorization  ← highest priority

Add `[Authorize]` across controllers (controller-level, with `[AllowAnonymous]` on `LoginController`).

Currently one endpoint of ~fifteen is protected. Listed as complete in earlier notes; it is not. This is the most likely thing to be probed in an interview, and "the service throws an exception" is a much weaker answer than "the pipeline rejects unauthenticated requests before the action runs."

## 2. Pagination / Filtering / Sorting — Expenses

Paychecks is done (above). Expenses remains:

- `ExpenseQueryRequest` — has `CategoryId`, `PaycheckId`, `FromDate`, `ToDate`; needs `Search` for parity
- `ExpenseQueryRequestValidator` — bounds, `SortBy` whitelist, `ToDate >= FromDate`
- Service: filter → sort → count → page → project, mirroring `PaycheckService`
- Sort fields: Description, Amount, ExpenseDate, CategoryName

Design decision already taken: `ExpenseQueryRequest.PaycheckId` is nullable, so it becomes an *optional* filter — omit it to get all expenses across paychecks. The query-string name stays `paycheckId`, so existing callers are unaffected.

## 3. Fix the test project

`ExpenseTracker.Api.Tests` **does not compile.** `PaycheckServiceTests.cs`, `ExpenseServiceTests.cs`, and `CategoriesServiceTests.cs` construct services with only a `DbContext` (e.g. `new PaycheckService(context)`), but the real constructors need `(DbContext, ICurrentUserService, ILogger<T>)`.

Fix: a fake `ICurrentUserService` returning a fixed `UserId`, plus `NullLogger<T>.Instance`.

This is not future work — it is broken code in the tree today, and it blocks every later testing task.

## 4. Missing CRUD operations

- Paychecks: Update, Delete
- Categories: Update, Delete

Both were previously listed as complete. Decide whether they are actually wanted — a closed paycheck arguably should not be deletable, and category delete already has `OnDelete(SetNull)` semantics designed for it.

## 5. Wire up or remove `GetDashboardAsync`

Implemented in `PaycheckService`, declared on `IPaycheckService`, reachable from nothing. Either add the controller action or delete it.

## 6. Database Improvements

- ~~Foreign Key Indexes~~ — **already done.** EF Core creates these automatically; `InitialCreate` has `IX_Categories_UserId`, `IX_Expenses_CategoryId`, `IX_Expenses_PaycheckId`, `IX_Paychecks_UserId`.
- **Unique Email Index — missing, and it is a correctness bug.** No `HasIndex`/`IsUnique` in the DbContext, no `unique: true` in any migration. `RegisterAsync` checks uniqueness in application code, which is a check-then-act race: two concurrent registrations for the same email can both pass and both insert. The DB constraint is what makes it safe; the app check only produces a friendlier error.
- **Unique Category Name per user — missing.** Note the nullable `UserId` (system categories share `UserId = NULL`), so this wants a filtered/partial unique index.
- **Performance indexes** — now has a concrete target: the columns paging actually sorts and filters on (`Paychecks.ReceivedDate`, `Paychecks.IsClosed`, `Expenses.ExpenseDate`). Add them because the query plan needs them, not speculatively.

## 7. Memory Cache

Nothing exists yet — no `AddMemoryCache`, no `IMemoryCache`, no Redis anywhere in the code or `.csproj`.

Cache: System Categories.
Do NOT cache: Paychecks, Expenses.
Future: replace MemoryCache with Redis.

> **[unverified] Prerequisite:** `GetCategoriesAsync` reportedly filters `c.UserId == _currentUserService.UserId`, while system categories are seeded with `UserId == null` — which would mean system categories are never returned at all. If true, this query must be fixed before caching it, or you will be caching an empty set. Confirm before starting.

## 8. Swagger Improvements

- XML Comments
- Response Types (`[ProducesResponseType]`)
- Better Endpoint Documentation

---

# Frontend Phase

Not started — no `package.json` or frontend directory exists.

Build React application consuming the APIs.

Pages: Login, Dashboard, Paychecks, Expenses, Categories.

Need: JWT Authentication, Protected Routes, API Layer, Validation, Error Handling, Loading States.

---

# Production Phase

- **Unit Tests** — see Remaining Backend Task #3. The project exists and is broken; this is not greenfield work.
- **Integration Tests** — not started.
- **Docker** — not started; no `Dockerfile` or `docker-compose` present.
- **GitHub Actions** — not started. Note: a `.github/` folder exists at `ExpenseTracker.Api/.github/` containing only `copilot-instructions.md`. **GitHub only reads `.github/` from the repository root** (`FlexiBudgetTracker/`), so that file is currently ignored, and workflows placed there would never run. Move it to the repo root.
- **Azure / AWS Deployment** — not started.
- **Redis** — not started.

---

# Known Repo Gotchas

- **Config placed in `ExpenseTracker.Api/` is not read.** The repository root is `FlexiBudgetTracker/`, but the code lives one level down in `ExpenseTracker.Api/`. Tools that read config from the repo root therefore miss anything placed in the project folder. This has already bitten `ExpenseTracker.Api/.github/copilot-instructions.md` and `ExpenseTracker.Api/.claude/settings.json` (whose `ask` rules never fire). Check placement for anything config-shaped.
- **DTO namespaces do not mirror folders.** `PaycheckResponse`, `ExpenseResponse`, and `PagedResponse<T>` all sit in namespace `ExpenseTracker.Api.Dtos` despite living under `Dtos/ResponseDtos/`.
- **`Search` case-insensitivity comes from the database, not the code.** `Description.Contains(...)` becomes SQL `LIKE`; SQL Server's default collation is case-insensitive. On a case-sensitive collation these filters would quietly start missing rows.
- **`MSB3021 ... file in use` on build means the API is already running** and holding the exe — it is not a code error. Stop the running instance or probe the port instead.
- **`appsettings.json` has live-looking secrets committed** — SQL `sa` credentials and a plaintext JWT signing key. Replace with per-environment secrets before any deployment work.

---

# Design Principles

- Business logic belongs in Services.
- Validation belongs in FluentValidation.
- Logging:
  - Middleware → Request Lifecycle
  - Services → Business Events
  - Global Exception Handler → Unexpected Exceptions
- Keep IQueryable until final execution.
- Avoid unnecessary patterns such as Repository, CQRS, MediatR, or AutoMapper unless the project grows enough to justify them.

---

# Learning Goal

The objective is not just to finish the project.

Every implementation should be understandable and explainable:

- Why it exists
- What problem it solves
- Alternative approaches
- Trade-offs
- Industry best practices

The project is intended to prepare for .NET backend interviews at approximately the 4–5 years experience level while building a production-style portfolio project.
