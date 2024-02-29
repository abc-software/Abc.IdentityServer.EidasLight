// ----------------------------------------------------------------------------
// <copyright file="DefaultIgniteClientFactory.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.EidasLight.Ignite;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Client;
using Microsoft.Extensions.Logging;

namespace Abc.IdentityServer.EidasLight.Services
{
    /// <summary>
    /// Default ignite client factory implementation.
    /// </summary>
    public class DefaultIgniteClientFactory : IIgniteClientFactory
    {
        private readonly EidasLightOptions _options;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultIgniteClientFactory"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public DefaultIgniteClientFactory(EidasLightOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc/>
        public IIgniteClient CreateClient()
        {
            return Ignition.StartClient(_options.IgniteCacheConfiguration.GetIgniteClientConfiguration(_loggerFactory));
        }
    }
}