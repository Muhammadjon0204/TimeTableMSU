using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SpecialityConfiguration : IEntityTypeConfiguration<Speciality>
{
    public void Configure(EntityTypeBuilder<Speciality> builder)
    {
        builder.ToTable("specialities");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        
        builder.Property(p => p.Name)
        .IsRequired()
        .HasMaxLength(200);
        
        builder.HasMany(s => s.Groups)
            .WithOne(g => g.Speciality)
            .HasForeignKey(g => g.SpecialityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}