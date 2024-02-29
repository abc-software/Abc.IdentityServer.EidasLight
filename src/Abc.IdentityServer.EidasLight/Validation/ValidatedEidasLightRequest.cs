// ----------------------------------------------------------------------------
// <copyright file="ValidatedEidasLightRequest.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Stores;

namespace Abc.IdentityServer.EidasLight.Validation
{
    /// <summary>
    /// Models a validated request to the eIDAS endpoint.
    /// </summary>
    public class ValidatedEidasLightRequest : ValidatedRequest
    {
        /// <summary>
        /// Gets or sets the eIDAS message.
        /// </summary>
        /// <value>
        /// The eIDAS message.
        /// </value>
        public EidasLightRequest Message { get; set; }

        /// <summary>
        /// Gets or sets the relying party.
        /// </summary>
        /// <value>
        /// The relying party.
        /// </value>
        public RelyingParty RelyingParty { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        public string RedirectUri { get; set; }
    }
}