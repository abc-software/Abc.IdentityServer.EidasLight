using Abc.IdentityServer.EidasLight.Validation;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.ResponseProcessing.UnitTests
{
    public class SignInInteractionResponseGeneratorFixture
    {
        private readonly IdentityServerOptions _options = new IdentityServerOptions();
        private readonly SignInInteractionResponseGenerator _target;
        private readonly StubClock _clock = new StubClock();

        public SignInInteractionResponseGeneratorFixture()
        {
            _target = new SignInInteractionResponseGenerator(
                _clock,
                TestLogger.Create<SignInInteractionResponseGenerator>());
        }

        [Fact]
        public async Task Anonymous_User_must_SignIn()
        {
            var request = new ValidatedEidasLightRequest
            {
                ClientId = "foo",
                Subject = Principal.Anonymous,
            };

            var result = await _target.ProcessInteractionAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_must_not_SignIn()
        {
            var request = new ValidatedEidasLightRequest
            {
                ClientId = "foo",
                ValidatedResources = new ResourceValidationResult(),
                Subject = new Ids.IdentityServerUser("123")
                {
                    IdentityProvider = Ids.IdentityServerConstants.LocalIdentityProvider,
                }.CreatePrincipal(),
                Client = new Client(),
            };

            var result = await _target.ProcessInteractionAsync(request);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_allowed_current_Idp_must_not_SignIn()
        {
            var request = new ValidatedEidasLightRequest
            {
                ClientId = "foo",
                Subject = new Ids.IdentityServerUser("123")
                {
                    IdentityProvider = Ids.IdentityServerConstants.LocalIdentityProvider,
                }.CreatePrincipal(),
                Client = new Client
                {
                    IdentityProviderRestrictions = new List<string>
                    {
                        Ids.IdentityServerConstants.LocalIdentityProvider,
                    },
                },
            };

            var result = await _target.ProcessInteractionAsync(request);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_restricted_current_Idp_must_SignIn()
        {
            var request = new ValidatedEidasLightRequest
            {
                ClientId = "foo",
                Subject = new Ids.IdentityServerUser("123")
                {
                    IdentityProvider = Ids.IdentityServerConstants.LocalIdentityProvider,
                }.CreatePrincipal(),
                Client = new Client
                {
                    EnableLocalLogin = false,
                    IdentityProviderRestrictions = new List<string>
                    {
                        "some_idp",
                    },
                },
            };

            var result = await _target.ProcessInteractionAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_within_client_user_sso_lifetime_should_not_signin()
        {
            var request = new ValidatedEidasLightRequest
            {
                ClientId = "foo",
                Subject = new Ids.IdentityServerUser("123")
                {
                    IdentityProvider = "local",
                    AuthenticationTime = _clock.UtcNow.UtcDateTime.Subtract(TimeSpan.FromSeconds(10)),
                }.CreatePrincipal(),
                Client = new Client()
                {
                    UserSsoLifetime = 3600, // 1h
                },
            };

            var result = await _target.ProcessInteractionAsync(request);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_beyond_client_user_sso_lifetime_should_signin()
        {
            var request = new ValidatedEidasLightRequest
            {
                ClientId = "foo",
                Subject = new Ids.IdentityServerUser("123")
                {
                    IdentityProvider = "local",
                    AuthenticationTime = _clock.UtcNow.UtcDateTime.Subtract(TimeSpan.FromSeconds(3700)),
                }.CreatePrincipal(),
                Client = new Client()
                {
                    UserSsoLifetime = 3600, // 1h
                },
            };

            var result = await _target.ProcessInteractionAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task locally_authenticated_user_but_client_does_not_allow_local_should_sign_in()
        {
            var request = new ValidatedEidasLightRequest
            {
                ClientId = "foo",
                Subject = new Ids.IdentityServerUser("123")
                {
                    IdentityProvider = Ids.IdentityServerConstants.LocalIdentityProvider,
                }.CreatePrincipal(),
                Client = new Client()
                {
                    EnableLocalLogin = false,
                },
            };

            var result = await _target.ProcessInteractionAsync(request);

            result.IsLogin.Should().BeTrue();
        }
    }
}