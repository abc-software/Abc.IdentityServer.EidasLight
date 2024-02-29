// ----------------------------------------------------------------------------
// <copyright file="EidasLightProxyEndpoint.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.EidasLight.ResponseProcessing;
using Abc.IdentityServer.EidasLight.Stores;
using Abc.IdentityServer.EidasLight.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Endpoints
{
    internal class EidasLightProxyEndpoint : EidasLightEndpointBase
    {
        public EidasLightProxyEndpoint(
            IEventService events,
            IUserSession userSession,
            ILogger<EidasLightProxyEndpoint> logger,
            IIgniteProxyStore igniteStore,
            ISignInInteractionResponseGenerator interaction,
            ISignInValidator signinValidator,
            ISignInResponseGenerator generator)
            : base(events, userSession, logger, igniteStore, interaction, signinValidator, generator)
        {
        }

        /// <inheritdoc/>
        public override async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            this.Logger.LogDebug("Start eIDAS proxy request");

            if (!HttpMethods.IsPost(context.Request.Method))
            {
                this.Logger.LogWarning("Invalid HTTP method for eIDAS proxy endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            if (!context.Request.HasFormContentType)
            {
                this.Logger.LogWarning("No form content for eIDAS proxy endpoint.");
                return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
            }

            var tokenString = context.Request.Form[/*EidasLightConstants.ElementNames.Token*/"token"];
            if (string.IsNullOrEmpty(tokenString))
            {
                this.Logger.LogWarning("Missing or empty form 'token' field for eIDAS proxy endpoint.");
                return new StatusCodeResult(HttpStatusCode.BadRequest);
            }

            var getResult = await this.IgniteProxyStore.GetAsync(tokenString);
            if (getResult.IsError)
            {
                return await CreateSignInErrorResult(
                    "EidasLight proxy sign in request retrieving failed",
                    null,
                    getResult.Error,
                    getResult.ErrorDescription);
            }

            // user can be null here (this differs from HttpContext.User where the anonymous user is filled in)
            var user = await this.UserSession.GetUserAsync();
            var result = await this.ProcessAuthorizeRequestAsync(getResult.Request, user);

            this.Logger.LogTrace("End eIDAS proxy request. result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }
    }
}