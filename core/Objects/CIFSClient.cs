using System;
using System.Collections.Generic;
using System.IO;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Utilities;
using Microsoft.TemplateEngine.Utils;
using SMBLibrary;
using SMBLibrary.Client;

namespace Core.Objects
{
    public class CIFSClient : ICIFSClient
    {
        public string Server { get; private set; }
        public List<SharedFolder> SharedFolders { get; private set; }
        public bool IsConnected { get; private set; }

        private ISMBClient _smbClient;
        private string _domain;
        private string _username;
        private string _password;

        public CIFSClient(string server, IEnumerable<Uri> uris)
        {
            Server = server;
            _smbClient = new SMB2Client();
            _domain = Environment.GetEnvironmentVariable("CIFS_DOMAIN");
            _username = Environment.GetEnvironmentVariable("CIFS_USERNAME");
            _password = Environment.GetEnvironmentVariable("CIFS_PASSWORD");
            if(string.IsNullOrEmpty(_domain) && string.IsNullOrEmpty(_username) && string.IsNullOrEmpty(_password))
            {
                Log.Warning("CIFS credentials not found. Please set CIFS_DOMAIN, CIFS_USERNAME and CIFS_PASSWORD environment variables");
            }
            else
            {
                Connect();

                if(IsConnected)
                {
                    SharedFolders = [];
                    uris.ForEach(uri => SharedFolders.Add(new SharedFolder(uri, _smbClient)));
                }
            }
        }

        public void Connect()
        {
            Log.Debug($"Connecting to SMB server {Server} with username {_username}");
            IsConnected = _smbClient.Connect(Server, SMBTransportType.DirectTCPTransport);
            if (!IsConnected)
            {
                Log.Debug($"Failed to connect to {Server}");
                Log.Warning($"Failed to connect to {Server}");
            }

            var status = _smbClient.Login(_domain, _username, _password);
            if (status != NTStatus.STATUS_SUCCESS)
            {
                Log.Debug($"Fail status {status}");
                Log.Warning($"Failed to login to {Server} with username {_username}");
            }
        }

        public void Disconnect()
        {
            // Implement disconnection logic here
            Console.WriteLine("Disconnecting from SMB server");
        }
    }

    public class SharedFolder : ISharedFolder
    {
        public bool Exists { get; private set; }
        private ISMBFileStore _smbFileStore { get; set; }
        private ISMBClient _client { get; set; }
        private string _server { get; set; }
        private string _share { get; set; }
        private string _path { get; set; }
        private Uri _uri { get; set; }

        public SharedFolder(Uri uri, ISMBClient client)
        {
            _client = client;
            _server = uri.Host;
            _share = uri.PathAndQuery.Split("/")[1];
            _path = uri.PathAndQuery.Replace($"/{_share}", "").Substring(1);
            _uri = uri;
            Load();
        }

        private void Load()
        {
            _smbFileStore = _client.TreeConnect(_share, out NTStatus status);
            if (status != NTStatus.STATUS_SUCCESS)
            {
                Log.Debug($"Fail status {status}");
                Log.Warning($"Failed to connect to share {_share} on {_server}");
                Exists = false;
            }
            else
            {
                Exists = true;
            }
        }

        public Tuple<Uri, Stream> GetFile(string fileName)
        {
            Tuple<Uri, Stream> fileStream = null;
            var filepath = $"{_path}/{fileName}";
            var status = _smbFileStore.CreateFile(out object fileHandle, out FileStatus fileStatus, filepath, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Normal, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                var stream = new MemoryStream();
                long bytesRead = 0;
                while (true)
                {
                    status = _smbFileStore.ReadFile(out byte[] data, fileHandle, bytesRead, (int)_client.MaxReadSize);
                    if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_END_OF_FILE)
                    {
                        throw new Exception($"Failed to read file {filepath}");
                    }

                    if (status == NTStatus.STATUS_END_OF_FILE || data.Length == 0)
                    {
                        break;
                    }
                    bytesRead += data.Length;
                    stream.Write(data, 0, data.Length);
                }
                var uri = new Uri(_uri, filepath);
                fileStream = new Tuple<Uri, Stream>(uri, stream);
            }

            return fileStream;
        }
    }
}