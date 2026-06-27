using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");
        
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Semester)
            .IsRequired()
            .HasColumnType("smallint");
        
        builder.Property(s => s.HourCount)
            .IsRequired();
        
        builder.Property(s => s.ControlForm)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

    }
}
