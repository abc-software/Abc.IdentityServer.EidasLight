using Abc.IdentityServer.EidasLight.ResponseProcessing;
using Abc.IdentityServer.EidasLight.Validation;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.UnitTests
{
    internal class StubSignInInteractionResponseGenerator : ISignInInteractionResponseGenerator
    {
        internal InteractionResponse Response { get; set; } = new InteractionResponse();

        public Task<InteractionResponse> ProcessInteractionAsync(ValidatedEidasLightRequest request)
        {
            return Task.FromResult(Response);
        }
    }
}