// ----------------------------------------------------------------------------
// <copyright file="EidasLightOptions.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.EidasLight.Ignite;
using Abc.IdentityModel.Protocols.EidasLight;
using IdentityModel;
using Microsoft.IdentityModel.Tokens.Saml2;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Abc.IdentityServer.EidasLight
{
    /// <summary>
    /// The default options for the relying party's behavior.
    /// </summary>
    public class EidasLightOptions
    {
        /// <summary>
        /// Gets or sets the name identifier format. Defaults to Unspecified.
        /// </summary>
        /// <value>
        /// The name identifier format.
        /// </value>
        public string DefaultNameIdentifierFormat { get; set; } = Saml2Constants.NameIdentifierFormats.UnspecifiedString;

        /// <summary>
        /// Gets or sets the ignite cache configuration.
        /// </summary>
        /// <value>
        /// The ignite cache configuration.
        /// </value>
        public EidasProxyIgniteCacheConfiguration IgniteCacheConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the clock screw. Defaults 5min.
        /// </summary>
        /// <value>
        /// The clock screw.
        /// </value>
        public TimeSpan ClockScrew { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the default claim mapping.
        /// </summary>
        /// <value>
        /// The default claim mapping.
        /// </value>
        public IDictionary<string, string> DefaultClaimMapping { get; set; } = new Dictionary<string, string>
        {
            { JwtClaimTypes.Subject, ClaimTypes.NameIdentifier },
            { "ppid", EidasConstants.ClaimTypes.PersonIdentifier },
            { JwtClaimTypes.GivenName, EidasConstants.ClaimTypes.FirstName },
            { JwtClaimTypes.FamilyName, EidasConstants.ClaimTypes.FamilyName },
            { JwtClaimTypes.BirthDate, EidasConstants.ClaimTypes.DateOfBirth },
            { JwtClaimTypes.Gender, EidasConstants.ClaimTypes.Gender },
            { "birthname", EidasConstants.ClaimTypes.BirthName },
            { "placeofbirth", EidasConstants.ClaimTypes.PlaceOfBirth },
            { JwtClaimTypes.Address, EidasConstants.ClaimTypes.CurrentAddress },
        };
    }
}