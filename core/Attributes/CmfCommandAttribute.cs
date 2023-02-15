namespace Cmf.CLI.Core.Attributes
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
        /// gets or sets the command Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the parent command name.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public string Parent { get; set; }

        /// <summary>
        /// Gets or sets the parent command id. This property has precedence to the Parent property.
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// Description for the command
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// should hide the command in help screens?
        /// </summary>
        public bool IsHidden { get; set; } = false;

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