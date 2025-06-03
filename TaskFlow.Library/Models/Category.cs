namespace TaskFlow.Library.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int? UserId { get; set; } // Nullable for global categories
        public string CategoryName { get; set; } = string.Empty;
    }
}