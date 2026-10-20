using System.ComponentModel.DataAnnotations;

namespace IT_Helpdesk_Platform.Models
{
    public class KnowledgeArticle
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The Article Title is required.")]
        [StringLength(255)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "The Content field cannot be empty.")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "Please select a Category.")]
        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? IconClass { get; set; }

        [StringLength(50)]
        public string? ColorClass { get; set; }
    }
}