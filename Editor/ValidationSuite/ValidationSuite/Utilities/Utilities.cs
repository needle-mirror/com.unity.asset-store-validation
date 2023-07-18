using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Assertions;
using UnityEditor.Compilation;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
using Assembly = UnityEditor.Compilation.Assembly;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    static class Utilities
    {
        internal const string PackageJsonFilename = "package.json";
        internal const string AssetStoreVSuiteName = "com.unity.asset-store-validation";

        /// <summary>
        /// Validates if the editor is able to reach the internet or not
        /// </summary>
        public static bool NetworkNotReachable { get { return Application.internetReachability == NetworkReachability.NotReachable; } }

        /// <summary>
        /// Generates the string in package id format: name@version
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string CreatePackageId(string name, string version)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(version))
                throw new ArgumentNullException("Both name and version must be specified.");

            return string.Format("{0}@{1}", name, version);
        }

        internal static T GetDataFromJson<T>(string jsonFile)
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(jsonFile));
        }

        /// <summary>
        /// Creates a directory on the given path
        /// </summary>
        /// <param name="path"></param>
        public static void EnsureDirectoryExists(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (IOException) when (Directory.Exists(path))
            {
            }
        }

        internal static string CreatePackage(string path, string workingDirectory)
        {
            //No Need to delete the file, npm pack always overwrite: https://docs.npmjs.com/cli/pack
            var packagePath = Path.Combine(Path.Combine(Application.dataPath, ".."), path);

            var launcher = new NodeLauncher();
            launcher.WorkingDirectory = workingDirectory;
            launcher.NpmPack(packagePath);

            var packageName = launcher.OutputLog.ToString().Trim();
            return packageName;
        }

        internal static PackageInfo[] UpmSearch(string packageIdOrName = null, bool throwOnRequestFailure = false)
        {
            Profiler.BeginSample("UpmSearch");
            var request = string.IsNullOrEmpty(packageIdOrName) ? Client.SearchAll() : Client.Search(packageIdOrName);
            while (!request.IsCompleted)
            {
                if (Utilities.NetworkNotReachable)
                    throw new Exception("Failed to fetch package information: network not reachable");
                Thread.Sleep(100);
            }
            if (throwOnRequestFailure && request.Status == StatusCode.Failure)
                throw new Exception("Failed to fetch package information.  Error details: " + request.Error.errorCode + " " + request.Error.message);
            Profiler.EndSample();
            return request.Result;
        }

        internal static PackageInfo[] UpmListOffline(string packageIdOrName = null)
        {
            Profiler.BeginSample("UpmListOffline");

#if UNITY_2019_2_OR_NEWER
            var request = Client.List(true, true);
#else
            var request = Client.List(true);
#endif

            while (!request.IsCompleted)
                Thread.Sleep(100);
            var result = new List<PackageInfo>();
            foreach (var upmPackage in request.Result)
            {
                if (!string.IsNullOrEmpty(packageIdOrName) && !(upmPackage.name == packageIdOrName || upmPackage.packageId == packageIdOrName))
                    continue;
                result.Add(upmPackage);
            }

            Profiler.EndSample();

            return result.ToArray();
        }

        internal static string DownloadPackage(string packageId, string workingDirectory)
        {
            //No Need to delete the file, npm pack always overwrite: https://docs.npmjs.com/cli/pack
            var launcher = new NodeLauncher();
            launcher.WorkingDirectory = workingDirectory;
            launcher.NpmRegistry = NodeLauncher.ProductionRepositoryUrl;

            try
            {
                launcher.NpmPack(packageId);
            }
            catch (ApplicationException exception)
            {
                exception.Data["code"] = "fetchFailed";
                throw exception;
            }

            var packageName = launcher.OutputLog.ToString().Trim();
            return packageName;
        }

        internal static bool PackageExistsOnProduction(string packageId, string version = null)
        {
            var launcher = new NodeLauncher();
            launcher.NpmRegistry = NodeLauncher.ProductionRepositoryUrl;

            try
            {
                launcher.NpmView(packageId);
            }
            catch (ApplicationException exception)
            {
                if (exception.Message.Contains("npm ERR! code E404") &&
                    exception.Message.Contains("is not in the npm registry."))
                    return false;
                exception.Data["code"] = "fetchFailed";
                throw;
            }

            var packageData = launcher.OutputLog.ToString().Trim();

            return !string.IsNullOrEmpty(packageData) && (version == null || packageData.Contains(version));
        }

        /// <summary>
        /// Extract a package to the target directory
        /// </summary>
        /// <param name="fullPackagePath"></param>
        /// <param name="workingPath"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="packageName"></param>
        /// <param name="deleteOutputDir"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ApplicationException"></exception>
        public static string ExtractPackage(string fullPackagePath, string workingPath, string outputDirectory, string packageName, bool deleteOutputDir = true)
        {
            Profiler.BeginSample("ExtractPackage");

            //verify if package exists
            if (!fullPackagePath.EndsWith(".tgz"))
                throw new ArgumentException("Package should be a .tgz file");

            if (!File.Exists(fullPackagePath))
                throw new FileNotFoundException(fullPackagePath + " was not found.");

            if (deleteOutputDir)
            {
                try
                {
                    if (Directory.Exists(outputDirectory))
                        Directory.Delete(outputDirectory, true);

                    Directory.CreateDirectory(outputDirectory);
                }
                catch (IOException e)
                {
                    if (e.Message.ToLowerInvariant().Contains("1921"))
                        throw new ApplicationException("Failed to remove previous module in " + outputDirectory + ". Directory might be in use.");

                    throw;
                }
            }

            var tarPath = fullPackagePath.Replace(".tgz", ".tar");
            if (File.Exists(tarPath))
            {
                File.Delete(tarPath);
            }

            //Unpack the tgz into temp. This should leave us a .tar file
            PackageBinaryZipping.Unzip(fullPackagePath, workingPath);

            //See if the tar exists and unzip that
            var tgzFileName = Path.GetFileName(fullPackagePath);
            var targetTarPath = Path.Combine(workingPath, packageName + "-tar");
            if (Directory.Exists(targetTarPath))
            {
                Directory.Delete(targetTarPath, true);
            }

            if (File.Exists(tarPath))
            {
                PackageBinaryZipping.Unzip(tarPath, targetTarPath);
            }

            //Move the contents of the tar file into outputDirectory
            var packageFolderPath = Path.Combine(targetTarPath, "package");
            if (Directory.Exists(packageFolderPath))
            {
                //Move directories and meta files
                foreach (var dir in Directory.GetDirectories(packageFolderPath))
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName != null)
                    {
                        Directory.Move(dir, Path.Combine(outputDirectory, dirName));
                    }
                }

                foreach (var file in Directory.GetFiles(packageFolderPath))
                {
                    if ((file.Contains("package.json") && !fullPackagePath.Contains(packageName + ".tests") && !fullPackagePath.Contains(packageName + ".samples")) ||
                        !file.Contains("package.json"))
                    {
                        File.Move(file, Path.Combine(outputDirectory, Path.GetFileName(file)));
                    }
                }
            }

            //Remove the .tgz and .tar artifacts from temp
            var cleanupPaths = new List<string>();
            cleanupPaths.Add(fullPackagePath);
            cleanupPaths.Add(tarPath);
            cleanupPaths.Add(targetTarPath);

            foreach (var p in cleanupPaths)
            {
                try
                {
                    var attr = File.GetAttributes(p);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        // This is a directory
                        Directory.Delete(targetTarPath, true);
                        continue;
                    }

                    File.Delete(p);
                }
                catch (DirectoryNotFoundException)
                {
                    //Pass since there is nothing to delete
                }
            }

            Profiler.EndSample();
            return outputDirectory;
        }

        public static string GetPathFromRoot(string filePath, string root)
        {
            return filePath.Remove(0, root.Length);
        }

        static bool IsTestAssembly(Assembly assembly)
        {
            // see https://unity.slack.com/archives/C26EP4SUQ/p1555485851157200?thread_ts=1555441110.131100&cid=C26EP4SUQ for details about how this is verified
            if (assembly.allReferences.Contains("TestAssemblies"))
            {
                return true;
            }

            // Marking an assembly with UNITY_INCLUDE_TESTS means:
            // Include this assembly in the Unity project only if that package is in a testable state.
            // Otherwise, the assembly is ignored
            //
            // for now, we must read the test assembly file directly
            // because the defineConstraints field is not available on the assembly object
            var assemblyInfo = Utilities.AssemblyInfoFromAssembly(assembly);
            var assemblyDefinition = Utilities.GetDataFromJson<AssemblyDefinition>(assemblyInfo.asmdefPath);
            return assemblyDefinition.defineConstraints.Contains("UNITY_INCLUDE_TESTS");
        }

        /// <summary>
        /// Returns the Assembly instances which contain one or more scripts in a package, given the list of files in the package.
        /// </summary>
        public static IEnumerable<Assembly> AssembliesForPackage(string packageRootPath)
        {
            var filesInPackage = Directory.GetFiles(packageRootPath, "*", SearchOption.AllDirectories);
            filesInPackage = filesInPackage.Select(p => p.Replace('\\', '/')).ToArray();

            var projectAssemblies = CompilationPipeline.GetAssemblies();
            var assemblyHash = new HashSet<Assembly>();

            foreach (var path in filesInPackage)
            {
                if (!string.Equals(Path.GetExtension(path), ".cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                var assembly = GetAssemblyFromScriptPath(projectAssemblies, path);
                if (assembly != null && !Utilities.IsTestAssembly(assembly))
                {
                    assemblyHash.Add(assembly);
                }
            }

            return assemblyHash;
        }

        static Assembly GetAssemblyFromScriptPath(Assembly[] assemblies, string scriptPath)
        {
            var fullScriptPath = Path.GetFullPath(scriptPath);

            foreach (var assembly in assemblies)
            {
                foreach (var packageSourceFile in assembly.sourceFiles)
                {
                    var fullSourceFilePath = Path.GetFullPath(packageSourceFile);

                    if (fullSourceFilePath == fullScriptPath)
                    {
                        return assembly;
                    }
                }
            }

            return null;
        }

        // Return all types from an assembly that can be loaded
        public static IEnumerable<Type> GetTypesSafe(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        internal static AssemblyInfo AssemblyInfoFromAssembly(Assembly assembly)
        {
            var path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
            if (string.IsNullOrEmpty(path))
                return null;

            return new AssemblyInfo(assembly, path);
        }

        internal static void RecursiveDirectorySearch(string path, string searchPattern, ref List<string> matches)
        {
            if (!Directory.Exists(path))
                return;

            var files = Directory.GetFiles(path, searchPattern);
            if (files.Any())
                matches.AddRange(files);

            foreach (var subDir in Directory.GetDirectories(path)) RecursiveDirectorySearch(subDir, searchPattern, ref matches);
        }

        // System.IO.FileExists will return false on ArgumentException/IOException/UnauthorizedAccessException exceptions
        // which can be very misleading since it hides the underlying error and pretends the file doesn't exist.
        // This alternative method will not catch these exceptions in order to surface the underlying issue.
        internal static bool FileExists(string path)
        {
            try
            {
                new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).Close();
                return true; // file exists
            }
            catch (IOException e) when (e is FileNotFoundException || e is DirectoryNotFoundException || e is DriveNotFoundException)
            {
                return false; // it does not exist
            }
            // other errors bubble up, e.g. PathTooLongException.
        }

        internal enum DirectoryItemType
        {
            File,
            Directory
        }
        internal struct DirectoryItem
        {
            internal string Path;
            internal DirectoryItemType Type;
            internal int Depth;
            internal int ChildCount;
        }

        internal static IEnumerable<DirectoryItem> GetDirectoryAndFilesIn(string path)
        {
            var result = new List<DirectoryItem>();
            RecursiveDirectoryListing(path, path, 1, result);
            return result;
        }

        static void RecursiveDirectoryListing(string rootPath, string path, int currentDepth, List<DirectoryItem> items)
        {
            var assets = Directory.GetFiles(path);
            foreach (var asset in assets)
            {
                var relativePath = GetRelativePath(asset, rootPath);

                items.Add(new DirectoryItem()
                {
                    Path = relativePath,
                    Type = DirectoryItemType.File,
                    Depth = currentDepth,
                    ChildCount = 0,
                });
            }

            //No need to check the root folder itself
            if (path != rootPath)
            {
                var relativePath = GetRelativePath(path, rootPath);
                items.Add(new DirectoryItem()
                {
                    Path = relativePath,
                    Type = DirectoryItemType.Directory,
                    Depth = currentDepth,
                    ChildCount = assets.Length
                });
            }

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                RecursiveDirectoryListing(rootPath, directory, currentDepth + 1, items);
            }
        }

        static string GetRelativePath(string path, string directory)
        {
            Assert.IsNotNull(path);
            Assert.IsNotNull(directory);

            if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString()) && !Path.HasExtension(directory))
            {
                directory += Path.DirectorySeparatorChar;
            }

            try
            {
                directory = Path.GetFullPath(directory);
                path = Path.GetFullPath(path);

                var folderUri = new Uri(directory);
                var pathUri = new Uri(path);

                return Uri.UnescapeDataString
                    (
                        folderUri.MakeRelativeUri(pathUri).ToString()
                            .Replace('/', Path.DirectorySeparatorChar)
                    );
            }
            catch (UriFormatException uriFormatException)
            {
                throw new UriFormatException($"Failed to get relative path.\nPath: {path}\nDirectory:{directory}\n{uriFormatException}");
            }
        }

        internal static void HandleWarnings(List<string> faultyPaths, string warningText, string informationText, Action<string> warnFunc, Action<string> infoFunc)
        {
            if (faultyPaths.Count > 0)
            {
                warnFunc($"{warningText} ");
                PrintInformationFor(faultyPaths, informationText, infoFunc);
            }
        }

        static void PrintInformationFor(List<string> faultyPaths, string warningText, Action<string> infoFunc) => faultyPaths.ForEach(s => infoFunc(warningText + s));
    }
}
