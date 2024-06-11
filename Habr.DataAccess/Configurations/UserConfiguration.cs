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
                .HasMaxLength(100);

            entityTypeBuilder
                .HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entityTypeBuilder
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
        }
    }
}
