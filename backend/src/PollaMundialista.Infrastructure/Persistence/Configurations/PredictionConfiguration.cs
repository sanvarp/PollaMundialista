using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PollaMundialista.Domain.Entities;
using PollaMundialista.Infrastructure.Identity;

namespace PollaMundialista.Infrastructure.Persistence.Configurations;

public class PredictionConfiguration : IEntityTypeConfiguration<Prediction>
{
    public void Configure(EntityTypeBuilder<Prediction> builder)
    {
        builder.Property(p => p.UserId).IsRequired();

        builder
            .HasOne(p => p.Match)
            .WithMany(m => m.Predictions)
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to the Identity user without a navigation on the Domain side.
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One prediction per (user, match).
        builder.HasIndex(p => new { p.UserId, p.MatchId }).IsUnique();
    }
}