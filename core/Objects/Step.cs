using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Xml.Serialization;

namespace Cmf.CLI.Core.Objects
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
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(Order = 2)]
        public StepType? Type { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [JsonProperty(Order = 4)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the on execute.
        /// </summary>
        /// <value>
        /// The on execute.
        /// </value>
        [JsonProperty(Order = 6)]
        public string OnExecute { get; set; }

        /// <summary>
        /// Gets or sets the content path.
        /// </summary>
        /// <value>
        /// The content path.
        /// </value>
        [JsonProperty(Order = 5)]
        public string ContentPath { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [JsonProperty(Order = 10)]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        [JsonProperty(Order = 7)]
        public string File { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [tag file].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tag file]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(Order = 8)]
        public bool? TagFile { get; set; }

        /// <summary>
        /// Gets or sets the target database.
        /// </summary>
        /// <value>
        /// The target database.
        /// </value>
        [JsonProperty(Order = 9)]
        public string TargetDatabase { get; set; }
        
        /// <summary>
        /// Gets or sets the old system name.
        /// </summary>
        /// <value>
        /// The old system name.
        /// </value>
        [JsonProperty(Order = 11)]
        public string OldSystemName { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(Order = 3)]
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
        [JsonProperty(Order = 1)]
        public int Order { get; set; }

        /// <summary>
        /// Whether it is to run script against master database
        /// ProductDabatase Package Type specific
        /// </summary>
        public bool? RunInMaster { get; set; }

        /// <summary>
        /// Whether it is to run script against all product databases
        /// ProductDabatase Package Type specific
        /// </summary>
        public bool? TargetAll { get; set; }

        /// <summary>
        /// Gets or Sets the relativePath.
        /// Useful for step handlers that allow specifying relative paths for the content.
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// Gets or sets the target platform for master data content.
        /// Default value = self
        /// </summary>
        public MasterDataTargetPlatformType? TargetPlatform { get; set; }

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
        /// <param name="relativePath"></param>
        /// <param name="filePath">The file path.</param>
        /// <exception cref="ArgumentNullException">type</exception>
        public Step(StepType? type, string title, string onExecute, string contentPath, string file, bool? tagFile, string targetDatabase, MessageType? messageType, string relativePath, string filePath)
            : this(type, title, onExecute, contentPath, file, tagFile, targetDatabase, messageType, relativePath, filePath, null)
        { }
        
        public Step(StepType? type, string title, string onExecute, string contentPath, string file, bool? tagFile, string targetDatabase, MessageType? messageType, string relativePath, string filePath, string oldSystemName)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Title = title;
            OnExecute = onExecute;
            ContentPath = contentPath;
            File = file;
            TagFile = tagFile;
            TargetDatabase = targetDatabase;
            MessageType = messageType;
            RelativePath = relativePath;
            FilePath = filePath;
            OldSystemName = oldSystemName;
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
                   MessageType == other.MessageType &&
                   RelativePath == other.RelativePath &&
                   FilePath == other.FilePath &&
                   OldSystemName == other.OldSystemName;
        }

        #endregion
    }
}