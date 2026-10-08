# BlogApp API

BlogApp is an ASP.NET Core Web API for user authentication and blog posts, categories, and comments. It uses SQL Server through EF Core, ASP.NET Core Identity, JWT bearer authentication, optional post image uploads, and Swagger in Development.

## Features

- Register users and log in to receive a JWT
- Public, database-paginated post listing
- Authenticated post detail, create, update, and soft-delete
- Post ownership checks for update/delete; Admin can update/delete any post
- Authenticated comments with author taken from the JWT
- Admin-only category creation
- FluentValidation in application services
- Identity roles (`Admin` and `User`)
- Automatic `CreatedDate` and `UpdatedDate` handling
- Global soft-delete query filters for posts, comments, and categories

## Architecture

The solution retains four projects:

```text
BlogApp.API            Controllers, middleware, authentication setup
BlogApp.Application    Service contracts, request models, DTOs, validators
BlogApp.Domain         Entities and shared domain types
BlogApp.Infrastructure Service implementations, repositories, EF Core, Identity
```

Request flow:

```text
Controller -> Service interface (Application)
           -> Service implementation (Infrastructure)
           -> Repository -> EF Core DbContext -> SQL Server
```

Application service interfaces (`IPostService`, `IAuthService`, `ICommentService`, `ICategoryService`) are declared in `BlogApp.Application` and implemented in `BlogApp.Infrastructure`. Services receive FluentValidation `IValidator<T>` instances and validate their request models. MediatR, commands, queries, handlers, and the MediatR validation pipeline are not used.

## Technology and prerequisites

- .NET 8 SDK
- SQL Server
- EF Core 8
- ASP.NET Core Identity and JWT bearer authentication
- FluentValidation
- Explicit DTO mapping in the application service implementations
- Swashbuckle / Swagger UI

## Configuration

`BlogApp.API/appsettings.json` does not contain a connection string or JWT secret. Supply both via User Secrets, environment variables, or another trusted configuration provider. Infrastructure registration fails on startup if `JwtSettings:Secret` is missing.

Example environment variables:

```bash
export ConnectionStrings__DefaultConnection='<sql-server-connection-string>'
export JwtSettings__Secret='<long-random-secret>'
```

The issuer, audience, and token expiry are configured in `JwtSettings` (`ExpiryMinutes`, currently 60). Do not commit real credentials or secrets.

## Database setup

EF Core migrations are in `BlogApp.Infrastructure/Migrations`. Apply them from the repository root:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update \
  --project BlogApp.Infrastructure \
  --startup-project BlogApp.API
```

The model includes Identity tables, blog entities, soft-delete columns, post image URLs, and seeded `Admin` and `User` roles.

## Run locally

```bash
dotnet restore BlogApp.sln
dotnet build BlogApp.sln
dotnet run --project BlogApp.API
```

Swagger UI is available at `/swagger` in the Development environment. Launch profiles use `http://localhost:5178` and `https://localhost:7090`.

## API endpoints

All API routes use the `/api` prefix.

| Method | Route | Access | Behavior |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Public | Register a user; returns 201 |
| `POST` | `/api/auth/login` | Public | Return a JWT; invalid credentials return 401 |
| `GET` | `/api/posts` | Public | Return a paginated post list |
| `GET` | `/api/posts/{id}` | Authenticated | Return a post and comments |
| `POST` | `/api/posts` | Authenticated | Create a post, optionally with an image; returns 201 |
| `PUT` | `/api/posts` | Authenticated | Update an owned post or any post as Admin |
| `DELETE` | `/api/posts/{id}` | Authenticated | Soft-delete an owned post or any post as Admin; returns 204 |
| `POST` | `/api/comments` | Authenticated | Add a comment; author is taken from the JWT; returns 201 |
| `POST` | `/api/categories` | Admin | Create a category; returns 201 |

### Pagination

`GET /api/posts` accepts `pageNumber` (default `1`) and `pageSize` (default `10`). Both must be at least 1. The service caps `pageNumber` at 1,000,000 and `pageSize` at 100. Invalid values return 400. The repository runs a database count and a separate database query for the requested page.

Response fields: `items`, `currentPage`, `totalPages`, `pageSize`, and `totalCount`.

### Authentication examples

Register:

```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "userName": "jane",
  "password": "password123"
}
```

Login:

```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "jane",
  "password": "password123"
}
```

For protected endpoints, send:

```http
Authorization: Bearer <token>
```

### Create a post

Post creation accepts `multipart/form-data`:

```bash
curl -X POST http://localhost:5178/api/posts \
  -H 'Authorization: Bearer <token>' \
  -F 'title=My first post' \
  -F 'content=Post content' \
  -F 'categoryId=1' \
  -F 'image=@./image.jpg'
```

The title must contain 5–100 characters and content is required. Uploaded files are stored below `BlogApp.API/wwwroot/uploads` and served as static files.

## Validation and authorization

- Post list pagination, post create/update, comment creation, category creation, register, and login requests have validators.
- Identity currently requires passwords to have at least six characters; digit, uppercase, and non-alphanumeric requirements are disabled.
- Update/delete are limited to the post owner or Admin.
- Comments require authentication. The service reads the author from the `UserName` claim in the JWT; the client request does not accept an author field.
- Invalid login credentials return 401. A non-owner attempting to update/delete a post receives 403.

## Tests and CI

`BlogApp.Tests` is an xUnit project in the solution. It tests post ownership enforcement, service validation, and page/page-size limits.

Run tests with:

```bash
dotnet test BlogApp.sln
```

The projects target .NET 8, so the local machine needs the .NET 8 SDK and runtime for the plain command above. The CI workflow installs .NET 8 explicitly with `actions/setup-dotnet`; a machine with only .NET 10 cannot run the .NET 8 test host unless .NET 8 runtime is installed. The test project does not enable runtime roll-forward.

`.github/workflows/ci.yml` restores, builds, and tests the solution on pushes and pull requests using .NET 8. Its configuration is reviewed in source; this statement does not claim a GitHub Actions run has succeeded.
