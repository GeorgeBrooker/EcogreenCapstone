using System.Net;
using System.Runtime.Versioning;
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
    private readonly string _clientSecret;
    public CognitoService(IAmazonCognitoIdentityProvider cognito, IConfiguration configuration)
    {
        _clientSecret = configuration["Cognito:ClientSecret"]!;
        _cognitoClient = cognito;
        _userPool = new CognitoUserPool(
            poolID: configuration["Cognito:UserPoolId"],
            clientID: configuration["Cognito:ClientId"],
            provider: _cognitoClient,
            clientSecret: _clientSecret
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

    public async Task<CognitoUser?> GetUser(string? id, string? accessToken)
    {
        CognitoUser? user = null;
        try
        {
            if (id == null || accessToken == null) throw new Exception();
            user = _userPool.GetUser(id);
            
            // Populate the user object with the user's attributes
            var request = new GetUserRequest { AccessToken =  accessToken};
            var response = await _cognitoClient.GetUserAsync(request);
            foreach (var attribute in response.UserAttributes)
            {
                user.Attributes.Add(attribute.Name, attribute.Value);
            }
        }
        catch (Exception)
        {
            Console.WriteLine("User not found in cognito pool");
        }
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
    
    public async Task<SignUpResponse> Register(string email, string password, string fname, string lname)
    {
        Console.WriteLine("CognitoRegister function");
        var signUpRequest = new SignUpRequest
        {
            ClientId = _userPool.ClientID,
            Username = email,
            Password = password,
            SecretHash = CalculateSecretHash(_userPool.ClientID, _clientSecret, email),
            UserAttributes = 
            {
                new AttributeType
                {
                    Name = "email",
                    Value = email
                },
                new AttributeType
                {
                    Name = "name",
                    Value = fname + " " + lname
                },
                new AttributeType
                {
                    Name = "family_name",
                    Value = lname
                },
                new AttributeType
                {
                    Name = "given_name",
                    Value = fname
                }
            }
        };
        
        var signUpResponse = await _cognitoClient.SignUpAsync(signUpRequest);
        if (signUpResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"Registration failed with status code: {signUpResponse.HttpStatusCode}");
            return signUpResponse;
        }
        
        // User is now registered, add them to the 'Customers' group by default
        var groupAddRequest = new AdminAddUserToGroupRequest
        {
            UserPoolId = _userPool.PoolID,
            Username = email,
            GroupName = "Customers"
        };
        
        var groupAdditionResponse = await _cognitoClient.AdminAddUserToGroupAsync(groupAddRequest);
        if (groupAdditionResponse.HttpStatusCode != HttpStatusCode.OK)
            Console.WriteLine($"Group addition failed with status code: {groupAdditionResponse.HttpStatusCode}");
        
        // Return original signup response
        return signUpResponse;
    }
    
    public async Task<ConfirmSignUpResponse> ConfirmUser(string email, string code)
    {
        var confirmRequest = new ConfirmSignUpRequest
        {
            ClientId = _userPool.ClientID,
            Username = email,
            ConfirmationCode = code,
            SecretHash = CalculateSecretHash(_userPool.ClientID, _clientSecret, email)
        };
        
        var confirmResponse = await _cognitoClient.ConfirmSignUpAsync(confirmRequest);
        if (confirmResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"Confirmation failed with status code: {confirmResponse.HttpStatusCode}");
        }

        return confirmResponse;
    }
    
    private static string CalculateSecretHash(string userPoolClientId, string userPoolClientSecret, string userName)
    {
        var dataString = userName + userPoolClientId;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(userPoolClientSecret));
        var dataBytes = Encoding.UTF8.GetBytes(dataString);
        var hash = hmac.ComputeHash(dataBytes);

        return Convert.ToBase64String(hash);
    }
}