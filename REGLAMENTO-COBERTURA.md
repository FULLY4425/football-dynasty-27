# Control, Dynasty IQ y cobertura del reglamento

## Incorporado en esta actualización

- Indicador turquesa con aro y flecha sobre el futbolista controlado, más su nombre en pantalla. Se oculta en los menús y repeticiones.
- Cambio al compañero que recibe/controla el balón. El pase ya no transfiere el control antes de llegar. Un rebote rápido no basta y el cambio manual tiene prioridad breve.
- Dynasty IQ: puntuación de líneas de pase, penalización de pases interceptables, anticipación de balón en movimiento, receptor que ataca la trayectoria y conducción hacia el espacio cuando un rival bloquea el avance. Se conservan los tres niveles de dificultad.
- El balón deja de devolverse artificialmente al campo. Al cruzar completamente una línea, se asigna banda, meta o córner según la línea y el último equipo en tocarlo. La posición de salida se interpola; si cruza dos líneas en un paso físico se usa la primera.
- Árbitro principal visible y con colisión. Sigue la jugada procurando evitar a los jugadores. El contacto con él dentro del campo produce un saque neutral por petición del usuario. Los demás jugadores se separan, el balón cae al suelo y no se permite gol antes de tocar dos jugadores distintos.
- Banda con lanzamiento desde arriba, meta ejecutada por el portero y córner. Las tres reanudaciones están exentas de fuera de juego directo. Se controla el doble toque del ejecutor y el gol directo inválido; las distancias se mantienen también cerca de las esquinas.

En una reanudación del equipo del usuario, **J** o **K** ejecutan la acción; en móvil B/A. La IA reanuda automáticamente. El saque neutral se deja caer automáticamente.

## Qué significa «reglas oficiales» aquí

La referencia del fútbol 11 es el reglamento de IFAB. Un reglamento completo no equivale solamente a detectar faltas y salidas. Este proyecto sigue siendo un simulador en desarrollo y **no implementa íntegramente las 17 leyes**.

El saque neutral tras *cada* contacto con el árbitro es una regla personalizada solicitada. En la regla oficial, si el balón permanece en el campo, el contacto debe producir alguno de los efectos previstos, como cambio de posesión, ataque prometedor o gol directo. Si ya sale del campo, corresponde la reanudación por la salida. [IFAB, Ley 9](https://www.theifab.com/laws/latest/the-ball-in-and-out-of-play/).

| Ley | Estado en el juego | Trabajo pendiente |
|---|---|---|
| 1. Terreno | Medidas, líneas principales, áreas y porterías del prototipo | Marcajes y verificación completa del terreno |
| 2. Balón | Esfera de 22 cm y masa de 430 g | Procedimiento de balón defectuoso |
| 3. Jugadores | Once por equipo y plantillas de carrera | Sustituciones durante el partido, expulsiones y mínimo de jugadores |
| 4. Equipamiento | Colores y modelos de los equipos | Comprobaciones de equipamiento y correcciones |
| 5. Árbitro | Modelo, colisión, silbato y decisiones básicas | Ventaja, disciplina integral y gestión completa del partido |
| 6. Otros oficiales | Pendiente | Asistentes y sus señales; VAR si se incorpora al formato |
| 7. Duración | Partido acelerado de 180 s, pausa del reloj en reanudaciones | Dos tiempos, cambio de campo y tiempo añadido |
| 8. Inicio y reanudación | Saques tras gol y balón neutral | Procedimiento completo de inicio y todas las infracciones |
| 9. Balón en juego/fuera | Cruce completo y primera línea de salida | Modo de contacto arbitral estrictamente condicionado por la ley oficial |
| 10. Resultado | Goles por cruce físico y restricciones de gol directo | Prórroga y tanda de penales jugables |
| 11. Fuera de juego | Instantánea al toque, intervención por contacto y excepciones de banda/meta/córner | Interferencia sin tocar, visión del portero y evaluación esquelética |
| 12. Faltas | Barrida al cuerpo antes del balón; falta o penal según lugar | Manos, tarjetas, entradas peligrosas, ventaja y demás conductas |
| 13. Tiros libres | Ejecutor, distancia, directo/indirecto y doble toque | Casos completos de barreras e infracciones |
| 14. Penal | Punto de 11 m, ejecutor y colocación básica | Invasión, adelantamiento y repeticiones reglamentarias |
| 15. Banda | Último toque, lugar de salida, lanzamiento alto y gol directo inválido | Animación de dos manos, control de ambos pies y saque incorrecto |
| 16. Meta | Portero, área, distancia y excepción de fuera de juego | Todos los casos particulares e infracciones |
| 17. Córner | Esquina, distancia y excepción de fuera de juego | Animación, señales y todos los casos particulares |

Los contactos usan cápsulas físicas y posiciones centrales, con tolerancias de prototipo. La posición del árbitro y la inclinación de las barridas aún no se guardan como actores/poses completos en las repeticiones.

## Verificación

Las pruebas de dominio incluyen posesión y selección, ambas bandas, ambas líneas de meta, salida completa y diagonal, distancias junto a las cuatro esquinas, líneas de pase y anticipación. El comprobador `Football Dynasty > Comprobar reglas en Play (partido de prueba)` añade escenarios con actores/Rigidbody del juego para recepción, banda, neutral, meta y doble toque.

Compilar y pasar esas pruebas no equivale a haber validado visualmente un partido completo. El resultado de Play Mode se registra en `RulesPlayCheck-result.txt` cuando se ejecuta el comprobador.

Referencias: [Leyes del juego](https://www.theifab.com/), [balón neutral](https://theifab.com/laws/latest/the-start-and-restart-of-play/), [banda](https://www.theifab.com/laws/latest/the-throw-in/).
