# Code signing policy

## Current status / Estado actual

VolumeGuard does **not yet have approved free signing** from SignPath Foundation. Current development installers are unsigned. A public repository or an MIT license does not grant signing access automatically. No affiliation, endorsement or certificate issuance is claimed.

La firma gratuita está pendiente de solicitud y aprobación. Los instaladores de desarrollo actuales no están firmados. No se declara afiliación, patrocinio ni certificado emitido por SignPath.

## Project roles

- Maintainer, committer, reviewer and proposed signing approver: [deimisq](https://github.com/deimisq).
- Outside contributions require maintainer review.
- Every future signing request requires maintainer approval.
- Repository and signing accounts must use MFA before applying.

## Proposed verifiable release process

1. Review source and build workflow, choose the release commit and version.
2. Build Windows x64 binaries from that exact source in GitHub Actions.
3. After SignPath approval and integration, sign the project's own service/admin binaries, then the installer and uninstaller; preserve existing vendor signatures on dependencies.
4. Verify Authenticode signatures and timestamps before publishing release assets.
5. Publish release notes and SHA256 hashes. Never commit signing keys or tokens.

SignPath settings, organization/project identifiers and approved signing policies must be configured after acceptance; no fake credentials or nonfunctional signing workflow is included.

If accepted, add SignPath's required attribution accurately: “Free code signing provided by SignPath.io, certificate by SignPath Foundation.” Do not present this as an existing service before approval. Signing does not guarantee absence of SmartScreen warnings, and UAC remains required to install the service.

## Privacy

The current application does not transfer information to other networked systems automatically. Audio volume access, settings, status and technical logs are local. Local error logs can contain file paths with user names; inspect them before sharing. A user opening a future donation link will visit an external provider under that provider's privacy terms.

## Requirements to complete before applying

- Public repository and first public release with documented functionality.
- Maintainer MFA and clear source-to-binary provenance.
- Verify dependency licensing and preserve third-party notices in release distributions.
- Publish privacy information and disclose service/configuration changes during installation.
- Apply at [SignPath Foundation](https://signpath.org/) and review its [conditions](https://signpath.org/terms.html).
