using System.Dynamic;

namespace Semio.ClinWeb.Common.Exporters
{
    public class ExportDataParameters<T>
    {
        public FileHeaderOptions FileHeaderOptions { get; private set; }

        public T DataContext { get; private set; }

        public ExportDataParameters(T dataContext) : this(null, dataContext)
        {
        }

        public ExportDataParameters(FileHeaderOptions fileHeaderOptions, T dataContext)
        {
            FileHeaderOptions = fileHeaderOptions;
            DataContext = dataContext;
            AdditionalData = new ExpandoObject();
        }

        public dynamic AdditionalData { get; private set; }
    }
}