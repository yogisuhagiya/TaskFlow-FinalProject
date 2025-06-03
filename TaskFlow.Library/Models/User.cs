namespace TaskFlow.Library.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // For light security
        public string? Email { get; set; }
    }
}