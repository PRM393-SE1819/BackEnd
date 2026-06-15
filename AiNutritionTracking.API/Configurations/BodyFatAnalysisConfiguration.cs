using AiNutritionTracking.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiNutritionTracking.API.Configurations;

public class BodyFatAnalysisConfiguration : IEntityTypeConfiguration<BodyFatAnalysis>
{
    public void Configure(EntityTypeBuilder<BodyFatAnalysis> entity)
    {
        entity.HasKey(e => e.Id).HasName("BodyFatAnalysis_pkey");

        entity.ToTable("BodyFatAnalysis");

        entity.Property(e => e.EstimatedBodyFat).HasColumnType("double precision");
        entity.Property(e => e.TargetWeight).HasColumnType("double precision");
        entity.Property(e => e.Category).HasMaxLength(50);
        entity.Property(e => e.HealthAssessment).HasMaxLength(500);
        entity.Property(e => e.Recommendation).HasMaxLength(1000);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnType("timestamp without time zone");

        entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("BodyFatAnalysis_UserId_fkey");
    }
}
