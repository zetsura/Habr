using Habr.DataAccess.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Habr.DataAccess.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> entityTypeBuilder)
        {
            entityTypeBuilder.Property(p => p.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            entityTypeBuilder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            entityTypeBuilder.Property(p => p.Text)
                .IsRequired()
                .HasMaxLength(4000);

            entityTypeBuilder.Property(p => p.Created)
                .IsRequired()
                .HasColumnType("datetime");
        }
    }
}