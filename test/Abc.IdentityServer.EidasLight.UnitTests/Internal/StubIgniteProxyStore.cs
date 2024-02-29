using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Stores;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.UnitTests
{
    internal class StubIgniteProxyStore : IIgniteProxyStore
    {
        public IgniteProxyGetResult Result { get; set; }

        public Task<IgniteProxyGetResult> GetAsync(string token)
        {
            return Task.FromResult(Result);
        }

        public Task<string> StoreAsync(EidasLightResponse message)
        {
            return Task.FromResult("123");
        }
    }
}
