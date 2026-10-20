using IT_Helpdesk_Platform.Models;
using System.Collections.Generic;

namespace IT_Helpdesk_Platform.Services.Interfaces
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories(string? searchString = null);
        Category? GetCategoryById(int id);
        void CreateCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(int id);
        bool CategoryExists(int id);
    }
}