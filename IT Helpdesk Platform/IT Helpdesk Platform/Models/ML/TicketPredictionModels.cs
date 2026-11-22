using Microsoft.ML.Data;

namespace IT_Helpdesk_Platform.Models.ML
{
    // Input training data for ML.NET multiclass classification
    public class TicketTrainingData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Dept { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }

    // Internal prediction output schema for individual ML.NET models
    public class SingleFieldPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedValue { get; set; } = string.Empty;

        public float[] Score { get; set; } = Array.Empty<float>();
    }

    // Unified prediction result returned by TicketPredictionService
    public class TicketPredictionOutput
    {
        public string PredictedCategory { get; set; } = string.Empty;
        public string PredictedDepartment { get; set; } = string.Empty;
        public string PredictedPriority { get; set; } = string.Empty;
        public double ConfidencePercentage { get; set; }
        public float[] Score { get; set; } = Array.Empty<float>();
    }

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
