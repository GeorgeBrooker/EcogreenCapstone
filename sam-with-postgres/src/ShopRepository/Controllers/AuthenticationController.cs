using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using ShopRepository.Data;
using ShopRepository.Models;

namespace ShopRepository.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IShopRepo _repo;
        private readonly string _secretKey;

        public AuthenticationController(IShopRepo repo)
        {
            _repo = repo;
            // TODO replace with AWS secrets manager
            _secretKey = Environment.GetEnvironmentVariable("SECRET_COOKIE_KEY")!;
        }

        [HttpPost("GenerateCookie/{customerId:guid}")]
        public IActionResult GenerateCookie(Guid customerId)
        {
            SetCookie(Response, customerId, _secretKey);
            return Ok();
        }

        [HttpGet("GetCustomerFromCookie")]
        public async Task<ActionResult<Customer>> GetCustomerFromCookie()
        {
            var customer = await _repo.GetCustomerFromCookie(Request, _secretKey);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpPost("VerifyLogin")]
        public async Task<ActionResult<bool>> VerifyLogin(string username, string password)
        {
            return Ok(await _repo.ValidLogin(username, password));
        }

        private static void SetCookie(HttpResponse response, Guid customerId, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var customerIdBytes = Encoding.UTF8.GetBytes(customerId.ToString());
            var hash = hmac.ComputeHash(customerIdBytes);
            var hashString = Convert.ToBase64String(hash);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            response.Cookies.Append("CustomerId", customerId.ToString(), cookieOptions);
            response.Cookies.Append("CustomerHash", hashString, cookieOptions);
        }
    }
}