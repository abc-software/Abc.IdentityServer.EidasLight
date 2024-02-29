// ----------------------------------------------------------------------------
// <copyright file="SignInValidationResult.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------


namespace Abc.IdentityServer.EidasLight.Validation
{
    /// <summary>
    /// Models the validation result of eIDAS request.
    /// </summary>
    public class SignInValidationResult : ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignInValidationResult"/> class.
        /// </summary>
        /// <param name="validatedRequest">The eIDAS validated request.</param>
        /// <exception cref="System.ArgumentNullException">validatedRequest is <c>null</c>.</exception>
        public SignInValidationResult(ValidatedEidasLightRequest validatedRequest)
        {
            this.IsError = false;
            this.ValidatedRequest = validatedRequest ?? throw new System.ArgumentNullException(nameof(validatedRequest));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignInValidationResult"/> class.
        /// </summary>
        /// <param name="validatedRequest">The eIDAS validated request.</param>
        /// <param name="error">The error.</param>
        /// <param name="errorDescription">The error description.</param>
        /// <exception cref="System.ArgumentNullException">validatedRequest is <c>null</c>.</exception>
        public SignInValidationResult(ValidatedEidasLightRequest validatedRequest, string error, string errorDescription = null)
        {
            this.IsError = true;
            this.ValidatedRequest = validatedRequest ?? throw new System.ArgumentNullException(nameof(validatedRequest));
            this.Error = error;
            this.ErrorDescription = errorDescription;
        }

        /// <summary>
        /// Gets the eIDAS validated request.
        /// </summary>
        /// <value>
        /// The eIDAS validated request.
        /// </value>
        public ValidatedEidasLightRequest ValidatedRequest { get; }
    }
}