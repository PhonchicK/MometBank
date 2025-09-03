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
        public DbSet<Folder> Folders { get; set; }
        public DbSet<FolderTag> FolderTags { get; set; }
        public DbSet<Gcode> Gcodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=bank.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model>()
                .HasOne(m => m.Folder)
                .WithMany(f => f.Models)
                .HasForeignKey(m => m.FolderId);

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

            modelBuilder.Entity<FolderTag>()
                .HasKey(ft => new { ft.FolderId, ft.TagId });

            modelBuilder.Entity<FolderTag>()
                .HasOne(ft => ft.Folder)
                .WithMany(f => f.FolderTags)
                .HasForeignKey(ft => ft.FolderId);

            modelBuilder.Entity<FolderTag>()
                .HasOne(ft => ft.Tag)
                .WithMany(t => t.FolderTags)
                .HasForeignKey(ft => ft.TagId);

            modelBuilder.Entity<Gcode>()
                .HasOne(g => g.Model)
                .WithMany(m => m.Gcodes)
                .HasForeignKey(g => g.ModelId);

            modelBuilder.Entity<Gcode>()
                .HasOne(g => g.Folder)
                .WithMany(f => f.Gcodes)
                .HasForeignKey(g => g.FolderId);
        }
    }
}
