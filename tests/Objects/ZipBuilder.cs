using System;
using System.IO;
using System.IO.Compression;

namespace tests.Objects
{
    public class ZipBuilder : IDisposable
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
        public ZipBuilder()
        {
            memoryStream = new MemoryStream();
            zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public ZipBuilder CreateEntry(string fileName, string content)
        {
            var demoFile = zipArchive.CreateEntry(fileName);

            using var entryStream = demoFile.Open();
            using var streamWriter = new StreamWriter(entryStream);
            streamWriter.Write(content);

            return this;
        }

        /// <summary>
        /// ToByteArray
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            byte[] result = memoryStream.ToArray();
            Dispose();
            return result;
        }

        public void Dispose()
        {
            zipArchive.Dispose();
            memoryStream.Dispose();
        }
    }
}
