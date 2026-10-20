using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_Helpdesk_Platform.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        public Ticket? Ticket { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<Attachment>? Attachments { get; set; }
    }
}
