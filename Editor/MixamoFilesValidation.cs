using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
using UnityEngine;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class MixamoFilesValidation : BaseValidation
    {
        const string k_mixamoName = "Mixamo, Inc.";
        const string k_mixamoRig = "mixamorig";
        const int k_bytesInHeader = 2400;
        const string k_ErrorReadingFile = "File {0} could not be read. {1}";
        const string k_IvalidContent = "The file content is invalid.";
        const string k_UnsupportedRead = "The stream does not support reading.";
        const string k_PathTooLong = "The path of the file is too long.";
        static readonly string k_DocsFilePath = "mixamo_files_validation.html";

        internal static readonly string k_MixamoErrorMessage = "Package contains mixamo files {0}. " +
                                                               ErrorDocumentation.GetLinkMessage(k_DocsFilePath,
                                                                   "package-contains-mixamo-files");

        internal static bool s_ApplicationIsUnity = !string.IsNullOrWhiteSpace(Application.unityVersion);

        public MixamoFilesValidation()
        {
            TestName = "Mixamo Files";
            TestDescription = "Validate if the package contains mixamo files.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] {ValidationType.Structure, ValidationType.AssetStore};
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            if (s_ApplicationIsUnity)
            {
                ValidateMixamoFilesUnity();
                return;
            }

            ValidateMixamoFilesFileSystem();
        }

        void ValidateMixamoFilesFileSystem()
        {
            var filesInFileSystem =
                Directory.EnumerateFiles(Context.PublishPackageInfo.path, "*",
                    SearchOption.AllDirectories);
            var mixamoFiles = new ConcurrentBag<string>();

            Parallel.ForEach(filesInFileSystem, new ParallelOptions {MaxDegreeOfParallelism = 4}, file =>
            {
                try
                {
                    var buffer = new byte[k_bytesInHeader];
                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Read(buffer, 0, buffer.Length);
                        fileStream.Close();

                        var result = Encoding.Default.GetString(buffer);
                        if (result.Contains(k_mixamoName))
                        {
                            mixamoFiles.Add(file);
                        }
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    AddError(string.Format(k_ErrorReadingFile, file, k_IvalidContent));
                }
                catch (NotSupportedException)
                {
                    AddError(string.Format(k_ErrorReadingFile, file, k_UnsupportedRead));
                }
                catch (PathTooLongException)
                {
                    AddError(string.Format(k_ErrorReadingFile, file, k_PathTooLong));
                }
            });

            CheckIfListContainsMixamo(mixamoFiles.ToList());
        }

        void CheckIfListContainsMixamo(List<string> mixamoFiles)
        {
            if (!mixamoFiles.Any())
            {
                return;
            }

            AddError(string.Format(k_MixamoErrorMessage, string.Join(",", mixamoFiles)));
        }

        void ValidateMixamoFilesUnity()
        {
            var guids = AssetDatabase.FindAssets("", new[] {$@"Packages/{Context.PublishPackageInfo.name}"});
            var mixamoFiles = new List<string>();

            foreach (var guid in guids)
            {
                var objectPath = AssetDatabase.GUIDToAssetPath(guid);
                var assetsAtPath =  AssetDatabase.LoadAllAssetRepresentationsAtPath(objectPath);

                foreach (var assetObject in assetsAtPath)
                {
                    if (!(assetObject is GameObject gameObject) || !gameObject.name.StartsWith(k_mixamoRig)) continue;
                    mixamoFiles.Add(objectPath);
                    break;
                }
            }

            CheckIfListContainsMixamo(mixamoFiles);
        }
    }
}