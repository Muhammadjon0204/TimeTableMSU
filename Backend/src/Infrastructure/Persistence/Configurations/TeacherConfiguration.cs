using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TeacherConfiguration:IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");
        
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();
        
        builder.Property(t => t.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(t => t.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.FatherName)
            .HasMaxLength(50);
        
        builder.Property(t => t.Telephone)
            .HasMaxLength(20);
        builder.Property(t => t.Address)
            .HasMaxLength(200);
        builder.Property(t => t.Email)
            .HasMaxLength(100);
        
        
        builder.Property(t => t.TeacherDegree)
            .HasMaxLength(50)
            .HasConversion<string>();
        
        builder.Property(t => t.TeacherTitle)
            .HasMaxLength(50)
            .HasConversion<string>();
        
        builder.Property(t => t.TeacherPost)
            .HasMaxLength(50)
            .HasConversion<string>();
        
        
    }
}