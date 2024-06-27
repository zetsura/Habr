using Habr.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Habr.DataAccess.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(u => u.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            entityTypeBuilder
                .Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            entityTypeBuilder
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);

            entityTypeBuilder
                .HasIndex(u => u.Email)
                .IsUnique();

            entityTypeBuilder
                .Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(100);

            entityTypeBuilder
                .Property(u => u.RegisteredDate)
                .IsRequired()
                .HasColumnType("datetime");

            entityTypeBuilder
                .Property(u => u.EmailConfirmed)
                .IsRequired()
                .HasDefaultValue(false);
        }
    }
}
