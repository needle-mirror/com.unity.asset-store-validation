using System;
using System.Linq;
using System.Text.RegularExpressions;
using Editor.ValidationSuite.ValidationSuite.ValidationTests;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class AuthorFieldValidation : BaseHttpValidation
    {
        internal const int k_AuthorNameMaxLengthLimit = 512;
        internal const int k_EmailMaxLengthLimit = 320;
        internal const int k_UrlMaxLengthLimit = 2048;
        const int k_UrlCheckTimeoutSeconds = 4;

        static readonly string s_DocsFilePath = "author_field_validation.html";

        static readonly string[] s_UnityAuthorNames = new[] {"Unity",
                                                             "Unity Technologies",
                                                             "Unity Technologies ApS",
                                                             "Unity Technologies SF",
                                                             "Unity Technologies Inc\\."
                                                            };

        readonly Regex m_AuthorNameRegexCheck = new Regex(string.Join("|", s_UnityAuthorNames.Select(x => "^" + x + "$")),
                                                         RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        readonly Regex m_EmailRegexCheck = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9]{2,}(?:[a-z0-9-]*[a-z0-9])?)\Z",
                                                        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        readonly Regex m_UrlRegexCheck = new Regex(@"^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?([\?]?[^{}|\^~[\]` ])*$",
                                                        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        internal static readonly string k_MandatoryAuthorFieldErrorMessage = $"The `author` field is mandatory and cannot be empty. Please add `author` field in your package.json file. " +
                                                               $"The `author` field in your package.json file can be a string or an object ( name, email, URL )." +
                                                               $" {ErrorDocumentation.GetLinkMessage(s_DocsFilePath, "author-is-mandatory")}";

        internal static readonly string k_StringAuthorFieldExceedMaxCharacterLengthErrorMessage = $"Invalid `author` field. The `author` field in your package.json file must fit in {k_AuthorNameMaxLengthLimit} characters." +
                                                                                    $" {ErrorDocumentation.GetLinkMessage(s_DocsFilePath, "author-name-is-too-long")}";

        internal static readonly string k_AuthorNameIsUnityAuthorNameErrorMessage = $"The `author` field cannot be any of the following: [{string.Join(",", s_UnityAuthorNames)}]." +
                                                                      $"Please replace the `author` name in your package.json file. {ErrorDocumentation.GetLinkMessage(s_DocsFilePath, "author-is-unity-author-name")}";

        internal static readonly string k_AuthorMailFieldExceedMaxCharacterLengthErrorMessage = $"Invalid `email` field. The `email` field in your package.json file must fit in {k_EmailMaxLengthLimit} characters." +
                                                                                  $" {ErrorDocumentation.GetLinkMessage(s_DocsFilePath, "email-is-too-long")}";

        internal static readonly string k_AuthorMailFieldInvalidFormatErrorMessage = $"Invalid `email` field. The `email` field in your package.json file is optional, if set should meets RFC 2822 Format." +
                                                                       $" {ErrorDocumentation.GetLinkMessage(s_DocsFilePath, "email-has-invalid-format")}";

        internal static readonly string k_AuthorUrlFieldExceedMaxCharacterLengthErrorMessage = $"Invalid `URL` field. The `URL` field in the author field of your package.json file must fit in {k_UrlMaxLengthLimit} characters." +
                                                                                  $" {ErrorDocumentation.GetLinkMessage(s_DocsFilePath, "url-is-too-long")}";

        internal static readonly string k_AuthorUrlFieldInvalidFormatErrorMessage = $"Invalid `URL` field. The `URL` field in the author field of your package.json file is optional. If set, only HTTPS and HTTP are allowed protocols." +
                                                                                  $" {ErrorDocumentation.GetLinkMessage(s_DocsFilePath, "url-has-invalid-format")}";

        internal static readonly string k_AuthorUrlFieldIsUnreachableWarningMessage = $"Unreachable `URL`. The `URL` field in the author field of your package.json is unreachable." +
                                                                                  $" {ErrorDocumentation.GetLinkMessage(s_DocsFilePath, "url-is-unreachable")}";
        internal static string AuthorUrlFieldNotTestedMessage(string url) => $"Untested `URL`. The URL \"{url}\" provided in the `URL` property in the author field of the package.json has not been tested. Please manually validate that the url is reachable" +
                                                                             $" {ErrorDocumentation.GetLinkMessage(s_DocsFilePath, "url-not-tested")}";

        public AuthorFieldValidation()
        {
            TestName = "Manifest: Author Field";
            TestDescription = "Validates that the package author field is valid.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new ValidationType[] { ValidationType.Structure, ValidationType.AssetStore, ValidationType.InternalTesting };
        }

        protected override void Run()
        {
            base.Run();
            // Start by declaring victory
            TestState = TestState.Succeeded;
            Validate(Context.ProjectPackageInfo);
        }

        void Validate(ManifestData manifestData)
        {
            //structure validation pass even when the author is not set
            if (Context.ValidationType == ValidationType.Structure
                && String.IsNullOrEmpty(manifestData.author)
                && manifestData.authorDetails == null) return;

            if (manifestData.authorDetails == null)
                ValidateStringAuthorField(manifestData.author);
            else
                ValidateAuthorDetailField(manifestData.authorDetails);
        }

        void ValidateAuthorDetailField(AuthorDetails authorDetails)
        {
            ValidateStringAuthorField(authorDetails.name);
            ValidateAuthorDetailMail(authorDetails.email);
            ValidateAuthorDetailUrl(authorDetails.url);
        }

        void ValidateStringAuthorField(string authorName)
        {
            // no author set neither string nor object
            if (String.IsNullOrWhiteSpace(authorName))
            {
                AddError(k_MandatoryAuthorFieldErrorMessage);
            }
            else
            {
                //author name exceed the author character max length limit
                if (authorName.Length > k_AuthorNameMaxLengthLimit)
                    AddError(k_StringAuthorFieldExceedMaxCharacterLengthErrorMessage);
                else if (IsUnityAuthorNames(authorName))
                    AddError(k_AuthorNameIsUnityAuthorNameErrorMessage);
            }
        }

        void ValidateAuthorDetailMail(string email)
        {
            //email is empty
            if (string.IsNullOrEmpty(email)) return;

            //email exceed the email character max length limit
            if (email.Length > k_EmailMaxLengthLimit)
                AddError(k_AuthorMailFieldExceedMaxCharacterLengthErrorMessage);

            //the email should be valid
            if (!IsValidEmail(email))
                AddError(k_AuthorMailFieldInvalidFormatErrorMessage);

        }

        void ValidateAuthorDetailUrl(string url)
        {
            //empty URL
            if (string.IsNullOrEmpty(url)) return;

            //URL exceed the URL character max length limit
            if (url.Length >= k_UrlMaxLengthLimit)
                AddError(k_AuthorUrlFieldExceedMaxCharacterLengthErrorMessage);

            //the URL should be valid (HTTP | HTTPS)
            if (!IsValidUrl(url))
                AddError(k_AuthorUrlFieldInvalidFormatErrorMessage);

            // For internal testing there should not be any network calls
            if (Context.ValidationType == ValidationType.InternalTesting)
            {
                AddWarning(AuthorUrlFieldNotTestedMessage(url));
            }
            else
            {
                //warning in case of valid but unreachable URL, 
                if (CheckUrlStatus(url) == UrlStatus.Unreachable)
                    AddWarning(k_AuthorUrlFieldIsUnreachableWarningMessage);
            }
        }

        bool IsUnityAuthorNames(string authorName) => !string.IsNullOrWhiteSpace(authorName) && m_AuthorNameRegexCheck.IsMatch(authorName);

        /// <summary>
        /// This method return true when Email address meets RFC 2822 Format.
        /// The format of an email address is local-part@domain, where the local 
        /// part may be up to 64 octets long and the domain may have a maximum of 255 octets. 
        /// The formal definitions are in RFC 5322 (sections 3.2. 3 and 3.4. 1) and RFC 5321
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        bool IsValidEmail(string email) => m_EmailRegexCheck.IsMatch(email);

        bool IsValidUrl(string url) => !string.IsNullOrWhiteSpace(url) && m_UrlRegexCheck.IsMatch(url);
    }
}
