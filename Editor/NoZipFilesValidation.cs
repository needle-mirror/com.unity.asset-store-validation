using System.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class NoZipFilesValidation : BaseFileExtensionValidation
    {
        internal static readonly string k_DocsFilePath = "no_zip_files_validation.html";
        internal static readonly string k_ErrorMessageHeader = "Zip files are automatically ignored when packing the package, please remove the following zip file(s) or convert them to .unitypackage files";

        readonly string[] k_RestrictedFileExtensions =
        {
            ".zip"
        };

        public NoZipFilesValidation()
        {
            TestName = "No Zip Files";
            TestDescription = "A package must not contain any zip files.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.Structure, ValidationType.AssetStore };
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            CheckForZipFiles(Context.ProjectPackageInfo.path);
        }

        void CheckForZipFiles(string packagePath)
        {
            var restrictedFiles = GetPathsToFilesWithExtension(packagePath, k_RestrictedFileExtensions);
            if (!restrictedFiles.Any()) return;

            AddError(AsvUtilities.BuildErrorMessage(k_ErrorMessageHeader, restrictedFiles, ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "zip-file-found")));
        }
    }
}
