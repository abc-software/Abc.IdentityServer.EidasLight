// ----------------------------------------------------------------------------
// <copyright file="ISignInValidator.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Validation
{
    /// <summary>
    /// Service for validating a eIDAS request.
    /// </summary>
    public interface ISignInValidator
    {
        /// <summary>
        /// Validates eIDAS request.
        /// </summary>
        /// <param name="message">The eIDAS message.</param>
        /// <param name="user">The user.</param>
        /// <returns>The validation result.</returns>
        Task<SignInValidationResult> ValidateAsync(EidasLightRequest message, ClaimsPrincipal user);
    }
}