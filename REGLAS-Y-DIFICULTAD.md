# Dynasty IQ, dificultad y arbitraje básico

Actualización posterior: [control, saques y cobertura de las 17 leyes](REGLAMENTO-COBERTURA.md). Ese documento describe la implementación más reciente y las reglas que siguen pendientes.

## Menú y competiciones

La carrera y la previa muestran las competiciones del club, con fase, eliminación o título. Las copas nacionales ya incluyen participantes; se identifican Copa del Rey y FA Cup. Champions y Europa League aparecen para sus participantes, sin hacer participar al mismo equipo en ambas.

Europa League añade una temporada simulada de 36 clubes y eliminatorias. Su selección inicial usa las valoraciones de los clubes europeos restantes de la base del juego: **no son clasificados oficiales de la temporada 2026/27**. Las copas y los torneos europeos siguen simulándose al avanzar la jornada; los partidos 3D seleccionables siguen siendo los de liga. Se migran los guardados anteriores que no tenían Europa League.

## Tres niveles

Se elige Fácil, Medio o Difícil desde carrera o la previa y se guarda con la carrera. La IA rival cambia su intervalo de reacción, anchura del bloque, precisión y frecuencia de intentos de barrida. No cambia las valoraciones de los futbolistas. Los compañeros del usuario mantienen la coordinación de nivel Medio.

Dynasty IQ ahora calcula líneas compactas alrededor del balón, limita la distancia entre defensa y ataque, separa compañeros próximos, mantiene un único recuperador/presionante y procura evitar desmarques en posición adelantada. La IA evita receptores adelantados y penaliza opciones de pase muy marcadas.

## Controles y reglas implementadas

- PC: **Q** barrida, **J** pase, **K/espacio** tiro, **L** cambiar jugador, **P** pausa.
- Móvil: **D** barrida, junto a A/B/C y el joystick.
- La barrida dura 0,55 s, tiene recuperación de 2,4 s y consume energía. La IA también puede barrer. El modelo se inclina y baja su cápsula de colisión durante la acción.
- Se barre el segmento de movimiento para comparar el primer contacto con el balón y con el cuerpo rival. Rival antes del balón produce falta; dentro del área propia del infractor, penal. Un intento sin contacto no inventa una falta.
- El silbato detiene la acción. Se coloca el balón, se separan los rivales y se designa ejecutor. En penales se usa el punto a 11 m de la línea de gol. El jugador ejecuta con J/K; el ejecutor de la IA reanuda automáticamente.
- Fuera de juego: se conserva la posición al toque/pase del compañero, se compara con balón y penúltimo rival, se excluye el campo propio y se sanciona al intervenir/tocar el balón. Volver a una posición legal antes de recibir no cancela una posición adelantada del pase. Un control deliberado rival reinicia la evaluación; un rebote por colisión no. La sanción es tiro libre indirecto y no se permite marcar directamente sin otro contacto.

## Límites actuales

Es arbitraje de prototipo. Usa posiciones y contactos de cápsulas en el plano del campo, no captura esquelética de pies y brazos. No implementa aún tarjetas, ventaja, fuera de juego por obstruir la visión sin tocar el balón, todas las excepciones de reanudación ni entradas peligrosas que golpean primero el balón. La inclinación de la barrida todavía no queda registrada como pose completa en las repeticiones. No equivale a un árbitro completo de IFAB.

## Comprobaciones

- 257 comprobaciones de dominio aprobadas: calendario y temporadas, guardado, torneos, coordinación en tres dificultades, fuera de juego en ambas direcciones y contacto barrido/falta/penal.
- Código de ejecución y código de editor compilados con el compilador y referencias de Unity 6000.3.26f1.
- `Football Dynasty > Comprobar reglas en Play (partido de prueba)` ejecuta siete comprobaciones contra los actores y Rigidbody reales, desde carrera en Play. Rechaza interrumpir un partido activo y no avanza ni guarda la carrera. Escribe `RulesPlayCheck-result.txt`.
- Esa prueba de ejecución y la revisión visual de estos cambios están **pendientes**: el servicio de control de aplicaciones no consiguió activar Unity Hub minimizado. La compilación y las pruebas de dominio no demuestran por sí solas el comportamiento durante un partido completo.

Referencias de reglas: https://www.theifab.com/laws/latest/offside/ y https://www.theifab.com/laws/latest/fouls-and-misconduct/
