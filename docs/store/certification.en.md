# Certification notes — English

Product: VolumeGuard Store edition, Windows 11 x64. No account or additional purchase required. Install the submitted package, not the administrative EXE.

**runFullTrust justification:** desktop WinForms app using Core Audio COM to read/change output master volume and NotifyIcon for notification-area access. Runs as current user, without elevation, Windows services, administrative tasks or app-owned network traffic. No microphone access or audio recording.

1. Use a test machine without the administrative VolumeGuard service and with an active audio output. Keep playback quiet; playback is not required to inspect volume.
2. Launch from Start as a standard user: no UAC. Initial limit 40 %.
3. Save 30 %. Raise Windows volume manually to 50 % and observe reduction to approximately 30 % after polling; allow driver rounding. Percentages are not dB.
4. Click the number, enter 25, press Enter and Save. Escape cancels editing; focus loss confirms selection only; Save applies it.
5. Minimize or press X: the engine continues in the notification area. Double-click restores. “Close panel” exits and stops limiting; it does not restore previous volume.
6. With no active output, an issue is shown; reconnect a compatible endpoint and verify recovery.
7. Test fresh sessions with Spanish and English Windows UI language; other languages fall back to English. Store does not run the Inno Setup language dialog.
8. Exit/reopen to check persistence. After reboot the app must be opened again. Uninstall must leave no services or scheduled tasks.

Declared limits: process-lifetime operation, user can change/exit, brief peaks possible with ~120 ms polling, no acoustic/medical guarantee, no physical amplifier control. Only local data/logs, no account/ads/IAP. Owner determines price, rating and markets.

Before submission: add a real reviewer contact and WACK results. These notes are instructions, not a passed-test report.
