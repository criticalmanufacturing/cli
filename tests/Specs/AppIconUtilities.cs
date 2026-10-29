using Cmf.CLI.Utilities;
using Cmf.Common.Cli.TestUtilities;
using SkiaSharp;
using System.IO;
using Xunit;

namespace tests.Specs
{
    public class AppIconUtilities
    {
        private const string PNG_ICON_FILENAME = "valid_icon.png";
        private const string PNG_ICON_NO_EXTENSION_FILENAME = "valid_icon";
        private const string NON_SQUARE_ICON_FILENAME = "non_square_icon.png";
        private const string NON_PNG_ICON_FILENAME = "non_png_icon.jpg";

        public string TestFolder { get; private set; } = TestUtilities.GetTmpDirectory();

        private class ImageCreator
        {
            public static void CreatePngImage(string filePath, int width, int height)
            {
                CreateImage(filePath, width, height, SKEncodedImageFormat.Png);
            }

            public static void CreateJpgImage(string filePath, int width, int height)
            {
                CreateImage(filePath, width, height, SKEncodedImageFormat.Jpeg);
            }

            private static void CreateImage(string filePath, int width, int height, SKEncodedImageFormat format)
            {
                using SKBitmap bitmap = new(width, height);
                bitmap.Erase(SKColors.Transparent);
                using SKImage image = SKImage.FromBitmap(bitmap);
                using SKData data = image.Encode(format, 100);
                using FileStream output = File.Create(filePath);
                data.SaveTo(output);
            }
        }

        [Theory]
        [InlineData(PNG_ICON_NO_EXTENSION_FILENAME)]
        [InlineData(PNG_ICON_FILENAME)]
        [InlineData("valid_icon.jpg")]
        public void ValidPngIcon_ReturnsTrue(string filename)
        {
            var path = Path.Combine(TestFolder, filename);

            try
            {
                ImageCreator.CreatePngImage(Path.Combine(TestFolder, filename), 100, 100);

                bool result = Cmf.CLI.Core.Utilities.AppIconUtilities.IsIconValid(path);
                Assert.True(result, userMessage: "Icon is not valid, either not PNG or not square shaped.");
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void NonExistentIcon_ThrowsCliException()
        {
            Assert.Throws<CliException>(() => Cmf.CLI.Core.Utilities.AppIconUtilities.IsIconValid("nonexistent.png"));
        }

        [Fact]
        public void NonSquareIcon_ThrowsCliException()
        {
            var path = Path.Combine(TestFolder, NON_SQUARE_ICON_FILENAME);

            try
            {
                ImageCreator.CreatePngImage(Path.Combine(TestFolder, NON_SQUARE_ICON_FILENAME), 150, 200);

                var exception = Assert.Throws<CliException>(() => Cmf.CLI.Core.Utilities.AppIconUtilities.IsIconValid(path));
                Assert.Equal("The icon provided is not square shaped", exception.Message);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Theory]
        [InlineData(NON_PNG_ICON_FILENAME)]
        [InlineData("disguised_jpeg.png")]
        public void NonPngIcon_ThrowsCliException(string filename)
        {
            var path = Path.Combine(TestFolder, filename);

            try
            {
                ImageCreator.CreateJpgImage(path, 100, 100);

                var exception = Assert.Throws<CliException>(() => Cmf.CLI.Core.Utilities.AppIconUtilities.IsIconValid(path));
                Assert.Equal("The icon provided is not PNG", exception.Message);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void InvalidImage_ThrowsCliException()
        {
            var path = Path.Combine(TestFolder, "invalid.png");
            try
            {
                File.WriteAllText(path, "This is not an image.");
                var exception = Assert.Throws<CliException>(() => Cmf.CLI.Core.Utilities.AppIconUtilities.IsIconValid(path));
                Assert.Equal("The icon provided is not PNG", exception.Message);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
