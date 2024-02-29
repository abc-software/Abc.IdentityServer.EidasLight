// ----------------------------------------------------------------------------
// <copyright file="Constants.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.IdentityServer.EidasLight
{
    internal static class Constants
    {
        public static class ProtocolTypes
        {
            public const string EidasLight = "eidaslight";
        }

        public static class EndpointNames
        {
            public const string EidasLightProxy = "eidaslightproxy";
            public const string EidasLightProxyCallback = "eidaslightproxycallback";
        }

        public static class ProtocolRoutePaths
        {
            public const string EidasLightProxy = "eidasproxy";
            public const string EidasLightProxyCallback = EidasLightProxy + "/callback";
        }

        public static class DefaultRoutePathParams
        {
            public const string MessageStoreId = "authzId";
        }
    }
}