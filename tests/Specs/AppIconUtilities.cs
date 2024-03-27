using Cmf.CLI.Utilities;
using Cmf.Common.Cli.TestUtilities;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.IO;
using Xunit;
using Cmf.CLI.Core.Utilities;

namespace tests.Specs
{
    public class AppIconUtilitiesTests
    {
        private const string PNG_ICON_PATH = "valid_icon.png";
        private const string NON_SQUARE_ICON_PATH = "non_square_icon.png";
        private const string NON_PNG_ICON_PATH = "non_png_icon.jpg";

        public string TestFolder { get; private set; } = TestUtilities.GetTmpDirectory();

        private class ImageCreator
        {
            public static void CreatePngImage(string filePath, int width, int height)
            {
                using Image<Rgba32> image = new(width, height);
                image.SaveAsPng(filePath);
            }

            public static void CreateJpgImage(string filePath, int width, int height)
            {
                using Image<Rgba32> image = new(width, height);
                image.SaveAsJpeg(filePath);
            }
        }

        [Fact]
        public void ValidPngIcon_ReturnsTrue()
        {
            var path = Path.Combine(TestFolder, PNG_ICON_PATH);

            try
            {
                ImageCreator.CreatePngImage(Path.Combine(TestFolder, PNG_ICON_PATH), 100, 100);
                
                bool result = AppIconUtilities.IsIconValid(path);
                Assert.True(result);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void NonExistentIcon_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => AppIconUtilities.IsIconValid("nonexistent.png"));
        }

        [Fact]
        public void NonSquareIcon_ThrowsCliException()
        {
            var path = Path.Combine(TestFolder, NON_SQUARE_ICON_PATH);

            try
            {
                ImageCreator.CreatePngImage(Path.Combine(TestFolder, NON_SQUARE_ICON_PATH), 150, 200);
                
                var exception = Assert.Throws<CliException>(() => AppIconUtilities.IsIconValid(path));
                Assert.Equal("The icon provided is not square shaped", exception.Message);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void NonPngIcon_ThrowsCliException()
        {
            var path = Path.Combine(TestFolder, NON_PNG_ICON_PATH);
            
            try
            {
                ImageCreator.CreateJpgImage(Path.Combine(TestFolder, NON_PNG_ICON_PATH), 100, 100);

                var exception = Assert.Throws<CliException>(() => AppIconUtilities.IsIconValid(path));
                Assert.Equal("The icon provided is not PNG", exception.Message);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
