using System.Diagnostics;
using System.IO;

namespace Semio.Core.IO
{
    public class IOFactory : IIOFactory
    {
        public string GetTempFilename()
        {
            return Path.GetTempFileName();
        }

        public string GetTempDirectory()
        {
            return Path.GetTempPath();
        }

        public Process StartProcess(string filename)
        {
            return Process.Start(filename);
        }

        public IFile GetFile(string filename)
        {
            return new File(filename);
        }

        public IDirectory GetDirectory(string directoryName)
        {
            return GetDirectory(directoryName, false);
        }

        public IDirectory GetDirectory(string directoryName, bool createIfNotExist)
        {
            if (createIfNotExist && !System.IO.Directory.Exists(directoryName))
                System.IO.Directory.CreateDirectory(directoryName);

            return new Directory(directoryName);
        }
    }
}