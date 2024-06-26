﻿using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Helper;
using ShopRepository.Models;
using ShopRepository.Services;


namespace ShopRepository.Controllers;

[Route("api/inventory")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "AdminCognitoAuth")]
public class InventoryController(IShopRepo repo, StripeService stripeService, IConfiguration config, ILogger<ShopController> logger, StockUploadHelper stockUploader) : ControllerBase
{
//
// STOCK
//
    // GET
    [HttpGet("GetAllStock")]
    public async Task<ActionResult<IEnumerable<Stock>>> GetAllStock([FromQuery] int limit = 1000)
    {
        var stock = await repo.GetAllStock(limit);
        return Ok(await repo.GetAllStock(limit));
    }
    // POST
    [HttpPost("AddStock")]
    public async Task<ActionResult<bool>> AddStock([FromBody] StockInput stock)
    {
        try
        {
            var stockId = await repo.AddStock(stock) ?? throw new Exception("Failed to add stock to database");
            if (stock.CreateWithoutStripeLink) 
                return Ok(stockId); // Don't add to stripe if the flag is set.
            
            var stripeUpload = await stripeService.PersistStockToStripe(stockId);
            if (!stripeUpload)
            {
                await repo.DeleteStock(stockId);
                throw new Exception("Failed to upload stock to stripe");
            }

            return Ok(stockId);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // PUT
    [HttpPut("UpdateStock")]
    public async Task<ActionResult<bool>> UpdateStock([FromBody] StockInput stock)
    {
        if (stock.StripeId == null)
            return BadRequest("Cannot update implicitly via stripeId when input stock has no stripeId");

        var retrievedStock = await repo.GetStockFromStripe(stock.StripeId);
        if (retrievedStock == null)
            return BadRequest($"Stock could not be found with StripeId={stock.StripeId}");

        return await UpdateStockHelper(retrievedStock, stock);
    }

    [HttpPut("UpdateStock/{stockId:guid}")]
    public async Task<ActionResult<bool>> UpdateStock(Guid stockId, [FromBody] StockInput stock)
    {
        var retrievedStock = await repo.GetStock(stockId);
        if (retrievedStock == null) return BadRequest($"Stock could not be found with GUID={stockId}");

        return await UpdateStockHelper(retrievedStock, stock);
    }
    
    // Helper method for updating stock in the database and stripe simultaneously with rollback on failure in either.
    private async Task<ActionResult<bool>> UpdateStockHelper(Stock stock, StockInput stockInput)
    {
        var backup = stock.DeepCopy(); // Backup the stock in case of rollback 
        
        //Once bound a stripeID should not be changed, this is why we don't update it here.
        stock.Name = stockInput.Name;
        stock.TotalStock = stockInput.TotalStock; 
        stock.PhotoUri = stockInput.PhotoUri;
        stock.Description = stockInput.Description;
        stock.Price = stockInput.Price;
        stock.DiscountPercentage = stockInput.DiscountPercentage;
        stock.Active = stockInput.Active;
        try
        { 
            var updateResult = await repo.UpdateStock(stock);
            if (!updateResult) throw new Exception("Failed to update stock in database");

            var stripeUpdate = await stripeService.UpdateStripeStock(stock);
            if (!stripeUpdate) throw new Exception("Failed to update stock in stripe");

            return Ok("Stock updated successfully");
        } // Rollback changes if either the database or stripe fails to update
        catch (Exception e)
        {
            logger.LogInformation("Failed to update stock. Rolling back changes...");
            var rollback = await repo.UpdateStock(backup);
            
            if (!rollback)
            {
                logger.LogError("Failed to rollback changes. Manual intervention required.");
                return BadRequest($"Failed to update stock and rollback failed. Manual intervention required. Error {e.Message}");
            }
            
            logger.LogInformation("Rollback complete");
            return BadRequest("Failed to update stock, rollback completed successfully, Please try again.");
        }
    }
    
    // DELETE / ARCHIVE
    [HttpDelete("DeleteStock/{id:guid}")]
    public async Task<ActionResult<bool>> DeleteStock(Guid id)
    {
        Stock? backupStock = null;
        try
        {
            if (Guid.Empty == id) return ValidationProblem("StockId in request is not a valid Id.");

            var retrievedStock = await repo.GetStock(id) ?? throw new ResourceNotFoundException($"No stock exists with stockId={id}");
            backupStock = retrievedStock.DeepCopy();
        
            var dbDelete = await repo.DeleteStock(id);
            if (!dbDelete) throw new Exception("Failed to delete stock from database.");
        
            var stripeDelete = await stripeService.DeleteStripeStock(retrievedStock);
            if (!stripeDelete) throw new Exception("Failed to delete stock from stripe. NOTE: once stock has been used in a transaction it cannot be deleted, archive the stock instead.");
            
            return Ok();
        }
        catch (ResourceNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            if (backupStock == null) return BadRequest($"Failed to delete stock. Error {e.Message}");
            var restore = await repo.RestoreStock(backupStock);
            return BadRequest(
                !restore ? $"Failed to delete stock and rollback failed. Manual intervention required. Error {e.Message}" 
                : $"Failed to delete stock, rollback completed successfully. Please try again. Error {e.Message}"
                );
        }
    }

    [HttpPut("SetStockArchiveState/{id:guid}")]
    public async Task<ActionResult<bool>> SetStockArchiveState(Guid id, [FromBody] Dictionary<string, bool> stateChange)
    {
        var newState = stateChange["active"];
        logger.LogInformation(newState ? $"Archiving stock with id={id}" : $"Restoring stock with id={id}");
        var stock = await repo.GetStock(id);
        if (stock == null) return NotFound($"No stock exists with id={id}");
        
        var backup = stock.DeepCopy();
        stock.Active = newState;
        
        try
        { 
            var updateResult = await repo.UpdateStock(stock);
            if (!updateResult) throw new Exception("Failed to update stock in database");

            var stripeUpdate = await stripeService.UpdateStripeStock(stock);
            if (!stripeUpdate) throw new Exception("Failed to update stock in stripe");

            return Ok("Stock updated successfully");
        } // Rollback changes if either the database or stripe fails to update
        catch (Exception e)
        {
            logger.LogInformation("Failed to update stock. Rolling back changes...");
            var rollback = await repo.UpdateStock(backup);
            
            if (!rollback)
            {
                logger.LogError("Failed to rollback changes. Manual intervention required.");
                return BadRequest($"Failed to update stock and rollback failed. Manual intervention required. Error {e.Message}");
            }
            
            logger.LogInformation("Rollback complete");
            return BadRequest("Failed to update stock, rollback completed successfully, Please try again.");
        }
    }
    
    //
    // CUSTOMER
    //
    
    // DELETE
    [HttpDelete("DeleteCustomer/{id:guid}")]
    public async Task<ActionResult> DeleteCustomer(Guid id)
    {
        if (id == Guid.Empty) return ValidationProblem("Invalid request ID");

        var customer = await repo.GetCustomer(id);
        if (customer == null)
            return NotFound($"No customer exists with id {id}");

        await repo.DeleteCustomer(customer.Id);
        return Ok();
    }
    
    //
    // ORDERS
    //

    // GET
    [HttpGet("GetOrders")]
    public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders([FromQuery] int limit = 100)
    {
        return Ok(await repo.GetAllOrders(limit));
    }

    [HttpGet("GetOrder/{id:guid}")]
    public async Task<ActionResult<Order>> GetOrderById(Guid id)
    {
        return Ok(await repo.GetOrder(id));
    }

    [HttpGet("GetOrderByStripeCheckout/{id}")]
    public async Task<ActionResult<Order>> GetOrderByPaymentId(string id)
    {
        return Ok(await repo.GetOrderFromStripe(id));
    }

    [HttpGet("GetOrderStock/{id:guid}")]
    public async Task<ActionResult<IEnumerable<StockRequest>>> GetOrderStock(Guid id)
    {
        return Ok(await repo.GetOrderStock(id));
    }

    // POST
    // TODO This will need to be modified to sync with stripe. We may want to call this internally from a different endpoint.
    [HttpPost("AddOrder")]
    public async Task<ActionResult<OrderInput>> AddOrder([FromBody] OrderInput nOrder)
    {
        if (await repo.AddOrder(nOrder) != null)
            return Ok(nOrder);

        return BadRequest();
    }

    // PUT
    // This method should only be called with a complete OrderInput DTO as input.
    // See equivalent customer method for more details
    [HttpPut("UpdateOrder/{id:guid}")]
    public async Task<ActionResult> UpdateOrder(Guid id, [FromBody] OrderInput? order)
    {
        if (Guid.Empty == id || order == null) return ValidationProblem("Invalid payload");

        var updated = await repo.GetOrder(id);
        if (updated == null) throw new Exception($"Could not find existing order with id={id}. Update canceled.");

        updated.StripeCheckoutSession = order.StripeCheckoutSession;
        updated.CustomerId = order.CustomerId;
        updated.DeliveryLabelUid = order.DeliveryLabel;
        updated.TrackingNumber = order.Tracking;
        updated.PackageReference = order.PackageRef;
        updated.CustomerAddress = order.CustomerAddress;
        updated.OrderStatus = order.OrderStatus;

        await repo.UpdateOrder(updated);
        return Ok();
    }

    // DELETE
    [HttpDelete("DeleteOrder/{id:guid}")]
    public async Task<ActionResult> DeleteOrder(Guid id)
    {
        if (Guid.Empty == id) return ValidationProblem("Invalid request ID");

        var order = await repo.GetOrder(id);
        if (order == null) return NotFound($"No order exists with id={id}");

        await repo.DeleteOrder(id);
        return Ok();
    }
    
//
// CUSTOMER ADDRESSES
//

    // GET
    [HttpGet("GetCustomerAddresses/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<Address>>> GetCustomerAddresses(Guid customerId)
    {
        return Ok(await repo.GetCustomerAddresses(customerId));
    }

    [HttpGet("GetCustomerAddress/{customerId:guid}/{addressName}")]
    public async Task<ActionResult<Address>> GetCustomerAddress(Guid customerId, string addressName)
    {
        return Ok(await repo.GetCustomerAddress(customerId, addressName));
    }

    // POST
    [HttpPost("AddCustomerAddress")]
    public async Task<ActionResult<bool>> AddCustomerAddress([FromBody] Address address)
    {
        var addressExists = await repo.GetCustomerAddress(address.CustomerId, address.AddressName);
        if (addressExists != null) return BadRequest("Address already exists.");
        return Ok(await repo.AddCustomerAddress(address));
    }

    // PUT
    [HttpPut("UpdateCustomerAddress")]
    public async Task<ActionResult<bool>> UpdateCustomerAddress([FromBody] AddressInput nAddress)
    {
        var address = await repo.GetCustomerAddress(nAddress.CustomerId, nAddress.AddressName);
        if (address == null) return BadRequest("Address does not exist.");

        // Map the input to the existing address
        address.CustomerId = nAddress.CustomerId;
        address.AddressName = nAddress.AddressName;
        address.Line1 = nAddress.StreetNumber + " " + nAddress.Street;
        address.Line2 = nAddress.State;
        address.City = nAddress.City;
        address.PostCode = nAddress.PostCode;
        address.Country = nAddress.Country;
        address.Email = nAddress.Email;

        return Ok(await repo.UpdateCustomerAddress(address));
    }

    // DELETE
    [HttpDelete("DeleteCustomerAddress/{customerId:guid}/{addressName}")]
    public async Task<ActionResult<bool>> DeleteCustomerAddress(Guid customerId, string addressName)
    {
        var address = await repo.GetCustomerAddress(customerId, addressName);
        if (address == null) return BadRequest("Address does not exist.");

        return Ok(await repo.DeleteCustomerAddress(address));
    }
    
//
// *CUSTOMER*
//

    // GET
    [HttpGet("GetCustomers")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers([FromQuery] int limit = 20)
    {
        return Ok(await repo.GetAllCustomers(limit));
    }

    [HttpGet("GetCustomerFromStripe/{stripeId}")]
    public async Task<ActionResult<Customer>> GetCustomerFromStripe(string stripeId)
    {
        return Ok(await repo.GetCustomerFromStripe(stripeId));
    }

    [HttpGet("GetCustomerFromEmail/{email}")]
    public async Task<ActionResult<Customer>> GetCustomerFromEmail(string email)
    {
        return Ok(await repo.GetCustomerFromEmail(email));
    }

    [HttpGet("GetCustomerByID/{id:guid}")]
    public async Task<ActionResult<Customer>> GetCustomer(Guid id)
    {
        return Ok(await repo.GetCustomer(id));
    }

    [HttpGet("GetCustomerOrders/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetCustomerOrders(Guid customerId)
    {
        return Ok(await repo.GetCustomerOrders(customerId));
    }

    // POST 
    [HttpPost("AddCustomer")]
    public async Task<ActionResult<CustomerInput>> AddCustomer([FromBody] CustomerInput nCustomer)
    {
        if (await repo.GetCustomerFromEmail(nCustomer.Email) != null)
            return BadRequest("Customer with that email already exists.");

        if (await repo.AddCustomer(nCustomer))
            return Ok(nCustomer);

        return BadRequest("Sign up failed, please try again.");
    }

    // PUT 

    // This method should only be called with a complete CustomerInput DTO as input.
    // Input DTO should be created from a fresh retrieval of the customer information.
    // You will have to retrieve the customer object to get ID anyway so this shouldn't be expensive.
    [HttpPut("UpdateCustomer/{id:guid}")]
    public async Task<ActionResult> UpdateCustomer(Guid id, [FromBody] CustomerInput? customer)
    {
        if (id == Guid.Empty || customer == null) return ValidationProblem("Invalid payload");

        var updated = await repo.GetCustomer(id);
        if (updated == null) return NotFound($"Could not find existing customer with id={id}. Update canceled.");

        updated.Email = customer.Email;
        updated.FirstName = customer.Fname;
        updated.LastName = customer.Fname;
        updated.Password = new PasswordHasher<Customer>().HashPassword(updated, customer.Pass);

        await repo.UpdateCustomer(updated);
        return Ok();
    }
    
//
// STOCK REQUESTS
//

    // GET
    [HttpGet("GetStockRequest/{orderId:guid}/{stockId:guid}")]
    public async Task<ActionResult<StockRequest>> GetStockRequest(Guid orderId, Guid stockId)
    {
        return Ok(await repo.GetStockRequest(orderId, stockId));
    }

    // POST

    // TESTING ONLY. StockRequest SHOULD ONLY BE CREATED BY BACKEND BUSINESS LOGIC, NOT FRONTEND REQUESTS.
    // FOR THIS REASON NO DTO IS PROVIDED
    [HttpPost("AddStockRequest")]
    public async Task<ActionResult<bool>> AddStockRequest(
        [FromQuery] Guid orderId,
        [FromQuery] Guid stockId,
        [FromQuery] int quantity)
    {
        var stockRequest = new StockRequest
        {
            OrderId = orderId,
            ProductId = stockId,
            Quantity = quantity
        };

        return Ok(await repo.AddStockRequest(stockRequest));
    }

    // PUT
    [HttpPost("UpdateStockRequest")]
    public async Task<ActionResult<bool>> UpdateStockRequest(
        [FromQuery] Guid? orderId,
        [FromQuery] Guid? stockId,
        [FromQuery] int? quantity)
    {
        if (orderId == null || stockId == null || quantity == null)
            return BadRequest("All attributes must be specified to update a stockRequest");

        var stockRq = await repo.GetStockRequest((Guid)orderId, (Guid)stockId);
        if (stockRq == null)
            return NotFound($"StockRequest for stock id={stockId} in order id={orderId} could not be found");

        stockRq.Quantity = (int)quantity;
        await repo.UpdateStockRequest(stockRq);

        return true;
    }

    // Delete
    [HttpDelete("DeleteStockRequest/{orderId:guid}/{stockId:guid}")]
    public async Task<ActionResult<bool>> DeleteStockRequest(Guid orderId, Guid stockId)
    {
        if (Guid.Empty == orderId || Guid.Empty == stockId)
            return ValidationProblem("One or more of the inputted Ids are invalid.");

        var retreivedStockRq = await repo.GetStockRequest(orderId, stockId);
        if (retreivedStockRq == null)
            return NotFound($"No request for stock id={stockId} could be found in order id={orderId}");

        await repo.DeleteStockRequest(retreivedStockRq);
        return Ok();
    }
    
    
    
//
// STOCK PHOTOS
//

    [HttpPost("UploadPhotos/{stockId:guid}")]
    public async Task<ActionResult<bool>> UploadPhotos(Guid stockId)
    {
        var form = Request.Form;
        var files = Request.Form.Keys;
        var images = new byte[files.Count][];

        var i = 0;
        foreach (var key in files)
        {
            images[i] = Convert.FromBase64String(form[key]!);
            i++;
        }

        var result = await stockUploader.UploadImages(images, stockId);

        if (result)
            return Ok();

        return BadRequest("Image upload failed");
    }
}