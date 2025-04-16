using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Enums
{
    public enum PropertyRequirement
    {
        /// <summary>
        /// Indicates that a certain property is not used by the component
        /// </summary>
        Ignored,

        /// <summary>
        /// Indicates that a certain property is used by a component, if present
        /// </summary>
        Optional,

        /// <summary>
        /// Indicates that a certain property must be present and is needed by the component to work
        /// </summary>
        Mandatory
    }
}
