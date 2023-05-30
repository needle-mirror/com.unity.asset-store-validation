using System.Text.RegularExpressions;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class PackageUnityFieldCheck: BaseValidation
    {
        const string k_UnityRegex = @"^[0-9]{4}\.[0-9]+$";
        const string k_UnityReleaseRegex = @"^[0-9]+[a|b|f]{1}[0-9]+$";

        internal const string k_DocsFilePath = "package_unity_field_check.html";

        public PackageUnityFieldCheck()
        {
            TestName = "Package Unity Field Check";
            TestDescription = "A package needs to have a valid Unity field to correctly notify compatibility with Unity versions";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] {ValidationType.Structure, ValidationType.AssetStore};
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;

            var manifestData = Context.ProjectPackageInfo;

            var hasUnity = !string.IsNullOrWhiteSpace(manifestData.unity);
            var hasUnityRelease = !string.IsNullOrWhiteSpace(manifestData.unityRelease);

            // We have nothing to check
            if (!hasUnity && !hasUnityRelease) return;

            // Check that unity field is valid
            if (hasUnity && manifestData.unity.Length > 6 || !Regex.Match(manifestData.unity, k_UnityRegex).Success)
                AddError($"In {manifestData.path}, \"unity\" is invalid. It should only be <MAJOR>.<MINOR> (e.g. 2018.4). Current unity = {manifestData.unity}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unity-is-invalid")}");

            // Check that unity release field is valid
            if (hasUnityRelease && !Regex.Match(manifestData.unityRelease, k_UnityReleaseRegex).Success)
            {
                AddError(
                    $"In {manifestData.path}, \"unityRelease\" is invalid. Current unityRelease = {manifestData.unityRelease}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unityrelease-is-invalid")}");
            }

            // Release should be accompanied by Unity
            if (!hasUnity)
            {
                AddError(
                    $"In {manifestData.path}, \"unityRelease\" needs a \"unity\" field to be used. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unityrelease-without-unity")}");
            }
        }
    }
}
