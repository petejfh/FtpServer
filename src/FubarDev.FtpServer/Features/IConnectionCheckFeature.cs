using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Features
{
    public interface IConnectionCheckFeature
    {
        DateTime ConnectTimeUtc { get; }
        DateTime LastActiveTimeUtc { get; }
        HashSet<string> ActiveDataTransfers { get; }
        DateTime? ExpirationTimeUtc { get; }

        void UpdateLastActiveTime(string reason);
    }
}
