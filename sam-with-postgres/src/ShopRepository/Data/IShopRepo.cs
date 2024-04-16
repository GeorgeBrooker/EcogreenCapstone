using ShopRepository.Models;

namespace ShopRepository.Data;

public interface IShopRepo
{
    // Orders
    Task<Order> GetOrderAsync(int orderId);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task AddOrderAsync(Order order);
    Task UpdateOrderAsync(Order order);
    Task DeleteOrderAsync(int orderId);

    // Customers
    Task<Customer> GetCustomerAsync(int customerId);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task AddCustomerAsync(Customer customer);
    Task UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(int customerId);

    // Photos
    Task<Photo> GetPhotoAsync(int photoId);
    Task<IEnumerable<Photo>> GetAllPhotosAsync();
    Task AddPhotoAsync(Photo photo);
    Task UpdatePhotoAsync(Photo photo);
    Task DeletePhotoAsync(int photoId);

    // Stock
    Task<Stock> GetStockAsync(int stockId);
    Task<IEnumerable<Stock>> GetAllStockAsync();
    Task AddStockAsync(Stock stock);
    Task UpdateStockAsync(Stock stock);
    Task DeleteStockAsync(int stockId);

}