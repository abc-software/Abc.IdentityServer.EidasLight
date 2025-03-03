﻿// ----------------------------------------------------------------------------
// <copyright file="ErrorPageResult.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Endpoints.Results
{
    internal class ErrorPageResult : IEndpointResult
    {
        private IMessageStore<ErrorMessage> _errorMessageStore;
        private IdentityServerOptions _options;
        private IServerUrls _urls;
        private IClock _clock;

        public ErrorPageResult(string error, string errorDescription)
        {
            Error = error;
            ErrorDescription = errorDescription;
        }

        internal ErrorPageResult(string error, string errorDescription, IdentityServerOptions options, IClock clock, IServerUrls urls, IMessageStore<ErrorMessage> errorMessageStore)
            : this(error, errorDescription)
        {
            _options = options;
            _clock = clock;
            _urls = urls;
            _errorMessageStore = errorMessageStore;
        }

        public string Error { get; }

        public string ErrorDescription { get; }

        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            var errorMessage = new ErrorMessage
            {
                RequestId = context.TraceIdentifier,
                Error = Error,
                ErrorDescription = ErrorDescription,
            };

            var message = new Message<ErrorMessage>(errorMessage, _clock.UtcNow.UtcDateTime);
            var id = await _errorMessageStore.WriteAsync(message);

            var redirectUrl = _options.UserInteraction.ErrorUrl;
            redirectUrl = redirectUrl.AddQueryString(_options.UserInteraction.ErrorIdParameter, id);

            context.Response.Redirect(_urls.GetAbsoluteUrl(redirectUrl));
        }

        private void Init(HttpContext context)
        {
            _errorMessageStore ??= context.RequestServices.GetRequiredService<IMessageStore<ErrorMessage>>();
            _options ??= context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _urls ??= context.RequestServices.GetRequiredService<IServerUrls>();
            _clock ??= context.RequestServices.GetRequiredService<IClock>();
        }
    }
}