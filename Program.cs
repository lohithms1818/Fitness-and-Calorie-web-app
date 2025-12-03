using System.Text;
using FitnessApp.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Infrastructure services (DbContext, Identity, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSubscription", policy =>
        policy.RequireAuthenticatedUser());
    
    options.AddPolicy("RequireInstructor", policy =>
        policy.RequireRole("Instructor", "Admin"));
    
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FitnessApp API",
        Version = "v1",
        Description = "A comprehensive fitness application API with subscription management and live classes"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Global exception handler
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = ex.Message, stackTrace = ex.StackTrace });
    }
});

// Configure the HTTP request pipeline - enable swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FitnessApp API v1");
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Serve static files (HTML, CSS, JS)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations and seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    // Create database if it doesn't exist
    var context = services.GetRequiredService<FitnessApp.Infrastructure.Data.ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    // Seed roles
    var roleManager = services
        .GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
    
    string[] roles = { "Admin", "Instructor", "User" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(role));
        }
    }
    
    // Seed initial subscription plans
    var unitOfWork = services.GetRequiredService<FitnessApp.Domain.Interfaces.IUnitOfWork>();
    var existingPlans = await unitOfWork.SubscriptionPlans.GetAllAsync();
    
    if (!existingPlans.Any())
    {
        var plans = new[]
        {
            new FitnessApp.Domain.Entities.SubscriptionPlan
            {
                Name = "Basic",
                Description = "Access to recorded classes and basic features",
                Price = 499m,
                DurationInDays = 30,
                MaxClassBookingsPerMonth = 10,
                IncludesLiveClasses = false,
                IncludesRecordedClasses = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new FitnessApp.Domain.Entities.SubscriptionPlan
            {
                Name = "Premium",
                Description = "Unlimited access to live and recorded classes",
                Price = 999m,
                DurationInDays = 30,
                MaxClassBookingsPerMonth = 0, // Unlimited
                IncludesLiveClasses = true,
                IncludesRecordedClasses = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new FitnessApp.Domain.Entities.SubscriptionPlan
            {
                Name = "Pro",
                Description = "Premium features plus personal training sessions",
                Price = 1499m,
                DurationInDays = 30,
                MaxClassBookingsPerMonth = 0, // Unlimited
                IncludesLiveClasses = true,
                IncludesRecordedClasses = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        foreach (var plan in plans)
        {
            await unitOfWork.SubscriptionPlans.AddAsync(plan);
        }
        await unitOfWork.SaveChangesAsync();
    }
    
    // Seed sample fitness classes
    var existingClasses = await unitOfWork.FitnessClasses.GetAllAsync();
    if (!existingClasses.Any())
    {
        var fitnessClasses = new[]
        {
            new FitnessApp.Domain.Entities.FitnessClass
            {
                Title = "Morning HIIT Blast",
                Description = "High-intensity interval training to kickstart your day with energy and burn maximum calories.",
                ClassType = FitnessApp.Domain.Entities.ClassType.Live,
                Category = FitnessApp.Domain.Entities.ClassCategory.HIIT,
                Difficulty = FitnessApp.Domain.Entities.DifficultyLevel.Intermediate,
                DurationMinutes = 45,
                MaxParticipants = 30,
                ScheduledAt = DateTime.UtcNow.AddDays(1).AddHours(7),
                IsLive = true,
                InstructorName = "Coach Mike",
                CreatedAt = DateTime.UtcNow
            },
            new FitnessApp.Domain.Entities.FitnessClass
            {
                Title = "Yoga Flow & Relaxation",
                Description = "A calming yoga session focusing on flexibility, balance, and mindfulness meditation.",
                ClassType = FitnessApp.Domain.Entities.ClassType.Live,
                Category = FitnessApp.Domain.Entities.ClassCategory.Yoga,
                Difficulty = FitnessApp.Domain.Entities.DifficultyLevel.AllLevels,
                DurationMinutes = 60,
                MaxParticipants = 25,
                ScheduledAt = DateTime.UtcNow.AddDays(1).AddHours(9),
                IsLive = true,
                InstructorName = "Sarah Chen",
                CreatedAt = DateTime.UtcNow
            },
            new FitnessApp.Domain.Entities.FitnessClass
            {
                Title = "Power Strength Training",
                Description = "Build muscle and increase strength with targeted weight training exercises.",
                ClassType = FitnessApp.Domain.Entities.ClassType.Live,
                Category = FitnessApp.Domain.Entities.ClassCategory.Strength,
                Difficulty = FitnessApp.Domain.Entities.DifficultyLevel.Intermediate,
                DurationMinutes = 50,
                MaxParticipants = 20,
                ScheduledAt = DateTime.UtcNow.AddDays(2).AddHours(18),
                IsLive = true,
                InstructorName = "Coach Marcus",
                CreatedAt = DateTime.UtcNow
            },
            new FitnessApp.Domain.Entities.FitnessClass
            {
                Title = "Cardio Dance Party",
                Description = "Fun dance workout that doesn't feel like exercise! Great music, great moves, great results.",
                ClassType = FitnessApp.Domain.Entities.ClassType.Recorded,
                Category = FitnessApp.Domain.Entities.ClassCategory.Dance,
                Difficulty = FitnessApp.Domain.Entities.DifficultyLevel.Beginner,
                DurationMinutes = 45,
                MaxParticipants = 40,
                ScheduledAt = DateTime.UtcNow.AddDays(2).AddHours(12),
                IsLive = false,
                InstructorName = "Jessica Taylor",
                CreatedAt = DateTime.UtcNow
            },
            new FitnessApp.Domain.Entities.FitnessClass
            {
                Title = "Core Pilates Foundations",
                Description = "Strengthen your core and improve posture with fundamental pilates exercises.",
                ClassType = FitnessApp.Domain.Entities.ClassType.Live,
                Category = FitnessApp.Domain.Entities.ClassCategory.Pilates,
                Difficulty = FitnessApp.Domain.Entities.DifficultyLevel.Beginner,
                DurationMinutes = 40,
                MaxParticipants = 20,
                ScheduledAt = DateTime.UtcNow.AddDays(3).AddHours(10),
                IsLive = true,
                InstructorName = "Emma Wilson",
                CreatedAt = DateTime.UtcNow
            },
            new FitnessApp.Domain.Entities.FitnessClass
            {
                Title = "Spin Cycle Challenge",
                Description = "High-energy indoor cycling class with hill climbs, sprints, and endurance training.",
                ClassType = FitnessApp.Domain.Entities.ClassType.Live,
                Category = FitnessApp.Domain.Entities.ClassCategory.Cycling,
                Difficulty = FitnessApp.Domain.Entities.DifficultyLevel.Advanced,
                DurationMinutes = 45,
                MaxParticipants = 25,
                ScheduledAt = DateTime.UtcNow.AddDays(3).AddHours(17),
                IsLive = true,
                InstructorName = "Coach David",
                CreatedAt = DateTime.UtcNow
            },
            new FitnessApp.Domain.Entities.FitnessClass
            {
                Title = "Beginner's Full Body Workout",
                Description = "Perfect for fitness newcomers. Learn proper form and build a solid foundation.",
                ClassType = FitnessApp.Domain.Entities.ClassType.Recorded,
                Category = FitnessApp.Domain.Entities.ClassCategory.Strength,
                Difficulty = FitnessApp.Domain.Entities.DifficultyLevel.Beginner,
                DurationMinutes = 35,
                MaxParticipants = 15,
                ScheduledAt = DateTime.UtcNow.AddDays(4).AddHours(11),
                IsLive = false,
                InstructorName = "Lisa Johnson",
                CreatedAt = DateTime.UtcNow
            },
            new FitnessApp.Domain.Entities.FitnessClass
            {
                Title = "Advanced Boxing Conditioning",
                Description = "Boxing-inspired workout combining cardio, strength, and agility training.",
                ClassType = FitnessApp.Domain.Entities.ClassType.Live,
                Category = FitnessApp.Domain.Entities.ClassCategory.Boxing,
                Difficulty = FitnessApp.Domain.Entities.DifficultyLevel.Advanced,
                DurationMinutes = 55,
                MaxParticipants = 20,
                ScheduledAt = DateTime.UtcNow.AddDays(5).AddHours(19),
                IsLive = true,
                InstructorName = "Coach Tony",
                CreatedAt = DateTime.UtcNow
            }
        };
        
        foreach (var fitnessClass in fitnessClasses)
        {
            await unitOfWork.FitnessClasses.AddAsync(fitnessClass);
        }
        await unitOfWork.SaveChangesAsync();
    }
}

app.Run();
