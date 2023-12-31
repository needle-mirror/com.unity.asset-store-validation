﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
using UnityEngine;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class MetaFilesValidation : BaseValidation
    {
        readonly Regex m_RxStartingWithDot = new Regex($"\\{Path.DirectorySeparatorChar}[.]");
        readonly Regex m_RxEndingWithTilde = new Regex($"[~]\\{Path.DirectorySeparatorChar}");
        
        internal static readonly string k_DocsFilePath = "meta_files_validation.html";

        public MetaFilesValidation()
        {
            TestName = "Meta Files";
            TestDescription = "Validate that metafiles are present for all package files and that no useless metafiles exist.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] {ValidationType.Structure, ValidationType.AssetStore, ValidationType.InternalTesting};

        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            CheckMetasInFolderTree(Context.PublishPackageInfo.path);
        }

        void CheckMetasInFolderTree(string folder)
        {
            try
            {
                var folderFiles = Directory.GetFileSystemEntries(folder , "*", SearchOption.AllDirectories);
                var filesAndFoldersRequiringMetaFiles = new HashSet<string>();
                var metaFiles = new HashSet<string>();
                
                // Build the lists of normal files/folders and metafiles in package
                foreach (var filepath in folderFiles)
                {
                    var upperDirectoryPath = Path.GetDirectoryName(filepath) + Path.DirectorySeparatorChar;
                    
                    if (filepath.EndsWith(".meta"))
                    {
                        // Meta files found in Documentation~ folder should definitely not be there since Unity does not generate them
                        if (upperDirectoryPath.Contains(Path.DirectorySeparatorChar + "Documentation~" + Path.DirectorySeparatorChar))
                        {
                            AddError($"Documentation~ folder should not have any metafiles inside: {filepath}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "Documentation~_folder_should_not_have_any_metafiles_inside")}");
                            continue;
                        }

                        metaFiles.Add(Path.DirectorySeparatorChar + filepath);
                        continue;
                    }

                    // Files/Directories that start with '.' do not require metafiles
                    if (m_RxStartingWithDot.Matches(upperDirectoryPath).Count > 0)
                        continue;
                    // Directories that end with '~' do not require metafiles for it
                    // Files under a directory that end with '~' do not require metafiles (Samples~ folder content excluded)
                    if (m_RxEndingWithTilde.Matches(upperDirectoryPath).Count > 0 && !upperDirectoryPath.Contains( Path.DirectorySeparatorChar + "Samples~" +  Path.DirectorySeparatorChar))
                        continue;
                    // Hidden files do not require meta files
                    if (IsHidden(Path.GetFileName(filepath)))
                        continue;
                    
                    filesAndFoldersRequiringMetaFiles.Add(Path.DirectorySeparatorChar + filepath);
                }
                
                foreach (var filepath in filesAndFoldersRequiringMetaFiles)
                {
                    if (metaFiles.Contains(filepath + ".meta"))
                        metaFiles.Remove(filepath + ".meta");
                    else
                        AddError($"Did not find meta file for {filepath}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "did-not-find-corresponding-meta-file")}");
                }
                
                foreach (var filepath in metaFiles)
                {
                    // Metafiles found in hidden folders (Excluding Samples~ and Documentations~ exceptions) are not generated by Unity and can be deleted, but users might have another use of them, so we warn for them instead of error
                    var upperDirectoryPath = Path.GetDirectoryName(filepath) + Path.DirectorySeparatorChar;
                    if (m_RxStartingWithDot.Matches(upperDirectoryPath).Count > 0 || m_RxEndingWithTilde.Matches(upperDirectoryPath).Count > 0){
                        AddWarning($"Metafile found at location {filepath} has no corresponding file and is therefore safe to delete. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "useless-meta-file-found-in-hidden-folder-in-package")}");
                        continue;
                    }
                    
                    if (!filesAndFoldersRequiringMetaFiles.Contains(TrimMetafile(filepath)))
                        AddError($"Metafile found at location {filepath} has no corresponding file and is therefore safe to delete. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "useless-meta-file-found-in-package")}");
                }
            }
            catch (Exception e)
            {
                AddError($"Exception {e.Message}");
            }
        }

        bool IsHidden(string name)
        {
            return name.StartsWith(".") || name.EndsWith("~");
        }

        string TrimMetafile(string name)
        {
            return name.Substring(0, name.Length - 5);
        }
    }   
}
