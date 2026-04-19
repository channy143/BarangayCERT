namespace MauiApp1.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        public bool IsAdmin => Role == UserRole.Admin;
    }
}
