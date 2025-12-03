using FitnessApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessApp.Infrastructure.Data.Configurations;

public class FitnessClassConfiguration : IEntityTypeConfiguration<FitnessClass>
{
    public void Configure(EntityTypeBuilder<FitnessClass> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.Description)
            .HasMaxLength(2000);
            
        builder.Property(x => x.ThumbnailUrl)
            .HasMaxLength(500);
            
        builder.Property(x => x.VideoUrl)
            .HasMaxLength(500);
            
        builder.Property(x => x.StreamUrl)
            .HasMaxLength(500);
            
        builder.Property(x => x.MeetingId)
            .HasMaxLength(100);
            
        builder.Property(x => x.InstructorId)
            .HasMaxLength(450);
        
        builder.Property(x => x.InstructorName)
            .HasMaxLength(200);
            
        builder.Property(x => x.ClassType)
            .HasConversion<string>();
            
        builder.Property(x => x.Category)
            .HasConversion<string>();
            
        builder.Property(x => x.Difficulty)
            .HasConversion<string>();

        // Relationships
        builder.HasOne(x => x.Instructor)
            .WithMany(u => u.InstructedClasses)
            .HasForeignKey(x => x.InstructorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.MinimumPlan)
            .WithMany()
            .HasForeignKey(x => x.MinimumPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.ScheduledAt);
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.InstructorId);
    }
}
