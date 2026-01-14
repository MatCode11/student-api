using System.ComponentModel.DataAnnotations;

namespace FirstAPI.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Subject { get; set; }
    }
}
