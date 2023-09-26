using System.IO;
using System.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class NoSymlinksValidation : BaseValidation
    {
        internal static readonly string k_DocsFilePath = "no_symlinks_validation.html";
        internal static readonly string k_ErrorMessageHeader = "The following symlink(s) must be removed:";
        internal static readonly string k_ErrorMessageFooter = $"Unity does not allow symlinks within packages that are published to the Asset Store. Please remove symlinks from your package, then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "symlink-found-within-project")}";

        public NoSymlinksValidation()
        {
            TestName = "No Symlinks";
            TestDescription = "A package must not contain any symbolic links or shortcuts to other files.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] {ValidationType.AssetStore, ValidationType.InternalTesting};
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            ValidateNoSymlinks(Context.ProjectPackageInfo.path);
        }

        void ValidateNoSymlinks(string packagePath)
        {
            var symLinks = Directory.EnumerateFileSystemEntries(packagePath, "*.*", SearchOption.AllDirectories).Where(IsSymlink).ToList();
            
            if (symLinks.Any())
            {
                AddError(AsvUtilities.BuildErrorMessage(k_ErrorMessageHeader,symLinks,k_ErrorMessageFooter));
            }
        }

        bool IsSymlink(string path)
        {
            return new FileInfo(path).Attributes.HasFlag(FileAttributes.ReparsePoint);
        }
    }
}
