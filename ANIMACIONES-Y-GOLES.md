# Animaciones y celebraciones

Los 22 jugadores tienen un modelo articulado original con pivotes independientes en hombros y caderas. Se generan poses de reposo, carrera/sprint, pase, tiro, recepción del receptor previsto, parada del portero, saque de banda y barrida. La fase de carrera varía según un identificador estable de cada jugador; no son clips únicos capturados para cada futbolista.

Un gol válido activa 4,5 segundos de celebración: tres variantes (brazos abiertos y saltos, brazos elevados, gesto con giro del torso) para el equipo anotador y reacción del rival. El reloj y la acción se detienen; después saca el equipo que recibió el gol. La cámara acerca la celebración y vuelve a la transmisión. Un gol en el último instante termina su celebración antes de cerrar el encuentro.

Las repeticiones registran piernas, brazos, apertura de hombros, inclinación, giro y elevación del modelo. Los archivos antiguos se admiten con valores nuevos a cero. El tiempo del replay avanza independientemente del reloj deportivo, evitando fotogramas con tiempos repetidos durante faltas y celebraciones. El último gol se actualiza al finalizar la celebración.

Estas son animaciones procedurales para los modelos de cápsulas actuales. Quedan pendientes modelos humanos con esqueleto completo, codos/rodillas articulados, captura de movimiento y sincronización exacta del pie con el balón; la física del golpeo sigue siendo inmediata.

Pruebas: 333 comprobaciones de dominio, incluidas poses, variantes individuales, prioridades de barrida y copia independiente de la animación en replay. Runtime y editor compilados con referencias de Unity 6000.3.26f1.

En Unity: Football Dynasty > Comprobar animaciones y gol (8 s), desde carrera en Play sin un partido activo. Inicia un encuentro temporal, fuerza un cruce de gol real y comprueba animación, celebración, reloj, saque y replay sin guardar el resultado en la carrera. Informe: AnimationPlayCheck-result.txt.

Comprobación ejecutada en Unity: SUCCESS 9 Unity runtime animation checks. Se observó el equipo anotador con brazos abiertos y la cámara cercana, y el regreso posterior a carrera sin avanzar ni guardar el resultado de prueba. No se ha validado todavía en Android/iOS físicos.
