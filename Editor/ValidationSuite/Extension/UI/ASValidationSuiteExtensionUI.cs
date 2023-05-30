using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

[assembly: InternalsVisibleTo("Unity.asset-store-validation.EditorTests")]
namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.UI
{
    class ASValidationSuiteExtensionUI : VisualElement
    {
        string[] PackageNamePrefixList = { "com.unity.", "com.autodesk.", "com.havok.", "com.ptc." }; //TODO: only 'com.unity' is mentioned in the standard
        const string PackagePath = "Packages/com.unity.asset-store-validation/";
        const string ResourcesPath = PackagePath + "Editor/ValidationSuite/Res/";
        const string TemplatePath = ResourcesPath + "Templates/ValidationSuiteTools.uxml";
        const string DarkStylePath = ResourcesPath + "Styles/Dark.uss";
        const string LightStylePath = ResourcesPath + "Styles/Light.uss";

        VisualElement root;

        PopupField<string> validationPopupField;

        List<string> _validationChoices = ValidationTypeDropdown.ToList();
        public List<string> ValidationChoices
        {
            get => _validationChoices;
            private set => _validationChoices = value;
        }

        PackageInfo CurrentPackageinfo { get; set; }
        string PackageId { get; set; }

        public static ASValidationSuiteExtensionUI CreateUI()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TemplatePath);
            return asset == null ? null : new ASValidationSuiteExtensionUI(asset);
        }

        ASValidationSuiteExtensionUI(VisualTreeAsset asset)
        {
            root = asset.CloneTree();
            string path = EditorGUIUtility.isProSkin ? DarkStylePath : LightStylePath;
            var styleSheet = EditorGUIUtility.Load(path) as StyleSheet;
            root.styleSheets.Add(styleSheet);
            Add(root);

            validationPopupField = new PopupField<string>("", ValidationChoices, 0);
            root.Q<VisualElement>("ASValidationTypeDropdown").Add(validationPopupField);

            ValidateButton.clickable.clicked += Validate;
            ViewResultsButton.clickable.clicked += ViewResults;
            ViewDiffButton.clickable.clicked += ViewDiffs;
        }

        static bool PackageAvailable(PackageInfo packageInfo)
        {
            var installedPackageInfo = PackageInfo.FindForAssetPath($"Packages/{packageInfo.name}");
            return installedPackageInfo != null && installedPackageInfo.version == packageInfo.version;
        }

        static bool SourceSupported(PackageInfo info)
        {
            PackageSource source = info.source;

            // Tarball is available here only, so check if its a tarball and return true
            if (source == PackageSource.LocalTarball) return true;

            return source == PackageSource.Embedded
                   || source == PackageSource.Local
                   || source == PackageSource.Registry
                   || source == PackageSource.Git
                   || (info.source == PackageSource.BuiltIn && info.type != "module");
        }

        public void OnPackageSelectionChange(PackageInfo packageInfo)
        {
            if (root == null)
                return;

            var showValidationUI = packageInfo != null && PackageAvailable(packageInfo) && SourceSupported(packageInfo);
            UIUtils.SetElementDisplay(this, showValidationUI);
            if (!showValidationUI)
                return;

            CurrentPackageinfo = packageInfo;
            PackageId = CurrentPackageinfo.name + "@" + CurrentPackageinfo.version;
            ValidationResults.text = string.Empty;

            AddRemoveUnitySpecificValidations(NamePrefixEligibleForUnityStandardsOptions(CurrentPackageinfo.name));

            validationPopupField.value = NamePrefixEligibleForUnityStandardsOptions(CurrentPackageinfo.name) ? ValidationTypeDropdown.AssetStoreLabelText : ValidationTypeDropdown.StructureLabelText;

            UIUtils.SetElementDisplay(ViewResultsButton, ValidationSuiteReport.ReportExists(PackageId));
            UIUtils.SetElementDisplay(ViewDiffButton, ValidationSuiteReport.DiffsReportExists(PackageId));

            root.style.backgroundColor = Color.gray;
        }

        public void AddRemoveUnitySpecificValidations(bool showUnityStandards)
        {
            ValidationChoices.Clear();

            ValidationChoices.Add(ValidationTypeDropdown.StructureLabelText);
            ValidationChoices.Add(ValidationTypeDropdown.AssetStoreLabelText);
        }

        void Validate()
        {
            if (root == null)
                return;

            if (Utilities.NetworkNotReachable)
            {
                EditorUtility.DisplayDialog("", "Validation suite requires network access and cannot be used offline.", "Ok");
                return;
            }

            var validationType = ValidationTypeDropdown.ValidationTypeFromDropdown(validationPopupField.value, CurrentPackageinfo.source);

            var results = ValidationSuite.ValidatePackage(PackageId, validationType);
            var report = ValidationSuiteReport.GetReport(PackageId);

            UIUtils.SetElementDisplay(ViewResultsButton, ValidationSuiteReport.ReportExists(PackageId));
            UIUtils.SetElementDisplay(ViewDiffButton, ValidationSuiteReport.DiffsReportExists(PackageId));

            if (!results)
            {
                ValidationResults.text = "Failed";
                root.style.backgroundColor = Color.red;
            }
            else if (report != null && report.Tests.Any(t => t.TestOutput.Any(o => o.Type == TestOutputType.Warning)))
            {
                ValidationResults.text = "Warnings";
                root.style.backgroundColor = Color.yellow;
            }
            else
            {
                ValidationResults.text = "Success";
                root.style.backgroundColor = Color.green;
            }
        }

        void ViewResults()
        {
            var filePath = ValidationSuiteReport.GetTextReportPath(PackageId);
            try
            {
                try
                {
                    var targetFile = Directory.GetCurrentDirectory() + "/" + filePath;
                    if (!File.Exists(targetFile))
                        throw new Exception("Validation Result not found!");

                    Process.Start(targetFile);
                }
                catch (Exception)
                {
                    var data = File.ReadAllText(filePath);
                    EditorUtility.DisplayDialog("Validation Results", data, "Ok");
                }
            }
            catch (Exception)
            {
                EditorUtility.DisplayDialog("Validation Results", "Results are missing", "Ok");
            }
        }

        void ViewDiffs()
        {
            if (ValidationSuiteReport.DiffsReportExists(PackageId))
            {
                Application.OpenURL("file://" + Path.GetFullPath(ValidationSuiteReport.DiffsReportPath(PackageId)));
            }
        }

        public bool NamePrefixEligibleForUnityStandardsOptions(string packageName)
        {
            return PackageNamePrefixList
                .Any(packageName.StartsWith);
        }

        internal Label ValidationResults { get { return root.Q<Label>("ASvalidationResults");} }

        internal Button ValidateButton { get { return root.Q<Button>("ASvalidateButton"); } }

        internal Button ViewResultsButton { get { return root.Q<Button>("ASviewResults"); } }

        internal Button ViewDiffButton { get { return root.Q<Button>("ASviewdiff"); } }
    }
}
