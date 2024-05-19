using System.Drawing.Printing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Models;
using ShopRepository.Services;

namespace ShopRepository.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController(IShopRepo repo, IConfiguration config, CognitoService cognito) : ControllerBase
    {
        [HttpPost("CustomerLogin")]
        public async Task<IActionResult> Login([FromBody] CustomerInput customer)
        {
            try
            {
                var session = await cognito.Login(customer.Email, customer.Pass);
                return Ok(new
                {
                    Email = customer.Email,
                    Token = session.IdToken,
                    AccessToken = session.AccessToken,
                    RefreshToken = session.RefreshToken
                });
            }
            catch (Exception e)
            {
                return Unauthorized(e.Message);
            }
        }
        
        //TODO This needs to be modified to use the CognitoService
        [Authorize(Policy = "CustomerOnly")]
        [HttpGet("ValidateCustomer")]
        public async Task<IActionResult> CheckLogin()
        {
            var customerClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerClaim == null) return Unauthorized("We made it into the check login method!, this is the find user claim error!");

            var customerId = customerClaim.Value;
            var customer = await repo.GetCustomer(Guid.Parse(customerId));
            Console.WriteLine("We got into the check login method!");
            if (customer == null) return Unauthorized("Invalid Account");

            var returnCustomer = new CustomerSession
            {
                Email = customer.Email,
                Fname = customer.FirstName,
                Lname = customer.LastName,
                Id = customer.Id.ToString(),
            };
            return Ok(returnCustomer);
        }
    }
}