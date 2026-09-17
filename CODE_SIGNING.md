# Code signing policy

## Provider

Free code signing provided by SignPath.io, certificate by SignPath Foundation.

RM Dark Helper is applying for the SignPath Foundation Open Source program. Until approval and integration are complete, the currently published v1.0.0 binary remains unsigned.

## Build provenance

Public release binaries are built from this GitHub repository using the checked-in build script and GitHub Actions workflow.

Release binaries are accompanied by SHA-256 checksums.

Signing must only be requested for binaries produced from this repository and its public build configuration.

## Roles

Current project roles are held by the repository owner:

- Committer and reviewer: [Christine Fritz (@christine-fritz)](https://github.com/christine-fritz)
- Signing approver: [Christine Fritz (@christine-fritz)](https://github.com/christine-fritz)

## Privacy

See [PRIVACY.md](PRIVACY.md).

This program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it.

## Release approval

Each release intended for signing must be manually approved by the signing approver after the corresponding public source revision and automated build have been reviewed.
