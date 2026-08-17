// <copyright file="DefaultSslStreamWrapperFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Authentication
{
    /// <summary>
    /// The default implementation of the <see cref="ISslStreamWrapperFactory"/> interface.
    /// </summary>
    public class DefaultSslStreamWrapperFactory : ISslStreamWrapperFactory
    {
        private readonly ILogger<DefaultSslStreamWrapperFactory>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSslStreamWrapperFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultSslStreamWrapperFactory(
            ILogger<DefaultSslStreamWrapperFactory>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Stream> WrapStreamAsync(
            Stream unencryptedStream,
            bool keepOpen,
            X509Certificate certificate,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger?.LogTrace("Create SSL stream");
                var sslStream = CreateSslStream(unencryptedStream, keepOpen);

                try
                {
                    _logger?.LogTrace("Authenticate as server");

                    SslServerAuthenticationOptions opts = new SslServerAuthenticationOptions();
                    opts.ServerCertificate = certificate;
                    opts.ClientCertificateRequired = false;
                    opts.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    opts.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;
                    opts.EncryptionPolicy = EncryptionPolicy.RequireEncryption;
                    
#if NET8_0_OR_GREATER
                    // Left at the .NET default (true). Setting this to false was tried as a fix for
                    // the intermittent control-connection resets under concurrent load (see below) -
                    // ruled out by testing: the same failures recur with resumption disabled, and
                    // turning it off costs a real security property (FTPS clients like FileZilla
                    // warn when the data connection can't resume the control connection's session).
                    opts.AllowTlsResume = true;
#endif
                    // Server never requests client certs mid-session (ClientCertificateRequired is
                    // false and there's no protocol upgrade path); disabling renegotiation closes
                    // off the renegotiation DoS/MITM surface at no functional cost.
                    opts.AllowRenegotiation = false;

                    await sslStream.AuthenticateAsServerAsync(opts, cancellationToken)
                       .ConfigureAwait(false);
                    
                    _logger?.LogDebug("SSL/TLS authentication complete. Protocol: {Protocol}, Cipher: {Cipher}, KeyExchange: {KeyExchange}, Hash: {Hash}", 
                        sslStream.SslProtocol, 
                        sslStream.CipherAlgorithm, 
                        sslStream.KeyExchangeAlgorithm,
                        sslStream.HashAlgorithm);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "SSL/TLS authentication failed: {Message}", ex.Message);
                    sslStream.Dispose();
                    throw;
                }

                return sslStream;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to wrap stream in SSL: {Message}", ex.Message);
                throw;
            }
        }

#if NETCOREAPP || NET47
        /// <inheritdoc />
        public async Task CloseStreamAsync(Stream sslStream, CancellationToken cancellationToken)
        {
            if (sslStream is SslStream s)
            {
                try
                {
                    await s.ShutdownAsync().ConfigureAwait(false);

                    // Why is this needed? I get a GnuTLS error -110 when it's not called!
                    await Task.Yield();

                    await s.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error during SSL stream shutdown: {Message}", ex.Message);
                }
                finally
                {
                    try
                    {
                        s.Close();
                    }
                    catch
                    {
                        // Ignore close errors
                    }
                }
            }
        }
#else
        /// <inheritdoc />
        public Task CloseStreamAsync(Stream sslStream, CancellationToken cancellationToken)
        {
            if (sslStream is SslStream s)
            {
#if NET461 || NETSTANDARD2_0
                s.Close();
#else
                s.Dispose();
#endif
            }

            return Task.CompletedTask;
        }
#endif

        /// <summary>
        /// Create a new <see cref="SslStream"/> instance.
        /// </summary>
        /// <param name="unencryptedStream">The stream to wrap in an <see cref="SslStream"/> instance.</param>
        /// <param name="keepOpen">Keep the inner stream open.</param>
        /// <returns>The new <see cref="SslStream"/>.</returns>
        protected virtual SslStream CreateSslStream(
            Stream unencryptedStream,
            bool keepOpen)
        {
#if USE_GNU_SSL_STREAM
            return new GnuSslStream(unencryptedStream, keepOpen);
#else
            return new SslStream(unencryptedStream, keepOpen);
#endif
        }
    }
}
