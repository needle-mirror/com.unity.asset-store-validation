using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Editor.ValidationSuite.ValidationSuite.ValidationTests;
using UnityEditor.PackageManager.AssetStoreValidation.Models;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class DocumentationValidation : BaseHttpValidation
    {
        static readonly string k_DocsFilePath = "documentation_validation.html";
        const string k_DocumentationFolderName = "Documentation~";

        static readonly string k_NoDocsHeader = "Documentation for this package has not been defined. Please provide a documentation URL in the \"documentationUrl\" field in package.json, or add documentation files to a \"Documentation~\" folder in your package structure.\nValid documentation extensions are:";
        static readonly string k_NoDocsFooter = $"{ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "no-documentation-found")}";

        internal static string k_CapitalizationError(string folderName) =>
            $"The documentation folder needs to be properly capitalized. Please rename the folder \"{folderName}\" in your package structure to \"Documentation~\". {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "documentation-folder-capitalization")}";

        static readonly string k_EmptyDocumentationHeader =
            $"At least one file of with contents must exist inside the \"Documentation~\" folder. Valid documentation extensions are:";

        static readonly string k_EmptyDocumentationFooter =
            $"Please validate that your documentation files have contents inside. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "empty-documentation-files")}";

        internal static string NoDocumentationMessage => AsvUtilities.BuildErrorMessage(k_NoDocsHeader, k_AllowedExtensions, k_NoDocsFooter);
        internal static string EmptyDocumentationMessage =>
            AsvUtilities.BuildErrorMessage(k_EmptyDocumentationHeader, k_AllowedExtensions, k_EmptyDocumentationFooter);

        internal static string UnreachableUrlMessage(string url) =>
            $"The url \"{url}\", provided in the \"documentationUrl\" field in package.json, is not reachable. To avoid broken links, please validate that the URL is correct. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "documentationUrl-not-reachable")}";
        
        internal static string UntestedUrlMessage(string url) =>
            $"The url \"{url}\", provided in the \"documentationUrl\" field in package.json, has not been tested. Please validate manually that the URL is accurate and reachable. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "documentationUrl-not-tested")}";

        static readonly string[] k_AllowedExtensions = new string[]
        {
            ".rtf",
            ".pdf",
            ".md",
            ".html",
            ".txt"
        };

        Regex m_DocumentationFolderRegex = new Regex(k_DocumentationFolderName, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public DocumentationValidation()
        {
            TestName = "Documentation";
            TestDescription = "A package must contain basic usage documentation for consumers.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.Structure, ValidationType.AssetStore, ValidationType.InternalTesting };
        }

        protected override void Run()
        {
            base.Run();
            // Start by declaring victory
            TestState = TestState.Succeeded;

            var manifestData = Context.ProjectPackageInfo;
            var docsUrl = manifestData.documentationUrl;
            
            var docsUrlStatus = UrlStatus.None;
            
            // For internal testing there should not be any network calls
            if (Context.ValidationType == ValidationType.InternalTesting)
            {
                if (!string.IsNullOrWhiteSpace(docsUrl)) // Only write a warning if there's a url to check
                    AddWarning(UntestedUrlMessage(docsUrl));
            }
            else
            {
                docsUrlStatus = CheckUrlStatus(docsUrl);
                if (docsUrlStatus == UrlStatus.Unreachable)
                    AddWarning(UnreachableUrlMessage(docsUrl));
            }

            var offlineDocsResult = ValidateOfflineDocs();
            
            // Shortcut out when things are ok
            if (docsUrlStatus != UrlStatus.None || offlineDocsResult == OfflineDocsStatus.Ok) return;

            if (offlineDocsResult == OfflineDocsStatus.NoDocs)
            {
                // No docs at all
                AddError(NoDocumentationMessage);
            }
            else if (offlineDocsResult == OfflineDocsStatus.EmptyFiles)
            {
                // Some docs files but all empty
                AddError(EmptyDocumentationMessage);
            }
        }

        void CheckDocumentationFolderName()
        {
            var dirs = Directory.EnumerateDirectories(Context.PublishPackageInfo.path, "*", SearchOption.TopDirectoryOnly).ToArray();

            if (dirs.Length == 0) return;

            foreach (var dirPath in dirs)
            {
                var dirName = Path.GetFileName(dirPath);
                if (!m_DocumentationFolderRegex.IsMatch(dirPath)) continue;

                if (dirName != k_DocumentationFolderName)
                    AddError(k_CapitalizationError(dirName));
            }
        }

        OfflineDocsStatus ValidateOfflineDocs()
        {
            CheckDocumentationFolderName();

            // Get all valid documentation files
            var files = GetDocumentationFiles();
            if (files == null) return OfflineDocsStatus.NoDocs;

            // Check if they are empty
            var allFilesEmpty =  files.All(f => IsFileEmpty(f));

            if (allFilesEmpty) return OfflineDocsStatus.EmptyFiles;

            return OfflineDocsStatus.Ok;
        }

        IEnumerable<string> GetDocumentationFiles()
        {
            var docsPath = Path.Combine(Context.PublishPackageInfo.path, k_DocumentationFolderName);
            if (!Directory.Exists(docsPath)) return null;

            var allFiles = Directory.EnumerateFiles(docsPath, "*", SearchOption.AllDirectories).ToArray();
            var validFiles = allFiles.Where(f => k_AllowedExtensions.Contains(Path.GetExtension(f))).ToArray();
            return validFiles.Length == 0 ? null : validFiles;
        }

        bool IsFileEmpty(string path)
        {
            var info = new FileInfo(path);
            return info.Length == 0;
        }
    }
}
