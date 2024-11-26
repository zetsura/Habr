using Habr.DataAccess.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Habr.DataAccess.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(p => p.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            entityTypeBuilder
                .Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            entityTypeBuilder
                .Property(p => p.Text)
                .IsRequired()
                .HasMaxLength(4000);

            entityTypeBuilder
                .Property(p => p.Created)
                .IsRequired()
                .HasColumnType("datetime");

            entityTypeBuilder
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entityTypeBuilder
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}