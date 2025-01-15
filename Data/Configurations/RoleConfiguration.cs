using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvcMovie.Models;

namespace MvcMovie.Data;

// Data/Configurations/RolePermissionConfiguration.cs
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        // Cấu hình khóa chính kết hợp
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Cấu hình mối quan hệ với Role
        builder.HasOne(rp => rp.Role)
            .WithMany()
            .HasForeignKey(rp => rp.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Cấu hình mối quan hệ với Permission
        builder.HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Cấu hình các thuộc tính
        builder.Property(x => x.RoleId)
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.PermissionId)
            .HasMaxLength(50)
            .IsUnicode(false);
    }
}