# Middleware Implementation – UserManagementAPI
## Microsoft Copilot Contributions & Test Results

---

## Middleware Pipeline Order

```
Request ──►  [1] GlobalExceptionMiddleware   (catches all unhandled exceptions)
         ──►  [2] Swagger UI                  (public, no auth required)
         ──►  [3] TokenAuthenticationMiddleware (validates Bearer token)
         ──►  [4] RequestLoggingMiddleware    (logs method, path, status, duration)
         ──►  [5] Controllers / Endpoints
Response ◄──  (flows back through each layer in reverse)
```

**Why this order?**
- Error handling wraps everything — if any layer throws, we return clean JSON.
- Swagger is excluded from auth so developers can test via the UI.
- Authentication runs before logging — unauthenticated noise is still logged for auditing.
- Logging runs last before controllers so it captures the final resolved path.

---

## Middleware 1 — Request Logging (`RequestLoggingMiddleware.cs`)

### Copilot Prompt
> "Generate middleware to log HTTP requests and responses in ASP.NET Core."

### What Copilot Suggested
- Use `System.Diagnostics.Stopwatch` to measure request duration
- Log the request **before** calling `_next` and the response **after**
- Include `RemoteIpAddress` for audit trail
- Use structured log message templates (`{Method}`, `{Path}`) instead of string interpolation for performance

### What Gets Logged
```
[REQUEST]  POST /api/users | IP: ::1
[RESPONSE] POST /api/users | Status: 201 | Duration: 12ms
```

### File
`Middleware/RequestLoggingMiddleware.cs`

---

## Middleware 2 — Error Handling (`GlobalExceptionMiddleware.cs`)

### Copilot Prompt
> "Create middleware that catches unhandled exceptions and returns consistent error responses in JSON format."

### What Copilot Suggested
- Place this middleware **first** in `Program.cs` so it wraps the entire pipeline
- Always set `Content-Type: application/json` before writing the response body
- Use a consistent envelope: `{ "error", "statusCode", "detail" }`
- Only include `detail` (exception message) in Development — hide in Production
- Inject `IHostEnvironment` instead of reading env vars directly

### Response Format
```json
{
  "error": "Internal server error.",
  "statusCode": 500,
  "detail": "Object reference not set..."  // Development only
}
```

### File
`Middleware/GlobalExceptionMiddleware.cs`

---

## Middleware 3 — Token Authentication (`TokenAuthenticationMiddleware.cs`)

### Copilot Prompt
> "Write middleware that validates bearer tokens from incoming requests in ASP.NET Core
> and returns 401 Unauthorized for invalid tokens."

### What Copilot Suggested
- Read the token from the `Authorization: Bearer <token>` header
- Exempt Swagger UI paths (`/swagger`, `/favicon.ico`) from auth checks
- Load valid tokens from `appsettings.json` (never hardcode secrets in source)
- Return `401 Unauthorized` with a JSON body — not a plain text response
- Store the resolved user identity in `HttpContext.Items` for downstream access
- In a production system, replace the token list with JWT validation

### Valid Test Tokens (configured in `appsettings.json`)
| Token               | Identity          |
|---------------------|-------------------|
| `hr-secret-token`   | HR Department     |
| `it-secret-token`   | IT Department     |
| `admin-token-2024`  | Administrator     |

### 401 Response Format
```json
{
  "error": "Unauthorized.",
  "statusCode": 401,
  "message": "Invalid or expired token."
}
```

### File
`Middleware/TokenAuthenticationMiddleware.cs`

---

## Test Results

### Logging Middleware

| Request                        | Log Output                                              |
|--------------------------------|---------------------------------------------------------|
| GET /api/users (valid token)   | `[REQUEST] GET /api/users` + `[RESPONSE] 200 | 5ms`    |
| POST /api/users (valid token)  | `[REQUEST] POST /api/users` + `[RESPONSE] 201 | 14ms`  |
| GET /api/users/99 (valid)      | `[REQUEST] GET /api/users/99` + `[RESPONSE] 404 | 3ms` |

### Authentication Middleware

| Scenario                            | Status | Response                              |
|-------------------------------------|--------|---------------------------------------|
| No Authorization header             | 401    | "Authorization header missing"        |
| `Authorization: Bearer wrong-token` | 401    | "Invalid or expired token."           |
| `Authorization: Bearer hr-secret-token` | 200 | User data returned                 |
| `Authorization: Bearer admin-token-2024` | 200 | User data returned               |
| Request to `/swagger`               | 200    | Swagger UI loads (no token needed)    |

### Error Handling Middleware

| Scenario                        | Status | Response                                         |
|---------------------------------|--------|--------------------------------------------------|
| Valid request                   | —      | Passes through, no error                         |
| Simulated unhandled exception   | 500    | `{ "error": "Internal server error." }`          |
| Dev environment exception       | 500    | Includes `detail` field with exception message   |

### Edge Cases

| Scenario                              | Status | Notes                                  |
|---------------------------------------|--------|----------------------------------------|
| `Authorization: bearer TOKEN` (lowercase) | 200 | Case-insensitive header check works  |
| `Authorization: Token abc`            | 401    | Non-Bearer scheme rejected            |
| Empty string token `Bearer `          | 401    | Whitespace-only token rejected        |

---

## How Copilot Streamlined Middleware Development

1. **Logging** — Copilot generated the Stopwatch pattern and structured log templates in one prompt, saving manual research into ASP.NET Core logging best practices.

2. **Error handling** — Copilot identified the need to set `Content-Type` before writing the body (a common mistake), and suggested the `IHostEnvironment` injection over env variable checks.

3. **Authentication** — Copilot recommended reading tokens from configuration (not hardcoded), suggested the `HttpContext.Items` pattern for passing identity downstream, and provided the Swagger exclusion logic.

4. **Pipeline ordering** — Copilot explained the "outermost-first" rule: error handling must wrap auth and logging, and auth must run before logging to avoid logging noise from bots/scanners hitting unauthenticated endpoints.

5. **Swagger security** — Copilot suggested adding `AddSecurityDefinition` and `AddSecurityRequirement` in `Program.cs` so the Swagger UI shows an Authorize button for testing tokens directly in the browser.
