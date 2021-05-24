using System.IO;

namespace Cmf.Common.Cli.Objects
{
    /// <summary>
    ///
    /// </summary>
    public class FileToPack
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public FileInfo Source { get; set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public FileInfo Target { get; set; }

        /// <summary>
        /// Gets or sets the content to pack.
        /// </summary>
        /// <value>
        /// The content to pack.
        /// </value>
        public ContentToPack ContentToPack { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileToPack"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="contentToPack">The content to pack.</param>
        public FileToPack(FileInfo source, FileInfo target, ContentToPack contentToPack)
        {
            Source = source;
            Target = target;
            ContentToPack = contentToPack;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileToPack"/> class.
        /// </summary>
        public FileToPack()
        {
        }

        #endregion
    }
}