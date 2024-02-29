// ----------------------------------------------------------------------------
// <copyright file="IClaimsService.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Services
{
    /// <summary>
    /// The claims service is responsible for determining which claims to include in tokens.
    /// </summary>
    public interface IClaimsService
    {
        /// <summary>
        /// Returns claims for an security token.
        /// </summary>
        /// <param name="validatedRequest">The validated request.</param>
        /// <param name="requestedClaimTypes">The requested claims.</param>
        /// <returns>
        /// Claims for the security token.
        /// </returns>
        Task<IEnumerable<Claim>> GetClaimsAsync(ValidatedRequest validatedRequest, IEnumerable<string> requestedClaimTypes);

        /// <summary>
        /// Maps the claims.
        /// </summary>
        /// <param name="claimsMapping">The claims mapping.</param>
        /// <param name="claims">The claims.</param>
        /// <returns>The mapped claims.</returns>
        IEnumerable<Claim> MapClaims(IDictionary<string, string> claimsMapping, IEnumerable<Claim> claims);
    }
}