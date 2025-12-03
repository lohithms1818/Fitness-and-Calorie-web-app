using FitnessApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessApp.Infrastructure.Data.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Description)
            .HasMaxLength(500);
            
        builder.Property(x => x.Price)
            .HasPrecision(10, 2);
            
        builder.Property(x => x.StripePriceId)
            .HasMaxLength(100);
            
        builder.Property(x => x.StripeProductId)
            .HasMaxLength(100);

        // Seed initial subscription plans (prices in INR)
        builder.HasData(
            new SubscriptionPlan
            {
                Id = 1,
                Name = "Basic",
                Description = "Access to recorded fitness classes and basic features",
                Price = 499m,
                DurationInDays = 30,
                IncludesLiveClasses = false,
                IncludesRecordedClasses = true,
                MaxClassBookingsPerMonth = 10,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SubscriptionPlan
            {
                Id = 2,
                Name = "Premium",
                Description = "Unlimited access to live and recorded classes",
                Price = 999m,
                DurationInDays = 30,
                IncludesLiveClasses = true,
                IncludesRecordedClasses = true,
                MaxClassBookingsPerMonth = 0, // Unlimited
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SubscriptionPlan
            {
                Id = 3,
                Name = "Pro",
                Description = "Premium features plus personal training sessions",
                Price = 1499m,
                DurationInDays = 30,
                IncludesLiveClasses = true,
                IncludesRecordedClasses = true,
                MaxClassBookingsPerMonth = 0, // Unlimited
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
