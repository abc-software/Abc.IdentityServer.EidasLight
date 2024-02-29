using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.Endpoints.Results.UnitTests
{
    public class LoginPageResultFixture
    {
        private LoginPageResult _target;
        private ValidatedEidasLightRequest _request;
        private IdentityServerOptions _options;
        private ISystemClock _clock = new StubClock();
        private DefaultHttpContext _context;
        private EidasLightProtocolSerializer _protocolSerializer = new EidasLightProtocolSerializer();
        private AuthorizationParametersMessageStoreMock _authorizationParametersMessageStore;

        public LoginPageResultFixture()
        {
            _context = new DefaultHttpContext();
            _context.SetIdentityServerOrigin("https://server");
            _context.SetIdentityServerBasePath("/");
            _context.Response.Body = new MemoryStream();

            _options = new IdentityServerOptions();
            _options.UserInteraction.LoginUrl = "~/login";
            _options.UserInteraction.LoginReturnUrlParameter = "returnUrl";

            _authorizationParametersMessageStore = new AuthorizationParametersMessageStoreMock();

            _request = new ValidatedEidasLightRequest();

            _target = new LoginPageResult(_request, _options, _clock, _protocolSerializer, _authorizationParametersMessageStore);
        }

        [Fact]
        public async Task login_should_redirect_to_login_page_and_passs_info()
        {
            await _target.ExecuteAsync(_context);

            _authorizationParametersMessageStore.Messages.Count.Should().Be(1);
            _context.Response.StatusCode.Should().Be(302);

            var location = _context.Response.Headers["Location"].First();
            location.Should().StartWith("https://server/login");

            var query = QueryHelpers.ParseQuery(new Uri(location).Query);
            query["returnUrl"].First().Should().StartWith("/eidasproxy/callback");
            query["returnUrl"].First().Should().Contain("?authzId=" + _authorizationParametersMessageStore.Messages.First().Key);
        }
    }
}