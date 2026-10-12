//--------------------------------------------------------------------------------
//<FileInfo>
//  <copyright file="Settings.cs" company="Critical Manufacturing, SA">
//        <![CDATA[Copyright © Critical Manufacturing SA. All rights reserved.]]>
//  </copyright>
//  <Author>João Brandão</Author>
//</FileInfo>
//--------------------------------------------------------------------------------

#region Using Directives

using Cmf.Core.PageObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion Using Directives

namespace Settings
{
    /// <summary>
    /// Represents the text initialization class.
    /// </summary>
    /// <seealso cref="BaseContext" />
    [TestClass]
    public class Context : BaseContext
    {
        #region Private Variables
        #endregion

        #region Public Variables
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            BaseInit(context);
        }

        /// <summary>
        /// Ends this instance.
        /// </summary>
        [AssemblyCleanup]
        public static void End()
        {
            BaseEnd();
        }

        #endregion

        #region Private & Internal Methods
        #endregion

        #region Event handling Methods
        #endregion
    }
}
