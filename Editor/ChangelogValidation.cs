using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.Semver;
using UnityEditor.PackageManager.AssetStoreValidation.Models;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class ChangelogValidation : BaseValidation
    {
        static readonly string k_DocsFilePath = "changelog_validation.html";
        const string k_changelogUrlField = "changelogUrl";

        const string k_ValidEntryExample = "## [x.y.z] - YYYY-MM-DD";
        const string k_EntryLineRegex = @"## \[(?<version>.*)]( - (?<date>.*))?";
        const string k_HeaderLineRegex = @"### (?<header>.*)";
        const string k_DateFormat = "yyyy-MM-dd";
        static string[] k_DateWarningFormats = { "yyyy-MM-d", "yyyy-M-dd", "yyyy-M-d" };
        bool k_PackageVersionFoundInChangelog;
        string k_ChangelogPath;
        
        static readonly List<string> m_Headers = new List<string> { "Added", "Changed", "Deprecated", "Removed", "Fixed", "Security" };
        static readonly string k_ValidHeadersAsString = $"[{string.Join(", ", m_Headers)}]";

        internal static readonly string k_ExcessWhitespaceInHeaderWarning = $"There is an excess amount of whitespace after the '###' header in the changelog of this package. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "empty-header-or-whitespace")}";
        internal static readonly string k_NoValidEntriesFoundError = $"No valid changelog entries were found. The changelog needs to follow the https://keepachangelog.org specifications (`{k_ValidEntryExample}`)";
        internal static readonly string k_ChangelogContainsUnreleasedEntryError = $"Unreleased sections are not permitted in Asset Store package changelogs. Please remove this section or update it with a new version. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "changelog-contains-unreleased-entry")}";

        private IUnityWebRequestHandler m_UnityWebRequestHandler;

        internal static string NoChangelogError(string changelogPath) => $"Cannot find changelogUrl property in package.json or a {AsvUtilities.k_ChangelogName} at '{changelogPath}'. Please create a '{AsvUtilities.k_ChangelogName}' file and '{AsvUtilities.k_ChangelogName}.meta' or provide a link to an online changelog 'changelogUrl' in package.json. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "no-changelog-found")}";
        internal static string CapitalizationError(string actualChangelogFilename) => $"The changelog file needs to be properly capitalized. Please rename {actualChangelogFilename} to {AsvUtilities.k_ChangelogName}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "changelog-invalid-capitalization")}";
        internal static string InvalidExtensionError(string actualChangelogFilename) => $"The changelog file {actualChangelogFilename} must be named {AsvUtilities.k_ChangelogName} with the proper file extension. To resolve this error, ensure the changelog file is named {AsvUtilities.k_ChangelogName}, then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "invalid-or-missing-file-extension")}";
        internal static string EmptyHeaderOrEntryError(int lineNumber) => $"Empty header or entry found in changelog on line {lineNumber}. Please remove all empty sections or fill them out properly. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "empty-header-or-whitespace")}";
        internal static string UnexpectedHeaderError(string entry, string unexpectedHeader) => $"Changelog entry '{entry}' contains an unexpected header '{unexpectedHeader}'. It is recommended to use the headers listed on https://www.keepachangelog.com {k_ValidHeadersAsString}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "unexpected-header-entry")}";
        internal static string IncorrectHeaderOrderError(string entry) => $"The headers for changelog entry {entry} are not in the correct order. Please arrange the applicable headers in the following order: {k_ValidHeadersAsString}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "changelog-header-order-is-incorrect")}";
        internal static string RepeatedHeaderError(string entry) => $"Changelog entry '{entry}' contains a duplicated header. Please delete the duplicated header, then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "repeated-headers-in-changelog-entry")}";
        internal static string InvalidEntryVersionFormatError(string version, string changelogPath) => $"Version format '{version}' is not valid in '{changelogPath}'. Please correct the version format to follow the https://keepachangelog.org specifications (`{k_ValidEntryExample}`).";
        internal static string ChangelogEntryMissingError(string version, string path) => $"The date field is missing for entry version {version} in {path}. Please add a date in ISO 8601 format 'YYYY-MM-DD', then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "changelog-entry-is-missing-date")}";
        internal static string DeprecatedDateWarning(string entry, string date) => $"Changelog entry '{entry}' contains a deprecated date '{date}'. Expecting format '{k_DateFormat}'. Update the date to one of the supported values. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "changelog-entry-date-format-is-deprecated")}";
        internal static string InvalidDateError(string entry, string date) => $"Changelog entry '{entry}' contains an invalid date '{date}'. Expecting format '{k_DateFormat}'. Update the date to one of the supported values. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "changelog-entry-date-format-is-invalid")}";
        internal static string PackageVersionNotInChangelogError(string version, string path) => $"No changelog entry for version `{version}` was found in '{path}'. Please add or fix a section so you have a `## [{version}] - '{k_DateFormat}'` section. { ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-version-is-not-in-changelog")}";
        internal static string CurrentVersionNotFirstEntryInChangelogError(string changelogPath, int foundIndex) => $"Found changelog entry but it was not the first entry in '{changelogPath}' (it was entry #{foundIndex}). Please rearrange your changelog with the most recent section at the top. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-version-is-not-first-entry-in-changelog")}";
        internal static string UnreachableUrlMessage(string url) => $"The URL \"{url}\", provided in the \"{k_changelogUrlField}\" field in the package.json, is not reachable. To avoid broken links, please validate that the URL is correct. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "changelog-url-not-reachable")}";
        internal static string UrlNotTestedMessage(string url) => $"The URL \"{url}\", provided in the \"{k_changelogUrlField}\" field in the package.json, has not been tested. Please validate manually that the URL is accurate and reachable. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "changelog-url-not-tested")}";

        public ChangelogValidation(IUnityWebRequestHandler unityWebRequestHandler)
        {
            TestName = "Changelog";
            TestDescription = "Validates that the package contains a valid CHANGELOG.md or links to a changelog online.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.AssetStore, ValidationType.InternalTesting };
            m_UnityWebRequestHandler = unityWebRequestHandler;
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;
            var manifestData = Context.ProjectPackageInfo;
            var packagePath = manifestData.path;

            CheckChangelog(packagePath, manifestData);
        }

        void CheckChangelog(string packagePath, ManifestData manifestData)
        {
            // Check the embedded changelog first to assess if we should show an error later when there's no changelog url
            var offlineChangelogName = TryGetOfflineChangelogName(packagePath);
            var hasOfflineChangelog = !string.IsNullOrWhiteSpace(offlineChangelogName);
            if (hasOfflineChangelog)
            {   
                if (Path.GetExtension(offlineChangelogName) != Path.GetExtension(AsvUtilities.k_ChangelogName))
                    AddError(InvalidExtensionError(offlineChangelogName));
                else if (offlineChangelogName != (AsvUtilities.k_ChangelogName))
                    AddError(CapitalizationError(offlineChangelogName));

                k_ChangelogPath = Path.Combine(packagePath, offlineChangelogName);
                ScanChangelog(k_ChangelogPath);
            }
            
            // For internal testing there should not be any network calls
            if (Context.ValidationType == ValidationType.InternalTesting)
            {
                if (!string.IsNullOrWhiteSpace(manifestData.changelogUrl)) // Only write a warning if there's a url to check
                    AddWarning(UrlNotTestedMessage(manifestData.changelogUrl));
                
                return;
            }
            
            // Check the changelog url status
            var changelogUrlStatus = m_UnityWebRequestHandler.IsUrlReachable(manifestData.changelogUrl);

            if (changelogUrlStatus == UrlStatus.Unreachable)
                AddWarning(UnreachableUrlMessage(manifestData.changelogUrl));
            else if (changelogUrlStatus == UrlStatus.None && !hasOfflineChangelog)
                AddError(NoChangelogError(Path.Combine(packagePath, AsvUtilities.k_ChangelogName)));
        }

        string TryGetOfflineChangelogName(string path)
        {
            var changelogReg = new Regex("changelog[.].*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var changelogFileName = Path.GetFileName(Directory.GetFiles(path, "*.*").FirstOrDefault(x => changelogReg.IsMatch(x) || string.Equals(Path.GetFileName(x), "CHANGELOG", StringComparison.OrdinalIgnoreCase)));

            return changelogFileName;
        }

        void ScanChangelog(string changelogPath)
        {
            ChangelogEntry changelogEntry = null;
            var lineNum = 0;
            var numEntries = 0;
            
            try
            {
                using (var streamReader = new StreamReader(changelogPath))
                {
                    string rawLine;
                    while ((rawLine = streamReader.ReadLine()) != null)
                    {
                        // Cleanup the whole line to avoid having extra whitespaces when matching regex.
                        // Without this, test: When_Headers_Are_Empty_Or_WhiteSpace_Fail was failing because it detected '###          ' as a valid header 
                        var line = rawLine.Trim();
                        lineNum++;

                        // Avoid doing unnecessary work on lines that aren't entries or headers
                        if (!IsEntryOrHeader(line))
                            continue;

                        if (IsEmptyHeaderOrEntry(line))
                            AddError(EmptyHeaderOrEntryError(lineNum));

                        var entry = Regex.Match(line, k_EntryLineRegex);
                        if (entry.Success)
                        {
                            changelogEntry = new ChangelogEntry(entry, ++numEntries);
                            CheckEntry(changelogEntry);
                        }

                        var header = Regex.Match(line, k_HeaderLineRegex);
                        if (!header.Success) continue;

                        CheckHeadersInEntry(changelogEntry, header);
                    }

                    if (!k_PackageVersionFoundInChangelog)
                        AddError(PackageVersionNotInChangelogError(Context.ProjectPackageInfo.version, changelogPath));

                    if (numEntries == 0)
                        AddError(k_NoValidEntriesFoundError);
                }
            }
            catch (IOException e)
            {
                AddError($"Error while parsing file at ${changelogPath}: {e.Message}");
            }
            catch (OutOfMemoryException e)
            {
                AddError($"The file located at {changelogPath} cause an OutOfMemoryException: {e.Message}");
            }
        }

        void CheckEntry(ChangelogEntry entry)
        {
            var versionInEntry = entry.Version;
            var dateInEntry = entry.Date;

            // Unreleased entries are not allowed in published Asset Store packages
            if (versionInEntry == "Unreleased")
            {
                AddError(k_ChangelogContainsUnreleasedEntryError);
                return;
            }

            if (!SemVersion.TryParse(versionInEntry, out _, true))
            {
                AddError(InvalidEntryVersionFormatError(versionInEntry, k_ChangelogPath));
                return;
            }

            if (!k_PackageVersionFoundInChangelog && versionInEntry.Equals(Context.ProjectPackageInfo.version))
            {
                k_PackageVersionFoundInChangelog = true;

                if (entry.k_Index != 1)
                    AddError(CurrentVersionNotFirstEntryInChangelogError(k_ChangelogPath, entry.k_Index));
            }

            if (IsDateValid(dateInEntry))
                return;

            if (IsDateDeprecated(dateInEntry))
            {
                AddWarning(DeprecatedDateWarning(versionInEntry, dateInEntry));
                return;
            }

            if (string.IsNullOrWhiteSpace(dateInEntry))
            {
                AddError(ChangelogEntryMissingError(versionInEntry, k_ChangelogPath));
                return;
            }

            AddError(InvalidDateError(versionInEntry, dateInEntry));
        }

        void CheckHeadersInEntry(ChangelogEntry entry, Match newHeader)
        {
            var header = newHeader.Groups["header"]?.ToString().TrimEnd();

            // Too much whitespace after '##' or '###' triggers a warning. ex: "##   Added"
            if (header?.TakeWhile(char.IsWhiteSpace).Count() > 1)
            {
                AddWarning(k_ExcessWhitespaceInHeaderWarning);
                return;
            }

            if (!m_Headers.Contains(header))
            {
                AddWarning(UnexpectedHeaderError(entry?.ToString(), newHeader.ToString()));
                return;
            }

            entry.AddHeader(m_Headers.IndexOf(header));

            CheckEntryHeaderOrder(entry);
            CheckEntryHeadersDistinct(entry);
        }

        void CheckEntryHeaderOrder(ChangelogEntry entry)
        {
            var sortedEntryIndices = new List<int>(entry.m_Headers);
            sortedEntryIndices.Sort();

            if (!entry.m_Headers.SequenceEqual(sortedEntryIndices))
                AddError(IncorrectHeaderOrderError(entry.ToString()));
        }

        void CheckEntryHeadersDistinct(ChangelogEntry entry)
        {
            if (entry.m_Headers.Count() != entry.m_Headers.Distinct().Count())
                AddError(RepeatedHeaderError(entry.ToString()));
        }

        bool IsDateValid(string date) => DateTime.TryParseExact(date, k_DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        bool IsDateDeprecated(string date) => DateTime.TryParseExact(date, k_DateWarningFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        bool IsEmptyHeaderOrEntry(string line) => line.Trim().Equals("##") || line.Trim().Equals("###");
        bool IsEntryOrHeader(string line) => line.Trim().StartsWith("##");
    }
}
