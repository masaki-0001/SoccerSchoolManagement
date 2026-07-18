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

    public DbSet<Lesson> Lessons { get; set; }

    public DbSet<Attendance> Attendances { get; set; }

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

        modelBuilder.Entity<Lesson>()
            .HasOne(lesson => lesson.SoccerClass)
            .WithMany(soccerClass => soccerClass.Lessons)
            .HasForeignKey(lesson => lesson.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lesson>()
            .HasIndex(lesson => new
            {
                lesson.ClassId,
                lesson.LessonDate,
                lesson.StartTime
            })
            .HasDatabaseName("IX_Lessons_ClassDateStartTime")
            .IsUnique()
            .HasFilter("\"IsDeleted\" = 0");

        modelBuilder.Entity<Attendance>()
            .HasOne(attendance => attendance.Lesson)
            .WithMany(lesson => lesson.Attendances)
            .HasForeignKey(attendance => attendance.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Attendance>()
            .HasOne(attendance => attendance.Student)
            .WithMany(student => student.Attendances)
            .HasForeignKey(attendance => attendance.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Attendance>()
            .HasIndex(attendance => new
            {
                attendance.LessonId,
                attendance.StudentId
            })
            .HasDatabaseName("IX_Attendances_LessonStudent")
            .IsUnique()
            .HasFilter("\"IsDeleted\" = 0");
    }
}