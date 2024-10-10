// ----------------------------------------------------------------------------
// <copyright file="SignInInteractionResponseGenerator.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.EidasLight.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.ResponseProcessing
{
    /// <summary>
    /// Default logic for determining if user must login when making requests to the eIDAS proxy endpoint.
    /// </summary>
    /// <seealso cref="Abc.IdentityServer4.EidasLight.ResponseProcessing.ISignInInteractionResponseGenerator" />
    public class SignInInteractionResponseGenerator : ISignInInteractionResponseGenerator
    {
#pragma warning disable SA1401 // Fields should be private
        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// The clock.
        /// </summary>
        protected readonly IClock Clock;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Initializes a new instance of the <see cref="SignInInteractionResponseGenerator"/> class.
        /// </summary>
        /// <param name="clock">The clock.</param>
        /// <param name="logger">The logger.</param>
        public SignInInteractionResponseGenerator(IClock clock, ILogger<SignInInteractionResponseGenerator> logger)
        {
            Clock = clock;
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual Task<InteractionResponse> ProcessInteractionAsync(ValidatedEidasLightRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // unauthenticated user
            var user = request.Subject;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                Logger.LogInformation("Showing login: User is not authenticated");
                return Task.FromResult(new InteractionResponse { IsLogin = true });
            }

            string currentIdp = user.GetIdentityProvider();

            // check local idp restrictions
            if (currentIdp == Ids.IdentityServerConstants.LocalIdentityProvider)
            {
                if (!request.Client.EnableLocalLogin)
                {
                    Logger.LogInformation("Showing login: User logged in locally, but client does not allow local logins");
                    return Task.FromResult(new InteractionResponse { IsLogin = true });
                }
            }

            // check external idp restrictions if user not using local idp
            else if (request.Client.IdentityProviderRestrictions != null &&
                request.Client.IdentityProviderRestrictions.Any() &&
                !request.Client.IdentityProviderRestrictions.Contains(currentIdp))
            {
                Logger.LogInformation("Showing login: User is logged in with idp: {idp}, but idp not in client restriction list.", currentIdp);
                return Task.FromResult(new InteractionResponse { IsLogin = true });
            }

            // check client's user SSO timeout
            if (request.Client.UserSsoLifetime.HasValue)
            {
                long authTimeEpoch = user.GetAuthenticationTimeEpoch();
                long diff = Clock.UtcNow.ToUnixTimeSeconds() - authTimeEpoch;
                if (diff > request.Client.UserSsoLifetime.Value)
                {
                    Logger.LogInformation("Showing login: User's auth session duration: {sessionDuration} exceeds client's user SSO lifetime: {userSsoLifetime}.", diff, request.Client.UserSsoLifetime);
                    return Task.FromResult(new InteractionResponse { IsLogin = true });
                }
            }

            return Task.FromResult(new InteractionResponse());
        }
    }
}