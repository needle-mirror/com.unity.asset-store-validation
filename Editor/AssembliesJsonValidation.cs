using System;
using System.IO;
using UnityEngine;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;


namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class AssembliesJsonValidation : BaseValidation
    {
        
        internal static readonly string k_DocsFilePath = "assemblies_json_validation.html";

        public AssembliesJsonValidation()
        {
            TestName = "Assemblies Json";
            TestDescription = "Validate that the assembly files are valid json.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] {ValidationType.Structure, ValidationType.AssetStore};

        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            
            CheckAssemblies(Context.PublishPackageInfo.path);
        }

        void CheckAssemblies(string folder)
        {
            var asmdefs = Directory.GetFileSystemEntries(folder, "*.asmdef", SearchOption.AllDirectories);
            var asmrefs = Directory.GetFileSystemEntries(folder, "*.asmref", SearchOption.AllDirectories);

            foreach (var file in asmdefs)
            {
                try
                {
                    JsonUtility.FromJson<string>(File.ReadAllText(file));
                }
                catch (Exception e)
                {
                    AddError($"{e.Message} {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "assemblies-must-be-valid-json")}");
                }
            }

            foreach (var file in asmrefs)
            {
                try
                {
                    JsonUtility.FromJson<string>(File.ReadAllText(file));
                }
                catch (Exception e)
                {
                    AddError($"{e.Message} {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "assemblies-must-be-valid-json")}");
                }
            }
        }
    }   
}