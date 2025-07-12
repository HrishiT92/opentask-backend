# OpenTask Backend

.NET 8 Web API backend for the OpenTask project, implementing Clean Architecture with Entity Framework Core and PostgreSQL.

## Features

- Clean Architecture (Domain, Application, Infrastructure, API layers)
- Entity Framework Core 8 with PostgreSQL
- JWT Authentication with role-based access control
- Swagger/OpenAPI documentation
- Serilog structured logging
- Health checks
- CORS configuration for frontend integration

## Prerequisites

- .NET 8 SDK
- PostgreSQL (or use Docker Compose from opentask-infra)

## Getting Started

1. **Clone the repository:**
   ```bash
   git clone https://github.com/HrishiT92/opentask-backend.git
   cd opentask-backend
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Update connection string:**
   Edit `src/OpenTask.Api/appsettings.json` with your PostgreSQL connection string.

4. **Run the application:**
   ```bash
   cd src/OpenTask.Api
   dotnet run
   ```

5. **Access Swagger UI:**
   Open http://localhost:5000 in your browser.

## Project Structure

```
src/
├── OpenTask.Api/           # Web API layer
├── OpenTask.Application/   # Application services and interfaces
├── OpenTask.Domain/        # Domain entities and business logic
└── OpenTask.Infrastructure/ # Data access and external services

tests/
├── OpenTask.Api.Tests/
├── OpenTask.Application.Tests/
├── OpenTask.Domain.Tests/
└── OpenTask.Infrastructure.Tests/
```

## Building

```bash
dotnet build
```

## Testing

```bash
dotnet test
```

## Docker

Build the Docker image:
```bash
docker build -t opentask-backend .
```

Run with Docker:
```bash
docker run -p 5000:8080 opentask-backend
```

## API Documentation

Once running, visit http://localhost:5000 for interactive Swagger documentation.

## Configuration

Key configuration sections in `appsettings.json`:

- `ConnectionStrings`: Database connection
- `JwtSettings`: JWT token configuration
- `Serilog`: Logging configuration

## Contributing

1. Create a feature branch
2. Make your changes
3. Run tests: `dotnet test`
4. Submit a pull request
