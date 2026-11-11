# Notas para certificación — Español

Producto: VolumeGuard, edición Store, Windows 11 x64. No requiere cuenta ni compra adicional. Instalar el paquete del envío; no usar el EXE administrativo.

**Justificación de runFullTrust:** aplicación WinForms de escritorio que usa Core Audio COM para consultar/modificar volumen de salida y NotifyIcon para el área de notificación. Se ejecuta con los permisos del usuario, sin elevación, sin servicio Windows, sin tareas administrativas y sin acceso de red propio. No solicita micrófono ni graba audio.

1. Usar una máquina de prueba sin el servicio administrativo VolumeGuard y con un dispositivo de salida activo. Mantener cualquier reproducción a nivel bajo; no necesita reproducir audio para consultar el volumen.
2. Abrir desde Inicio como usuario estándar: no aparece UAC. Configuración inicial 40 %.
3. Guardar 30 %. Subir manualmente el volumen de Windows a 50 % y observar que vuelve aproximadamente a 30 % tras el sondeo; tolerar redondeo del controlador. El porcentaje no representa dB.
4. Pulsar el número, escribir 25 y Enter; Guardar. Escape cancela la edición; salir del campo confirma solo la selección; Guardar aplica.
5. Minimizar o pulsar X: motor sigue activo junto al reloj. Doble clic restaura. «Cerrar panel» sale y deja de limitar (no restaura el volumen anterior).
6. Sin salida activa se indica incidencia; conectar un dispositivo compatible y comprobar recuperación.
7. Verificar idioma Windows español e inglés en sesiones nuevas; otro idioma usa inglés. No hay selector Inno Setup dentro de Store.
8. Cerrar, reabrir y comprobar persistencia. Reiniciar requiere abrir de nuevo la app. Desinstalar no debe dejar servicios o tareas.

Limitaciones declaradas: control solo mientras corre el proceso, sin protección contra el usuario, sin garantía contra picos de ~120 ms, sin garantía acústica/médica y sin control de amplificadores físicos. Datos y logs solo locales; no cuenta, anuncios o IAP. Precio, rating y mercados quedan a cargo del titular.

Antes del envío: agregar contacto real para evaluadores y resultados WACK. Estas notas son instrucciones, no un informe de pruebas superadas.
