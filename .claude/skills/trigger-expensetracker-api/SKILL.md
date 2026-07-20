---
name: trigger-expensetracker-api
description: Call ExpenseTracker.Api endpoints live to seed or inspect test data. Use when the user wants to hit an API endpoint, create test/sample/seed data, try out a controller action, or verify an endpoint works against the running app. Triggers on "trigger the api", "call the endpoint", "create some test paychecks/expenses/categories", "add data so I can test", "seed data".
---

# Trigger ExpenseTracker API

Drives the locally running ExpenseTracker.Api to create test data or exercise an endpoint.

## Step 1 — ALWAYS ask first (do not skip)

Before doing anything else, use **AskUserQuestion** to ask which **controller** and which **endpoint** to hit. Never assume or guess, even if the conversation seems to imply one — ask every time this skill runs.

Offer the current controllers/actions as options:

| Controller | Endpoints |
|---|---|
| `Paychecks` | `CreatePaycheck` (POST), `GetPaychecks` (GET), `GetPaycheck` (GET), `ClosePaycheck` (POST), `GetSummary` (GET) |
| `Expenses` | `CreateExpense` (POST), `expenses` (GET), `GetExpense` (GET), `UpdateExpense` (PUT), `DeleteExpense` (DELETE) |
| `Categories` | `GetCategories` (GET) |
| `Login` | `register` (POST), `login` (POST) |

Re-read `Controllers/` to confirm before offering — the list above goes stale as endpoints are added.

Also ask how many records to create, if the endpoint is a create endpoint and the user didn't say.

## Step 2 — Make sure the API is up

Check for a running instance before starting one:

```bash
netstat -ano | grep -E ":5041|:7226"
```

If nothing is listening, start it in the background:

```bash
cd ExpenseTracker.Api && dotnet run --project ExpenseTracker.Api.csproj
```

If `dotnet build` fails with `MSB3021 ... being used by another process`, that is not a code error — an instance is already running and holding the exe. Probe the port instead of rebuilding.

## Step 3 — Get a token

Every endpoint except `Login/*` needs a JWT. Tokens expire after 15 minutes (`Jwt:DurationInMinutes`), so fetch a fresh one at the start of each batch rather than reusing one from earlier in the session.

```bash
TOKEN=$(curl -sk -X POST "https://localhost:7226/api/Login/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"testuser@example.com","password":"TestPass123!"}' \
  | python -c "import sys,json;print(json.load(sys.stdin)['data']['token'])")
```

If login 404s with "Invalid email or password", register first via `POST /api/Login/register` with `{firstName, lastName, email, password}`.

## Step 4 — Call the endpoint

```bash
curl -sk -X POST "https://localhost:7226/api/Paychecks/CreatePaycheck" \
  -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" \
  -d '{"description":"January Salary","amount":3200.00,"receivedDate":"2026-01-31T00:00:00"}'
```

## Gotchas that will waste your time

- **Always use `https://localhost:7226` with `curl -k`.** Port 5041 (HTTP) returns a bare `307` redirect because of `UseHttpsRedirection()`, and the body comes back empty — it looks like a failure but isn't. `-k` is needed for the self-signed dev cert.
- **Routes are `/api/<Controller>/<ActionName>`, not REST-conventional.** The action name is spelled out in the `[HttpGet("...")]` / `[HttpPost("...")]` attribute — read the controller rather than guessing (`GetPaychecks`, not `GET /api/Paychecks`). Note `Expenses` uses a lowercase `expenses` for its list action.
- **`ClosePaycheck` takes query params, not a body**: `?paycheckId=2&toggle=true`.
- Responses are wrapped in the `ApiResponse<T>` envelope — the payload is under `data`, not at the top level.
- Business rules will reject some seed data on purpose: expenses can't exceed the paycheck's remaining balance, and can't be added to a closed paycheck. If a create 400s, read the message before assuming a bug.

## Making the data useful

Vary the seed data so it actually exercises filtering, sorting, and paging — identical rows prove nothing. For paychecks that means a spread of `receivedDate`s, some repeated `amount`s (to shake out tie-breaking), a mix of `isClosed` via `ClosePaycheck`, and descriptions that partially overlap so `Search` has something to match.

Create more rows than the default `PageSize` of 10 if paging is what's being tested.

## Reporting back

Show the user what was created — id, description, and the fields they'll filter or sort on — not just a success count. If a call failed, show the status and the message rather than glossing over it.
