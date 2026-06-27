using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("schedules"); 

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.Den).IsRequired().HasColumnType("smallint");
        builder.Property(s => s.Para).IsRequired().HasColumnType("smallint");
        builder.Property(s => s.WeekId).IsRequired();
        
        builder.Property(s => s.LectureType)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.HasOne(s => s.Discipline)
            .WithMany(d => d.Schedules)
            .HasForeignKey(s => s.DisciplineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Teacher)
            .WithMany(t => t.Schedules)
            .HasForeignKey(s => s.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Audience)
            .WithMany(a => a.Schedules)
            .HasForeignKey(s => s.AudienceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Group)
            .WithMany(g => g.Schedules)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Week)
            .WithMany(w => w.Schedules)
            .HasForeignKey(s => s.WeekId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
