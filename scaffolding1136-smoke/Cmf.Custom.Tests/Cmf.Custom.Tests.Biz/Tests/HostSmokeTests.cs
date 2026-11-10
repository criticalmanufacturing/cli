using System.Threading.Tasks;
using Cmf.Custom.Tests.Biz.Common;
using Cmf.Foundation.BusinessOrchestration.SecurityManagement.InputObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cmf.Custom.Tests.Biz.Tests
{
    /// <summary>Read-only Host requests using the generated MES LBO client.</summary>
    [TestClass]
    public class HostSmokeTests
    {
        /// <summary>Configures LBO transport before each test, without changing user roles.</summary>
        [TestInitialize]
        public void Initialize()
        {
            SmokeSettings.ConfigureLboClient();
        }

        /// <summary>Retrieves the configured MES user through an authenticated Host request.</summary>
        [TestMethod]
        [TestCategory("NightlySmoke")]
        [TestCategory("Host")]
        [Timeout(120000)]
        public async Task GetUserByAccount_ReturnsRequestedUser()
        {
            var expectedAccount = SmokeSettings.UserAccount;
            var response = await new GetUserByUserAccountInput
            {
                UserAccount = expectedAccount
            }.GetUserByUserAccountAsync(false);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.User);
            Assert.IsTrue(response.User.Id > 0, "The Host should return a persisted MES user.");
            Assert.AreEqual(expectedAccount, response.User.UserAccount, true);
        }

        /// <summary>Reads a user twice through different Host services and compares their identities.</summary>
        [TestMethod]
        [TestCategory("NightlySmoke")]
        [TestCategory("Host")]
        [Timeout(120000)]
        public async Task GetUserById_MatchesUserRetrievedByAccount()
        {
            var byAccount = await new GetUserByUserAccountInput
            {
                UserAccount = SmokeSettings.UserAccount
            }.GetUserByUserAccountAsync(false);
            Assert.IsNotNull(byAccount.User);

            var byId = await new GetUserByIdInput
            {
                Id = byAccount.User.Id,
                IsToLoadFromDataBase = true
            }.GetUserByIdAsync(false);

            Assert.IsNotNull(byId.User);
            Assert.AreEqual(byAccount.User.Id, byId.User.Id);
            Assert.AreEqual(byAccount.User.UserAccount, byId.User.UserAccount);
        }
    }
}
