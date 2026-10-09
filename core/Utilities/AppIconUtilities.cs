using Cmf.CLI.Utilities;
using System;
using System.IO;
using System.Linq;

namespace Cmf.CLI.Core.Utilities
{
    /// <summary>
    /// Icon utilities for cmf apps
    /// </summary>
    public static class AppIconUtilities
    {
        /// <summary>
        /// Checks if the specified icon image is of the correct type (PNG) and shape (square).
        /// </summary>
        /// <param name="path">The file path of the icon image to validate.</param>
        /// <returns>True if the icon is valid; otherwise, false.</returns>
        /// <exception cref="CliException">
        /// Thrown when the icon does not exist, is not in PNG format or is not square shaped.
        /// </exception>
        public static bool IsIconValid(string path)
        {
            const string PNG_EXTENSION = "png";

            if (!File.Exists(path))
            {
                throw new CliException("File not found.");
            }

            using SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(path);

            bool isPNG = image.Metadata.DecodedImageFormat.FileExtensions.Contains(PNG_EXTENSION);

            if (!isPNG)
            {
                throw new CliException("The icon provided is not PNG");
            }

            bool isSquareShaped = image.Height == image.Width;

            if (!isSquareShaped)
            {
                throw new CliException("The icon provided is not square shaped");
            }

            return true;
        }
    }
}
