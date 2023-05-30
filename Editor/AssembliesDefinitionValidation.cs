using System.Collections.Generic;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
using System.IO;
using System;
using System.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.Models;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class AssembliesDefinitionValidation : BaseValidation
    {
        internal const string k_DocsFilePath = "assemblies_definition_validation.html";
        internal const string k_SamplesTildeFolderName = "Samples~";
        internal const string k_AssemblyDefExt = ".asmdef";
        internal const string k_AssemblyRefExt = ".asmref";
        internal const string k_ScriptExt = ".cs";
        internal const string k_HiddenFolderCvs = "cvs";

        Dictionary<int, List<FileOrFolderInfo>> m_FilesDictionary = new Dictionary<int, List<FileOrFolderInfo>>();
        Dictionary<int, List<FileOrFolderInfo>> m_FoldersDictionary = new Dictionary<int, List<FileOrFolderInfo>>();

        internal static string MoreThanOneAssemblyDefinitionErrorMessage(FileOrFolderInfo folder, List<FileOrFolderInfo> filesInFolder) => $"More than one assembly definition 'asmdef|asmref' file \"{AssembliesDefinitionFileNames(filesInFolder)} \" has been found in \"{ folder.Path }\" folder." +
                                 $"Only one assembly definition is allowed per folder: { ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "more-than-one-assembly-definition")}";
        internal static string ScriptWithoutAssemblyDefinitionAssociatedErrorMessage(FileOrFolderInfo folder, List<FileOrFolderInfo> filesInFolder) => $"The following C# script(s) \"{ScriptsDefinitionFileNames(filesInFolder)}\" were found in \"{ folder.Path }\" folder, but no corresponding asmdef or asmref file was found in " +
                                       $"the folder or in ancestors folders: { ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "script-found-without-asmdef-or-asmref-associated")}";

        internal static string AssemblyDefinitionWithoutScriptAssociatedErrorMessage(FileOrFolderInfo folder, List<FileOrFolderInfo> filesInFolder) => $"Assembly definition file \"{AssembliesDefinitionFileNames(filesInFolder)} \" found in \"{ folder.Path }\" folder, but no C# script associated to it in this folder or in " +
                              $"descendants folders: { ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "assembly-definition-found-without-script-associated")}";

        public AssembliesDefinitionValidation()
        {
            TestName = "Assemblies Definition Validation";
            TestDescription = "Validates that the package assemblies definition meets certain criteria.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.Structure, ValidationType.AssetStore };

        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            var assemblyAndCsFiles = Directory.GetFiles(Context.PublishPackageInfo.path, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(k_AssemblyDefExt, StringComparison.OrdinalIgnoreCase)
                                                                                                              || s.EndsWith(k_AssemblyRefExt, StringComparison.OrdinalIgnoreCase)
                                                                                                              || s.EndsWith(k_ScriptExt, StringComparison.OrdinalIgnoreCase))
                                                                                                            .Where(s => !Path.GetFileName(s).StartsWith(".")).ToList();

            var foldersInPackage = Directory.GetDirectories(Context.PublishPackageInfo.path, "*", SearchOption.AllDirectories).Where(s => !new DirectoryInfo(s).Name.StartsWith(".") &&
                                                                                                                               !new DirectoryInfo(s).Name.Equals(k_HiddenFolderCvs, StringComparison.OrdinalIgnoreCase) &&
                                                                                                                              (!s.EndsWith("~") || new DirectoryInfo(s).Name.Equals(k_SamplesTildeFolderName, StringComparison.InvariantCulture))).ToList();
            AddPathsToDictionary(assemblyAndCsFiles, m_FilesDictionary);
            AddPathsToDictionary(foldersInPackage, m_FoldersDictionary);

            var pendingAsmdefOrRef = 0;
            _ = CheckAssembliesDefinition(new FileOrFolderInfo() { Name = new DirectoryInfo(Context.PublishPackageInfo.path).Name, Path = Context.PublishPackageInfo.path, ParentFolderName = "", PathDepth = Context.PublishPackageInfo.path.Split(Path.DirectorySeparatorChar).Length - 2 },
                                          false,
                                          ref pendingAsmdefOrRef,
                                          false);
        }

        void AddPathsToDictionary(List<string> filesPath, Dictionary<int, List<FileOrFolderInfo>> pathsDictionary)
        {
            foreach (var filePath in filesPath)
            {
                var filePathArray = filePath.Split(Path.DirectorySeparatorChar);
                var key = filePathArray.Length - 2;
                var fileRegister = new FileOrFolderInfo()
                {
                    Name = filePathArray[key + 1],
                    Path = filePath,
                    ParentFolderName = filePathArray[key],
                    PathDepth = key
                };
                if (pathsDictionary.ContainsKey(key))
                    pathsDictionary[key].Add(fileRegister);
                else
                    pathsDictionary.Add(key, new List<FileOrFolderInfo>() { fileRegister });
            }
        }

        bool CheckAssembliesDefinition(FileOrFolderInfo folderRegister, bool ancestorFolderHasAsmdefOrRef, ref int pendingAsmdefOrRef, bool insideSampleTilde)
        {
            var filesInFolder = new List<FileOrFolderInfo>();

            if (m_FilesDictionary.ContainsKey(folderRegister.PathDepth + 1))
                filesInFolder = m_FilesDictionary[folderRegister.PathDepth + 1].Where(s => s.ParentFolderName.Equals(folderRegister.Name, StringComparison.OrdinalIgnoreCase)).ToList();

            insideSampleTilde = insideSampleTilde || folderRegister.Name.Equals(k_SamplesTildeFolderName, StringComparison.InvariantCulture);

            var asmdefOrAsmrefFilesCount = AsmdefAndAsmrefFilesCount(filesInFolder);
            var previousAsmdefOrRefDefined = ancestorFolderHasAsmdefOrRef || asmdefOrAsmrefFilesCount > 0;

            var folderHasCsFilesDefined = filesInFolder.Any(s => s.Name.EndsWith(k_ScriptExt, StringComparison.OrdinalIgnoreCase));

            var folderHasCsWithNoAsmdef = folderHasCsFilesDefined && asmdefOrAsmrefFilesCount == 0 && !insideSampleTilde;

            //folder contains at least one script, .cs file
            if (folderHasCsFilesDefined)
            {
                if (previousAsmdefOrRefDefined)
                {
                    if (pendingAsmdefOrRef > 0 && folderHasCsWithNoAsmdef)
                        pendingAsmdefOrRef--;
                }
                else
                {
                    if (!insideSampleTilde)
                        AddError(ScriptWithoutAssemblyDefinitionAssociatedErrorMessage(folderRegister, filesInFolder));
                }
            }
            else
            {
                if (asmdefOrAsmrefFilesCount > 0)
                    pendingAsmdefOrRef++;
            }

            //more than one assembly definition in the same folder
            if (asmdefOrAsmrefFilesCount > 1)
                AddError(MoreThanOneAssemblyDefinitionErrorMessage(folderRegister, filesInFolder));

            if (m_FoldersDictionary.ContainsKey(folderRegister.PathDepth + 1))
            {
                foreach (var folderReg in m_FoldersDictionary[folderRegister.PathDepth + 1].Where(s => s.ParentFolderName == folderRegister.Name))
                    folderHasCsWithNoAsmdef = CheckAssembliesDefinition(folderReg, previousAsmdefOrRefDefined, ref pendingAsmdefOrRef, insideSampleTilde) || folderHasCsWithNoAsmdef;
            }

            if (pendingAsmdefOrRef > 0 && !folderHasCsFilesDefined && !folderHasCsWithNoAsmdef && asmdefOrAsmrefFilesCount > 0)
            {
                AddError(AssemblyDefinitionWithoutScriptAssociatedErrorMessage(folderRegister, filesInFolder));
                pendingAsmdefOrRef--;
            }

            if (pendingAsmdefOrRef > 0 && !folderHasCsFilesDefined && folderHasCsWithNoAsmdef && asmdefOrAsmrefFilesCount > 0)
                folderHasCsWithNoAsmdef = false;

            return folderHasCsWithNoAsmdef;
        }

        static string AssembliesDefinitionFileNames(List<FileOrFolderInfo> filesInFolder)
        {
            return string.Join(", ", filesInFolder.Where(s => s.Name.EndsWith(k_AssemblyDefExt, StringComparison.OrdinalIgnoreCase)
                                                           || s.Name.EndsWith(k_AssemblyRefExt, StringComparison.OrdinalIgnoreCase)).Select(s => s.Name));
        }

        static string ScriptsDefinitionFileNames(List<FileOrFolderInfo> filesInFolder)
        {
            return string.Join(", ", filesInFolder.Where(s => s.Name.EndsWith(k_ScriptExt, StringComparison.OrdinalIgnoreCase)).Select(s => s.Name));
        }

        static int AsmdefAndAsmrefFilesCount(List<FileOrFolderInfo> filesInFolder)
        {
            return filesInFolder.Count(s => s.Name.EndsWith(k_AssemblyDefExt, StringComparison.OrdinalIgnoreCase)
                                         || s.Name.EndsWith(k_AssemblyRefExt, StringComparison.OrdinalIgnoreCase));
        }
    }
}
