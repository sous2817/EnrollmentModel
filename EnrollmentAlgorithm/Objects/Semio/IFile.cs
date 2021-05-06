using System.IO;

namespace Semio.Core.IO
{
    public interface IFile
    {
        string Name { get; }

        string FullName { get; }

        IDirectory Directory { get; }

        string Extension { get; }

        bool Exists { get; }

        FileInfo AdditionalAttributes { get; }

        Stream Open(FileMode mode);

        Stream Open(FileMode mode, FileAccess access, FileShare share);

        byte[] ReadAll();

        string[] ReadAllLines();

        string ReadAllText();

        void Delete();

        void WriteAllText(string contents);
    }
}