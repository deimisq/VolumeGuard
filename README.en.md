# VolumeGuard for Windows 11

[Español](README.md) · [MIT license](LICENSE) · [Contributing](CONTRIBUTING.md) · [Code signing policy](docs/CODE_SIGNING.md)

VolumeGuard periodically checks the master volume of active Windows output devices and reduces it to an administrator-selected limit. The main distribution is a Windows installer that installs an automatically started LocalSystem service. Closing the UI does not stop that service.

## Features

- Volume limit from 0 to 100 %, default 40 %.
- Automatic service start with Windows and configuration protected by administrator/SYSTEM permissions.
- Spanish and English installer and interface; installer language selector defaults to Windows UI language.
- Slider or click-to-edit percentage; Enter confirms, Escape cancels, Save applies.
- Notification area icon; minimizing or pressing X hides the panel. Tray menu exits the panel.
- Local status and rotating technical error logs; no accounts, advertising or app-owned telemetry.

## Build

Windows 11 x64, .NET SDK 8+ and Inno Setup 6 are required. Initial package restore requires access to NuGet.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
# Nonstandard Inno Setup location:
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -InnoSetupCompiler 'C:\Tools\Inno Setup 6\ISCC.exe'
```

Output: `dist/VolumeGuard-Setup-1.0.2.exe`. Run as administrator. Installation creates the VolumeGuard service and protects `%ProgramData%\VolumeGuard`; uninstall stops/removes the service. The panel requires UAC to change its configuration.

Current development installers are **unsigned**. Free code signing has not been approved; see the [signing policy](docs/CODE_SIGNING.md). CI artifacts are test builds, not automatically published official releases. Verify source and download origin rather than disabling Windows protections.

## Limitations

Checks run approximately every 120 ms; brief peaks can occur. Windows volume percentages are not decibels or a guarantee of safe sound exposure. External amplifiers and independent physical volume controls are not controlled. Some audio drivers may not support Core Audio from session 0; verify the target hardware. A per-user-session audio agent would be needed if that limitation occurs. It is not a security barrier against administrators, SYSTEM or offline access.

Logs and configuration are local. Inspect technical logs for personal file paths before sharing. Installation and audio enforcement should be tested on a VM or dedicated machine before distribution.

## Open source and support

Licensed under [MIT](LICENSE). [Voluntary support](docs/SUPPORT.md) can help maintain and improve the project; no donation URL is configured yet. All functions are available without donating.

Bug reports and contributions are welcome in Spanish or English. See [contribution guidance](CONTRIBUTING.md) and [security reporting](SECURITY.md).

## Experimental Store preparation

The separate per-user Store project and [bilingual documentation](docs/store/README.en.md) are experimental. They do not provide the administrative service or tamper resistance and are not the current distribution path. Development APPX/MSIX packages use a fake identity and must not be submitted to Microsoft Store.
