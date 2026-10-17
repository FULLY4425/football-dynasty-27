# VAR solicitado por el usuario

En partido, pulsa V o el botón Solicitar VAR. Se habilita para un incidente registrado en los últimos 12 segundos reales y se consume al pedir la revisión. Los saques de reanudación y el saque de centro invalidan la solicitud pendiente. La revisión automática de línea de gol y los replays tienen prioridad para evitar superponer cámaras y pausas.

Casos conectados: gol permitido o inválido por restricciones de reanudación, balón sobre la línea, fuera de juego sancionado al intervenir, falta/penal por barrida y entrada limpia con rival cercano. El sistema conserva una descripción de la evidencia del incidente. No inventa manos, tarjetas ni otras infracciones que el simulador todavía no modela.

La secuencia congela física y reloj, mueve al árbitro hacia un monitor junto a la banda con piernas/brazos animados, muestra la consulta y un gesto de decisión, anuncia la decisión y su motivo y restaura la posición original del árbitro, la cámara y la velocidad de juego. El trayecto tarda entre 3 y 12 segundos según la distancia; la revisión/decisión añade 6 segundos. Si existe replay suficiente, el monitor muestra un esquema animado de las posiciones reales de esa jugada.

Es una opción propia del juego. Esta primera versión confirma y explica la evidencia resuelta por el motor determinista; no incorpora errores arbitrales aleatorios ni un segundo evaluador independiente que revoque fallos de juicio. No se anuncia como una implementación completa del protocolo VAR oficial. No hay audio de voz ni guardado de esta escena como cámara independiente del replay.

350 comprobaciones de dominio aprobadas, incluida la clasificación de decisiones y la caducidad de incidentes. Runtime y editor compilados con referencias de Unity 6000.3.26f1.

Prueba: Football Dynasty > Comprobar VAR solicitado (20 s), desde carrera en Play sin partido activo. Crea un penal de prueba mediante el sistema de reanudaciones y comprueba solicitud, desplazamiento, pausa, anuncio y restauración. Escribe VarPlayCheck-result.txt sin guardar un resultado de carrera.

Prueba ejecutada en Unity: SUCCESS 6 Unity user-requested VAR checks. Se comprobó visualmente el desplazamiento animado del árbitro, con cámara cercana y partido detenido. No se ha validado en un dispositivo móvil físico.
