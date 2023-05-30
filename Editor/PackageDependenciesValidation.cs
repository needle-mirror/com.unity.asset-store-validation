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
        HashSet<string> m_PrecompiledReferences = new HashSet<string>();
        HashSet<string> m_References = new HashSet<string>();
        HashSet<string> m_PackagesInUse = new HashSet<string>();
        Dictionary<string, string> m_RegisteredPackagesDlls = new Dictionary<string, string>();
        const string k_GuidReferencePrefix = "GUID:";
        const char K_GuidReferenceSeperator = ':';

        internal readonly string k_ErrorMissingDependenciesFromAssembly =
            "The following package dependencies {0} are not used and could be removed from the package manifest. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unused-package-dependency-in-manifest");

        internal readonly string k_ErrorMissingDependenciesFromManifest =
            "The following package dependencies {0} \n are missing from the package manifest. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-dependency-in-manifest");
        
        internal readonly string k_ErrorMissingPrecompiledFromManifest =
                "The following precompiled references : {0} are missing from the package dependencies. " +
                ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-dependency-in-manifest");
        
        internal readonly string k_ErrorFormatGuidReference =
            "Malformed guid reference {0}. Please validate your assemblies. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "malformed-guid-refence");

        internal readonly string k_ErrorUnknownReference =
            "Unknown package reference {0}. Please verify your assembly. " +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unknown-reference");

        internal readonly string k_ErrorUnknownPrecompiledReference =
            $"The following precompiled reference: {0} could not be found." +
            ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unknown-precompiled-reference");

        internal IPackageInfoHelper m_PackageInfoHelper;

        public PackageDependenciesValidation()
        {
            TestName = "Package Dependencies Validation";
            TestDescription = "Validate that the package dependencies are useful and complete";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] {ValidationType.AssetStore};
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
            foreach (var dependency in Context.PublishPackageInfo.dependencies)
            {
                if (!m_PackagesInUse.Contains(dependency.Key))
                {
                    unUsedPackages.AppendLine($"{dependency.Key}@{dependency.Value}");
                }
            }

            CheckErrors(unUsedPackages, k_ErrorMissingDependenciesFromAssembly);
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
                    .ToList();
            var assemblyReferences = Directory
                .EnumerateFiles(Context.PublishPackageInfo.path, "*.asmref", SearchOption.AllDirectories).ToList();

            assemblyDefinitions.AddRange(assemblyReferences);

            foreach (var assemblyDefinition in assemblyDefinitions)
            {
                var content = File.ReadAllText(assemblyDefinition);
                var packageParsedMetaData = JObject.Parse(content);

                ExtractReferencesFromAssembly(packageParsedMetaData, m_References, "references");
                ExtractReferencesFromAssembly(packageParsedMetaData, m_PrecompiledReferences, "precompiledReferences");
            }
        }

        void ExtractReferencesFromAssembly(JObject packageParsedMetaData, HashSet<string> collection,
            string key)
        {
            if (!packageParsedMetaData.TryGetValue(key, out var packageVersionsAsJson) ||
                !packageVersionsAsJson.HasValues || !packageVersionsAsJson.Any())
            {
                return;
            }

            var references = packageVersionsAsJson.Value<JArray>()?.ToObject<List<string>>();

            foreach (var reference in references)
            {
                if (string.IsNullOrWhiteSpace(reference))
                {
                    continue;
                }

                collection.Add(reference);
            }
        }

        void ValidateReferences()
        {
            var missingReferences = new StringBuilder();

            foreach (var reference in m_References)
            {
                var packageName = reference.StartsWith(k_GuidReferencePrefix)
                    ? GetGuidReference(reference)
                    : GetPackageNameFromReference(reference);

                if (string.IsNullOrWhiteSpace(packageName) || packageName.Equals(Context.PublishPackageInfo.name))
                {
                    continue;
                }

                if (!Context.PublishPackageInfo.dependencies.TryGetValue(packageName, out _))
                {
                    missingReferences.AppendLine($"{reference}({packageName})");
                }
                else
                {
                    m_PackagesInUse.Add(packageName);
                }
            }
            
            CheckErrors(missingReferences, k_ErrorMissingDependenciesFromManifest);
        }

        void ValidatePrecompiledReferences()
        {
            var missingReferences = new StringBuilder();

            foreach (var precompiledReference in m_PrecompiledReferences)
            {
                var fileName = Path.GetFileNameWithoutExtension(precompiledReference);
                if (!m_RegisteredPackagesDlls.TryGetValue(fileName, out var packageName))
                {
                    AddError(string.Format(k_ErrorUnknownPrecompiledReference, precompiledReference));
                    continue;
                }

                if (packageName.Equals(Context.PublishPackageInfo.name))
                {
                    continue;
                }

                // check if the precompiled reference is in the package dependency
                if (!Context.PublishPackageInfo.dependencies.TryGetValue(packageName, out _))
                {
                    missingReferences.AppendLine($"{precompiledReference}({packageName})");
                }
                else
                {
                    m_PackagesInUse.Add(packageName);
                }
            }

            CheckErrors(missingReferences, k_ErrorMissingPrecompiledFromManifest);
        }

        string GetGuidReference(string referenceName)
        {
            var referenceItem = referenceName.Split(K_GuidReferenceSeperator);
            if (referenceItem.Length != 2)
            {
                AddError(string.Format(k_ErrorFormatGuidReference, referenceName));
                return string.Empty;
            }

            var packageName = m_PackageInfoHelper.GetPackageNameFromGuid(referenceItem[1]);
            if (!string.IsNullOrWhiteSpace(packageName)) return packageName;
            AddError(string.Format(k_ErrorUnknownReference, referenceName));
            return string.Empty;
        }

        string GetPackageNameFromReference(string referenceName)
        {
            var name = m_PackageInfoHelper.GetPackageNameFromReferenceName(referenceName);
            if (!string.IsNullOrWhiteSpace(name)) return name;
            AddError(string.Format(k_ErrorUnknownReference, referenceName));
            return string.Empty;
        }
    }
}