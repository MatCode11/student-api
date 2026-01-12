using System.ComponentModel.DataAnnotations;

namespace FirstAPI.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Subject { get; set; }

        public DateTime DateHired { get; set; } = DateTime.UtcNow;
    }
}
