using System.Text.RegularExpressions;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class MandatoryPackageUnityFieldValidation : BaseValidation
    {
        internal const string k_DocsFilePath = "mandatory_package_unity_fields_validation.html";

        internal static string UnityFieldIsMandatoryErrorMessage(string path) => $"In {path}, the \"unity\" field is mandatory. Please ensure that \"unity\" field is set," +
            $" then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unity-field-mandatory")}";
        internal static string InvalidUnityFieldErrorMessage(string path, string unityVersion) => $"In {path}, \"unity\" is invalid. It should only be <MAJOR>.<MINOR> (e.g. 2018.4)." +
            $" Current unity = {unityVersion}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unity-field-mandatory")}";

        internal static string UnityFieldMinimunVersionErrorMessage(string version) => $"The \"unity\" field value {version} must be greater" +
            $" than {k_MinimumMajorUnityVersion}.{k_MinimumMinorUnityVersion}. Please update the \"unity\" field with a value greater than {k_MinimumMajorUnityVersion}.{k_MinimumMinorUnityVersion}," +
            $" then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unity-field-min-version")}";

        internal static string UnityReleaseFieldIsMandatoryErrorMessage(string path) => $"In {path}, the \"unityRelease\" field is mandatory. Please ensure that \"unityRelease\" field is set," +
            $" then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unityRelease-field-mandatory")}";
        internal static string InvalidUnityReleaseFieldErrorMessage(string path, string unityRelease) => $"In {path}, the \"unityRelease\" field is invalid. It should only be <number>.<a|b|f>.<number> (e.g. 26.f.1)." +
            $"Current unityRelease = {unityRelease}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unityRelease-field-mandatory")}";

        internal static string UnityReleaseFieldMinimunVersionErrorMessage(string unity, string unityRelease) =>
            $"The combination of the \"unity\" and \"unityRelease\" fields must be greater" +
            $" than {k_MinimunVersion}. Currently, they are {unity}.{unityRelease}. Please update the \"unityRelease\" field with a value greater than {k_MinimunVersion}," +
            $" then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unityRelease-field-min-version")}";

        internal const int k_MinimumMajorUnityVersion = 2021;
        internal const int k_MinimumMinorUnityVersion = 3;

        internal const int k_MinimumUnityReleaseFirstNumberVersion = 26;
        internal const char k_MinimumUnityReleaseLetterVersion = 'f';
        internal const int k_MinimumUnityReleaseSecondNumberVersion = 1;

        internal static readonly string k_MinimunVersion = $"{k_MinimumMajorUnityVersion}.{k_MinimumMinorUnityVersion}.{k_MinimumUnityReleaseFirstNumberVersion}{k_MinimumUnityReleaseLetterVersion}{k_MinimumUnityReleaseSecondNumberVersion}";

        Regex m_UnityFieldRegex = new Regex(@"^([0-9]{4})\.([0-9]+)$");
        Regex m_UnityReleaseFieldRegex = new Regex(@"^([0-9]+)([a|b|f]{1})([0-9]+)$");

        public MandatoryPackageUnityFieldValidation()
        {
            TestName = "Manifest: Mandatory Unity and UnityRelease Fields";
            TestDescription = "A package must have a valid Unity and UnityRelease fields to correctly notify compatibility with Unity versions.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.Structure, ValidationType.AssetStore };
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;

            var manifestData = Context.ProjectPackageInfo;

            var hasUnity = !string.IsNullOrWhiteSpace(manifestData.unity);
            var hasUnityRelease = !string.IsNullOrWhiteSpace(manifestData.unityRelease);

            if (!hasUnity)
            {
                AddError(UnityFieldIsMandatoryErrorMessage(manifestData.path));
            }
            else if (!IsUnityFieldValid(manifestData.unity))
            {
                AddError(InvalidUnityFieldErrorMessage(manifestData.path, manifestData.unity));
            }
            else
            {
                var matchVersion = m_UnityFieldRegex.Match(manifestData.unity);

                var majorVersion = int.Parse(matchVersion.Groups[1].Value);
                var minorVersion = int.Parse(matchVersion.Groups[2].Value);
                if (IsUnityFieldLowerThanMinimunVersion(majorVersion, minorVersion))
                {
                    AddError(UnityFieldMinimunVersionErrorMessage($"{majorVersion}.{minorVersion}"));
                }
                else if (!hasUnityRelease)
                {
                    AddError(UnityReleaseFieldIsMandatoryErrorMessage(manifestData.path));
                }
                else if (!IsUnityReleaseValid(manifestData.unityRelease))
                {
                    AddError(InvalidUnityReleaseFieldErrorMessage(manifestData.path, manifestData.unity));
                }
                else
                {
                    var matchPatch = m_UnityReleaseFieldRegex.Match(manifestData.unityRelease);

                    var releaseFirstNumber = int.Parse(matchPatch.Groups[1].Value);
                    char releaseLetter = matchPatch.Groups[2].Value[0];
                    var releaseSecondNumber = int.Parse(matchPatch.Groups[3].Value);

                    if (IsUnityReleaseLowerThanMinimunVersion(releaseFirstNumber, releaseLetter, releaseSecondNumber))
                    {
                        AddError(UnityReleaseFieldMinimunVersionErrorMessage(manifestData.unity, $"{releaseFirstNumber}{releaseLetter}{releaseSecondNumber}"));
                    }
                }
            }
        }

        private bool IsUnityFieldValid(string unity)
        {
            return unity.Length >= 6 && m_UnityFieldRegex.IsMatch(unity);
        }

        private static bool IsUnityFieldLowerThanMinimunVersion(int majorVersion, int minorVersion)
        {
            return k_MinimumMajorUnityVersion > majorVersion || (k_MinimumMajorUnityVersion == majorVersion && k_MinimumMinorUnityVersion > minorVersion);
        }

        private bool IsUnityReleaseValid(string unityRelease)
        {
            return m_UnityReleaseFieldRegex.IsMatch(unityRelease);
        }

        private static bool IsUnityReleaseLowerThanMinimunVersion(int releaseFirstNumber, char releaseLetter, int releaseSecondNumber)
        {
            return k_MinimumUnityReleaseFirstNumberVersion > releaseFirstNumber
                                    || (k_MinimumUnityReleaseFirstNumberVersion == releaseFirstNumber && releaseLetter.CompareTo(k_MinimumUnityReleaseLetterVersion) < 0)
                                    || (k_MinimumUnityReleaseFirstNumberVersion == releaseFirstNumber && releaseLetter.CompareTo(k_MinimumUnityReleaseLetterVersion) == 0 && k_MinimumUnityReleaseSecondNumberVersion > releaseSecondNumber);
        }

    }
}
