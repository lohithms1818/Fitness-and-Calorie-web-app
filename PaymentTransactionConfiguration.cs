using FitnessApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessApp.Infrastructure.Data.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);
            
        builder.Property(x => x.Amount)
            .HasPrecision(10, 2);
            
        builder.Property(x => x.RefundAmount)
            .HasPrecision(10, 2);
            
        builder.Property(x => x.Currency)
            .HasMaxLength(10);
            
        builder.Property(x => x.Description)
            .HasMaxLength(500);
            
        builder.Property(x => x.StripePaymentIntentId)
            .HasMaxLength(100);
            
        builder.Property(x => x.StripeChargeId)
            .HasMaxLength(100);
            
        builder.Property(x => x.StripeInvoiceId)
            .HasMaxLength(100);
            
        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);
            
        builder.Property(x => x.Status)
            .HasConversion<string>();
            
        builder.Property(x => x.Type)
            .HasConversion<string>();

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Subscription)
            .WithMany()
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.StripePaymentIntentId);
        builder.HasIndex(x => x.UserId);
    }
}
