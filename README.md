# BlogApp API

BlogApp is a layered ASP.NET Core Web API for publishing blog posts, managing categories, and adding comments. It includes JWT-based authentication, role-aware authorization, SQL Server persistence through Entity Framework Core, optional post image uploads, and Swagger documentation in the Development environment.

## Features

- User registration and JWT login
- Public, paginated post listing
- Authenticated post creation, editing, and soft deletion
- Post details with comments
- Optional image upload for posts
- Comment creation
- Admin-only category creation
- ASP.NET Core Identity roles (`Admin` and `User`)
- FluentValidation pipeline behavior
- Automatic `CreatedDate` and `UpdatedDate` handling
- Global soft-delete query filters for posts, comments, and categories

## Architecture

The solution is organized into four projects:

```text
BlogApp.API            HTTP endpoints, middleware, authentication setup
BlogApp.Application    Commands, queries, DTOs, validation, mappings
BlogApp.Domain         Entities and shared domain types
BlogApp.Infrastructure EF Core DbContext, repositories, Identity, JWT, file storage
```

The API controllers send commands and queries through MediatR. The application layer depends on repository and service abstractions, while the infrastructure layer provides their implementations.

## Technology Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server
- ASP.NET Core Identity
- JWT Bearer authentication
- MediatR
- FluentValidation
- AutoMapper
- Swashbuckle / Swagger UI

## Prerequisites

- .NET 8 SDK
- SQL Server running locally or in a container
- A database connection string with permission to create and update the `BlogDb` database
- Entity Framework Core CLI tools for applying migrations:

```bash
dotnet tool install --global dotnet-ef
```

## Configuration

The API reads its connection string and JWT settings from the standard ASP.NET Core configuration providers. Configure these values for your environment instead of committing credentials to source control.

The required settings are:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<sql-server-connection-string>"
  },
  "JwtSettings": {
    "Secret": "<long-secret-key>",
    "Issuer": "<issuer>",
    "Audience": "<audience>",
    "ExpiryMinutes": 60
  }
}
```

For local development, these can be supplied with User Secrets or environment variables. For example:

```bash
dotnet user-secrets --project BlogApp.API set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=BlogDb;User Id=sa;Password=<password>;TrustServerCertificate=True;"
dotnet user-secrets --project BlogApp.API set "JwtSettings:Secret" "<long-development-secret>"
```

Do not reuse development credentials or JWT secrets in production.

## Database Setup

The repository contains Entity Framework Core migrations in `BlogApp.Infrastructure/Migrations`. Apply them from the repository root:

```bash
dotnet ef database update \
  --project BlogApp.Infrastructure \
  --startup-project BlogApp.API
```

The model includes Identity tables, blog entities, soft-delete columns, post image URLs, and seeded `Admin` and `User` roles.

## Run Locally

Restore dependencies and build the solution:

```bash
dotnet restore BlogApp.sln
dotnet build BlogApp.sln
```

Start the API with the Development environment:

```bash
dotnet run --project BlogApp.API
```

The configured launch profiles expose the API at:

- `http://localhost:5178`
- `https://localhost:7090`

Swagger UI is available at `/swagger` when `ASPNETCORE_ENVIRONMENT=Development`.

## API Endpoints

All routes use the `/api` prefix. Protected routes require a bearer token in the `Authorization` header.

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Public | Create a user account |
| `POST` | `/api/auth/login` | Public | Return a JWT token |
| `GET` | `/api/posts` | Public | Return paginated posts |
| `GET` | `/api/posts/{id}` | Authenticated | Return a post and its comments |
| `POST` | `/api/posts` | Authenticated | Create a post, optionally with an image |
| `PUT` | `/api/posts` | Authenticated | Update a post |
| `DELETE` | `/api/posts/{id}` | Authenticated | Soft-delete a post |
| `POST` | `/api/comments` | Public | Add a comment to a post |
| `POST` | `/api/categories` | Admin role | Create a category |

### Pagination

`GET /api/posts` accepts the following query parameters:

```text
pageNumber=1&pageSize=10
```

The response contains `items`, `currentPage`, `totalPages`, `pageSize`, and `totalCount`.

### Authentication Examples

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

Use the returned token on protected requests:

```http
Authorization: Bearer <jwt-token>
```

### Create a Post

Post creation uses `multipart/form-data` because an image can be uploaded with the post:

```bash
curl -X POST http://localhost:5178/api/posts \
  -H "Authorization: Bearer <jwt-token>" \
  -F "title=My first post" \
  -F "content=Post content" \
  -F "categoryId=1" \
  -F "image=@./image.jpg"
```

The title must be between 5 and 100 characters. Post content is required. Uploaded files are stored below `BlogApp.API/wwwroot/uploads` and are served as static files.

## Validation and Authorization Notes

- Post listing is the only post route explicitly marked as anonymous; post details still require authentication.
- Users can soft-delete their own posts. Admin users can soft-delete any post.
- Categories are protected by the `Admin` role.
- Identity password policy currently requires a minimum of six characters, without requiring digits, uppercase letters, or non-alphanumeric characters.
- Validation failures are processed through the registered MediatR validation pipeline.

## Project Layout

```text
BlogApp.API/
  Controllers/       HTTP endpoints
  Middleware/         Exception handling middleware
  Services/           Current-user access service
  wwwroot/uploads/    Uploaded post images
BlogApp.Application/
  Features/           MediatR commands, queries, handlers, validators
  DTOs/               API-facing data transfer objects
  Interfaces/         Repository and service contracts
BlogApp.Domain/
  Entities/           Post, Comment, Category, and AppUser
BlogApp.Infrastructure/
  Persistence/        EF Core DbContext
  Repositories/       Repository implementations
  Services/           Authentication and file services
  Migrations/         EF Core database migrations
```

## Current Scope

This repository contains the backend API. No frontend application, automated test project, container definition, or CI/CD workflow is included in the solution at this time.