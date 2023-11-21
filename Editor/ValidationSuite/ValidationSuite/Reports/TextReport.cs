using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    class TextReport
    {
        public string FilePath { get; set; }

        readonly string m_PackageVersion;

        public TextReport(string packageId)
        {
            FilePath = ReportPath(packageId);

            // Ensure results directory exists before trying to write to it
            Directory.CreateDirectory(ValidationSuiteReport.ResultsPath);
        }

        internal TextReport(string packageId, string packageVersion)
            : this(packageId)
        {
            m_PackageVersion = packageVersion;
        }

        internal void Initialize(VettingContext context)
        {
            var packageInfo = context.ProjectPackageInfo;
            Write(
                string.Format("Validation Suite Results for package \"{0}\"\n", packageInfo.name) +
                string.Format(" - Path: {0}\n", packageInfo.path) +
                string.Format(" - Version: {0}\n", packageInfo.version) +
                string.Format(" - Type: {0}\n", context.PackageType) +
                string.Format(" - Context: {0}\n", context.ValidationType) +
                string.Format(" - Test Time: {0}\n", DateTime.Now) +
                string.Format(" - Tested with {0} version: {1}\n", context.VSuiteInfo.name, context.VSuiteInfo.version)
            );

            if (context.ProjectPackageInfo.dependencies.Any())
            {
                Append("\nPACKAGE DEPENDENCIES:\n");
                Append("--------------------\n");
                foreach (var dependencies in context.ProjectPackageInfo.dependencies)
                {
                    Append(string.Format("    - {0}@{1}\n", dependencies.Key, dependencies.Value));
                }
            }

            Append("\nVALIDATION RESULTS:\n");
            Append("------------------\n");
        }

        /// <summary>
        /// Deletes the report text file
        /// </summary>
        public void Clear()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        /// <summary>
        /// Writes to the report text file
        /// </summary>
        /// <param name="text"></param>
        void Write(string text)
        {
            File.WriteAllText(FilePath, text);
        }

        /// <summary>
        /// Appends a string to the report text file
        /// </summary>
        /// <param name="text"></param>
        public void Append(string text)
        {
            File.AppendAllText(FilePath, text);
        }

        /// <summary>
        /// Generates the report text file with the provided ValidationSuite
        /// </summary>
        /// <param name="suite"></param>
        public void GenerateReport(ValidationSuite suite)
        {
            SaveTestResult(suite, TestState.Failed);
            SaveTestResult(suite, TestState.Warning);
            SaveTestResult(suite, TestState.Succeeded);
            SaveTestResult(suite, TestState.NotRun);
            SaveTestResult(suite, TestState.NotImplementedYet);
        }

        void SaveTestResult(ValidationSuite suite, TestState testState)
        {
            if (suite.ValidationTests == null)
            {
                return;
            }

            foreach (var testResult in suite.ValidationTests.Where(t => t.TestState == testState))
            {
                Append(string.Format("\n{0} - \"{1}\"\n    ", testResult.TestState, testResult.TestName));
                foreach (var testOutput in testResult.TestOutput)
                {
                    Append(testOutput.ToString());
                    Append("\n\n    ");
                }
            }
        }

        /// <summary>
        /// Get the report text file path for the given Package ID
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static string ReportPath(string packageId)
        {
            return Path.Combine(ValidationSuiteReport.ResultsPath, packageId + ".txt");
        }

        /// <summary>
        /// Check if the report text file exists for the given Package ID
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static bool ReportExists(string packageId)
        {
            return File.Exists(ReportPath(packageId));
        }
    }
}
