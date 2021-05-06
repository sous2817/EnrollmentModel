using System.Diagnostics;

namespace Semio.Core.IO
{
    public interface IIOFactory
    {
        #region file operations (files, directories)

        IFile GetFile(string filename);

        IDirectory GetDirectory(string directoryName);

        IDirectory GetDirectory(string directoryName, bool createIfNotExist);

        string GetTempFilename();

        string GetTempDirectory();

        #endregion file operations (files, directories)

        #region process operations (start)

        Process StartProcess(string filename);

        #endregion process operations (start)
    }
}