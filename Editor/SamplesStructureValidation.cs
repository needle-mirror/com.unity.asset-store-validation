using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using System.IO;
using System.Linq;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class SamplesStructureValidation : BaseValidation
    {
        #region Constants

        internal static readonly string k_DocsFilePath = "samples_structure_validation";

        const string k_SamplesTildeDirName = "Samples~";

        internal static readonly string k_MissingSamplesFolder =
            $"Missing samples folder. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-samples-folder")}";

        internal static readonly string k_SamplesArrayNotFound =
            "Samples field in {0} is empty but a " + k_SamplesTildeDirName + " was found. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "samples-array-in-package-manifest-is-empty");

        internal static readonly string k_SamplesNotFound =
            $"No samples found. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "no-samples-found")}";

        internal static readonly string k_HiddenFoldersFound =
            "Hidden folders found {0}. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "hidden-files-or-folders-found");

        internal static readonly string k_HiddenFilesFound =
            "Hidden files found {0}." + $" {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "hidden-files-or-folders-found")}";

        internal static readonly string k_NoUsefulFilesFound =
            $"Folder does not contain any files considered useful. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "empty-samples-folder")}";

        internal static readonly string k_EmptySamplesFolder =
            "Empty samples folder: {0}. " + ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "empty-samples-folder");

        internal static readonly string k_MissingSamples =
            $"Missing Samples~ entry in Package manifest. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-samples-entry-in-package-manifest")}";

        internal static readonly string k_MissingFolder =
            "Folder {0} is missing from the samples. " + ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "folder-is-missing-from-the-samples");

        internal static string EmptyDescription(string displayName) =>
            $"Empty description property for sample with displayName \"{displayName}\". {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "empty-property-for-sample")}";

        internal static string EmptyPath(string displayName) =>
            $"Empty path property for sample with displayName \"{displayName}\". {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "empty-property-for-sample")}";

        internal static string EmptyDisplayName(string path) =>
            $"Empty display property for sample with path \"{path}\". {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "empty-property-for-sample")}";

        internal static readonly string k_DuplicatedDescription =
            "Duplicated description : {0}. " + ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "duplicated-description");

        internal static readonly string k_DuplicatedPath =
            "Duplicated path : {0}. " + ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "duplicated-path");

        internal static readonly string k_DuplicatedDisplayName =
            "Duplicated display name : {0}. " + ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "duplicated-display-name");

        internal static readonly string k_WrongFolder =
            $"Samples should be in {k_SamplesTildeDirName} folder. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "samples-should-be-in-samples~-folder")}";

        internal static readonly string k_DirectoryNotExist =
            "Directory {0} does not exist. " + ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "directory-does-not-exist");

        #endregion

        public SamplesStructureValidation()
        {
            TestName = "Samples";
            TestDescription = "Verify that samples meet expectations, if the package has samples.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.Structure, ValidationType.AssetStore, ValidationType.InternalTesting };
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            var samplesTildeFolderLocation = Path.Combine(Context.PublishPackageInfo.path, k_SamplesTildeDirName);
            var samplesTildeDirExists = Directory.Exists(samplesTildeFolderLocation);

            // there is Samples~ and an entry in the Package.json
            if (samplesTildeDirExists && Context.PublishPackageInfo.samples.Any())
            {
                ValidateSampleFolderContent(samplesTildeFolderLocation);
                return;
            }
            if (Context.PublishPackageInfo.samples.Any())
            {
                // No Samples~ folder, but there is an entry in the manifest
                AddError(k_MissingSamplesFolder);
                return;
            }
            if (samplesTildeDirExists)
            {
                // Samples~ exist but no entry in the package manifest
                AddError(string.Format(k_SamplesArrayNotFound, Context.PublishPackageInfo.path));
                return;
            }

            // There are no samples and no entry in the Package json. Nothing to do here
            AddInformation(k_SamplesNotFound);
            TestState = TestState.NotRun;
        }

        void ValidateSampleFolderContent(string samplesTildeFolderLocation)
        {
            // move to predicate because the check is in several places
            var foldersInSamplesFolder = Directory
                .GetDirectories(samplesTildeFolderLocation, "*", SearchOption.TopDirectoryOnly)
                .Select(x => new DirectoryInfo(x))
                .Where(x => !CheckHiddenFolder(x)).ToList();

            var allFilesInDirectory = ValidateHiddenFoldersOrFiles(samplesTildeFolderLocation);
            ValidateSamplesManifestEntries();
            var usableFilesOutsideFolders = ValidateContentSamplesFolder(foldersInSamplesFolder, allFilesInDirectory);
            CheckFoldersCorrespondToManifest(foldersInSamplesFolder, usableFilesOutsideFolders);
        }

        bool CheckHiddenFolder(DirectoryInfo directoryInfo)
        {
            return directoryInfo.Name.StartsWith(".") || directoryInfo.Name.EndsWith("~") ||
                   string.Equals(directoryInfo.Name, "csv", StringComparison.InvariantCultureIgnoreCase);
        }

        bool CheckHiddenFile(FileInfo fileInfo)
        {
            return fileInfo.Name.StartsWith(".") || fileInfo.Name.EndsWith("~") ||
                   string.Equals(Path.GetFileNameWithoutExtension(fileInfo.Name), "csv",
                       StringComparison.InvariantCultureIgnoreCase) ||
                   fileInfo.Extension.Equals(".tmp", StringComparison.InvariantCultureIgnoreCase);
        }

        List<FileInfo> ValidateHiddenFoldersOrFiles(string samplesTildeFolderLocation)
        {
            var filesInDirectory = Directory
                .EnumerateFiles(samplesTildeFolderLocation, "*", SearchOption.AllDirectories)
                .Select(x => new FileInfo(x)).ToList();

            var hiddenFolders = Directory
                .EnumerateDirectories(samplesTildeFolderLocation, "*", SearchOption.AllDirectories)
                .Select(x => new DirectoryInfo(x))
                .Where(CheckHiddenFolder).ToList();

            if (hiddenFolders.Any())
            {
                AddWarning(string.Format(k_HiddenFoldersFound,
                    string.Join(",", hiddenFolders.Select(x => x.FullName))));
            }

            if (filesInDirectory.Any(CheckHiddenFile))
            {
                AddWarning(string.Format(k_HiddenFilesFound,
                    string.Join(",", filesInDirectory.Where(x => CheckHiddenFile(x)).Select(x => x.FullName))));
            }

            return filesInDirectory.Where(x => !CheckHiddenFile(x)).ToList();
        }

        /// <summary>
        /// A samples folder should contain at least 1 file
        /// </summary>
        /// <param name="foldersInSamplesFolder"></param>
        /// <param name="allFilesInSamples"></param>
        List<FileInfo> ValidateContentSamplesFolder(List<DirectoryInfo> foldersInSamplesFolder,
            List<FileInfo> allFilesInSamples)
        {
            var usableFiles = allFilesInSamples.Where(x =>
                !x.Extension.Equals(".meta", StringComparison.InvariantCultureIgnoreCase)
                && !x.Name.EndsWith(".sample.json", StringComparison.InvariantCultureIgnoreCase)
                && !x.Extension.Equals(".asmdef", StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (!usableFiles.Any())
            {
                AddError(k_NoUsefulFilesFound);
                return new List<FileInfo>();
            }

            foreach (var folderInSamplesFolder in foldersInSamplesFolder)
            {
                var filesPartOfThatFolder = usableFiles.Where(x =>
                        x.FullName.StartsWith(folderInSamplesFolder.FullName,
                            StringComparison.InvariantCultureIgnoreCase))
                    .ToList();

                // if any of the folders do not contain a valid file then raise an error
                if (!filesPartOfThatFolder.Any())
                {
                    AddError(string.Format(k_EmptySamplesFolder,
                        Path.GetDirectoryName(folderInSamplesFolder.FullName)));
                    continue;
                }

                // remove already processed files to reduce search
                usableFiles.RemoveAll(x => filesPartOfThatFolder.Any(z => z.FullName == x.FullName));
            }

            return usableFiles;
        }

        void CheckFoldersCorrespondToManifest(List<DirectoryInfo> folderContent,
            List<FileInfo> usableFiles)
        {
            var manifestHasSamplesTildeFolder = Context.PublishPackageInfo.samples.Any(x =>
                new DirectoryInfo(x.path).Name.Equals(k_SamplesTildeDirName, StringComparison.InvariantCulture));

            // if there are any files left (outside of dedicated folders) then the sample data should contain a samples folder
            if (usableFiles.Any() && !manifestHasSamplesTildeFolder)
            {
                AddError(k_MissingSamples);
            }

            foreach (var entry in folderContent)
            {
                if (Context.PublishPackageInfo.samples.Any(x => x.path.EndsWith($"{k_SamplesTildeDirName}/{entry.Name}")|| x.path.EndsWith($"{ k_SamplesTildeDirName}\\{entry.Name}")))
                    continue;
                if (!manifestHasSamplesTildeFolder)
                {
                    AddError(string.Format(k_MissingFolder, entry));
                }
            }
        }

        void ValidateSamplesManifestEntries()
        {
            var description = new HashSet<string>();
            var path = new HashSet<string>();
            var displayName = new HashSet<string>();

            foreach (var sampleData in Context.PublishPackageInfo.samples)
            {
                // if no path is set, error + skip the rest
                if (string.IsNullOrWhiteSpace(sampleData.path))
                {
                    AddError(EmptyPath(sampleData.displayName));
                    continue;
                }
                
                // If a package has samples, the entry in the package manifest should contain at least this structure: {path, displayName, description} 
                ValidateSamplesProperty(sampleData.description, EmptyDescription(sampleData.displayName));
                ValidateSamplesProperty(sampleData.displayName, EmptyDisplayName(sampleData.path));
                
                // find duplicate description
                if (!string.IsNullOrWhiteSpace(sampleData.description))
                {
                    ValidateHashSet(description, sampleData.description,
                        string.Format(k_DuplicatedDescription, sampleData.description));
                }
                // find duplicate path
                if (!string.IsNullOrWhiteSpace(sampleData.path))
                {
                    ValidateHashSet(path, sampleData.path, string.Format(k_DuplicatedPath, sampleData.path));
                }
                // find duplicate display name
                if (!string.IsNullOrWhiteSpace(sampleData.displayName))
                {
                    ValidateHashSet(displayName, sampleData.displayName,
                        string.Format(k_DuplicatedDisplayName, sampleData.displayName));
                }
                
                // If a package has samples, then they should be in a folder called Samples~
                var rootPath = sampleData.path.Split(new[] { Path.DirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                if (!rootPath.StartsWith(k_SamplesTildeDirName, StringComparison.InvariantCultureIgnoreCase))
                {
                    AddWarning(k_WrongFolder);
                }

                // If a package has a sample entry in package.json, the configuration should point to valid and non-empty paths
                if (!Directory.Exists(Path.Combine(Context.PublishPackageInfo.path, sampleData.path)))
                {
                    AddError(string.Format(k_DirectoryNotExist, sampleData.path));
                }
            }
        }

        void ValidateSamplesProperty(string value, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(errorMessage);
            }
        }

        void ValidateHashSet(ISet<string> entries, string entry, string message)
        {
            if (!entries.Add(entry.ToLower()))
            {
                AddError(message);
            }
        }
    }
}