using IT_Helpdesk_Platform.Models;

namespace IT_Helpdesk_Platform.Services.Interfaces
{
    public interface IKnowledgeBaseService
    {
        Task<IEnumerable<KnowledgeArticle>> GetAllArticlesAsync();
        Task<KnowledgeArticle?> GetArticleByIdAsync(int id);
        Task CreateArticleAsync(KnowledgeArticle article);
        Task UpdateArticleAsync(KnowledgeArticle article);
        Task DeleteArticleAsync(int id);
        bool ArticleExists(int id);
        Task<IEnumerable<KnowledgeArticle>> GetFilteredArticlesAsync(string? searchString, string? category);
    }
}