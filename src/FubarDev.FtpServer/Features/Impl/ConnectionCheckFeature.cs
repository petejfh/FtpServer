using System;
using System.Collections.Generic;

using FubarDev.FtpServer.Events;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.Features.Impl
{
    public class ConnectionCheckFeature : IConnectionCheckFeature, IDisposable
    {
        private readonly DateTime _connectTimeUtc;
        private readonly TimeSpan _inactivityTimeout;
        private readonly HashSet<string> _activeDataTransfers;
        private readonly IDisposable _subscription;
        private readonly ILogger<IConnectionCheckFeature> _logger;

        private DateTime _utcLastActiveTime;
        private DateTime? _utcExpirationTime;

        public ConnectionCheckFeature(IOptions<FtpConnectionOptions> options, IFtpConnection connection, ILogger<IConnectionCheckFeature> logger)
        {
            _logger = logger;
            _inactivityTimeout = options.Value.InactivityTimeout ?? TimeSpan.MaxValue;
            _connectTimeUtc = DateTime.UtcNow;
            _activeDataTransfers = new HashSet<string>();
            UpdateLastActiveTime("Created");

            if (connection is IObservable<IFtpConnectionEvent> observable)
            {
                _subscription = observable.Subscribe(new EventObserver(this));
            }
            else
            {
                _subscription = null;
            }
        }

        public DateTime LastActiveTimeUtc => _utcLastActiveTime;

        public HashSet<string> ActiveDataTransfers => _activeDataTransfers;

        public DateTime? ExpirationTimeUtc => _utcExpirationTime;

        public DateTime ConnectTimeUtc => _connectTimeUtc;

        public void Dispose()
        {
            _subscription.Dispose();
        }

        public void UpdateLastActiveTime(string reason)
        {
            DateTime previousLastActiveTime = _utcLastActiveTime;
            DateTime? previousExpirationTime = _utcExpirationTime;

            _utcLastActiveTime = DateTime.UtcNow;
            _utcExpirationTime = DateTime.UtcNow.Add(_inactivityTimeout);

            Log($"UpdateLastActiveTime. Previous: LastActive={previousLastActiveTime}, Expiration={previousExpirationTime}. New: LastActive={_utcLastActiveTime}, Expiration={_utcExpirationTime}. Reason: {reason}");
        }

        private void Log(string s)
        {
            _logger.LogInformation($"ConnectionCheckFeature: {s}");
        }

        private class EventObserver : IObserver<IFtpConnectionEvent>
        {
            private readonly IConnectionCheckFeature _feature;

            public EventObserver(IConnectionCheckFeature feature)
            {
                _feature = feature;
            }

            /// <inheritdoc />
            public void OnCompleted()
            {
                // Ignore, connection was closed.
            }

            /// <inheritdoc />
            public void OnError(Exception error)
            {
                // Ignore
            }

            /// <inheritdoc />
            public void OnNext(IFtpConnectionEvent value)
            {
                switch (value)
                {
                    case FtpConnectionCommandReceivedEvent e:
                        _feature.UpdateLastActiveTime($"FtpConnectionCommandReceivedEvent: {e.Command}");
                        break;

                    case FtpConnectionDataTransferStartedEvent e:
                        _feature.UpdateLastActiveTime($"FtpConnectionDataTransferStartedEvent: {e.TransferId}");
                        _feature.ActiveDataTransfers.Add(e.TransferId);
                        break;

                    case FtpConnectionDataTransferStoppedEvent e:
                        _feature.UpdateLastActiveTime($"FtpConnectionDataTransferStoppedEvent: {e.TransferId}");
                        _feature.ActiveDataTransfers.Remove(e.TransferId);
                        break;
                }
            }
        }
    }
}
