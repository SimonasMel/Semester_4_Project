using BackEnd.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace BackEnd.Data
{
    public class CarDbContext : IdentityDbContext<ApplicationUser>
    {
        public CarDbContext(DbContextOptions<CarDbContext> options) : base(options) { }

        public DbSet<Car> Cars { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<CarLike> CarLikes { get; set; }
        public DbSet<MutualMatch> MutualMatches { get; set; }

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

            modelBuilder.Entity<UserPreferences>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.HasIndex(e => e.UserId).IsUnique(); // one preferences row per user
            });

            modelBuilder.Entity<CarLike>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LikerUserId).IsRequired();
                entity.Property(e => e.LikedCarId).IsRequired();
                entity.Property(e => e.LikedCarOwnerId).IsRequired();
                entity.HasIndex(e => new { e.LikerUserId, e.LikedCarId }).IsUnique();
            });

            modelBuilder.Entity<MutualMatch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CurrentUserId).IsRequired();
                entity.Property(e => e.MatchedUserId).IsRequired();
                entity.Property(e => e.CurrentUserCarId).IsRequired();
                entity.Property(e => e.MatchedUserCarId).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => new { e.CurrentUserId, e.MatchedUserId, e.CurrentUserCarId, e.MatchedUserCarId }).IsUnique();
            });
        }
    }
}