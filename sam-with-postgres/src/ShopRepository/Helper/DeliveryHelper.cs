using Amazon.DynamoDBv2.DataModel;
using ShopRepository.Dtos;
using ShopRepository.Models;
using ShopRepository.Services;

namespace ShopRepository.Helper
{
    public static class DeliveryHelper
    {
        private static readonly NzPostService _nzPostService = new NzPostService();

        // Generate Delivery Label for an Order
        public static async Task<string> GenerateDeliveryLabelAsync(OrderInput orderInput, IDynamoDBContext dbContext)
        {
            // Retrieve Customer details from DynamoDB using CustomerId
            var customer = await dbContext.LoadAsync<Customer>(new Guid(orderInput.CustomerId));
            if (customer == null)
            {
                throw new Exception($"Customer with ID {orderInput.CustomerId} not found");
            }

            // Use NZ Post API to generate a delivery label
            var deliveryLabel = await _nzPostService.GenerateDeliveryLabelAsync(orderInput.PaymentId, orderInput.CustomerId);

            // Save delivery label to Order in DynamoDB
            var order = await dbContext.LoadAsync<Order>(new Guid(orderInput.PaymentId));
            if (order == null)
            {
                throw new Exception($"Order with PaymentIntentId {orderInput.PaymentId} not found");
            }

            order.DeliveryLabelUid = deliveryLabel;
            await dbContext.SaveAsync(order);

            return deliveryLabel;
        }

        // Track Order status using NZ Post
        public static async Task<string> TrackOrderAsync(string trackingId)
        {
            return await _nzPostService.TrackOrderAsync(trackingId);
        }

        // Estimate Delivery Date based on current date and shipping method
        public static async Task<DateTime> EstimateDeliveryDateAsync(DateTime orderDate, string shippingMethod)
        {
            return await _nzPostService.EstimateDeliveryDateAsync(shippingMethod);
        }
    }
}
