// ----------------------------------------------------------------------------
// <copyright file="SignInResponseGenerator.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abc.IdentityServer.EidasLight.ResponseProcessing
{
    public class SignInResponseGenerator : ISignInResponseGenerator
    {
        private const string LevelOfAssuranceTypeProperty = "http://schemas.xmlsoap.org/ws/2005/05/identity/claimproperties/LevelOfAssurance";

        private readonly EidasLightOptions _options;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly Services.IClaimsService _claims;
        private readonly IResourceStore _resources;
        private readonly IIssuerNameService _issuerNameService;
        private readonly ILogger _logger;

        public SignInResponseGenerator(
            IHttpContextAccessor contextAccessor,
            EidasLightOptions options,
            Services.IClaimsService claimsService,
            IResourceStore resources,
            IIssuerNameService issuerNameService,
            ILogger<SignInResponseGenerator> logger)
        {
            _contextAccessor = contextAccessor;
            _options = options;
            _claims = claimsService;
            _resources = resources;
            _issuerNameService = issuerNameService;
            _logger = logger;
        }

        public async Task<EidasLightResponse> GenerateResponseAsync(SignInValidationResult validationResult)
        {
            _logger.LogDebug("Creating eIDAS light signin response");

            var outgoingSubject = await CreateSubjectAsync(validationResult);

            return await CreateResponseAsync(validationResult.ValidatedRequest, outgoingSubject);
        }

        protected virtual async Task<IList<string>> GetRequestedClaimTypesAsync(IEnumerable<string> scopes)
        {
            var requestedClaimTypes = new List<string>();

            var identityResources = await _resources.FindEnabledIdentityResourcesByScopeAsync(scopes);
            foreach (var resource in identityResources)
            {
                foreach (var claim in resource.UserClaims)
                {
                    requestedClaimTypes.Add(claim);
                }
            }

            return requestedClaimTypes;
        }

        protected virtual async Task<ClaimsIdentity> CreateSubjectAsync(SignInValidationResult result)
        {
            var validatedRequest = result.ValidatedRequest;
            var requestedClaimTypes = await GetRequestedClaimTypesAsync(validatedRequest.ValidatedResources.ParsedScopes.Select(x => x.ParsedName));
            var requestedClaims = new List<string>(requestedClaimTypes);

            var relyingParty = validatedRequest.RelyingParty;
            var claimsMapping =
                relyingParty?.ClaimMapping != null && relyingParty.ClaimMapping.Any()
                ? relyingParty.ClaimMapping
                : _options.DefaultClaimMapping;

            // requested claims
            foreach (var claimType in validatedRequest.Message.RequestedAttributes.Select(a => a.Definition))
            {
                var pair = claimsMapping.FirstOrDefault(x => x.Value == claimType);
                var jwtClaimType = pair.Key != null
                    ? pair.Key
                    : claimType;

                if (!requestedClaims.Contains(jwtClaimType))
                {
                    requestedClaims.Add(jwtClaimType);
                }
            }

            var issuedClaims = await _claims.GetClaimsAsync(validatedRequest, requestedClaims);

            var outboundClaims = new List<Claim>();
            outboundClaims.AddRange(_claims.MapClaims(claimsMapping, issuedClaims));

            if (!outboundClaims.Exists(x => x.Type == ClaimTypes.NameIdentifier))
            {
                var nameid = new Claim(ClaimTypes.NameIdentifier, validatedRequest.Subject.GetSubjectId());
                nameid.Properties[ClaimProperties.SamlNameIdentifierFormat] =
                    validatedRequest.RelyingParty?.SamlNameIdentifierFormat ?? _options.DefaultNameIdentifierFormat;
                outboundClaims.Add(nameid);
            }

            // by default low level of assurance
            if (!outboundClaims.Exists(x => x.Type == ClaimTypes.AuthenticationMethod))
            {
                outboundClaims.Add(new Claim(ClaimTypes.AuthenticationMethod, EidasConstants.AuthenticationContextClasses.LevelOfAssuranceLowString));
            }

            return new ClaimsIdentity(outboundClaims, "idsrv");
        }

        private async Task<EidasLightResponse> CreateResponseAsync(ValidatedEidasLightRequest validatedRequest, ClaimsIdentity outgoingSubject)
        {
            var eidasLightRequest = validatedRequest.Message;

            var eidasLightResponse = new EidasLightResponse()
            {
                InResponseToId = eidasLightRequest.Id,
                IpAddress = _contextAccessor.HttpContext.GetClientIpAddress(),
                Issuer = await _issuerNameService.GetCurrentAsync(),
                RelayState = eidasLightRequest.RelayState,
                Status = new EidasLightResponseStatus()
                {
                    Failure = false,
                    StatusCode = Tuple.Create(new Uri("urn:oasis:names:tc:SAML:2.0:status:Success"), (Uri)null),
                },
            };

            foreach (var claim in outgoingSubject.Claims)
            {
                switch (claim.Type)
                {
                    case ClaimTypes.NameIdentifier:
                        eidasLightResponse.Subject = claim.Value;
                        if (claim.Properties.ContainsKey(ClaimProperties.SamlNameIdentifierFormat))
                        {
                            eidasLightResponse.SubjectNameIdFormat = new Uri(claim.Properties[ClaimProperties.SamlNameIdentifierFormat]);
                        }

                        break;
                    case ClaimTypes.AuthenticationMethod:
                        if (claim.Properties.ContainsKey(LevelOfAssuranceTypeProperty))
                        {
                            LevelOfAssuranceType levelOfAssuranceType;
                            switch (claim.Properties[LevelOfAssuranceTypeProperty])
                            {
                                case "notified":
                                    levelOfAssuranceType = LevelOfAssuranceType.Notified;
                                    break;
                                case "nonnotified":
                                    levelOfAssuranceType = LevelOfAssuranceType.NonNotified;
                                    break;
                                default:
                                    throw new InvalidCastException("not allowed value");
                            }

                            eidasLightResponse.LevelOfAssurance = new LevelOfAssurance(new Uri(claim.Value), levelOfAssuranceType);
                            break;
                        }

                        eidasLightResponse.LevelOfAssurance = new LevelOfAssurance(new Uri(claim.Value));
                        break;
                    case ClaimTypes.AuthenticationInstant:
                        break;
                    default:
                        eidasLightResponse.Attributes.Add(new AttributeDefinition(claim.Type, claim.Value));
                        break;
                }
            }

            return eidasLightResponse;
        }
    }
}