// ----------------------------------------------------------------------------
// <copyright file="NoRelyingPartyStore.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Stores
{
    public class NoRelyingPartyStore : IRelyingPartyStore
    {
        public Task<RelyingParty> FindRelyingPartyByRealm(string realm)
        {
            return Task.FromResult<RelyingParty>(null);
        }
    }
}