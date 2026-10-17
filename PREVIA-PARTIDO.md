# Centro de partido

Al pulsar «Jugar próximo partido», se abre la previa antes de crear el balón y los jugadores. «Jugar ahora» inicia el encuentro. También se puede volver a carrera o revisar el once.

Incluye competición, temporada, jornada, local/visitante, rival del calendario, valoración del once, últimos cinco resultados de liga y clasificación alrededor del club del usuario. Las otras competiciones muestran su estado real y se identifican como simuladas al avanzar la jornada; no inventan un próximo rival.

Presentación original en azul oscuro y turquesa, con tarjetas y distintivos de iniciales en los colores del club. No incluye escudos ni imágenes extraídos de la captura de referencia. Distribución horizontal para PC y apilada para pantallas verticales.

## Validación

- CoreHarness: 220 comprobaciones aprobadas. Incluye los 114 clubes de las seis ligas, ausencia de cambios al consultar la previa, avance al siguiente partido, forma local/visitante y final de temporada.
- Compilación de todo el código de ejecución con el compilador y las referencias reales de Unity 6000.3.26f1: sin errores. Persiste un aviso anterior por `FindObjectOfType`.
- Revisión visual de esta nueva pantalla en Play Mode: pendiente. El servicio de control de aplicaciones agotó el tiempo de espera al localizar ventanas. No se ha confirmado aún su aspecto dentro del editor ni en un dispositivo móvil.
