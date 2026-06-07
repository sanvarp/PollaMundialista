using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.Property(t => t.Code).IsRequired().HasMaxLength(3);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(60);
        builder.Property(t => t.GroupName).IsRequired().HasMaxLength(2);

        builder.HasIndex(t => t.Code).IsUnique();
    }
}