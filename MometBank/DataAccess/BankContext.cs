using Microsoft.EntityFrameworkCore;
using MometBank.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MometBank.DataAccess
{
    public class BankContext : DbContext
    {
        public DbSet<Model> Models { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ModelTag> ModelTags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=bank.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModelTag>()
                .HasKey(mt => new { mt.ModelId, mt.TagId });

            modelBuilder.Entity<ModelTag>()
                .HasOne(mt => mt.Model)
                .WithMany(m => m.ModelTags)
                .HasForeignKey(mt => mt.ModelId);

            modelBuilder.Entity<ModelTag>()
                .HasOne(mt => mt.Tag)
                .WithMany(t => t.ModelTags)
                .HasForeignKey(mt => mt.TagId);
        }
    }
}
