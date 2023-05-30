# Asset Store Publisher Validation
### What is this validation?
This validation ensures that only one Asset Store product is linked to a UPM package. This can be validate in one of two ways:
- 1. If the package being published does not already exist on the UPM registry.
- 2. If the package being published is an update to the same linked Asset Store product.

### Why do we need this validation?
It is important to ensure that users know exactly which package they are installing into their project. This is why it is also important that each Asset Store product is linked with one UPM package, so that the user can be certain that is the one that will be installed.

# Errors
### There is no publisher linked to this account
No publisher account was found for the logged in user. Please visit the Asset store and create an account.

### This package name belongs to another publisher
This package name belongs to another publisher. Please rename your package to be able to publish.