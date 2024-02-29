// ----------------------------------------------------------------------------
// <copyright file="InMemoryRelyingPartyStore.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Stores
{
    public class InMemoryRelyingPartyStore : IRelyingPartyStore
    {
        private readonly IEnumerable<RelyingParty> relyingParties;

        public InMemoryRelyingPartyStore(IEnumerable<RelyingParty> relyingParties)
        {
            this.relyingParties = relyingParties;
        }

        public Task<RelyingParty> FindRelyingPartyByRealm(string realm)
        {
            return Task.FromResult(this.relyingParties.FirstOrDefault(r => r.Realm == realm));
        }
    }
}