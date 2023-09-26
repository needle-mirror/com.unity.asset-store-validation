using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class NoJpgsValidation : BaseFileExtensionValidation
    {
        internal static readonly string k_DocsFilePath = "no_jpgs_validation.html";
        internal static readonly string k_ErrorMessageHeader = "Asset Store Packages do not allow for jpg files (or jpg variations). The following files cannot be included in your package:";
        internal static readonly string k_ErrorMessageFooter = $"All images we share with users must use a lossless compression format (e.g. .png). This is because the final build suffers when using assets made with compressed file formats like \".jpg\" and \".jpeg\". Please remove these files or convert them to a lossless compression format like .png then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-contains-jpgs")}";

        readonly string[] k_RestrictedFileExtensions =
        {
            ".jpg",
            ".jpeg",
            ".jpe",
            ".jif",
            ".jfif",
            ".jfi"
        };

        readonly HashSet<string> imageQualityExceptionPaths = new HashSet<string>
        {
            "documentation~",
            "tests",
        };

        public NoJpgsValidation()
        {
            TestName = "No Jpgs";
            TestDescription = "A package must not contain any jpgs outside of the Documentation~ and Tests folders.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] {ValidationType.AssetStore, ValidationType.InternalTesting};
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            CheckForJpgs(Context.ProjectPackageInfo.path);
        }

        void CheckForJpgs(string packagePath)
        {
            var restrictedFiles =  GetPathsToFilesWithExtension(packagePath, k_RestrictedFileExtensions).Where(file => !AsvUtilities.IsFileInExceptionPath(file,imageQualityExceptionPaths)).ToList();
            if (!restrictedFiles.Any()) return;

            AddError(AsvUtilities.BuildErrorMessage(k_ErrorMessageHeader,restrictedFiles, k_ErrorMessageFooter));
        }

    }
}
