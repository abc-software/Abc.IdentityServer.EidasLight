using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Endpoints;
using Abc.IdentityServer.EidasLight.Endpoints.Results;
using Abc.IdentityServer.EidasLight.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.UnitTests.Endpoints
{
    public class EidasLightProxyCallbackEndpointFixture
    {
        private HttpContext _context;

        private TestEventService _fakeEventService = new TestEventService();
        private ILogger<EidasLightProxyCallbackEndpoint> _fakeLogger = TestLogger.Create<EidasLightProxyCallbackEndpoint>();
        private MockUserSession _mockUserSession = new MockUserSession();
        private StubIgniteProxyStore _mockIgniteProxyStore = new StubIgniteProxyStore();
        private StubSignInInteractionResponseGenerator _stubInteractionGenerator = new StubSignInInteractionResponseGenerator();
        private StubSignInResponseGenerator _stubResponseGenerator = new StubSignInResponseGenerator();
        private StubSignInRequestValidator _stubRequestValidator = new StubSignInRequestValidator();
        private AuthorizationParametersMessageStoreMock _mockAuthorizationParametersMessageStore = new AuthorizationParametersMessageStoreMock();

        private ClaimsPrincipal _user = new Ids.IdentityServerUser("alise").CreatePrincipal();
        private EidasLightRequest _request = new EidasLightRequest();
        private EidasLightProxyCallbackEndpoint _target;

        public EidasLightProxyCallbackEndpointFixture()
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

            _target = new EidasLightProxyCallbackEndpoint(
               _fakeEventService,
               _mockUserSession,
               _fakeLogger,
               _mockIgniteProxyStore,
               _stubInteractionGenerator,
               _stubRequestValidator,
               _stubResponseGenerator,
               new EidasLightProtocolSerializer(),
               _mockAuthorizationParametersMessageStore);
        }

        [Fact]
        public async Task ProcessAsync_post_to_entry_point_should_return_405()
        {
            _context.Request.Method = "POST";

            var result = await _target.ProcessAsync(_context);

            result.Should().BeOfType<StatusCodeResult>();

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }

        [Fact]
        public async Task ProcessAsync_callback_after_login_path_should_return_authorization_result()
        {
            _mockAuthorizationParametersMessageStore.Messages.Add("134", new Message<Dictionary<string, string[]>>());

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/eidasproxy/callback");
            _context.Request.QueryString = new QueryString("?authzId=134");
            _mockUserSession.User = _user;

            var result = await _target.ProcessAsync(_context);

            result.Should().BeOfType<SignInResult>();
        }
    }
}