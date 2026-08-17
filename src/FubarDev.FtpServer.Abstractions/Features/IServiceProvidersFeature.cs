// <copyright file="IServiceProvidersFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Provides access to the <see cref="IServiceProvider"/> for the current connection.
    /// </summary>
    public interface IServiceProvidersFeature
    {
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> for the current connection.
        /// </summary>
        IServiceProvider RequestServices { get; }
    }
}
