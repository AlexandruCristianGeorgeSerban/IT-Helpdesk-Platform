using IT_Helpdesk_Platform.Models;
using System.Collections.Generic;

namespace IT_Helpdesk_Platform.Services.Interfaces
{
    public interface IDepartmentService
    {
        IEnumerable<Department> GetAllDepartments(string? searchString = null);
        Department? GetDepartmentById(int id);
        void CreateDepartment(Department department);
        void UpdateDepartment(Department department);
        void DeleteDepartment(int id);
        bool DepartmentExists(int id);
    }
}