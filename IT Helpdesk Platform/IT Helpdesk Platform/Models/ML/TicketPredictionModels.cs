namespace IT_Helpdesk_Platform.Models.ML
{
    // DTO returned by RecommendationService for live TF-IDF suggestions in SubmitTicket.cshtml
    public class ArticleRecommendationDto
    {
        public int ArticleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Similarity { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    // DTO for the "Similar Resolved Tickets" panel in ViewTicket.cshtml
    public class SimilarTicketDto
    {
        public int TicketId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Similarity { get; set; }
    }
}
