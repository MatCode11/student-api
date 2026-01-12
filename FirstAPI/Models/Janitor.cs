using System.ComponentModel.DataAnnotations;

namespace FirstAPI.Models
{
    public class Janitor
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

        [MaxLength(100)]
        public string AssignedArea { get; set; }

        [MaxLength(50)]
        public string Shift { get; set; } // Morning / Evening / Night

        public DateTime DateHired { get; set; } = DateTime.UtcNow;
    }
}
