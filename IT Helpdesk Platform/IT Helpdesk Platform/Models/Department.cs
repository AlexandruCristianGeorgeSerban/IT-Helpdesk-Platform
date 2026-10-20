using System.ComponentModel.DataAnnotations;

namespace IT_Helpdesk_Platform.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
