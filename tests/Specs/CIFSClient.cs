using System;
using System.Collections.Generic;
using System.IO;
using Core.Objects;
using Cmf.CLI.Core.Interfaces;
using Moq;
using SMBLibrary;
using SMBLibrary.Client;
using Xunit;
using Cmf.CLI.Core.Repository.Credentials;

namespace tests.Specs
{
    public class CIFSClientTest
    {
        private readonly Mock<ISMBClient> _mockSmbClient;
        private readonly CIFSClient _cifsClient;

        public CIFSClientTest()
        {
            _mockSmbClient = new Mock<ISMBClient>();
            _cifsClient = new CIFSClient("testServer", new List<Uri> { new Uri(@"\\testServer\testShare") }, _mockSmbClient.Object);
        }

        [Fact]
        public void Connect_ShouldSetIsConnectedToTrue_WhenConnectionIsSuccessful()
        {
            // Arrange
            _mockSmbClient.Setup(client => client.Connect(It.IsAny<string>(), It.IsAny<SMBTransportType>())).Returns(true);
            _mockSmbClient.Setup(client => client.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(NTStatus.STATUS_SUCCESS);

            // Act
            _cifsClient.Connect();

            // Assert
            Assert.True(_cifsClient.IsConnected);
        }

        [Fact]
        public void Connect_ShouldSetIsConnectedToFalse_WhenConnectionFails()
        {
            // Arrange
            _mockSmbClient.Setup(client => client.Connect(It.IsAny<string>(), It.IsAny<SMBTransportType>())).Returns(false);

            // Act
            _cifsClient.Connect();

            // Assert
            Assert.False(_cifsClient.IsConnected);
        }

        [Fact]
        public void Disconnect_ShouldCallDisconnectOnSmbClient()
        {
            // Arrange
            _mockSmbClient.Setup(client => client.Disconnect());

            // Act
            _cifsClient.Disconnect();

            // Assert
            _mockSmbClient.Verify(client => client.Disconnect(), Times.Once);
        }

        [Fact]
        public void SharedFolder_GetFile_ShouldReturnFileStream_WhenFileExists()
        {
            // Arrange
            var mockFileStore = new Mock<ISMBFileStore>();
            var fileData = new byte[] { 1, 2, 3, 4 };
            mockFileStore.Setup(store => store.CreateFile(out It.Ref<object>.IsAny, out It.Ref<FileStatus>.IsAny, It.IsAny<string>(), It.IsAny<AccessMask>(), It.IsAny<SMBLibrary.FileAttributes>(), It.IsAny<ShareAccess>(), It.IsAny<CreateDisposition>(), It.IsAny<CreateOptions>(), null)).Returns(NTStatus.STATUS_SUCCESS);
            mockFileStore.Setup(store => store.ReadFile(out fileData, It.IsAny<object>(), It.IsAny<long>(), It.IsAny<int>())).Returns(NTStatus.STATUS_END_OF_FILE);

            _mockSmbClient.Setup(client => client.TreeConnect(It.IsAny<string>(), out It.Ref<NTStatus>.IsAny)).Returns(mockFileStore.Object);

            var sharedFolder = new SharedFolder(new Uri(@"\\testServer\testShare\Folder"), _mockSmbClient.Object);

            // Act
            var result = sharedFolder.GetFile("testFile.txt");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(new Uri(@"\\testServer\testShare\Folder\testFile.txt"), result.Item1);
        }
    }
}