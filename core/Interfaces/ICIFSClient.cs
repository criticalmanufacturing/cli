using System.Collections.Generic;
using Core.Objects;

namespace Cmf.CLI.Core.Interfaces
{
    public interface ICIFSClient
    {
        public string Server { get; }
        public List<SharedFolder>? SharedFolders { get;}
        public bool IsConnected { get; }
        public void Connect();
        public void Disconnect();
    }
}