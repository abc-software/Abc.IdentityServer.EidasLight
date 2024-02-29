// ----------------------------------------------------------------------------
// <copyright file="EidasLightProxyCallbackEndpoint.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.ResponseProcessing;
using Abc.IdentityServer.EidasLight.Stores;
using Abc.IdentityServer.EidasLight.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Endpoints
{
    internal class EidasLightProxyCallbackEndpoint : EidasLightEndpointBase
    {
        private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;
        private readonly EidasLightProtocolSerializer _protocolSerializer;

        public EidasLightProxyCallbackEndpoint(
            IEventService events,
            IUserSession userSession,
            ILogger<EidasLightProxyCallbackEndpoint> logger,
            IIgniteProxyStore igniteStore,
            ISignInInteractionResponseGenerator interaction,
            ISignInValidator signinValidator,
            ISignInResponseGenerator generator,
            EidasLightProtocolSerializer protocolSerializer,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore)
            : base(events, userSession, logger, igniteStore, interaction, signinValidator, generator)
        {
            _protocolSerializer = protocolSerializer;
            _authorizationParametersMessageStore = authorizationParametersMessageStore;
        }

        /// <inheritdoc/>
        public override async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            Logger.LogDebug("Start eIDAS proxy callback request");

            if (!HttpMethods.IsGet(context.Request.Method))
            {
                Logger.LogWarning("Invalid HTTP method for eIDAS proxy callback endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var messageStoreId = context.Request.Query[Constants.DefaultRoutePathParams.MessageStoreId];
            var data = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
            await _authorizationParametersMessageStore.DeleteAsync(messageStoreId);

            var requestMessage = _protocolSerializer.ReadMessageDictionary(data?.Data) as EidasLightRequest;

            // user can be null here (this differs from HttpContext.User where the anonymous user is filled in)
            var user = await UserSession.GetUserAsync();
            var result = await ProcessAuthorizeRequestAsync(requestMessage, user);

            Logger.LogTrace("End eIDAS proxy callback request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }
    }
}