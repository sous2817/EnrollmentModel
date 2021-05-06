using System;

namespace Semio.ClinWeb.Common.Interfaces
{
    public interface IWorkbookExporter<in T> : IDisposable
    {
        void ExportTo(string filename, T data);

        string UserName { get; set; }
    }
}