// ----------------------------------------------------------------------------
// <copyright file="SignInTokenIssuedSuccessEvent.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Validation;
using Abc.IdentityServer.Extensions;
using System.Collections.Generic;

namespace Abc.IdentityServer.EidasLight.Events
{
    /// <summary>
    ///  Event for successful token issuance.
    /// </summary>
    /// <seealso cref="IdentityServer4.Events.TokenIssuedSuccessEvent" />
    public class SignInTokenIssuedSuccessEvent : TokenIssuedSuccessEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignInTokenIssuedSuccessEvent"/> class.
        /// </summary>
        /// <param name="token">The response token.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="request">The request.</param>
        public SignInTokenIssuedSuccessEvent(string token, EidasLightResponse responseMessage, SignInValidationResult request)
            : base()
        {
            ClientId = request.ValidatedRequest.Client?.ClientId;
            ClientName = request.ValidatedRequest.Client?.ClientName;
            Endpoint = Constants.EndpointNames.EidasLightProxy;
            SubjectId = request.ValidatedRequest.Subject?.GetSubjectId();
            Scopes = request.ValidatedRequest.ValidatedResources?.RawScopeValues.ToSpaceSeparatedString();

            var tokens = new List<Token>();
            tokens.Add(new Token("eIDASResponseToken", token));
            Tokens = tokens;
        }
    }
}