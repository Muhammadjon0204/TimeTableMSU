using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AttendanceConfiguration: IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        
        builder.ToTable("attendances");
        
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        
        builder.Property(a => a.Day).HasColumnType("smallint").IsRequired();
        builder.Property(a => a.Para).HasColumnType("smallint").IsRequired();
        
        builder.Property(a => a.Mark)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired(false);
        
        
        builder.HasOne(a => a.Student)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(a => a.Week)
            .WithMany(w => w.Attendances)
            .HasForeignKey(a => a.WeekId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
