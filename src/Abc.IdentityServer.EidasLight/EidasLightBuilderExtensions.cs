// ----------------------------------------------------------------------------
// <copyright file="EidasLightBuilderExtensions.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityServer.EidasLight;
using Abc.IdentityServer.EidasLight.Endpoints;
using Abc.IdentityServer.EidasLight.ResponseProcessing;
using Abc.IdentityServer.EidasLight.Services;
using Abc.IdentityServer.EidasLight.Stores;
using Abc.IdentityServer.EidasLight.Validation;
using Abc.IdentityServer.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EidasLightBuilderExtensions
    {
        public static IIdentityServerBuilder AddEidasLight(this IIdentityServerBuilder builder)
        {
            return AddEidasLight<NoRelyingPartyStore>(builder);
        }

        public static IIdentityServerBuilder AddEidasLight<TStore>(this IIdentityServerBuilder builder)
            where TStore : class, IRelyingPartyStore
        {
            builder.Services.AddSingleton<Abc.IdentityModel.Protocols.EidasLight.EidasLightProtocolSerializer>();
            builder.Services.AddSingleton(
                resolver => resolver.GetRequiredService<IOptions<EidasLightOptions>>().Value);

            builder.Services.AddTransient<ISignInResponseGenerator, SignInResponseGenerator>();
            builder.Services.AddTransient<ISignInValidator, SignInValidator>();
            builder.Services.AddTransient<Abc.IdentityServer.EidasLight.Services.IClaimsService, Abc.IdentityServer.EidasLight.Services.DefaultClaimsService>();
            builder.Services.AddTransient<ISignInInteractionResponseGenerator, SignInInteractionResponseGenerator>();
            builder.Services.AddTransient<Ids.Services.IReturnUrlParser, EidasLightReturnUrlParser>();
            builder.Services.AddTransient<IIgniteProxyStore, IgniteProxyStore>();
            builder.Services.AddTransient<IIgniteClientFactory, DefaultIgniteClientFactory>();
            builder.Services.TryAddTransient<IRelyingPartyStore, TStore>();

            builder.AddEndpoint<EidasLightProxyEndpoint>(Constants.EndpointNames.EidasLightProxy, Constants.ProtocolRoutePaths.EidasLightProxy.EnsureLeadingSlash());
            builder.AddEndpoint<EidasLightProxyCallbackEndpoint>(Constants.EndpointNames.EidasLightProxyCallback, Constants.ProtocolRoutePaths.EidasLightProxyCallback.EnsureLeadingSlash());
            return builder;
        }

        public static IIdentityServerBuilder AddEidasLight(this IIdentityServerBuilder builder, Action<EidasLightOptions> setupAction)
        {
            builder.Services.Configure(setupAction);
            return builder.AddEidasLight();
        }

        public static IIdentityServerBuilder AddEidasLight(this IIdentityServerBuilder builder, IConfiguration configuration)
        {
            builder.Services.Configure<EidasLightOptions>(configuration);
            return builder.AddEidasLight();
        }
    }
}