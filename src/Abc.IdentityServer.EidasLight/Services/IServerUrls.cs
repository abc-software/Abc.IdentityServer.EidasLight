// ----------------------------------------------------------------------------
// <copyright file="IServerUrls.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

#if IDS4 || IDS8

#if IDS4
namespace IdentityServer4.Services;
#else
namespace IdentityServer8.Services;
#endif

/// <summary>
/// Configures the per-request URLs and paths into the current server.
/// </summary>
public interface IServerUrls
{
    /// <summary>
    /// Gets or sets the origin for IdentityServer. For example, "https://server.acme.com:5001".
    /// </summary>
    string Origin { get; set; }

    /// <summary>
    /// Gets or sets the base path of IdentityServer.
    /// </summary>
    string BasePath { get; set; }

    /// <summary>
    /// Gets the base URL for IdentityServer.
    /// </summary>
    string BaseUrl { get => Origin + BasePath; }
}

#endif