// ----------------------------------------------------------------------------
// <copyright file="ISignInInteractionResponseGenerator.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.EidasLight.Validation;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.ResponseProcessing
{
    /// <summary>
    /// Interface for determining if user must login when making requests to the eIDAS light endpoint.
    /// </summary>
    public interface ISignInInteractionResponseGenerator
    {
        /// <summary>
        /// Processes the interaction logic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The interaction response.</returns>
        Task<InteractionResponse> ProcessInteractionAsync(ValidatedEidasLightRequest request);
    }
}