# VolumeGuard para Windows 11

[English](README.en.md) · [Licencia MIT](LICENSE) · [Contribuir](CONTRIBUTING.md) · [Code signing policy](docs/CODE_SIGNING.md)

VolumeGuard limita el volumen maestro de los dispositivos de salida de Windows mediante un servicio que arranca con el sistema. Incluye un panel en español e inglés y un icono en el área de notificación. Cerrar el panel no detiene el servicio.

**Distribución principal: instalador EXE con servicio administrativo.** Los binarios de desarrollo actuales no están firmados; la firma gratuita está pendiente de solicitud y aceptación. No desactives las protecciones de Windows para instalar archivos de origen desconocido.

Código abierto bajo licencia MIT. Puedes [apoyar voluntariamente al creador en GitHub Sponsors](https://github.com/sponsors/deimisq). Los aportes ayudan a mantener VolumeGuard y otros proyectos de código abierto. Todas las funciones están disponibles sin aportar.

## Preparación experimental para Microsoft Store (no se distribuye por esta vía)

Se incluye una edición Store por usuario, sin servicio ni elevación y activa solo mientras la app corre. La edición administrativa descrita abajo conserva sus funciones y su instalador EXE. Consulta la [guía en español](docs/store/README.es.md), la [guía en inglés](docs/store/README.en.md) y la [identidad de ejemplo](packaging/store/identity.example.json) antes de compilar con `build-store.ps1`. No enviar un paquete con identidad ficticia ni atribuir protección administrativa a la edición Store.

Proyecto fuente para un limitador de volumen administrado mediante servicio de Windows.

## Requisitos de compilación

- Windows 11 x64
- .NET 8 SDK (con acceso a NuGet para restaurar `System.ServiceProcess.ServiceController`)
- Inno Setup 6

## Generar instalador

Abre PowerShell en la carpeta raíz y ejecuta:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

El instalador se generará en `dist\VolumeGuard-Setup-1.0.3.exe`.
Ejecuta el instalador como administrador. El acceso directo `VolumeGuard` abre el panel y solicita UAC.
Antes de instalar se muestra un selector Español / English, con el idioma de Windows seleccionado de forma predeterminada (inglés si no es español). El panel y sus notificaciones usan el idioma elegido. Para cambiarlo, ejecuta de nuevo el instalador y selecciona el otro idioma. Si el panel ya está abierto, ciérralo desde su icono y ábrelo de nuevo después de actualizar.

## Funcionamiento

- Servicio `VolumeGuard` con arranque automático como LocalSystem.
- Revisión de dispositivos de salida activos cada ~120 ms y reducción al límite configurado.
- Valor predeterminado: 40 %.
- Configuración: `%ProgramData%\VolumeGuard\settings.json`, protegida por ACL del instalador.
- La interfaz requiere elevación UAC. Los usuarios estándar no pueden modificar la configuración ni detener el servicio por los medios habituales.
- Al minimizar el panel o pulsar la X, queda disponible en el área de notificación junto al reloj. Haz doble clic en el icono para abrirlo; su menú permite cerrar el panel sin detener el servicio. Windows puede agrupar el icono dentro del menú de iconos ocultos.
- La configuración se relee automáticamente sin reiniciar el servicio.
- El límite puede escribirse directamente en el campo numérico (0 a 100) o ajustarse con la barra. Ambos controles se sincronizan; pulsa Guardar límite para aplicarlo.
- Si la configuración falta o es inválida, el servicio conserva el último límite válido leído durante su ejecución. Hasta leer el primero aplica 0 %. El instalador crea una configuración inicial de 40 % si no existe.
- El panel muestra el estado comunicado por el servicio y avisa si lleva más de 10 segundos sin actualizarse. Guardar la configuración no confirma por sí solo que el audio esté protegido.
- Los errores se guardan en `%ProgramData%\VolumeGuard\service.log`, con rotación a `service.log.old` al superar aproximadamente 1 MB y un máximo de una entrada por minuto. El estado se publica en `status.json` cada 2 segundos.
- El instalador comprueba permisos, registro e inicio del servicio; las actualizaciones detienen el servicio antes de reemplazar sus archivos. Los fallos se detallan en `setup-error.log` dentro de la carpeta de instalación.
- El instalador incorpora desinstalación que detiene y elimina el servicio.

## Limitaciones importantes

- Se han compilado los ejecutables y el instalador y comprobado la interfaz. La instalación y el control real de audio deben validarse con los dispositivos y cuentas de destino antes de distribuir nuevas versiones.
- El control aplica al volumen maestro de los dispositivos de salida activos. No controla amplificadores externos, controles físicos independientes, ni garantiza que nunca exista un pico breve superior al límite (sondeo de ~120 ms).
- No es una barrera de seguridad contra otros administradores, cuentas SYSTEM ni arranque desde medios externos.
- En algunos controladores de audio, el acceso a Core Audio desde un servicio en sesión 0 puede no funcionar; validar con los dispositivos de destino. Si ocurre, debe incorporarse un agente de audio por sesión de usuario controlado por el servicio.
- El porcentaje de Windows no equivale a un límite de decibelios o de presión sonora segura.
