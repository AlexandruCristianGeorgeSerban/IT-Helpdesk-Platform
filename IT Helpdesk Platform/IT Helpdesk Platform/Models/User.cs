using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace IT_Helpdesk_Platform.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        [Required]
        [StringLength(20)]
        public string? Role { get; set; } // e.g., "Employee", "IT Agent"

        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        [InverseProperty("Requester")]
        public ICollection<Ticket> SubmittedTickets { get; set; } = new List<Ticket>();

        [InverseProperty("AssignedAgent")]
        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    }
}
