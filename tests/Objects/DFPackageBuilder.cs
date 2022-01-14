using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;

namespace tests.Objects
{
    public class DFPackageBuilder : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private readonly MemoryStream memoryStream;

        /// <summary>
        ///
        /// </summary>
        private readonly ZipArchive zipArchive;

        /// <summary>
        ///
        /// </summary>
        public DFPackageBuilder()
        {
            memoryStream = new MemoryStream();
            zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, false);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public DFPackageBuilder CreateEntry(string fileName, string content)
        {
            var demoFile = zipArchive.CreateEntry(fileName);

            using var entryStream = demoFile.Open();
            using var streamWriter = new StreamWriter(entryStream);
            streamWriter.Write(content);

            return this;
        }

        /// <summary>
        /// Create DeploymentFramework manifest
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public DFPackageBuilder CreateManifest(string id, string version, Dictionary<string, string> dependencies = null, Dictionary<string, string> testPackages = null)
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
            zipArchive.Dispose();
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
            zipArchive.Dispose();
            memoryStream.Dispose();
        }
    }
}
