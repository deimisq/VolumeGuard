# Publicar VolumeGuard en Microsoft Store

Preparación técnica y editorial: 11 de noviembre de 2026. No es una certificación ni una publicación completada.

## Ediciones y decisión de arquitectura

La edición administrativa original conserva su instalador EXE, servicio LocalSystem, UAC y configuración protegida en ProgramData. No debe convertirse directamente a APPX y presentarse como compatible con Store: Microsoft indica que las aplicaciones MSIX que necesitan elevación no se admiten en la tienda.

Se añadió `src/VolumeGuard.Store`: interfaz y motor de audio compartidos, ejecutados en el proceso del usuario, manifiesto asInvoker, sin instalar servicios, sin elevación y sin modificar las ACL de la máquina. Usa LocalApplicationData/VolumeGuardStore (Windows puede redirigir esta ubicación al almacenamiento privado del paquete). No comparte configuración con la edición administrativa. Crea un límite inicial de 40 % y conserva el idioma de interfaz de Windows: español si es es, inglés en los demás casos. Store gestiona instalación/actualización sin el selector de Inno Setup.

La edición Store funciona mientras su proceso siga abierto, también al minimizar o cerrar la ventana con X. «Cerrar panel» desde el icono detiene el motor. No inicia automáticamente al iniciar sesión. No impide que el usuario cambie el límite o cierre la app; no es control parental ni restricción administrativa. No ejecutar ambas ediciones simultáneamente durante pruebas: el límite menor prevalecería.

## Archivos preparados

- `build-store.ps1`: publica x64 autocontenido y genera MSIX o APPX con MakeAppx; no firma ni instala certificados.
- `packaging/store/identity.example.json`: valores obligatorios de Partner Center, sin inventar identidad real.
- `packaging/store/AppxManifest.template.xml`: Windows 11 x64, es-ES/en-US, runFullTrust. Sin allowElevation ni packagedServices.
- `packaging/store/New-Assets.ps1`: logos PNG de paquete y logo de ficha a partir del icono existente.
- `listing.es.md` / `listing.en.md`: textos para copiar manualmente a Partner Center; no son un CSV de importación.
- `privacy.es.md` / `privacy.en.md`: borradores para publicar en una URL HTTPS propia después de completar el responsable y contacto.
- `requirements.es.md` / `requirements.en.md`: especificaciones y lista de validación.
- `certification.es.md` / `certification.en.md`: pasos para evaluadores.

## Compilar

Requisitos: Windows 11 x64, .NET SDK 8 o posterior con targeting net8.0 y Windows SDK con MakeAppx x64. Restauración NuGet inicial con Internet. No necesita Inno Setup.

Como alternativa al SDK instalado en el sistema, restaurar las herramientas oficiales de Microsoft en el proyecto: `dotnet restore packaging/store/BuildTools.csproj --packages artifacts/store/tools`. El script busca también MakeAppx en esa ubicación. La versión de BuildTools queda fijada en el proyecto; revisar actualizaciones de seguridad antes de publicar. WACK se instala por separado con Windows SDK.

1. Crear o verificar la cuenta de desarrollador y reservar el nombre en Partner Center.
2. Copiar `identity.example.json` a `identity.local.json`; rellenar Name, Publisher (CN completo) y PublisherDisplayName exactamente como aparecen en Identidad del producto. Versión inicial sugerida: 1.0.3.0; aumentar para cada actualización y mantener la cuarta parte en cero.
3. Ejecutar desde la raíz:

```powershell
.\build-store.ps1 -IdentityFile .\packaging\store\identity.local.json
# APPX alternativo; mismo contenido y mismas condiciones de certificación:
.\build-store.ps1 -IdentityFile .\packaging\store\identity.local.json -Format appx
```

Salida: `dist/store/submission/VolumeGuard_<version>_x64.msix` o `.appx`, sin firmar. El script valida identidad no vacía, versión, publicación y resultado de MakeAppx. MSIX es la opción moderna recomendada. Un paquete generado no implica aprobación de runFullTrust ni certificación. Para probar una identidad ficticia, usar valores propios y `-Development`; no enviar ese paquete al portal.

En equipos con ejecución de scripts deshabilitada, usar `powershell -NoProfile -ExecutionPolicy Bypass -File .\build-store.ps1 -IdentityFile .\packaging\store\identity.local.json`. También se incluyen tareas de VS Code para Store y para desarrollo. La identidad `identity.development.json` permite probar el pipeline con `-Development` sin inventar datos del editor; jamás enviarla a Store.

## Firma y prueba local

Microsoft firma el paquete distribuido por Store. Para instalar localmente, usar una copia firmada con SignTool y un certificado cuyo Subject coincida exactamente con Publisher; confiar solo en el certificado de desarrollo que controles en la máquina de pruebas. No subir PFX, claves ni contraseñas al repositorio. No distribuir un certificado autofirmado a usuarios como sustituto de Store.

```powershell
# Ejemplo con certificado ya configurado en el almacén personal:
signtool sign /fd SHA256 /sha1 HUELLA_DEL_CERTIFICADO .\dist\store\development\VolumeGuard_1.0.3.0_x64.msix
Add-AppxPackage -Path .\dist\store\development\VolumeGuard_1.0.3.0_x64.msix
```

Ejecutar Windows App Certification Kit sobre el paquete firmado localmente, guardar el informe y resolver los fallos. Probar instalación limpia y actualización en VM/cuenta estándar sin el servicio administrativo instalado. Ejecutar el protocolo bilingüe de certificación. El selector de Inno Setup pertenece solo a la distribución EXE; no se incluye ni ejecuta en MSIX.

## Enviar

Elegir envío de paquete MSIX/APPX, cargar el paquete con identidad real y completar categoría Utilidades y herramientas, PC, Windows 11, x64, idiomas es-ES y en-US. Completar disponibilidad/precio/mercados, cuestionario IARC, datos de soporte y privacidad. Declarar runFullTrust y explicar que permite WinForms/Core Audio/NotifyIcon a nivel de usuario, sin elevación ni servicio. Adjuntar notas de certificación y capturas reales de cada idioma. Revisar advertencias del portal y enviar solo después de pruebas y URLs públicas verificadas.

## Datos pendientes del titular

- Identidad y cuenta verificada de Partner Center, disponibilidad del nombre VolumeGuard y derechos sobre marca/icono.
- Nombre legal del editor, correo de soporte, sitio/URL de soporte y URL pública de privacidad en ambos idiomas.
- Mercados, precio, política de soporte y clasificación IARC respondida por el titular.
- Capturas reales de la edición empaquetada; informe WACK y pruebas de instalación/actualización/desinstalación.
- Validación final del rendimiento de audio en dispositivos físicos USB/HDMI/Bluetooth y de accesibilidad/escalado.

## Fuentes oficiales

- [Preparación y limitaciones de elevación](https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-prepare)
- [Empaquetado de escritorio y runFullTrust](https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-manual-conversion)
- [Identidad de paquete](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/package-identity-overview)
- [Firma MSIX](https://learn.microsoft.com/windows/msix/package/sign-msix-package-guide)
- [Versiones y WACK](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/package-version-numbering)
- [Capturas y recursos de ficha](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/screenshots-and-images)
- [Alternativa EXE/MSI](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msi/app-package-requirements): si se necesita conservar la protección administrativa, estudiar esta vía por separado; requiere firma de los binarios y URL versionada, además de la revisión de Microsoft.
