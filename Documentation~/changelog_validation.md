# Changelog Validation
### What is this validation?
This validation ensures that a valid `CHANGELOG.md` file is included in the package's root folder. This file contains information about every new feature and bug fix in reverse chronological order from the latest update to when the package was first published. For more information visit [keepachangelog.com](https://keepachangelog.com/en/1.0.0/).

The changelog has the following basic structure:

- `## [X.Y.Z] - YYYY-MM-DD` entries in descending order (most recent first). The `# [X.Y.Z]` part states the full version of the package, for example `# [1.2.3]`
- Under `##` headers, `### <type>` headers are listed in this order: `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`.

**Note: Published Asset Store packages do not allow for the** `[Unreleased]` **entry that is usually allowed within changelogs**

### Why do we need this validation?
A changelog makes it easier for users and contributors to see precisely what notable changes have been made between each release of a package.

## Errors

### No changelog found
No valid changelog was found for the package. This means that there was no reachable `changelogUrl` field found in the package.json **AND** that no valid `CHANGELOG.md` file was found in the root of the package. Please add either a valid and reachable `changelogUrl` to the package.json file, and/or a valid `CHANGELOG.md` file to the root of your package.

### Changelog invalid capitalization
The changelog file provided in the package did not have the proper capitalization. As a part of Unity standards, the changelog within Asset Store packages must be named `CHANGELOG.md`. In order to resolve this error; rename the file to have proper capitalization.

### Invalid or missing file extension
Unity changelogs are required to be named `CHANGELOG.md`. This error indicates that the changelog file extension found within the package is either missing or invalid. To resolve this error, ensure that the changelog is named `CHANGELOG.md` and run the validation again.

### Changelog contains unreleased entry
All Asset Store packages are that are published qualify as a released version of that package and therfor have no use for an `[Unreleased]` entry in the changelog. In order to resolve this error; either remove this entry or move it's contents into an entry that follows valid semantic versioning.

### Package version is not in changelog
The version found in the package's manifest (package.json) does not appear anywhere in the changelog. Whenever a new version of a package is published, it is important to keep a changelog of the updates. To resolve this error; add a new entry into the changelog with the package's current version.

### Changelog entry is missing date
Every changelog entry is required to have the date at which that update was published. This error means that an entry in the package's `CHANGELOG.md` is missing a date. To resolve this error; add the missing date to the changelog.

### Changelog entry date format is invalid
The date in one of the changelog entries does not follow the proper `YYYY-MM-DD` format or is malformed. To resolve this error; ensure that every date associated with each changelog entry follows this format.

### Package version is not first entry in changelog
The package entry found for the version in the package manifest (package.json) is not the first entry in the changelog. Since the `CHANGELOG.md` file follows reverse chronological order, the latest version should be the first entry in the changelog. To resolve this error; make sure that the latest package version is the first entry in the changelog.

### Changelog header order is incorrect
The headers underneath each changelog entry should be listed in the following order: `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`. This error indicates that the order in which they appear is not as stated. To resolve this error; fix the order in which the headers appear to match that which is standard.

### Repeated headers in changelog entry
An entry in the `CHANGELOG.md` file has repeating headers (e.g `### Added` appears twice). To resolve this error; consolidate the two/more headers into one header, then run the validation again.

### Empty header or whitespace
One of the entries or headers in the `CHANGELOG.md` file is blank or contains nothing but whitespace. To resolve this error either remove the empty header/entry or add a title to it, then run the validation again.


## Warnings

### Changelog URL not reachable
This warning will be thrown when there is a changelogUrl field set in the package.json, but it is not reachable throught the browser. Ensure that the url is correct and that you are connected to the internet before running the validation again. 

### Changelog entry date format is deprecated
Changelog entry dates should follow `YYYY-MM-DD`. This warning will be thrown if any changelog entry uses deprecated dates such as `YYYY-MM-D`, `YYYY-M-DD`, or `YYYY-M-D`. To remove this warning, simply adhere to the standard `YYYY-MM-DD` format within all changelog entries.

### Unexpected header entry
An unexpected header appears underneath one or more of the entries in the `CHANGELOG.md` file. The expected headers are as follows (case-sensitive): `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`. While users are allowed to use any headers they want, it is strongly encouraged to use the headers listed above. To remove the warning; simply adhear to the recommended headers.