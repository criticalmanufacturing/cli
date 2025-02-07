using Microsoft.TemplateEngine.Abstractions.PhysicalFileSystem;
using Microsoft.TemplateEngine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Cmf.CLI.Core.Templating
{
    /// <summary>
    /// Adapter to allow using an <see cref="System.IO.Abstractions.IFileSystem"/> anywhere where a 
    /// <see cref="Microsoft.TemplateEngine.Abstractions.PhysicalFileSystem.IPhysicalFileSystem"/> was expected.
    /// </summary>
    public class IOAbstractionsFileSystem : IPhysicalFileSystem
    {
        private readonly IFileSystem _basis;

        public IOAbstractionsFileSystem(IFileSystem basis)
        {
            _basis = basis;
        }

        public void CreateDirectory(string path)
        {
            _basis.Directory.CreateDirectory(path);
        }

        public Stream CreateFile(string path)
        {
            return _basis.File.Create(path);
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            _basis.Directory.Delete(path, recursive);
        }

        public bool DirectoryExists(string directory)
        {
            return _basis.Directory.Exists(directory);
        }

        public IEnumerable<string> EnumerateDirectories(string path, string pattern, SearchOption searchOption)
        {
            return _basis.Directory.EnumerateDirectories(path, pattern, searchOption);
        }

        public IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption searchOption)
        {
            return _basis.Directory.EnumerateFiles(path, pattern, searchOption);
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string directoryName, string pattern, SearchOption searchOption)
        {
            return _basis.Directory.EnumerateFileSystemEntries(directoryName, pattern, searchOption);
        }

        public void FileCopy(string sourcePath, string targetPath, bool overwrite)
        {
            _basis.File.Copy(sourcePath, targetPath, overwrite);
        }

        public void FileDelete(string path)
        {
            _basis.File.Delete(path);
        }

        public bool FileExists(string file)
        {
            return _basis.File.Exists(file);
        }

        public string GetCurrentDirectory()
        {
            return _basis.Directory.GetCurrentDirectory();
        }

        public Stream OpenRead(string path)
        {
            return _basis.File.OpenRead(path);
        }

        public string ReadAllText(string path)
        {
            return _basis.File.ReadAllText(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            return _basis.File.ReadAllBytes(path);
        }

        public void WriteAllText(string path, string value)
        {
            _basis.File.WriteAllText(path, value);
        }

        public FileAttributes GetFileAttributes(string file)
        {
            return _basis.File.GetAttributes(file);
        }

        public void SetFileAttributes(string file, FileAttributes attributes)
        {
            _basis.File.SetAttributes(file, attributes);
        }

        public DateTime GetLastWriteTimeUtc(string file)
        {
            return _basis.File.GetLastWriteTimeUtc(file);
        }

        public void SetLastWriteTimeUtc(string file, DateTime lastWriteTimeUtc)
        {
            _basis.File.SetLastAccessTimeUtc(file, lastWriteTimeUtc);
        }

        public string PathRelativeTo(string target, string relativeTo)
        {
            return _basis.Path.GetRelativePath(relativeTo, target);
        }

        /// <summary>
        /// Currently not implemented in <see cref="IOAbstractionsFileSystem"/>.
        /// Just returns <see cref="IDisposable"/> object, but never calls callback.
        /// </summary>
        public IDisposable WatchFileChanges(string filePath, FileSystemEventHandler fileChanged)
        {
            return new MemoryStream();
        }
    }
}
