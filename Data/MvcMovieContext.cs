using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie.Data
{

    public class MvcMovieContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {

        public DbSet<MvcMovie.Models.Movie> Movie { get; set; } = default!;
        public DbSet<User> User { get; set; } = default!;

        public DbSet<Permission> Permissions { get; set; }

        public MvcMovieContext(DbContextOptions<MvcMovieContext> options)
            : base(options)
        {
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Gọi base configuration từ IdentityDbContext
            base.OnModelCreating(modelBuilder);
            // Cấu hình các bảng Identity
            ConfigureIdentityTables(modelBuilder);

            // Cấu hình Permission và các relationship
            ConfigurePermissionSystem(modelBuilder);

            // Áp dụng tất cả các Entity Type Configuration từ assembly hiện tại
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            if (!modelBuilder.Model.GetEntityTypes().Any(t => t.Name == "Movie"))
            {
                modelBuilder.Entity<Movie>().ToTable("Movie");
            }
            // Apply configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());

            // Configure Identity tables
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");


        }

        private void ConfigureIdentityTables(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
                b.Property(x => x.Status).HasDefaultValue(UserStatus.Active);
                b.Property(x => x.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Role>(b =>
            {
                b.ToTable("Roles");
                b.Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
            });

            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        }

        private void ConfigurePermissionSystem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>(b =>
            {
                b.ToTable("Permissions");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired().HasMaxLength(200);
                b.Property(x => x.Description).HasMaxLength(500);
                b.Property(x => x.Group).HasMaxLength(100);
            });

            modelBuilder.Entity<RolePermission>(b =>
            {
                b.ToTable("RolePermissions");
                b.HasKey(x => new { x.RoleId, x.PermissionId });
            });
        }

    }
}


