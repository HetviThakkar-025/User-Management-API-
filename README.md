# UserManagementAPI

A RESTful User Management API built with **ASP.NET Core 9** for TechHive Solutions.  
Supports full CRUD operations for managing internal user records across HR and IT departments.

## Tech Stack
- ASP.NET Core 9 Web API
- Swagger / OpenAPI (Swashbuckle)
- In-memory data store (no database required)
- Dependency Injection

## Endpoints

| Method | Endpoint           | Description              |
|--------|--------------------|--------------------------|
| GET    | `/api/users`       | Get all users            |
| GET    | `/api/users/{id}`  | Get user by ID           |
| POST   | `/api/users`       | Create a new user        |
| PUT    | `/api/users/{id}`  | Update an existing user  |
| DELETE | `/api/users/{id}`  | Delete a user            |

## Running the API

```bash
dotnet run
```

Swagger UI is available at the root URL in Development mode.

## Sample POST Body

```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@techhive.com",
  "department": "Engineering",
  "role": "Backend Developer"
}
```

## Copilot Contributions
See [CopilotContributions.md](./CopilotContributions.md) for a detailed breakdown of how Microsoft Copilot assisted in building this API.
