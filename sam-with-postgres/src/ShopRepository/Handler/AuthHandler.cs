using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ShopRepository.Services;

namespace ShopRepository.Handler;

public class AuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TimeProvider clock,
    CognitoService cognitoService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
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
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken)) return AuthenticateResult.Fail("Invalid token");
        
        // Validate the access token
        var isValid = await cognitoService.ValidateToken(accessToken);
        if (!isValid)
        {
            // If the token is not valid, try to refresh it
            var newAccessToken = await cognitoService.RefreshSession(refreshToken);
            if (string.IsNullOrEmpty(newAccessToken)) return AuthenticateResult.Fail("Invalid token");
            
            accessToken = newAccessToken;
        }
        
        // Create the claims and principal
        var claims = GetTokenClaims(accessToken);
        var identity = new ClaimsIdentity(claims, Scheme.Name) { BootstrapContext = accessToken };
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    
        return AuthenticateResult.Success(ticket);
    }
    
    private IEnumerable<Claim> GetTokenClaims(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(accessToken) as JwtSecurityToken;
        var tokenClaims = token.Claims.ToList();

        return tokenClaims;
    }
}