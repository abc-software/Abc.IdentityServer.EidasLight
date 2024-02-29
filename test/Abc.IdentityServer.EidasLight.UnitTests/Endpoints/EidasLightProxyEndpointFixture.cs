using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Endpoints;
using Abc.IdentityServer.EidasLight.Endpoints.Results;
using Abc.IdentityServer.EidasLight.Events;
using Abc.IdentityServer.EidasLight.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.UnitTests.Endpoints
{
    public class EidasLightProxyEndpointFixture
    {
        private HttpContext _context;

        private TestEventService _fakeEventService = new TestEventService();
        private ILogger<EidasLightProxyEndpoint> _fakeLogger = TestLogger.Create<EidasLightProxyEndpoint>();
        private MockUserSession _mockUserSession = new MockUserSession();
        private StubIgniteProxyStore _mockIgniteProxyStore = new StubIgniteProxyStore();
        private StubSignInInteractionResponseGenerator _stubInteractionGenerator = new StubSignInInteractionResponseGenerator();
        private StubSignInResponseGenerator _stubResponseGenerator = new StubSignInResponseGenerator();
        private StubSignInRequestValidator _stubRequestValidator = new StubSignInRequestValidator();

        private ClaimsPrincipal _user = new Ids.IdentityServerUser("alise").CreatePrincipal();
        private EidasLightRequest _request = new EidasLightRequest();
        private EidasLightProxyEndpoint _target;
        public EidasLightProxyEndpointFixture()
        {
            _context = new DefaultHttpContext();
            _fakeEventService.Clear();

            _mockIgniteProxyStore.Result = new Stores.IgniteProxyGetResult(_request);

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

            _target = new EidasLightProxyEndpoint(
               _fakeEventService,
               _mockUserSession,
               _fakeLogger,
               _mockIgniteProxyStore,
               _stubInteractionGenerator,
               _stubRequestValidator,
               _stubResponseGenerator);
        }

        [Fact]
        public async Task ProcessAsync_get_should_return_405()
        {
            _context.Request.Method = "GET";

            var result = await _target.ProcessAsync(_context);

            result.Should().BeOfType<StatusCodeResult>();

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }

        [Fact]
        public async Task ProcessAsync_post_without_form_content_type_should_return_415()
        {
            _context.Request.Method = "POST";

            var result = await _target.ProcessAsync(_context);

            result.Should().BeOfType<StatusCodeResult>();

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(415);
        }

        [Fact]
        public async Task ProcessAsync_post_without_token_param_should_return_400()
        {
            _context.Request.Method = "POST";
            _context.Request.Form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()
                );

            var result = await _target.ProcessAsync(_context);

            result.Should().BeOfType<StatusCodeResult>();

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task signin_ignite_retrieving_produces_error_should_show_error_page()
        {
            _stubRequestValidator.Result.IsError = true;
            _stubRequestValidator.Result.Error = "some_ignite_error";

            var result = await _target.ProcessAuthorizeRequestAsync(_request, _user);

            result.Should().BeOfType<ErrorPageResult>();
            ((ErrorPageResult)result).Error.Should().Be("some_ignite_error");

            _fakeEventService.AssertEventWasRaised<SignInTokenIssuedFailureEvent>();
        }

        [Fact]
        public async Task signin_ignite_retrieving_produces_error_should_show_error_page_with_error_description_if_present()
        {
            _stubRequestValidator.Result.IsError = true;
            _stubRequestValidator.Result.Error = "some_ignite_error";
            _stubRequestValidator.Result.ErrorDescription = "some error description";

            var result = await _target.ProcessAuthorizeRequestAsync(_request, _user);

            result.Should().BeOfType<ErrorPageResult>();
            ((ErrorPageResult)result).Error.Should().Be("some_ignite_error");
            ((ErrorPageResult)result).ErrorDescription.Should().Be("some error description");

            _fakeEventService.AssertEventWasRaised<SignInTokenIssuedFailureEvent>();
        }

        [Fact]
        public async Task ProcessAsync_authorize_path_should_return_authorization_result()
        {
            _context.Request.Method = "POST";
            _context.Request.Path = new PathString("/eidaslight");
            _context.Request.Form = new FormCollection(
                new Dictionary<string, StringValues>()
                {
                    { "token", new StringValues("some_token") },
                });

            _mockUserSession.User = _user;

            var result = await _target.ProcessAsync(_context);

            result.Should().BeOfType<SignInResult>();
        }
    }
}