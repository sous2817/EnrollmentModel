using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Semio.ClientService.Data.Intelligence.Enrollment;

namespace EnrollmentAlgorithmTests.TestData
{
    public static class SampleData
    {
        public static EnrollmentCollection GetData(string solutionName, string fileName)
        {
            var sampleData = GetTestData(solutionName, fileName);
            return JsonConvert.DeserializeObject<EnrollmentCollection>(sampleData);
        }

        private static string GetTestData(string dataLocation, string dataFile)
        {
            var asm = Assembly.GetExecutingAssembly();
            var resource = $"{dataLocation}.{dataFile}";

            using (var stream = asm.GetManifestResourceStream(resource))
            {
                Debug.Assert(stream != null, "stream != null");
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

    }
}
