## Auth0

Currently out of the Box we support `Auth0`

To use `Auth0` just add `Auth0` settings in your `appsettings.*.json` (or dependent on your configuration providers environment variabley, appconfiguration services,secets.json..) and the `SimpleStartup` will automatically setup 
`Auth0`. You dont have to do anyting just configure and use it :) 

```json
{
  "Auth0": {
    "Authority": "https://auth0.com/",                // Your Auth0 enpoint
    "TokenEndpoint": "https://auth0.com/oauth/token", // Your Auth0 token endpoint
    "ClientId": "123456",                             // Your client id
    "ClientSecret": "I am a secret",                  // Please never put this in your appsettings.json ;)
    "Audience": "development",                        // Development
    "TokenCacheTime": "01:00:00"                      // The token cache time
  }
}
```


## OAuth2Scope
```csharp
[HttpPost]
[OAuth2Scope("users.write")] // OAuth2 scope attribute
[ProducesResponseType(typeof(AddUserResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[SwaggerOperation(OperationId = "addUser")]
public Task<AddUserResponse> AddUserAsync(User user)
{
    return _mediator.SendAsync(new AddUser(user));
}
```