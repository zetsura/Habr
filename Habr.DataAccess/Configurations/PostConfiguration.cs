using Habr.DataAccess.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Habr.DataAccess.ApplicationConstants;

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
                .HasMaxLength(Constants.MaxTitleLength);

            entityTypeBuilder
                .Property(p => p.Text)
                .IsRequired()
                .HasMaxLength(Constants.MaxTextLength);

            entityTypeBuilder
                .Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            entityTypeBuilder
                .Property(p => p.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            entityTypeBuilder
                .Property(p => p.PublishedDate)
                .HasColumnType("datetime");

            entityTypeBuilder
                .Property(p => p.IsPublished)
                .IsRequired()
                .HasDefaultValue(false);

            entityTypeBuilder
                .Property(p => p.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

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