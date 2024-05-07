using System.Net.Http.Headers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;



namespace ShopRepository.Handler;

public class AuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TimeProvider time,
    IConfiguration config)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly TimeProvider _time = time;
    private readonly ILogger _logger = logger.CreateLogger<AuthHandler>();
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (config["Environment"] == "local") //Access-Control-Allow-Origin headers are only set in local environment
        {
            Console.WriteLine("Running Locally");
            Response.Headers.AccessControlAllowOrigin = "*";
        }
        
        // Check for authorization header & token
        var parseSuccess = AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var authHeader);
        if (!parseSuccess || string.IsNullOrEmpty(authHeader!.Parameter))
        {
            Response.Headers.WWWAuthenticate = "Bearer";
            return Task.FromResult(AuthenticateResult.Fail("Authorization header not found"));
        }
        var token = authHeader.Parameter;
        
        // set up handler and validation parameters
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
        };
        
        // Attempt to validate token
        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            if (principal == null)
                return Task.FromResult(AuthenticateResult.Fail("Invalid login token"));
            
            // Add Customer claim to claimsPrincipal if token has type "Customer
            var jwtToken = validatedToken as JwtSecurityToken;
            var typeClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Typ);
            if (typeClaim != null && principal.Identity is ClaimsIdentity)
            {
                var claimsIdentity = (ClaimsIdentity)principal.Identity;
                // Check if the claim already exists
                if (!claimsIdentity.HasClaim(c => c.Type == typeClaim.Type && c.Value == typeClaim.Value))
                {
                    var claims = new List<Claim>(claimsIdentity.Claims) { typeClaim };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    principal = new ClaimsPrincipal(identity);
                }
            }
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (SecurityTokenExpiredException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Expired Token"));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Authentication failed.");
            return Task.FromResult(AuthenticateResult.Fail("Authentication failed."));
        }
    }
}