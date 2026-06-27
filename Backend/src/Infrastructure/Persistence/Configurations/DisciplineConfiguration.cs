using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DisciplineConfiguration:IEntityTypeConfiguration<Discipline>
{
    public void Configure(EntityTypeBuilder<Discipline> builder)
    {
        builder.ToTable("disciplines");
        
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        
        builder.Property(d => d.LectureHourCount)
            .HasDefaultValue(0);
        builder.Property(d => d.PracticeHourCount)
            .HasDefaultValue(0);
        builder.Property(d => d.LaboratoryHourCount)
            .HasDefaultValue(0);
        builder.Property(d => d.OtherHourCount)
            .HasDefaultValue(0);
        
        builder.HasOne(d => d.Subject)
            .WithMany(s => s.Disciplines)
            .HasForeignKey(d => d.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(d => d.Teacher)
            .WithMany(t => t.Disciplines)
            .HasForeignKey(d => d.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(d => d.Group)
            .WithMany(g => g.Disciplines)
            .HasForeignKey(d => d.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}