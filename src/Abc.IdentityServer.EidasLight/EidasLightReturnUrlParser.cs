// ----------------------------------------------------------------------------
// <copyright file="EidasLightReturnUrlParser.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Validation;
using Abc.IdentityServer.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight
{
    public class EidasLightReturnUrlParser : IReturnUrlParser
    {
        private readonly ILogger logger;
        private readonly ISignInValidator signinValidator;
        private readonly IUserSession userSession;
        private readonly IAuthorizationParametersMessageStore authorizationParametersMessageStore;
        private readonly EidasLightProtocolSerializer protocolSerializer;

        public EidasLightReturnUrlParser(
            IUserSession userSession,
            ISignInValidator signinValidator,
            EidasLightProtocolSerializer protocolSerializer,
            ILogger<EidasLightReturnUrlParser> logger,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore)
        {
            this.signinValidator = signinValidator;
            this.protocolSerializer = protocolSerializer;
            this.userSession = userSession;
            this.logger = logger;
            this.authorizationParametersMessageStore = authorizationParametersMessageStore;
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            if (returnUrl != null && returnUrl.IsLocalUrl())
            {
                var index = returnUrl.IndexOf('?');
                if (index >= 0)
                {
                    returnUrl = returnUrl.Substring(0, index);
                }

                if ((returnUrl.EndsWith(Constants.ProtocolRoutePaths.EidasLightProxy, StringComparison.Ordinal)
                    || returnUrl.EndsWith(Constants.ProtocolRoutePaths.EidasLightProxyCallback, StringComparison.Ordinal))
                    && index >= 0)
                {
                    this.logger.LogTrace("eIDAS light - returnUrl is valid");
                    return true;
                }
            }

            this.logger.LogTrace("eIDAS light - returnUrl is not valid");
            return false;
        }

        public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
        {
            if (!this.IsValidReturnUrl(returnUrl))
            {
                return null;
            }

            var signInMessage = await this.GetSignInRequestMessage(returnUrl);
            if (signInMessage == null)
            {
                return null;
            }

            var user = await this.userSession.GetUserAsync();
            var result = await this.signinValidator.ValidateAsync(signInMessage, user);
            if (result.IsError)
            {
                return null;
            }

            var validatedRequest = result.ValidatedRequest;
            var request = new AuthorizationRequest()
            {
                Client = validatedRequest.Client,
                //IdP = validatedRequest.WsFederationMessage.Whr,
                RedirectUri = validatedRequest.RedirectUri,
                ValidatedResources = validatedRequest.ValidatedResources,
            };

            return request;
        }

        private async Task<EidasLightRequest> GetSignInRequestMessage(string returnUrl)
        {
            var index = returnUrl.IndexOf('?');
            if (index >= 0)
            {
                returnUrl = returnUrl.Substring(index);
            }

            var query = QueryHelpers.ParseNullableQuery(returnUrl);
            if (!query.ContainsKey(Constants.DefaultRoutePathParams.MessageStoreId))
            {
                return null;
            }

            string messageStoreId = query[Constants.DefaultRoutePathParams.MessageStoreId];
            var data = await this.authorizationParametersMessageStore.ReadAsync(messageStoreId);
            return this.protocolSerializer.ReadMessageDictionary(data?.Data) as EidasLightRequest;
        }
    }
}