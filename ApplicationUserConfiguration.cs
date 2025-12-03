using FitnessApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessApp.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.ProfilePictureUrl)
            .HasMaxLength(500);
            
        builder.Property(x => x.Bio)
            .HasMaxLength(1000);
            
        builder.Property(x => x.StripeCustomerId)
            .HasMaxLength(100);
            
        builder.Property(x => x.Specializations)
            .HasMaxLength(1000);
            
        builder.Property(x => x.Certifications)
            .HasMaxLength(1000);

        // Ignore computed property
        builder.Ignore(x => x.FullName);

        // Indexes
        builder.HasIndex(x => x.StripeCustomerId);
    }
}
