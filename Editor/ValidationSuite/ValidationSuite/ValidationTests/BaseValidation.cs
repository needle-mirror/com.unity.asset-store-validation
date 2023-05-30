using System;
using System.Collections.Generic;
using System.Configuration;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests
{
    abstract class BaseValidation : IValidationTest, IValidationTestResult
    {
        Type[] dependsOn;

        public ValidationType[] SupportedValidations { get; set; }

        public PackageType[] SupportedPackageTypes { get; set; }

        public ValidationSuite Suite { get; set; }

        public string TestName { get; protected set; }

        public string TestDescription { get; protected set; }

        // Category mostly used for sorting tests, or grouping in UI.
        public TestCategory TestCategory { get; protected set; }

        public IValidationTest ValidationTest { get { return this; } }

        public TestState TestState { get; set; }

        public List<ValidationTestOutput> TestOutput { get; set; }

        List<VettingReportEntry> VettingEntries { get; set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public VettingContext Context { get; set; }

        public bool ShouldRun { get; set; }

        /// <summary>
        /// This property represents Validation types that must be run before this Validation.
        /// </summary>
        public Type[] DependsOn
        {
            get => dependsOn;
            set
            {
                foreach (var type in value)
                    if (this.GetType() == type || GetBaseType(type) != typeof(BaseValidation))
                        throw new SettingsPropertyWrongTypeException($"Only types derived from {nameof(BaseValidation)} and different from the current type are valid for this property");
                dependsOn = value;
            }
        }

        static Type GetBaseType(Type type)
        {
            var baseType = type;
            while (baseType.BaseType != null && baseType.BaseType != typeof(object))

                baseType = baseType.BaseType;

            return baseType;
        }
        
        protected BaseValidation()
        {
            TestState = TestState.NotRun;
            TestOutput = new List<ValidationTestOutput>();
            ShouldRun = true;
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            SupportedValidations = new[] { ValidationType.AssetStore, ValidationType.CI, ValidationType.LocalDevelopment, ValidationType.LocalDevelopmentInternal, ValidationType.Promotion, ValidationType.VerifiedSet };
            SupportedPackageTypes = new[] { PackageType.Tooling, PackageType.Template };
        }

        // This method is called synchronously during initialization,
        // and allows a test to interact with APIs, which need to run from the main thread.
        public virtual void Setup()
        {
        }

        public void RunTest()
        {
            ActivityLogger.Log("Starting validation test \"{0}\"", TestName);
            StartTime = DateTime.Now;
            
            Run();

            EndTime = DateTime.Now;
            var elapsedTime = EndTime - StartTime;
            ActivityLogger.Log("Finished validation test \"{0}\" in {1}ms", TestName, elapsedTime.TotalMilliseconds);
        }

        // This needs to be implemented for every test
        protected abstract void Run();

        public void AddError(string message)
        {
            TestOutput.Add(new ValidationTestOutput() { Type = TestOutputType.Error, Output = message });
                TestState = TestState.Failed;
        }

        protected void AddWarning(string message)
        {
            TestOutput.Add(new ValidationTestOutput() { Type = TestOutputType.Warning, Output = message });
        }

        protected void AddInformation(string message)
        {
            TestOutput.Add(new ValidationTestOutput() { Type = TestOutputType.Information, Output = message });
        }
    }
}
