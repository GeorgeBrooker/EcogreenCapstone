using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ShopRepository.Services;
using Stripe;

namespace ShopRepository.Handler;

public class AdminAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{


    private readonly IConfiguration _config;

    public AdminAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TimeProvider clock,
        IServiceProvider serviceProvider,
        IConfiguration config
        ) : base(options, logger, encoder)
    {
        _config = config;
    }
    
    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Console.WriteLine("Scheme.Name: " + Scheme.Name);
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        string? accessToken = null;
        string? refreshToken = null;
        
        // Extract the access and refresh tokens from the Authorization header
        if (authHeader != null)
        {
            var parts = authHeader.Split(' ');
            if (parts.Length == 2 && parts[0] == "Bearer")
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
                var tokens = decoded.Split(':');
                if (tokens.Length == 2)
                {
                    accessToken = tokens[0];
                    refreshToken = tokens[1];
                }
            }
        }
        
        if (!CorrectPool(accessToken)) return AuthenticateResult.Fail("User is not in the customer pool");
        
        var expectedIssuer = "https://cognito-idp." + _config["Cognito:Region"] + ".amazonaws.com/" + _config["Cognito:Admin:UserPoolId"];
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(accessToken);
        var tokenParams = new TokenValidationParameters
        {
            IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
            {
                using var httpClient = new HttpClient();
                var json = httpClient.GetStringAsync(parameters.ValidIssuer + "/.well-known/jwks.json").GetAwaiter().GetResult();
                var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(json).Keys;
                return (IEnumerable<SecurityKey>)keys;
            },
            AudienceValidator = (audiences, securityToken, validationParameters) =>
            {
                var authToken = (JwtSecurityToken)securityToken;
                var aud = authToken.Claims.FirstOrDefault(c => c.Type == "client_id");
                return aud != null && aud.Value == _config["Cognito:Admin:ClientId"];
            },
            ValidIssuer = expectedIssuer,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateAudience = true
        };
        

        var principal = tokenHandler.ValidateToken(accessToken, tokenParams, out _);
        if (principal == null) return AuthenticateResult.Fail("Invalid token");
        
        var claims = jwt.Claims.ToList();
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        
        Console.WriteLine("User authenticated successfully");
        return AuthenticateResult.Success(ticket);
    }
    
    private bool CorrectPool(string? token)
    {
        if (token == null) return false;
        
        var expectedIssuer = "https://cognito-idp." + _config["Cognito:Region"] + ".amazonaws.com/" + _config["Cognito:Admin:UserPoolId"];
        Console.WriteLine(expectedIssuer);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        
        return jwt.Issuer == expectedIssuer;
    }
}