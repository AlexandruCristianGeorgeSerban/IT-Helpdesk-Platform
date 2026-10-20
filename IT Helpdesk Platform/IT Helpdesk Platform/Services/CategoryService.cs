using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace IT_Helpdesk_Platform.Services
{
    public class CategoryService : Interfaces.ICategoryService
    {
        private readonly IRepositoryWrapper _wrapper;

        public CategoryService(IRepositoryWrapper wrapper) => _wrapper = wrapper;

        public IEnumerable<Category> GetAllCategories(string? searchString = null)
        {
            var query = _wrapper.Category.FindAll();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Name!.Contains(searchString) || c.Id.ToString() == searchString);
            }

            return query.OrderBy(c => c.Name).ToList();
        }

        public Category? GetCategoryById(int id) =>
            _wrapper.Category.FindByCondition(c => c.Id == id).FirstOrDefault();

        public void CreateCategory(Category category)
        {
            _wrapper.Category.Create(category);
            _wrapper.Save();
        }

        public void UpdateCategory(Category category)
        {
            _wrapper.Category.Update(category);
            _wrapper.Save();
        }

        public void DeleteCategory(int id)
        {
            var category = GetCategoryById(id);
            if (category != null)
            {
                _wrapper.Category.Delete(category);
                _wrapper.Save();
            }
        }

        public bool CategoryExists(int id) =>
            _wrapper.Category.FindByCondition(c => c.Id == id).Any();
    }
}