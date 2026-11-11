# Especificaciones y validación — Store

## Especificación del paquete

| Campo | Valor preparado / acción |
|---|---|
| Formato | MSIX recomendado; APPX mediante -Format appx |
| Sistema mínimo | Windows 11, build 22000 |
| Arquitectura | x64; no se declara ARM64/x86 nativo |
| Runtime | .NET 8 autocontenido, sin descarga de runtime al instalar |
| Proceso | WinForms full trust por usuario, asInvoker |
| Permiso restringido | runFullTrust; justificar en Partner Center |
| Servicios / elevación | Ninguno en Store |
| Identidad | Name y Publisher exactos del portal, pendientes |
| Versión | 1.0.3.0 propuesta; cuarta parte cero; aumentar en cada subida |
| Idiomas | es-ES y en-US; interfaz según Windows |
| Datos | Perfil local del usuario, separados de edición administrativa |
| Red / cuentas / IAP / anuncios | Ninguno implementado |
| Inicio automático | No implementado; abrir la app tras iniciar sesión |
| Instalación / actualización | Store; sin ejecutar instalador EXE |

## Recursos visuales

El generador entrega StoreLogo 50×50, Square44x44Logo 44×44, Square150x150Logo 150×150 y ListingLogo300 300×300 en PNG. Los tres primeros son recursos del manifiesto; el último es un recurso para la ficha. No confundir logos de paquete con capturas. Revisar nitidez/transparencia en tema claro y oscuro. El logo de 300 px deriva de un icono de 256 px: revisar calidad antes de usarlo como material promocional.

Para PC: mínimo una captura real por ficha localizada; Microsoft recomienda 5–8. Preparar PNG de 1920×1080 (al menos 1366×768), interfaz auténtica de la edición Store, texto legible y sin datos personales. Verificar límites de tamaño/cantidad actuales del portal. Capturar estados de límite, edición, confirmación, icono de notificación y aviso de fallo. No subir los PNG de `.preview`: muestran pruebas simuladas, no evidencia de la app instalada.

Arte promocional adicional es opcional salvo que Partner Center lo exija para el flujo elegido. Usar únicamente plantillas/dimensiones vigentes del portal; no bloquear el envío por inventar requisitos de Xbox/móvil. Completar pies alternativos/accesibles y traducciones donde el portal los permita.

## Lista de salida

- [ ] Identidad reservada y valores reales; derechos de nombre/icono confirmados.
- [ ] Publicación Store y MakeAppx sin errores; hash conservado.
- [ ] Firma local de prueba y WACK sin fallos bloqueantes; informe archivado.
- [ ] Instalación limpia, arranque por usuario estándar sin UAC y actualización conservando ajustes.
- [ ] Sin servicio administrativo instalado ni escrituras a ProgramData/Program Files.
- [ ] 0, 40 y 100 %; Enter/Escape/salir del editor; Guardar; barra sincronizada.
- [ ] Audio verificado con salida integrada, USB, HDMI y Bluetooth disponibles; desconectar/reconectar y suspensión/reanudación.
- [ ] X y minimizar conservan motor; menú de salida lo detiene; reinicio no promete autoarranque.
- [ ] Idiomas es/en y fallback de otro idioma; DPI 100/150/200 %, teclado y lector de pantalla.
- [ ] Sin reproducción fuerte durante pruebas; porcentajes no interpretados como umbral médico.
- [ ] CPU/memoria durante 30 minutos; ausencia de crecimiento continuo; logs rotan y no se envían.
- [ ] Desinstalación, restablecimiento y reinstalación verificados.
- [ ] Capturas reales y textos de ambos idiomas completos y honestos.
- [ ] Privacidad/soporte HTTPS publicados y accesibles sin iniciar sesión; datos del editor completos.
- [ ] IARC, precio, mercados y explicación de runFullTrust completados.
- [ ] El titular revisó licencia, obligaciones y envío final.

Los elementos no marcados están pendientes; no se consideran aprobados por existir este archivo.
