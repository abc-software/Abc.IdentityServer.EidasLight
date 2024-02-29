// ----------------------------------------------------------------------------
// <copyright file="IgniteProxyStore.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Services;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Stores
{
    public class IgniteProxyStore : IIgniteProxyStore
    {
        private readonly EidasLightOptions _options;
        private readonly EidasLightProtocolSerializer _protocolSerializer;
        private readonly ISystemClock _clock;
        private readonly IIgniteClientFactory _igniteClientFactory;

        public IgniteProxyStore(
            EidasLightOptions options,
            EidasLightProtocolSerializer protocolSerializer,
            ISystemClock clock,
            IIgniteClientFactory igniteClientFactory)
        {
            _options = options;
            _protocolSerializer = protocolSerializer;
            _clock = clock;
            _igniteClientFactory = igniteClientFactory;
        }

        public async Task<IgniteProxyGetResult> GetAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new IgniteProxyGetResult("eidaslight_token", "No present eIDAS light token.");
            }

            // parse token
            var binaryLightToken = (BinaryLightToken)null;
            try
            {
                binaryLightToken = BinaryLightToken.Parse(token);
            }
            catch (FormatException)
            {
                return new IgniteProxyGetResult("eidaslight_token", "Invalid eIDAS light token.");
            }

            // time comparison
            var tokenTimestamp = binaryLightToken.Timestamp.ToUniversalTime();
            var now = _clock.UtcNow.UtcDateTime;
            if (tokenTimestamp.InFuture(now, _options.ClockScrew) || tokenTimestamp.InPast(now, _options.ClockScrew))
            {
                return new IgniteProxyGetResult("eidaslight_token", "Sender current time is in past or future");
            }

            // signature validation
            if (!binaryLightToken.Validate(_options.IgniteCacheConfiguration.IncomingSecret))
            {
                return new IgniteProxyGetResult("eidaslight_token", "Invalid eIDAS light token digest.");
            }

            // get payload from Ignite cache
            string data;
            using (var client = _igniteClientFactory.CreateClient())
            {
                var cache = client.GetOrCreateCache<string, string>(_options.IgniteCacheConfiguration.IncomingCacheName);
                var result = await cache.GetAndRemoveAsync(binaryLightToken.Id);
                data = result.Value;
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                return new IgniteProxyGetResult("ignite", "No data present in ignite cache.");
            }

            // Serialize to EidasLightRequest
            var message = _protocolSerializer.ReadMessageString(data) as EidasLightRequest;

            // FIX: Original request contain invalid issuer
            if (message != null)
            {
                message.Issuer = binaryLightToken.IssuerName;
            }

            return new IgniteProxyGetResult(message);
        }

        public async Task<string> StoreAsync(EidasLightResponse message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var binaryLightToken = new BinaryLightToken(message.Issuer, message.Id, _clock.UtcNow.DateTime);

            var data = _protocolSerializer.WriteMessageString(message);

            // Store message into ignite
            using (var client = _igniteClientFactory.CreateClient())
            {
                var cache = client.GetOrCreateCache<string, string>(_options.IgniteCacheConfiguration.OutgoingCacheName);
                await cache.PutAsync(binaryLightToken.Id, data);
            }

            // Calculate message hash
            var token = binaryLightToken.Sign(_options.IgniteCacheConfiguration.OutgoingSecret);

            return token;
        }
    }
}