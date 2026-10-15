# Repeticiones y mejoras del partido

## Uso preparado en Unity

- Durante el partido: pulsa **R** o **Ver últimos 10 segundos**. Si el partido lleva menos tiempo, se reproduce lo disponible. Los diez segundos son de juego real, no diez minutos del reloj futbolístico acelerado.
- Después de un gol: **Repetir último gol** conserva la jugada hasta que se marque otro gol o se inicie otro partido. Se registra el cruce antes de reiniciar el saque de centro.
- En el visor: pausa, vuelve al inicio, mueve la barra de tiempo, selecciona 0,25× / 0,5× / 1× / 2× o cambia entre televisión, cerca del balón y portería.
- Pulsa **Guardar repetición** para conservarla. Desde Carrera → **Repeticiones guardadas** puedes abrirla después de salir del partido o reiniciar el juego. El nombre del archivo incluye fecha, jugada y equipos.
- **Volver / Esc** devuelve exactamente el estado de pausa previo. Ver la repetición no entrega resultados al calendario ni mueve los cuerpos del partido.

Las repeticiones se guardan como archivos `.fdreplay.json` en la carpeta `replays` de `Application.persistentDataPath`. Son grabaciones de la escena 3D, no vídeos MP4. Contienen equipos, jugadores, colores, poses, giro del balón, piernas y marcador; se pueden volver a ver aunque cambie la plantilla. No capturan audio. Una repetición no guardada se pierde al cerrar el juego.

## Implementación

`Domain/Replay.cs` contiene el búfer de diez segundos, copias independientes, validación, búsqueda de fotogramas e interpolación temporal. `Runtime/MatchReplay.cs` registra los 22 agentes y el balón en cada paso de física (normalmente 50 Hz). El visor crea actores sin colisiones y congela el partido; avanza con tiempo independiente de la pausa. La biblioteca carga los archivos del disco, valida estructura y límites y rechaza archivos excesivos o corruptos. Se escribe primero un archivo temporal y después se renombra.

## Realismo añadido

- El gol exige que el balón cruce completamente la línea y que el cruce ocurra dentro de los postes y debajo del larguero. Se interpola el punto de cruce para tiros rápidos.
- Resistencia del aire y curvatura por giro; fuerza y dispersión de tiros/pases según atributos, fatiga y dirección del jugador.
- Portero con despeje básico; IA que pasa al sentirse presionada.
- Botas vinculadas al movimiento de las piernas; balón con textura procedural de paneles pentagonales.

Son mejoras del prototipo. Todavía faltan modelos humanos con rig, animaciones profesionales, paradas animadas, árbitro, faltas y saques reglamentarios. Los coeficientes de vuelo están ajustados como parámetros de juego y requieren calibración en Unity.

## Verificación real

El núcleo C# pasó **61 comprobaciones**, incluidas ventana de diez segundos, búsqueda/interpolación, guardado y lectura de un archivo real, independencia del clip respecto al búfer, rechazo de datos inválidos y detección de goles en ambas porterías. Los controles de reproducción, la restauración de cuerpos/cámara, los materiales y el comportamiento físico requieren la prueba dentro del editor. Unity sigue sin estar instalado: no se dispone de comprobación visual ni ejecutable nuevo.
