using Semio.Core.IO;
using System;

namespace Semio.ClinWeb.Common.Exporters
{
    public class FileHeaderOptions
    {
        public string UserName { get; private set; }

        public IFile File { get; private set; }

        public DateTime ExportDateTime
        {
            get
            {
                return DateTime.Now;
            }
        }

        public string ExportDateFormatted
        {
            get
            {
                return ExportDateTime.ToString(Constants.Formats.DATE);
            }
        }

        public string ExportTimeFormatted
        {
            get
            {
                return ExportDateTime.ToString(Constants.Formats.TIME);
            }
        }

        public FileHeaderOptions(IFile file, string userName)
        {
            UserName = userName;
            File = file;
        }
    }
}