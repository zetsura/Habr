using Habr.DataAccess.ApplicationConstants;
using Habr.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Metadata;

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
                .HasMaxLength(Constants.MaxNameLength);

            entityTypeBuilder
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(Constants.MaxEmailLength);

            entityTypeBuilder
                .HasIndex(u => u.Email)
                .IsUnique();

            entityTypeBuilder
                .Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(Constants.MaxPasswordLength);

            entityTypeBuilder
                .Property(u => u.RegisteredDate)
                .IsRequired()
                .HasColumnType("datetime");

            entityTypeBuilder
                .Property(u => u.IsEmailConfirmed)
                .IsRequired()
                .HasDefaultValue(false);
        }
    }
}
