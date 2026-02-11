using System;
using System.IO;

namespace Cmf.CLI.Core.Interfaces
{
    public interface ISharedFolder
    {
        public Tuple<Uri, Stream>? GetFile(string fileName);
    }
}