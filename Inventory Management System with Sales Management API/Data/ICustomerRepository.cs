using Inventory_Management_System_with_Sales_Management_API.Models;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerModel>> GetAllAsync();
        Task<CustomerModel> GetByIdAsync(int id);
        Task<int> InsertAsync(CustomerModel customer);
        Task<bool> UpdateAsync(CustomerModel customer);
        Task<bool> DeleteAsync(int customerId, int deletedBy);
    }
}
