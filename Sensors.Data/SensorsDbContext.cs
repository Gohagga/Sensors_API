using Microsoft.EntityFrameworkCore;
using Sensors.Data.Entities;

namespace Sensors.Data
{
    public class SensorsDbContext : DbContext
    {
        public SensorsDbContext(
            DbContextOptions<SensorsDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DeviceConfiguration());
            modelBuilder.ApplyConfiguration(new CounterConfiguration());

            modelBuilder.Entity<CounterQuery>().HasNoKey();
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Counter> Counters { get; set; }
        public DbSet<CounterQuery> CounterQuery { get; set; }

    }

    public class CounterQuery
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public DateTime CountDateTime { get; set; }
    }
}
