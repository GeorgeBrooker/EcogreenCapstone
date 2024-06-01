using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ShopRepository.Services;
using Newtonsoft.Json.Linq;

namespace ShopRepository.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly NZPostService _nzPostService;
        private readonly ILogger<AddressController> _logger;

        public AddressController(NZPostService nzPostService, ILogger<AddressController> logger)
        {
            _nzPostService = nzPostService;
            _logger = logger;
        }

        [HttpGet("search-addresses")]
        public async Task<IActionResult> SearchAddresses([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query parameter is required");
            }

            try
            {
                _logger.LogInformation("Attempting to get access token.");
                var accessToken = await _nzPostService.GetAccessTokenAsync();
                _logger.LogInformation("Access token retrieved successfully.");

                _logger.LogInformation("Searching addresses with query: {Query}", query);
                var result = await _nzPostService.SearchAddressesAsync(accessToken, query);

                _logger.LogInformation("Addresses search successful.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching addresses.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("create-pickup-booking")]
        public async Task<IActionResult> CreatePickupBooking([FromBody] JObject bookingRequest)
        {
            if (bookingRequest == null)
            {
                return BadRequest("Booking request cannot be null.");
            }

            try
            {
                _logger.LogInformation("Attempting to get access token.");
                var accessToken = await _nzPostService.GetAccessTokenAsync();
                _logger.LogInformation("Access token retrieved successfully.");

                _logger.LogInformation("Creating pickup booking.");
                var result = await _nzPostService.CreatePickupBookingAsync(accessToken, bookingRequest);

                _logger.LogInformation("Pickup booking created successfully.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating pickup booking.");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}


