# GropMng

ASP.NET Core MVC application for managing the plants a user keeps in a garden, yard, or balcony. The system is intended to track what plants the user has, their care requirements, and their day-to-day maintenance.

## Purpose

The project focuses on plant management, including scenarios such as:

- Recording the plants owned by the user
- Tracking plant needs such as watering, light, soil, fertilization, and seasonal care
- Organizing plant care and maintenance information
- Supporting future extensions around reminders, observations, and plant-specific history

## Tech Stack

- .NET 8 (ASP.NET Core MVC)
- Entity Framework Core 8
- Microsoft SQL Server
- Node.js tooling for SCSS build (Sass + concurrently)

## Prerequisites

- .NET SDK 8.x
- SQL Server instance accessible by the application
- Node.js 18+ and npm

## Quick Start

1. Restore .NET packages:

```bash
dotnet restore
```

2. Install frontend dependencies:

```bash
npm install
```

This installs the root npm package and automatically provisions the nested frontend dependencies under:

- `Presentation/GropMng.Web/wwwroot/node_modules`

The root package also provides the `sass` and `concurrently` tooling used by the SCSS scripts.

3. Configure SQL Server in `Presentation/GropMng.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=GropMng;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Database": {
    "EnableSqlServer": true
  }
}
```

4. Build the solution:

```bash
dotnet build GropMng.sln
```

5. Build SCSS assets:

```bash
npm run build:css
```

6. Run the web application:

```bash
dotnet run --project Presentation/GropMng.Web
```

The default route is:

- `Home/Index`

## Useful Commands

Build solution:

```bash
dotnet build GropMng.sln
```

Run tests:

```bash
dotnet test GropMng.sln
```

Watch SCSS (all):

```bash
npm run watch:css
```

If you cloned the repository fresh, run `npm install` once from the solution root before starting any watch command.

Watch SCSS (core only):

```bash
npm run watch:core
```

Watch SCSS (themes only):

```bash
npm run watch:themes
```

## Database Notes

- The application is configured to use SQL Server through `ConnectionStrings:DefaultConnection` in `Presentation/GropMng.Web/appsettings.json`.
- SQL Server registration is wired in `Presentation/GropMng.Web/Program.cs`.
- On startup, the application attempts to run Entity Framework Core migrations automatically when SQL Server is enabled and a connection string is present.
- The intended database platform for this project is Microsoft SQL Server only.

## Repository Hygiene

This repository excludes from source control:

- Build outputs (`bin/`, `obj/`)
- IDE/system files (`.vs/`, `.vscode/`, temp files)
- Node dependencies (`node_modules/`)
- Local/reference material (`_ReferenceFiles/`, `docs/`)
- Local development settings and secrets (`appsettings.Development.json`, `.env*`)
- Local database files

## Project Structure

```text
GropMng/
├── Libraries/
│   ├── GropMng.Core/       # Domain layer, entities, abstractions, shared types
│   ├── GropMng.Data/       # EF Core DbContext, mappings, persistence
│   └── GropMng.Services/   # Application and business services
├── Presentation/
│   └── GropMng.Web/        # ASP.NET Core MVC web application
├── Tests/
│   └── GropMng.Tests/      # Automated tests
├── GropMng.sln             # Visual Studio solution file
└── README.md               # Project overview and setup guide
```

## Architecture

The application follows a layered architecture:

1. **Core Layer** (`GropMng.Core`): Domain models, shared abstractions, and core contracts
2. **Data Layer** (`GropMng.Data`): Entity Framework Core context, persistence logic, and database access
3. **Services Layer** (`GropMng.Services`): Business logic and application services
4. **Presentation Layer** (`GropMng.Web`): MVC controllers, views, static assets, and UI composition
5. **Tests Layer** (`GropMng.Tests`): Test coverage for critical application behavior

All application layers are composed around the web project, with dependency injection configured in `Presentation/GropMng.Web/Program.cs`.

## Notes

- The current product scope is plant care management for gardens and balconies.
