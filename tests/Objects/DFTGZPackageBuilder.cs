using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;

namespace tests.Objects
{
    public class DFTGZPackageBuilder : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private readonly MemoryStream memoryStream;

        /// <summary>
        ///
        /// </summary>
        private readonly GZipStream gzArchive;
        
        private readonly TarWriter tarArchive;

        /// <summary>
        ///
        /// </summary>
        public DFTGZPackageBuilder()
        {
            memoryStream = new MemoryStream();
            
            gzArchive = new GZipStream(memoryStream, CompressionLevel.NoCompression);
            
            tarArchive = new TarWriter(gzArchive);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public DFTGZPackageBuilder CreateEntry(string fileName, string content)
        {
            var entry = new V7TarEntry(TarEntryType.V7RegularFile, fileName);
            var entryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(entryStream);
            streamWriter.Write(content);
            streamWriter.Flush();
            entryStream.Seek(0, SeekOrigin.Begin);
            entry.DataStream = entryStream;
            tarArchive.WriteEntry(entry);
            
            return this;
        }

        /// <summary>
        /// Create DeploymentFramework manifest
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public DFTGZPackageBuilder CreateManifest(string id, string version, Dictionary<string, string> dependencies = null, Dictionary<string, string> testPackages = null)
        {
            return CreateEntry("manifest.xml",
                    @$"<?xml version=""1.0"" encoding=""utf-8""?>
                        <deploymentPackage>
                          <packageId>{id}</packageId>
                          <version>{version}</version>
                          {(dependencies != null
                                ? "<dependencies>" + string.Join(System.Environment.NewLine, dependencies.Select(d => @$"<dependency id=""{d.Key}"" version=""{d.Value}"" />")) + "</dependencies>"
                                : string.Empty)}
                          {(testPackages != null
                                ? "<testPackages>" + string.Join(System.Environment.NewLine, testPackages.Select(t => @$"<dependency id=""{t.Key}"" version=""{t.Value}"" />")) + "</testPackages>"
                                : string.Empty)}
                        </deploymentPackage>");
        }

        /// <summary>
        /// ToByteArray
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            // Flush ZipArchive
            // gzArchive.Flush();
            gzArchive.Dispose();
            byte[] result = memoryStream.ToArray();

            // Dispose MemoryStream
            memoryStream.Dispose();

            return result;
        }

        public MockFileData ToMockFileData()
        {
            return new MockFileData(ToByteArray());
        }

        public void Dispose()
        {
            tarArchive.Dispose();
            gzArchive.Dispose();
            memoryStream.Dispose();
        }
    }
}
