namespace Cmf.CLI.Core.Repository.Credentials
{
    
    public enum CredentialSource
    {
        /// <summary>
        /// Credential was manually registered by the user
        /// </summary>
        Manual,

        /// <summary>
        /// Credential automatically derived from a manual Credential for a SSO repository
        /// </summary>
        Derived,
    }
}
