//-----------------------------------------------------------------------
// <copyright file="FtpLogExtensions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using FubarDev.FtpServer.Features;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for logging <see cref="FtpCommand"/> and <see cref="FtpResponse"/> objects.
    /// </summary>
    internal static class FtpLogExtensions
    {
        /// <summary>
        /// Logs a trace message with the data of the <see cref="FtpCommand"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use.</param>
        /// <param name="connection">The <see cref="IFtpConnection"/> to log.</param>
        /// <param name="command">The <see cref="FtpCommand"/> to log.</param>
        public static void Command(this ILogger log, IFtpConnection connection, FtpCommand command)
        {
            string username = "???";

            IAuthorizationInformationFeature? authInfoFeature = connection.Features.Get<IAuthorizationInformationFeature>();
            if (authInfoFeature != null)
            {
                if (authInfoFeature.FtpUser != null && authInfoFeature.FtpUser.Identity != null && authInfoFeature.FtpUser.Identity.Name != null)
                {
                    username = authInfoFeature.FtpUser.Identity.Name;
                }
            }

            var arguments = string.Equals(command.Name, "PASS", System.StringComparison.OrdinalIgnoreCase)
                ? @"**************** (password omitted)"
                : command.Argument;

            using (log.BeginScope("{Username}@{RemoteEndPoint}", username, connection.RemoteEndPoint))
            {
                log.LogInformation("{name} {arguments}", command.Name, arguments);
            }
        }

        /// <summary>
        /// Logs a trace message with the data of the <see cref="FtpResponse"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use.</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log.</param>
        public static void Trace(this ILogger log, IFtpResponse response)
        {
            if (response is FtpResponse ftpResponse)
            {
                log.LogTrace("{code} {message}", response.Code, ftpResponse.Message);
            }
            else
            {
                log.LogTrace(response.ToString());
            }
        }

        /// <summary>
        /// Logs a debug message with the data of the <see cref="FtpResponse"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use.</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log.</param>
        public static void Debug(this ILogger log, IFtpResponse response)
        {
            if (response is FtpResponse ftpResponse)
            {
                log.LogDebug("{code} {message}", response.Code, ftpResponse.Message);
            }
            else
            {
                log.LogDebug(response.ToString());
            }
        }

        /// <summary>
        /// Logs a info message with the data of the <see cref="FtpResponse"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use.</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log.</param>
        public static void Info(this ILogger log, IFtpResponse response)
        {
            if (response is FtpResponse ftpResponse)
            {
                log.LogInformation("{code} {message}", response.Code, ftpResponse.Message);
            }
            else
            {
                log.LogInformation(response.ToString());
            }
        }

        /// <summary>
        /// Logs a warning message with the data of the <see cref="FtpResponse"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use.</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log.</param>
        public static void Warn(this ILogger log, IFtpResponse response)
        {
            if (response is FtpResponse ftpResponse)
            {
                log.LogWarning("{code} {message}", response.Code, ftpResponse.Message);
            }
            else
            {
                log.LogWarning(response.ToString());
            }
        }

        /// <summary>
        /// Logs an error message with the data of the <see cref="FtpResponse"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use.</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log.</param>
        public static void Error(this ILogger log, IFtpResponse response)
        {
            if (response is FtpResponse ftpResponse)
            {
                log.LogError("{code} {message}", response.Code, ftpResponse.Message);
            }
            else
            {
                log.LogError(response.ToString());
            }
        }

        /// <summary>
        /// Logs a message with the data of the <see cref="FtpResponse"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use.</param>
        /// <param name="connection">The <see cref="IFtpConnection"/> to use.</param>
        /// <param name="response">The <see cref="FtpResponse"/> to log.</param>
        /// <remarks>
        /// It logs either a trace, debug, or warning message depending on the
        /// <see cref="FtpResponse.Code"/>.
        /// </remarks>
        public static void LogResponse(this ILogger log, IFtpConnection connection, string response)
        {
            string username = "???";

            IAuthorizationInformationFeature? authInfoFeature = connection.Features.Get<IAuthorizationInformationFeature>();
            if (authInfoFeature != null)
            {
                if (authInfoFeature.FtpUser != null && authInfoFeature.FtpUser.Identity != null && authInfoFeature.FtpUser.Identity.Name != null)
                {
                    username = authInfoFeature.FtpUser.Identity.Name;
                }
            }

            using (log.BeginScope("{Username}@{RemoteEndPoint}", username, connection.RemoteEndPoint))
            {
                log.LogInformation("{response}", response);
            }

            return;
        }

#if NETSTANDARD1_3
        internal static void LogError(
            this ILogger log,
            System.Exception exception,
            string message,
            params object[] args)
        {
            log.LogError(0, exception, message, args);
        }

        internal static void LogWarning(
            this ILogger log,
            System.Exception exception,
            string message,
            params object[] args)
        {
            log.LogWarning(0, exception, message, args);
        }

        internal static void LogCritical(
            this ILogger log,
            System.Exception exception,
            string message,
            params object[] args)
        {
            log.LogCritical(0, exception, message, args);
        }
#endif
    }
}
