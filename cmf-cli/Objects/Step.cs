using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Utilities;
using System;
using System.Xml.Serialization;

namespace Cmf.Common.Cli.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Step" />
    public class Step : IEquatable<Step>
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public StepType? Type { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the on execute.
        /// </summary>
        /// <value>
        /// The on execute.
        /// </value>
        public string OnExecute { get; set; }

        /// <summary>
        /// Gets or sets the content path.
        /// </summary>
        /// <value>
        /// The content path.
        /// </value>
        public string ContentPath { get; set; }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public string File { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [tag file].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tag file]; otherwise, <c>false</c>.
        /// </value>
        public bool? TagFile { get; set; }

        /// <summary>
        /// Gets or sets the target database.
        /// </summary>
        /// <value>
        /// The target database.
        /// </value>
        public string TargetDatabase { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public MessageType? MessageType { get; set; }
        
        /// <summary>
        /// the path of the file to load via masterdata
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// create master data in collection
        /// </summary>
        public bool? CreateInCollection { get; set; }

        /// <summary>
        /// the base path of the checklist images
        /// </summary>
        public string ChecklistImagePath { get; set; }

        /// <summary>
        /// the base path of the DEEs
        /// </summary>
        public string DeeBasePath { get; set; }

        /// <summary>
        /// the base path of the documents
        /// </summary>
        public string DocumentFileBasePath { get; set; }

        /// <summary>
        /// the base path for the maps
        /// </summary>
        public string MappingFileBasePath { get; set; }

        /// <summary>
        /// the base path for the automation workflows
        /// </summary>
        public string AutomationWorkflowFileBasePath { get; set; }

        /// <summary>
        /// the base path for the exported objects
        /// </summary>
        public string ImportXMLObjectPath { get; set; }

        /// <summary>
        /// the order of the steps
        /// </summary>
        [XmlIgnore]
        public int Order { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Step" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="title">The title.</param>
        /// <param name="onExecute">The on execute.</param>
        /// <param name="contentPath">The content path.</param>
        /// <param name="file">The file.</param>
        /// <param name="tagFile">The tag file.</param>
        /// <param name="targetDatabase">The target database.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <exception cref="ArgumentNullException">type</exception>
        public Step(StepType? type, string title, string onExecute, string contentPath, string file, bool? tagFile, string targetDatabase, MessageType? messageType)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Title = title;
            OnExecute = onExecute;
            ContentPath = contentPath;
            File = file;
            TagFile = tagFile;
            TargetDatabase = targetDatabase;
            MessageType = messageType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Step" /> class.
        /// </summary>
        public Step()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Step" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <exception cref="ArgumentNullException">type</exception>
        public Step(StepType? type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(Step other)
        {
            return other != null &&
                   Type == other.Type &&
                   Title.IgnoreCaseEquals(other.Title) &&
                   OnExecute.IgnoreCaseEquals(other.OnExecute) &&
                   ContentPath.IgnoreCaseEquals(other.ContentPath) &&
                   File.IgnoreCaseEquals(other.File) &&
                   TagFile == other.TagFile &&
                   TargetDatabase == other.TargetDatabase &&
                   MessageType == other.MessageType;
        }

        #endregion
    }
}