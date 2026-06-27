using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AcademicPerformanceConfiguration : IEntityTypeConfiguration<AcademicPerformance>
{
    public void Configure(EntityTypeBuilder<AcademicPerformance> builder)
    {
        builder.ToTable("academic_performance");
        
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        
        builder.Property(ap => ap.Tur)
            .HasColumnType("smallint");
        
        builder.Property(ap => ap.Mark)
            .HasColumnType("smallint");
        
        builder.Property(ap => ap.ControlForm)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(ap => ap.Student)
            .WithMany(s => s.AcademicPerformances)
            .HasForeignKey(ap => ap.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ap => ap.Discipline)
            .WithMany(d => d.AcademicPerformances)
            .HasForeignKey(ap => ap.DisciplineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ap => ap.Teacher)
            .WithMany(t => t.AcademicPerformances)
            .HasForeignKey(ap => ap.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}