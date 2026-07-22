# FlexiBudgetTracker — Project Context for Claude

## Project Overview

This is **not** a simple expense tracker. It's a **paycheck planning and budgeting application**, built as a portfolio project to demonstrate enterprise-level .NET development practices.

The core idea: users create *future* paychecks and plan how that money will be allocated before it's actually spent. As expenses are paid, they're recorded against the paycheck, and the remaining balance updates automatically. Once all expenses are settled, the paycheck can be closed.

The goal throughout is production-ready architecture and coding standards — not just making the app work.

The solution (`ExpenseTracker.sln`) lives at the repo root, with the API under `src\ExpenseTracker.Api\` and the xUnit test project under `tests\ExpenseTracker.Api.Tests\`. It is a backend-only ASP.NET Core Web API; there is no frontend yet.

## Technology Stack

### Backend (implemented)
- ASP.NET Core 8 Web API (controller-based, not minimal APIs)
- C#, nullable reference types + implicit usings enabled
- Entity Framework Core 8 (Code First) + SQL Server
- FluentValidation
- JWT Bearer authentication
- Swagger / OpenAPI (Swashbuckle)
- xUnit + EF Core InMemory (test project)

### Frontend (planned, not yet built)
- React, consuming the ASP.NET Core APIs
- JWT authentication
- Dashboard, paycheck management, expense management

## Project Goals

This codebase is meant to demonstrate:
- Clean architecture principles (without unnecessary complexity)
- SOLID principles where appropriate
- Dependency injection
- Middleware
- Logging
- Exception handling
- Authentication & authorization
- DTO usage
- Entity Framework Core + LINQ
- Validation and business-rule enforcement
- Enterprise coding practices generally

**When suggesting implementations, prefer maintainability and readability over shortcuts or clever code.**

## Repo Layout

Repo root holds `ExpenseTracker.sln`, `src\`, `tests\`, and `.github\workflows\ci.yml`. The API project lives in `src\ExpenseTracker.Api\`; the paths below are relative to it:

| Folder | Responsibility |
|---|---|
| `Controllers/` | Thin HTTP endpoints — delegate to services, wrap results in `ApiResponse<T>` |
| `Services/` | Business logic, EF Core queries, one implementation per interface |
| `Interface/` | Service contracts (`I*Service`) used for DI |
| `Models/` | EF Core entity classes (persistence model) |
| `Data/` | `ExpenseTrackerDbContext` |
| `Dtos/RequestDtos/` | Inbound request shapes (includes `QueryRequest/` for pagination-style queries) |
| `Dtos/ResponseDtos/` | Outbound response shapes (includes `ApiResponse<T>`, `QueryResponse/PagedResponse<T>`) |
| `Validators/` | FluentValidation `AbstractValidator<T>` classes, one per request DTO needing validation |
| `Filters/` | `ValidationFilter<T>` — generic action filter that runs FluentValidation and short-circuits on failure |
| `Exceptions/` | Custom exceptions (`NotFoundException`, `ConflictException`, `BusinessRuleException`) + `ExceptionHandling/GlobalExceptionHandler` |
| `Middleware/` | `RequestLoggingMiddleware` |
| `Migrations/` | EF Core Code First migrations |

The xUnit test project sits at the repo root under `tests\ExpenseTracker.Api.Tests\` (separate `.csproj`, references the API project, both included in `ExpenseTracker.sln`).

## Architecture

```
Controllers
    ↓
Services
    ↓
Entity Framework DbContext
    ↓
SQL Server
```

- **Business logic belongs in services, not controllers.** Controllers stay thin: parse the request, call a service, wrap the result.
- Supporting components: DTOs, service interfaces, middleware, FluentValidation, structured logging, `CurrentUserService`, JWT authentication.
- Controllers return a uniform envelope, `ApiResponse<T>` (`Dtos/ResponseDtos/ApiResponse.cs`) — `{ success, message, data }`. New endpoints should follow this shape.
- DI convention: every service is registered in `Program.cs` as `AddScoped<IXService, XService>()`.

## Authentication

- JWT-based. Current features: user registration, login, password hashing, JWT token generation.
- `ICurrentUserService` (`Services/CurrentUserService.cs`) retrieves the authenticated user's ID from JWT claims via `IHttpContextAccessor`.
- **All user-specific queries must filter by the authenticated user's ID** (via `ICurrentUserService`), never by a `UserId` accepted from the API request.

## Current Features

### Users
- Register
- Login

### Paychecks
A paycheck contains: Description, Amount, Expected/Received Date, IsClosed.

Operations: create paycheck, view paychecks, view paycheck details, close paycheck.

Business rules:
- Closed paychecks cannot be modified.
- Expenses cannot be added to closed paychecks.

### Expenses
Each expense belongs to one paycheck. Fields: Description, Amount, Expense Date, Category (optional).

Business rules:
- Expense amount cannot exceed the remaining paycheck balance.
- Expenses belong only to the authenticated user, through their paycheck.

```
Remaining Balance = Paycheck Amount − Total Expenses
```

### Categories
Categories are optional. If a category is deleted:
- Existing expenses remain.
- `CategoryId` becomes `NULL`.

Implemented via EF Core's `OnDelete(SetNull)`.

## Entity Framework

The project uses:
- Code First + Migrations
- Navigation properties
- Fluent API
- Async LINQ
- Projection into DTOs
- `Include()` where appropriate

**Never return EF entities directly from controllers — always return DTOs.**

## Validation

- Input validation uses FluentValidation (`Validators/`), wired up via `ValidationFilter<T>` and applied per-action with `[ServiceFilter(typeof(ValidationFilter<TRequest>))]`.
- Controllers should not contain validation logic.
- Business-rule validation belongs in the service layer, not in validators.

## Logging

- Logging is backed by **Serilog** (`Program.cs`: `UseSerilog(...)`), configured entirely from `appsettings.json`'s `Serilog` section — console sink plus a rolling daily file sink under `Logs/log-.txt`. `Microsoft.AspNetCore` and `Microsoft.EntityFrameworkCore` are overridden to `Warning` to cut noise.
- `RequestLoggingMiddleware` records: TraceId, request method, request path, UserId, response status, execution time.
- Use structured logging through `ILogger`.
- Avoid logging sensitive information.

## Request Pipeline (`Program.cs`)

DI registrations of note beyond what's covered above: `AddHttpContextAccessor()`, `Configure<JwtSettings>(...)` bound from the `Jwt` config section, `AddValidatorsFromAssemblyContaining<Program>()` (auto-registers every `AbstractValidator<T>` in `Validators/`), `AddScoped(typeof(ValidationFilter<>))` (open generic — no per-DTO registration needed), `AddExceptionHandler<GlobalExceptionHandler>()` + `AddProblemDetails()`.

Middleware order: `RequestLoggingMiddleware` → `UseExceptionHandler()` → (`UseSwagger`/`UseSwaggerUI` in Development) → `UseHttpsRedirection()` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()`.

## Exception Handling

A global exception-handling middleware (`Exceptions/ExceptionHandling/GlobalExceptionHandler`, implementing `IExceptionHandler`) converts exceptions into proper HTTP responses.

- Avoid wrapping every service method in try/catch unless: additional context needs to be logged, an exception should be translated into a different business exception, or recovery is possible.
- Otherwise, let exceptions bubble up to the global handler.

## Coding Standards

- Keep controllers thin.
- Put business logic in services.
- Use dependency injection.
- Prefer async methods (suffix with `Async`).
- Use DTOs instead of returning entities.
- Follow REST conventions.
- Write readable code over clever code.
- Keep methods focused on a single responsibility.
- Use meaningful naming.
- Prefer composition over duplication.

Naming conventions observed in the codebase:
- PascalCase for classes, methods, properties, public members.
- camelCase with underscore prefix for private fields (`_context`, `_currentUserService`).
- Interfaces prefixed with `I` (`IExpenseService`).
- Suffix conventions: `*Request`/`*Response` (DTOs), `*Service`/`I*Service`, `*Exception`, `*Validator`, `*Controller`.
- Namespaces mirror folder structure.

## Build, Run, Test

Run from the repo root:

```
dotnet build ExpenseTracker.sln
dotnet run --project src/ExpenseTracker.Api/ExpenseTracker.Api.csproj
dotnet test ExpenseTracker.sln
dotnet test --filter "FullyQualifiedName~PaycheckServiceTests.CreatePaycheckAsync_CreatesOpenPaycheck"   # single test
dotnet test --filter "FullyQualifiedName~PaycheckServiceTests"                                            # single class
```

- CI: `.github/workflows/ci.yml` runs `dotnet restore`/`build`/`test` on the solution for every push and PR to `main`.
- Launch profiles (`Properties/launchSettings.json`) serve Swagger UI at `http://localhost:5041` (HTTP) or `https://localhost:7226` (HTTPS), with `ASPNETCORE_ENVIRONMENT=Development`.
- EF Core migrations (run from the API project dir): `dotnet ef migrations add <Name>` / `dotnet ef database update`.
- Test project (`tests/ExpenseTracker.Api.Tests/`) uses `TestDbContextFactory.CreateContext()` for an EF Core InMemory context per test, and `FakeCurrentUserService` (settable `UserId`, default 1) + `NullLogger<T>.Instance` to construct services. Note: the InMemory provider can't emulate SQL Server's case-insensitive collation or FK cascade behavior — cover those with SQLite in-memory or Testcontainers instead.

## Configuration

- Connection string and JWT settings live in `appsettings.json`, with environment-specific overrides in `appsettings.Development.json`. Keys: `ConnectionStrings:DefaultConnection`, `Jwt:Key`/`Jwt:Issuer`/`Jwt:Audience`/`Jwt:DurationInMinutes`, plus `Serilog:*` for logging.
- `appsettings.json` currently has local-dev-looking values checked into source (SQL `sa` credentials, a plaintext JWT signing key). Treat these as placeholders to replace with real per-environment secrets, not as safe defaults to build on.

## Known Gotchas

- **DTO namespaces don't mirror their folders.** Despite the `Dtos/ResponseDtos/`, `Dtos/RequestDtos/QueryRequest/` structure, several DTOs (`PaycheckResponse`, `ExpenseResponse`, `PagedResponse<T>`) are declared directly in namespace `ExpenseTracker.Api.Dtos` rather than a namespace matching their folder. Don't assume folder path ⇒ namespace for these.
- **Pagination/filtering scaffolding exists but may be partially wired.** `Dtos/RequestDtos/QueryRequest/QueryRequest.cs` (base: `Page`, `PageSize`, `SortBy`, `Descending`), `PaycheckQueryRequest`, `ExpenseQueryRequest`, and `Dtos/ResponseDtos/QueryResponse/PagedResponse.cs` were originally added but left disconnected from any controller/service — check current usage before assuming a given list endpoint is or isn't paginated yet, since this is being wired up incrementally.
- `IPaycheckService.GetDashboardAsync()` exists in the service layer with no corresponding controller action — check `Controllers/` before assuming a service method is reachable over HTTP.

## Development Approach

When helping with this project:
1. Preserve the existing architecture.
2. Explain the reasoning behind architectural decisions.
3. Follow enterprise .NET practices.
4. Avoid unnecessary abstractions or overengineering.
5. If multiple approaches exist, explain the trade-offs and recommend the most maintainable option.

Do not rewrite the project architecture unless there is a strong technical reason. Build upon the existing design incrementally while maintaining consistency across the codebase.
