using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models;

namespace SOAP.Web.Data.Configurations
{
    public class SchoolConfiguration : IEntityTypeConfiguration<School>
    {
        public void Configure(EntityTypeBuilder<School> builder)
        {
            builder.HasKey(s => s.Id);
            
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(s => s.Code)
                .IsRequired()
                .HasMaxLength(10);
            
            builder.Property(s => s.County)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(s => s.Code)
                .IsUnique();
            
            builder.HasIndex(s => s.County);
        }
    }
}