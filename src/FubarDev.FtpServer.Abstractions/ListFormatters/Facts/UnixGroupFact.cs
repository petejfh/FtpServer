using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    public class UnixGroupFact : IFact
    {
        public UnixGroupFact(string group)
        {
            Value = group;
        }

        public string Name => "UNIX.gid";

        public string Value { get; }
    }
}
