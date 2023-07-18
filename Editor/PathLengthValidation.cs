using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class PathLengthValidation : BaseValidation
    {
        internal static readonly string k_DocsFilePath = "path_length_validation.html";
        internal static readonly string k_LongPathPrefix = @"\\?\";
        const int k_MaxPathLength = 140;

        internal static readonly string k_LongPathError = "The following paths: {0} are longer than {1}. " +
                                                          ErrorDocumentation.GetLinkMessage(k_DocsFilePath,
                                                              "path-too-long");

        public PathLengthValidation()
        {
            TestName = "Path Length";
            TestDescription = $"Ensure that any file and folder paths should not exceed {k_MaxPathLength} characters.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] {ValidationType.AssetStore};
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            CheckPathLength(Context.PublishPackageInfo.path);
        }

        void CheckPathLength(string path)
        {
#if UNITY_EDITOR_WIN
            path = k_LongPathPrefix + path; // add escape characters for windows
#endif
            var allDirectoryEntries =
                Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories);

            var longPaths = allDirectoryEntries.Where(x => Utilities.GetPathFromRoot(x, path).Length > k_MaxPathLength).ToArray();

            if (!longPaths.Any())
            {
                return;
            }

#if UNITY_EDITOR_WIN
            RemoveLongPathPrefix(longPaths);
#endif

            var errors = new StringBuilder();
            foreach (var longPath in longPaths)
            {
                errors.AppendLine(longPath);
            }
                
            AddError(string.Format(k_LongPathError, errors, k_MaxPathLength));
        }

        string RemoveLongPathPrefix(string path)
        {
            return path.StartsWith(k_LongPathPrefix) ? path.Substring(k_LongPathPrefix.Length) : path;
        }

        void RemoveLongPathPrefix(string[] paths)
        {
            for (var i = 0; i < paths.Length; i++)
            {
                paths[i] = RemoveLongPathPrefix(paths[i]);
            }
        }
    }
}