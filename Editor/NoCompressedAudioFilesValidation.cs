using System.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class NoCompressedAudioFilesValidation : BaseFileExtensionValidation
    {
        internal static readonly string k_DocsFilePath = "no_compressed_audio_files_validation.html";
        internal static readonly string k_ErrorMessageHeader = "Unity Asset Store packages do not allow for lossy compressed audio formats. The following files must be removed or converted to a lossless format such as .wav";

        readonly string[] k_RestrictedFileExtensions =
        {
            ".mp3",
            ".ogg"
        };        
        
        public NoCompressedAudioFilesValidation()
        {
            TestName = "No Compressed Audio Files Validation";
            TestDescription = "A package must not contain any lossy compressed audio files.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] {ValidationType.AssetStore};
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;     
            CheckForCompressedAudioFiles(Context.ProjectPackageInfo.path);
        }

        void CheckForCompressedAudioFiles(string packagePath)
        {
            var restrictedFiles = GetPathsToFilesWithExtension(packagePath, k_RestrictedFileExtensions);
            if (!restrictedFiles.Any()) return;
            
            AddError(AsvUtilities.BuildErrorMessage(k_ErrorMessageHeader ,restrictedFiles,ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "compressed-audio-file-found")));
        }
    }
}