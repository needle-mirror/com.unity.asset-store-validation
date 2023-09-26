using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    static class AsvUtilities
    {
        public static string k_ChangelogName = "CHANGELOG.md";
        const string k_PackageJsonFilename = "package.json";

        /// <summary>
        /// Validate if any of the pieces of a path are part of the provided exception path HashSet 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="exceptionPaths"></param>
        /// <returns></returns>
        public static bool IsFileInExceptionPath(string filePath, HashSet<string> exceptionPaths)
        {
            var pathParts = filePath.ToLower().Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            return pathParts.Any(exceptionPaths.Contains);
        }

        /// <summary>
        /// Generates a single string containing the concatenated files values and the provided footer 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="files"></param>
        /// <param name="footer"></param>
        /// <returns></returns>
        public static string BuildErrorMessage(string header, IEnumerable<string> files, string footer = null)
        {
            var builder = new StringBuilder(header + Environment.NewLine);
            foreach (var file in files)
            {
                builder.AppendLine(file);
            }

            if (!string.IsNullOrWhiteSpace(footer))
            {
                builder.AppendLine(footer);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns the status for a given URL to validate if is reachable or not. 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="m_HttpUtils"></param>
        /// <returns>UrlStatus.Ok when reachable, UrlStatus.Unreachable when not, and UrlStatus.None when the provided string is empty</returns>
        public static UrlStatus CheckUrlStatus(string url, IReachable m_HttpUtils)
        {
            if (string.IsNullOrWhiteSpace(url)) return UrlStatus.None;
            return m_HttpUtils.IsURLReachable(url) ? UrlStatus.Ok : UrlStatus.Unreachable;
        }
    }
}