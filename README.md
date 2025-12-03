# FitnessApp - Subscription-Based Fitness Platform

A modern, production-ready fitness web application built with **ASP.NET Core 8**, featuring subscription management, live/recorded classes, and Stripe payment integration.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
FitnessApp/
├── src/
│   ├── FitnessApp.Domain/          # Core business entities & interfaces
│   │   ├── Entities/               # Domain models
│   │   └── Interfaces/             # Repository & service interfaces
│   │
│   ├── FitnessApp.Infrastructure/  # Data access & external services
│   │   ├── Data/                   # EF Core DbContext & configurations
│   │   ├── Repositories/           # Repository implementations
│   │   └── Services/               # External service implementations (Stripe)
│   │
│   └── FitnessApp.API/             # Web API presentation layer
│       ├── Controllers/            # API endpoints
│       └── DTOs/                   # Data transfer objects
│
└── FitnessApp.sln                  # Solution file
```

## 🚀 Features

### User Management
- ✅ User registration & authentication (JWT)
- ✅ Role-based authorization (User, Instructor, Admin)
- ✅ User profile management
- ✅ ASP.NET Core Identity integration

### Subscription System
- ✅ Multiple subscription plans (Basic, Premium, Pro)
- ✅ Stripe payment integration
- ✅ Subscription management (purchase, cancel, upgrade)
- ✅ Webhook handling for payment events
- ✅ Automatic subscription status updates

### Fitness Classes
- ✅ Live classes with scheduling
- ✅ Recorded/on-demand classes
- ✅ Category-based filtering (Yoga, HIIT, Strength, etc.)
- ✅ Difficulty levels
- ✅ Instructor management

### Booking System
- ✅ Class booking with subscription validation
- ✅ Booking limits based on plan
- ✅ Booking history
- ✅ Cancellation support

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8
- **ORM**: Entity Framework Core 8
- **Authentication**: ASP.NET Core Identity + JWT
- **Payment**: Stripe API
- **Database**: SQL Server (LocalDB for development)
- **API Documentation**: Swagger/OpenAPI

## 📋 Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Stripe account (for payment integration)

## 🚀 Getting Started

### 1. Clone and restore packages

```bash
cd "C:\Users\LOHITH MS\Downloads\C# project"
dotnet restore
```

### 2. Configure the database

Update the connection string in `src/FitnessApp.API/appsettings.Development.json` if needed.

### 3. Configure Stripe (Optional for development)

1. Create a [Stripe account](https://dashboard.stripe.com/register)
2. Get your test API keys from the Stripe Dashboard
3. Update `appsettings.Development.json`:
   ```json
   "Stripe": {
     "SecretKey": "sk_test_your_key",
     "PublishableKey": "pk_test_your_key",
     "WebhookSecret": "whsec_your_secret"
   }
   ```

### 4. Run database migrations

```bash
cd src/FitnessApp.API
dotnet ef migrations add InitialCreate --project ../FitnessApp.Infrastructure
dotnet ef database update --project ../FitnessApp.Infrastructure
```

### 5. Run the application

```bash
dotnet run --project src/FitnessApp.API
```

The API will be available at:
- **Swagger UI**: https://localhost:5001/swagger
- **API Base URL**: https://localhost:5001/api

## 📚 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login and get JWT token |
| GET | `/api/auth/me` | Get current user profile |

### Subscriptions
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/subscriptions/plans` | Get all subscription plans |
| GET | `/api/subscriptions/my-subscription` | Get current subscription |
| POST | `/api/subscriptions/checkout` | Create Stripe checkout session |
| POST | `/api/subscriptions/cancel` | Cancel subscription |

### Classes
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/classes/live` | Get upcoming live classes |
| GET | `/api/classes/recorded` | Get recorded classes |
| GET | `/api/classes/{id}` | Get class details |
| POST | `/api/classes` | Create class (Instructor) |

### Bookings
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/bookings` | Get user's bookings |
| POST | `/api/bookings` | Book a class |
| DELETE | `/api/bookings/{id}` | Cancel booking |

## 🔐 Security

- JWT-based authentication
- Password hashing with ASP.NET Identity
- Role-based authorization
- Secure payment handling via Stripe (no card data stored)
- HTTPS required in production

## 📈 Next Steps (Roadmap)

1. **Sprint 3-4**: Add live streaming integration (WebRTC/Video service)
2. **Sprint 5**: Enhanced user dashboard with progress tracking
3. **Sprint 6**: Email notifications and reminders
4. **Sprint 7**: Mobile-responsive frontend (React/Angular/Blazor)
5. **Sprint 8**: Analytics and reporting for instructors

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 License

This project is for educational purposes.

---

Built with ❤️ using ASP.NET Core
