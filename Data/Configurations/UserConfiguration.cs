
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvcMovie.Models;


namespace MvcMovie.Data;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
        builder.Property(x => x.Status).HasDefaultValue(UserStatus.Active);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
    }
}