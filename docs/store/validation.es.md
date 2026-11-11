# Validación realizada — 11 de noviembre de 2026

## Completado

- Compilación Release de Store, Admin y Service: cero errores; la comprobación final del servicio tras restaurar NuGet no mostró advertencias.
- Publicación Store x64 autocontenida .NET 8.
- Interfaz Store renderizada y revisada en español e inglés usando datos simulados; textos completos y aviso explícito de duración del control.
- Pruebas del panel sin iniciar motor: editor oculto al abrir, edición sin selección inicial, confirmación, cancelación, pérdida de foco, rango 0–100, sincronización de barra, X, minimizar, restaurar y salir desde el icono.
- Generador de recursos PNG ejecutado; sintaxis PowerShell y XML comprobadas.
- Script rechaza identidad sin completar y evita usar la identidad de desarrollo como envío.
- MakeAppx 10.0.26100.8249 (herramientas oficiales NuGet) generó MSIX y APPX de desarrollo con éxito.
- Inspección ZIP del manifiesto y payload: identidad de desarrollo, runFullTrust, ejecutable Store y blockmap presentes; sin allowElevation, packagedServices o extensión de servicio.

## Paquetes de desarrollo sin firmar

Identidad ficticia: VolumeGuard.Development / CN=VolumeGuardDevelopment. Versión 1.0.3.0 x64. No son paquetes de envío ni instalables mediante doble clic sin firma y confianza de prueba.

| Archivo | SHA256 |
|---|---|
| VolumeGuard_1.0.3.0_x64.msix | C2FE327A03A780C580C35985AF22EC4181E637E64C33ECEEAEE7473670CC2BFA |
| VolumeGuard_1.0.3.0_x64.appx | 59EEE3EA5083EF08F503B548E31700B68AAC5B2EE74D69D2E2A73F997C57AFC8 |

Las reconstrucciones pueden producir hashes diferentes. El script imprime el hash de cada resultado.

## Pendiente antes de publicar

Identidad real, firma local de prueba, instalación/actualización/desinstalación MSIX real, arranque estándar y comportamiento de audio de la app empaquetada, WACK, revisión de Microsoft de runFullTrust, capturas auténticas y URLs públicas de soporte/privacidad. Las previsualizaciones no se usaron como capturas finales ni se modificó el volumen durante las pruebas del panel.

La instalación completa de Windows SDK se inició, pero no había confirmado su finalización al realizar estas comprobaciones. Para empaquetar se usaron BuildTools NuGet locales; no se atribuyen a este entorno pruebas WACK.
