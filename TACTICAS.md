# Dynasty IQ · Inteligencia táctica

Sistema propio, modular, de decisiones para jugadores sin balón. Usa los ejes del campo: X apunta a las porterías, Z a las bandas. Local ataca +X; visitante -X.

## Roles y fases

`TacticalRole`: CazadorDeAreas, ExtremoInvertido, CreadorDeJuego, CentralDeCobertura, MediocentroDefensivo y Portero.

`GamePhase`: Ofensivo para el poseedor, Defensivo para el rival y BalonDisputado cuando nadie controla el balón. El detector considera proximidad, altura y velocidad: un jugador cercano a un balón aéreo o muy rápido no recibe automáticamente la posesión. Hay 0,4 s de continuidad de fase en un pase corto; durante ese intervalo puede no existir un portador identificado. El saque de centro reinicia la posesión.

## Decisiones

- Extremo invertido: con posesión propia, cuando el balón supera 10 m desde el centro hacia la banda opuesta a su posición de formación, corre hacia X=±43 y Z=±4. Reduce su separación lateral y entra por dentro.
- Creador: se ofrece aproximadamente 7 m por detrás del portador y 4–8 m a un lado. Compara dos líneas de pase y prefiere la menos ocupada por rivales. Si el balón está en tránsito, usa su posición como referencia temporal.
- Cazador de áreas: ataca el área en campo contrario y se aproxima progresivamente desde zonas más retrasadas.
- Central de cobertura: protege la zona delante de su propia portería y desplaza la línea con el balón.
- Mediocentro defensivo: se coloca entre balón y portería para proteger el carril central.
- Portero: conserva su posición cercana a la línea y acompaña lateralmente el balón.

En defensa presiona el jugador de campo más cercano de cada equipo; los demás conservan sus responsabilidades. Con balón suelto solo uno por equipo inicia la recuperación. El portador conserva una conducción básica; el jugador controlado por la persona recibe sus entradas manuales.

## Cambiar roles en el juego

En **Once y cartas → Roles del once**, pulsa el botón del rol de cada jugador para recorrer las opciones. Los cambios se guardan y se aplican al siguiente partido. Solo puedes cambiar roles de tu club. Los porteros conservan ese rol. Los guardados anteriores obtienen valores iniciales según posición y formación: extremos exteriores en 4-3-3, cazador central, un mediocentro defensivo, creadores y centrales de cobertura.

La banda del extremo se deduce de su posición de formación; asignarlo a un puesto central no crea artificialmente una banda. La etiqueta del partido muestra la fase de tu equipo y el rol del jugador controlado.

## Arquitectura

- `Assets/Scripts/Domain/Tactics.cs`: enums, observaciones, detector compartido de posesión y máquina de estados determinista. Sin dependencias Unity ni asignaciones por decisión.
- `Assets/Scripts/Runtime/TeamTactics.cs`: toma una instantánea compartida del balón y los 22 agentes al comienzo de cada paso de física, evalúa objetivos y los entrega a los agentes.
- `FootballAgent.Step`: consume el objetivo, manteniendo aceleración, sprint, fatiga, colisiones y animación existentes. Se sustituyó la antigua lógica de perseguir siempre al balón.
- `Career`: asignación, validación y guardado de roles por jugador; valores predeterminados para guardados anteriores.

Para ampliar, añade roles y reglas a TacticalBrain o extrae políticas por rol manteniendo las mismas observaciones/decisiones. Después pueden incorporarse instrucciones de equipo, alturas de bloque y transiciones.

## Pruebas realizadas y prueba pendiente dentro de Unity

**101 pruebas del núcleo aprobadas**, incluidas 17 nuevas: fases opuestas, diagonales en banda contraria y en ambos sentidos del campo, apoyo corto, cobertura, balón aéreo, recuperación única, reinicio de posesión, asignación/guardado y compatibilidad. Un escenario cinemático de 500 pasos a 0,02 s utiliza el evaluador real y comprueba que el extremo llega al área y el creador se acerca al portador. No es una prueba de PhysX ni una ejecución del partido Unity.

**28 archivos** pasan revisión de sintaxis C# 9. El núcleo compila en .NET sin errores/advertencias. El editor de Unity sigue sin estar instalado: no se ha ejecutado ni verificado el partido con esta integración.

Se preparó una prueba para cuando el editor esté disponible:

1. Abre Football.unity en Play y entra en un partido, sin pausa ni repetición.
2. Menú **Football Dynasty → Comprobar tácticas durante partido (10 s)**.
3. El escenario usa los agentes y Rigidbody reales con portador fijo en una banda, extremo en la contraria y creador retrasado.
4. La consola registra éxito/error según sus estados y posiciones. Al terminar vuelve a carrera y descarta el partido de prueba sin guardar ni avanzar jornadas.

Esta comprobación se preparó pero **no se ejecutó**. Comprueba también manualmente roles cambiados, presión rival, repeticiones y guardado en un partido normal.

## Límites

Es una estructura base: no reproduce algoritmos internos de juegos comerciales. No hay todavía fuera de juego, anticipación avanzada, separación completa entre compañeros, marcaje individual sofisticado ni planificación táctica colectiva. Los umbrales deben calibrarse observando partidos reales en el editor y midiendo el coste en móvil.
