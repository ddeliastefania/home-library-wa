using HomeLibrary.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeLibrary.Persistence.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(
        DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("library");

            entity.HasKey(book => book.Id);

            entity.Property(book => book.Id)
                .HasColumnName("id");

            entity.Property(book => book.Name)
                .HasColumnName("name")
                .IsRequired();

            entity.Property(book => book.Author)
                .HasColumnName("author")
                .IsRequired();

            entity.Property(book => book.Genre)
                .HasColumnName("genre")
                .IsRequired();

            entity.Property(book => book.ImportDate)
                .HasColumnName("import_date")
                .IsRequired();
        });
    }
}
