# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

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
