using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_Helpdesk_Platform.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Required]
        [StringLength(20)]
        public string? Role { get; set; }

        public bool EmailNotificationsEnabled { get; set; } = false;
        public bool PushNotificationsEnabled { get; set; } = false;

        [StringLength(255)]
        public string? ProfilePicturePath { get; set; }

        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        [InverseProperty("Requester")]
        public ICollection<Ticket> SubmittedTickets { get; set; } = new List<Ticket>();

        [InverseProperty("AssignedAgent")]
        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    }
}