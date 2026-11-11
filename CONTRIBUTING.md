# Contributing / Contribuir

Bug reports and pull requests are welcome. Contributions are provided under the [MIT license](LICENSE).

For audio issues, include Windows version, audio driver/device type, app version and expected/observed behavior. Remove personal data and local user names from logs. Never attach private keys, certificates containing private keys, passwords or tokens.

Build on Windows 11 x64 with .NET SDK 8 and Inno Setup 6 using `powershell -NoProfile -ExecutionPolicy Bypass -File build.ps1`. Validate changes with quiet audio and a test account or VM. Installing the administrative edition creates a LocalSystem service and changes configuration permissions; document installation changes in your pull request. Do not use real loud playback to test enforcement.

Se aceptan incidencias y cambios en español. Indica la versión de Windows y de VolumeGuard, tipo de dispositivo/controlador, comportamiento esperado y observado. Elimina datos personales de los registros. Las contribuciones se realizan bajo [licencia MIT](LICENSE). Nunca incluyas claves, contraseñas ni tokens.

La compilación automática genera instaladores de prueba **sin firmar**. No significa que hayan superado pruebas de instalación ni que SignPath los haya aprobado. Las publicaciones oficiales deben ser revisadas por el mantenedor.
