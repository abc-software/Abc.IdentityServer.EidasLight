// ----------------------------------------------------------------------------
// <copyright file="SignInValidator.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Stores;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Validation
{
    public class SignInValidator : ISignInValidator
    {
        private readonly IClientStore clients;
        private readonly IRelyingPartyStore relyingParties;
        private readonly IRedirectUriValidator uriValidator;
        private readonly IdentityServerOptions options;
        private readonly IUserSession userSession;
        private readonly ILogger logger;

        public SignInValidator(
            IdentityServerOptions options,
            IClientStore clients,
            IRelyingPartyStore relyingParties,
            IRedirectUriValidator uriValidator,
            IUserSession userSession,
            ILogger<SignInValidator> logger)
        {
            this.options = options;
            this.clients = clients;
            this.relyingParties = relyingParties;
            this.uriValidator = uriValidator;
            this.userSession = userSession;
            this.logger = logger;
        }

        public async Task<SignInValidationResult> ValidateAsync(EidasLightRequest message, ClaimsPrincipal user)
        {
            var validatedRequest = new ValidatedEidasLightRequest()
            {
                Options = this.options,
                Message = message,
            };

            this.logger.LogInformation("Start eIDAS signin request validation");

            if (message is null)
            {
                return new SignInValidationResult(validatedRequest, "invalid_relying_party", "Cannot find Client configuration");
            }

            // check client
            var client = await this.clients.FindEnabledClientByIdAsync(message.Issuer);
            if (client == null)
            {
                return new SignInValidationResult(validatedRequest, "invalid_relying_party", "Cannot find Client configuration");
            }

            if (client.ProtocolType != Constants.ProtocolTypes.EidasLight)
            {
                return new SignInValidationResult(validatedRequest, "invalid_relying_party", "Client is not configured for eIDAS light");
            }

            validatedRequest.SetClient(client);

            validatedRequest.RedirectUri = client.RedirectUris.FirstOrDefault();
            if (validatedRequest.RedirectUri == null)
            {
                return new SignInValidationResult(validatedRequest, "invalid_relying_party", "No redirect URL configured for relying party");
            }

            // check if additional relying party settings exist
            validatedRequest.RelyingParty = await this.relyingParties.FindRelyingPartyByRealm(message.Issuer);

            validatedRequest.SessionId = await this.userSession.GetSessionIdAsync();
            validatedRequest.Subject = user;

            await this.ValidateRequestedResourcesAsync(validatedRequest);

            return new SignInValidationResult(validatedRequest);
        }

        protected virtual Task ValidateRequestedResourcesAsync(ValidatedEidasLightRequest validatedRequest)
        {
            /*
            var resourceValidationResult = await resourceValidator.ValidateRequestedResourcesAsync(new ResourceValidationRequest
            {
                Client = validatedResult.Client,
                Scopes = validatedResult.Client.AllowedScopes,
            });
            if (!resourceValidationResult.Succeeded)
            {
                if (resourceValidationResult.InvalidScopes.Any())
                {
                    LogError("Invalid scopes requested");
                }
                else
                {
                    LogError("Invalid scopes for client requested");
                }
                return false;
            }
            */
            var resourceValidationResult = new ResourceValidationResult();

            foreach (var item in validatedRequest.Client.AllowedScopes)
            {
                resourceValidationResult.ParsedScopes.Add(new ParsedScopeValue(item));
            }

            validatedRequest.ValidatedResources = resourceValidationResult;
            return Task.CompletedTask;
        }
    }
}