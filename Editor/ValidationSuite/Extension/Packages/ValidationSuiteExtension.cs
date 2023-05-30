using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.PackageManager.UI;
using UnityEngine.UIElements;
#endif

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.UI
{
#if !UNITY_PACKAGE_MANAGER_DEVELOP_EXISTS
#if UNITY_2019_1_OR_NEWER

    [InitializeOnLoad]
    internal class ValidationSuiteExtension : IPackageManagerExtension
    {
        private PackageInfo packageInfo;
        private ASValidationSuiteExtensionUI ui;

        public ValidationSuiteExtension()
        {
        }

        public VisualElement CreateExtensionUI()
        {
            return ui ?? (ui = ASValidationSuiteExtensionUI.CreateUI()) ?? new VisualElement();
        }

        public void OnPackageSelectionChange(PackageInfo packageInfo)
        {
            if (packageInfo == this.packageInfo)
                return;

            if (ui == null)
                return;

            this.packageInfo = packageInfo;
            ui.OnPackageSelectionChange(this.packageInfo);
        }

        public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
        {
        }

        public void OnPackageRemoved(PackageInfo packageInfo)
        {
        }

        static ValidationSuiteExtension()
        {
            PackageManagerExtensions.RegisterExtension(new ValidationSuiteExtension());
        }
    }
#endif
#endif
}
