using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
using UnityEngine.Profiling;

class ValidationSuiteTestRunManager
{
    Dictionary<Type, int> testValidationMap = new Dictionary<Type, int>();
    BaseValidation[] validationTests;
    SingleTestCompletedDelegate singleTestCompletionDelegate;
    ValidationType selectedValidationType;
    bool m_DependencieMode = false;

    public ValidationSuiteTestRunManager(IEnumerable<BaseValidation> validationTests, ValidationType validationType, SingleTestCompletedDelegate singleTestCompletionDelegate)
    {
        this.validationTests = validationTests.ToArray();
        this.singleTestCompletionDelegate = singleTestCompletionDelegate;
        selectedValidationType = validationType;

        if (this.validationTests.Any(test => test.DependsOn != null && test.SupportedValidations.Contains(this.selectedValidationType)))
        {
            m_DependencieMode = true;
            for (var i = 0; i < this.validationTests.Length; i++)
                testValidationMap.Add(this.validationTests[i].GetType(), i);
        }
    }

    /// <summary>
    /// This method run the list of validation test taking into account the dependencies between them.
    /// The dependencies information is taken from DependsOn property of BaseValidation Class. If not
    /// DependOn property is set it run a list of test in the discovery order.
    /// </summary>
    /// <returns></returns>
    public TestState RunValidations()
    {
        return RunValidations(m_DependencieMode ? validationTests
                                                : validationTests.Where(test => test.SupportedValidations.Contains(selectedValidationType)).ToArray());
    }

    TestState RunValidations(BaseValidation[] validationTests)
    {
        var result = TestState.Succeeded;
        // Run through tests
        foreach (var test in validationTests)
        {
            if (!test.ShouldRun)
                continue;

            if (m_DependencieMode && test.DependsOn != null && test.DependsOn.Length > 0)
            {
                var prerequisiteValidation = new BaseValidation[test.DependsOn.Length];
                for (var i = 0; i < test.DependsOn.Length; i++)
                {
                    prerequisiteValidation[i] = this.validationTests[testValidationMap[test.DependsOn[i]]];
                    if (prerequisiteValidation[i].SupportedValidations != null && prerequisiteValidation[i].SupportedValidations.Length > 0)
                        prerequisiteValidation[i].SupportedValidations[0] = selectedValidationType;
                }
                if (RunValidations(prerequisiteValidation) == TestState.Failed)
                {
                    test.TestState = TestState.Failed;
                    var dependencyFailMessageError = DependenciesFailMessageError(prerequisiteValidation);
                    test.AddError(dependencyFailMessageError);
                    result = TestState.Failed;
                }
            }

            if (test.TestState == TestState.NotRun || !m_DependencieMode)
                RunTest(test);

            if (test.TestState == TestState.Failed)
                result = TestState.Failed;
        }
        return result;
    }

    internal static string DependenciesFailMessageError(BaseValidation[] prerequisiteValidation)
    {
        var failingPrerequisites = $"({string.Join(", ", prerequisiteValidation.Where(test => test.TestState == TestState.Failed).Select(x => x.TestName).ToList())})";
        var plural = failingPrerequisites.Length > 1 ? "" : "s";
        var dependencyFailMessageError = $"This Validation fails because it has failing Validation prerequisite{plural} {failingPrerequisites}. Fix first the failing prerequisite validation{plural} {failingPrerequisites}, then run the validations again.";
        return dependencyFailMessageError;
    }

    TestState RunTest(BaseValidation test)
    {
        try
        {
            Profiler.BeginSample(test.TestName + ".run");
            test.RunTest();
            Profiler.EndSample();

            // Signal single test results to caller.
            singleTestCompletionDelegate(test);

            return test.TestState;
        }
        catch (Exception ex)
        {
            test.TestState = TestState.Failed;
            // Change the test outcome.
            test.AddError(ex.ToString());
            singleTestCompletionDelegate(test);
            return test.TestState;
        }
    }
}
