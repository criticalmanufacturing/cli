using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests.Objects
{
    /// <summary>
    /// 
    /// </summary>
    public class CmfMockJsonData : MockFileData
    {
        public CmfMockJsonData(string textContents) : base(System.OperatingSystem.IsWindows() ? textContents.Replace("/", "\\\\") : textContents)
        {
        }
    }
}
