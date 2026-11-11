# Validation performed — November 11, 2026

## Completed

- Release builds of Store, Admin and Service: no errors; final Service check after NuGet restore had no warnings.
- Self-contained .NET 8 x64 Store publish.
- Store UI rendered and visually reviewed in Spanish and English with simulated status, readable text and explicit process-lifetime notice.
- UI checks without starting the engine: initially hidden editor, no automatic selection, confirm/cancel/focus loss, 0–100 bounds, slider sync, X, minimize, restore and tray exit.
- PNG generation, PowerShell syntax and XML checks.
- Rejection of incomplete identity and prevention of development identity in submission output.
- Successful MSIX and APPX packaging with MakeAppx 10.0.26100.8249 from Microsoft's official NuGet BuildTools.
- ZIP payload/manifest inspection: development identity, runFullTrust, Store executable and blockmap; no allowElevation, packagedServices or service extension.

## Unsigned development packages

Fake identity: VolumeGuard.Development / CN=VolumeGuardDevelopment. Version 1.0.3.0 x64. Not submission packages; local installation requires a trusted test signature.

| File | SHA256 |
|---|---|
| VolumeGuard_1.0.3.0_x64.msix | C2FE327A03A780C580C35985AF22EC4181E637E64C33ECEEAEE7473670CC2BFA |
| VolumeGuard_1.0.3.0_x64.appx | 59EEE3EA5083EF08F503B548E31700B68AAC5B2EE74D69D2E2A73F997C57AFC8 |

Rebuilds may produce different hashes; the script prints each output hash.

## Still required

Real identity, local test signing, actual MSIX install/update/uninstall, standard-user packaged launch and live audio tests, WACK, Microsoft approval of runFullTrust, genuine screenshots and public support/privacy URLs. Preview images are not final listing screenshots; UI tests did not change system volume.

The full Windows SDK installer was started but had not confirmed completion during these checks. Local NuGet BuildTools were used for packaging. No WACK pass is claimed.
