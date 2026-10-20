using IT_Helpdesk_Platform.Models;
using System.Threading.Tasks;

namespace IT_Helpdesk_Platform.Repositories.Interfaces
{
    public interface IKnowledgeBaseRepository : IRepositoryBase<KnowledgeArticle>
    {
        Task<KnowledgeArticle?> GetArticleByIdAsync(int id);
        bool ArticleExists(int id);
    }
}