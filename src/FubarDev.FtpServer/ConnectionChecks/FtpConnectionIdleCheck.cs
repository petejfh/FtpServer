// <copyright file="FtpConnectionIdleCheck.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.ConnectionChecks
{
    /// <summary>
    /// An activity-based keep-alive detection.
    /// </summary>
    public class FtpConnectionIdleCheck : IFtpConnectionCheck
    {
        /// <inheritdoc />
        public FtpConnectionCheckResult Check(FtpConnectionCheckContext context)
        {
            IConnectionCheckFeature feature = context.Connection.Features.Get<IConnectionCheckFeature>();

            FtpConnectionCheckResult result;

            if (feature == null)
            {
                result = new FtpConnectionCheckResult(true);
            }
            else
            {
                if (feature.ExpirationTimeUtc == null)
                {
                    result = new FtpConnectionCheckResult(true);
                }
                else if (feature.ActiveDataTransfers.Count != 0)
                {
                    result = new FtpConnectionCheckResult(true);
                }
                else if (context.Connection.Features.Get<IBackgroundTaskLifetimeFeature?>() != null)
                {
                    result = new FtpConnectionCheckResult(true);
                }
                else if (DateTime.UtcNow <= feature.ExpirationTimeUtc.Value)
                {
                    result = new FtpConnectionCheckResult(true);
                }
                else
                {
                    result = new FtpConnectionCheckResult(false);
                }
            }

            return result;
        }
    }
}
