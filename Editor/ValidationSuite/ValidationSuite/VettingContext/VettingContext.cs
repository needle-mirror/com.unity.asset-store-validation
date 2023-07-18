using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor.PackageManager.AssetStoreValidation.Semver;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    /// <summary>
    /// Class containing package data required for vetting.
    /// </summary>
    class VettingContext
    {
        public bool IsCore { get; set; }

        public ManifestData ProjectPackageInfo { get; set; }
        public ManifestData PublishPackageInfo { get; set; }
        public ManifestData PreviousPackageInfo { get; set; }
        public float PublishPackageSizeMb { get; set; }
        public string[] AllVersions { get; set; }

        public ManifestData VSuiteInfo { get; set; }

        public bool PackageExistsOnProduction { get; set; }
        public bool PackageVersionExistsOnProduction { get; set; }

        public string PreviousPackageBinaryDirectory { get; set; }
        public ValidationType ValidationType { get; set; }
        public PackageType PackageType { get; set; }
        public const string PreviousVersionBinaryPath = "Temp/ApiValidationBinaries";
        public List<RelatedPackage> relatedPackages = new List<RelatedPackage>();

#if UNITY_2021_1_OR_NEWER
        public Dictionary<string, PackageInfo> PackageInfoList = new Dictionary<string, PackageInfo>();

        public PackageInfo GetPackageInfo(string packageName)
        {
            if (PackageInfoList.ContainsKey(packageName))
            {
                return PackageInfoList[packageName];
            }

            var packageInfo = PackageInfo.FindForAssetPath($"Packages/{packageName}");
            PackageInfoList.Add(packageName, packageInfo);
            return packageInfo;
        }

#endif
        public static VettingContext CreatePackmanContext(string packageId, ValidationType validationType)
        {
            Profiler.BeginSample("CreatePackmanContext");
            ActivityLogger.Log("Starting Packman Context Creation");

            var context = new VettingContext();
            var packageParts = packageId.Split('@');
            var packageList = Utilities.UpmListOffline();

            ActivityLogger.Log("Looking for package {0} in project", packageParts[0]);
            var packageInfo = packageList.SingleOrDefault(p => p.name == packageParts[0] && p.version == packageParts[1]);

            if (packageInfo == null)
            {
                throw new ArgumentException("Package Id " + packageId + " is not part of this project.");
            }

            context.IsCore = packageInfo.source == PackageSource.BuiltIn && packageInfo.type != "module";

            context.ValidationType = validationType;
            context.ProjectPackageInfo = GetManifest(packageInfo.resolvedPath);
            context.PackageType = context.ProjectPackageInfo.PackageType;

            if (context.ValidationType != ValidationType.VerifiedSet)
            {
                try
                {
                    ActivityLogger.Log($"Checking if package {packageInfo.name} exists in the Unity registry");
                    context.PackageExistsOnProduction = Utilities.PackageExistsOnProduction(packageInfo.name);
                    ActivityLogger.Log(
                        $"Package {packageInfo.name} {(context.PackageExistsOnProduction ? "is" : "is not")} in the Unity registry");

                    ActivityLogger.Log($"Checking if package {packageInfo.packageId} exists in the Unity registry");
                    context.PackageVersionExistsOnProduction =
                        Utilities.PackageExistsOnProduction(packageInfo.packageId, packageInfo.version);
                    ActivityLogger.Log(
                        $"Package {packageInfo.packageId} {(context.PackageExistsOnProduction ? "is" : "is not")} in the Unity registry");
                }
                catch (Exception e) when (e is ApplicationException or NpmUnusableException)
                {
                    ActivityLogger.Log($"An error occurred when trying to verify if the package already exists in the Unity registry\n{e}");
                }
            }

            if (packageInfo.source == PackageSource.Embedded || packageInfo.source == PackageSource.Local)
            {
                var publishPackagePath = PublishPackage(context);
                context.PublishPackageInfo = GetManifest(publishPackagePath);
            }
            else
            {
                context.PublishPackageInfo = GetManifest(packageInfo.resolvedPath);
            }

            Profiler.BeginSample("RelatedPackages");
            foreach (var relatedPackage in context.PublishPackageInfo.relatedPackages)
            {
                // Check to see if the package is available locally
                // We are only focusing on local packages to avoid validation suite failures in CI
                // when the situation arises where network connection is impaired
                ActivityLogger.Log("Looking for related package {0} in the project", relatedPackage.Key);
                var foundRelatedPackage = Utilities.UpmListOffline().Where(p => p.name.Equals(relatedPackage.Key));
                var relatedPackageInfo = foundRelatedPackage.ToList();
                if (!relatedPackageInfo.Any())
                {
                    ActivityLogger.Log(String.Format("Cannot find the relatedPackage {0} ", relatedPackage.Key));
                    continue;
                }
                context.relatedPackages.Add(new RelatedPackage(relatedPackage.Key, relatedPackage.Value,
                    relatedPackageInfo.First().resolvedPath));
            }
            Profiler.EndSample();

            // No need to compare against the previous version of the package if we're testing out the verified set.
            if (context.ValidationType != ValidationType.VerifiedSet)
            {
                ActivityLogger.Log("Looking for previous package version");

                // List out available versions for a package
                var foundPackages = Utilities.UpmSearch(context.ProjectPackageInfo.name);
                
                ActivityLogger.Log($"Search of {context.ProjectPackageInfo.name} finished. Upm found {foundPackages.Length} matches");

                // If it exists, get the last one from that list.
                if (foundPackages != null && foundPackages.Length > 0)
                {
                    // Get the last released version of the package
                    var previousPackagePath = GetPreviousReleasedPackage(context.ProjectPackageInfo, foundPackages[0]);
                    if (!string.IsNullOrEmpty(previousPackagePath))
                    {
                        context.PreviousPackageInfo = GetManifest(previousPackagePath);
                        context.DownloadAssembliesForPreviousVersion();
                    }

                    // Fill the versions for later use
                    context.AllVersions = foundPackages[0].versions.all;
                }
            }
            else
            {
                context.PreviousPackageInfo = null;
            }

            context.VSuiteInfo = GetPackageValidationSuiteInfo(packageList);

            Profiler.EndSample();

            return context;
        }

        public static VettingContext CreateAssetStoreContext(string packageName, string packageVersion, string packagePath, string previousPackagePath)
        {
            var context = new VettingContext();
            context.ProjectPackageInfo = new ManifestData() { path = packagePath, name = packageName, version = packageVersion };
            context.PublishPackageInfo = new ManifestData() { path = packagePath, name = packageName, version = packageVersion };
            context.PreviousPackageInfo = string.IsNullOrEmpty(previousPackagePath) ? null : new ManifestData() { path = previousPackagePath, name = packageName, version = "Previous" };
            context.ValidationType = ValidationType.AssetStore;
            return context;
        }

        public static ManifestData GetManifest(string packagePath)
        {
            Profiler.BeginSample("GetManifest");

            // Start by parsing the package's manifest data.
            var manifestPath = Path.Combine(packagePath, Utilities.PackageJsonFilename);

            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException(manifestPath);
            }

            // Read manifest json data, and convert it.
            try
            {
                var textManifestData = File.ReadAllText(manifestPath);

                var parsedManifest = SimpleJsonReader.ReadObject(textManifestData);
                if (parsedManifest == null)
                    throw new ArgumentException("invalid JSON");

                var manifest = new ManifestData();

                var unmarshallingErrors = new List<UnmarshallingException>();
                manifest = JsonUnmarshaller.GetValue<ManifestData>(parsedManifest, ref unmarshallingErrors);
                manifest.decodingErrors.AddRange(unmarshallingErrors);

                manifest.path = packagePath;

                Profiler.EndSample();

                return manifest;
            }
            catch (ArgumentException e)
            {
                Profiler.EndSample();
                throw new Exception($"Could not parse json in file {manifestPath} because of: {e}");
            }
        }

        internal VersionChangeType VersionChangeType
        {
            get
            {
                if (PreviousPackageInfo == null || PreviousPackageInfo.version == null ||
                    ProjectPackageInfo == null || ProjectPackageInfo.version == null)
                {
                    return VersionChangeType.Unknown;
                }
                var prevVersion = SemVersion.Parse(PreviousPackageInfo.version);
                var curVersion = SemVersion.Parse(ProjectPackageInfo.version);

                if (curVersion.CompareByPrecedence(prevVersion) < 0)
                    throw new ArgumentException("Previous version number comes after current version number");

                if (curVersion.Major > prevVersion.Major)
                    return VersionChangeType.Major;
                if (curVersion.Minor > prevVersion.Minor)
                    return VersionChangeType.Minor;
                if (curVersion.Patch > prevVersion.Patch)
                    return VersionChangeType.Patch;

                throw new ArgumentException("Previous version number " + PreviousPackageInfo.version + " is the same major/minor/patch version as the current package " + ProjectPackageInfo.version);
            }
        }

        static string PublishPackage(VettingContext context)
        {
            var packagePath = context.ProjectPackageInfo.path;
            if (!context.ProjectPackageInfo.PackageType.NeedsLocalPublishing())
            {
                return packagePath;
            }

            Profiler.BeginSample("PublishPackage");

            var tempPath = System.IO.Path.GetTempPath();

            //Use upm-template-tools package-ci
            var packagesGenerated = PackageCIUtils.Pack(packagePath, tempPath);

            if (packagesGenerated != null && packagesGenerated.Count > 0)
            {
                context.PublishPackageSizeMb = (new FileInfo(packagesGenerated[0]).Length / 1024f) / 1024f;
            }

            var publishPackagePath = Path.Combine(tempPath, "publish-" + context.ProjectPackageInfo.Id);
            var deleteOutput = true;
            foreach (var packageTgzName in packagesGenerated)
            {
                Utilities.ExtractPackage(packageTgzName, tempPath, publishPackagePath, context.ProjectPackageInfo.name, deleteOutput);
                deleteOutput = false;
            }

            Profiler.EndSample();

            return publishPackagePath;
        }

        static string GetPreviousReleasedPackage(ManifestData projectPackageInfo, PackageInfo packageInfo)
        {
            var version = SemVersion.Parse(projectPackageInfo.version);
            var previousVersions = packageInfo.versions.all.Where(v =>
            {
                var prevVersion = SemVersion.Parse(v);
                // ignore pre-release and build tags when finding previous version
                return prevVersion < version && !(prevVersion.Major == version.Major && prevVersion.Minor == version.Minor && prevVersion.Patch == version.Patch);
            });

            // Find the last version on Production
            string previousVersion = null;
            previousVersions = previousVersions.Reverse();

            foreach (var prevVersion in previousVersions)
            {
                try
                {
                    if (Utilities.PackageExistsOnProduction(packageInfo.name + "@" + prevVersion))
                    {
                        previousVersion = prevVersion;
                        break;
                    }    
                }
                catch (Exception e) when (e is NpmUnusableException or ApplicationException)
                {
                    ActivityLogger.Log(e.Message);
                    break;
                }
            }

            if (previousVersion != null)
            {
                try
                {
                    ActivityLogger.Log("Retrieving previous package version {0}", previousVersion);
                    var previousPackageId = ManifestData.GetPackageId(projectPackageInfo.name, previousVersion);
                    var tempPath = Path.GetTempPath();
                    var previousPackagePath = Path.Combine(tempPath, "previous-" + previousPackageId);
                    var packageFileName = Utilities.DownloadPackage(previousPackageId, tempPath);
                    Utilities.ExtractPackage(Path.Combine(tempPath, packageFileName), tempPath, previousPackagePath, projectPackageInfo.name);
                    return previousPackagePath;
                }
                catch (Exception exception)
                {
                    // Failing to fetch when there is no prior version, which is an accepted case.
                    if ((string)exception.Data["reason"] == "fetchFailed")
                        EditorUtility.DisplayDialog("Data: " + exception.Message, "Failed", "ok");
                }
            }

            return string.Empty;
        }

        void DownloadAssembliesForPreviousVersion()
        {
            Profiler.BeginSample("DownloadAssembliesForPreviousVersion");

            if (Directory.Exists(PreviousVersionBinaryPath))
                Directory.Delete(PreviousVersionBinaryPath, true);

            Directory.CreateDirectory(PreviousVersionBinaryPath);

            ActivityLogger.Log("Retrieving assemblies for previous package version {0}", PreviousPackageInfo.version);
            var packageDataZipFilename = PackageBinaryZipping.PackageDataZipFilename(PreviousPackageInfo.name, PreviousPackageInfo.version);
            var zipPath = Path.Combine(PreviousVersionBinaryPath, packageDataZipFilename);
            var uri = Path.Combine("https://artifactory.prd.it.unity3d.com/artifactory/pkg-api-validation/v2/", packageDataZipFilename);

            var request = new UnityWebRequest(uri);
            request.timeout = 60; // 60 seconds time out
            request.downloadHandler = new DownloadHandlerFile(zipPath);
            var operation = request.SendWebRequest();
            while (!operation.isDone)
                Thread.Sleep(1);

#if UNITY_2020_1_OR_NEWER
            var requestError = request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.DataProcessingError;
#else
            var requestError = request.isHttpError || request.isNetworkError;
#endif
            if (requestError || !PackageBinaryZipping.Unzip(zipPath, PreviousVersionBinaryPath))
            {
                ActivityLogger.Log(String.Format("Could not download binary assemblies for previous package version from {0}. {1}", uri, request.responseCode));
                PreviousPackageBinaryDirectory = null;
            }
            else
                PreviousPackageBinaryDirectory = PreviousVersionBinaryPath;

            ActivityLogger.Log("Done retrieving assemblies for previous package", PreviousPackageInfo.version);
            Profiler.EndSample();
        }

        static ManifestData GetPackageValidationSuiteInfo(PackageInfo[] packageList)
        {
            var vSuitePackageInfo = packageList.SingleOrDefault(p => p.name == Utilities.AssetStoreVSuiteName);

            if (vSuitePackageInfo == null)
            {
                throw new ArgumentException($"The package {Utilities.AssetStoreVSuiteName} could not be found in this project.");
            }

            return new ManifestData()
            {
                version = vSuitePackageInfo.version,
                name = vSuitePackageInfo.name,
                displayName = vSuitePackageInfo.displayName
            };
        }
    }
}
