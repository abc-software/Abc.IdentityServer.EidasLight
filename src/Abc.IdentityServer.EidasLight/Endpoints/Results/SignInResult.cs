// ----------------------------------------------------------------------------
// <copyright file="SignInResult.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Endpoints.Results
{
    /// <summary>
    /// Result for eIDAS response.
    /// </summary>
    /// <seealso cref="IEndpointResult" />
    public class SignInResult : IEndpointResult
    {
        private IdentityServerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignInResult"/> class.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="redirectUri">The redirect URI.</param>
        public SignInResult(string token, string redirectUri)
        {
            Token = token;
            RedirectUri = redirectUri;
        }

        internal SignInResult(string token, string redirectUri, IdentityServerOptions options)
            : this(token, redirectUri)
        {
            _options = options;
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        public string Token { get; }

        /// <summary>
        /// Gets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        public string RedirectUri { get; }

        /// <inheritdoc/>
        public Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            context.Response.SetNoCache();
            context.Response.AddFormPostCspHeaders(_options.Csp, RedirectUri.GetOrigin(), "sha256-veRHIN/XAFeehi7cRkeVBpkKTuAUMFxwA+NMPmu2Bec=");

            var html = BuildFormPost(RedirectUri, Token);
            return context.Response.WriteHtmlAsync(html);
        }

        private void Init(HttpContext context)
        {
            _options ??= context.RequestServices.GetRequiredService<IdentityServerOptions>();
        }

        private string BuildFormPost(string uri, string token)
        {
            return $"<!DOCTYPE html><html><head><title>Working...</title></head><body><form action='{uri}' method='post'><input type='hidden' name='token' value='{token}'/><noscript><p><strong>Note:</strong> Since your browser does not support JavaScript, you must press the Continue button once to proceed.</p><input type='submit' value='Continue'/></noscript></form><script>window.setTimeout(function() {{document.forms[0].submit();}}, 0);</script></body></html>";
        }
    }
}