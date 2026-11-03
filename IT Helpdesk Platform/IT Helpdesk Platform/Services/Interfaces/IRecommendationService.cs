using IT_Helpdesk_Platform.Models.ML;

namespace IT_Helpdesk_Platform.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<ArticleRecommendationDto>> GetRecommendedArticlesAsync(string text, int topK = 3);
        Task<List<SimilarTicketDto>> GetSimilarResolvedTicketsAsync(int ticketId, int topK = 3, int? requesterId = null);
    }
}

