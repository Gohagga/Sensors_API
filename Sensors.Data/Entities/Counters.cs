using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensors.Data.Entities
{
    public class Counter
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public double Count { get; set; } // float in SQL is mapped to double in C#
        public bool IsRunning { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime? Deleted { get; set; }

        // Navigation property for related Device
        public Device Device { get; set; }
    }

    public class CounterConfiguration : IEntityTypeConfiguration<Counter>
    {
        public void Configure(EntityTypeBuilder<Counter> builder)
        {
            builder.ToTable("Counters", "dbo");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("Id").IsRequired();
            builder.Property(c => c.DeviceId).HasColumnName("DeviceId").IsRequired();
            builder.Property(c => c.Count).HasColumnName("Count").IsRequired();
            builder.Property(c => c.IsRunning).HasColumnName("IsRunning").IsRequired();
            builder.Property(c => c.Created).HasColumnName("Created").IsRequired().HasColumnType("datetime2");
            builder.Property(c => c.Modified).HasColumnName("Modified").HasColumnType("datetime2");
            builder.Property(c => c.Deleted).HasColumnName("Deleted").HasColumnType("datetime2");

            // Foreign key relationship
            builder.HasOne(c => c.Device)
                   .WithMany(d => d.Counters)
                   .HasForeignKey(c => c.DeviceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
