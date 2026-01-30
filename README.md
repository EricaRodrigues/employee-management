# Employee Management – Backend

Backend developed in .NET 8 as part of a technical challenge, focusing on best practices, clear business rules, and clean architecture.

The application manages employees of a fictional company, enforcing role hierarchy, permissions, and domain validations.

This API is consumed by a React frontend application developed as part of the same challenge.

---

## Technologies

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Docker / Docker Compose
- Serilog (logs)
- xUnit, Moq e FluentAssertions
- Swagger (OpenAPI)

---

## Architecture

The project follows a layered architecture, inspired by Clean Architecture:

API → Application → Domain → Infrastructure

- **API**: Controllers, JWT authentication and exception middleware
- **Application**: Business rules, validations and services
- **Domain**: Entities and enums
- **Infrastructure**: EF Core, database and repositories

The main idea was to keep thin controllers and centralize business rules in the service layer, making the code easier to maintain and test.

The application is containerized to simplify local setup and ensure environment consistency.

---

## Business Rules

- An employee must have:
    - First name, last name, email and document number (unique)
    - Must be at least 18 years old
    - Can have multiple phone numbers
    - Optional manager (another employee)

### Role hierarchy
- Employee
- Leader
- Director

Main rules:
- A user cannot create or update an employee with a higher role than their own
- A user cannot change their own role
- Deletion respects hierarchy:
    - Employee cannot delete anyone
    - Leader can delete only Employees
    - Director can delete any user (except themselves)

---

## Features

- JWT authentication
- Full employee CRUD
- Business validations centralized in the Service layer
- Structured logs (creation, update, deletion and errors)
- Unit tests covering the main scenarios

---

## Authentication

Login returns a JWT containing:
- EmployeeId
- Role

Permission rules are not handled in controllers, but enforced in the service layer, avoiding duplication and keeping business logic centralized.

---

## Seed

To simplify local testing, a **default admin user is created via migration**:

```bash
Email: admin@company.com
Password: admin123
Role: Director
```

> This seed exists only for testing and evaluation purposes.

---

## How to Run

From the `backend` folder:

Start the database:
```bash
docker-compose up -d
```

Run the API:

```bash
dotnet run --project src/EmployeeManagement.API
```

Swagger:
```bash
/swagger
```

---

## Tests

Unit tests cover:
- Valid creation
- Underage validation
- Duplicate document
- Non-existent manager
- Role hierarchy rules
- Update and delete scenarios
- Authentication

To run unit tests:
```bash
dotnet test
```

---

# Employee Management – Frontend

Frontend application for the Employee Management system, developed as part of a technical challenge.
The app consumes a .NET 8 REST API and focuses on clean UI, proper validations, and role-based access.
---
## Tech Stack
- React
- TypeScript
- Vite
- Tailwind CSS
- React Router
- JWT Authentication
---
## Features
- JWT login and protected routes
- Role-based access (Employee, Leader, Director)
- Employee list with create, edit and delete
- Manager selection (any employee can be a manager)
- Client-side form validation
- Dynamic phone fields (add / remove)
- Clear error handling and feedback

---

## Validation & Permissions
- Required fields are validated on the frontend for better UX
- Email and phone formats are validated before submit
- Age and permission rules are enforced on the backend
- UI also hides actions the user cannot perform

---

## Running the Project

From the `frontend` folder:

```bash
npm install
npm run dev
```

---

## App runs on:

```bash
http://localhost:5173
```

---

## Notes
- Backend remains the source of truth for all business rules
- Frontend focuses on usability and clear interaction
- UI structure follows patterns used in real production projects

---