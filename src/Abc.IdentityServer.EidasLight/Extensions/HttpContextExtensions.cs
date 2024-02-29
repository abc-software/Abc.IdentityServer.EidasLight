// ----------------------------------------------------------------------------
// <copyright file="HttpContextExtensions.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Microsoft.AspNetCore.Http
{
    internal static class HttpContextExtensions
    {
        public static string GetClientIpAddress(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress.ToString();
        }
    }
}