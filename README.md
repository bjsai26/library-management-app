# Library Management API

A library management REST API built on ASP.NET Core 10 with a three-layer architecture. It is an API-only project — there is no frontend, and everything is exercised through **Swagger UI**.

Scope: full CRUD over Books and Categories, with JWT authentication and role-based authorization.

[Quick start](#quick-start) · [Architecture](#architecture) · [Dependencies](#dependencies) · [API reference](#api-reference) · [Validation](#validation) · [Security](#security) · [Configuration](#configuration) · [Database](#database) · [Verification](#verification)

---

## Quick start

You need the .NET 10 SDK and a SQL Server instance (Express, Developer or LocalDB all work).

```bash
# 1. Point the API at your SQL Server if the default does not match your machine
#    src/LibraryManagement.API/appsettings.json -> ConnectionStrings:DefaultConnection

# 2. Run it. The database is created, migrated and seeded automatically on first run.
dotnet run --project src/LibraryManagement.API
```

Then open **<http://localhost:5080>** — Swagger UI is served at the root. The raw OpenAPI document is at `/swagger/v1/swagger.json`.

The solution also ships an `https` launch profile on `https://localhost:7080`:

```bash
dotnet run --project src/LibraryManagement.API --launch-profile https
```

Swagger is registered unconditionally rather than behind an `IsDevelopment()` check, because the UI is the only way this project is meant to be driven.

### Signing in from Swagger

1. Expand `POST /api/auth/login` and send one of the seeded accounts below.
2. Copy `data.accessToken` from the response.
3. Click **Authorize** at the top right, paste the token, and confirm. The `Bearer` prefix is added for you.

### Seeded accounts

| Role | Email | Password | Can do |
| --- | --- | --- | --- |
| Admin | `admin@library.com` | `Admin@123` | Read and write everything |
| Member | `member@library.com` | `Member@123` | Read only |

The database is also seeded with 3 categories, 3 authors and 3 books. Authors have no endpoints of their own — they exist as a lookup that books reference, so use a seeded `authorId` when creating a book.

Self-registration always produces a **Member**. No endpoint can create an Admin; admins are seeded, or promoted directly in the database. That keeps privilege escalation off the public surface.

---

## Architecture

Three layers, each its own project, with dependencies flowing in one direction only:

```
LibraryManagement.API              (Presentation)    Controllers, middleware, JWT and Swagger setup
        │  depends on
        ▼
LibraryManagement.AppServices      (Business Logic)  Services, DTOs, validation, mapping, password hashing
        │  depends on
        ▼
LibraryManagement.Infrastructure   (Data Access)     EF Core DbContext, entities, generic repository
```

The API project never references `DbContext` or an entity — it talks to service interfaces and receives DTOs. AppServices never references EF Core types — it talks to `IGenericRepository<T>`. Each layer registers its own dependencies, so `Program.cs` wires the whole application with two calls:

```csharp
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddAppServices();
```

The one place the boundary is deliberately relaxed: `ITokenService` takes a `User` entity rather than a DTO, since minting a token needs the persisted identity and nothing else in the layer benefits from a parallel model.

### Project layout

```
LibraryManagement.sln
└── src/
    ├── LibraryManagement.Infrastructure/
    │   ├── Entities/         BaseEntity, User, Author, Category, Book
    │   ├── Enums/            UserRole
    │   ├── Data/
    │   │   ├── AppDbContext.cs      Fluent API configuration and timestamp stamping
    │   │   ├── DataSeeder.cs        Baseline rows written by the initial migration
    │   │   └── Migrations/          EF Core migrations
    │   ├── Repositories/     IGenericRepository<T> and its implementation
    │   └── DependencyInjection.cs
    ├── LibraryManagement.AppServices/
    │   ├── DTOs/             Request and response models, grouped by feature
    │   ├── Interfaces/       One interface per service
    │   ├── Services/         Auth, Book, Category, Token
    │   ├── Mappings/         Hand-written entity to DTO extension methods
    │   ├── Security/         PBKDF2 password hasher
    │   ├── Settings/         JwtSettings, bound from appsettings.json
    │   ├── Common/           ServiceResult / ServiceResult<T>
    │   └── DependencyInjection.cs
    └── LibraryManagement.API/
        ├── Controllers/      BaseApi, Auth, Books, Categories
        ├── Middleware/       Global exception handler
        ├── Extensions/       JWT, Swagger and validation-response setup
        └── Program.cs
```

### Design notes

**No CQRS.** Each controller calls one service interface, and each service owns both the read and write logic for its resource. No command/query objects, no handlers, no mediator.

**Services return results, they do not throw.** Expected failures — not found, duplicate ISBN, category still in use — come back as a `ServiceResult` carrying a `ServiceError`. `BaseApiController` maps that enum to the HTTP status code, which is why no controller contains a try/catch. Unexpected exceptions are caught by the middleware and returned in the same envelope.

**One generic repository.** `IGenericRepository<T>` is registered as an open generic, so `IGenericRepository<Book>`, `IGenericRepository<Author>` and the rest all resolve from a single line of DI. Services inject the ones they need. It supports paging, filtering, ordering and eager loading through expression parameters, which covers every query in the project.

**No N+1 queries.** `CountByAsync` issues one `GROUP BY` and returns a dictionary, so the `bookCount` on a page of categories costs a single query instead of one per row. Listing categories is three queries — total count, the page, and the grouped count — whatever the page size.

**Swagger is documented from the code.** Every action carries `///` summaries and `<response>` tags, and `GenerateDocumentationFile` feeds the generated XML into `IncludeXmlComments`, so the descriptions in Swagger UI cannot drift from the controllers. `ProducesResponseType` declares the envelope type for each status code an action can return.

**One envelope, no exceptions to it.** `ApiResponse<T>` wraps success and failure alike — including the two responses ASP.NET Core would normally return with an empty body. `JwtBearerEvents.OnChallenge` and `OnForbidden` are overridden so 401 and 403 carry the same JSON shape, and `InvalidModelStateResponseFactory` replaces the default `ValidationProblemDetails` for DataAnnotation failures.

**Writes return 200, not 201.** `POST` returns `200 OK` with the created resource in `data` rather than `201 Created` with a `Location` header, so every endpoint has one predictable response shape. A production API would more likely use 201.

**Deletes are real deletes**, guarded by a check first: a category that still has books returns 409 rather than cascading. The database agrees — both foreign keys on `Books` use `DeleteBehavior.Restrict`, so the rule holds even if a row is deleted outside the service.

**Reads do not track.** `GetPagedAsync` uses `AsNoTracking`, since list responses are projected straight to DTOs and never written back. Write paths load tracked entities normally.

**Cancellation is threaded end to end.** Every controller action takes a `CancellationToken` and passes it through the service into EF Core, so an abandoned request stops work at the database rather than running to completion.

**Timestamps are handled centrally.** `AppDbContext.SaveChangesAsync` overrides the base call and stamps `CreatedAt` / `UpdatedAt` from the change tracker, so no service can forget to set them.

---

## Dependencies

The project uses the .NET base class library wherever possible. Five packages are present, each because the task requires it:

| Package | Version | Why |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.12 | The ORM |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.12 | Migration tooling (build-time only, `PrivateAssets=all`) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.12 | Bearer token authentication |
| `System.IdentityModel.Tokens.Jwt` | 8.22.0 | Signing and issuing the tokens |
| `Swashbuckle.AspNetCore` | 10.2.3 | Swagger UI and `swagger.json` |

Notably **not** used: AutoMapper (mapping is hand-written in `Mappings/MappingExtensions.cs`), FluentValidation (validation uses `System.ComponentModel.DataAnnotations`), MediatR, and any password-hashing library (`PasswordHasher` uses PBKDF2-SHA256 from `System.Security.Cryptography`).

---

## API reference

12 operations across 6 routes. Every response uses the same envelope:

```json
{
  "success": true,
  "message": "Request processed successfully.",
  "data": { },
  "errors": []
}
```

On failure, `success` is `false` and `errors` carries one entry per broken rule. Two serialization settings shape the payload: enums are written by name (`"Admin"`, not `2`), and null properties are omitted entirely.

### Authentication — `/api/auth`

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| POST | `/register` | Anonymous | Creates a Member account and returns a JWT |
| POST | `/login` | Anonymous | Returns a JWT |

Both return `userId`, `fullName`, `email`, `role`, `accessToken`, `expiresAt` and `tokenType`.

### Books — `/api/books`

| Method | Route | Access |
| --- | --- | --- |
| GET | `/` | Any signed-in user |
| GET | `/{id}` | Any signed-in user |
| POST | `/` | Admin |
| PUT | `/{id}` | Admin |
| DELETE | `/{id}` | Admin |

`BookDto` resolves the foreign keys for the caller: it carries `authorName` and `categoryName` alongside the ids, so a client does not need a second lookup.

### Categories — `/api/categories`

Identical shape to books: read for any signed-in user, write for Admin. `CategoryDto` includes a `bookCount`, resolved for the whole page in one grouped query.

### List parameters

Both list endpoints accept `pageNumber`, `pageSize` and `search`. The `QueryParameters` setters clamp rather than reject: a `pageNumber` below 1 becomes 1, and a `pageSize` outside 1–100 falls back to the default of 10 or the ceiling of 100. Search matches title or ISBN for books, and the name for categories. Results are ordered by title and by name respectively.

Responses carry `items`, `pageNumber`, `pageSize`, `totalCount` and a computed `totalPages`.

---

## Validation

Validation happens in two places.

**Request shape** is enforced by DataAnnotations on the DTOs and checked automatically by `[ApiController]`. `BookRequest` also implements `IValidatableObject` for the cross-field rule that a published year cannot be in the future. Failures return 400 with one message per broken rule:

```json
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "errors": [
    "Email: Email must be a valid email address.",
    "Password: Password must contain an uppercase letter, a lowercase letter, a digit and a special character.",
    "ConfirmPassword: Password and confirmation password do not match."
  ]
}
```

Rules applied: email format and 150-character limit, password strength (8–100 characters with upper, lower, digit and symbol), password confirmation match, full name 3–150 characters, category name 2–100 characters, title 1–250 characters, ISBN of exactly 10 or 13 digits, published year between 1450 and 2100 with a second rule rejecting anything past the current year, price 0–100,000, copies available 0–10,000, and positive author and category ids.

**Business rules** live in the services, since they need the database:

- Email must be unique across accounts
- ISBN must be unique across books (the check excludes the row being updated)
- Category names must be unique (likewise)
- A book must reference an author and category that exist
- A category cannot be deleted while books reference it

Uniqueness is enforced twice on purpose: the service check produces the friendly 409, and unique indexes on `Users.Email`, `Categories.Name` and `Books.Isbn` are the backstop if two requests race.

### Status codes

| Code | Meaning |
| --- | --- |
| 200 | Success, including creates and updates |
| 400 | Validation failure or a broken business rule |
| 401 | Missing, expired or invalid token; bad credentials |
| 403 | Authenticated but the role does not permit the action |
| 404 | Resource does not exist |
| 409 | Conflict — duplicate key, or a delete blocked by existing state |
| 500 | Unexpected error (details are only included in Development) |

---

## Security

- Passwords are stored as PBKDF2-SHA256 with a per-user 16-byte random salt, 100,000 iterations and a 32-byte derived key. Verification uses a fixed-time comparison, and a stored value that is not valid Base64 fails closed instead of throwing.
- Access tokens are HMAC-SHA256 JWTs carrying the user id, name, email and role, valid for 60 minutes. Issuer, audience, lifetime and signing key are all validated, with `ClockSkew` set to zero so an expired token is rejected the moment it expires rather than five minutes later.
- Startup fails fast if `Jwt:Key` is missing or shorter than 32 characters, rather than issuing weakly signed tokens.
- Login returns the same message for an unknown email and a wrong password, so the endpoint does not disclose which accounts exist.
- The exception middleware includes the exception type and message only in Development; deployed clients get the generic message and an empty `errors` array, while the full exception is logged server-side.

> The `Jwt:Key` in `appsettings.json` is a development value committed for convenience. Replace it, and move it to a secret store, before deploying anywhere real. The seeded account passwords are likewise for local evaluation only.

---

## Configuration

`src/LibraryManagement.API/appsettings.json`:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LibraryManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "...",                      // 32+ characters; startup fails if shorter
    "Issuer": "LibraryManagement.API",
    "Audience": "LibraryManagement.Client",
    "ExpiryMinutes": 60
  }
}
```

A missing `ConnectionStrings:DefaultConnection` or `Jwt` section throws at startup with a message naming the setting, rather than failing later on the first request.

`appsettings.Development.json` raises `Microsoft.EntityFrameworkCore.Database.Command` to `Information`, so generated SQL is visible in the console while developing.

**Other SQL Server setups:**

```jsonc
// LocalDB
"Server=(localdb)\\MSSQLLocalDB;Database=LibraryManagementDb;Trusted_Connection=True;TrustServerCertificate=True"

// Named instance
"Server=localhost\\SQLEXPRESS;Database=LibraryManagementDb;Trusted_Connection=True;TrustServerCertificate=True"

// SQL authentication
"Server=localhost;Database=LibraryManagementDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
```

---

## Database

EF Core code-first. The app calls `Database.MigrateAsync()` on startup, so the database is created and seeded on first run — no manual step needed. Seed data goes through `HasData`, which means it lives in the initial migration rather than in startup code, and re-running the app never duplicates it.

To manage migrations yourself:

```bash
dotnet tool install --global dotnet-ef

dotnet ef migrations add <Name> \
  --project src/LibraryManagement.Infrastructure \
  --startup-project src/LibraryManagement.API \
  --output-dir Data/Migrations

dotnet ef database update \
  --project src/LibraryManagement.Infrastructure \
  --startup-project src/LibraryManagement.API

# Start over
dotnet ef database drop --force \
  --project src/LibraryManagement.Infrastructure \
  --startup-project src/LibraryManagement.API
```

### Schema

| Table | Notes |
| --- | --- |
| `Users` | Unique index on email; PBKDF2 hash and salt; role persisted as int |
| `Authors` | Non-unique index on name |
| `Categories` | Unique index on name |
| `Books` | Unique index on ISBN; `decimal(18,2)` price; restrict-delete foreign keys to author and category |

Every table carries `Id`, `CreatedAt` and `UpdatedAt` from `BaseEntity`, stamped centrally as described above. Entity configuration is written inline in `OnModelCreating` rather than split across `IEntityTypeConfiguration` classes, which keeps the whole schema readable in one place at this size.

---

## Verification

No automated test project is included. Everything is verified by hand through Swagger UI: sign in as a seeded account, then walk the Books and Categories endpoints to check role enforcement on the writes, validation failures, paging and search, and the delete guards.

The services depend only on `IGenericRepository<T>`, so an xUnit project testing them against a fake repository — no EF Core, no database — is the natural next step.
