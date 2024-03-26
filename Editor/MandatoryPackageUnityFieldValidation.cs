using System;
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

        internal static string UnityFieldMinimumVersionErrorMessage(string version) => $"The \"unity\" field value {version} must be greater" +
            $" than {k_MinimumUnityVersion}. Please update the \"unity\" field with a value greater than {k_MinimumUnityVersion}," +
            $" then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unity-field-min-version")}";

        internal static string UnityReleaseFieldIsMandatoryErrorMessage(string path) => $"In {path}, the \"unityRelease\" field is mandatory. Please ensure that \"unityRelease\" field is set," +
            $" then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unityRelease-field-mandatory")}";
        internal static string InvalidUnityReleaseFieldErrorMessage(string path, string unityRelease) => $"In {path}, the \"unityRelease\" field is invalid. It should only be <number>.<a|b|f>.<number> (e.g. 26.f.1)." +
            $"Current unityRelease = {unityRelease}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unityRelease-field-mandatory")}";

        internal static string UnityReleaseFieldMinimumVersionErrorMessage(string unity, string unityRelease) =>
            $"The combination of the \"unity\" and \"unityRelease\" fields must be greater" +
            $" than {k_MinimumVersion}. Currently, they are {unity}.{unityRelease}. Please update the \"unityRelease\" field with a value greater than {k_MinimumVersion}," +
            $" then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-unityRelease-field-min-version")}";

        internal static readonly UnityVersion k_MinimumUnityVersion = new UnityVersion("2021.3");

        internal static readonly UnityRelease k_MinimumUnityReleaseVersion = new UnityRelease("26f1");

        internal static readonly string k_MinimumVersion = $"{k_MinimumUnityVersion}.{k_MinimumUnityReleaseVersion}";

        UnityVersion m_UnityVersion;
        UnityRelease m_UnityRelease;

        public MandatoryPackageUnityFieldValidation()
        {
            TestName = "Manifest: Mandatory Unity and UnityRelease Fields";
            TestDescription = "A package must have a valid Unity and UnityRelease fields to correctly notify compatibility with Unity versions.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.Structure, ValidationType.AssetStore, ValidationType.InternalTesting };
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;

            var manifestData = Context.ProjectPackageInfo;

            ValidateUnityFields(manifestData);

            if (TestState != TestState.Succeeded) return;

            VerifyIsVersionIsLowerThanMinimumVersion(manifestData);
        }

        private void ValidateUnityFields(ManifestData manifestData)
        {
            try
            {
                m_UnityVersion = new UnityVersion(manifestData.unity);
            }
            catch (UnityVersionRequiredException)
            {
                AddError(UnityFieldIsMandatoryErrorMessage(manifestData.path));
            }
            catch (UnityVersionFormatException)
            {
                AddError(InvalidUnityFieldErrorMessage(manifestData.path, manifestData.unity));
            }

            try
            {
                m_UnityRelease = new UnityRelease(manifestData.unityRelease);
            }
            catch (UnityReleaseRequiredException)
            {
                AddError(UnityReleaseFieldIsMandatoryErrorMessage(manifestData.path));
            }
            catch (UnityReleaseFormatException)
            {
                AddError(InvalidUnityReleaseFieldErrorMessage(manifestData.path, manifestData.unityRelease));
            }
        }

        private void VerifyIsVersionIsLowerThanMinimumVersion(ManifestData manifestData)
        {
            if (m_UnityVersion < k_MinimumUnityVersion)
                AddError(UnityFieldMinimumVersionErrorMessage($"{m_UnityVersion}"));
            else if (m_UnityVersion == k_MinimumUnityVersion && m_UnityRelease < k_MinimumUnityReleaseVersion)
                AddError(UnityReleaseFieldMinimumVersionErrorMessage(manifestData.unity, $"{m_UnityRelease}"));
        }
    }

    public class UnityVersion : IEquatable<UnityVersion>, IComparable<UnityVersion>
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }

        private static readonly Regex UnityVersionRegex = new Regex(@"^(?<major>[0-9]{4})\.(?<minor>[0-9]+)$");

        public UnityVersion(string unityVersion)
        {
            if (string.IsNullOrWhiteSpace(unityVersion))
                throw new UnityVersionRequiredException("Unity version cannot be null or empty.", nameof(unityVersion));

            var versionMatch = UnityVersionRegex.Match(unityVersion);
            if (!versionMatch.Success)
            {
                throw new UnityVersionFormatException("Invalid Unity version format.", nameof(unityVersion));
            }
            Major = int.Parse(versionMatch.Groups["major"].Value);
            Minor = int.Parse(versionMatch.Groups["minor"].Value);
        }

        public override bool Equals(object obj) => Equals(obj as UnityVersion);
        public bool Equals(UnityVersion other)
        {
            return other != null &&
                   Major == other.Major &&
                   Minor == other.Minor;
        }
        public override int GetHashCode() => HashCode.Combine(Major, Minor);
        public int CompareTo(UnityVersion other)
        {
            if (other == null) return 1;

            int majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0) return majorComparison;

            return Minor.CompareTo(other.Minor);
        }
        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }

        public static bool operator ==(UnityVersion left, UnityVersion right) => Equals(left, right);
        public static bool operator !=(UnityVersion left, UnityVersion right) => !Equals(left, right);
        public static bool operator <(UnityVersion left, UnityVersion right) => left.CompareTo(right) < 0;
        public static bool operator >(UnityVersion left, UnityVersion right) => left.CompareTo(right) > 0;
        public static bool operator <=(UnityVersion left, UnityVersion right) => left.CompareTo(right) <= 0;
        public static bool operator >=(UnityVersion left, UnityVersion right) => left.CompareTo(right) >= 0;
    }

    public class UnityVersionFormatException : ArgumentException
    {
        public UnityVersionFormatException(string message, string paramName) : base(message)
        { }
    }
    
    public class UnityVersionRequiredException : ArgumentException
    {
        public UnityVersionRequiredException(string message, string paramName) : base(message)
        { }
    }

    public class UnityRelease : IEquatable<UnityRelease>, IComparable<UnityRelease>
    {
        private static readonly Regex UnityReleaseRegex = new Regex(@"^(?<releaseNumber>[0-9]+)(?<releaseLetter>[a|b|f])(?<releasePatch>[0-9]+)$");
        public int ReleaseNumber { get; }
        public char ReleaseLetter { get; }
        public int ReleasePatch { get; }

        public UnityRelease(string unityRelease)
        {
            if (string.IsNullOrWhiteSpace(unityRelease))

                throw new UnityReleaseRequiredException("Unity release cannot be null or empty.", nameof(unityRelease));

            var releaseMatch = UnityReleaseRegex.Match(unityRelease);
            if (!releaseMatch.Success)
            {
                throw new UnityReleaseFormatException("Invalid Unity release format.", nameof(unityRelease));
            }
            ReleaseNumber = int.Parse(releaseMatch.Groups["releaseNumber"].Value);
            ReleaseLetter = releaseMatch.Groups["releaseLetter"].Value[0];
            ReleasePatch = int.Parse(releaseMatch.Groups["releasePatch"].Value);
        }
        public override bool Equals(object obj) => Equals(obj as UnityRelease);
        public bool Equals(UnityRelease other)
        {
            return other != null &&
                   ReleaseNumber == other.ReleaseNumber &&
                   ReleaseLetter == other.ReleaseLetter &&
                   ReleasePatch == other.ReleasePatch;
        }
        public override int GetHashCode() => HashCode.Combine(ReleaseNumber, ReleaseLetter, ReleasePatch);
        public int CompareTo(UnityRelease other)
        {
            if (other == null) return 1;

            int numberComparison = ReleaseNumber.CompareTo(other.ReleaseNumber);
            if (numberComparison != 0) return numberComparison;

            int letterComparison = ReleaseLetter.CompareTo(other.ReleaseLetter);
            if (letterComparison != 0) return letterComparison;

            return ReleasePatch.CompareTo(other.ReleasePatch);
        }
        public override string ToString()
        {
            return $"{ReleaseNumber}{ReleaseLetter}{ReleasePatch}";
        }

        public static bool operator ==(UnityRelease left, UnityRelease right) => Equals(left, right);
        public static bool operator !=(UnityRelease left, UnityRelease right) => !Equals(left, right);
        public static bool operator <(UnityRelease left, UnityRelease right) => left.CompareTo(right) < 0;
        public static bool operator >(UnityRelease left, UnityRelease right) => left.CompareTo(right) > 0;
        public static bool operator <=(UnityRelease left, UnityRelease right) => left.CompareTo(right) <= 0;
        public static bool operator >=(UnityRelease left, UnityRelease right) => left.CompareTo(right) >= 0;
    }

    public class UnityReleaseFormatException : ArgumentException
    {
        public UnityReleaseFormatException(string message, string paramName) : base(message)
        {
        }
    }

    public class UnityReleaseRequiredException : ArgumentException
    {
        public UnityReleaseRequiredException(string message, string paramName) : base(message)
        {
        }
    }

}
