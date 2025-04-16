using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Repository.Credentials
{
    /// <summary>
    /// Optional interface that <see cref="IRepositoryCredentials" implementation classes
    /// can choose to additionally implement as well.
    /// 
    /// When implemented, indicates that the repository supports performing an automatic login, 
    /// without the user having to type in credentials/tokens when calling the command.
    /// 
    /// The primary use case for this is through Critical Manufacturing's Portal,
    /// where login is possible using the account the user has active in his default browser.
    /// </summary>
    public interface IRepositoryAutomaticLogin
    {
        Task<ICredential> AutomaticLogin();
    }
}
