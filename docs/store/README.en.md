# Publish VolumeGuard in Microsoft Store

Prepared November 11, 2026. This is packaging and submission preparation, not certification or a completed publication.

## Editions

The existing administrative edition retains its EXE installer, LocalSystem service, UAC and protected ProgramData configuration. Microsoft's MSIX desktop guidance excludes Store apps requiring elevation. Do not submit a direct wrapper of that installer.

The separate `src/VolumeGuard.Store` project shares the UI and audio engine but runs entirely as the current user. It uses asInvoker, installs no service and changes no machine ACLs. Configuration/logs/status are stored under LocalApplicationData/VolumeGuardStore, subject to Windows package data redirection. Settings are separate from the administrative edition. The initial limit is 40 %. Windows UI language selects Spanish for es, English otherwise. Store handles installation and updates; the Inno Setup language dialog is not part of the package.

Volume control runs only while the app process is running, including in the notification area after minimizing or pressing X. “Close panel” exits and stops control. There is no automatic launch at sign-in. Users can change settings or exit; this edition is not parental control, tamper resistance or an administrator-enforced policy. Avoid running both editions together: the lower limit would prevail.

## Build

Use Windows 11 x64, .NET SDK 8+ capable of targeting net8.0, and Windows SDK with MakeAppx x64. Internet is required for initial restore. Inno Setup is not needed.

Instead of a machine-wide SDK, restore Microsoft's build tools locally: `dotnet restore packaging/store/BuildTools.csproj --packages artifacts/store/tools`. The packaging script also discovers MakeAppx there. BuildTools is pinned in the project; review security updates before publishing. Install WACK separately through Windows SDK.

Reserve the product name in Partner Center. Copy `packaging/store/identity.example.json` to `identity.local.json` and enter the exact package Name, full Publisher CN and PublisherDisplayName from product identity. Use a new increasing four-part version ending in .0 for each upload (initial proposal 1.0.3.0).

```powershell
.\build-store.ps1 -IdentityFile .\packaging\store\identity.local.json
.\build-store.ps1 -IdentityFile .\packaging\store\identity.local.json -Format appx
```

Unsigned output: `dist/store/submission/VolumeGuard_<version>_x64.msix` or `.appx`. MSIX is preferred. `-MakeAppxPath` overrides SDK discovery. `-Development` puts test-identity packages in a separate development folder. Never submit a fake identity. The script does not install certificates or sign packages. Successful packaging does not imply Store approval.

If script execution is disabled, use `powershell -NoProfile -ExecutionPolicy Bypass -File .\build-store.ps1 -IdentityFile .\packaging\store\identity.local.json`. VS Code tasks are included for submission and development builds. Use `identity.development.json` with `-Development` to validate the pipeline without real publisher details; never upload this identity to Store.

## Local validation

Store signs the distributed package. For local installation, sign a development copy with SignTool using a certificate whose Subject matches Publisher exactly; trust your own development certificate only on your test machine. Keep all private keys/PFX/passwords out of source control.

```powershell
signtool sign /fd SHA256 /sha1 CERTIFICATE_THUMBPRINT .\dist\store\development\VolumeGuard_1.0.3.0_x64.msix
Add-AppxPackage -Path .\dist\store\development\VolumeGuard_1.0.3.0_x64.msix
```

Run Windows App Certification Kit and retain its report. Test clean install, update and uninstall in a VM/standard account without the administrative service. Follow the bilingual certification protocol. Inspect actual audio behavior and CPU usage with physical devices. No live audio or packaged-install test is implied by documentation.

## Submission materials

Use the MSIX/APPX submission path, PC family, Windows 11 x64, Utilities & tools category, es-ES/en-US listings. Submit the real-identity package, explain runFullTrust for WinForms/Core Audio/NotifyIcon as current user (no elevation or service), attach review notes and real localized screenshots. Complete pricing, availability, markets, IARC rating and support/privacy URLs. Resolve all Partner Center validation errors before submission.

Prepared files: bilingual listing text, privacy drafts, specifications/checklists and certification notes in this folder; package manifest, identity template and asset generator in `packaging/store`. These Markdown listings are copy/paste material, not Partner Center import CSVs.

Owner must still supply: verified developer account; reserved identity/name; legal publisher and brand rights; support email/website; public HTTPS privacy pages in both languages; pricing/markets; IARC answers; real package screenshots; WACK report and completed release tests. Replace all placeholders before publication.

## Official references

- [Desktop packaging restrictions](https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-prepare)
- [Manifest and runFullTrust](https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-manual-conversion)
- [Package identity](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/package-identity-overview)
- [Signing](https://learn.microsoft.com/windows/msix/package/sign-msix-package-guide)
- [Versioning and WACK](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/package-version-numbering)
- [Listing assets](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/screenshots-and-images)
- [EXE/MSI alternative requirements](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msi/app-package-requirements): evaluate separately if retaining administrative enforcement is essential; signed binaries, versioned hosting and Microsoft review remain required.
