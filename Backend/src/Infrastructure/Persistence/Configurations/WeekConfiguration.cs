using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WeekConfiguration:IEntityTypeConfiguration<Weeks>
{
    public void Configure(EntityTypeBuilder<Weeks> builder)
    {
        builder.ToTable("weeks");
        
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(w => w.StartDate)
            .IsRequired()
            .HasColumnType("date");
        
        builder.Property(w => w.EndDate)
            .IsRequired()
            .HasColumnType("date");
    }
}