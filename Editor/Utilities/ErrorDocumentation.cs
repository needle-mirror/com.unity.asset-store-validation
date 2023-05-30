namespace UnityEditor.PackageManager.AssetStoreValidation
{
    static class ErrorDocumentation
    {
        const string k_DocsUrl = "https://docs.unity3d.com/Packages/com.unity.asset-store-validation@latest/index.html?preview=1&subfolder=/manual";

        /// <summary>
        /// Generate a message with the link to the package documentation in docs.unity3d.com
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileSection"></param>
        /// <returns></returns>
        public static string GetLinkMessage(string filePath, string fileSection)
        {
            return $"Read more about this error and potential solutions at {k_DocsUrl}/{filePath}#{fileSection}";
        }
    }
}
