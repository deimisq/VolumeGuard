# Store specifications and release checklist

| Field | Prepared value / action |
|---|---|
| Format | MSIX preferred; APPX via -Format appx |
| Minimum OS | Windows 11 build 22000 |
| Architecture | x64 only; no native ARM64/x86 claim |
| Runtime | Self-contained .NET 8 |
| Process | Per-user full-trust WinForms, asInvoker |
| Restricted capability | runFullTrust; explain in Partner Center |
| Services / elevation | None in Store edition |
| Identity | Exact Partner Center Name and Publisher, pending |
| Version | Proposed 1.0.3.0; increment each upload; final component zero |
| Languages | es-ES/en-US; follows Windows UI language |
| Data | Local user profile, separate from administrative edition |
| Network / accounts / IAP / ads | None implemented |
| Automatic startup | Not implemented; open app after sign-in |
| Installation / updates | Store, no EXE installer execution |

## Visual assets

Generator creates PNG StoreLogo 50×50, Square44x44Logo 44×44, Square150x150Logo 150×150 and ListingLogo300 300×300. The first three are manifest assets; the last is for the listing. Inspect transparency and clarity in light/dark themes. The 300 px image is derived from a 256 px icon; approve its quality before promotional use.

For desktop, provide at least one actual screenshot per localized listing; Microsoft recommends 5–8. Prepare 1920×1080 PNGs (at least 1366×768), genuine installed Store-edition UI, readable text and no personal data. Check current portal count/file-size limits. Capture selection, editing, confirmed status, notification area and an error state. Do not upload `.preview` images: they use simulated status and are not installed-package evidence.

Additional promotional assets are optional unless the chosen portal flow requests them. Use its current templates/dimensions rather than inventing Xbox/mobile requirements. Supply localized captions and accessibility descriptions where supported.

## Release checklist

- [ ] Reserved real identity and confirmed name/icon rights.
- [ ] Store build and MakeAppx succeeded; hash retained.
- [ ] Locally signed test package passes WACK; report retained.
- [ ] Clean install, standard-user launch without UAC and update preserving settings.
- [ ] No administrative service or ProgramData/Program Files writes.
- [ ] 0/40/100 %, Enter/Escape/focus loss, Save and slider synchronization.
- [ ] Built-in/USB/HDMI/Bluetooth endpoints where available, hotplug, suspend/resume.
- [ ] Minimize/X keep engine running; tray exit stops it; no false automatic-start claim.
- [ ] Spanish/English/other-language fallback, DPI 100/150/200 %, keyboard and screen reader.
- [ ] Quiet audio during tests; no interpretation as a medically safe threshold.
- [ ] 30-minute CPU/memory observation, no continuous growth, local log rotation.
- [ ] Uninstall, reset and reinstall behavior verified.
- [ ] Genuine screenshots and accurate complete bilingual copy.
- [ ] Public HTTPS privacy/support URLs without sign-in; owner information completed.
- [ ] IARC, pricing, markets and runFullTrust explanation completed.
- [ ] Owner approves licensing, obligations and final submission.

Unchecked items remain pending. This file does not assert certification.
