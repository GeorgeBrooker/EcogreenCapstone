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
    Task<IEnumerable<Order>> GetAllOrders(int limit);
    Task<IEnumerable<StockRequest>> GetOrderStock(Guid orderId);
    Task<Guid?> AddOrder(OrderInput order);
    Task<bool> UpdateOrder(Order order);
    Task<bool> DeleteOrder(Guid id);

    // Customers
    Task<Customer?> GetCustomerFromStripe(string customerId);
    Task<Customer?> GetCustomer(Guid id);

    Task<Customer?> GetCustomerFromEmail(string email);

    // Secret key is used to sign the cookie.
    // It should be a secret string that is not stored in the database or hard coded.
    // We will store this in AWS secrets manager during deployment.
    Task<Customer?> ValidLogin(string email, string password);
    Task<IEnumerable<Order>?> GetCustomerOrders(Guid id);
    Task<IEnumerable<Customer>> GetAllCustomers(int limit);
    Task<bool> AddCustomer(CustomerInput customer);
    Task<bool> UpdateCustomer(Customer customer);
    Task<bool> DeleteCustomer(Guid customerId);

    // Customer Addresses
    Task<Address?> GetCustomerAddress(Guid customerId, string addressName);
    Task<IEnumerable<Address>> GetCustomerAddresses(Guid customerId);
    Task<bool> AddCustomerAddress(Address address);
    Task<bool> UpdateCustomerAddress(Address address);
    Task<bool> DeleteCustomerAddress(Address address);

    // Stock
    Task<Stock?> GetStockFromStripe(string stockId);
    Task<Stock?> GetStock(Guid id);
    Task<IEnumerable<Stock>> GetAllStock(int limit);
    Task<Guid?> AddStock(StockInput stock);
    Task<bool> UpdateStock(Stock? stock);
    Task<bool> DeleteStock(Guid id);

    // StockRequest

    // This is not the proper way to query StockRequest. Stock requests should always be retrieved via the parent order.
    Task<StockRequest?> GetStockRequest(Guid orderId, Guid stockId);
    Task<bool> AddStockRequest(StockRequest stock);
    Task<bool> UpdateStockRequest(StockRequest stock);
    Task<bool> DeleteStockRequest(StockRequest stock);
}