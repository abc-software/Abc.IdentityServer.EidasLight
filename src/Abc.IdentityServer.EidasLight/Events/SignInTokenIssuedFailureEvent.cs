// ----------------------------------------------------------------------------
// <copyright file="SignInTokenIssuedFailureEvent.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.EidasLight.Validation;

namespace Abc.IdentityServer.EidasLight.Events
{
    /// <summary>
    /// Event for failed token issuance.
    /// </summary>
    /// <seealso cref="IdentityServer4.Events.TokenIssuedFailureEvent" />
    public class SignInTokenIssuedFailureEvent : TokenIssuedFailureEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignInTokenIssuedFailureEvent"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="error">The error.</param>
        /// <param name="description">The description.</param>
        public SignInTokenIssuedFailureEvent(ValidatedEidasLightRequest request, string error, string description)
            : base()
        {
            if (request != null)
            {
                ClientId = request.Client?.ClientId;
                ClientName = request.Client?.ClientName;

                if (request.Subject != null && request.Subject.Identity.IsAuthenticated)
                {
                    SubjectId = request.Subject.GetSubjectId();
                }
            }

            Endpoint = Constants.EndpointNames.EidasLightProxy;
            Error = error;
            ErrorDescription = description;
        }
    }
}