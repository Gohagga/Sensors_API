using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensors.Data.Entities
{
    public class Device
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime? Deleted { get; set; }

        // Navigation property for related Counters
        public ICollection<Counter> Counters { get; set; } = new List<Counter>();
    }

    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable("Devices", "dbo");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).HasColumnName("Id").IsRequired();
            builder.Property(d => d.Code).HasColumnName("Code").IsRequired().HasColumnType("nvarchar(max)");
            builder.Property(d => d.Name).HasColumnName("Name").IsRequired().HasColumnType("nvarchar(max)");
            builder.Property(d => d.Description).HasColumnName("Description").HasColumnType("nvarchar(max)");
            builder.Property(d => d.IsDisabled).HasColumnName("IsDisabled").IsRequired();
            builder.Property(d => d.Created).HasColumnName("Created").IsRequired().HasColumnType("datetime2");
            builder.Property(d => d.Modified).HasColumnName("Modified").HasColumnType("datetime2");
            builder.Property(d => d.Deleted).HasColumnName("Deleted").HasColumnType("datetime2");
        }
    }
}
