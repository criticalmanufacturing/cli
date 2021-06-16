using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Cmf.Common.Cli.Utilities
{
    /// <summary>
    ///
    /// </summary>
    public static class ExtensionMethods
    {
        #region Public Methods

        /// <summary>
        /// Determines whether a sequence contains any elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if the specified source has any; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasAny<TSource>(this IEnumerable<TSource> source)
        {
            return source != null && source.Any();
        }

        /// <summary>
        /// Determines whether [has] [the specified object].
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if [has] [the specified object]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Has(this IEnumerable<object> objects, object obj)
        {
            return objects != null && objects.Any(x => x.Equals(obj));
        }

        /// <summary>
        /// Gets the name of the property value from token.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public static object GetPropertyValueFromTokenName(this object obj, string token)
        {
            object result = null;

            if (token.IsToken())
            {
                string propertyName = GetTokenName(token);

                PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    result = propertyInfo.GetValue(obj, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Ignores the case equals.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool IgnoreCaseEquals(this string str, string value)
        {
            return str != null && value != null && str.Equals(value, System.StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the CMF package files from sub directories.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="packageType">Type of the package.</param>
        /// <param name="setDefaultValues"></param>
        /// <returns></returns>
        public static CmfPackageCollection LoadCmfPackagesFromSubDirectories(this IDirectoryInfo directory, PackageType packageType = PackageType.None, bool setDefaultValues = false)
        {
            CmfPackageCollection cmfPackages = new();

            IDirectoryInfo[] subDirectories = directory.GetDirectories();

            foreach (IDirectoryInfo subDirectory in subDirectories)
            {
                IFileInfo[] cmfPackageFiles = subDirectory.GetFiles(CliConstants.CmfPackageFileName, SearchOption.AllDirectories);
                foreach (IFileInfo cmfPackageFile in cmfPackageFiles)
                {
                    CmfPackage cmfPackage = CmfPackage.Load(cmfPackageFile, setDefaultValues);

                    if (packageType == PackageType.None || (packageType != PackageType.None && cmfPackage.PackageType == packageType))
                    {
                        cmfPackages.Add(cmfPackage);
                    }
                }
            }

            return cmfPackages;
        }

        /// <summary>
        /// Determines whether [is null or empty].
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if [is null or empty] [the specified object]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this object obj)
        {
            bool result = false;

            if (obj == null ||
                (obj.IsList() && (obj as IList).Count == 0))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Determines whether this instance is list.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is list; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsList(this object obj)
        {
            return obj != null && (obj is IList);
        }

        /// <summary>
        /// Gets the first (in document order) child element with the specified <see cref="XName" />.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="name">The <see cref="XName" /> to match.</param>
        /// <param name="ignoreCase">If set to <c>true</c> case will be ignored whilst searching for the <see cref="XElement" />.</param>
        /// <returns>
        /// A <see cref="XElement" /> that matches the specified <see cref="XName" />, or null.
        /// </returns>
        public static XElement Element(this XContainer element, XName name, bool ignoreCase)
        {
            var el = element.Element(name);
            if (el != null)
                return el;

            if (!ignoreCase)
                return null;

            var elements = element.Elements().Where(e => e.Name.LocalName.ToString().ToLowerInvariant() == name.ToString().ToLowerInvariant());
            return !elements.Any() ? null : elements.First();
        }

        /// <summary>
        /// Gets the package json file.
        /// </summary>
        /// <param name="packDirectory">The pack directory.</param>
        /// <returns></returns>
        public static object GetPackageJsonFile(this IDirectoryInfo packDirectory)
        {
            dynamic obj = null;

            IFileInfo packageJsonFile = packDirectory.GetFiles(CliConstants.PackageJson).FirstOrDefault();

            if (packageJsonFile != null)
            {
                obj = JsonConvert.DeserializeObject(packageJsonFile.ReadToString());
            }

            return obj;
        }

        /// <summary>
        /// Converts to camelcase.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string ToCamelCase(this string str)
        {
            return !string.IsNullOrEmpty(str) && str.Length > 1 ? char.ToLowerInvariant(str[0]) + str[1..] : str;
        }

        /// <summary>
        /// Determines whether this instance is directory.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>
        ///   <c>true</c> if the specified URI is directory; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDirectory(this Uri uri)
        {
            return (uri.HostNameType == UriHostNameType.Basic) || (uri.HostNameType == UriHostNameType.Dns && uri.IsUnc);
        }

        #region Private Methods

        /// <summary>
        /// Determines whether the specified value is token.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is token; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsToken(this string value)
        {
            return value.StartsWith(CliConstants.TokenStartElement) && value.EndsWith(CliConstants.TokenEndElement);
        }

        /// <summary>
        /// Gets the name of the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static string GetTokenName(this string token)
        {
            int startIndex = token.IndexOf(CliConstants.TokenStartElement, 0) + CliConstants.TokenStartElement.Length;
            int endIndex = token.IndexOf(CliConstants.TokenEndElement, startIndex);

            return token[startIndex..endIndex];
        }

        #endregion

        #endregion
    }
}