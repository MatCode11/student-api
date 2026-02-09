# FirstAPI

A professional .NET Web API project demonstrating modern backend development practices, clean architecture, and secure authentication. Built as a learning project to explore real-world backend architecture and best practices.

## 📋 Project Overview

FirstAPI is a RESTful Web API built with ASP.NET Core that provides endpoints for managing students and teachers with JWT-based authentication. The project follows clean architecture principles, implementing separation of concerns through controllers, services, DTOs, and data models. It serves as both a learning resource and a portfolio piece showcasing industry-standard backend development practices.

### Key Features

- 🔐 **JWT Authentication** - Secure user registration and login with token-based authentication
- 📚 **RESTful API** - Full CRUD operations for Students and Teachers
- ✅ **Input Validation** - FluentValidation for robust data validation
- 🛡️ **Error Handling** - Global exception middleware for consistent error responses
- 📝 **Structured Logging** - Serilog for comprehensive logging to console and files
- 🗄️ **Database Migrations** - Entity Framework Core with MySQL/MariaDB
- 🏥 **Health Checks** - Database health monitoring endpoint
- 📖 **API Documentation** - Swagger/OpenAPI integration for interactive API exploration

## 🛠️ Tech Stack

- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core Web API** - Web API framework
- **Entity Framework Core 9.0** - ORM for database operations
- **Pomelo.EntityFrameworkCore.MySql** - MySQL/MariaDB provider
- **JWT Bearer Authentication** - Token-based authentication
- **FluentValidation** - Input validation library
- **Serilog** - Structured logging framework
- **Swashbuckle (Swagger)** - API documentation
- **DotNetEnv** - Environment variable management

## 📁 Project Structure

```
FirstAPI/
├── Controllers/          # API endpoints (Auth, Students, Teachers)
├── Services/            # Business logic layer
│   ├── Auth/           # Authentication service
│   ├── Students/       # Student service
│   └── Teachers/       # Teacher service
├── DTOs/               # Data Transfer Objects
│   ├── Auth/          # Authentication DTOs
│   ├── Students/      # Student DTOs
│   └── Teachers/      # Teacher DTOs
├── Models/            # Entity models (Student, Teacher, User)
├── Data/              # Database context and configuration
├── Validators/        # FluentValidation validators
├── Middleware/        # Custom middleware (Exception handling)
├── Migrations/        # EF Core database migrations
└── Program.cs         # Application entry point and configuration
```

### Architecture Overview

The project follows a **layered architecture** pattern:

- **Controllers** - Handle HTTP requests/responses, minimal business logic
- **Services** - Contain business logic and orchestrate data operations
- **DTOs** - Data transfer objects for API contracts
- **Models** - Entity models representing database tables
- **Validators** - Input validation rules using FluentValidation
- **Middleware** - Cross-cutting concerns (exception handling, logging)

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- MySQL 8.0+ or MariaDB 10.3+
- A code editor (Visual Studio, VS Code, or Rider)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd csharpAPI/FirstAPI
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Create a `.env` file** in the `FirstAPI` directory:
   ```env
   CONNECTION_STRING=Server=localhost;Database=FirstAPI;User=root;Password=your_password;Port=3306
   JWT_KEY=your-super-secret-key-minimum-32-characters-long-for-security
   JWT_ISSUER=FirstAPI
   JWT_AUDIENCE=FirstAPIUsers
   JWT_EXPIRES_MINUTES=60
   ```

   **Important:** 
   - Replace `your_password` with your MySQL root password
   - Replace `your-super-secret-key...` with a secure key (minimum 32 characters)
   - Never commit the `.env` file to version control

4. **Create the database**
   ```sql
   CREATE DATABASE FirstAPI;
   ```

5. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

The API will be available at:
- **HTTP:** `http://localhost:5000`
- **HTTPS:** `https://localhost:5001`
- **Swagger UI:** `https://localhost:5001/swagger` (Development mode only)

## 🗄️ Database & Migrations

### Database Configuration

The project uses **Entity Framework Core** with **MySQL/MariaDB**. The connection string is configured via environment variables or `appsettings.json`.

### Running Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Apply migrations to database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Database Schema

The application includes three main entities:
- **Users** - Authentication and user management
- **Students** - Student records with personal information
- **Teachers** - Teacher records with personal information

## 🔐 Authentication (JWT)

### Overview

The API uses **JSON Web Tokens (JWT)** for authentication. Users must register and login to obtain a JWT token, which is then used to access protected endpoints.

### Authentication Flow

1. **Register** - Create a new user account
   ```
   POST /api/auth/register
   ```
   - Validates email uniqueness
   - Enforces strong password requirements
   - Hashes password using BCrypt
   - Returns user information

2. **Login** - Authenticate and receive JWT token
   ```
   POST /api/auth/login
   ```
   - Validates credentials
   - Returns JWT token with expiration

3. **Protected Endpoints** - Include token in Authorization header
   ```
   Authorization: Bearer <your-jwt-token>
   ```

### Password Requirements

- Minimum 8 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one number (0-9)
- At least one special character (!@#$%^&*(),.?":{}|<>)

### JWT Configuration

JWT settings can be configured via environment variables:
- `JWT_KEY` - Secret key for signing tokens (required, min 32 chars)
- `JWT_ISSUER` - Token issuer (default: "FirstAPI")
- `JWT_AUDIENCE` - Token audience (default: "FirstAPIUsers")
- `JWT_EXPIRES_MINUTES` - Token expiration time (default: 60 minutes)

## 📡 API Endpoints

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Login and get JWT token | No |

### Student Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/students` | Get all students | Yes |
| GET | `/api/students/{id}` | Get student by ID | Yes |
| POST | `/api/students` | Create new student | Yes |
| PUT | `/api/students/{id}` | Update student | Yes |
| DELETE | `/api/students/{id}` | Delete student | Yes |

### Teacher Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/teacher` | Get all teachers | Yes |
| GET | `/api/teacher/{id}` | Get teacher by ID | Yes |
| POST | `/api/teacher` | Create new teacher | Yes |
| PUT | `/api/teacher/{id}` | Update teacher | Yes |
| DELETE | `/api/teacher/{id}` | Delete teacher | Yes |

### Health Check

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/health` | Check API and database health | No |

## 📝 Example API Usage

### 1. Register a New User

```bash
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "id": 1,
  "email": "user@example.com",
  "role": "User",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Login

```bash
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "id": 1,
  "email": "user@example.com",
  "role": "User",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 3. Get All Students (Protected)

```bash
GET https://localhost:5001/api/students
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
[
  {
    "id": 1,
    "fullName": "John Doe",
    "email": "john@example.com",
    "age": 20
  }
]
```

### 4. Create a Student (Protected)

```bash
POST https://localhost:5001/api/students
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@example.com",
  "age": 22
}
```

## 🛡️ Error Handling & Middleware

### Exception Middleware

The application includes a global exception middleware (`ExceptionMiddleware`) that:
- Catches all unhandled exceptions
- Logs errors using Serilog
- Returns consistent error responses in JSON format
- Prevents sensitive error details from being exposed to clients

### Error Response Format

```json
{
  "statusCode": 500,
  "message": "An unexpected error occurred."
}
```

### HTTP Status Codes

- `200 OK` - Successful request
- `201 Created` - Resource created successfully
- `204 No Content` - Successful request with no content
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required or failed
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource conflict (e.g., duplicate email)
- `500 Internal Server Error` - Server error

## 📊 Logging

### Serilog Configuration

The project uses **Serilog** for structured logging with the following configuration:

- **Console Sink** - Logs to console output
- **File Sink** - Logs to `Logs/log-{date}.txt` with daily rolling
- **Request Logging** - Automatic HTTP request logging

### Log Levels

- **Information** - General application flow
- **Warning** - Warning messages
- **Error** - Error messages with stack traces

### Log File Location

Logs are written to: `FirstAPI/Logs/log-YYYYMMDD.txt`

## ✅ Validation

### FluentValidation

The project uses **FluentValidation** for input validation. Validators are automatically registered and executed before controller actions.

### Validation Rules

**Student Creation:**
- First name and last name are required
- Email must be valid format
- Age must be between 18 and 100

**User Registration:**
- Email must be valid format and unique
- Password must meet strength requirements (see Authentication section)

### Validation Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 8 characters"]
  }
}
```

## 🔒 Security Features

- ✅ **JWT Authentication** - Secure token-based authentication
- ✅ **Password Hashing** - BCrypt password hashing
- ✅ **HTTPS Enforcement** - Automatic HTTPS redirection
- ✅ **HSTS** - HTTP Strict Transport Security (production)
- ✅ **Input Validation** - Prevents malicious input
- ✅ **Email Uniqueness** - Prevents duplicate registrations
- ✅ **Environment Variables** - Sensitive data stored securely
- ✅ **CORS Configuration** - Configurable cross-origin resource sharing

## 🚧 Future Improvements

This is a learning project, and the following enhancements could be implemented:

- [ ] **Role-Based Authorization** - Implement role-based access control (Admin, User, etc.)
- [ ] **Rate Limiting** - Prevent API abuse with rate limiting
- [ ] **Unit Tests** - Add comprehensive unit tests for services and controllers
- [ ] **Integration Tests** - Add API integration tests
- [ ] **API Versioning** - Implement API versioning strategy
- [ ] **Pagination** - Add pagination for list endpoints
- [ ] **Filtering & Sorting** - Add query parameters for filtering and sorting
- [ ] **Refresh Tokens** - Implement token refresh mechanism
- [ ] **Email Verification** - Add email verification for new registrations
- [ ] **Password Reset** - Implement password reset functionality
- [ ] **Audit Logging** - Track changes to entities
- [ ] **Caching** - Add response caching for performance
- [ ] **Docker Support** - Containerize the application

## 📚 Learning Resources

This project demonstrates:
- Clean architecture and separation of concerns
- Dependency injection patterns
- Repository/Service pattern
- DTO pattern for API contracts
- Middleware implementation
- JWT authentication flow
- Entity Framework Core migrations
- Input validation best practices
- Error handling strategies
- Structured logging

## ⚠️ Disclaimer

**This is a learning project** built for educational purposes and portfolio demonstration. While it implements many industry best practices, it may not be production-ready for all use cases. Some considerations:

- Security configurations may need adjustment for production environments
- Error messages are simplified for learning purposes
- Some features are intentionally basic to focus on core concepts
- Performance optimizations may be needed for high-traffic scenarios

## 📄 License

This project is open source and available for educational purposes.

## 👤 Author

Built as a learning project to explore modern .NET backend development practices.

---

**Note:** For detailed setup instructions, troubleshooting, and additional documentation, see:
- `SETUP_INSTRUCTIONS.md` - Detailed setup guide
- `PROJECT_ANALYSIS.md` - Project analysis and recommendations
- `SECURITY_IMPLEMENTATION_SUMMARY.md` - Security implementation details
