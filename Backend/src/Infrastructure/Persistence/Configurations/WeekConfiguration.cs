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

        builder.Property(w => w.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(Domain.Enums.WeekType.Study);

        builder.Property(w => w.IsCurrent)
            .IsRequired();

        builder.HasOne(w => w.AcademicYear)
            .WithMany(x => x.Weeks)
            .HasForeignKey(w => w.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.AcademicPeriod)
            .WithMany(x => x.Weeks)
            .HasForeignKey(w => w.AcademicPeriodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(w => new { w.AcademicYearId, w.StartDate, w.EndDate });

        builder.HasIndex(w => new { w.AcademicYearId, w.Name })
            .IsUnique();
    }
}
