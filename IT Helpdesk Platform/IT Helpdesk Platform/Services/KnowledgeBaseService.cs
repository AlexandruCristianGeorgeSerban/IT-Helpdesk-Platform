using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IT_Helpdesk_Platform.Services
{
    public class KnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly IRepositoryWrapper _wrapper;
        public KnowledgeBaseService(IRepositoryWrapper wrapper) => _wrapper = wrapper;

        public async Task<IEnumerable<KnowledgeArticle>> GetAllArticlesAsync() =>
            await _wrapper.KnowledgeBase.FindAll().ToListAsync();

        public async Task<KnowledgeArticle?> GetArticleByIdAsync(int id) =>
            await _wrapper.KnowledgeBase.GetArticleByIdAsync(id);

        public async Task CreateArticleAsync(KnowledgeArticle article)
        {
            _wrapper.KnowledgeBase.Create(article);
            await _wrapper.SaveAsync();
        }

        public async Task UpdateArticleAsync(KnowledgeArticle article)
        {
            _wrapper.KnowledgeBase.Update(article);
            await _wrapper.SaveAsync();
        }

        public async Task DeleteArticleAsync(int id)
        {
            var article = await _wrapper.KnowledgeBase.GetArticleByIdAsync(id);
            if (article != null)
            {
                _wrapper.KnowledgeBase.Delete(article);
                await _wrapper.SaveAsync();
            }
        }

        public bool ArticleExists(int id) => _wrapper.KnowledgeBase.ArticleExists(id);

        public async Task<IEnumerable<KnowledgeArticle>> GetFilteredArticlesAsync(string? searchString, string? category)
        {
            var query = _wrapper.KnowledgeBase.FindAll();

            if (!string.IsNullOrEmpty(category))
            {
                if (category == "Hardware") query = query.Where(a => a.Category == "Hardware" || a.Category == "Gear");
                else if (category == "Software") query = query.Where(a => a.Category == "Software" || a.Category == "Accounts");
                else if (category == "Network") query = query.Where(a => a.Category == "Network" || a.Category == "Security");
                else query = query.Where(a => a.Category == category);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.Title!.Contains(searchString) || a.Content!.Contains(searchString));
            }

            return await query.ToListAsync();
        }
    }
}