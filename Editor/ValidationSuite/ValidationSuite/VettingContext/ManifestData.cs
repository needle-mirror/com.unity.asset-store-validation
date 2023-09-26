using System;
using System.Collections.Generic;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    class ManifestData
    {
        public string path = "";
        public string name = "";
        public string documentationUrl = "";
        public string displayName = "";
        public string description = "";
        public string unity = "";
        public string unityRelease = "";
        public string version = "";
        public string type = "";
        public string changelogUrl = "";
        [AlternativeSerializationFormat(nameof(authorDetails))]
        public string author;
        [NonSerialized]
        public AuthorDetails authorDetails;
        public List<SampleData> samples = new List<SampleData>();
        public Dictionary<string, string> repository = new Dictionary<string, string>();
        public Dictionary<string, string> dependencies = new Dictionary<string, string>();
        public Dictionary<string, string> relatedPackages = new Dictionary<string, string>();
        public List<string> keywords = new List<string>();

        //Errors during parsing time - to be passed for validation
        [NonSerialized]
        internal List<UnmarshallingException> decodingErrors = new List<UnmarshallingException>();

        public PackageType PackageType => PackageTypeParser.Parse(type);

        [Obsolete("use PackageType instead")]
        public bool IsProjectTemplate
        {
            get { return PackageType == PackageType.Template; }
        }

        public string Id
        {
            get { return GetPackageId(name, version); }
        }

        public static string GetPackageId(string name, string version)
        {
            return name + "@" + version;
        }

        /// <summary>
        /// If the package we are evaluating is authored by Unity
        /// </summary>
        /// <returns></returns>
        public bool IsAuthoredByUnity()
        {
            return name.StartsWith("com.unity.");
        }
    }

    [Serializable]
    class SampleData
    {
        public string displayName = "";
        public string description = "";
        public string path = "";
    }

    [Serializable]
    class AuthorDetails
    {
        public string name = "";
        public string email = "";
        public string url = "";
    }
}
