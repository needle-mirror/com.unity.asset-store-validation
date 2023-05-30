using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Profiling;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    class PackageCIUtils
    {
        internal static List<string> Pack(string path, string destinationPath)
        {
            Profiler.BeginSample("Pack");
            var packRequest = Client.Pack(path, destinationPath);
            while (!packRequest.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (packRequest.Status != StatusCode.Success)
                throw new Exception("Failed to properly pack package.  Error = " + packRequest.Error.message);

            var generatedPackages = new List<string>();
            generatedPackages.Add(packRequest.Result.tarballPath);
            Profiler.EndSample();
            return generatedPackages;
        }
    }
}
