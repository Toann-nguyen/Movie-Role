using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    // Bạn có thể thêm các thuộc tính bổ sung ở đây nếu cần.
    public string? Status { get; set; }
}
