// ----------------------------------------------------------------------------
// <copyright file="IIgniteClientFactory.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Apache.Ignite.Core.Client;

namespace Abc.IdentityServer.EidasLight.Services
{
    /// <summary>
    /// Ignite client factory.
    /// </summary>
    public interface IIgniteClientFactory
    {
        /// <summary>
        /// Creates and configures an <seealso cref="IIgniteClient"/> instance.
        /// </summary>
        /// <returns>A new <seealso cref="IIgniteClient"/> instance.</returns>
        IIgniteClient CreateClient();
    }
}