using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    abstract class BaseFileExtensionValidation : BaseValidation
    {
        protected static List<string> GetPathsToFilesWithExtension(string pathToFolder, string[] extensions)
        {
            return Directory.EnumerateFiles(pathToFolder, "*.*", SearchOption.AllDirectories).Where(s => extensions.Any(ext => s.EndsWith(ext,StringComparison.OrdinalIgnoreCase))).ToList();
        }
    }
}