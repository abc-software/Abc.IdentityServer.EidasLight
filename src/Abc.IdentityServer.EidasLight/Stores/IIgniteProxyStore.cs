// ----------------------------------------------------------------------------
// <copyright file="IIgniteProxyStore.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Stores
{
    public interface IIgniteProxyStore
    {
        Task<IgniteProxyGetResult> GetAsync(string token);

        Task<string> StoreAsync(EidasLightResponse message);
    }
}