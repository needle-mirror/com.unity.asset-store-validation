using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class NoExecutablesValidation : BaseFileExtensionValidation
    {
        internal static readonly string k_DocsFilePath = "no_executables_validation.html";
        internal static readonly string k_ErrorMessageHeader = "Package is not allowed to contain the following executables";
        internal static readonly string k_ErrorMessageHeaderRSPs = "Package must not contain the following rsp file(s)";
        internal static readonly string k_ErrorMessageFooter = $"Please remove any executables from the package, then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-contains-executables")}";

        // this changes the compilation of the end user's project  
        internal static readonly string[] k_fileNames = new string[]
        {
            "msp.rsp",
            "csc.rsp"
        };

        readonly string[] k_fileExtensions =
        {
            ".bat",
            ".bin",
            ".com",
            ".csh",
            ".dom",
            ".dmg",
            ".exe",
            ".js",
            ".jse",
            ".lib",
            ".msi",
            ".msp",
            ".mst",
            ".pkg",
            ".ps1",
            ".sh",
            ".vb",
            ".vbe",
            ".vbs",
            ".vbscript",
            ".vs",
            ".vsd",
            ".vsh"
        };

        public NoExecutablesValidation()
        {
            TestName = "No Executables";
            TestDescription = "Validates that the package does not include restricted executable files.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] {ValidationType.AssetStore};
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            var filesInTheSystem = new HashSet<string>(Directory
                .EnumerateFiles(Context.PublishPackageInfo.path, "*", SearchOption.AllDirectories)).ToList();

            CheckForExecutables(Context.PublishPackageInfo.path);
            ValidateRspFiles(filesInTheSystem);
        }

        void ValidateRspFiles(List<string> filesInTheSystem)
        {
            var rspFiles = filesInTheSystem.Select(x => new {FilePath = x, FileName = Path.GetFileName(x).Trim()})
                .Where(x => k_fileNames.Contains(x.FileName, StringComparer.InvariantCultureIgnoreCase)).ToList();

            if (!rspFiles.Any()) return;

            AddError(AsvUtilities.BuildErrorMessage(k_ErrorMessageHeaderRSPs, rspFiles.Select(x => x.FilePath), k_ErrorMessageFooter));
        }

        void CheckForExecutables(string packagePath)
        {
            var filesRestrictedExtensions = GetPathsToFilesWithExtension(packagePath, k_fileExtensions);

            if (!filesRestrictedExtensions.Any()) return;

            AddError( AsvUtilities.BuildErrorMessage(k_ErrorMessageHeader, filesRestrictedExtensions, k_ErrorMessageFooter));
        }
    }
}
