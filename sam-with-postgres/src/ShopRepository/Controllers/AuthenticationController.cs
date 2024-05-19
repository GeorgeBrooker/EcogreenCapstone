using System.Drawing.Printing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        
        [Authorize(Policy = "CustomerOnly")]
        [HttpGet("ValidateCustomer")]
        public async Task<IActionResult> CheckLogin()
        {
            
            var userSubClaim = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
            var accessToken = (User.Identity as ClaimsIdentity)?.BootstrapContext?.ToString();
            
            var cognitoUser = await cognito.GetUser(userSubClaim, accessToken);
            if (cognitoUser == null) return Unauthorized("Cannot find user in cognito pool");
            if (!cognitoUser.Attributes.TryGetValue("email", out var email)) return Unauthorized("Malformed user");
            
            // TODO GET CUSTOMER FROM REPO
            var customer = await repo.GetCustomerFromEmail(email);
            if (customer == null) return Unauthorized("User not found in database");

            var returnCustomer = new CustomerSession
            {
                Email = customer.Email,
                Fname = customer.FirstName,
                Lname = customer.LastName,
                Id = customer.Id.ToString(),
            };
            return Ok(returnCustomer);
        }
        
        [HttpPost("RegisterCustomer")]
        public async Task<IActionResult> Register([FromBody] CustomerInput customer)
        {
            Console.WriteLine("We out here");
            if (await repo.GetCustomerFromEmail(customer.Email) != null)
                return BadRequest("Customer with that email already exists.");
            try
            {
                var localSuccess = await repo.AddCustomer(customer);
                if (!localSuccess) BadRequest("Failed to add use to local database");
                
                var success = await cognito.Register(customer.Email, customer.Pass, customer.Fname, customer.Lname);
                if (!success) throw new Exception("Failed to register user in cognito");
                
                return Ok("User registered successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var cInDb = await repo.GetCustomerFromEmail(customer.Email);
                if (cInDb != null) 
                    await repo.DeleteCustomer(cInDb.Id);
                
                return BadRequest("Sign up failed, please try again.");
            }
        }
    }
}