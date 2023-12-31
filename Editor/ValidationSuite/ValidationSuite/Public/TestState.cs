namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    /// <summary>
    /// The various states a test can be in
    /// </summary>
    public enum TestState
    {
        /// <summary>The test succeeded.</summary>
        Succeeded,
        /// <summary>The test failed.</summary>
        Failed,
        /// <summary>The test was not run.</summary>
        NotRun,
        /// <summary>The test is currently running.</summary>
        Running,
        /// <summary>The test is not yet implemented and should be skipped.</summary>
        NotImplementedYet,
        /// <summary>The test finished with warnings.</summary>
        Warning
    }
}
