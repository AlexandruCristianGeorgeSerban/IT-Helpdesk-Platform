using System.ComponentModel.DataAnnotations;

namespace IT_Helpdesk_Platform.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
