using Cmf.CLI.Utilities;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using System.IO.Abstractions;
using System.IO;

namespace tests.Specs
{
    public class FileSystemUtilitiesTests
    {
        [Fact]
        public void SetUnixDirectoryPermissions_SetsPermissionsOnRealFileSystem()
        {
            // This test only runs on Unix-like systems where UnixFileMode is applicable
            if (!System.OperatingSystem.IsLinux() && !System.OperatingSystem.IsMacOS())
            {
                return;
            }

            var fileSystem = new FileSystem(); // real file system
            var tempDir = fileSystem.DirectoryInfo.New(System.IO.Path.GetTempPath() + "/test_permissions");
            tempDir.Create();

            try
            {
                var permissions = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                                  UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                                  UnixFileMode.OtherRead | UnixFileMode.OtherWrite;

                FileSystemUtilities.SetUnixDirectoryPermissions(fileSystem, tempDir, permissions, false);

                Assert.Equal(permissions, tempDir.UnixFileMode);
            }
            finally
            {
                tempDir.Delete(true);
            }
        }

        [Fact]
        public void SetUnixDirectoryPermissions_DoesNotThrowOnWindows()
        {
            // This test ensures the method doesn't throw on Windows
            if (System.OperatingSystem.IsLinux() || System.OperatingSystem.IsMacOS())
            {
                return;
            }

            var fileSystem = new MockFileSystem();
            var dir = fileSystem.DirectoryInfo.New("/test");
            dir.Create();

            var permissions = UnixFileMode.UserRead;

            // Should not throw
            FileSystemUtilities.SetUnixDirectoryPermissions(fileSystem, dir, permissions, false);
        }
    }
}