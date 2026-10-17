# Portero con manos y carrera articulada

El portero intenta atrapar balones alcanzables dentro de su propia área. La altura, la distancia, la velocidad y el atributo MAN limitan la captura. Los disparos demasiado fuertes mantienen el comportamiento de rechazo. Se evita el agarre de un pase directo con el pie de un compañero registrado como último golpeador; no implementa todas las excepciones de cesión, doble manejo o sanciones del reglamento completo.

Durante el agarre el balón sigue el punto medio de ambas manos, con recogida y codos flexionados. Su Rigidbody queda temporalmente cinemático y el collider se desactiva para evitar rebotes en el cuerpo. Al soltar se restauran física, gravedad y colisión. No se aplican fuerzas aerodinámicas a un balón cinemático.

Si es tu equipo, el portero recibe el control: J distribuye con la mano y K despeja; WASD permite orientar el saque sin desplazarse. La IA distribuye aproximadamente a los 1,7 segundos; el portero humano distribuye automáticamente a los 6 segundos si no actúas. Hay recuperación para evitar que capture inmediatamente su propio saque. Reiniciar o salir libera el estado de agarre.

Los modelos tienen caderas, rodillas, hombros, codos y manos. La carrera incorpora piernas alternadas, flexión de rodilla en recuperación, codos doblados, giro y balanceo del torso, rebote vertical moderado y velocidad de animación suavizada. Las fases individuales se conservan mediante una semilla estable. Las poses nuevas se registran en los replays; los archivos antiguos se admiten con los nuevos campos a cero.

Siguen siendo figuras originales de cápsulas, sin captura de movimiento, esqueleto humano importado, dedos ni IK avanzada. No incluye aún estiradas completas del portero ni una animación de atrapar cualquier balón inalcanzable.

Verificación: 360 comprobaciones de dominio aprobadas. Runtime y comprobador de editor compilados con el compilador y referencias de Unity 6000.3.26f1. No se ejecutó el nuevo escenario en Play ni se validó visualmente: el control de aplicaciones rechazó la restauración de Unity al detectar entrada manual.

Para probarlo: abre carrera en Play y selecciona Football Dynasty > Comprobar portero con manos (6 s). El escenario coloca un balón cerca del portero y comprueba agarre, seguimiento de manos, flexión de codos, rodillas, saque y replay. Informe: KeeperPlayCheck-result.txt. No guarda el resultado en la carrera.
