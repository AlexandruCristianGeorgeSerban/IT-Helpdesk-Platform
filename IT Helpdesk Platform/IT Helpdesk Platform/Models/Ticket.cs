using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_Helpdesk_Platform.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string? Title { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string? Status { get; set; } // Open, In Progress, Resolved, Closed

        [Required]
        [StringLength(20)]
        public string? Priority { get; set; } // Low, Medium, High, Critical

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public int? RequesterId { get; set; }
        [ForeignKey("RequesterId")]
        public User? Requester { get; set; }

        public int? AssignedAgentId { get; set; }
        [ForeignKey("AssignedAgentId")]
        public User? AssignedAgent { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
