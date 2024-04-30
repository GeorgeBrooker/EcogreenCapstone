using ShopRepository.Dtos;
using ShopRepository.Models;

namespace ShopRepository.Data;


// NOTE: There are two kinds of item ID in the database. id represents the UUID assigned by us to objects.
// ({class}Id -> orderId) represents the UUID of the stripe object associated with the object in the database.
// This is why id is stored as GUID while {class}Id is stored as a string. (and also why the associated queries are different)
public interface IShopRepo
{
    // Orders
    Task<Order?> GetOrder(Guid id);
    Task<Order?> GetOrderFromPaymentId(string paymentIntentId);
    Task<IEnumerable<Order>?> GetAllOrders(int limit);
    Task<bool> AddOrder(Order order);
    Task<bool> UpdateOrder(Order? order);
    Task<bool> DeleteOrder(string orderId);

    // Customers
    Task<Customer?> GetCustomerFromStripe(string customerId);
    Task<Customer?> GetCustomer(Guid id);
    Task<Customer?> GetCustomerFromEmail(string email);
    Task<IEnumerable<Order>?> GetCustomerOrders(string customerId);
    Task<IEnumerable<Customer>> GetAllCustomers(int limit);
    Task<bool> AddCustomer(CustomerInput customer);
    Task<bool> UpdateCustomer(Customer customer);
    Task<bool> DeleteCustomer(Guid customerId);

    // Stock
    Task<Stock?> GetStockFromStripe(string stockId);
    Task<Stock?> GetStock(Guid id);
    Task<IEnumerable<Stock>?> GetAllStock(int limit);
    Task<bool> AddStock(Stock stock);
    Task<bool> UpdateStock(Stock stock);
    Task<bool> DeleteStock(string stockId);
}