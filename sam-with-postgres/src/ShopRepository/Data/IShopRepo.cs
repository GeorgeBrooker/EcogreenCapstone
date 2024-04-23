using ShopRepository.Models;

namespace ShopRepository.Data;

public interface IShopRepo
{
    // Orders
    Task<Order?> GetOrder(int orderId);
    Task<Order?> GetOrderFromPaymentId(string paymentIntentId);
    Task<IEnumerable<Order>?> GetAllOrders(int limit);
    Task<bool> AddOrder(Order order);
    Task<bool> UpdateOrder(Order? order);
    Task<bool> DeleteOrder(int orderId);

    // Customers
    Task<Customer?> GetCustomer(string customerId);
    Task<IEnumerable<Order>?> GetCustomerOrders(string customerId);
    Task<IEnumerable<Customer>?> GetAllCustomers(int limit);
    Task<bool> AddCustomer(Customer customer);
    Task<bool> UpdateCustomer(Customer customer);
    Task<bool> DeleteCustomer(int customerId);

    // Stock
    Task<Stock?> GetStock(string stockId);
    Task<IEnumerable<Stock>?> GetAllStock(int limit);
    Task<bool> AddStock(Stock stock);
    Task<bool> UpdateStock(Stock stock);
    Task<bool> DeleteStock(int stockId);
}