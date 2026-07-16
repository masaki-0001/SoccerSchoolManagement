using Microsoft.EntityFrameworkCore;
using SoccerSchoolManagement.Models;

namespace SoccerSchoolManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }

    public DbSet<SoccerClass> Classes { get; set; }

    public DbSet<StudentClass> StudentClasses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StudentClass>()
            .HasOne(studentClass => studentClass.Student)
            .WithMany(student => student.StudentClasses)
            .HasForeignKey(studentClass => studentClass.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentClass>()
            .HasOne(studentClass => studentClass.SoccerClass)
            .WithMany(soccerClass => soccerClass.StudentClasses)
            .HasForeignKey(studentClass => studentClass.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentClass>()
            .HasIndex(studentClass => new
            {
                studentClass.StudentId,
                studentClass.ClassId
            })
            .HasDatabaseName("IX_StudentClasses_CurrentMembership")
            .IsUnique()
            .HasFilter("\"EndDate\" IS NULL AND \"IsDeleted\" = 0");
    }
}