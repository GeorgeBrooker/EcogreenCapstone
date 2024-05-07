using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Models;

namespace ShopRepository.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController(IShopRepo repo, IConfiguration config) : ControllerBase
    {
        [HttpPost("CustomerLogin")]
        public async Task<IActionResult> Login([FromBody] CustomerInput customer)
        {
            var authCustomer = await repo.ValidLogin(customer.Email, customer.Pass);
            if (authCustomer == null) return Unauthorized("Invalid username or password");
            
            var tokenString = GenerateJsonWebToken(authCustomer);
            return Ok(new { token = tokenString });
        }
        
        [Authorize(Policy = "CustomerOnly")]
        [HttpGet("ValidateCustomer")]
        public async Task<IActionResult> CheckLogin()
        {
            var customerClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerClaim == null) return Unauthorized("Invalid token");

            var customerId = customerClaim.Value;
            var customer = await repo.GetCustomer(Guid.Parse(customerId));
            if (customer == null) return Unauthorized("Invalid Account");

            var returnCustomer = new CustomerSession
            {
                Email = customer.Email,
                Fname = customer.FirstName,
                Lname = customer.FirstName,
                Id = customer.Id.ToString(),
            };
            return Ok(returnCustomer);
        }
        
        private string GenerateJsonWebToken(Customer customer)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, customer.Email),
                new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Typ, "Customer")
            };
            var token = new JwtSecurityToken(
                config["Jwt:Issuer"],
                config["jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(14),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}