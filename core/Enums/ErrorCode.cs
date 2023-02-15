using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Enums
{
    public enum ErrorCode
    {
        Default = -1,
        Success = 0,
        InvalidArgument = 22 // 22 is invalid argument in bash
    }
}
