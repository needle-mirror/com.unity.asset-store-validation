using System.IO;
using UnityEditor.Compilation;
using UnityEngine;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests
{
    class AssemblyInfo
    {
        public readonly Assembly assembly;
        public readonly string asmdefPath;

        AssemblyDefinition cachedAssemblyDefinition;

        public AssemblyInfo(Assembly assembly, string asmdefPath)
        {
            this.assembly = assembly;
            this.asmdefPath = Path.GetFullPath(asmdefPath);
        }
    }
}
