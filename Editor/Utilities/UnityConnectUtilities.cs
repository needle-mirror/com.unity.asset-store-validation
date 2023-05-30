using System;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    static class UnityConnectUtilities
    {
        internal static string s_CloudEnvironment;

        internal const string k_ProductionEnvironment = "production";
        internal const string k_StagingEnvironment = "staging";
        internal const string k_CloudEnvironmentArg = "-cloudEnvironment";

        static UnityConnectUtilities()
        {
            SetEnvironment(Environment.GetCommandLineArgs());
        }

        internal static void SetEnvironment(string[] args)
        {
            var envPos = Array.IndexOf(args, k_CloudEnvironmentArg);
            try
            {
                s_CloudEnvironment = envPos == -1 ? k_ProductionEnvironment : args[envPos + 1];
            }
            catch (Exception)
            {
                // We want to make sure our cloud environment has a default value regardless of what happens.
                s_CloudEnvironment = k_ProductionEnvironment;
            }
        }

        /// <summary>
        /// Return true if the environment the editor is running on is Staging
        /// </summary>
        /// <returns></returns>
        public static bool IsStagingEnvironment()
        {
            return string.Equals(s_CloudEnvironment, k_StagingEnvironment,
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Get all the current packages in the project
        /// </summary>
        /// <returns></returns>
        public static PackageInfo[] GetAllRegisteredPackages()
        {
            return PackageInfo.GetAllRegisteredPackages();
        }
    }
}