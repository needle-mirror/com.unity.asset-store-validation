using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
using UnityEngine;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class ManifestNameFieldValidation : BaseValidation
    {
        Regex _regex = new Regex("^[a-z0-9][a-z0-9-._]{1,213}$");
        internal static readonly string k_DocsFilePath = "manifest_name_field_validation.html";
        
        public ManifestNameFieldValidation()
        {
            TestName = "Manifest Name Field Validation";
            TestDescription = "Validates that the package manifest name field meets certain criteria.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] {ValidationType.Structure, ValidationType.AssetStore};
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            CheckNameField(Context.ProjectPackageInfo);
        }

        void CheckNameField(ManifestData manifestData)
        {
            
            // Unity package names should not contain any uppercase letters
            if (manifestData.name.Any(char.IsUpper))
            {
                AddError($"Unity does not allow for uppercase letters in the package name {manifestData.name}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-contains-uppercase-letters")}");
            }

            // Unity package name cannot exceed 214 characters
            if (manifestData.name.Length > 214)
            {
                AddError($"Unity package name cannot exceed 214 characters {manifestData.name}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-is-too-long")}");
            }
            
            // Unity will try to do things to packages ending in these extensions that may be undesired
            if (manifestData.name.EndsWith(".plugin") || manifestData.name.EndsWith(".bundle") || manifestData.name.EndsWith(".framework"))
            {
                AddError($"Unity package name cannot end with extensions \".plugin\", \".bundle\", or \".framework\". The current package name is: {manifestData.name}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-ends-in-forbidden-extension")}");
            }

            if (manifestData.name.Trim('.').Split('.').Length < 2)
            {
                AddError($"Package name {manifestData.name} must contain a dot separator \".\" to be considered valid. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-must-contain-dot-separator")}");
            }

            if (!_regex.IsMatch(manifestData.name))
            {
                AddError($"Package name {manifestData.name} is invalid. Please ensure that the package does not contain special characters, then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-does-not-match-regex")}");
            }
        }
    }   
}
