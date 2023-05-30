using System;
using System.Collections.Generic;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    interface IValidationTestResult
    {
        // The test associated to this result
        IValidationTest ValidationTest { get; }

        TestState TestState { get; }

        List<ValidationTestOutput> TestOutput { get; }

        DateTime StartTime { get; }

        DateTime EndTime { get; }
    }
}
