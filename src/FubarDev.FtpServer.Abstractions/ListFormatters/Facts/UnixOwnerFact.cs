namespace FubarDev.FtpServer.ListFormatters.Facts
{
    public class UnixOwnerFact : IFact
    {
        public UnixOwnerFact(string owner)
        {
            Value = owner;
        }

        public string Name => "UNIX.uid";

        public string Value { get; }
    }
}
