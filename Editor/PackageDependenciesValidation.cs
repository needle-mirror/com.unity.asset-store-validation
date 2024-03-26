using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class PackageDependenciesValidation : BaseValidation
    {
        static readonly string k_DocsFilePath = "package_dependencies_validation.html";
        List<ReferenceNameAndPath> m_PrecompiledReferencesNameAndPath = new ();
        List<ReferenceNameAndPath> m_ReferencesNameAndPath = new ();
        HashSet<string> m_PackagesInUse = new ();
        Dictionary<string, string> m_RegisteredPackagesDlls = new ();
        const string k_GuidReferencePrefix = "GUID:";
        const char K_GuidReferenceSeperator = ':';

        
        internal readonly string k_ErrorSingleMissingDependenciesFromAssembly =
            "The following package dependency {0} is not used and could be removed from the package manifest. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unused-package-dependency-in-manifest");
        
        internal readonly string k_ErrorMissingDependenciesFromAssembly =
            "The following package dependencies \n{0}\nare not used and could be removed from the package manifest. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unused-package-dependency-in-manifest");

        internal readonly string k_ErrorMissingSingleDependencyFromManifest =
            "The following package dependency {0} is missing from the package manifest. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-dependency-in-manifest");
        
        internal readonly string k_ErrorMissingDependenciesFromManifest =
            "The following package dependencies \n{0}\nare missing from the package manifest. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-dependency-in-manifest");
        
        internal readonly string k_ErrorMissingPrecompiledFromManifest =
                "The following precompiled references : {0} are missing from the package dependencies. " +
                ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-dependency-in-manifest");
        
        internal readonly string k_ErrorFormatGuidReference =
            "Malformed guid reference {0}. Please validate your assemblies. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "malformed-guid-reference");

        internal readonly string k_ErrorUnknownReference =
            "Unknown package reference of {0} in {1}. Please verify your assembly. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unknown-reference");

        internal readonly string k_ErrorUnknownPrecompiledReference =
            $"The following precompiled reference {0} in {1} could not be found. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unknown-precompiled-reference");

        internal IPackageInfoHelper m_PackageInfoHelper;

        public PackageDependenciesValidation()
        {
            TestName = "Package Dependencies";
            TestDescription = "Validate that the package dependencies are useful and complete.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.AssetStore };
            m_PackageInfoHelper = new PackageInfoHelper();
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            m_RegisteredPackagesDlls = m_PackageInfoHelper.GetDllsFromRegisteredPackages();
            GetReferencedPackagesFromAssembly();

            ValidateReferences();
            ValidatePrecompiledReferences();
            ValidateUnusedPackageDependencies();
        }

        void ValidateUnusedPackageDependencies()
        {
            var unUsedPackages = new StringBuilder();
            var unUsedPackagesTracker = new List<string>();
            foreach (var dependency in Context.PublishPackageInfo.dependencies)
            {
                if(!m_PackagesInUse.Any(i => i.Contains(dependency.Key)))
                    unUsedPackagesTracker.Add($"{dependency.Key}@{dependency.Value}");
            }

            unUsedPackages.Append(string.Join(",\n", unUsedPackagesTracker));
            CheckErrors(unUsedPackages,
                unUsedPackagesTracker.Count > 1
                    ? k_ErrorMissingDependenciesFromAssembly
                    : k_ErrorSingleMissingDependenciesFromAssembly);
        }

        void CheckErrors(StringBuilder errors, string message)
        {
            if (errors.Length > 0)
            {
                AddError(string.Format(message, errors));
            }
        }

        void GetReferencedPackagesFromAssembly()
        {
            var assemblyDefinitions =
                Directory.EnumerateFiles(Context.PublishPackageInfo.path, "*.asmdef", SearchOption.AllDirectories)
                    .Where(i => !i.Split(Path.DirectorySeparatorChar).Any(j => (j.EndsWith("~") || j.StartsWith(".")) && !j.Contains("Samples") && j.Equals("cvs", StringComparison.OrdinalIgnoreCase))).ToList();
            var assemblyReferences = Directory
                .EnumerateFiles(Context.PublishPackageInfo.path, "*.asmref", SearchOption.AllDirectories).ToList();

            assemblyDefinitions.AddRange(assemblyReferences);

            foreach (var assemblyDefinition in assemblyDefinitions)
            {
                var content = File.ReadAllText(assemblyDefinition);
                var packageParsedMetaData = JObject.Parse(content);
                var path = Path.Combine(Context.PublishPackageInfo.name, assemblyDefinition.Substring(Context.PublishPackageInfo.path.Length + 1));
                ExtractReferencesFromAssembly(packageParsedMetaData, m_ReferencesNameAndPath, "references", path);
                ExtractReferencesFromAssembly(packageParsedMetaData, m_PrecompiledReferencesNameAndPath, "precompiledReferences", path);
            }
        }

        void ExtractReferencesFromAssembly(JObject packageParsedMetaData, List<ReferenceNameAndPath> collection, string key, string path)
        {
            if (!packageParsedMetaData.TryGetValue(key, out var packageVersionsAsJson) ||
                !packageVersionsAsJson.HasValues || !packageVersionsAsJson.Any())
            {
                return;
            }

            var references = packageVersionsAsJson.Value<JArray>()?.ToObject<List<string>>();

            foreach (var reference in (references ?? new List<string>()).Where(reference => !string.IsNullOrWhiteSpace(reference)))
            {
                collection.Add(new ReferenceNameAndPath {name = reference, path = path});
            }
        }

        void ValidateReferences()
        {
            var missingReferences = new StringBuilder();
            var missingReferencesTracker = new List<string>();

            foreach (var reference in m_ReferencesNameAndPath)
            {
                var packageId = reference.name.StartsWith(k_GuidReferencePrefix)
                    ? GetGuidReference(reference)
                    : GetPackageIdFromReference(reference);
                var packageName = packageId.Split("@")[0];

                if (string.IsNullOrWhiteSpace(packageName) || packageName.Equals(Context.PublishPackageInfo.name))
                {
                    continue;
                }

                if (!Context.PublishPackageInfo.dependencies.TryGetValue(packageName, out _))
                {
                    var dependency = $"{reference.name}({packageId})";
                    if(!missingReferencesTracker.Contains(dependency))
                        missingReferencesTracker.Add(dependency);
                }
                else
                {
                    m_PackagesInUse.Add(packageId);
                }
            }

            missingReferences.Append(string.Join(",\n", missingReferencesTracker));
            CheckErrors(missingReferences,
                missingReferencesTracker.Count > 1
                    ? k_ErrorMissingDependenciesFromManifest
                    : k_ErrorMissingSingleDependencyFromManifest);
        }

        void ValidatePrecompiledReferences()
        {
            var missingReferences = new StringBuilder();

            foreach (var precompiledReference in m_PrecompiledReferencesNameAndPath)
            {
                var fileName = Path.GetFileNameWithoutExtension(precompiledReference.name);
                if (!m_RegisteredPackagesDlls.TryGetValue(fileName, out var packageId))
                {
                    AddError(string.Format(k_ErrorUnknownPrecompiledReference, precompiledReference.name, precompiledReference.path));
                    continue;
                }

                var packageName = packageId?.Split("@")[0];
                if (packageName?.Equals(Context.PublishPackageInfo.name) == true)
                {
                    continue;
                }

                // check if the precompiled reference is in the package dependency
                if (!Context.PublishPackageInfo.dependencies.TryGetValue(packageName ?? string.Empty, out _))
                {
                    missingReferences.AppendLine($"{precompiledReference.name}({packageId})");
                }
                else
                {
                    m_PackagesInUse.Add(packageId);
                }
            }

            CheckErrors(missingReferences, k_ErrorMissingPrecompiledFromManifest);
        }

        string GetGuidReference(ReferenceNameAndPath reference)
        {
            var referenceItem = reference.name.Split(K_GuidReferenceSeperator);
            if (referenceItem.Length != 2)
            {
                AddError(string.Format(k_ErrorFormatGuidReference, reference.name));
                return string.Empty;
            }

            var packageId = m_PackageInfoHelper.GetPackageIdFromGuid(referenceItem[1]);
            if (!string.IsNullOrWhiteSpace(packageId))
            {
                return packageId;
            }
            AddError(string.Format(k_ErrorUnknownReference, reference.name, reference.path));
            return string.Empty;
        }

        string GetPackageIdFromReference(ReferenceNameAndPath reference)
        {
            var packageId = m_PackageInfoHelper.GetPackageIdFromReferenceName(reference.name);
            if (!string.IsNullOrWhiteSpace(packageId))
            {
                return packageId;
            }
            AddError(string.Format(k_ErrorUnknownReference, reference.name, reference.path));
            return string.Empty;
        }
        
        private class ReferenceNameAndPath
        {
            public string name;
            public string path;
        }
    }
}