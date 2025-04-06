using FileManager.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileManager.DB;

public class FileManagerDbContext : DbContext
{
    public DbSet<FileRecord> FileRecords { get; set; }

    public FileManagerDbContext(DbContextOptions<FileManagerDbContext> options) : base(options) 
    { 
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
