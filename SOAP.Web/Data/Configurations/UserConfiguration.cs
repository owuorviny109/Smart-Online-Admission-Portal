using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);
            
            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique();
            
            builder.HasOne(u => u.School)
                .WithMany()
                .HasForeignKey(u => u.SchoolId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}