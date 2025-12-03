<!-- Copilot Instructions for FitnessApp -->

## Project Overview
This is an ASP.NET Core 8 Web API for a fitness subscription platform following Clean Architecture.

## Architecture Guidelines
- **Domain Layer**: Contains entities and interfaces only. No external dependencies.
- **Infrastructure Layer**: Implements repositories, DbContext, and external services (Stripe).
- **API Layer**: Controllers, DTOs, and configuration. Thin controllers that delegate to services.

## Coding Standards
- Use async/await for all database and external service calls
- Use DTOs for API responses, never expose domain entities directly
- Use IUnitOfWork for transaction management
- Follow repository pattern for data access
- Use dependency injection throughout

## Key Entities
- `ApplicationUser`: Extended IdentityUser with fitness-specific properties
- `SubscriptionPlan`: Defines available subscription tiers
- `UserSubscription`: Links users to their active subscription
- `FitnessClass`: Live or recorded fitness classes
- `ClassBooking`: User bookings for classes
- `PaymentTransaction`: Payment history

## Authentication
- JWT-based authentication
- Roles: User, Instructor, Admin
- Use `[Authorize]` attribute for protected endpoints
- Access user ID via `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`

## Payment Integration
- Stripe for subscription payments
- Use webhook endpoints for payment event handling
- Never store raw card data

## Testing Endpoints
Use Swagger UI at `/swagger` to test API endpoints.
Include JWT token in Authorization header: `Bearer {token}`

## Common Commands
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run API
dotnet run --project src/FitnessApp.API

# Add migration
dotnet ef migrations add MigrationName --project src/FitnessApp.Infrastructure --startup-project src/FitnessApp.API

# Update database
dotnet ef database update --project src/FitnessApp.Infrastructure --startup-project src/FitnessApp.API
```
