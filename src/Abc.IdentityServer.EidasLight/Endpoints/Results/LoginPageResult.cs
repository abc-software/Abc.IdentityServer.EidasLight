// ----------------------------------------------------------------------------
// <copyright file="LoginPageResult.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Validation;
using Abc.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Endpoints.Results
{
    /// <summary>
    /// Result for login page.
    /// </summary>
    /// <seealso cref="IEndpointResult" />
    public class LoginPageResult : IEndpointResult
    {
        private readonly ValidatedEidasLightRequest _request;
        private IdentityServerOptions _options;
        private IAuthorizationParametersMessageStore _authorizationParametersMessageStore;
        private EidasLightProtocolSerializer _protocolSerializer;
        private ISystemClock _clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPageResult"/> class.
        /// </summary>
        /// <param name="request">The validated eIDAS light request.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
        public LoginPageResult(ValidatedEidasLightRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        internal LoginPageResult(ValidatedEidasLightRequest request, IdentityServerOptions options, ISystemClock clock, EidasLightProtocolSerializer protocolSerializer, IAuthorizationParametersMessageStore authorizationParametersMessageStore)
            : this(request)
        {
            _options = options;
            _clock = clock;
            _protocolSerializer = protocolSerializer;
            _authorizationParametersMessageStore = authorizationParametersMessageStore;
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            var returnUrl = context.GetIdentityServerBasePath().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.EidasLightProxyCallback;

            var data = _protocolSerializer.WriteMessageDictionary(_request.Message);
            var msg = new Message<IDictionary<string, string[]>>(data, _clock.UtcNow.UtcDateTime);
            var id = await _authorizationParametersMessageStore.WriteAsync(msg);
            returnUrl = returnUrl.AddQueryString(Constants.DefaultRoutePathParams.MessageStoreId, id);

            var loginUrl = _options.UserInteraction.LoginUrl;
            if (!loginUrl.IsLocalUrl())
            {
                // this converts the relative redirect path to an absolute one if we're
                // redirecting to a different server
                returnUrl = context.GetIdentityServerHost().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
            }

            var url = loginUrl.AddQueryString(_options.UserInteraction.LoginReturnUrlParameter, returnUrl);
            context.Response.RedirectToAbsoluteUrl(url);
        }

        private void Init(HttpContext context)
        {
            _options ??= context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _protocolSerializer ??= context.RequestServices.GetRequiredService<EidasLightProtocolSerializer>();
            _authorizationParametersMessageStore ??= context.RequestServices.GetRequiredService<IAuthorizationParametersMessageStore>();
            _clock ??= context.RequestServices.GetRequiredService<ISystemClock>();
        }
    }
}