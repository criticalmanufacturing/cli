using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Core.Objects;
using Moq;
using SMBLibrary;
using SMBLibrary.Client;
using Xunit;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Repository.Credentials;
using Microsoft.Extensions.DependencyInjection;

namespace tests.Specs
{
    public class CIFSClientTest
    {
        private readonly Mock<IRepositoryAuthStore> _repositoryAuthStoreMock;
        private readonly Mock<ISMBClient> _mockSmbClient;
        private readonly CIFSClient _cifsClient;

        public CIFSClientTest()
        {
            _repositoryAuthStoreMock = new Mock<IRepositoryAuthStore>();
            _repositoryAuthStoreMock.Setup(x => x.GetOrLoad()).ReturnsAsync(new CmfAuthFile());
            _repositoryAuthStoreMock.Setup(x => x.GetCredentialsFor<CIFSRepositoryCredentials>(It.IsAny<CmfAuthFile>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new BasicCredential { Domain = "CORP", Username = "root", Password = "qwerty" });

            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton(_repositoryAuthStoreMock.Object)
                .BuildServiceProvider();

            _mockSmbClient = new Mock<ISMBClient>();
            _cifsClient = new CIFSClient(new Uri(@"\\testServer\testShare"), _mockSmbClient.Object);

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
            _mockSmbClient.Verify(client => client.Login(
                It.Is("CORP", StringComparer.CurrentCulture),
                It.Is("root", StringComparer.CurrentCulture),
                It.Is("qwerty", StringComparer.CurrentCulture)
            ), Times.AtLeastOnce());
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

        // TODO Add test method for UNhappy path of PutFile

        [Fact]
        public void SharedFolder_PutFile_ShouldWriteFileToSharedFolder()
        {
            // Arrange
            var localFilePath = "localTestFile.txt";
            var localFileData = "Test data";    
            var remoteFilePath = "remoteTestFile.txt";
            var remoteShareFilePath = @$"Folder/{remoteFilePath}";
            var mockFileData = new MockFileData(localFileData);
            var mockFileSystem = new MockFileSystem();
            var mockFileStore = new Mock<ISMBFileStore>();
            
            var writtenData = new MemoryStream();

            mockFileSystem.AddFile(localFilePath, mockFileData);
            mockFileStore.Setup(store => store.CreateFile(
                out It.Ref<object>.IsAny,
                out It.Ref<FileStatus>.IsAny,
                remoteFilePath,
                It.IsAny<AccessMask>(),
                It.IsAny<SMBLibrary.FileAttributes>(),
                It.IsAny<ShareAccess>(),
                It.IsAny<CreateDisposition>(),
                It.IsAny<CreateOptions>(),
                null
            )).Returns(NTStatus.STATUS_SUCCESS);

            mockFileStore.Setup(store => store.WriteFile(
                out It.Ref<int>.IsAny,
                It.IsAny<object>(),
                It.IsAny<long>(),
                It.IsAny<byte[]>()
            )).Callback((out int numberOfBytesWritten, object handle, long offset, byte[] buffer) =>
            {
                writtenData.Write(buffer, 0, buffer.Length);
                numberOfBytesWritten = buffer.Length;
            }).Returns(NTStatus.STATUS_SUCCESS);

            _mockSmbClient.Setup(client => client.TreeConnect(It.IsAny<string>(), out It.Ref<NTStatus>.IsAny)).Returns(mockFileStore.Object);
            _mockSmbClient.Setup(client => client.MaxWriteSize).Returns(4096);

            var sharedFolder = new SharedFolder(new Uri(@"\\testServer\testShare\Folder"), _mockSmbClient.Object, mockFileSystem.FileSystem);

            // Act
            sharedFolder.PutFile(localFilePath, remoteFilePath);

            // Assert
            mockFileStore.Verify(store => store.CreateFile(
                out It.Ref<object>.IsAny,
                out It.Ref<FileStatus>.IsAny,
                remoteShareFilePath,
                It.IsAny<AccessMask>(),
                It.IsAny<SMBLibrary.FileAttributes>(),
                It.IsAny<ShareAccess>(),
                It.IsAny<CreateDisposition>(),
                It.IsAny<CreateOptions>(),
                null
            ), Times.Once);

            mockFileStore.Verify(store => store.WriteFile(
                out It.Ref<int>.IsAny,
                It.IsAny<object>(),
                It.IsAny<long>(),
                It.IsAny<byte[]>()
            ), Times.AtLeastOnce);

            writtenData.Seek(0, SeekOrigin.Begin);
            var resultingData = writtenData.ToArray();
            var originalData = Encoding.ASCII.GetBytes(localFileData);

            Assert.Equal(originalData, resultingData);
        }
    }
}