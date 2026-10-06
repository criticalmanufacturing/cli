
#region Using Directives

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Cmf.Foundation.ApplicationVersion;


#endregion Using Directives

namespace <%= $CLI_PARAM_Organization %>.<%= $CLI_PARAM_Tenant %>.ApplicationVersion
{
    /// <summary>
    /// NRepresents the application version information gathering.
    /// </summary>
    [Export(typeof(IApplicationVersionInfo))]
    public sealed class <%= $CLI_PARAM_Tenant %>VersionInfo : IApplicationVersionInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets the application build date.
        /// </summary>
        public DateTime BuildDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the build number of the file.
        /// </summary>
        public int FileBuildPart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major part of the file version number.
        /// </summary>
        public int FileMajorPart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor part of the file version number.
        /// </summary>
        public int FileMinorPart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file private part.
        /// </summary>
        /// <value>
        /// The file private part.
        /// </value>
        public int FilePrivatePart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the build of the product this file is associated with.
        /// </summary>
        public int ProductBuildPart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major part of the file version number.
        /// </summary>
        public int ProductMajorPart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor part of the product this file is associated with.
        /// </summary>
        public int ProductMinorPart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the product private part.
        /// </summary>
        /// <value>
        /// The product private part.
        /// </value>
        public int ProductPrivatePart
        {
            get;
            set;
        }

        #endregion

        #region Constructors
        #endregion

        #region Private & Internal Methods
        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the version information.
        /// </summary>
        public void GetVersionInfo()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            FileInfo fi = new FileInfo(asm.Location);

            Version version = asm.GetName().Version;

            this.FileMajorPart = fvi.FileMajorPart;
            this.FileMinorPart = fvi.FileMinorPart;
            this.FileBuildPart = fvi.FileBuildPart;
            this.FilePrivatePart = fvi.FilePrivatePart;
            this.BuildDate = fi.LastWriteTimeUtc.ToLocalTime();

            this.ProductMajorPart = version.Major;
            this.ProductMinorPart = version.Minor;
            this.ProductBuildPart = version.Build;
            this.ProductPrivatePart = version.Revision;

            this.ProductName = fvi.ProductName;
        }

        #endregion

        #region Event handling Methods
        #endregion
    }
}
