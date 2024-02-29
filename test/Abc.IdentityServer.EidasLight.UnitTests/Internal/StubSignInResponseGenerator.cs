using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.ResponseProcessing;
using Abc.IdentityServer.EidasLight.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.UnitTests
{
    internal class StubSignInResponseGenerator : ISignInResponseGenerator
    {
        public EidasLightResponse Response { get; set; } = new EidasLightResponse();

        public Task<EidasLightResponse> GenerateResponseAsync(SignInValidationResult validationResult)
        {
            return Task.FromResult(Response);
        }
    }
}