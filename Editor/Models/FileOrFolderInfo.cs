namespace UnityEditor.PackageManager.AssetStoreValidation.Models
{
    class FileOrFolderInfo
    {
        public string Path { get; set; }
        public string ParentFolderPath { get; set; }
        public string Name { get; set; }
        public int PathDepth { get; set; }
    }
}