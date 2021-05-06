using System.Collections.Generic;

namespace Semio.Core.IO
{
    public interface IDirectory
    {
        string Name { get; }
        string FullName { get; }
        bool Exists { get; }
        IEnumerable<IDirectory> SubDirectories { get; }
        IEnumerable<IDirectory> SubDirectoriesRecursive { get; }
        IEnumerable<IFile> Files { get; }
        IEnumerable<IFile> FilesRecursive { get; }

        void Delete();

        void Delete(bool recursive);
    }
}