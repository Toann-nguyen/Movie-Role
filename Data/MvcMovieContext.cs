using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie.Data
{
    public class MvcMovieContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public MvcMovieContext(DbContextOptions<MvcMovieContext> options)
            : base(options)
        {
        }

        public DbSet<MvcMovie.Models.Movie> Movie { get; set; } = default!;
        public DbSet<User> User { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            if (!modelBuilder.Model.GetEntityTypes().Any(t => t.Name == "Movie"))
            {
                modelBuilder.Entity<Movie>().ToTable("Movie");
            }
            // if (!modelBuilder.Model.GetEntityTypes().Any(t => t.Name == "Product"))
            // {
            //     // modelBuilder.Entity<Move>().ToTable("Product");
            // }

        }

    }
    
}
