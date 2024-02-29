using Abc.IdentityModel.Protocols.EidasLight;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.UnitTests
{
    public class EidasLightProtocolSerializerExtensionFixture
    {
        private EidasLightRequest _request;

        public EidasLightProtocolSerializerExtensionFixture()
        {
            var request = new EidasLightRequest
            {
                CitizenCountryCode = "LV",
                Id = "id",
                Issuer = "issuer",
                NameIdFormat = new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"),
                SpType = EidasSpType.Public,
                ProviderName = "VPM",
                RelayState = "context",
                RequesterId = "req:request",
                SpCountryCode = "CA",
            };

            request.LevelsOfAssurance.Add(new LevelOfAssurance(new Uri("http://eidas.europa.eu/LoA/low"), LevelOfAssuranceType.Notified));

            request.RequestedAttributes.Add(new AttributeDefinition("http://www.stork.gov.eu/1.0/dateOfBirth"));
            request.RequestedAttributes.Add(new AttributeDefinition("http://www.stork.gov.eu/1.0/eIdentitfer"));

            _request = request;
        }

        [Fact]
        public void ReadWriteEidasMessageString() {
            var serializer = new EidasLightProtocolSerializer();
            var str = serializer.WriteMessageString(_request);

            var message = serializer.ReadMessageString(str);
            Assert(message);
        }

        [Fact]
        public void ReadWriteEidasMessageDictionary()
        {
            var serializer = new EidasLightProtocolSerializer();
            var dict = serializer.WriteMessageDictionary(_request);

            var message = serializer.ReadMessageDictionary(dict);
            Assert(message);
        }

        [Fact]
        public async Task ReadWriteEidasMessageStore()
        {
            var handleGenerationService = new DefaultHandleGenerationService();
            var optionsAccessor = Options.Create(new MemoryDistributedCacheOptions());
            var authorizationParametersMessageStore = new Ids.Stores.Default.DistributedCacheAuthorizationParametersMessageStore(new MemoryDistributedCache(optionsAccessor), handleGenerationService);

            var serializer = new EidasLightProtocolSerializer();
            var dict = serializer.WriteMessageDictionary(_request);
            var key = await authorizationParametersMessageStore.WriteAsync(new Message<IDictionary<string, string[]>>(dict, DateTime.UtcNow));

            var message = await authorizationParametersMessageStore.ReadAsync(key);
            var message0 = serializer.ReadMessageDictionary(message.Data);
            Assert(message0);
        }

        private void Assert(EidasLightMessage message)
        {
            message.Should().BeOfType<EidasLightRequest>();
            var request = message as EidasLightRequest;
            request.Id.Should().Be("id");
            request.Issuer.Should().Be("issuer");
            request.NameIdFormat.Should().Be(new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"));
            request.SpType.Should().Be(EidasSpType.Public);
            request.ProviderName.Should().Be("VPM");
            request.RelayState.Should().Be("context");
            request.SpCountryCode.Should().Be("CA");
        }
    }
}