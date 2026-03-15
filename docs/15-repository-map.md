# Repository Map

## Root
- `FortuneInternalData.sln` - Visual Studio solution file
- `global.json` - target .NET SDK version
- `README.md` - project overview and handover summary
- `samples/` - sample import files
- `docs/` - implementation, architecture, QA, and handover docs

## src/FortuneInternalData.Web
Purpose: MVC entry project
- `Program.cs` - startup wiring
- `Controllers/` - page controllers
- `Views/` - Razor UI pages
- `ViewModels/` - page models
- `Security/` - policy names and authorization setup
- `Extensions/` - DI composition helpers

## src/FortuneInternalData.Application
Purpose: use cases and contracts
- `Interfaces/` - service and repository contracts
- `Services/` - application services and query services
- `DTOs/` - transport/view data models
- `Validators/` - reusable validation logic

## src/FortuneInternalData.Domain
Purpose: core business definitions
- `Entities/` - core entities
- `Constants/` - roles, statuses, and import constants

## src/FortuneInternalData.Infrastructure
Purpose: technical implementation
- `Persistence/` - DbContext
- `Repositories/` - EF Core repositories
- `Services/` - import, file storage, parsing services
- `Identity/` - identity scaffold, bootstrap, TOTP setup

## Suggested Developer Starting Points
1. Read `README.md`
2. Read `docs/02-mvp-functional-specification.md`
3. Read `docs/09-implementation-notes-for-it.md`
4. Read `docs/12-import-service-design.md`
5. Open `Program.cs`, `DependencyInjection.cs`, `ApplicationDbContext.cs`
6. Continue implementation from controllers and services
