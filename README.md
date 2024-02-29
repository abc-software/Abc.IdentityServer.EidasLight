# Abc.IdentityServer.EidasLight ![](https://github.com/abc-software/Abc.IdentityServer.eEidasLight/actions/workflows/dotnet.yml/badge.svg)


## Overview
Implementation eIDAS Light IdP support for Duende IdentityServer and IdentityServer4 with .NET CORE.

More information on [eIDAS-Node Integration Package](https://ec.europa.eu/digital-building-blocks/sites/display/DIGITAL/eIDAS-Node+Integration+Package).

## eIDAS Light proxy endpoint
The eIDAS Light proxy endpoints is implemented via an `Duende.IdentityServer.Hosting.IEndpointHanlder`.
Endpoint _~/eidaslight_ process eIDAS sing-in requests.
This endpoints handles the eIDAS sing-in protocol requests and redirects the user to the login page if needed.

The login page will then use the normal return URL mechanism to redirect back to the eIDAS endpoint
to create the protocol response.

## Response generation
The `SignInResponseGenerator` class does the heavy lifting of creating the contents of the eIDAS Light response:

* it calls the IdentityServer profile service to retrieve the configured claims for the relying party
* it tries to map the standard claim types to WS-* style claim types
* it creates the eIDAS Light response

## Configuration
For most parts, the eIDAS endpoint can use the standard IdentityServer client configuration for relying parties.
But there are also options available for setting eIDAS specific options.

### Defaults
You can configure global defaults in the `EidasLightOptions` class, e.g.:

* default name identifier format
* default mappings from "short" claim types to WS-* claim types
* specify Ignite cache configuratoin

### Relying party configuration
The following client settings are used by the eIDAS endpoint:

```csharp
public static IEnumerable<Client> GetClients()
{
    return new[]
    {
        new Client
        {
            // eIDAS proxy identifier
            ClientId = "urn:proxy",
            
            // must be set to eIDAS
            ProtocolType = "eidaslight",

            // reply URL
            RedirectUris = { "http://localhost:10313/" },
        }
    };
}
```

### eIDAS specific relying party settings
If you want to deviate from the global defaults (e.g. set a different token type or claim mapping) for a specific
relying party, you can define a `RelyingParty` object that uses the same realm name as the client ID used above.

This sample contains an in-memory relying party store that you can use to make these relying party specific settings
available to the eIDAS engine (using the `AddInMemoryRelyingParty` extension method).
Otherwise, if you want to use your own store, you will need an implementation of `IRelyingPartyStore`.

### Configuring IdentityServer
This repo contains an extension method for the IdentityServer builder object to register all the necessary services in DI, e.g.:

```csharp
services.AddIdentityServer()
    .AddSigningCredential(cert)
    .AddInMemoryIdentityResources(Config.GetIdentityResources())
    .AddInMemoryApiResources(Config.GetApiResources())
    .AddInMemoryClients(Config.GetClients())
    .AddTestUsers(TestUsers.Users)
    .AddEidasLight()
    .AddInMemoryRelyingParties(Config.GetRelyingParties());
```