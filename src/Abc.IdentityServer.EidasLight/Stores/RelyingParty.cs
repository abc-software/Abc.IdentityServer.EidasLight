// ----------------------------------------------------------------------------
// <copyright file="RelyingParty.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Abc.IdentityServer.EidasLight.Stores
{
    public class RelyingParty
    {
        public string Realm { get; set; }

        public string SamlNameIdentifierFormat { get; set; }

        public IDictionary<string, string> ClaimMapping { get; set; }
    }
}