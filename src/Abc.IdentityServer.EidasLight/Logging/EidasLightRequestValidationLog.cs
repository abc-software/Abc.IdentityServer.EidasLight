// ----------------------------------------------------------------------------
// <copyright file="EidasLightRequestValidationLog.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

using Abc.IdentityModel.Protocols.EidasLight;
using Abc.IdentityServer.EidasLight.Validation;
using IdentityModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abc.IdentityServer.EidasLight.Logging
{
    internal class EidasLightRequestValidationLog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EidasLightRequestValidationLog"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public EidasLightRequestValidationLog(ValidatedEidasLightRequest request)
        {
            if (request.Client != null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;

                AllowedRedirectUris = request.Client.RedirectUris;
            }

            if (request.Subject != null)
            {
                var subjectClaim = request.Subject.FindFirst(JwtClaimTypes.Subject);
                SubjectId = subjectClaim != null ? subjectClaim.Value : "anonymous";
            }

            RedirectUri = request.RedirectUri;
            Request = request.Message;
        }

        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string RedirectUri { get; set; }
        public IEnumerable<string> AllowedRedirectUris { get; set; }
        public string SubjectId { get; set; }
        public EidasLightRequest Request { get; set; }

        public override string ToString()
        {
            return LogSerializer.Serialize(this);
        }
    }
}