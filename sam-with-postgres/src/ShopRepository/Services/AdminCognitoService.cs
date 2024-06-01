using System.Net;
using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;


namespace ShopRepository.Services;

public class AdminCognitoService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly CognitoUserPool _userPool;

    public AdminCognitoService(IAmazonCognitoIdentityProvider cognito, IConfiguration configuration)
    {
        Console.WriteLine(configuration["Cognito:Admin:UserPoolId"] + " " + configuration["Cognito:Admin:ClientId"]);
        _cognitoClient = cognito;
        _userPool = new CognitoUserPool(
            configuration[$"Cognito:Admin:UserPoolId"],
            configuration[$"Cognito:Admin:ClientId"],
            _cognitoClient
        );
    }
    
    public async Task<bool> ValidateToken(string token)
    {
        Console.WriteLine("Validating token in pool: " + _userPool.PoolID + "\nClient: " + _userPool.ClientID);
        var request = new GetUserRequest { AccessToken = token };
        try
        {
            await _cognitoClient.GetUserAsync(request);
            return true;
        }
        catch (NotAuthorizedException e)
        {
            Console.WriteLine("Token is not authorized");
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something else is wrong with the token");
            return false;
        }
    }
    
    public async Task<string> RefreshSession(string refreshToken, string? deviceKey = null)
    {
        Console.WriteLine("Refreshing session with token: " + refreshToken);
        Console.WriteLine("PoolId: " + _userPool.PoolID);
        var user = new CognitoUser("username", "clientId", _userPool, _cognitoClient);
        if (deviceKey != null) user.Device = new CognitoDevice(new DeviceType { DeviceKey = deviceKey }, user);

        user.SessionTokens =
            new CognitoUserSession(null, null, refreshToken, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

        var authResponse = await user.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest
            { AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH });

        return authResponse.AuthenticationResult.AccessToken;
    }
}