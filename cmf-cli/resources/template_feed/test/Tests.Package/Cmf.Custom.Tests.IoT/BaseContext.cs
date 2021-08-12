//--------------------------------------------------------------------------------
//<FileInfo>
//  <copyright file="BaseContext.cs" company="Critical Manufacturing, SA">
//        <![CDATA[Copyright © Critical Manufacturing SA. All rights reserved.]]>
//  </copyright>
//  <Author>João Brandão</Author>
//</FileInfo>
//--------------------------------------------------------------------------------

#region Using Directives

using Cmf.Foundation.BusinessOrchestration.SecurityManagement.InputObjects;
using Cmf.Foundation.Security;
using Cmf.LightBusinessObjects.Infrastructure;
using Cmf.Navigo.BusinessObjects;
using Cmf.Navigo.BusinessOrchestration.LaborManagement.InputObjects;
using Cmf.TestScenarios.EmployeeHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

#endregion Using Directives

namespace Settings
{
    /// <summary>
    /// Represents the test base class
    /// </summary>
    public class BaseContext
    {
        #region Private Variables

        /// <summary>
        /// The configuration
        /// </summary>
        private static ClientConfiguration config = null;

        #endregion Private Variables

        #region Properties

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public static string Password
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public static string UserName
        {
            get;
            private set;
        }

        /// <summary>
        /// IoT Tests Mode
        /// </summary>
        /// <value>
        /// Enum with modes
        /// </value>
        public static IoTModes Mode
        {
            get;
            private set;
        } = IoTModes.Local;

        /// <summary>
        /// Gets the user role.
        /// </summary>
        /// <value>
        /// The user role.
        /// </value>
        public static string UserRole
        {
            get;
            private set;
        }

        /// <summary>
        /// Absolute Path to RunSettings File
        /// </summary>
        /// <value>
        /// Absolute Path to RunSettings File
        /// </value>
        public static string FilePath
        {
            get;
            private set;
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Ends this instance.
        /// </summary>
        public static void BaseEnd()
        {
            // Assembly clean up
        }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public static void BaseInit(TestContext context)
        {
            BaseContext.UserName = GetString(context, "userName");
            BaseContext.Password = GetString(context, "password");

            ClientConfigurationProvider.ConfigurationFactory = () =>
            {
                if (config == null)
                {
                    config = new ClientConfiguration()
                    {
                        HostAddress = System.IO.Directory.Exists(GetString(context, "hostAdress")) ? GetString(context, "hostAdress") : string.Format("{0}:{1}", GetString(context, "hostAdress"), int.Parse(GetString(context, "hostPort"))),
                        ClientTenantName = GetString(context, "clientTenantName"),
                        UseSsl = context.Properties.Contains("useSsl") ? bool.Parse(GetString(context, "useSsl")) : false,
                        ApplicationName = GetString(context, "applicationName"),
                        IsUsingLoadBalancer = context.Properties.Contains("useLoadBalancer") ? bool.Parse(GetString(context, "useLoadBalancer")) : false,
                        ThingsToDoAfterInitialize = null,
                        UserName = GetString(context, "userName"),
                        Password = GetString(context, "password"),
                        RequestTimeout = GetString(context, "requestTimeout")
                    };
                }
                return config;
            };

            UserRole = context.Properties.Contains("userRole") ? GetString(context, "userRole") : "Almost Admin";

            // Handle Culture
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(GetString(context, "culture"));

            #region Connect IoT

            if (context.Properties.Contains("mode"))
            {
                IoTModes ioTMode;
                Enum.TryParse(GetString(context, "mode"), out ioTMode);

                Mode = ioTMode;
            }
                        
            if (GetString(context, "TestRunDirectory").Contains("TestExecution"))
            {
                FilePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), GetString(context, "filePathRemote")));
            } else
            {
                FilePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), GetString(context, "filePathLocal")));
            }

            #endregion Connect IoT
        }

        /// <summary>
        /// Retries a given function after a period of time to avoid race condition
        /// </summary>
        public static void RetryRun(Action retryRun, int retryCount = 3, TimeSpan? span = null)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    retryRun();
                    break;
                }
                catch
                {
                    if (span == null)
                    {
                        Random random = new Random();
                        span = new TimeSpan(0, 0, random.Next(5, 15));
                    }

                    if (i + 1 == retryCount)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(span.Value);
                    }
                }
            }
        }

        #endregion Public Methods

        #region Private & Internal Methods

        /// <summary>
        /// Gets a string from the TestContext properties
        /// </summary>
        /// <param name="context">Test Context</param>
        /// <param name="property">Property to find</param>
        /// <returns>A string</returns>
        private static string GetString(TestContext context, string property)
        {
            if (context.Properties[property] == null)
            {
                throw new ArgumentException($"Property does not exist, does not have a value, or a test setting is not selected.", property);
            }
            else
            {
                return context.Properties[property].ToString();
            }
        }

        public enum IoTModes
        {
            Local,
            RemoteDownload,
            RemoteService
        }

        #endregion Private & Internal Methods
    }
}
