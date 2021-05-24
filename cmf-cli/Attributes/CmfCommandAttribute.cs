namespace Cmf.Common.Cli.Attributes
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class CmfCommandAttribute : System.Attribute
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public string Parent { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="CmfCommandAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CmfCommandAttribute(string name = null)
        {
            this.Name = name;
        }

        #endregion
    }
}