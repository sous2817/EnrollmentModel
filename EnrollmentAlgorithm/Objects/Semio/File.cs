using Semio.Core.Helpers;
using System.IO;

namespace Semio.Core.IO
{
    public class File : IFile
    {
        private readonly FileInfo _fileInfo;

        public File(string fullPathAndFile)
        {
            _fileInfo = new FileInfo(fullPathAndFile);
        }

        public string Name
        {
            get { return _fileInfo.Name; }
        }

        public string FullName
        {
            get { return _fileInfo.FullName; }
        }

        public IDirectory Directory
        {
            get { return new Directory(_fileInfo.DirectoryName); }
        }

        public string Extension
        {
            get { return _fileInfo.Extension; }
        }

        public bool Exists
        {
            get { return _fileInfo.Exists; }
        }

        public FileInfo AdditionalAttributes
        {
            get { return _fileInfo; }
        }

        public Stream Open(FileMode mode)
        {
            return Open(mode, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return System.IO.File.Open(_fileInfo.FullName, mode, access, share);
        }

        public byte[] ReadAll()
        {
            return new byte[0];
            //  return StreamHelper.ReadBinaryFile(_fileInfo.FullName);

        }

        public string[] ReadAllLines()
        {
            return System.IO.File.ReadAllLines(_fileInfo.FullName);
        }
        public string ReadAllText()
        {
            return System.IO.File.ReadAllText(_fileInfo.FullName);
        }

        public void Delete()
        {
            System.IO.File.Delete(_fileInfo.FullName);
        }

        public void WriteAllText(string contents)
        {
            System.IO.File.WriteAllText(_fileInfo.FullName, contents);
        }
    }
}