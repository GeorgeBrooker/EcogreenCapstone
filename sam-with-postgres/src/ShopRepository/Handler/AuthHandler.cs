using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ShopRepository.Data;


namespace ShopRepository.Handler;

public class AuthHandler(
    IShopRepo repo,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TimeProvider time)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly TimeProvider _time = time;

    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            Response.Headers.WWWAuthenticate = "Basic";
                
            return AuthenticateResult.Fail("Authorization header not found");
        }
            
        var authHeader = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
        var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(":");
        var userEmail = credentials[0];
        var password = credentials[1];
        
        
        if (await repo.ValidLogin(userEmail, password))
        {
            var claims = new[] { new Claim("userEmail", userEmail), new Claim("customer", userEmail) };
            var identity = new ClaimsIdentity(claims, "Basic");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
                
            return AuthenticateResult.Success(ticket);
        }
            
        return AuthenticateResult.Fail("Invalid login");
    }
}