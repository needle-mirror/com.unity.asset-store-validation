using System.IO;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    enum VettingReportEntryType
    {
        GeneralConcerns,

        LegalConcerns,

        SecurityConcerns,
    }

    class VettingReportEntry
    {
        VettingReportEntryType Type { get; set; }

        public string Entry { get; set; }
    }

    class VettingReport
    {
        internal static readonly string ResultsPath = Path.Combine("Library", "VettingReport");

        public VettingReport()
        {
        }

        public void GenerateReport(ValidationSuite suite)
        {
        }
    }
}
