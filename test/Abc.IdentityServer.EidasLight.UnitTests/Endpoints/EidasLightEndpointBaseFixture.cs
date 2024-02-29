using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Endpoints;
using Abc.IdentityServer.EidasLight.Endpoints.Results;
using Abc.IdentityServer.EidasLight.Events;
using Abc.IdentityServer.EidasLight.ResponseProcessing;
using Abc.IdentityServer.EidasLight.Stores;
using Abc.IdentityServer.EidasLight.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.UnitTests.Endpoints
{
    public class EidasLightEndpointBaseFixture
    {
        internal class TestEidasLightEndpoint : EidasLightEndpointBase
        {
            public TestEidasLightEndpoint(
                IEventService events, 
                IUserSession userSession, 
                ILogger<TestEidasLightEndpoint> logger, 
                IIgniteProxyStore igniteStore, 
                ISignInInteractionResponseGenerator interaction, 
                ISignInValidator signinValidator, 
                ISignInResponseGenerator generator) 
                : base(events, userSession, logger, igniteStore, interaction, signinValidator, generator)
            {
            }

            public override Task<IEndpointResult> ProcessAsync(HttpContext context)
            {
                throw new System.NotImplementedException();
            }
        }

        private TestEventService _fakeEventService = new TestEventService();
        private ILogger<TestEidasLightEndpoint> _fakeLogger = TestLogger.Create<TestEidasLightEndpoint>();
        private MockUserSession _mockUserSession = new MockUserSession();
        private StubIgniteProxyStore _mockIgniteProxyStore = new StubIgniteProxyStore();
        private StubSignInInteractionResponseGenerator _stubInteractionGenerator = new StubSignInInteractionResponseGenerator();
        private StubSignInResponseGenerator _stubResponseGenerator = new StubSignInResponseGenerator();
        private StubSignInRequestValidator _stubRequestValidator = new StubSignInRequestValidator();

        private ClaimsPrincipal _user = new Ids.IdentityServerUser("alise").CreatePrincipal();
        private EidasLightRequest _request = new EidasLightRequest();
        private TestEidasLightEndpoint _target;

        public EidasLightEndpointBaseFixture()
        {
            _fakeEventService.Clear();

            var validatedResult = new ValidatedEidasLightRequest()
            {
                ClientId = "client",
                Client = new Client
                {
                    ClientId = "client",
                    ClientName = "Test Client",
                },
                RedirectUri = "http://client/callback",
                Message = _request,
                Subject = _user,
            };

            _stubRequestValidator.Result = new SignInValidationResult(validatedResult);
            _stubInteractionGenerator.Response = new InteractionResponse();

            _target = new TestEidasLightEndpoint(
               _fakeEventService,
               _mockUserSession,
               _fakeLogger,
               _mockIgniteProxyStore,
               _stubInteractionGenerator,
               _stubRequestValidator,
               _stubResponseGenerator);
        }

        [Fact]
        public async Task signin_request_validation_produces_error_should_show_error_page()
        {
            _stubRequestValidator.Result.IsError = true;
            _stubRequestValidator.Result.Error = "some_validation_error";

            var result = await _target.ProcessAuthorizeRequestAsync(_request, _user);

            result.Should().BeOfType<ErrorPageResult>();
            ((ErrorPageResult)result).Error.Should().Be("some_validation_error");

            _fakeEventService.AssertEventWasRaised<SignInTokenIssuedFailureEvent>();
        }

        [Fact]
        public async Task signin_request_validation_produces_error_should_show_error_page_with_error_description_if_present()
        {
            _stubRequestValidator.Result.IsError = true;
            _stubRequestValidator.Result.Error = "some_validation_error";
            _stubRequestValidator.Result.ErrorDescription = "some error description";

            var result = await _target.ProcessAuthorizeRequestAsync(_request, _user);

            result.Should().BeOfType<ErrorPageResult>();
            ((ErrorPageResult)result).Error.Should().Be("some_validation_error");
            ((ErrorPageResult)result).ErrorDescription.Should().Be("some error description");

            _fakeEventService.AssertEventWasRaised<SignInTokenIssuedFailureEvent>();
        }

        [Fact]
        public async Task interaction_produces_error_should_show_error_page()
        {
            _stubInteractionGenerator.Response.Error = "some_interaction_error";

            var result = await _target.ProcessAuthorizeRequestAsync(_request, _user);

            result.Should().BeOfType<ErrorPageResult>();
            ((ErrorPageResult)result).Error.Should().Be("some_interaction_error");

            _fakeEventService.AssertEventWasRaised<SignInTokenIssuedFailureEvent>();
        }

        [Fact]
        public async Task interaction_produces_error_should_show_error_page_with_error_description_if_present()
        {
            _stubInteractionGenerator.Response.Error = "some_interaction_error";
            _stubInteractionGenerator.Response.ErrorDescription = "some error description";

            var result = await _target.ProcessAuthorizeRequestAsync(_request, _user);

            result.Should().BeOfType<ErrorPageResult>();
            ((ErrorPageResult)result).Error.Should().Be("some_interaction_error");
            ((ErrorPageResult)result).ErrorDescription.Should().Be("some error description");

            _fakeEventService.AssertEventWasRaised<SignInTokenIssuedFailureEvent>();
        }

        [Fact]
        public async Task interaction_produces_login_result_should_trigger_login()
        {
            _stubInteractionGenerator.Response.IsLogin = true;

            var result = await _target.ProcessAuthorizeRequestAsync(_request, _user);

            result.Should().BeOfType<LoginPageResult>();
        }

        [Fact]
        public async Task successful_authorization_request_should_generate_signin_result()
        {
            var result = await _target.ProcessAuthorizeRequestAsync(_request, _user);

            result.Should().BeOfType<SignInResult>();

            _fakeEventService.AssertEventWasRaised<SignInTokenIssuedSuccessEvent>();
        }
    }
}