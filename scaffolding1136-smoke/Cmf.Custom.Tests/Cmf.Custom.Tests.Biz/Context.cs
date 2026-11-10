using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Settings
{
    /// <summary>Initializes culture without logging in or changing MES roles.</summary>
    [TestClass]
    public class Context
    {
        /// <summary>Uses a deterministic culture for smoke-test serialization.</summary>
        /// <param name="context">The current test context.</param>
        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }
    }
}
