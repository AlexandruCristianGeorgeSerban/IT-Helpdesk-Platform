using System.ComponentModel.DataAnnotations;

namespace IT_Helpdesk_Platform.Models
{
    public class SubmitTicketViewModel
    {
        [Required(ErrorMessage = "Please provide a title.")]
        [StringLength(200)]
        public string Title { get; set; } = default!;

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        [Required]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Please provide a description.")]
        public string Description { get; set; } = default!;
    }
}