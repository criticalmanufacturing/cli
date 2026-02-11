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
        /// <exception cref="FileNotFoundException">Thrown when the specified file path does not exist.</exception>
        /// <exception cref="CliException">
        /// Thrown when the icon is not in PNG format or when it is not square shaped.
        /// </exception>
        public static bool IsIconValid(string path)
        {
            const string PNG_EXTENSION = "png";

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            using SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(path);

            bool isPNG = image.Metadata.DecodedImageFormat?.FileExtensions.Contains(PNG_EXTENSION) ?? false;

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
