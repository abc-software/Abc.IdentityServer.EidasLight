// ----------------------------------------------------------------------------
// <copyright file="EidasLightEndpointBase.cs" company="ABC software Ltd">
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
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Endpoints
{
    internal abstract class EidasLightEndpointBase : IEndpointHandler
    {
        private readonly IEventService _events;
        private readonly ISignInInteractionResponseGenerator _interaction;
        private readonly ISignInValidator _signinValidator;
        private readonly ISignInResponseGenerator _generator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EidasLightEndpointBase"/> class.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="userSession">The user session.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="igniteStore">The ignite store.</param>
        /// <param name="interaction">The eIDAS request interaction.</param>
        /// <param name="signinValidator">The eIDAS request validator.</param>
        /// <param name="generator">The eIDAS response generator.</param>
        protected EidasLightEndpointBase(
            IEventService events,
            IUserSession userSession,
            ILogger logger,
            IIgniteProxyStore igniteStore,
            ISignInInteractionResponseGenerator interaction,
            ISignInValidator signinValidator,
            ISignInResponseGenerator generator)
        {
            _events = events;
            UserSession = userSession;
            Logger = logger;
            IgniteProxyStore = igniteStore;
            _interaction = interaction;
            _signinValidator = signinValidator;
            _generator = generator;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the user session.
        /// </summary>
        /// <value>
        /// The user session.
        /// </value>
        protected IUserSession UserSession { get; }

        /// <summary>
        /// Gets the ignite proxy store.
        /// </summary>
        /// <value>
        /// The ignite proxy store.
        /// </value>
        protected IIgniteProxyStore IgniteProxyStore { get; }

        /// <inheritdoc/>
        public abstract Task<IEndpointResult> ProcessAsync(HttpContext context);

        internal async Task<IEndpointResult> ProcessAuthorizeRequestAsync(EidasLightRequest signin, ClaimsPrincipal user)
        {
            if (user != null && user.Identity.IsAuthenticated)
            {
                Logger.LogDebug("User in eIDAS proxy signin request: {subjectId}", user.GetSubjectId());
            }
            else
            {
                Logger.LogDebug("No user present in eIDAS proxy signin request");
            }

            var validationResult = await _signinValidator.ValidateAsync(signin, user);
            if (validationResult.IsError)
            {
                return await CreateSignInErrorResult(
                    "EidasLight proxy sign in request validation failed",
                    validationResult.ValidatedRequest,
                    validationResult.Error,
                    validationResult.ErrorDescription);
            }

            var interactionResult = await _interaction.ProcessInteractionAsync(validationResult.ValidatedRequest);
            if (interactionResult.IsError)
            {
                return await CreateSignInErrorResult(
                    "EidasLight proxy interaction generator error",
                    validationResult.ValidatedRequest,
                    interactionResult.Error,
                    interactionResult.ErrorDescription,
                    false);
            }

            if (interactionResult.IsLogin)
            {
                return new Results.LoginPageResult(validationResult.ValidatedRequest);
            }

            var responseMessage = await _generator.GenerateResponseAsync(validationResult);
            var token = await IgniteProxyStore.StoreAsync(responseMessage);

            await UserSession.AddClientIdAsync(validationResult.ValidatedRequest.ClientId);

            await _events.RaiseAsync(new Events.SignInTokenIssuedSuccessEvent(token, responseMessage, validationResult));
            return new Results.SignInResult(token, validationResult.ValidatedRequest.RedirectUri);
        }

        protected async Task<IEndpointResult> CreateSignInErrorResult(
            string logMessage,
            ValidatedEidasLightRequest request = null,
            string error = "server_error",
            string errorDescription = null,
            bool logError = true)
        {
            if (logError)
            {
                Logger.LogError(logMessage);
            }

            if (request != null)
            {
                Logger.LogInformation("{@validationDetails}", new Logging.EidasLightRequestValidationLog(request));
            }

            await _events.RaiseAsync(new Events.SignInTokenIssuedFailureEvent(request, error, errorDescription));

            return new Results.ErrorPageResult(error, errorDescription);
        }
    }
}