# Student API (.NET Web API)

A clean, well-structured ASP.NET Core Web API built as a learning-focused backend project.
This project demonstrates professional backend practices including layered architecture,
JWT authentication, validation, logging, and proper API documentation.

This API manages student data and serves as a foundation for learning real-world backend
development concepts.

---

##  Features Implemented

- RESTful API using ASP.NET Core
- CRUD operations for Student entity
- Entity Framework Core with MySQL
- Database migrations
- Clean layered architecture
- DTO usage to protect domain models
- Global exception handling (middleware)
- Structured logging using `ILogger<T>`
- Input validation using FluentValidation
- JWT-based authentication
- Role-based authorization
- Swagger UI with JWT support
- Professional documentation structure

---

##  Project Architecture

The project follows a layered architecture pattern:

- **Controllers**  
  Handle HTTP requests and responses only

- **Services**  
  Contain business logic and application rules

- **DTOs (Data Transfer Objects)**  
  Control input/output shape and prevent over-posting

- **Models (Entities)**  
  Represent database tables

- **Data**  
  Database context and EF Core configuration

- **Middleware**  
  Cross-cutting concerns (exception handling, logging, auth)

This separation improves maintainability, testability, and scalability.

---

##  Authentication & Authorization

- JWT (JSON Web Token) authentication
- Tokens are generated during login
- Tokens are stateless and sent via `Authorization: Bearer <token>`
- Role-based access control using `[Authorize(Roles = "...")]`
- Swagger UI configured to accept JWT tokens

---

##  Validation & Error Handling

- FluentValidation ensures input is validated at the API boundary
- Global exception middleware provides consistent error responses
- Controllers remain clean and minimal

---

##  Tech Stack

- ASP.NET Core Web API
- .NET 9
- Entity Framework Core
- MySQL
- FluentValidation
- Swagger / OpenAPI
- JWT Authentication

---

##  Running the Project

1. Configure your database connection in `appsettings.json`
2. Apply migrations:
   ```bash
   dotnet ef database update
