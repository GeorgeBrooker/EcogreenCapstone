using System.Net;
using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace ShopRepository.Services;

public class CognitoService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly CognitoUserPool _userPool;

    public CognitoService(IAmazonCognitoIdentityProvider cognito, IConfiguration configuration)
    {
        _cognitoClient = cognito;
        _userPool = new CognitoUserPool(
            configuration["Cognito:UserPoolId"],
            configuration["Cognito:ClientId"],
            _cognitoClient,
            configuration["Cognito:ClientSecret"]
        );
    }
    
    public async Task<bool> ValidateToken(string token)
    {
        var request = new GetUserRequest { AccessToken = token };
        try
        {
            await _cognitoClient.GetUserAsync(request);
            return true;
        }
        catch (NotAuthorizedException)
        {
            Console.WriteLine("Token is not authorized");
            return false;
        }
        catch (Exception)
        {
            Console.WriteLine("Something else is wrong with the token");
            return false;
        }
    }
    
    public CognitoUser GetUser(string email)
    {
        var user = _userPool.GetUser(email);
        return user;
    }
    public async Task<CognitoUserSession> Login(string email, string password)
    {
        var user = _userPool.GetUser(email);
        var authRequest = new InitiateSrpAuthRequest
        {
            Password = password
        };
        
        var authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

        if (authResponse.AuthenticationResult != null)
        {
            var authResult = authResponse.AuthenticationResult;
            return new CognitoUserSession(
                authResult.IdToken,
                authResult.AccessToken,
                authResult.RefreshToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddSeconds(authResult.ExpiresIn)
                );
        }
        
        throw new Exception("Invalid username or password");
    }
    
    public async Task<string> RefreshSession(string refreshToken, string? deviceKey = null)
    {
        var user = new CognitoUser("username", "clientId", _userPool, _cognitoClient);
        if (deviceKey != null) user.Device = new CognitoDevice(new DeviceType {DeviceKey = deviceKey}, user);
        
        user.SessionTokens = new CognitoUserSession(null, null, refreshToken, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

        var authResponse = await user.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest
            { AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH });

        return authResponse.AuthenticationResult.AccessToken;
    }
    
    public async Task<CognitoUser> Register(string email, string password)
    {
        var signUpRequest = new SignUpRequest
        {
            ClientId = _userPool.ClientID,
            Username = email,
            Password = password,
            UserAttributes = new List<AttributeType>
            {
                new AttributeType
                {
                    Name = "email",
                    Value = email
                } 
            } 
        };
        
        var signUpResponse = await _cognitoClient.SignUpAsync(signUpRequest);
        if (signUpResponse.HttpStatusCode == HttpStatusCode.OK) return _userPool.GetUser(email);

        throw new Exception("Registration failed");
    }
}