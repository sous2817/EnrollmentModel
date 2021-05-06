using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Semio.Core.IO
{
    public class Directory : IDirectory
    {
        private readonly DirectoryInfo _directoryInfo;

        public string Name
        {
            get { return _directoryInfo.Name; }
        }

        public string FullName
        {
            get { return _directoryInfo.FullName; }
        }

        public bool Exists
        {
            get { return _directoryInfo.Exists; }
        }

        public IEnumerable<IDirectory> SubDirectories
        {
            get { return _directoryInfo.GetDirectories().Select(directoryInfo => new Directory(directoryInfo.FullName)); }
        }

        public IEnumerable<IDirectory> SubDirectoriesRecursive
        {
            get { return _directoryInfo.GetDirectories("*.*", SearchOption.AllDirectories).Select(directoryInfo => new Directory(directoryInfo.FullName)); }
        }

        public IEnumerable<IFile> Files
        {
            get { return _directoryInfo.GetFiles().Select(fileInfo => new File(fileInfo.FullName)); }
        }

        public IEnumerable<IFile> FilesRecursive
        {
            get { return _directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Select(fileInfo => new File(fileInfo.FullName)); }
        }

        public void Delete()
        {
            Delete(false);
        }

        public void Delete(bool recursive)
        {
            System.IO.Directory.Delete(FullName, recursive);
        }

        public Directory(string directoryName)
        {
            _directoryInfo = new DirectoryInfo(directoryName);
        }
    }
}