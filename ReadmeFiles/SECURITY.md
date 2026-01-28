# Security Guide for FirstAPI

This document explains the security concepts, practices, and considerations for the FirstAPI ASP.NET Core Web API project. Understanding security is crucial for building production-ready applications.

---

## Table of Contents

1. [Authentication vs Authorization](#authentication-vs-authorization)
2. [JWT Authentication Overview](#jwt-authentication-overview)
3. [How JWT Works (Token Flow)](#how-jwt-works-token-flow)
4. [What is Stored in the Token (Claims)](#what-is-stored-in-the-token-claims)
5. [Token Validation in ASP.NET Core](#token-validation-in-aspnet-core)
6. [Why HTTPS is Required](#why-https-is-required)
7. [Password Hashing](#password-hashing)
8. [Common API Security Risks](#common-api-security-risks)
9. [Why DTOs Help Security](#why-dtos-help-security)
10. [Why Validation Helps Security](#why-validation-helps-security)
11. [Logging and Security](#logging-and-security)
12. [Token Expiration and Refresh Tokens](#token-expiration-and-refresh-tokens)
13. [Role-Based Authorization](#role-based-authorization)
14. [Secrets Management](#secrets-management)
15. [Future Security Improvements](#future-security-improvements)

---

## Authentication vs Authorization

**Authentication** answers: **"Who are you?"**
- Verifies the identity of a user
- Confirms they are who they claim to be
- Example: Login with email and password

**Authorization** answers: **"What are you allowed to do?"**
- Determines what resources/actions a user can access
- Happens AFTER authentication
- Example: Only admins can delete users, regular users can only view

### Why This Matters

Think of it like a building:
- **Authentication** = Showing your ID at the front desk (proving you're a valid person)
- **Authorization** = Having a keycard that lets you into specific floors (determining what you can access)

In FirstAPI:
- Authentication happens in `AuthController` when users login/register
- Authorization would control which endpoints users can access based on their role

**Without proper separation**: An attacker who bypasses authentication could access everything. With proper separation, even if authentication is compromised, authorization limits the damage.

---

## JWT Authentication Overview

**JWT (JSON Web Token)** is a compact, URL-safe token format used to securely transmit information between parties.

### Why JWT?

1. **Stateless**: The server doesn't need to store session data
2. **Scalable**: Works across multiple servers without shared session storage
3. **Self-contained**: Token contains all necessary information
4. **Standard**: Widely supported across platforms and languages

### JWT Structure

A JWT has three parts separated by dots (`.`):

```
Header.Payload.Signature
```

- **Header**: Algorithm and token type (e.g., `{"alg": "HS256", "typ": "JWT"}`)
- **Payload**: Claims (user data) - this is what you store in the token
- **Signature**: Ensures the token hasn't been tampered with

### Why Not Just Send User ID?

If you just sent a user ID, an attacker could:
- Change the ID to access another user's data
- Replay old requests
- Create fake requests

JWT's signature prevents tampering, and expiration prevents replay attacks.

---

## How JWT Works (Token Flow)

Here's the complete flow in FirstAPI:

### 1. Registration/Login Flow

```
User → POST /api/auth/register (or /login)
  ↓
AuthService validates credentials
  ↓
If valid: Generate JWT token
  ↓
Return token to client
  ↓
Client stores token (usually in localStorage or memory)
```

### 2. Authenticated Request Flow

```
Client → Request with token in Authorization header
  ↓
ASP.NET Core middleware extracts token
  ↓
Token validation (signature, expiration, issuer, audience)
  ↓
If valid: Extract claims, create ClaimsPrincipal
  ↓
Controller receives authenticated user context
  ↓
Process request
```

### 3. Implementation in FirstAPI

**Token Generation** (`AuthService.cs`):
- Creates claims (user ID, email, role)
- Signs token with secret key
- Sets expiration time

**Token Validation** (`Program.cs`):
- Validates signature (prevents tampering)
- Validates expiration (prevents old tokens)
- Validates issuer/audience (ensures token came from your API)

**Why Each Step Matters**:
- **Signature validation**: Prevents attackers from creating fake tokens
- **Expiration validation**: Limits damage if token is stolen
- **Issuer/Audience validation**: Ensures token is for YOUR API, not another service

---

## What is Stored in the Token (Claims)

**Claims** are pieces of information about the user stored inside the JWT.

### Current Claims in FirstAPI

Looking at `AuthService.cs`, the following claims are stored:

```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())  // User ID
new Claim(ClaimTypes.Email, user.Email)                   // Email address
new Claim(ClaimTypes.Role, user.Role)                     // User role
```

### Why These Specific Claims?

1. **NameIdentifier (User ID)**: 
   - Used to identify which user made the request
   - Needed for authorization checks
   - **Why not store more?** More data = larger token = slower requests

2. **Email**:
   - Useful for display purposes
   - Can be used for logging/auditing
   - **Security note**: Email is not sensitive, but be careful with PII

3. **Role**:
   - Enables role-based authorization
   - Determines what endpoints user can access
   - **Why in token?** Avoids database lookup on every request

### What NOT to Store in Tokens

❌ **Never store**:
- Passwords (even hashed)
- Credit card numbers
- Social Security Numbers
- Full user profiles
- Sensitive business data

**Why?** Tokens can be decoded (payload is base64, not encrypted). Anyone can read the payload, though they can't modify it without the secret key.

### Token Size Considerations

- Larger tokens = more bandwidth
- Some servers/proxies have header size limits
- Keep claims minimal and essential

---

## Token Validation in ASP.NET Core

In `Program.cs`, token validation is configured:

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,           // Token must come from our server
    ValidateAudience = true,          // Token must be for our API
    ValidateLifetime = true,          // Token must not be expired
    ValidateIssuerSigningKey = true,  // Signature must be valid
    ValidIssuer = jwtIssuer,
    ValidAudience = jwtAudience,
    IssuerSigningKey = new SymmetricSecurityKey(...)
};
```

### What Each Validation Does

1. **ValidateIssuer**: 
   - Checks the `iss` claim matches your server
   - **Why?** Prevents tokens from other services being used

2. **ValidateAudience**:
   - Checks the `aud` claim matches your API
   - **Why?** Prevents tokens meant for other APIs from working

3. **ValidateLifetime**:
   - Checks `exp` (expiration) and `nbf` (not before) claims
   - **Why?** Prevents expired or future-dated tokens

4. **ValidateIssuerSigningKey**:
   - Verifies the signature using your secret key
   - **Why?** This is the most important - prevents tampering

### What Happens if Validation Fails?

- Request is rejected with `401 Unauthorized`
- User must login again to get a new token
- **This is good!** It means security is working

### Why Validate Everything?

Defense in depth: Each validation catches different attack vectors:
- Expired tokens → Time-based attacks
- Wrong issuer → Token reuse across services
- Invalid signature → Token tampering

---

## Why HTTPS is Required

**HTTPS (HTTP Secure)** encrypts data in transit between client and server.

### The Problem with HTTP

Without HTTPS:
```
Client → [Plain text password] → Server
```

Anyone on the network can:
- See passwords in plain text
- Steal JWT tokens
- Modify requests (man-in-the-middle attacks)
- Impersonate your API

### How HTTPS Protects

1. **Encryption**: Data is encrypted, so even if intercepted, it's unreadable
2. **Integrity**: Detects if data was modified in transit
3. **Authentication**: Verifies you're talking to the real server (not a fake one)

### Implementation in FirstAPI

In `Program.cs`:
```csharp
app.UseHttpsRedirection();  // Redirects HTTP to HTTPS
app.UseHsts();               // Tells browsers to always use HTTPS
```

### Why HSTS?

**HSTS (HTTP Strict Transport Security)** tells browsers:
- "Always use HTTPS for this domain"
- Prevents downgrade attacks (forcing HTTP)
- **Only enabled in production** (line 120-124) because development often uses HTTP

### Real-World Impact

Without HTTPS:
- Coffee shop Wi-Fi attacker steals your JWT token
- They can impersonate you until token expires
- Your account is compromised

With HTTPS:
- Even if intercepted, token is encrypted
- Attacker can't read or modify it
- Your account stays safe

**Bottom line**: HTTPS is not optional for production APIs. Never deploy without it.

---

## Password Hashing

**Password hashing** converts passwords into irreversible strings that cannot be converted back to the original password.

### Why Hash Passwords?

If passwords are stored in plain text:
- Database breach = all passwords exposed
- Admin access = can see all passwords
- One vulnerability = complete compromise

### How Hashing Works

```
Password: "MyPassword123"
  ↓
Hash Function (e.g., bcrypt, PBKDF2)
  ↓
Hash: "a3f5b8c9d2e1f4..." (always same for same input)
```

**Key properties**:
- **One-way**: Cannot reverse hash to get password
- **Deterministic**: Same password = same hash
- **Salted**: Adds random data to prevent rainbow table attacks

### Implementation in FirstAPI

FirstAPI uses ASP.NET Core Identity's `PasswordHasher<User>`:

```csharp
// Registration
user.PasswordHash = _hasher.HashPassword(user, dto.Password);

// Login
var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
```

### Why This Implementation is Good

1. **Uses industry-standard algorithms** (PBKDF2 with HMAC-SHA256)
2. **Automatic salting**: Each password gets a unique salt
3. **Work factor**: Can be adjusted to slow down brute-force attacks
4. **Battle-tested**: Used by Microsoft in production systems

### What Happens During Login

1. User sends password
2. Server retrieves stored hash from database
3. Server hashes the provided password with the same salt
4. Compare hashes (not passwords!)
5. If match → login successful

**Why compare hashes?** Even the server never sees the actual password after registration.

### Security Best Practices

✅ **DO**:
- Use strong, well-tested hashing libraries (like Identity's PasswordHasher)
- Never implement your own hashing algorithm
- Use appropriate work factors (balance security vs performance)

❌ **DON'T**:
- Store passwords in plain text
- Use weak algorithms (MD5, SHA1)
- Reuse the same salt for all users
- Log passwords (even in error messages)

---

## Common API Security Risks

Understanding these risks helps you build more secure APIs.

### 1. Broken Authentication

**What it is**: Weaknesses in authentication mechanisms.

**Examples**:
- Weak passwords allowed
- No rate limiting on login attempts
- Tokens that never expire
- Predictable session IDs

**How to prevent**:
- Enforce strong password policies
- Implement rate limiting (e.g., max 5 login attempts per 15 minutes)
- Use token expiration (already implemented in FirstAPI)
- Use secure, random token generation

**Why it matters**: Broken authentication = anyone can become any user.

### 2. Injection

**What it is**: Malicious code inserted into queries/commands.

**SQL Injection Example**:
```csharp
// BAD - Vulnerable
var query = $"SELECT * FROM Users WHERE Email = '{email}'";

// GOOD - Parameterized (Entity Framework does this automatically)
var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
```

**How FirstAPI is protected**:
- Entity Framework Core uses parameterized queries automatically
- Never concatenate user input into SQL strings

**Why it matters**: Injection can lead to:
- Data theft
- Data deletion
- Complete database compromise

### 3. Over-Posting / Mass Assignment

**What it is**: Users sending more data than expected, setting fields they shouldn't.

**Example**:
```json
// User sends this in registration
{
  "email": "user@example.com",
  "password": "password123",
  "role": "Admin"  // ← User trying to make themselves admin!
}
```

**How FirstAPI prevents it**:
- Uses DTOs (Data Transfer Objects) that only accept specific fields
- `RegisterDto` only has `Email` and `Password`
- Even if user sends `role`, it's ignored

**Why it matters**: Without DTOs, users could:
- Set themselves as admin
- Modify other users' data
- Bypass business rules

### 4. Exposing Sensitive Data

**What it is**: Accidentally revealing sensitive information in responses or errors.

**Examples**:
- Returning full user objects with password hashes
- Error messages revealing database structure
- Logging sensitive data

**How FirstAPI handles it**:
- Uses `AuthResponseDto` instead of returning full `User` object
- Exception middleware can sanitize error messages
- Serilog can be configured to exclude sensitive fields

**Why it matters**: Information disclosure helps attackers:
- Understand your system structure
- Plan more sophisticated attacks
- Find other vulnerabilities

### 5. Insecure Direct Object References (IDOR)

**What it is**: Accessing resources by manipulating IDs without authorization checks.

**Example**:
```
GET /api/students/1  (user's own student)
GET /api/students/999  (another user's student - should be blocked!)
```

**How to prevent**:
- Always verify user owns the resource
- Use authorization checks before returning data
- Don't expose sequential IDs if possible (use GUIDs)

**Why it matters**: Without proper checks, users can access any data by guessing IDs.

---

## Why DTOs Help Security

**DTOs (Data Transfer Objects)** are classes that define exactly what data can be sent/received.

### The Problem Without DTOs

If you accept your full `User` model:
```csharp
[HttpPost("register")]
public IActionResult Register(User user)  // BAD!
```

Users could send:
```json
{
  "email": "user@example.com",
  "password": "password",
  "id": 999,           // Trying to set ID
  "role": "Admin",     // Trying to become admin
  "passwordHash": "..." // Trying to set hash directly
}
```

### How DTOs Protect

With DTOs (`RegisterDto`):
```csharp
public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    // That's it! No Id, Role, or PasswordHash
}
```

**Benefits**:
1. **Explicit contract**: Only these fields are accepted
2. **Prevents mass assignment**: Extra fields are ignored
3. **Clear intent**: Shows exactly what the endpoint needs
4. **Validation target**: Can validate only relevant fields

### Real-World Example

In `AuthController.Register()`:
- Accepts `RegisterDto` (only email + password)
- Service creates `User` with safe defaults (`Role = "User"`)
- User cannot manipulate internal fields

**Why this matters**: DTOs are your first line of defense against malicious input.

---

## Why Validation Helps Security

**Validation** ensures data meets your requirements before processing.

### Security Benefits of Validation

1. **Prevents injection attacks**: Validating format/size limits malicious input
2. **Prevents buffer overflows**: Length limits protect against oversized inputs
3. **Prevents business logic bypass**: Ensures data follows rules
4. **Prevents DoS attacks**: Rejects malformed requests early

### Validation in FirstAPI

FirstAPI uses **FluentValidation**:

```csharp
// RegisterDtoValidator.cs
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
```

### What This Prevents

**Without validation**:
- Empty email → Database error or unexpected behavior
- Invalid email format → Could be used for injection
- Short password → Weak security
- SQL injection in email field → Database compromise

**With validation**:
- Invalid data rejected before reaching database
- Clear error messages to user
- Consistent data quality
- Reduced attack surface

### Validation Layers

1. **Client-side**: Fast feedback (but can be bypassed)
2. **API validation**: **This is where security matters** (FluentValidation)
3. **Database constraints**: Last line of defense

**Why multiple layers?** Defense in depth - if one fails, others catch it.

### Best Practices

✅ **DO**:
- Validate all user input
- Validate format, length, and type
- Use whitelist approach (allow only known good)
- Return generic error messages (don't reveal system details)

❌ **DON'T**:
- Trust client-side validation alone
- Reveal validation rules in error messages
- Skip validation for "trusted" sources

---

## Logging and Security

**Logging** records events for debugging, monitoring, and security auditing.

### Security Benefits of Logging

1. **Audit trail**: Who did what, when
2. **Attack detection**: Unusual patterns indicate attacks
3. **Forensics**: Investigate security incidents
4. **Compliance**: Many regulations require audit logs

### Logging in FirstAPI

FirstAPI uses **Serilog**:
- Logs to console (development)
- Logs to files (production)
- Request logging middleware

### What to Log (Security-Related)

✅ **DO log**:
- Authentication attempts (success and failure)
- Authorization failures
- Sensitive operations (password changes, role changes)
- Unusual patterns (many failed logins)
- Request metadata (IP address, user agent, timestamp)

❌ **DON'T log**:
- Passwords (even in errors)
- Full request bodies with sensitive data
- Credit card numbers
- JWT tokens (log token presence, not the token itself)
- Personal information (unless necessary for compliance)

### Example: Secure Logging

```csharp
// GOOD - Logs event without sensitive data
Log.Information("User {UserId} logged in from {IpAddress}", userId, ipAddress);

// BAD - Logs sensitive data
Log.Information("User logged in with password {Password}", password);
```

### Log Analysis for Security

Monitor logs for:
- **Brute force attacks**: Many failed logins from same IP
- **Privilege escalation attempts**: Unauthorized access attempts
- **Unusual patterns**: Access at odd hours, from new locations
- **Error spikes**: Could indicate attack attempts

### Why This Matters

Without logging:
- Attack happens → No record → Can't investigate
- Compliance violation → Fines/legal issues
- No way to detect ongoing attacks

With proper logging:
- Security incidents are detectable
- Forensics are possible
- Compliance requirements met
- Patterns reveal attacks early

---

## Token Expiration and Refresh Tokens

### Token Expiration

**Why tokens expire**: Limits damage if token is stolen.

**Current implementation** (FirstAPI):
- Tokens expire after configured minutes (default: 60)
- After expiration, user must login again

**Trade-offs**:
- **Short expiration** (15 minutes): More secure, but frequent logins annoy users
- **Long expiration** (7 days): Better UX, but stolen token works longer

### Refresh Tokens (Conceptual)

**Refresh tokens** solve the UX vs security trade-off:

**How it works**:
1. User logs in → Gets access token (short-lived, e.g., 15 minutes) + refresh token (long-lived, e.g., 7 days)
2. Access token expires → Use refresh token to get new access token
3. Refresh token expires → User must login again

**Benefits**:
- Short access token lifetime (security)
- Long refresh token lifetime (UX)
- Refresh token can be revoked (logout, security incident)

### Implementation Concept

```csharp
// Conceptual - not yet implemented
public class AuthResponseDto
{
    public string AccessToken { get; set; }      // Short-lived (15 min)
    public string RefreshToken { get; set; }     // Long-lived (7 days)
    public DateTime AccessTokenExpires { get; set; }
}
```

**Refresh endpoint**:
```
POST /api/auth/refresh
Body: { "refreshToken": "..." }
Response: { "accessToken": "...", "refreshToken": "..." }
```

### Why This Matters

**Without refresh tokens**:
- Long-lived tokens = security risk
- Short-lived tokens = poor UX
- Can't revoke tokens without waiting for expiration

**With refresh tokens**:
- Best of both worlds (security + UX)
- Can revoke refresh token to force re-login
- Access tokens can be very short-lived

---

## Role-Based Authorization

**Role-Based Access Control (RBAC)** restricts access based on user roles.

### Current State in FirstAPI

- Users have a `Role` property (default: "User")
- Role is stored in JWT token
- Authorization infrastructure exists (`AddAuthorization()` in Program.cs)
- **Not yet implemented**: Controllers don't use `[Authorize(Roles = "...")]` yet

### How It Would Work

**Conceptual implementation**:

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("api/students/{id}")]
public IActionResult DeleteStudent(int id)
{
    // Only admins can delete
}

[Authorize(Roles = "Admin,Teacher")]
[HttpPost("api/students")]
public IActionResult CreateStudent(CreateStudentDto dto)
{
    // Admins and Teachers can create
}

[Authorize]  // Any authenticated user
[HttpGet("api/students/{id}")]
public IActionResult GetStudent(int id)
{
    // Any logged-in user can view
}
```

### Role Hierarchy (Conceptual)

Common roles:
- **User**: Basic access, own data only
- **Teacher**: Can manage students
- **Admin**: Full access, can manage users

### Why This Matters

**Without authorization**:
- Any authenticated user can do anything
- No way to restrict sensitive operations
- Can't implement least privilege principle

**With role-based authorization**:
- Users only access what they need
- Sensitive operations protected
- Clear separation of concerns
- Easier to audit who can do what

### Best Practices

1. **Principle of least privilege**: Give minimum access needed
2. **Explicit deny by default**: Require explicit permission
3. **Regular audits**: Review who has what roles
4. **Role naming**: Clear, descriptive names

---

## Secrets Management

**Secrets** are sensitive values like API keys, database passwords, and JWT signing keys.

### Why Secrets Management Matters

**Never commit secrets to code**:
- Git history is permanent
- Anyone with repo access sees secrets
- Public repos = secrets exposed to world

### Current Implementation in FirstAPI

FirstAPI supports multiple secret sources (in order of priority):

1. **Environment variables** (most secure)
2. **appsettings.json** (development only)
3. **.env file** (development, loaded via DotNetEnv)

**From Program.cs**:
```csharp
// JWT Key - tries environment variable first, then config
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? Environment.GetEnvironmentVariable("JWT_KEY");

// Connection string - same pattern
var connection = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");
```

### Secrets in appsettings.json

**Development** (`appsettings.Development.json`):
- Can contain secrets for local development
- **Never commit real production secrets here**
- Use `.gitignore` to exclude if needed

**Production** (`appsettings.json`):
- Should contain non-sensitive defaults only
- Real secrets come from environment variables

### Environment Variables (Recommended)

**Why environment variables?**
- Not stored in code
- Different per environment (dev, staging, prod)
- Easy to rotate without code changes
- Many hosting platforms support them

**Setting environment variables**:

**Windows (PowerShell)**:
```powershell
$env:JWT_KEY = "your-secret-key-here"
$env:CONNECTION_STRING = "your-connection-string"
```

**Linux/Mac**:
```bash
export JWT_KEY="your-secret-key-here"
export CONNECTION_STRING="your-connection-string"
```

**Production hosting** (Azure, AWS, etc.):
- Set in platform's configuration
- Usually through web UI or CLI

### .env Files (Development)

FirstAPI loads `.env` files for development:
```csharp
if (File.Exists(".env"))
{
    Env.Load();
}
```

**Best practices**:
- Add `.env` to `.gitignore`
- Create `.env.example` with placeholder values
- Never commit actual `.env` files

### Secrets Management Best Practices

✅ **DO**:
- Use environment variables for production
- Rotate secrets regularly
- Use different secrets per environment
- Use strong, random secrets (especially JWT keys)
- Limit who can access secrets

❌ **DON'T**:
- Commit secrets to Git
- Share secrets in chat/email
- Use the same secret everywhere
- Use weak secrets (like "password123")
- Log secrets (even accidentally)

### Future: Secret Management Services

For production, consider:
- **Azure Key Vault**: Managed secret storage
- **AWS Secrets Manager**: AWS secret management
- **HashiCorp Vault**: Open-source secret management
- **Docker secrets**: For containerized deployments

These provide:
- Encryption at rest
- Access control
- Audit logging
- Automatic rotation

---

## Future Security Improvements

This section outlines security enhancements to consider as the project grows.

### 1. Rate Limiting

**What**: Limit number of requests per IP/user/time period.

**Why**: Prevents brute force attacks and DoS.

**Implementation idea**:
```csharp
// Limit login attempts to 5 per 15 minutes per IP
services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("login", opt => {
        opt.Window = TimeSpan.FromMinutes(15);
        opt.PermitLimit = 5;
    });
});
```

### 2. Refresh Tokens

**What**: Long-lived tokens for getting new access tokens.

**Why**: Better balance of security (short access tokens) and UX (long refresh tokens).

**See**: [Token Expiration and Refresh Tokens](#token-expiration-and-refresh-tokens)

### 3. Role-Based Authorization

**What**: Implement `[Authorize(Roles = "...")]` on controllers.

**Why**: Restrict access based on user roles.

**See**: [Role-Based Authorization](#role-based-authorization)

### 4. CORS Configuration

**Current**: `AllowAll` policy (development only).

**Improvement**: Restrict to specific origins in production:
```csharp
options.AddPolicy("Production", policy => {
    policy.WithOrigins("https://yourdomain.com")
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

### 5. Input Sanitization

**What**: Clean user input to prevent XSS and injection.

**Why**: Even with validation, sanitization adds extra layer.

**Tools**: HTML encoding, SQL parameterization (already done via EF Core).

### 6. Security Headers

**What**: HTTP headers that tell browsers to enforce security.

**Examples**:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Content-Security-Policy`

**Implementation**: Use middleware like `NWebsec` or custom middleware.

### 7. API Versioning

**What**: Version your API (`/api/v1/`, `/api/v2/`).

**Why**: Allows security improvements without breaking clients.

### 8. Request/Response Logging

**What**: Log API requests and responses (sanitized).

**Why**: Audit trail and attack detection.

**Caution**: Don't log sensitive data (passwords, tokens, PII).

### 9. Two-Factor Authentication (2FA)

**What**: Require second factor (SMS, email, authenticator app) for login.

**Why**: Even if password is stolen, account is protected.

### 10. Account Lockout

**What**: Lock account after N failed login attempts.

**Why**: Prevents brute force attacks.

### 11. Password Policies

**What**: Enforce strong password requirements.

**Current**: Minimum length validation exists.

**Enhancement**: Add complexity requirements (uppercase, numbers, symbols).

### 12. Security Auditing

**What**: Regular security reviews and penetration testing.

**Why**: Find vulnerabilities before attackers do.

### 13. Dependency Scanning

**What**: Scan NuGet packages for known vulnerabilities.

**Tools**: `dotnet list package --vulnerable`, Snyk, WhiteSource.

### 14. HTTPS Certificate Management

**What**: Proper SSL/TLS certificate handling.

**Why**: Prevents certificate-related attacks.

**Consider**: Let's Encrypt for free certificates, automatic renewal.

### 15. Data Encryption at Rest

**What**: Encrypt sensitive database fields.

**Why**: Protects data if database is compromised.

**Note**: Usually handled by database or infrastructure, not application code.

---

## Summary

Security is not a feature you add once—it's an ongoing process. This guide covers the fundamentals, but security requires:

1. **Understanding threats**: Know what you're protecting against
2. **Defense in depth**: Multiple layers of protection
3. **Regular updates**: Keep dependencies and practices current
4. **Monitoring**: Watch for attacks and vulnerabilities
5. **Education**: Stay informed about new threats and best practices

Remember: **Security is everyone's responsibility**. Every line of code you write should consider security implications.

---

## Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/): Common web vulnerabilities
- [ASP.NET Core Security Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [JWT.io](https://jwt.io/): JWT debugger and information
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/): API-specific risks

---

**Last Updated**: January 2025
**Project**: FirstAPI - ASP.NET Core Web API Learning Project
