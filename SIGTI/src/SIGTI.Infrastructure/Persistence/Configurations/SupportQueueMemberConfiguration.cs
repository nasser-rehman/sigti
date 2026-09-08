using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGTI.Domain.Entities;

namespace SIGTI.Infrastructure.Persistence.Configurations
{
    public class SupportQueueMemberConfiguration
        : IEntityTypeConfiguration<SupportQueueMember>
    {
        public void Configure(EntityTypeBuilder<SupportQueueMember> builder)
        {
            builder.ToTable("SupportQueueMembers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.IsActive).IsRequired();

            builder.Property(x => x.JoinedAt).IsRequired();

            builder.Property(x => x.LeftAt);

            builder.Property(x => x.MaxConcurrentTickets).IsRequired();

            builder.Property(x => x.SupportQueueId).IsRequired();

            builder.Property(x => x.TechnicianId).IsRequired();

            builder
                .HasOne(x => x.SupportQueue)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.SupportQueueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Technician)
                .WithMany(x => x.QueueMemberships)
                .HasForeignKey(x => x.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.IsActive);
            builder
                .HasIndex(x => new { x.SupportQueueId, x.TechnicianId })
                .IsUnique();
        }
    }
}
