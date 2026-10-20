using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace IT_Helpdesk_Platform.Services
{
    public class DepartmentService : Interfaces.IDepartmentService
    {
        private readonly IRepositoryWrapper _wrapper;

        public DepartmentService(IRepositoryWrapper wrapper) => _wrapper = wrapper;

        public IEnumerable<Department> GetAllDepartments(string? searchString = null)
        {
            var query = _wrapper.Department.FindAll();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.Name!.Contains(searchString) || d.Id.ToString() == searchString);
            }

            return query.OrderBy(d => d.Name).ToList();
        }

        public Department? GetDepartmentById(int id) =>
            _wrapper.Department.FindByCondition(d => d.Id == id).FirstOrDefault();

        public void CreateDepartment(Department department)
        {
            _wrapper.Department.Create(department);
            _wrapper.Save();
        }

        public void UpdateDepartment(Department department)
        {
            _wrapper.Department.Update(department);
            _wrapper.Save();
        }

        public void DeleteDepartment(int id)
        {
            var department = GetDepartmentById(id);
            if (department != null)
            {
                _wrapper.Department.Delete(department);
                _wrapper.Save();
            }
        }

        public bool DepartmentExists(int id) =>
            _wrapper.Department.FindByCondition(d => d.Id == id).Any();
    }
}