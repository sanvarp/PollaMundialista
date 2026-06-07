using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.Property(m => m.GroupName).IsRequired().HasMaxLength(2);
        builder.Property(m => m.Stage).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Status).HasConversion<int>();

        builder.Ignore(m => m.HasResult);

        builder
            .HasOne(m => m.HomeTeam)
            .WithMany(t => t.HomeMatches)
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(m => m.AwayTeam)
            .WithMany(t => t.AwayMatches)
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.KickoffUtc);
    }
}