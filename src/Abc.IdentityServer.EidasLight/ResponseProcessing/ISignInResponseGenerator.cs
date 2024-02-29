// ----------------------------------------------------------------------------
// <copyright file="ISignInResponseGenerator.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Validation;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.ResponseProcessing
{
    /// <summary>
    /// Interface for the eIDAS light response generator.
    /// </summary>
    public interface ISignInResponseGenerator
    {
        /// <summary>
        /// Generates the eIDAS light response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns>The eIDAS light response.</returns>
        Task<EidasLightResponse> GenerateResponseAsync(SignInValidationResult validationResult);
    }
}