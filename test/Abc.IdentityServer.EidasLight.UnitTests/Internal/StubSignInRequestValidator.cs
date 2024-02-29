using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Validation;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.UnitTests
{
    internal class StubSignInRequestValidator : ISignInValidator
    {
        public SignInValidationResult Result { get; set; }

        public Task<SignInValidationResult> ValidateAsync(EidasLightRequest message, ClaimsPrincipal user)
        {
            return Task.FromResult(Result);
        }
    }
}