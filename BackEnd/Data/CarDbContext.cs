using BackEnd.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace BackEnd.Data
{
    public class CarDbContext : IdentityDbContext<ApplicationUser>
    {
        public CarDbContext(DbContextOptions<CarDbContext> options) : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Brand).IsRequired();
                entity.Property(e => e.Model).IsRequired();
                entity.Property(e => e.Location).IsRequired();
                entity.Property(e => e.ContactInfo).IsRequired(false);
                entity.Property(e => e.PrimaryImagePath).IsRequired();
                entity.Property(e => e.VIN).IsRequired(false);
            });
        }
    }
}
