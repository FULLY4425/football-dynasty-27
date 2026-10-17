# Revisión de línea de gol

La revisión muestra una repetición lenta del recorrido reciente del balón con cámara cercana, seguida de GOL o NO GOL y una vista lateral ampliada de la línea y el balón. Se activa al cruzar completamente una portería válida, o cuando un balón cercano a la raya se detiene o regresa al campo dentro del marco. Los tiros claramente desviados y por encima del larguero no interrumpen el partido.

El criterio usa el mismo plano y radio del balón que la simulación: centro de línea en ±52,5 m y radio 0,11 m. El borde posterior debe superar el plano para entrar completamente. La revisión presenta la decisión de la simulación; no cambia goles ya adjudicados ni inventa un gol cuando el balón se queda sobre la raya. Los goles inválidos por otra infracción se resuelven por arbitraje sin presentar NO GOL como una falta de cruce.

La física y el reloj se pausan durante 4,8 segundos reales. Se puede continuar antes con Espacio o el botón. Al terminar vuelve la cámara deportiva; si hubo gol sigue la celebración. El acercamiento usa un balón visual sin colisiones y restaura la visibilidad del original y la velocidad del partido. La secuencia de revisión no se guarda como una cámara especial dentro de los archivos de replay; el recorrido normal del balón sí pertenece a la repetición del partido.

Validación: 347 comprobaciones de dominio aprobadas, incluyendo ambas porterías, balón sobre la línea, rebotes y tiros que no requieren revisión. Runtime y editor compilados con referencias de Unity 6000.3.26f1.

Prueba en Unity: Football Dynasty > Comprobar gol fantasma (12 s), desde carrera en Play sin partido activo. El escenario NO GOL invoca el observador con un balón colocado en la raya; el escenario GOL atraviesa el plano con el Rigidbody del juego. El informe se escribe en GoalLinePlayCheck-result.txt sin guardar el resultado en la carrera.

Comprobación realizada en Unity: SUCCESS 7 Unity goal-line review checks. Se revisó visualmente el acercamiento con el balón y la línea visibles, el panel lateral y la distancia ampliada. No se ha verificado en un móvil físico.
