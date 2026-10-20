using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Services.Interfaces;

namespace IT_Helpdesk_Platform.Controllers
{
    public class KnowledgeArticlesController : Controller
    {
        private readonly IKnowledgeBaseService _knowledgeArticleService;

        public KnowledgeArticlesController(IKnowledgeBaseService knowledgeArticleService)
        {
            _knowledgeArticleService = knowledgeArticleService;
        }

        public async Task<IActionResult> Index(string? searchString, string? categoryFilter)
        {
            ViewData["CurrentSearch"] = searchString;
            ViewData["CategoryFilter"] = categoryFilter;

            var articles = await _knowledgeArticleService.GetFilteredArticlesAsync(searchString, categoryFilter);
            return View(articles);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var article = await _knowledgeArticleService.GetArticleByIdAsync(id.Value);
            return article == null ? NotFound() : View(article);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Category,IconClass,ColorClass")] KnowledgeArticle knowledgeArticle)
        {
            if (ModelState.IsValid)
            {
                await _knowledgeArticleService.CreateArticleAsync(knowledgeArticle);
                return RedirectToAction(nameof(Index));
            }
            return View(knowledgeArticle);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var article = await _knowledgeArticleService.GetArticleByIdAsync(id.Value);
            return article == null ? NotFound() : View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Category,IconClass,ColorClass")] KnowledgeArticle knowledgeArticle)
        {
            if (id != knowledgeArticle.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _knowledgeArticleService.UpdateArticleAsync(knowledgeArticle);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_knowledgeArticleService.ArticleExists(knowledgeArticle.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(knowledgeArticle);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var article = await _knowledgeArticleService.GetArticleByIdAsync(id.Value);
            return article == null ? NotFound() : View(article);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _knowledgeArticleService.DeleteArticleAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}