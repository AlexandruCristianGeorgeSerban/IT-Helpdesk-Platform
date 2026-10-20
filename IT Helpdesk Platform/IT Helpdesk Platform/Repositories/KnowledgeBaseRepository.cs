using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace IT_Helpdesk_Platform.Repositories
{
    public class KnowledgeBaseRepository : RepositoryBase<KnowledgeArticle>, IKnowledgeBaseRepository
    {
        public KnowledgeBaseRepository(HelpdeskContext context) : base(context) { }

        public async Task<KnowledgeArticle?> GetArticleByIdAsync(int id) =>
            await FindByCondition(a => a.Id == id).FirstOrDefaultAsync();

        public bool ArticleExists(int id) => FindByCondition(a => a.Id == id).Any();
    }
}