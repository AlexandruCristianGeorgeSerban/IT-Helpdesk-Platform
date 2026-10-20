using IT_Helpdesk_Platform.Models;

namespace IT_Helpdesk_Platform.Repositories.Interfaces
{
    public interface IKnowledgeBaseRepository : IRepositoryBase<KnowledgeArticle>
    {
        Task<KnowledgeArticle?> GetArticleByIdAsync(int id);
        bool ArticleExists(int id);
    }
}