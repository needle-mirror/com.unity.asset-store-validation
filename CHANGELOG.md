# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.5.0] - 2024-02-15

### Added
- Added a validation for the package description to validate the length of the description is under 200 characters
- Added a validation to warn the user about the presence of zip files in a package

## [0.4.0] - 2023-11-21

### Fixed
- Fix false negative validation result when checking asmdef|asmref in nested folders with same parent folder name in AssembliesDefinitionValidation
- Fix files inside hidden folders taken into account in AssembliesDefinitionValidation
- Unity and UnityRelease version comparison
- Missing warning messages in the text report

### Changed

- All http calls are done from the same class now

## [0.3.0] - 2023-09-26

### Added

- Grouped validations that do not require network calls under `InternalTesting` ValidationType

### Changed

- Author Field, Changelog and Documentation validations to display a warning when using the `InternalTesting` ValidationType, if a url is present in the corresponding field

### Removed

- Use of `npm` and `node`

## [0.2.1] - 2023-09-05

### Fixed

- Mechanism used to detect if the user is currently logged in
- Validation report not showing when the package has not been published before
- Error in console when running validation on 2021 on a package with empty scene

## [0.2.0] - 2023-07-18

### Added

- Asset store publish validations
- Customizable timeout for checking if URLs are reachable
- Mandatory `unity` and `unityRelease` fields validation
- Short-circuit mechanism to avoid using npm when it has been flagged as "unusable"
- Warning result for validations that contain warnings

### Changed

- Clarified samples validation error message
- Clarified validation names
- Manifest display name validation to adhere to Asset Store guidelines
- Validation results now appear alphabetically

### Removed

- Package dependencies validation

### Fixed

- Unhandled exception when using npm to validate packages on production
- ValidationSuiteReport.GetReport returning null for existing JSON reports

## [0.1.3] - 2023-05-30
- First release of Asset Store Validation.
