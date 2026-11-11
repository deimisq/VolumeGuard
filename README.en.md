<div align="center">
  <img src="src/VolumeGuard.Admin/Assets/VolumeGuard.svg" width="88" height="88" alt="VolumeGuard icon">
  <h1>VolumeGuard</h1>
  <p>A Windows volume limit, managed by you.</p>
  <p>
    <a href="https://github.com/deimisq/VolumeGuard/releases/download/v1.0.3/VolumeGuard-Setup-1.0.3.exe"><img src="https://img.shields.io/badge/Download_installer-Windows_x64-2563eb?style=for-the-badge" alt="Download VolumeGuard 1.0.3 Windows x64 installer"></a>
  </p>
  <p>Test release 1.0.3 · Digitally unsigned · <a href="https://github.com/deimisq/VolumeGuard/releases/tag/v1.0.3">Release notes and SHA256 verification</a></p>
  <p>
    <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT license"></a>
    <a href="installer/VolumeGuard.iss"><img src="https://img.shields.io/badge/source_version-1.0.3-2563eb.svg" alt="Source version 1.0.3"></a>
    <a href="https://github.com/deimisq/VolumeGuard/actions/workflows/build.yml"><img src="https://github.com/deimisq/VolumeGuard/actions/workflows/build.yml/badge.svg" alt="Build status"></a>
    <a href="https://github.com/sponsors/deimisq"><img src="https://img.shields.io/badge/GitHub_Sponsors-Sponsor-ea4aaa?logo=githubsponsors&amp;logoColor=white" alt="Sponsor deimisq on GitHub"></a>
  </p>
  <p>
    <img src="https://img.shields.io/badge/Windows-11_x64-0078D4.svg" alt="Windows 11 x64">
    <img src="https://img.shields.io/badge/.NET-8-512BD4.svg" alt=".NET 8">
    <img src="https://img.shields.io/badge/languages-ES_%2F_EN-64748b.svg" alt="Spanish and English">
  </p>
  <p>
    <a href="README.md">Español</a> · <strong>English</strong> ·
    <a href="#installation">Installation</a> ·
    <a href="https://github.com/sponsors/deimisq">Support the creator</a> ·
    <a href="CONTRIBUTING.md#english">Contribute code</a> ·
    <a href="https://github.com/deimisq/VolumeGuard/issues">Report an issue</a>
  </p>
</div>

---

VolumeGuard keeps the master volume of active Windows output devices below an administrator-selected percentage. Its service starts with Windows and keeps running when the panel is closed.

## Features

| Feature | How it works |
|---|---|
| Choose a limit | Move the slider or click the percentage to enter 0–100. Save to apply. |
| Keep control active | The LocalSystem service starts automatically. Closing the panel does not stop it. |
| Protect settings | The panel requests UAC; settings are restricted to administrators and SYSTEM. |
| Hide the panel | Minimize or press X to keep it in the notification area. Double-click its icon to reopen. |
| Use your language | Spanish and English installer/UI; setup defaults to Windows display language. |
| Check status | View the applied limit and service issues. |

## Installation

**[Download VolumeGuard-Setup-1.0.3.exe](https://github.com/deimisq/VolumeGuard/releases/download/v1.0.3/VolumeGuard-Setup-1.0.3.exe)** — Windows 11 x64 installer, .NET included.

The public download is under [Releases → Assets](https://github.com/deimisq/VolumeGuard/releases/tag/v1.0.3), alongside `SHA256SUMS.txt`. No GitHub account is required. This is a **test release**: installation and audio enforcement still require target-device validation. The hash checks integrity and does not replace a digital signature.

You can also build from source or inspect development artifacts in [GitHub Actions](https://github.com/deimisq/VolumeGuard/actions/workflows/build.yml).

> **Signing pending:** development installers are unsigned. Free signing has not been approved. See the [code signing policy](docs/CODE_SIGNING.md).

1. Run the installer as administrator and choose Español or English.
2. Open **VolumeGuard**, choose a percentage and click **Save limit**.
3. Check **Protection active** and the applied value.

Installation creates a Windows service and protects its configuration folder. Uninstall stops/removes the service. Before updating, exit the panel from its tray menu. To change the UI language, run setup again and choose the other language.

## Support the creator

[![Sponsor on GitHub](https://img.shields.io/badge/Support_deimisq-GitHub_Sponsors-ea4aaa?logo=githubsponsors&logoColor=white)](https://github.com/sponsors/deimisq)

VolumeGuard is free and open source. Voluntary contributions help **deimisq** maintain this and other projects. **All features remain available without donating.** Sponsorship does not purchase priority support or delivery commitments.

Payments take place on GitHub, outside the app. [Learn more about contributions](docs/SUPPORT.md).

## Contribute code or report issues

| I want to… | Where to go |
|---|---|
| Support the creator financially | [GitHub Sponsors](https://github.com/sponsors/deimisq) |
| Improve code or documentation | [Contribution guide](CONTRIBUTING.md#english) |
| Report a bug or request a feature | [VolumeGuard issues](https://github.com/deimisq/VolumeGuard/issues) |
| Report a vulnerability | [Security policy](SECURITY.md) |

Reports and contributions are welcome in Spanish and English. Remove personal data from logs before sharing.

## Build from source

**Requirements:** Windows 11 x64, .NET SDK 8+ and Inno Setup 6. Initial dependency restore requires access to NuGet.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

For a custom Inno Setup location:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -InnoSetupCompiler 'C:\Tools\Inno Setup 6\ISCC.exe'
```

Output: `dist/VolumeGuard-Setup-1.0.3.exe`. A VS Code build task and a [GitHub Actions workflow](.github/workflows/build.yml) are included.

## Limitations and compatibility

- Volume is checked approximately every **120 ms**, so brief peaks can occur.
- Windows percentages **are not decibels and do not guarantee safe sound exposure**.
- External amplifiers and independent hardware controls are not controlled.
- It is not a barrier against administrators, SYSTEM or offline access.
- Some audio drivers may not work from a session-0 service. Validate target devices; those cases would require a per-user-session agent.
- Executables/installer have been compiled and the UI checked. New releases still require installation and live audio tests before distribution.

<details>
<summary><strong>Configuration and diagnostics</strong></summary>

- Initial limit: **40 %**.
- Settings: `%ProgramData%\VolumeGuard\settings.json`, reloaded without restarting the service.
- Invalid/unreadable settings retain the last valid limit. Until the first successful read, **0 %** is applied.
- Status: `status.json`, updated approximately every 2 seconds. The panel warns after over 10 seconds without updates.
- Local log: `service.log`, rotated to `service.log.old` at approximately 1 MB, at most one entry per minute.
- Installer errors: `setup-error.log` in the installation folder.
- No online accounts, ads or app-owned telemetry. Technical logs may contain paths including user names.

</details>

<details>
<summary><strong>Experimental Store preparation</strong></summary>

The main distribution is the EXE installer with an administrative service. A separate experimental Store edition runs per user without service/elevation and controls volume only while running. **It is not currently distributed through Microsoft Store.**

See the [preparation guide](docs/store/README.en.md) to explore that variant. Development APPX/MSIX packages use a fake identity and must not be submitted to Store.

</details>

## License and authorship

[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Created and maintained by [deimisq](https://github.com/deimisq). Available under the [MIT license](LICENSE). See the [code signing policy](docs/CODE_SIGNING.md) and [security policy](SECURITY.md).
