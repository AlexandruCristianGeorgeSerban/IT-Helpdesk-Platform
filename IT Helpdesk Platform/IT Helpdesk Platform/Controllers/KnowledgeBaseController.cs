using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IT_Helpdesk_Platform.Controllers
{
    public class KnowledgeBaseController : Controller
    {
        private readonly IKnowledgeBaseService _knowledgeBaseService;

        public KnowledgeBaseController(IKnowledgeBaseService knowledgeBaseService)
        {
            _knowledgeBaseService = knowledgeBaseService;
        }

        public async Task<IActionResult> Index(string searchString, string category)
        {
            var articles = await _knowledgeBaseService.GetFilteredArticlesAsync(searchString, category);

            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentCategory"] = category;

            return View(articles);
        }
    }
}