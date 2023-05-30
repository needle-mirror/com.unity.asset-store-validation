using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Unity.asset-store-validation.Editor.Extension")]
namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    class ValidationSuiteReport
    {
        public static readonly string ResultsPath = Path.Combine("Library", "ASValidationSuiteResults");

        readonly string jsonReportPath;
        TextReport TextReport { get; set; }
        VettingReport VettingReport { get; set; }

        ValidationSuiteReportData ReportData { get; set; }

        public ValidationSuiteReport()
        { }

        public ValidationSuiteReport(string packageId, string packageName, string packageVersion, string packagePath)
        {
            jsonReportPath = Path.Combine(ResultsPath, packageId + ".json");

            // Ensure results directory exists before trying to write to it
            Directory.CreateDirectory(ResultsPath);

#if !UNITY_PACKAGE_MANAGER_DEVELOP_EXISTS
            TextReport = new TextReport(packageId, packageVersion);
#endif
            TextReport?.Clear();

            if (File.Exists(jsonReportPath))
                File.Delete(jsonReportPath);
        }

        internal void Initialize(VettingContext context)
        {
            TextReport?.Initialize(context);
        }

        ValidationTestReport[] BuildReport(ValidationSuite suite)
        {
            var testReports = new ValidationTestReport[suite.ValidationTests.Count()];
            var i = 0;
            foreach (var validationTest in suite.ValidationTests)
            {
                testReports[i] = new ValidationTestReport();
                testReports[i].TestName = validationTest.TestName;
                testReports[i].TestDescription = validationTest.TestDescription;
                testReports[i].TestResult = validationTest.TestState.ToString();
                testReports[i].TestState = validationTest.TestState;
                testReports[i].TestOutput = validationTest.TestOutput.ToArray();
                testReports[i].StartTime = validationTest.StartTime.ToString();
                testReports[i].EndTime = validationTest.EndTime.ToString();
                var span = validationTest.EndTime - validationTest.StartTime;
                testReports[i].Elapsed = span.TotalMilliseconds > 1 ? (int)(span.TotalMilliseconds) : 1;
                i++;
            }

            return testReports;
        }

        /// <summary>
        /// Get the path to the report delta file for the given Package ID
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static string DiffsReportPath(string packageId)
        {
            return Path.Combine(ResultsPath, packageId + ".delta");
        }

        /// <summary>
        /// Validates if a text report exists for the given Package ID
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static bool ReportExists(string packageId)
        {
            return TextReport.ReportExists(packageId);
        }

        /// <summary>
        /// Get the path to the text report file for the given Package ID
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static string GetTextReportPath(string packageId)
        {
            return AssetStoreValidation.ValidationSuite.TextReport.ReportPath(packageId);
        }

        static string GetJsonReportPath(string packageId)
        {
            return Path.Combine(ResultsPath, packageId + ".json");
        }

        /// <summary>
        /// Validates if a JSON report exists for the given Package ID
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static bool JsonReportExists(string packageId)
        {
            return File.Exists(GetJsonReportPath(packageId));
        }

        /// <summary>
        /// Validates if a report delta exists for the given Package ID
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static bool DiffsReportExists(string packageId)
        {
            var deltaReportPath = Path.Combine(ResultsPath, packageId + ".delta");
            return File.Exists(deltaReportPath);
        }

        /// <summary>
        /// Get the JSON report data for the given Package ID
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static ValidationSuiteReportData GetReport(string packageId)
        {
            if (JsonReportExists(packageId))
                return null;

            return Utilities.GetDataFromJson<ValidationSuiteReportData>(GetJsonReportPath(packageId));
        }

        
        internal void OutputErrorReport(string error)
        {
            TextReport?.Append(error);
            ActivityLogger.Log(error);
        }
        
        internal void GenerateVettingReport(ValidationSuite suite)
        {
            VettingReport?.GenerateReport(suite);
        }

        /// <summary>
        /// Generate a Text report for the provided Validation Suite
        /// </summary>
        /// <param name="suite"></param>
        public void GenerateTextReport(ValidationSuite suite)
        {
            TextReport?.GenerateReport(suite);
        }

        public void GenerateJsonReport(ValidationSuite suite)
        {
            var testLists = BuildReport(suite);
            var span = suite.EndTime - suite.StartTime;

            ReportData = new ValidationSuiteReportData
            {
                Type = suite.context.ValidationType,
                TestResult = suite.testSuiteState,
                StartTime = suite.StartTime.ToString(),
                EndTime = suite.EndTime.ToString(),
                Elapsed = span.TotalMilliseconds > 1 ? (int)(span.TotalMilliseconds) : 1,
                Tests = testLists.ToList()
            };

            File.WriteAllText(jsonReportPath, JsonUtility.ToJson(ReportData));
        }
    }
}
