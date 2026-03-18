# Microsoft Copilot Contributions – UserManagementAPI

## Project Overview
**TechHive Solutions – User Management API**  
Built with ASP.NET Core 9 | In-memory data store | Swagger/OpenAPI

---

## How Microsoft Copilot Assisted This Project

### 1. Project Scaffolding (`Program.cs`)
**Prompt used:** *"Scaffold a Program.cs for an ASP.NET Core 9 Web API with Swagger, dependency injection, and controller routing."*

Copilot generated:
- `builder.Services.AddControllers()` and `AddEndpointsApiExplorer()` boilerplate
- Swagger/OpenAPI configuration with `SwaggerDoc` metadata
- Conditional Swagger UI enabled only in Development environment
- Suggestion to serve Swagger UI at root via `RoutePrefix = string.Empty`

---

### 2. Model & DTO Design (`User.cs`, `UserDtos.cs`)
**Prompt used:** *"Generate a User model and separate Create/Update DTOs with data annotations for an ASP.NET Core API."*

Copilot generated:
- `User` entity with `Id`, `FirstName`, `LastName`, `Email`, `Department`, `Role`, `CreatedAt`, `IsActive`
- `CreateUserDto` with `[Required]`, `[MaxLength]`, and `[EmailAddress]` validation attributes
- `UpdateUserDto` with all nullable fields for partial update support
- Recommended keeping DTOs separate from the domain model to avoid over-posting vulnerabilities

---

### 3. Service Layer (`UserService.cs`)
**Prompt used:** *"Create an in-memory UserService implementing an IUserService interface with CRUD methods."*

Copilot generated:
- Interface definition (`IUserService`) with clean method signatures
- Singleton-safe in-memory `List<User>` with auto-incrementing ID counter
- Seed data with three sample users for immediate testing
- **Null-coalescing update pattern** for `UpdateUser`:
  ```csharp
  user.FirstName = dto.FirstName ?? user.FirstName;
  ```
  This allows partial updates without requiring all fields.

---

### 4. Controller CRUD Endpoints (`UsersController.cs`)
**Prompt used:** *"Generate a full CRUD REST controller for a User resource in ASP.NET Core with proper HTTP status codes and ProducesResponseType attributes."*

Copilot generated:
- `GET /api/users` – returns `200 OK` with full list
- `GET /api/users/{id}` – returns `200 OK` or `404 Not Found`
- `POST /api/users` – returns `201 Created` with `Location` header via `CreatedAtAction`
- `PUT /api/users/{id}` – returns `200 OK` or `404 Not Found`
- `DELETE /api/users/{id}` – returns `204 No Content` or `404 Not Found`
- `[ProducesResponseType]` attributes on every action for Swagger documentation accuracy
- Suggested using `[Route("api/[controller]")]` with `[ApiController]` for automatic model validation

---

### 5. Code Quality Improvements
Copilot also suggested:
- Using `IActionResult` return types for flexibility over concrete types
- Adding XML `<summary>` doc comments for Swagger to pick up automatically
- Registering `UserService` as `Singleton` (not `Scoped`) since it holds in-memory state
- Structuring the solution with `Models/`, `Services/`, and `Controllers/` folders for separation of concerns

---

## Postman Test Results

| Endpoint              | Method | Status | Notes                          |
|-----------------------|--------|--------|--------------------------------|
| `/api/users`          | GET    | 200    | Returns seeded users           |
| `/api/users/1`        | GET    | 200    | Returns Alice Johnson          |
| `/api/users/999`      | GET    | 404    | Returns not found message      |
| `/api/users`          | POST   | 201    | Creates user, returns Location |
| `/api/users/1`        | PUT    | 200    | Partial update works           |
| `/api/users/2`        | DELETE | 204    | Removes Bob Smith              |
| `/api/users/2`        | GET    | 404    | Confirms deletion              |

---

## How to Run

```bash
cd UserManagementAPI
dotnet run
```

Navigate to `http://localhost:5000` (or `https://localhost:7000`) — Swagger UI loads automatically.
